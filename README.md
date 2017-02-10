# NFX
Server UNISTACK framework.

License: Apache 2.0

NFX is written in C# and runs on a CLR machine, however it has very little to do with .NET.
This framework contains truly unique intellectual assets and allows for unconventional things
that significantly boost performance and simplify the development (such as stateful web). 
The majority of the achievements are possible because of two key things:

* Unification of design - all components are written in the same way
* Sophisticated serialization mechanism aka "teleportation"

promoting:

* Stateful WEB programming (not mandatory)
* Full utilization of modern RAM capacities in-proc (i.e. 128 Gb resident) without GC problems 
* Serving 50,000+ BUSINESS web requests a second (with logic) on a 4 core 3.0 GHz machine 
  looking up data in a 300,000,000 business object cache in-RAM (no need for 3rd party cache)

The concepts have been well tested and used. In the past 7 years, *teleportation mechanism has 
moved trillions of various CLR object instances* (including non-trivial CLR cases like: structs with
 read-only fields, arrays of structs of structs, custom streamers like Dictionary<> with comparers etc.)



**GUIDES**:
NEW 20160117, we are adding:
 [NFX Documentation/Guides are here](Guides/README.md)

**NUGET**:
 https://www.nuget.org/packages/NFX/
 
 `pm> install-package NFX`

**Various Demo Projects**:
 https://github.com/aumcode/nfx-demos

NFX Provides:

* Unified App Container
  - Unified app models: console, web, service, all have: user, session, security, volatile state
  - Configuration: file, memory, db, vars, macros, structural merges, overrides, scripting
  - Dep injection: inject dependencies
  - Logger: async file, debug, db destinations with graphs, SLA rules, filtering and routing
  - Security: declr and/or imperative permission model, strong password manager, virtual credentials 
  
* Big Memory Model 
  - Pile memory manager: keeps hundreds of millions of CLR objects in memory without GC pauses
  - Distributed Pile (objects stored on cluster nodes)
  - Pile Cache: materialize 2,000,000 CLR objects/sec in-memory on a 4 core machine
  
* Full Web Stack
  - Web server
  - Rule-based network Gate (business firewall)
  - MVC: filters, attributes, complex model binding, security, web API, MVVM binding
  - Template
  - Client JS lib + MVVM
  
* Hybrid data access layer
  - RDBMS (we use: MySQL, MsSQL)
  - NoSQL (we use: MongoDB, Elastic search, Erlang OTP)
  - Native ultra-fast MongoDB driver (socket based)
  
* Full Instrumentation Suite
  - Gauges and Events keyed on business enities
  - Instrumentation buffers
  - Injectable instrumentation sinks (i.e. telemetry receiver)
  - Cluster real-time map:reduce on zones and regions
    
* High-level service oriented stack "Glue"
  - Contract based design with security
  - Injectable bindings: i.e. "Async TCP"
  - 150,000 ops/sec two-way calls using *business objects* (not byte array) on a 4 core machine
  - Unistack payload teleportation (no need to decorate various classes, teleport as-is)
  
* Serialization Suite
  - Slim: the *fastest general purpose* CLR serializer, very well tested and proven processing 
  - Teleportation: moving *CLR objects as-is without any extra metadata* between processes
  - BSON: an efficient BSON implementation
  - JSON: includes multi-language selective serialization of large graphs
  
* Erlang Support: including types, serialization and full OTP node
* Virtual File Systems: Amazon S3, SVN, Local, (Google Drive and DropBox created by others)
* Virtual payment processing: Braintree, Stripe, PayPal
* Code Analysis (building lexers/parsers)
* Type Conversion Accessors (i.e. object.AsInt(dflt)....)


**IMPORTANT!**

NFX does not use any 3rd party components but for some DB-access (i.e. MySQL and MsSQL).
NFX uses very Base-Class-Lib:
* Basic/primitive types: string, ints, doubles, decimal, dates, +Math
* Parallel task library: 25% of features - create, run, wait for completion,
  Task, Parallel.For/Each
* Collections: List, Dictionary, ConcurrentDictionary, HashSet, Queue
* Threading: Thread, lock()/Monitor, Interlocked*, AutoresetEvent
* Various: Stopwatch, Console, WinForms is used for SOME interactive tests(not needed for operation)
* Some ADO references (Reader/SQLStatement) in segregated data-access components
* Reflection API
* Drawing 2D (Graphics)