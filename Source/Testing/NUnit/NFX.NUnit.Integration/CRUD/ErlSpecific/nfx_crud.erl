-module(nfx_crud).
-export([bonjour/3, rpc/4, subscribe/4, schema_content/0, write/1, delete/1, select_all/1, select/2]).

-include("tfx_schema.hrl").

-define(GEN_ERROR, -1).

bonjour(InstID, AppName, UserName) when is_integer(InstID), is_list(AppName), is_list(UserName) ->
    mnesia:start(),
    mnesia:create_table(secdef, [{attributes,
                         [key,
                          instr,
                          secid,
                          xchg_secid,
                          isin,
                          descr,
                          ccy,
                          settl_ccy,
                          contr_mult,
                          und_symbol,
                          und_instr,
                          und_secid,
                          und_xchg_secid,
                          maturity,
                          strike,
                          init_mgn_buy,
                          init_mgn_sell,
                          init_mgn_synth,
                          status,
                          address,
                          px_step,
                          px_lo_lim,
                          px_hi_lim]}]),
    {bonjour, InstID, {ok, schema_content()}}.

rpc(ReqID, Mod, Fun, Args)
  when is_integer(ReqID), is_atom(Mod), is_atom(Fun), is_list(Args) ->
    try
        {Schema, Res} = erlang:apply(Mod, Fun, Args),
        {ReqID, {ok, Schema, Res}}
    catch _:Reason ->
        {ReqID, {error, ?GEN_ERROR, Reason}}
    end.

subscribe(ReqID, Mod, Fun, Args)
  when is_integer(ReqID), is_atom(Mod), is_atom(Fun), is_list(Args) ->
    try
        ok = erlang:apply(Mod, Fun, Args),
        {ReqID, ok}
    catch _:Reason ->
        {ReqID, {error, ?GEN_ERROR, Reason}}
    end.

%% Example: Row = {secdef, Key, Field, ...}
-spec write(Row::tuple()) -> {ok, integer()} | {error, any()}.
write(Row) when is_tuple(Row) ->
    case mnesia:dirty_write(Row) of
    ok    -> {ok, 1};
    Error -> {error, Error}
    end.

delete({Tab, Key}) ->
    ok = mnesia:dirty_delete(Tab, Key),
    {ok, 1}.

select(Tab, Key) ->
    Res = mnesia:dirty_select(Tab, [{{'_', Key, _ = '_'}, [], ['$_']}]),
    {Tab, Res}.

select_all(Tab) ->
    Res = mnesia:dirty_match_object(mnesia:table_info(Tab, wild_pattern)),
    {Tab, Res}.
    
schema_content() ->
    <<"<protocol>
      <schema name=\"echo_schema\">
          <field name=\"echoed_msg\" type=\"string\"/>
          <field name=\"ts\" type=\"datetime\"/>
      </schema>
      <schema name=\"world_news\">
          <field name=\"time\" type=\"datetime\"/>
          <field name=\"news\" type=\"string\"/>
      </schema>
      <schema name=\"secdef\" descr=\"Security Definition\" insert=\"true\" update=\"true\" delete=\"true\">
        <field name=\"xchg\" type=\"atom\" len=\"10\" title=\"Exchange\" descr=\"Exchange name\" key=\"true\" required=\"true\"/>
        <field name=\"symbol\" type=\"atom\" len=\"12\" title=\"Symbol\" descr=\"Internal name of the security\" key=\"true\" required=\"true\"/>
        <field name=\"instr\" type=\"binstr\" len=\"12\" title=\"Instr\" descr=\"Exch-specific instrument name\" required=\"true\"/>
        <field name=\"secid\" type=\"long\" title=\"SecID\" descr=\"Security ID\" required=\"true\"/>
        <field name=\"xchg_secid\" type=\"long\" title=\"ExchSecID\" descr=\"Exch-specific Security ID\" required=\"true\"/>
        <field name=\"isin\" type=\"string\" len=\"12\" title=\"ISIN\" descr=\"ISIN security code\" required=\"true\"/>
        <field name=\"descr\" type=\"string\" len=\"80\" title=\"Description\" descr=\"Description\" default=\"\"/>
        <field name=\"ccy\" type=\"atom\" len=\"3\" title=\"Ccy\" descr=\"Base currency\" required=\"true\"/>
        <field name=\"settl_ccy\" type=\"atom\" len=\"3\" title=\"SettlCcy\" descr=\"Settlement currency\" required=\"true\"/>
        <field name=\"contr_mult\" type=\"double\" title=\"ContractMult\" descr=\"Contract multiplier\" required=\"true\" display-format=\"{0:F2}\"/>
        <field name=\"und_symbol\" type=\"atom\" len=\"12\" title=\"UndSymbol\" descr=\"Underlying Internal name of the security\"/>
        <field name=\"und_instr\" type=\"binstr\" len=\"12\" title=\"UndInstr\" descr=\"Underlying Exch-specific instrument name\"/>
        <field name=\"und_secid\" type=\"long\" title=\"UndSecID\" descr=\"Underlying Security ID\" default=\"0\"/>
        <field name=\"und_xchg_secid\" type=\"long\" title=\"UndExchSecID\" descr=\"Underlying Exch-specific Security ID\" default=\"0\"/>
        <field name=\"maturity\" type=\"datetime\" title=\"Maturity\" descr=\"Maturity Date\" default=\"0\"/>
        <field name=\"strike\" type=\"double\" title=\"StrikePrice\" descr=\"Strike price in ccy\" default=\"0\"/>
        <field name=\"init_mgn_buy\" type=\"double\" title=\"InitMgnBuy\" descr=\"Initial buy side margin\" display-format=\"{0:F2}\" default=\"0\"/>
        <field name=\"init_mgn_sell\" type=\"double\" title=\"InitMgnSell\" descr=\"Initial sell side margin\" display-format=\"{0:F2}\" default=\"0\"/>
        <field name=\"init_mgn_synth\" type=\"double\" title=\"InitMgnSynth\" descr=\"Initial synthetic margin\" display-format=\"{0:F2}\" default=\"0\"/>
        <field name=\"status\" type=\"char\" title=\"Status\" descr=\"Trading status\" default=\"D\">
                <value code=\"D\" display=\"Disabled\"/>
                <value code=\"E\" display=\"Error\"/>
                <value code=\"S\" display=\"Suspended\"/>
                <value code=\"H\" display=\"Halted\"/>
                <value code=\"T\" display=\"Trading\"/>
            </field>
        <field name=\"address\" type=\"string\" len=\"25\" title=\"Address\" descr=\"Multicast address for change notifications\" default=\"\"/>
        <field name=\"px_step\" type=\"double\" title=\"PriceStep\" descr=\"Minimum price step\" required=\"true\"/>
        <field name=\"px_lo_lim\" type=\"double\" title=\"PriceLoLimit\" descr=\"Price low limit\" default=\"0\"/>
        <field name=\"px_hi_lim\" type=\"double\" title=\"PriceHiLimit\" descr=\"Price high limit\" default=\"0\"/>
      </schema>
      </protocol>\n">>.