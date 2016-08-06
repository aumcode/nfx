using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using NFX;
using NFX.DataAccess.CRUD;
using NFX.Erlang;

namespace NFX.DataAccess.Erlang
{
  /// <summary>
  /// Executes Erlang CRUD script-based queries
  /// </summary>
  public sealed class ErlCRUDScriptQueryHandler : ICRUDQueryHandler
  {
    #region CONSTS

       public static readonly IErlObject EXECUTE_OK_PATTERN    = ErlObject.Parse("{ReqID::int(), {ok, SchemaID::atom(), Rows::list()}}");
       public static readonly IErlObject EXECUTE_ERROR_PATTERN = ErlObject.Parse("{ReqID::int(), {error, Code::int(), Msg}}");

       public static readonly IErlObject EXECUTE_SUBSCRIBE_OK_PATTERN = ErlObject.Parse("{ReqID::int(), ok}");
       public static readonly IErlObject EXECUTE_SUBSCRIBE_ERROR_PATTERN = ErlObject.Parse("{ReqID::int(), {error, Code::int(), Msg}}");

       public const int INVALID_SUBSCRIPTION_REQUEST_EXCEPTION = -10;

       public static readonly ErlAtom ATOM_ReqID    = new ErlAtom("ReqID");
       public static readonly ErlAtom ATOM_SchemaID = new ErlAtom("SchemaID");
       public static readonly ErlAtom ATOM_Rows     = new ErlAtom("Rows");
       public static readonly ErlAtom ATOM_Code     = new ErlAtom("Code");
       public static readonly ErlAtom ATOM_Msg      = new ErlAtom("Msg");

       public static readonly ErlAtom ATOM_Subscriber = new ErlAtom("Subscriber");
       public static readonly ErlAtom ATOM_Timestamp = new ErlAtom("Timestamp");


    //public static readonly IErlObject EXECUTE_OK_PATTERN =
    //new ErlPatternMatcher {
    //    {"stop", (p, t, b, _args) => { active = false; return null; } },
    //    {"Msg",  (p, t, b, _args) => { Console.WriteLine(b["Msg"].ToString()); return null; } },
    //};


    #endregion

    #region .ctor
        public ErlCRUDScriptQueryHandler(ErlDataStore store, QuerySource source)
        {
          m_Store = store;
          m_Source = source;
        }
    #endregion

    #region Fields
        private ErlDataStore m_Store;
        private QuerySource m_Source;
    #endregion

    #region ICRUDQueryHandler


      public string Name{ get { return m_Source.Name; }}

      public ICRUDDataStore Store{ get { return m_Store;}}


      public Schema GetSchema(ICRUDQueryExecutionContext context, Query query)
      {
        return m_Store.Map.GetCRUDSchemaForName(m_Source.OriginalSource);
      }

      public Task<Schema> GetSchemaAsync(ICRUDQueryExecutionContext context, Query query)
      {
        return TaskUtils.AsCompletedTask(() => GetSchema(context, query) );
      }

      public RowsetBase Execute(ICRUDQueryExecutionContext context, Query query, bool oneRow = false)
      {
        var store = ((ErlCRUDQueryExecutionContext)context).DataStore;
        var mbox = ((ErlCRUDQueryExecutionContext)context).ErlMailBox;

        var parsed = prepareQuery(m_Source);

        var reqID = m_Store.NextRequestID;

        var bind = new ErlVarBind();

        foreach(var erlVar in parsed.ArgVars)
        {
           var name = erlVar.Name.Value;

           var clrPar = query[name];
           if (clrPar==null)
            throw new ErlDataAccessException(StringConsts.ERL_DS_QUERY_PARAM_NOT_FOUND_ERROR.Args(parsed.Source, name));

           bind.Add(erlVar, clrPar.Value);
        }

        var request = parsed.ArgTerm.Subst(bind);

        var args = new ErlList
        {
          reqID.ToErlObject(),
          parsed.Module,
          parsed.Function,
          request
        };

        var rawResp = store.ExecuteRPC(ErlDataStore.NFX_CRUD_MOD,
                                       ErlDataStore.NFX_RPC_FUN, args, mbox);

        var response = rawResp as ErlTuple;

        // {ReqID, {ok, SchemaID, [{row},{row}...]}}
        // {ReqID, {error, Reason}}

        if (response==null)
          throw new ErlDataAccessException(StringConsts.ERL_DS_INVALID_RESP_PROTOCOL_ERROR+"QryHndlr.Response timeout");


        bind = response.Match(EXECUTE_OK_PATTERN);
        checkForError(EXECUTE_ERROR_PATTERN, response, bind, reqID);

        if (bind[ATOM_ReqID].ValueAsLong != reqID)
            throw new ErlDataAccessException(StringConsts.ERL_DS_INVALID_RESP_PROTOCOL_ERROR+"QryHndlr.Response.ReqID mismatch");

        //{ReqID::int(), {ok, SchemaID::atom(), Rows::list()}}

        var schema = bind[ATOM_SchemaID].ValueAsString;
        var rows = bind[ATOM_Rows] as ErlList;

        //{ok, "tca_jaba",
        //[
        //  {tca_jaba, 1234, tav, "User is cool", true},
        //  {tca_jaba, 2344, zap, "Zaplya xochet pit", false},
        //  {tca_jaba, 8944, tav, "User is not good", false}
        //]};
        return m_Store.Map.ErlCRUDResponseToRowset(schema, rows, query.ResultRowType);
      }

      public Task<RowsetBase> ExecuteAsync(ICRUDQueryExecutionContext context, Query query, bool oneRow = false)
      {
        return TaskUtils.AsCompletedTask(() => Execute(context, query, oneRow) );
      }

      //used for subscription
      public int ExecuteWithoutFetch(ICRUDQueryExecutionContext context, Query query)
      {
        var store = ((ErlCRUDQueryExecutionContext)context).DataStore;
        var mbox = ((ErlCRUDQueryExecutionContext)context).ErlMailBox;
        var ts = ((ErlCRUDQueryExecutionContext)context).SubscriptionTimestamp;

        if (!ts.HasValue)
          throw new ErlDataAccessException(StringConsts.ERL_DS_QUERY_TMSTAMP_CTX_ABSENT_ERROR);

        var parsed = prepareQuery(m_Source);

        var reqID = m_Store.NextRequestID;

        var bind = new ErlVarBind();

        var wass = false;
        var wast = false;
        foreach(var erlVar in parsed.ArgVars)
        {
           var name = erlVar.Name.Value;

           if (erlVar.Name==ATOM_Subscriber &&
               erlVar.ValueType==ErlTypeOrder.ErlPid)
           {
             bind.Add(ATOM_Subscriber, mbox.Self);
             wass = true;
             continue;
           }

           if (erlVar.Name==ATOM_Timestamp &&
               erlVar.ValueType==ErlTypeOrder.ErlLong)
           {
             bind.Add(ATOM_Timestamp, new ErlLong(ts.Value.Microseconds));
             wast = true;
             continue;
           }

           var clrPar = query[name];
           if (clrPar==null)
            throw new ErlDataAccessException(StringConsts.ERL_DS_QUERY_PARAM_NOT_FOUND_ERROR.Args(parsed.Source, name));

           bind.Add(erlVar, clrPar.Value);
        }

        if (!wass)
          throw new ErlDataAccessException(StringConsts.ERL_DS_QUERY_SUBSCR_NOT_FOUND_ERROR);
        if (!wast)
          throw new ErlDataAccessException(StringConsts.ERL_DS_QUERY_TMSTAMP_NOT_FOUND_ERROR);


        var request = parsed.ArgTerm.Subst(bind);

        var args = new ErlList
        {
          reqID.ToErlObject(),
          parsed.Module,
          parsed.Function,
          request
        };

        var rawResp = store.ExecuteRPC(ErlDataStore.NFX_CRUD_MOD,
                                       ErlDataStore.NFX_SUBSCRIBE_FUN, args, null);

        var response = rawResp as ErlTuple;

        // {ReqID, {ok, SchemaID, [{row},{row}...]}}
        // {ReqID, {ReqID::int(), {error, Code::int(), Msg}}}

        if (response==null)
          throw new ErlDataAccessException(StringConsts.ERL_DS_INVALID_RESP_PROTOCOL_ERROR+"QryHndlr.Response==null");


        bind = response.Match(EXECUTE_SUBSCRIBE_OK_PATTERN);
        checkForError(EXECUTE_SUBSCRIBE_ERROR_PATTERN, response, bind, reqID);

        //{ReqID::int(), ok}
        return 0;
      }

      public Task<int> ExecuteWithoutFetchAsync(ICRUDQueryExecutionContext context, Query query)
      {
        return TaskUtils.AsCompletedTask(() => ExecuteWithoutFetch(context, query) );
      }


      public Cursor OpenCursor(ICRUDQueryExecutionContext context, Query query)
      {
        throw new NotSupportedException("Erl.OpenCursor");
      }

      public Task<Cursor> OpenCursorAsync(ICRUDQueryExecutionContext context, Query query)
      {
        throw new NotSupportedException("Erl.OpenCursorAsync");
      }

    #endregion


    #region .pvt

      private void checkForError(IErlObject pattern, IErlObject response, ErlVarBind bind, int reqID)
      {
        if (bind != null)
          return;

        bind = response.Match(pattern);
        if (bind == null)
          throw new ErlDataAccessException(StringConsts.ERL_DS_INVALID_RESP_PROTOCOL_ERROR + "invalid error response pattern", new Exception(response.ToString()));

        var gotReqID = bind[ATOM_ReqID].ValueAsLong;

        if (gotReqID != reqID)
          throw new ErlDataAccessException(StringConsts.ERL_DS_INVALID_RESP_PROTOCOL_ERROR + "unexpected transaction ID (expected={0}, got={1})".Args(reqID, gotReqID));

        var ecode = bind[ATOM_Code].ValueAsInt;
        var rmsg  = bind[ATOM_Msg];
        var emsg  = rmsg.TypeOrder == ErlTypeOrder.ErlString || rmsg.TypeOrder == ErlTypeOrder.ErlAtom
                  ? rmsg.ValueAsString
                  : rmsg.ToString();

        Exception error;

        switch (ecode)
        {
          case INVALID_SUBSCRIPTION_REQUEST_EXCEPTION:
            error = new NFX.DataAccess.CRUD.Subscriptions.InvalidSubscriptionRequestException(emsg, null);
            break;
          case -1:
            error = new ErlDataAccessException("Remote error message: {0}".Args(emsg));
            break;
          default:
            error = new ErlDataAccessException("Remote error code {0}. Message: {1}".Args(ecode, emsg));
            break;
        }

        throw error;
      }

      private struct parsedQuery
      {
        public string Source;
        public ErlAtom Module;
        public ErlAtom Function;
        public ErlList ArgTerm;
        public HashSet<ErlVar> ArgVars;
      }

      private static volatile Dictionary<string, parsedQuery> s_ParsedQueryCache = new Dictionary<string, parsedQuery>(StringComparer.Ordinal);


      // Args := {trade_ctrl, stop_strat,[StratID::atom(), OpDescr::string(), User::int()]}
      // nfx_crud:rpc(ReqID, Mod, Fun, Args)
      private static parsedQuery prepareQuery(QuerySource qSource)
      {

         var src = qSource.StatementSource;

         parsedQuery result;

         if (s_ParsedQueryCache.TryGetValue(src, out result)) return result;

         try
         {
           var mfa = NFX.Erlang.ErlObject.ParseMFA(src);

           var argsTerm = mfa.Item3;
           var vars = argsTerm.Visit(new HashSet<ErlVar>(), (a, o) => { if (o is ErlVar) a.Add((ErlVar)o); return a; });

           result = new parsedQuery()
           {
             Source = src,
             Module = mfa.Item1,
             Function = mfa.Item2,
             ArgTerm = argsTerm,
             ArgVars = vars
           };
         }
         catch(Exception error)
         {
            throw new ErlDataAccessException(StringConsts.ERL_DS_QUERY_SCRIPT_PARSE_ERROR.Args(src, error.ToMessageWithType()), error);
         }

         var dict = new Dictionary<string, parsedQuery>(s_ParsedQueryCache, StringComparer.Ordinal);
         dict[src] = result;
         s_ParsedQueryCache = dict;//atomic

         return result;
      }


    #endregion


  }
}
