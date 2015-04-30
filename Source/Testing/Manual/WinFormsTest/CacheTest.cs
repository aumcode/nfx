/*<FILE_LICENSE>
* NFX (.NET Framework Extension) Unistack Library
* Copyright 2003-2014 IT Adapter Inc / 2015 Aum Code LLC
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
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using System.Diagnostics;
using System.Threading;
using System.Threading.Tasks;

using NFX;
using NFX.DataAccess.Cache;

namespace WinFormsTest
{
    public partial class CacheTest : Form
    {
        public CacheTest()
        {
            InitializeComponent();
        }

        const string TBL = "tbl1";
        const string TBL2 = "tbl2";
        const string TBL3 = "tbl3";
        const string TBL4 = "tbl4";
        long id;

           class SomeHuman
           {
                public SomeHuman(int i)
                {
                    FirstName = "James_{0}".Args(i);
                    LastName = "Watson_{0}".Args(i);
                    MiddleName = "R.";

                    Address1 = "{0} Proletarskiy Sad".Args(i);
                    Address2 = (i%3==0) ? "Kv. {0}".Args(i) : string.Empty;
                    City= (i%3==0) ? "Kiev" : "Cleveland";
                    State= (i%3==0) ? "Ukraine" : "Ohio";
                    Zip = (i%7==0) ? "1234" : "54321";

                    Phone = "({0}) 555-({0})".Args(i%123);
                    EMail = "spider{0}@yagoo.kom".Args(i%12123);

                    DOB = DateTime.Now.AddDays(-(i%10000));
                    YearsInService = i;
                    Salary = 100.25M*i;
                    IsInsured = (i%7==0);
                    VotingThreshold = i * 0.12301d;
                }

                public string FirstName;
                public string LastName;
                public string MiddleName;

                public string Address1;
                public string Address2;
                public string City;
                public string State;
                public string Zip;

                public string Phone;
                public string EMail;

                public DateTime DOB;
                public int  YearsInService;
                public decimal  Salary;
                public bool  IsInsured;
                public double VotingThreshold;
           }


        CacheStore store;

        private void btnPopulate_Click(object sender, EventArgs e)
        {
            DateTime? exp = null;
            if (tbExpiration.Text!="")
            {
              exp = tbExpiration.Text.AsDateTime().ToUniversalTime();
            }
            
            
            var cnt = tbCount.Text.AsInt(100);
            var watch = Stopwatch.StartNew();
            var rnd = new Random();
            var tbl = store[TBL];
            for(var i=0; i<cnt; i++,id++)
            {
                var rid = rnd.Next();
                //tbl.Put((ulong)id, new SomeHuman((int)id));
                tbl.Put((ulong)rid, new SomeHuman(rid), absoluteExpirationUTC: exp);
            }

                

            //Parallel.For(0, cnt, (i) => 
            //                     {
            //                       var v = new SomeHuman((int)id);
            //                       store.Put(TBL,  (ulong)id, v);  
            //                       store.Put(TBL2, (ulong)id, v);
            //                       store.Put(TBL3, (ulong)id, v);
            //                       store.Put(TBL4, (ulong)id, v);
            //                       Interlocked.Increment(ref id); 
            //                     });

            
            btnPopulate.Text = "Populate "+id.ToString();

            Text = "Put {0:n2} objects in {1:n2} ms. Store count {2:n2}".Args(cnt, watch.ElapsedMilliseconds, store.Count);
        }


        private void btnFind_Click(object sender, EventArgs e)
        {
          
           CacheRec rec = null;
           
           var rnd = new Random();
           var ridx = new ulong[0xffff];
           for(var i=0; i< ridx.Length; i++)
            ridx[i] = (ulong)rnd.Next(Int32.MaxValue);


           const int CNT = 40000000;
           ulong id = (ulong)tbID.Text.AsLong();
          
           var watch = Stopwatch.StartNew();
           //for(var i=0; i<CNT; i++)
           //Parallel.For(0, CNT,(i)=> rec = store.Get(TBL, (ulong)(i%3000)+id));
           var tbl = store[TBL];
           Parallel.For(0, CNT,(i)=> rec = tbl.Get(ridx[i%0xffff]+(ulong)i));
           
           rec = tbl.Get(id);

           var elapsed = watch.ElapsedMilliseconds;
           Text = "Found {0} {1:n2} times in {2:n2} ms. Store count {3:n2} {4:n2} {5:N1}op/sec".Args(rec!=null,
                                                                            CNT,
                                                                            elapsed,
                                                                            store.Count,
                                                                            rec!=null?((SomeHuman)rec.Value).LastName:"",
                                                                            (int)(CNT / (elapsed/1000d)) 
                                                                            );
        }


          

           private void CacheTest_FormClosed(object sender, FormClosedEventArgs e)
           {
               store.Dispose();
           }

           private void CacheTest_Load(object sender, EventArgs e)
           {
               store = new CacheStore("Test Ztore1");
               store.InstrumentationEnabled = true;
               store.TableOptions.Register( new TableOptions(TBL, TableCapacity.Default));//.DBServer_128GB) );
               //store.TableOptions.Register( new TableOptions(TBL2, TableCapacity.M100,1024) );
               //store.TableOptions.Register( new TableOptions(TBL3, TableCapacity.M100,1024) );
               //store.TableOptions.Register( new TableOptions(TBL4, TableCapacity.M100,1024) );
           }

           private void btnGC_Click(object sender, EventArgs e)
           {
               var w = Stopwatch.StartNew();
               var b = GC.GetTotalMemory(false);
               GC.Collect();
               Text = "Freed: {0:n2} bytes in {1:n2} ms".Args(b - GC.GetTotalMemory(false), w.ElapsedMilliseconds);
           }

           private void tmr_Tick(object sender, EventArgs e)
           {
               if (!cbTimer.Checked) return;
               btnPopulate_Click(null, null);
           }

           private void button1_Click(object sender, EventArgs e)
           {
              var cnt = tbCount.Text.AsInt(100);
              var watch = Stopwatch.StartNew();
              var rnd = new Random();
              var cache = System.Runtime.Caching.MemoryCache.Default;
              
                for(var i=0; i<cnt; i++,id++)
                {
                    var rid = rnd.Next();
                    //tbl.Put((ulong)rid, new SomeHuman(rid));
                    cache.Add(rid.ToString(), new SomeHuman(rid), DateTimeOffset.MaxValue); 
                }

                

            //Parallel.For(0, cnt, (i) => 
            //                     {
            //                       var v = new SomeHuman((int)id);
            //                       store.Put(TBL,  (ulong)id, v);  
            //                       store.Put(TBL2, (ulong)id, v);
            //                       store.Put(TBL3, (ulong)id, v);
            //                       store.Put(TBL4, (ulong)id, v);
            //                       Interlocked.Increment(ref id); 
            //                     });

            
            button1.Text = "MS Populate "+id.ToString();

            Text = "Microsoft Put {0:n2} objects in {1:n2} ms. Store count {2:n2}".Args(cnt, watch.ElapsedMilliseconds, cache.GetCount());
           }

           private void button2_Click(object sender, EventArgs e)
           {
              var cache = System.Runtime.Caching.MemoryCache.Default;
           
           var rnd = new Random();
           var ridx = new ulong[0xffff];
           for(var i=0; i< ridx.Length; i++)
            ridx[i] = (ulong)rnd.Next(Int32.MaxValue);
           object rec = null;

           const int CNT = 40000000;
           ulong id = (ulong)tbID.Text.AsLong();
          
           var watch = Stopwatch.StartNew();
           Parallel.For(0, CNT,(i)=> rec = cache.Get((ridx[i%0xffff]+(ulong)i).ToString()) );
           
            rec = cache.Get(id.ToString());

           var elapsed = watch.ElapsedMilliseconds;
           Text = "MSft Found {0} {1:n2} times in {2:n2} ms. Store count {3:n2} {4:n2} {5:N1}op/sec".Args(rec!=null,
                                                                            CNT,
                                                                            elapsed,
                                                                            cache.GetCount(),
                                                                            rec!=null?((SomeHuman)rec).LastName:"",
                                                                            (int)(CNT / (elapsed/1000d)) 
                                                                            );
           }


           private List<byte[]> m_Huge;

           private void btnAllocHuge_Click(object sender, EventArgs e)
           {
             var w = Stopwatch.StartNew();

             if (m_Huge!=null)
             {
               m_Huge = null;
               var b = GC.GetTotalMemory(false);
               GC.Collect();
               Text = "Freed: {0:n2} bytes in {1:n2} ms".Args(b - GC.GetTotalMemory(false), w.ElapsedMilliseconds);
               return;
             }

             const long SZ1 = 3 * 1024;
             const long SZ2 = 16 * 1024 * 1024;

             m_Huge = new List<byte[]>((int)SZ1);
             for(int i=0; i<SZ1; i++)
             {
               var arr = new byte[SZ2];

               for(long j=0; j<arr.LongLength; j+=128)
                arr[j] = (byte)j;

               m_Huge.Add( arr );
             }

             Text = "Done {0:n2} bytes in in {1:n2} ms".Args(SZ1*SZ2, w.ElapsedMilliseconds);

           }


    }
}
