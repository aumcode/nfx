# Unistack

General unistack software development methodology/process conception is based on the following principles:

* You get a single library, where all sub-systems work in concert

* You get freedom from choice - as all system choices are made, you can change them, but why would you use, say nLog, if you already have logger as good if not better

* You get compression. Intellectual property compression. Less things to remember and keep in your head

* You get compression. Code compression - less code

* You get independence - you don’t care about new versions of a small lib that just does this little xyz task

* You get **unification** of thought, pattern and practice

**NFX** is a unistack philosophy incarnation where all tiers of code/system/team/business are interfacing via the same unified protocol.

**NFX** library

* Is written in C# from scratch

* Uses very BCL: arrays, `List<>`, `Dictionary<>`, `Thread`, `Monitor`, `Task` (only basic)

* Compiles on VS and Mono. Runs on Linux and Windows

* Has 95% of what any distributed/web/cluster app needs in 1 lib

* Does not depend on any 3rd parties but: MySQL client (if you need it)

* Does not use MS-Specific stuff like: IIS, ActiveDirectory, WCF, MVC, EF...

* Uses 2 language: C# and JavaScript for Web UI

* Does NOT concentrate on WEB-only, as there are many other cluster/cloud application types (i.e. high frequency trading)