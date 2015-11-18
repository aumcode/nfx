-module(nfx_test).
-export([echo/1, secdef_by_exchange/1]).
-include("tfx_schema.hrl").

%% (string()) -> 
%%   {ok, Schema,[{Schema, Msg::string(), Time::long()}]}
%% <schema>
%%  <field name="echoed_msg" type="string"/>
%%  <field name="ts" type="datetime"/>
%% </schema>

echo(Msg) when is_list(Msg) ->
    {echo_schema, [{echo_schema, "You said: " ++ Msg, erlang:system_time(micro_seconds)}]}.
    
secdef_by_exchange(_Exch) ->
    Res = mnesia:dirty_read(secdef,{'CLE', 'AUDJPY'}),
    {secdef, Res}.


%%    Res = mnesia:dirty_select(secdef, [{#secdef{key={'$1', '_'}, _ = '_'}, [{'=:=', '$1', Exch}], ['$_']}]),
%%    {secdef, Res}.
