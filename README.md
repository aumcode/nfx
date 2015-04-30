# NFX
C# Server UNISTACK framework.

This framework is written in C# from scratch and runs on Windows and Linux/Mono servers.
It does not use any 3rd paty compoents but for DB-access (MongoDB and MySQL are primary targets).
NFX uses very BCL:
* Basic/primitive types: string, ints, doubles, decimal, dates, +Math
* Collections: List, Dictionary, ConcurrentDictionary, HashSet
* Threading: Thread, lock()/Monitor, Interlocked*, AutoresetEvent
* Various: Stopwatch, Console, WinForms is used for SOME inetractive tests(not needed for operation)
* Some ADO references (Reader/SQLStatement) in segregated data-access components
* Reflection API

NFX Does not use avoid:
* ASP.NET
* MVC
* WCF
* Entity / LINQ to *
* System.Configuration
* Any Ms-serialization(BinaryFormatter/DataContractSerializer/JSON)
* 100s of "heavy" .NET "typical" classes (DataSet/DataTable/Unity etc.)

UNISTACK = all base components needed to create solutions/applications.

NFX UNISTACK includes:
* Application Container + Dep Injection
* Configuration engine
* Local/Distributed piles/heaps, ability to store hundreds of millions of bjects
  resident in memory for long times without killing GC
* Logging with 8+ destinations/sinks(text, email, flood filter etc.)
* Distributed contact-oriented communication framework NFX.Glue (replaces WCF)
* Security with users, credentials, roles, permissions
* JSON parsing, ser/deser support
* Ultra efficient Binary serialization support
* Erlang CLR support with native types: tuples, lists, pattern matching
* Text lexing parsing and processing pipeline: C#Lexer, JSONlexer/Parser
* RelationalSchema language compiler - generate DDL for different targets
* Templatization engine (for web, emails and not only textual content)
* NFX.WAVE - Web Server with hybrid injectable threading model (replaces IIS + ASP.NET)
* NFX.WAVE.Mvc - MVC framewok wor web pages
* WV.js - a web compoent library auto-binadble to server MVC/MVVM
* Database access layer with virtual commands/queries/transactions
* ID generation - GlobalDistributed IDS (GDID), FID - fast process-wide ID
* Virtual Social Network - Twitter/Facebook/Google+ et al
* Virtual Payment Processing - Stripe,PayPal providers
* Virtual File Systems - AmazonS3, SVN, Local
* QR Code Creation
* In progress: Virtual document model wth rendering to PDF, HTML and other formats
* In Progress: PDF DOM model + rendering







