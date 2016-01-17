#NFX/UNISTACK Overview

##Purpose
NFX provides a **single library** for any developer to create a complex/possibly distributed/large-scale
server system. It MAY be a web-related system, but does not have to be (i.e. database engine).

Most features of NFX **have been used in production** for over a year, base features over 8 years.

NFX is the first (that we are aware of) practical implementation 
of **UNISTACK software development methodology/process**, where all tiers of code/system/team/business are 
interfacing via the same unified protocol. Simply put - you achieve a great deal of efficiency by reducing numbers
of redundant standards that your code/team members have to support. The effect of **intellectual property compression** 
is promoted by UNISTACK, and becomes self-evident after the great reduction (or even complete absence) of 
typical code/meetings/problems/delays during project execution is observed.

Another important aspect is **platform transparency**. 

NFX is **not a typical .NET** library as it **avoids all of the major Microsoft technologies** which are platform-specific, bloated with legacy support and patents.
NFX does not use/need: ASP.net, WCF, ActiveDirectory*, EntityFramework, MVC, Razor, EntLib, COM etc... 

NOTE: **We are not criticizing anyone** (Microsoft or any 3rd parties), we are just sharing our view under the different angle. We understand that our approach is not applicable to much of the existing code or corporate-regulated large companies.

##NFX Features
NFX is a UNISTACK library. As such, it has a very broad horizon of covered features. This is because in NFX 
everything is re-using as much as possible from existing code base.

Practical example: 
 logging, glue, web server (and all other components) use NFX.Environment.Configuration, instead of relying 
 on log4net, nLog, EntLib, which all use different configuration mechanisms, in NFX all components are configured
 in a unified way, consequently **one does not need to remember** "how to declare a variable in config file for A 
 logger vs B logger".
