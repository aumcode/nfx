using System;
using System.IO;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;


using NFX;
using NFX.ApplicationModel;
using NFX.ApplicationModel.Pile;
using NFX.DataAccess;
using NFX.DataAccess.Distributed;
using NFX.IO;
using NFX.Serialization.Slim;

namespace ConsoleTest
{
  class CacheTest
  {
       private static LocalCache makeCache()
        {
           var cache = new LocalCache();
           cache.Pile = new DefaultPile(cache);
           cache.Configure(null);
           cache.Start();
           return cache;
        }


      public static void Run(int workers, int tables, int putCount, int durationSec)
      {
        var start =DateTime.UtcNow;

        using (var cache = makeCache())
        {
          var tasks = new Task[workers];

          var totalGet = 0;
          var totalPut = 0;
          var totalRem = 0;

          var getFound = 0;
          var putInsert = 0;
          var removed = 0;

          for(var i=0; i<workers; i++)
             tasks[i] = 
              Task.Factory.StartNew( 
               ()=>
               {
                 while(true)
                 {
                   var now = DateTime.UtcNow;
                   if ((now-start).TotalSeconds>=durationSec) return;

                   var t = NFX.ExternalRandomGenerator.Instance.NextScaledRandomInteger(0, tables);
                   var tbl = cache.GetOrCreateTable<string>("tbl_"+t);

                   var get = NFX.ExternalRandomGenerator.Instance.NextScaledRandomInteger(putCount*2, putCount * 4);
                   var put = NFX.ExternalRandomGenerator.Instance.NextScaledRandomInteger(putCount / 2, putCount);
                   var remove = NFX.ExternalRandomGenerator.Instance.NextScaledRandomInteger(0, putCount);

                   Interlocked.Add(ref totalGet, get);
                   Interlocked.Add(ref totalPut, put);
                   Interlocked.Add(ref totalRem, remove);

                   for(var j=0; j<get; j++) 
                    if (null!=tbl.Get( NFX.ExternalRandomGenerator.Instance.NextScaledRandomInteger(0, 10000000).ToString() )) Interlocked.Increment(ref getFound);

                   for(var j=0; j<put; j++) 
                    if (PutResult.Inserted==tbl.Put( 
                                                     NFX.ExternalRandomGenerator.Instance.NextScaledRandomInteger(0, 10000000).ToString(),
                                                     NFX.Parsing.NaturalTextGenerator.Generate()                     
                                                   )) Interlocked.Increment(ref putInsert);
                  
                   for(var j=0; j<remove; j++) 
                    if (tbl.Remove( 
                                   NFX.ExternalRandomGenerator.Instance.NextScaledRandomInteger(0, 1000000).ToString()
                                  )) Interlocked.Increment(ref removed);


                 }
               }
              , TaskCreationOptions.LongRunning );

          Task.WaitAll( tasks );

          Console.WriteLine("{0} workers, {1} tables, {2} put, {3} sec duration", workers, tables, putCount, durationSec);
          Console.WriteLine("-----------------------------------------------------------");

          Console.WriteLine("Total Gets: {0}, found {1}", totalGet, getFound);
          Console.WriteLine("Total Puts: {0}, inserted {1}", totalPut, putInsert);
          Console.WriteLine("Total Removes: {0}, removed {1}", totalRem, removed);


          Console.WriteLine();
        }
      }
  }
}
