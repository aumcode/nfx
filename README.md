# NFX - .NET Unified Software Stack + Big Memory
<img src="https://github.com/aumcode/nfx/blob/master/Elements/NFXLogo/New.NFX.Logo.50.png" alt="Logo">

Server UNISTACK *(unified full software stack, no 3rd paty libs)* framework. 

License: **Apache 2.0**

Runs/Builds:  **.NET 4.5+**, **Mono 3.x+** as of 2017/06; .NET **Core 2** planned Aug 2017

[<img src="https://ci.appveyor.com/api/projects/status/github/aumcode/nfx?svg=true" alt="Project Badge" width="200">](https://ci.appveyor.com/project/itadapter/nfx/history)

[Documentation http://nfxlib.com](http://nfxlib.com)

NFX is a modern .NET full stack framework designed for building **cloud** and **on-premises** apps.
It is written in C# and runs on a CLR machine. 
NFX supports app containers, configuration, big memory heaps, IPC, and functions that significantly simplify 
the development of large distributed systems. It boosts performance and simplifies the development (such as services/web). 
The majority of the achievements are possible because of the following key features:

* Unification of design - all components are written in the same way
* Sophisticated serialization mechanism - moves objects between distributed processes/nodes (aka "teleportation")
* Object Pile - a [**100%-managed "Big Memory" approach for utilization of hundreds of gigabytes of RAM**](./tree/master/Source/NFX/ApplicationModel/Pile) without GC stalls

promoting:

* Distributed systems: clusters/hierarchies
* IPC - contract-based object-oriented high performance services
* Stateful/stateless programming (web/service)
* Full utilization of big RAM capacities in-proc (i.e. 128 Gb resident) without GC stalls 
  
good for:

* Linux-style prоgramming in .NET. Minimalistic, elegant.
* Vendor de-coupling 
* General Scalability (i.e. hybrid DataStores with virtual queries and CQRS)
* Distributed Macro/micro/nano services, REST or RPC
* Actor-model systems with message passing and supervisors
* [**In-memory processing**, for **hundreds of millions of objects** in-RAM with full GC < 20ms](./tree/master/Source/NFX/ApplicationModel/Pile)
* **Supercomputer/Cluster/Farm** applications (i.e. bulk rendering cluster, social graphs etc.)
* **High-performance business** apps (i.e. serving 50,000+ BUSINESS WEB requests a second (with logic) on a general server 
  looking up data in a [300,000,000 business object cache in-RAM](./tree/master/Source/NFX/ApplicationModel/Pile))
* non-trivial CLR cases like: structs with read-only fields, arrays of structs of structs, custom streamers like Dictionary<> with comparers etc. In the past 7 years, teleportation mechanism has moved trillions of various CLR object instances


**GUIDES**:
NEW 20170617,
 all Guides and Docs/Samples/Tutorials are on NFXLIB:

 [NFXLIB - Documentation/Guides/Tutorials](http://nfxlib.com)
 
 

 
**NUGET**:

 https://www.nuget.org/packages/NFX/
 
 `pm> install-package NFX`

**Big Memory Object Pile + Cache**

[NFX/ApplicationModel/Pile](./tree/master/Source/NFX/ApplicationModel/Pile)

**Various Demo Projects**:

 https://github.com/aumcode/nfx-demos
 
 https://github.com/aumcode/howto
 
 
NFX Provides:

* Unified App Container
  - Unified app models: console, web, service, all have: user, session, security, volatile state
  - Configuration: file, memory, db, vars, macros, structural merges, overrides, scripting
  - Dep injection: inject dependencies
  - Logger: async file, debug, db destinations with graphs, SLA rules, filtering and routing
  - Security: declr and/or imperative permission model, strong password manager, virtual credentials 
  
* [**Big Memory Pile** ](./tree/master/Source/NFX/ApplicationModel/Pile)
  - Pile memory manager: keeps hundreds of millions of CLR objects in memory without GC pauses
  - Distributed Pile (objects stored on cluster nodes)
  - Pile Cache: materialize 2,000,000 CLR objects/sec in-memory on a 4 core machine
  
* **Full Web Stack**
  - Web server
  - Rule-based network Gate (business firewall)
  - MVC: filters, attributes, complex model binding, security, web API, MVVM binding
  - Template
  - Client JS lib + MVVM
  
* Hybrid data access layer
  - RDBMS (we use: MySQL, MsSQL)
  - NoSQL (we use: MongoDB, Elastic search, Erlang OTP)
  - Native ultra-fast MongoDB driver (socket based achieving 200K+reads/sec)
  
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

NFX uses the very Base-Class-Lib of .NET:
* Basic/primitive types: string, ints, doubles, decimal, dates, +Math
* Parallel task library: 25% of features - create, run, wait for completion,
  Task, Parallel.For/Each
* Collections: List, Dictionary, ConcurrentDictionary, HashSet, Queue
* Threading: Thread, lock()/Monitor, Interlocked*, AutoresetEvent
* Various: Stopwatch, Console, WinForms is used for SOME interactive tests(not needed for operation)
* Some ADO references (Reader/SQLStatement) in segregated data-access components
* Reflection API
* Drawing 2D (Graphics)

**NFX does not use any 3rd party components.** *(with the exception of MySQL/Postgresql drivers)*

