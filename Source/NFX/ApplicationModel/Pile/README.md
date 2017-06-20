# NFX Object Pile

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

The topic has been covered at length here:

[InfoQ -Big Memory .NET Part 1 – The Challenges in Handling 1 Billion Resident Business Objects](https://www.infoq.com/articles/Big-Memory-Part-1)

[InfoQ -Big Memory .NET Part 2 – The Challenges in Handling 1 Billion Resident Business Objects](https://www.infoq.com/articles/Big-Memory-Part-2)
