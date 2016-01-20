-module(nfx_test).
-export([echo/1, secdef_by_exchange/1, world_news/4]).
-export([start/0, stop/0, subscribe/2, unsubscribe/2,
         subscribers/1, broadcast/2]).

%% gen_server callbacks
-export([init/1, handle_call/3, handle_cast/2, handle_info/2, terminate/2,
         code_change/3]).

-behaviour(gen_server).

-include("tfx_schema.hrl").

-record(state, {
    subs :: map()   %% Maps SubscrKey::atom() -> [Subscriber::pid()]
}).

%%-----------------------------------------------------------------------------
%% CRUD DataStore testing API
%%-----------------------------------------------------------------------------    

%% (string()) -> 
%%   {ok, Schema,[{Schema, Msg::string(), Time::long()}]}
%% <schema>
%%  <field name="echoed_msg" type="string"/>
%%  <field name="ts" type="datetime"/>
%% </schema>

echo(Msg) when is_list(Msg) ->
    Now = erlang:system_time(micro_seconds),
    {echo_schema, [{echo_schema, "You said: " ++ Msg, Now}]}.
    
secdef_by_exchange(Exch) ->
    Query = [{#secdef{key={'$1', '_'}, _ = '_'}, [{'=:=', '$1', Exch}], ['$_']}],
    Res   = mnesia:dirty_select(secdef, Query),
    {secdef, Res}.

%% <schema name="world_news">
%%  <field name="time" type="datetime"/>
%%  <field name="news" type="string"/>
%% </schema>
world_news(Pid, TS, Count, PeriodMS)
  when is_pid(Pid), is_integer(TS), is_integer(Count), is_integer(PeriodMS) ->
    start(),
	Now = erlang:system_time(micro_seconds),
    Tab = world_news,
    subscribe(Tab, Pid),
    News = ["News" ++ integer_to_list(I) || I <- lists:seq(1, Count)],
    F = fun
        Loop(_Pids, []) ->
            ok;
        Loop(Pids, [Msg | Tail]) ->
            [P ! {Tab, Now, $w, [{Tab, erlang:system_time(micro_seconds), Msg}]} || P <- Pids],
            timer:sleep(PeriodMS),
            Loop(Pids, Tail)
        end,
    case subscribers(Tab) of
        []   -> ok;
        Subs -> spawn(fun() -> F(Subs, News) end)
    end,
    ok.
    
%%-----------------------------------------------------------------------------
%% Subscription service API
%%-----------------------------------------------------------------------------    
start() ->
    gen_server:start({local, ?MODULE}, ?MODULE, [], []).
stop() ->
    gen_server:call(?MODULE, stop).

subscribe(Tab, Pid) when is_atom(Tab), is_pid(Pid) ->
    gen_server:call(?MODULE, {subscribe, Tab, Pid}).

unsubscribe(Tab, Pid) when is_atom(Tab), is_pid(Pid) ->
    gen_server:call(?MODULE, {unsubscribe, Tab, Pid}).

-spec subscribers(Tab::atom()) -> [pid()].
subscribers(Tab) when is_atom(Tab) ->
    gen_server:call(?MODULE, {subscribers, Tab}).

broadcast(Tab, Msg) when is_atom(Tab) ->
    gen_server:cast(?MODULE, {broadcast, Tab, Msg}).

%%%----------------------------------------------------------------------------
%%% Callback functions from gen_server
%%%----------------------------------------------------------------------------
init([]) ->
    {ok, #state{subs = #{}}}.

handle_call({subscribe, Tab, Pid}, _From, #state{subs = Subs} = S) ->
    NewSubs = add(Tab, Pid, Subs),
    monitor(process, Pid),
    {reply, ok, S#state{subs = NewSubs}};
    
handle_call({unsubscribe, Tab, Pid}, _From, #state{subs = Subs} = S) ->
    NewSubs = remove(Tab, Pid, Subs),
    {reply, ok, S#state{subs = NewSubs}};

handle_call({subscribers, Tab}, _From, #state{subs = Subs} = S) ->
    try          {reply, maps:get(Tab, Subs), S}
    catch _:_ -> {reply, [], S}
    end;

handle_call(stop, _From, S) ->
    {stop, normal, S};

handle_call(Request, _From, State) ->
    {stop, {unknown_call, Request}, State}.

handle_cast({broadcast, Tab, Msg}, #state{subs = Subs} = S) ->
    case maps:find(Tab, Subs) of 
        {ok, Pids} ->
            [Pid ! Msg || Pid <- Pids];
        error ->
            ok
    end,
    {noreply, S};
handle_cast(Msg, State) ->
    {stop, {unknown_cast, Msg}, State}.

handle_info({'DOWN', _Ref, process, Pid, _Info}, #state{subs = Subs} = S) ->
    F = fun(Tab, Pids, M) -> M#{Tab := [P || P <- Pids, P =/= Pid]} end,
    {noreply, S#state{subs = maps:fold(F, Subs, Subs)}};
handle_info(_Info, State) ->
    {noreply, State}.

terminate(_Reason, #state{}) ->
    ok.
code_change(_OldVsn, State, _Extra) ->
    {ok, State}.

%%%----------------------------------------------------------------------------
%%% Internal functions
%%%----------------------------------------------------------------------------
add(Tab, Pid, Subs) when is_atom(Tab), is_pid(Pid), is_map(Subs) ->
    case maps:find(Tab, Subs) of
        {ok, List} ->
            List0 = [P || P <- List, P =/= Pid],
            Subs#{Tab := [Pid | List0]};
        error ->
            Subs#{Tab => [Pid]}
    end.

remove(Tab, Pid, Subs) when is_atom(Tab), is_pid(Pid), is_map(Subs) ->
    case maps:find(Tab, Subs) of 
        {ok, List} -> Subs#{Tab := List -- [Pid]};
        error      -> Subs
    end.