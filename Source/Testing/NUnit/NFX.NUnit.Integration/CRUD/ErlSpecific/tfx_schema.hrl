%% Mnesia tables

-record(secdef2, {
      symbol         :: atom()             %% Internal name of the security
}).

-record(secdef, {
      key            :: {atom(),atom()}    %% {Xchg,Symbol} Internal name of the security
    , instr          :: atom()             %% Exch-specific instrument name
    , secid          :: integer()          %% Security ID
    , xchg_secid     :: integer()          %% Exch-specific Security ID
    , isin           :: string()           %% ISIN security code
    , descr          :: string()           %% Description
    , ccy            :: atom()             %% Base currency
    , settl_ccy      :: atom()             %% Settlement currency
    , contr_mult     :: float()            %% Contract multiplier
    , und_symbol     :: atom()             %% Underlying Internal name of the security
    , und_instr      :: atom()             %% Underlying Exch-specific instrument name
    , und_secid      :: integer()          %% Underlying Security ID
    , und_xchg_secid :: integer()          %% Underlying Exch-specific Security ID
    , maturity       :: erlang:time_unit() %% Maturity Date
    , strike         :: float()            %% Strike price in ccy
    , init_mgn_buy   :: float()            %% Initial buy side margin
    , init_mgn_sell  :: float()            %% Initial sell side margin
    , init_mgn_synth :: float()            %% Initial synthetic margin
    , status         :: char()             %% Trading status
    , address        :: string()           %% Multicast address for change notifications
    , px_step        :: float()            %% Minimum price step
    , px_lo_lim      :: float()            %% Price low limit
    , px_hi_lim      :: float()            %% Price high limit
}).

