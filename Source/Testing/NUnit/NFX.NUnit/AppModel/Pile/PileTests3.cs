/*<FILE_LICENSE>
* NFX (.NET Framework Extension) Unistack Library
* Copyright 2003-2017 ITAdapter Corp. Inc.
*
* Licensed under the Apache License, Version 2.0 (the "License");
* you may not use this file except in compliance with the License.
* You may obtain a copy of the License at
*
* http://www.apache.org/licenses/LICENSE-2.0
*
* Unless required by applicable law or agreed to in writing, software
* distributed under the License is distributed on an "AS IS" BASIS,
* WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
* See the License for the specific language governing permissions and
* limitations under the License.
</FILE_LICENSE>*/
using System;
using System.IO;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Diagnostics;
using System.Threading;
using System.Threading.Tasks;

using NUnit.Framework;

using NFX;
using NFX.ApplicationModel;
using NFX.ApplicationModel.Pile;
using NFX.DataAccess;
using NFX.DataAccess.Distributed;
using NFX.DataAccess.CRUD;
using NFX.IO;
using NFX.Serialization.Slim;
using NFX.Serialization.JSON;


namespace NFX.NUnit.AppModel.Pile
{
  [TestFixture]
  public class PileTests3
  {
      [SetUp]
      public void SetUp()
      {
        GC.Collect();
      }

      [TearDown]
      public void TearDown()
      {
        GC.Collect();
      }

/*
20170120 SEXTOD
Dictionary Optimization, 111000
Created 111,000 in 5,655 ms at 19,629 ops/sec
Occupied bytes 805,306,368
Read 111,000 in 8,468 ms at 13,142 ops/sec
===============================================
20170120 SEXTOD
Array, Optimization, 111000
Created 111,000 in 5,200 ms at 21,387 ops/sec
Occupied bytes 805,306,368
Read 111,000 in 6,247 ms at 17,906 ops/sec

 Write: + 8.9%
 Read:  +36.2%

*/



      [TestCase(111000)]
      public void TestHeavyBenchmark(int count)
      {
        using (var pile = new DefaultPile())
        {
          pile.Start();
          var ipile = pile as IPile;

          var data = HeavyProductRow.Build();
          var pp = PilePointer.Invalid;

          var sw = Stopwatch.StartNew();
          for(var i=0; i<count; i++)
          {
            pp = ipile.Put(data);
          }

          var wms = sw.ElapsedMilliseconds;


          Console.WriteLine("Created {0:n0} in {1:n0} ms at {2:n0} ops/sec".Args(count, wms, count / (wms / 1000d)));
          Console.WriteLine("Occupied bytes {0:n0}".Args(pile.AllocatedMemoryBytes));

          HeavyProductRow got = null;
          sw.Restart();
          for(var i=0; i<count; i++)
          {
            got = ipile.Get(pp) as HeavyProductRow;
            Aver.IsNotNull( got );

            Aver.AreEqual( data.GDID, got.GDID);
          }
          var rms = sw.ElapsedMilliseconds;
          Console.WriteLine("Read {0:n0} in {1:n0} ms at {2:n0} ops/sec".Args(count, rms, count / (rms / 1000d)));

          Aver.AreEqual(3, got.SKUs.Length);
          Aver.AreEqual(6, got.Prices.Length);
        }
      }


      [Serializable]
      public class HeavyProductRow : TypedRow
      {
          public class SKURow : TypedRow
          {
            [Field] public GDID GDID { get; set;}
            [Field] public string SKU { get; set;}
            [Field] public string Image { get; set;}
            [Field] public long StockCount { get; set;}
            [Field] public NLSMap Name    { get; set;}
            [Field] public NLSMap About   { get; set;}
            [Field] public NLSMap Terms   { get; set;}
            [Field] public NLSMap Privacy { get; set;}
            [Field] public string[] Options { get; set;}
          }

          public class PriceRow : TypedRow
          {
            [Field] public GDID GDID { get; set;}
            [Field] public int Qty { get; set;}
            [Field] public decimal Price { get; set;}
            [Field] public decimal? CompareAt{ get; set;}
            [Field] public string[] Options { get; set;}
            [Field] public bool HasDiscount { get; set;}
            [Field] public bool IsResellable{ get; set;}
          }

        [Field] public GDID GDID { get; set;}

        [Field] public int Code { get; set;}

        [Field] public string Manufacturer { get; set;}
        [Field] public string Brand { get; set;}
        [Field] public string MasterSKU { get; set;}
        [Field] public long StockCount { get; set;}

        [Field] public NLSMap Name    { get; set;}
        [Field] public NLSMap About   { get; set;}
        [Field] public NLSMap Terms   { get; set;}
        [Field] public NLSMap Privacy { get; set;}

        [Field] public SKURow[] SKUs { get; set;}
        [Field] public PriceRow[] Prices { get; set;}


        public static HeavyProductRow Build()
        {
          var result = new HeavyProductRow()
          {
            GDID = new GDID(23, 12, 327647823648),
            Code = 27687443,
            Manufacturer = "Roskosmos",
            Brand = "Soyuz",
            MasterSKU = "KB787-2323RBI-27469XZ0",
            StockCount = 178,

            Name = new NLSMap.Builder().Add("eng", "dhfsfh sk wf i", "wuh we fwuih fiuqwhfihopqihuwihuehwqdiuhwq iduhiufhiushfhsdufh")
                                       .Add("rus", "дфио ящепоийр ще йщяей","ио я ящ х асдйий дй ияийий йидйямям ясК  йкйкйд йп йдпйд йпя")
                                       .Add("zzz", "wqe  i wqoiropi w","j wqeljr rjwlqjer wegfhspudif qfhuhf hsafuih fqhfjsahf hsf qwhj fh")
                                       .Map,

            About = new NLSMap.Builder().Add("eng", "dhfre", "weqr we wer wrwuh we fwuih fiuqwhfihopqihuwihuehwqdiuhwq iduhiufhiushfhsdufh")
                                       .Add("rus", "дфио ящепойр ще йщяейийр ще йщяей","ио я ящ хйр ще йщrrwrяей асдйий дй ияийий йидйямям ясК  йкйкйд йп йдпйд йпя")
                                       .Map,

            Terms = new NLSMap.Builder().Add("eng", "d juijij hfre", "  klj ldjw ;lej ;lwje d;ljw[qdj wqegrwqer h[wqoeir wqhj hwqoei hrwqoih ")
                                       .Add("rus", "дфио ящепоийр ще йщяей","ио я ящ х асдйиwй дй ияийий йиwreдйямям ясК  йкйкйд йп йдпйд йпя")
                                       .Map,

            Privacy = new NLSMap.Builder().Add("eng", "hh hio ", "j j wlefj lwj fjw ljf  ")
                                       .Add("rus", "дфи ящщяей","иомям ясК  йкйкйд йп йдпйд йпя")
                                       .Add("zzz", "дфи ящщяей","иомям ясК  йкйкйд йп йдпйд йпя")
                                       .Add("zza", "дфи ящщяей","иомям ясК  йкйк5йд йп йдпйд йпя")
                                       .Add("zzb", "дфи5 ящщяей","иомям ясК  йкйкйд23 йп йдпйд йпя")
                                       .Add("zzc", "дфи3 ящщяей","иомям ясКt  йкйкйд йп345 йдпйд йпя")
                                       .Add("zzd", "дфи ertящщяей","иомям ясК  йкйкйд й345п йдпйд йпя")
                                       .Add("zze", "дфиet ящщяей","иомям ясК  йкйкйд йп534 йдпйд йпя")
                                       .Map,

            SKUs = new SKURow[]
            {
              new SKURow
              {
                GDID = new GDID(3, 4, 2143123123), SKU = "KB787-2323RBI-27469XZ0-01", Image = "/dhasuhdiuwqgerwiegfr.jpg", StockCount = 18,
                Options = new string[]{"", "large", "green", "", ""},
                  Name = new NLSMap.Builder().Add("eng", "dhfsfh sk wf i", "wuh we fwuih fiuqwhfihopqihuwihuehwqdiuhwq iduhiufhiushfhsdufh")
                                       .Add("rus", "дфио ящепоийр ще йщяей","ио я ящ х асдйий дй ияийий йидйямям ясК  йкйкйд йп йдпйд йпя")
                                       .Add("zzz", "wqe  i wqoiropi w","j wqeljr rjwlqjer wegfhspudif qfhuhf hsafuih fqhfjsahf hsf qwhj fh")
                                       .Map,

                  About = new NLSMap.Builder().Add("eng", "dhfre", "weqr we wer wrwuh we fwuih fiuqwhfihopqihuwihuehwqdiuhwq iduhiufhiushfhsdufh")
                                             .Add("rus", "дфио ящепойр ще йщяейийр ще йщяей","ио я ящ хйр ще йщrrwrяей асдйий дй ияийий йидйямям ясК  йкйкйд йп йдпйд йпя")
                                             .Map,

                  Terms = new NLSMap.Builder().Add("eng", "d juijij hfre", "  klj ldjw ;lej ;lwje d;ljw[qdj wqegrwqer h[wqoeir wqhj hwqoei hrwqoih ")
                                             .Add("rus", "дфио ящепоийр ще йщяей","ио я ящ х асдйиwй дй ияийий йиwreдйямям ясК  йкйкйд йп йдпйд йпя")
                                             .Map,

                  Privacy = new NLSMap.Builder().Add("eng", "hh hio ", "j j wlefj lwj fjw ljf  ")
                                             .Add("rus", "дфи ящщяей","иомям ясК  йкйкйд йп йдпйд йпя")
                                             .Add("zzz", "дфи ящщяей","иомям ясК  йкйкйд йп йдпйд йпя")
                                             .Add("zza", "дфи ящщяей","иомям ясК  йкйк5йд йп йдпйд йпя")
                                             .Add("zzb", "дфи5 ящщяей","иомям ясК  йкйкйд23 йп йдпйд йпя")
                                             .Add("zzc", "дфи3 ящщяей","иомям ясКt  йкйкйд йп345 йдпйд йпя")
                                             .Add("zzd", "дфи ertящщяей","иомям ясК  йкйкйд й345п йдпйд йпя")
                                             .Add("zze", "дфиet ящщяей","иомям ясК  йкйкйд йп534 йдпйд йпя")
                                             .Map,
              },
              new SKURow
              {
                GDID = new GDID(3, 4, 21431343), SKU = "KB787-2323RBI-27469XZ0-02", Image = "/dha2342hdiuwqgerwiegfr.jpg", StockCount = 18,
                Options = new string[]{"aaaaaaaa", "large", "green", "", ""},
                  Name = new NLSMap.Builder().Add("eng", "dh54fsfh sk wf i", "wuh we fwuih fiuqwhfihopqihuwihuehwqdiuhwq iduhiufhiushfhsdufh")
                                       .Add("rus", "дфио я345щепоийр ще йщяей","ио я ящ х асдйий дй ияийий йидйямям ясК  йкйкйд йп йдпйд йпя")
                                       .Add("zzz", "wqe  i wqo345iropi w","j wqeljr rjwlqjer wegfhspudif qfhuhf hsafuih fqhfjsahf hsf qwhj fh")
                                       .Map,

                  About = new NLSMap.Builder().Add("eng", "dhfre5", "weqr we w53er wrwuh we fwuih fiuqwhfihopqihuwihuehwqdiuhwq iduhiufhiushfhsdufh")
                                             .Add("rus", "дфио ящепойр ще йщяейи345йр ще йщяей","ио я ящ хйр ще йщrrwrяей асдйий дй ияийий йидйямям ясК  йкйкйд йп йдпйд йпя")
                                             .Map,

                  Terms = new NLSMap.Builder().Add("eng", "d juij345ij hfre", "  klj ldj345w ;lej ;lwje d;ljw[qdj wqegrwqer h[wqoeir wqhj hwqoei hrwqoih ")
                                             .Add("rus", "дфио яще345поийр ще йщяей","ио я ящ х а345сдйиwй дй ияийий йиwreдйямям ясК  йкйкйд йп йдпйд йпя")
                                             .Map,

                  Privacy = new NLSMap.Builder().Add("eng", "hh hio ", "j j wlefj lwj f345jw ljf  3245345")
                                             .Add("rus", "дфи ящщяей","иомям ясК  йкйк4йд йп йдпйд й345435пя")
                                             .Add("zzz", "дфи ящщяей","ио345мям ясК  йкйкйд йп йдпйд йп435я")
                                             .Add("zza", "дфи ящщяей","иомям345 ясК  йкйк5йд йп йдп2345йд й435пя")
                                             .Add("zzb", "дфи5 ящщяей","иомя45м ясК  йкйкйд23 й345п йдпйд 324й435пя")
                                             .Add("zzc", "дфи3 ящщяей","иомям ясКt45 435 йкйкйд йп343253545 йдпйд345 йпя")
                                             .Add("zzd", "дфи ertящщяей","иомя345м ясК  йкйкйд й345п 3245йдпйд йпя")
                                             .Add("zze", "дфиet ящщяей","иомям ясК 435 йкйкйд йп533454 йд345пйд йпя")
                                             .Map,
              },

              new SKURow
              {
                GDID = new GDID(23, 8, 2133443), SKU = "KB723487-2323RBI-27469XZ0-02", Image = "/dha234224234rwiegfr.jpg", StockCount = 128,
                Options = new string[]{"aaa234aaaaa", "la3rge", "gr3een", "wetrrwer", "werwer"},
                  Name = new NLSMap.Builder().Add("eng", "dh54fswerfh sk wf i", "wuh we fwuih fiuqwhfihopqihuwihuehwqdiuhwq iduhiufhiushfhsdufh")
                                       .Add("rus", "дфио я345щепоийр ще йwerщяей","ио я ящ хewr асдйий дй ияийий йидйямям ясК  йкйкйд йп йдпйд йпя")
                                       .Add("zzz", "wqe  i wqo34wer5iropi w","j wqeljr rjwlqjer werwegfhspudif qfhuhf hsafuih fqhfjsahf hsf qwhj fh")
                                       .Map,

                  About = new NLSMap.Builder().Add("eng", "dherfre5", "weqr we w53er wrwuh we fwuih fiuwerqwhfihopqihuwihuehwqdiuhwq iduhiufhiushfhsdufh")
                                             .Add("rus", "дфиоwer ящепойр ще йщяейи345йр ще йщяей","ио я ящ хйр ще werйщrrwrяей асдйий дй ияийий йидйямям ясК  йкйкйд йп йдпйд йпя")
                                             .Map,

                  Terms = new NLSMap.Builder().Add("eng", "d juij345ij werhfre", "  klj ldj345w ;lej wer;lwje d;ljw[qdj wqegrwqer h[wqoeir wqhj hwqoei hrwqoih ")
                                             .Add("rus", "дфио яще345пwerоийр ще йщяей","ио я ящ х wrа345сдйиwй дй ияийий йиwreдйямям ясК  йкйкйд йп йдпйд йпя")
                                             .Map,

                  Privacy = new NLSMap.Builder().Add("eng", "hh hio ", "j j wlefj lwj fwer345jw ljf  3245345")
                                             .Add("rus", "дwerфи ящщяеewrй","иомям ясК  йкйк4йд йп йдпйд й345435пя")
                                             .Add("zzz", "дфwerи ящщяей","ио345мям ясК  werйкйкйд йп йдпйд йп435я")
                                             .Add("zza", "дфи ящewrщяей","иомям345 ясК  йwerкйк5йд йwerwп йдп2345йд й435пя")
                                             .Add("zzb", "дфи5 werящщяей","иомя45м ясК  йrwerкйкйд23 й345п йдпйд 324й435пя")
                                             .Add("zzc", "дфwerи3 яwerщщяей","иомям ясКt45erwe 435 йкйкйд йп343253545 йдпйд345 йпя")
                                             .Add("zzd", "дфwerи erwertящщяей","иомя345м ясК  йкйкйд й345п 3245йдпйд йпя")
                                             .Add("zze", "дфиet яwerщщяей","иомям ясК 435 йкйкйд йп533454 йд345пйд йпя")
                                             .Map,
              },


            },
            Prices = new PriceRow[]
            {
              new PriceRow
              {
                GDID = new GDID(23, 3, 2133443),
                Qty = 13, CompareAt = null,
                 HasDiscount = true, IsResellable = false, Price = 37673.23m,
                 Options = new string[]{"","","","","",""}
              },
              new PriceRow
              {
                GDID = new GDID(23, 3, 23),
                Qty = 143, CompareAt = null,
                 HasDiscount = true, IsResellable = false, Price = 376.23m,
                 Options = new string[]{"","","","","","dfg"}
              },
              new PriceRow
              {
                GDID = new GDID(23, 3, 77443),
                Qty = 133, CompareAt = null,
                 HasDiscount = true, IsResellable = false, Price = 673.23m,
                 Options = new string[]{"dsfg","dfg","dfg","dfg","dfg","dfg"}
              },
              new PriceRow
              {
                GDID = new GDID(23, 3, 21743),
                Qty = 133, CompareAt = 234.56m,
                 HasDiscount = true, IsResellable = false, Price = 3473.23m,
                 Options = new string[]{"","","dfg","","",""}
              },
              new PriceRow
              {
                GDID = new GDID(23, 3, 1443),
                Qty = 4313, CompareAt = null,
                 HasDiscount = true, IsResellable = false, Price = 3673.23m,
                 Options = new string[]{"","234","23","dfg","",""}
              },
              new PriceRow
              {
                GDID = new GDID(23, 3, 23473),
                Qty = 213, CompareAt = null,
                 HasDiscount = true, IsResellable = false, Price = 7673.23m,
                 Options = new string[]{"dfg","dfg","dfg","dfg","dfg","dfg"}
              }
            }
          };

          return result;
        }


      }


  }
}
