# Contributing to NFX Project

## IMPORTANT MUST READ!!!
This is **NOT a typical bloated .NET system**.
Actually, the **NFX framework has very little to do with a typical Microsoft software stack**, and the purpose of this project is to provide an alternative unified stack of software that uses core CLR functions only and the very base classes (such as list, array, dictionary). 

**.NET Core:**
 as of 06/2017 we are working on a 100% native support of .NET Core 2.x+.
 We are NOT going to support Core <2.x as it lacks proper serialization semantic. We expect to fully support .NET Core in Nov 2017.

 We are planning to support classic .NET Framework 4.5+ in future.


**NFX does not use any 3rd party components** but for some DB-access (MySQL and PostgreSQL are primary targets).

NFX uses the very base classes:
* Basic/primitive types: string, ints, doubles, decimal, dates, +Math
* Parallel task library: some features - create, run, wait for completion,
  Task, Parallel.For/Each
* Collections: List, Dictionary, ConcurrentDictionary, HashSet, Queue
* Threading: Thread, lock()/Monitor, Interlocked*, AutoresetEvent
* Various: Stopwatch, Console, WinForms is used for SOME interactive tests(not needed for operation)
* Some ADO references (Reader/SQLStatement) in segregated data-access components
* Reflection API
* Drawing 2D (Graphics) (subject to change on .Net Core)

**Avoid all non-BCL classes.**

## Naming Conventions
* Instance non-pub fields must begin with m_<PascalCase> i.e.  `"m_BoxColor"`
* Static non-pub fields must begin with   s_<PascalCase> i.e.  `"s_ServerInstance"`
* Thread-static non-pub fields must begin with ts_<PascalCase> i.e. `"ts_LockList"`

* Pub fields: PascalCase and preferably read-only	i.e. `"BoxColor"`
* Pub properties: PascalCase	i.e. `"BoxColor"`
* Pub Methods: PascalCase	i.e. `"GetPriority()"`
* Protected Methods: PascalCase, core virtual overrides Do<PascalCase> i.e. :  pub `Open()` calls `protected virtual DoOpen()`
* Localizable stirng constants must be moved to `StringConsts.cs`. **DO NOT USE .NET localization mechanisms**
* Non-localizable string constants must be declared in `CONSTS` region:
 * Numeric Time span values must end with `_MS`, `_SEC`, `_MIN` specifier. NFX does not use TimeSpan type for storing intervals in props/configs
 * Configuration Section names: `CONFIG_*_SECTION`, Configuration attr names: `CONFIG_*_ATTR`
 * Default values constants should start from `DEFAULT_*`
 * MIN/MAX value bounds should be prefixed with `MIN_*/MAX_*`
* Members prefixed with `"__"` are signifying a hack behaviour which is sometimes needed (see below).
 Business developers should never call these methods/members

## Code File Structure
Try to organize code file structure per the spec below, **in the specific order**:

1. License
1. USINGS - System, NFX, then your namespaces(if any)
1. Region "CONSTS"
1. Region ".ctor" - constructors, and static factories/properties, Destructor() calls
1. Region "Fields/.pvt .flds"
1. Region "Properties" - public properties
1. Region "Pub" - public methods
1. Region "Protected" - protected methods/stubs
1. Region ".pvt/.pvt.impl" - private implementations

Please see EMPTY_CLASS_TEMPLATE.cs under '/Source'

**C# Note:** in C# the access specifiers are not granular enough and in many cases one needs to 
"un-protect" some otherwise read-only field or property (i.e. there is no way to make some member
public ONLY to descendants of X). While it's true that "internal" is sometimes
sufficient, sometimes one needs to create a "hack" setter. In these cases please make an internal
function that starts with "__" (at least one underscore) and declare it near the field/property
 of interest:  `'private int m_Magic;  internal void __setMagic(int val){m_Magic = val;}'`. This 
 approach (vs just making the whole field internal) allows to signify the "non-normal" case.

## DOs and DONTs
* Never return a null from a string property UNLESS you need to signify the absence of a value (this is a rare case).
In most cases (98%) return string properties like `'get{ return m_Name ?? string.Empty;}'`
* DO NOT proliferate redundant argument checks that do not reflect the "business logic" - like much of
 the .NET code does. This is because the NFX code is a runtime for Aum, in Aum code contracts/args checks 
 are done using aspects. No need to write 1000s of IF statements that are not needed. 
 For example, do not check for nulls in this method: `'mystream.CompressInto( another )'`. If the user gets "obj ref" its ok
* DO protect MAJOR methods with arg checks (non null) where the purpose of the argument is not obvious. Example:
 `" code.Analyze( lexer, parser)"` - if lexer is not null then parser must be supplied as well
* DO NOT raise generic exceptions, derive(directly or indirectly) all exceptions from `NFXException` 
* DO NOT use cryptic names
* DO NOT reference 3rd-party (non-clr/system-core) DLLs from NFX
* DO NOT bring-in mixed license code
* DO NOT Rely on the proprietary Microsoft technologies from within NFX: 
   WCF, IIS, PowerShell, ASP.NET, MVC, Razor, MS.SQL, Entity, LINQto*, ActiveDirectory, etc.
   



   




