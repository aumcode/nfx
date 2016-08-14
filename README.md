# NFX Server UNISTACK framework.


[![Join the chat at https://gitter.im/aumcode/nfx](https://badges.gitter.im/aumcode/nfx.svg)](https://gitter.im/aumcode/nfx?utm_source=badge&utm_medium=badge&utm_campaign=pr-badge&utm_content=badge)

Real feeadback from developers and users of NFX - http://www.nfxsucks.com/

License: Apache 2.0

This framework is written in C# from scratch and runs on Windows and Linux/Mono servers.

**GUIDES**:
NEW 20160117, we are adding:
 [NFX Documentation/Guides are here](Guides/README.md)

**NUGET**:
 https://www.nuget.org/packages/NFX/
 
 `pm> install-package NFX`

**Various Demo Projects**:
 https://github.com/aumcode/nfx-demos

**IMPORTANT!**

This is NOT a typical .NET system.
Actually, the NFX framework has very little to do with Microsoft software stack, and the purpose
of this project is to provide an alternative unified stack of software that uses core CLR functions
and very base classes (such as list, array, dictionary). NFX is a BaseClassLibrary for Aum 
programming language that we are working on. For now, we have used C# and very-BCL from .NET framework.


NFX does not use any 3rd party components but for some DB-access (MongoDB and MySQL are primary targets).
NFX uses very BCL:
* Basic/primitive types: string, ints, doubles, decimal, dates, +Math
* Parallel task library: 25% of features - create, run, wait for completion,
  Task, Parallel.For/Each
* Collections: List, Dictionary, ConcurrentDictionary, HashSet, Queue
* Threading: Thread, lock()/Monitor, Interlocked*, AutoresetEvent
* Various: Stopwatch, Console, WinForms is used for SOME interactive tests(not needed for operation)
* Some ADO references (Reader/SQLStatement) in segregated data-access components
* Reflection API
* Drawing 2D (Graphics)

NFX Does NOT use/avoids:
* Windows-specific functions like kernel, user, gdi (there are some <10, we are getting rid of them)
* Windows-specific technologies (IIS, MS SQL Server, Active Directory, Windows Cluster, COM, Azure)
* .NET Specific tools: NCover, MsTest, all sorts of VS plugins - 
  any developer must be able to start developing in < 30 minutes after getting the code. No setup time/installs to run
* ASP.NET
* MVC
* WCF
* Silverlight, WPF, Phone
* Entity / LINQ to * (NFX concentrates on hybrid data stores with scaffolding and virtual commands, not only SQL)
* System.Configuration.*
* ASYNC / AWAIT - avoided on purpose
* Any Ms-serialization(BinaryFormatter/DataContractSerializer/JSON)
* 100s of "heavy" .NET "typical" classes (DataSet/DataTable/Unity etc.)
* References typical to many .NET projects:
    log4net, nLog, EntLib, Castle, NSpring, ServiceStack, Newtonsoft etc.
* No NuGet dependencies within NFX, the whole idea of "packages" is contrary to Unistack 

UNISTACK = all base components needed to create solutions/applications.

NFX UNISTACK includes:
* Application Container + Dependency Injection facilities
* Configuration engine
* Local/Distributed piles/heaps (**BigMemory**), ability to store hundreds of millions of objects
  resident in memory for long times without killing GC
* Logging with 8+ destinations/sinks(text, email, flood filter etc.)
* Distributed contact-oriented communication framework NFX.Glue (replaces WCF)
* Security with users, credentials, roles, permissions
* JSON parsing, ser/deser support
* Ultra efficient Binary serialization support
* Erlang CLR support with native types: tuples, lists, pattern matching
    - [Overview blog](http://blog.aumcode.com/2013/10/nfx-native-interoperability-of-net-with.html)
    - [Interop with Erlang/Mnesia and RPC abstraction](https://www.youtube.com/watch?v=o9utCAMLydA)
* Text lexing/parsing and processing pipeline: C# lexer, JSON lexer/Parser
* RelationalSchema language compiler - generate DDL for different targets
* Templatization engine (for web, emails and not only textual content)
* NFX.WAVE - Web Server with hybrid injectable threading model (replaces IIS + ASP.NET)
* NFX.WAVE.Mvc - MVC framework for web pages
* WV.js - a web component library auto-bindable to server MVC/MVVM
* Database access layer with virtual commands/queries/transactions
* ID generation - GlobalDistributed IDS (GDID), FID - fast process-wide ID
* Virtual Social Network - Twitter/Facebook/Google+ et al
* Virtual Payment Processing - Stripe,PayPal providers
* Virtual File Systems - AmazonS3, SVN, Local
* QR Code Creation
* In progress: Virtual document model with rendering to PDF, HTML and other formats
* In Progress: PDF DOM model + rendering







