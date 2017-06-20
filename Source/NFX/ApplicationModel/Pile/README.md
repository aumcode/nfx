# NFX Object Pile

## Overview

**Object Pile** is a technique of utilization of large amounts of RAM - (tens to hundreds of gigabytes) in-process in server applications via **regular CLR object** allocations, making **Big Memory** applications possible on a 100% managed runtime.

**Pile solves the notorious GC stalls problem** which existed since the first days of managed memory model. Unfortunately, the GC blocking can not be eradicated completely - it is the price we have to pay for the higher-level memory model. In recent years there have been significant improvements in GC mechanisms [see this MSDN blog post](https://blogs.msdn.microsoft.com/dotnet/2012/07/20/the-net-framework-4-5-includes-new-garbage-collector-enhancements-for-client-and-server-apps/). The GC became less intrusive, and more informative, so  we can now know about the upcoming  blocking GC phase and try to divert traffic to a nearby server. This has helped many use-cases. 
 
There is a number of systems, however, where GC remains problematic despite all of the improvements made in the runtime. It is a customary routine to use the out-of-box/process solutions such as Redis in such systems, however keep in mind that out-of-process stores still require serialization and transportation overhead.
  
**The GC starts to interfere when:**
*(regardless of GC mode, concurrency, notification and critical regions)*

* A process needs to store 100s of millions of objects and have predictable SLA (response times and throughput) 
* Long-lived objects (i.e. days) - so they all go to Gen2
* Objects are mutable - data changes, (not a read-only set)
* Locality of reference (box-bound dataset) is important
 
Big Memory Pile solves the GC problems by using the transparent serialization of CLR object graphs into large byte arrays, effectively “hiding” the objects from GC’s reach. Not all object types need to be fully serialized  - strings and byte[] get written into Pile as buffers bypassing all serialization mechanisms yielding over 6 M inserts/second for a 64 char string on a 6 core box.
 
The key benefit is the **practicality** of this **approach which obviates the construction of custom DTOs and other hack methods just to releave the pressure on the GC**. The real life cases have shown the phenomenal overall performance.

## Performance

## Examples
[IPile](IPile.cs) provides an abstraction of memory managers. NFX Provides two implementations out of the box:
* [DefaultPile](DefaultPile.cs) - stores data in byte[]
* [MMFPile](MMFPile.cs) - stores data in Memory Mapped Files
Both implemntations are 100% managed code C# only, no C++ involved.

**1 - Create IPile-implementing Instance**
Depending on your objectives you can allocate by hand, or use dependency injection:

```cs
  private IPileImplemntation m_Pile;
  .....
  //make by hand
  m_Pile = new DefaultPile();
  //or
  m_Pile = new MMFPile();
  //or inject from config
  m_Pile = FactoryUtils.MakeAndConfigure(configNode, typeof(DefaultPile));//if type is not specified in config
                                                                          //use DefaultPile as default type
  .....
  m_Pile.Start();//Start the service
  
  .... use pile ....
  
  //finalize:
  m_Pile.WaitForCompleteStop();
  m_Pile.Dispose();
  //or just 1 line instead of the 2
  DisposableObject.DisposeAndNull(ref m_Pile);
```
The MMFPile requires setting the `DataDirectoryRoot` to an existing folder, otherwise the MMPile would not start.

**2 - Use Raw Memory Allocator **

Raw memory allocator works with byte[] bypassing any serialization; this code yeilds multi-million ops/sec while inserting byte[64]:

```cs
  var buffer = new byte[1234];
  var ptr = pile.Put(buffer);
  ...
  var got = pile.Get(ptr) as byte[];
  Assert.IsNotNull(got);
  Assert.AreEqual(1234, got.Length);

```
we can do the same with strings, as strings use UTF8 direct encoding into memory buffer. The performance is similar.

** 3 - Working with CLR objects **

The **major business benefit of the Pile** is that it allows you to work with pretty much **any .NET types without special treatment**. You do not need to create and maintain extra DTO copies, instead - work with your business domain.

Working with regular .NET objects is no different than the example above, byte[] and strings are special types that bypass serializer altogether, whereas any other types go via SlimSerializer which is used in the special Batch mode for performance. See the (IPile Interface)[IPile.cs].

There are a few cases that Pile does not support by design:
* Classes with unmanaged handles/resources (unless they are serializable via ISerializable/[OnSer/Deser] mechanisms)
* Delegates/function pointers

Keep in mind: if you serialize a huge object graph into a Pile - it will take it as long as it fits in a segmnet (256 Mb by default), however this is not a good and intended design of using Pile. Store smaller business- oriented objects instead. If you need to store huge graphs use [ICache](ICache.cs) instead (see below).




## Resources 
The topic has been covered at length here:

[InfoQ -Big Memory .NET Part 1 – The Challenges in Handling 1 Billion Resident Business Objects](https://www.infoq.com/articles/Big-Memory-Part-1)

[InfoQ -Big Memory .NET Part 2 – The Challenges in Handling 1 Billion Resident Business Objects](https://www.infoq.com/articles/Big-Memory-Part-2)
