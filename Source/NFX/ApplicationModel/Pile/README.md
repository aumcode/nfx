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
 
The key benefit is the **practicality** of this **approach which obviates the need to construct custom DTOs and other hack methods just to relieve the pressure on the GC**. The real life cases have shown the overall performance of the solution to be much higher than using extrenal stores.

## How it Works
Pile provides a layered design, at its core there is a [memory allocator](PileImpl/DefaultPileBase.cs) that manages the sub-allocation within large ["segments"](PileImpl/DefaultPileBase.Segment.cs) which are backed by either [byte[]](PileImpl/LocalMemory.cs) or [MemoryMappedFiles](PileImpl/MMFMemory.cs). 

The main point of Pile is to exchange a CLR reference (which GC "sees") for a [PilePointer](PilePointer.cs) value type ( a `struct` of 3 ints which GC does not "see"). After you put an object into Pile, you need to keep a `PilePointer` reference (somewhere, see Cache below) which is invisible to GC and can be used later to resurrect the original object back into "real" CLR heap.

The `PilePointer` instances can be kept in regular CLR heap, in classes like `List<>` or `Dictionary<>` as they do not create any GC scan pressure. They can also be kept in other objects stored directly in Pile, thus it is possible to create **complex data structures (i.e. a PrefixTree)** that contains hundreds of millions of entries **completely in Pile**. 

When you put an object into a Pile (or cache), it treats `byte[]` ans `string` primitives as **directly writable into the memory** buffer, - this is called a "raw" mode - it works **much faster than arbitrary objects** as it bypasses serialization completely *(see benchmarks below)*. Other objects get serialized using [Slim serializer](../../Serialization/Slim/SlimSerializer.cs) which is very efficient at turning true CLR object graphs (even with cycles and polymorphism) into `byte[]`.

NFX features [ICache](ICache.cs) store which is based on [IPile](IPile.cs) memory manager. This allows for storage of very many objects in named tables - out of GC's reach. Tables are akin to thread-safe named dictionary instances. 

NFX solution provides a full-featured in-process caching server implementation of [ICache](ICache.cs) interface - [LocalCache](LocalCache.cs). As described above, the benefit of local cache is in the fact of in-process data availability - no socket connections/context switching needs to take place, consequently the performance is higher than that of an out-of-process store.


## Performance
Machine: Intel Core i7-3930K 3.2. Ghz, 6 HT Cores, 64 GB DDR3 87% free, Win 7 64bit, VS 2017, .NET 4.5, mid-grade SSD 1 TB 50% free

--------
Benchmarking Pile - insert **200,000,000** instances of **string[32]** each, by **12 threads**:

|    | Default Pile  |   MMF Pile   |
|----|-----------|--------------|
| Duration   | 24 sec |  41 sec |
| Throughput | 8.3M ops/sec | 4.9M ops/sec|
| RAM        | 8.5 GB  |  8.5 GB|
| Full GC    | < 8 ms  | < 10ms |
| Flush all data on Stop() | - | 10 sec|
| Load all data on Start()| - | 48 sec @ 177 mbyte/sec|

--------

Benchmarking Pile - insert **200,000,000** instances of **Person** (class with [7 fields](https://github.com/aumcode/nfx/blob/master/Source/Testing/Manual/WinFormsTest/PileForm.cs#L77-L84)), by **12 threads**:

|    | Default Pile  |   MMF Pile   |
|----|-----------|--------------|
| Duration   | 85 sec |  101 sec |
| Throughput | 2.4M ops/sec | 1.9M ops/sec|
| RAM        | 14.5 GB  |  14.5 GB|
| Full GC    | < 10 ms  | < 10ms |
| Flush all data on Stop() | - | 30 sec|
| Load all data on Start() | - | 50 sec @ 290 mbyte/sec|



## Examples
[IPile](IPile.cs) provides an abstraction of memory managers. NFX Provides two implementations out of the box:
* [DefaultPile](DefaultPile.cs) - stores data in byte[]
* [MMFPile](MMFPile.cs) - stores data in Memory Mapped Files
Both implemntations are 100% managed code C# only, no C++ involved.

**1 - Create IPile-implementing Instance**

Depending on your objectives you can allocate Pile by hand, or use dependency injection:

```cs
  private IPileImplemntation m_Pile;
  .....
  //make by hand
  m_Pile = new DefaultPile();
  //or
  m_Pile = new MMFPile();
  //or inject from config
  //if type is not specified in config use DefaultPile as default type
  m_Pile = FactoryUtils.MakeAndConfigure(configNode, typeof(DefaultPile));
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

**2 - Use Raw Memory Allocator**

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

**3 - Working with CLR objects**

The **major business benefit of the Pile** is that it allows you to work with pretty much **any .NET types without special treatment**. You do not need to create and maintain extra DTO copies, instead - work with your business domain.

Working with regular .NET objects is no different than the example above, byte[] and strings are special types that bypass serializer altogether, whereas any other types go via SlimSerializer which is used in the special Batch mode for performance. See the [IPile Interface](IPile.cs).

There are a few cases that Pile does not support by design:
* Classes with unmanaged handles/resources (unless they are serializable via ISerializable/[OnSer/Deser] mechanisms)
* Delegates/function pointers

```cs
  var person = new Person{ LastName="Shoikhed", FirstName="Dodik", Age=54 };
  var ptr = pile.Put(person);
  ...
  var got = pile.Get(ptr) as Person;
  Console.WriteLine( got.LastName );//Shoikhed
```

An example of a linked list with in-place mutation (changing data at existing pointer):

```cs
  public class ListNode
  {
    public PilePointer Previous;
    public PilePointer Next;
    public PilePointer Value;
  }
  ...
  private IPile m_Pile;//big memory pile 
  private PilePointer m_First;//list head
  private PilePointer m_Last;//list tail
  ...
  //Append a person instance to a person linked list stored in a Pile
  //returns last node
  public PilePointer Append(Person person)
  {
    var pValue = m_Pile.Put(person);

    var newLast = new ListNode{ Previous = m_Last, 
                                Next = PilePointer.Invalid, 
                                Value = pValue};
    
    var existingLast = m_Pile.Get(m_Last);
    existingLast.Next = node;
    m_Pile.Put(m_Last, existingLast);//in-place edit at the existing ptr m_Last
    m_Last = m_Pile.Put(newLast);//add new node to the tail

    return m_Last;
  }                                
```

Keep in mind: if you serialize a huge object graph into a Pile - it will take it as long as it fits in a segment (256 Mb by default), however this is not a good and intended design of using Pile. Store smaller business- oriented objects instead. If you need to store huge graphs use [ICache](ICache.cs) (see below).

**4 - Caching**

Making cache instance:
```cs
  var cache = new LocalCache();
  cache.Pile = new DefaultPile(cache);//Pile owned by cache
  cache.Configure(conf);
  cache.Start();
     ...
     //use
     ...
  //this will dispose Pile because it is owned by cache
  DisposableObject.DisposeandNull(ref cache);
```
Put all tables in the `Durable` CollisionMode. In this mode no data is going to get lost. Cache tables will work just like dictionary. In the `Speculative` mode the tables skip rehashing for speed - hence some data may get lost as governed by item `priority` on `Put()`:

```cs
  //Specify TableOptions for ALL tables
  cache.DefaultTableOptions = new TableOptions("*") 
  {
    CollisionMode = CollisionMode.Durable
  };
```

Create tables and put some data, note the **use of comparers**:

```cs
   var tA = cache.GetOrCreateTable<string>("A", StringComparer.Ordinal);
   var tB = cache.GetOrCreateTable<string>("B", StringComparer.OrdinalIgnoreCase);

   Assert.AreEqual(PutResult.Inserted, tA.Put("key1", "avalue1"));
   Assert.AreEqual(PutResult.Inserted, tA.Put("Key1", "avalue2"));
   Assert.AreEqual(PutResult.Inserted, tB.Put("key1", "bvalue1"));
   Assert.AreEqual(PutResult.Replaced, tB.Put("Key1", "bvalue2")); 
```

Max age, priority and absolute expiration:
```cs
  var tA = cache.GetOrCreateTable<int>("A");
  var data = new Person{...};
  var theEnd = App.TimeSource.UTCNow.AddHours(73);
  ta.Put(123, myData, maxAgeSec: 78000, priority: 2, absoluteExpirationUTC: theEnd);
  .....

  //set existing object age filter
  var newPerson = tA.Get(123, ageSec: 32) as Person;
  if (newPerson!=null)...
  ....
  ta.Rejuvenate(123);//true - reset object age to zero
  ....
  tA.Remove(123);//true
```

Enumerate all entries:
```cs
 var tA = cache.GetOrCreateTable<int>("A");
 var all = tA.AsEnumerable(withValues: true);

 foreach(var entry in all)
 {
    ...
    entry.Key
    entry.AgeSec
    entry.Priority
    entry.MaxAgeSec
    entry.ExpirationUTC

    // Returns value only if enumerator is in materializing mode, 
    //obtained by a call to AsEnumerable(withValues: true)
    entry.Value
 }
```


Imposing limits:
```cs
  var tA = cache.GetOrCreateTable<int>("A");
  tA.Options.MaximumCapacity = 1800;
  tA.Options.ShrinkFactor = 0.5d;
```

Atomic GetOrPut():
```cs
  var tA = cache.GetOrCreateTable<int>("A");

  tA.Put(1, "value 1");
  tA.Put(122, "value 122");

  PutResult? pResult;
  var v = tA.GetOrPut(2, (t, k, _) => "value "+k.ToString(), null, out pResult);
  Assert.AreEqual( "value 2", v);
  Assert.IsTrue( pResult.HasValue );
  Assert.AreEqual( PutResult.Inserted, pResult.Value);
```


Configure tables individually:
```cs
store
{
  default-table-options
  {
    initial-capacity=1000000
    detailed-instrumentation=true
  }

  table
  {
    name='A'
    minimum-capacity=800000
    maximum-capacity=987654321
    initial-capacity=780000
    growth-factor=2.3
    shrink-factor=0.55
    load-factor-lwm=0.1
    load-factor-hwm=0.9
    default-max-age-sec=145
    detailed-instrumentation=true
  }

  table
  {
    name='B'
    maximum-capacity=256000
    detailed-instrumentation=false
  }
}
```


## Resources 
The topic has been covered at length here:

[InfoQ -Big Memory .NET Part 1 – The Challenges in Handling 1 Billion Resident Business Objects](https://www.infoq.com/articles/Big-Memory-Part-1)

[InfoQ -Big Memory .NET Part 2 – The Challenges in Handling 1 Billion Resident Business Objects](https://www.infoq.com/articles/Big-Memory-Part-2)
