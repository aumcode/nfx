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

using NFX.Serialization.Slim;
using System.Runtime.Serialization.Formatters.Binary;
using System.IO;
using System.Diagnostics;
using System.Runtime.Serialization;
using System.Xml;
using NFX;
using System.Reflection;
using System.Reflection.Emit;
using System.Linq.Expressions;


namespace WinFormsTest
{
    public partial class SerializerForm2 : Form
    {
       const int CNT = 750000;//50000;//100000;//10000;
       
        public SerializerForm2()
        {
            InitializeComponent();
        /*

            {

            data.Name = "Library Of Congress";
            data.Int1 = 132;
            data.Date = DateTime.Now;
            data.Watcher = "The spirit of his magesty Stupid Stool #2";
            data.RequestID = Guid.NewGuid();
       //     data.Bin= new byte[]{1,2,3};
      //      data.Args = new object[]{"aaaa",1,true, 'a'};//, "a",true,1};//,2,3,4,5,6,7,8,9,0,1,2,3,4,5,6,7,8,9,0};
            //data.Args = new object[]{0,0,0,0,0,0,0,0,0,1,2,true,true,null,false,true,null,45.32,DateTime.Now};
            data.Books = new List<Book>() { new Book() { Author = new Perzon(){ FirstName = "Barack", LastName = "Obama"}, Title = "Dreams from My Father: A Story of Race and Inheritance", Year = 1995},

                                       new Book() { Author = new Perzon(){ FirstName = "Lewis", LastName = "Caroll"}, Title = "Alice's Adventures in Wonderland", Year = 1865},

                                       new Book() { Author = new Perzon()
                                                            { FirstName = "Kurt", LastName = "Vonnegut",
                                                              Names1 = new List<string>{"Felix Kutz", "Joseph Yong","Jerry Springer","Oleg Popoff"},
                                                              Names2 = new List<string>{"A", "B","C","D","E","F","G","H","I","J","K","L","M","N","O"} 
                                                            },
                                                             Title = "Welcome to the Monkey House", Year = 1968},

                                       new Book() { Author = new Perzon(){ FirstName = "Orson", MiddleName = "Scott", LastName = "Card"}, Title = "Ender's Game", Year = 1985}};

            data.Books[2].Author.Parent = data.Books[1].Author;
            data.Books[1].Author.Parent = data.Books[2].Author;
            data.Books[3].Author.Parent = data.Books[1].Author;

        ////   data.Books[1].Attributes1 = new object[]{1,2,true,true,null,false,true,null,45.32,DateTime.Now,'a',"abcdef"};
        ////   data.Books[1].Attributes2 = new object[]{0,0,0,0,0,0,0,0,0,1,2,true,true,null,false,true,null,45.32,DateTime.Now};

            for(var i=0; i<10; i++)
            {
              data.Books.Add(
                 new Book
                 {
                    Author = new Perzon{ FirstName = "Jack{0}".Args(i), LastName="Roacher{0}".Args(0), Age4 = i, Age3 = 1000-i, Age1 = i+900, ID1 = Guid.NewGuid(), ID2 = Guid.NewGuid(), ID3=Guid.NewGuid()},
                    Title = "The Dream of the Insatiable Malevolent Beast on the Precipice of the Debaucherous Revolt #{0}".Args(i),
                    Year = 2007+i,
            //    Attributes1 = new object[]{1,2,true,DateTime.Now},
            //    Attributes2 = new object[]{"Yes or no", 1,2,3},
            //       BinData = new byte[10]{0,45,22,23,45,129,0,0,12,250},
             //      ObjData = new byte[2]{10, 245},

             //      S1 = "Kolyan somertokov",
             //      S2 = "Buchan Ravi Shankar Draxma Mudya Zolotorev",
             //      S3 = "Deodik Amundens",
                    B1 = true, B2 = true, B3 = true,
                  //  I1= 3123, I2 = 4234, I3=234232,
                    Dec1 = 4234232.12m, Dec2=-1234324132m, Dec3 = 12312312312321.223m,
                    Dbl1 = 234.233d, Dbl2 = 23423423.0001d, Dbl3 = -12.0000001d,
                    Dt1 = DateTime.Now, Dt2 = DateTime.Now, Dt3 = DateTime.Now
                 }
              );

            }

           }     */
        }


        //long[] data = 
        //new long[]
        //{
        //  1,2,3,4,5,6,7,8,9,0,1,2,3,4,5,6,7,8,9,0,1,2,3,4,5,6,7,8,9,0,
        //  1,2,3,4,5,6,7,8,9,0,1,2,3,4,5,6,7,8,9,0,1,2,3,4,5,6,7,8,9,0,
        //  1,2,3,4,5,6,7,8,9,0,1,2,3,4,5,6,7,8,9,0,1,2,3,4,5,6,7,8,9,0,
        //  1,2,3,4,5,6,7,8,9,0,1,2,3,4,5,6,7,8,9,0,1,2,3,4,5,6,7,8,9,0,
        //  1,2,3,4,5,6,7,8,9,0,1,2,3,4,5,6,7,8,9,0,1,2,3,4,5,6,7,8,9,0
        //};
        
      // Library data = new Library();
       //Perzon data = new Perzon
       //{
       //   FirstName = "Alex",
       //   LastName = "Perzonov",
       //   Age1 = 10,
       //   Age2=20,
       //   Age3=99,
       //  //  Parent = new Perzon{ LastName="aaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaa", Salary1 = 3122d}
       //};

       TradingRec data = TradingRec.Build();

        private void button1_Click(object sender, EventArgs e)
        {
            GC.Collect(GC.MaxGeneration, GCCollectionMode.Forced);

            var ms = new MemoryStream();

            var slim = new SlimSerializer(new TypeRegistry(new Type[]{typeof(TradingRec), typeof(Library), typeof(Book), typeof(Perzon), typeof(List<Book>), typeof(object[]), typeof(string[])}, TypeRegistry.CommonCollectionTypes));
            slim.TypeMode = TypeRegistryMode.Batch;

            var w = Stopwatch.StartNew();
            for(var i=0; i<CNT; i++)
            {
             ms.Position = 0;
             slim.Serialize(ms, data); 
            }

            w.Stop();

            using (var fs =new FileStream("C:\\NFX\\SerializerForm2.slim", FileMode.Create))
            {
              fs.Write(ms.GetBuffer(), 0, (int)ms.Length);
            }

            Text = string.Format("Slim serialized {0} in {1} ms, {2}/sec {3}bytes",CNT, w.ElapsedMilliseconds, (CNT/(double)w.ElapsedMilliseconds)*1000, ms.Length);

         //   MessageBox.Show( ms.GetBuffer().ToDumpString(DumpFormat.Hex,0, (int)ms.Length));
        }

        private void button2_Click(object sender, EventArgs e)
        {
            var ms = new MemoryStream();

            var bf = new BinaryFormatter();

            var w = Stopwatch.StartNew();
            for(var i=0; i<CNT; i++)
            {
             ms.Position = 0;
             bf.Serialize(ms, data); 
            }

            w.Stop();

            using (var fs =new FileStream("C:\\NFX\\SerializerForm2.bin", FileMode.Create))
            {
              fs.Write(ms.GetBuffer(), 0, (int)ms.Length);
            }
            Text = string.Format("Bin Formatter serialized {0} in {1} ms, {2}/sec {3}bytes",CNT, w.ElapsedMilliseconds, (CNT/(double)w.ElapsedMilliseconds)*1000, ms.Length);
        }

        private void button3_Click(object sender, EventArgs e)
        {
            var ms = new MemoryStream();

            var dcs = new DataContractSerializer(typeof(Library)); //Library

            var w = Stopwatch.StartNew();
            for(var i=0; i<CNT; i++)
            {
             ms.Position = 0;
            
             //XmlDictionaryWriter binaryDictionaryWriter = XmlDictionaryWriter.CreateBinaryWriter(ms);
             //dcs.WriteObject(binaryDictionaryWriter, data);
             //binaryDictionaryWriter.Flush();
            
             dcs.WriteObject(ms, data);
            }

           

            w.Stop();

            using (var fs =new FileStream("C:\\NFX\\SerializerForm2.DATACONTRACT", FileMode.Create))
            {
              fs.Write(ms.GetBuffer(), 0, (int)ms.Length);
            } 

            Text = string.Format("Data Contract Serializer serialized {0} in {1} ms, {2}/sec {3}bytes",CNT, w.ElapsedMilliseconds, (CNT/(double)w.ElapsedMilliseconds)*1000, ms.Length);
        }

        private void button6_Click(object sender, EventArgs e)
        {
           var ms1 = new MemoryStream();
           var ms2 = new MemoryStream();

            var slim1 = new SlimSerializer(new TypeRegistry(new Type[]{typeof(Library), typeof(Book), typeof(Perzon), typeof(List<Book>), typeof(object[]), typeof(string[])}, TypeRegistry.CommonCollectionTypes));
            slim1.TypeMode = TypeRegistryMode.Batch;
            slim1.Serialize(ms1, data);
            slim1.Serialize(ms2, data);
            slim1.ResetCallBatch();

            ms1.Position = 0;
            slim1.Deserialize(ms1);
            var w = Stopwatch.StartNew();
            for(var i=0; i<CNT; i++)
            {
             ms2.Position = 0;
             slim1.Deserialize(ms2); 
            }

            w.Stop();

            Text = string.Format("Slim deserialized {0} in {1} ms, {2}/sec {3}bytes",CNT, w.ElapsedMilliseconds, (CNT/(double)w.ElapsedMilliseconds)*1000, ms2.Length);

        }

        private void button4_Click(object sender, EventArgs e)
        {
           var ms = new MemoryStream();

            var dcs = new DataContractSerializer(typeof(Library));//Library
            
            dcs.WriteObject(ms, data);
            //XmlDictionaryWriter binaryDictionaryWriter = XmlDictionaryWriter.CreateBinaryWriter(ms);
            //dcs.WriteObject(binaryDictionaryWriter, data);
            //binaryDictionaryWriter.Flush();

            var w = Stopwatch.StartNew();

            for(var i=0; i<CNT; i++)
            {
             ms.Position = 0;
            
             dcs.ReadObject(ms);
            }
            w.Stop();
            Text = string.Format("Data Contract Serializer deserialized {0} in {1} ms, {2}/sec {3}bytes",CNT, w.ElapsedMilliseconds, (CNT/(double)w.ElapsedMilliseconds)*1000, ms.Length);
        }

        private void button5_Click(object sender, EventArgs e)
        {
          var ms = new MemoryStream();

            var bf = new BinaryFormatter();

             bf.Serialize(ms, data);

            var w = Stopwatch.StartNew();
            for(var i=0; i<CNT; i++)
            {
             ms.Position = 0;
             bf.Deserialize(ms);
            }

            w.Stop();
            Text = string.Format("Bin Formatter deserialized {0} in {1} ms, {2}/sec {3}bytes",CNT, w.ElapsedMilliseconds, (CNT/(double)w.ElapsedMilliseconds)*1000, ms.Length);
        }



        public bool crap(string data)
        {
          if (data.Length<5) return true;
          if (data.IndexOf('a')>2) return true;
          return false;
        }

        private void button7_Click(object sender, EventArgs e)
        {
            const int CNT = 1000000;
            var mi = GetType().GetMethod("crap");
            var strs = new string[]{"olokalao","bobnroik","malokalokiokolkolo"};

            var w = Stopwatch.StartNew();
            for(var i=0; i<CNT; i++)
             mi.Invoke(this, new object[]{strs[i%3]});
            w.Stop();
            Text = string.Format("mi.Invoke {0} in {1} ms, {2}/sec",CNT, w.ElapsedMilliseconds, (CNT/(double)w.ElapsedMilliseconds)*1000);

            var d = mi.CreateDelegate(typeof(Func<string, bool>), this) as Func<string,bool>;
             w = Stopwatch.StartNew();
            for(var i=0; i<CNT; i++)
               d(strs[i%3]);
            w.Stop();
            Text += string.Format("Delegate {0} in {1} ms, {2}/sec",CNT, w.ElapsedMilliseconds, (CNT/(double)w.ElapsedMilliseconds)*1000);
        }

        private void button8_Click(object sender, EventArgs e)
        {
            const int CNT = 1000000;
            var w = Stopwatch.StartNew();
            for(var i=0; i<CNT; i++)
             doWithFlag(i);
            w.Stop();
            Text = string.Format("With Flag {0} in {1} ms, {2}/sec",CNT, w.ElapsedMilliseconds, (CNT/(double)w.ElapsedMilliseconds)*1000);

            w = Stopwatch.StartNew();
            for(var i=0; i<CNT; i++)
               doWithException(i);
            w.Stop();
            Text += string.Format("DoExeption {0} in {1} ms, {2}/sec",CNT, w.ElapsedMilliseconds, (CNT/(double)w.ElapsedMilliseconds)*1000);
        }

        public int dummy1, dummy2, dummy3;

        private bool doWithFlag(int i)
        {
          var r = doWithFlag_1(i);
          if (!r) return false;
          return true;
        }

          private bool doWithFlag_1(int i)
          {
            if (i%10==0) return false;
            dummy1++;
            return doWithFlag_2(i);
          }

          private bool doWithFlag_2(int i)
          {
            if (i%3==0) return false;
            dummy2++;
            return doWithFlag_3(i);
          }

          private bool doWithFlag_3(int i)
          {
            if (i%2==0) return false;
            dummy3++;
            return true;
          }
        

        private bool doWithException(int i)
        {
          try{ doWithException_1(i); return true; }
          catch{ return false;}
        }
         
          private bool doWithException_1(int i)
          {
            if (i%10==0) throw new Exception();
            dummy1++;
            return doWithException_2(i);
          }

          private bool doWithException_2(int i)
          {
            if (i%3==0) throw new Exception();
            dummy2++;
            return doWithFlag_3(i);
          }

          private bool doWithException_3(int i)
          {
            if (i%2==0) throw new Exception();
            dummy3++;
            return true;
          }

          private Expression<Func<object, int, int>> expression1; 
          private Expression<Func<object, int, int>> expression2; 
          private Func<object, int, int> test1;
          private Func<object, int, int> test2;

          private void prepare()
          {
             var p1 = Expression.Parameter(typeof(object));
             var p2 = Expression.Parameter(typeof(int));

             expression1 = Expression.Lambda<Func<object, int, int>>(
                 Expression.Multiply(p2, Expression.Constant(5)), p1, p2
             );
             expression2 = (o, x) => x * 5;
             test1 = expression1.Compile();
             test2 = expression2.Compile(); 
          }


          private void brnDynAss_Click(object sender, EventArgs e)
          {
            prepare();
             
             
              var asmBuilder = AppDomain.CurrentDomain.DefineDynamicAssembly(
                    new AssemblyName("ZmeyaTeztx"), AssemblyBuilderAccess.Run);
              var modBuilder = asmBuilder.DefineDynamicModule("myModule");
              var tpBuilder = modBuilder.DefineType("myType_1", TypeAttributes.Public);
              var methodBuilder = tpBuilder.DefineMethod(
                           "myMethod_1", MethodAttributes.Public | MethodAttributes.Static);
              expression1.CompileToMethod(methodBuilder);
              var tp = tpBuilder.CreateType();                            
              var asmF1 = (Func<int, int>)Delegate.CreateDelegate( typeof(Func<int, int>), this, tp.GetMethod("myMethod_1"));

              Text = asmF1(5).ToString();

              tpBuilder = modBuilder.DefineType("myType_2", TypeAttributes.Public);
              methodBuilder = tpBuilder.DefineMethod(
                           "myMethod_2", MethodAttributes.Public | MethodAttributes.Static);
              expression2.CompileToMethod(methodBuilder);
              tp = tpBuilder.CreateType();
              var asmF2 = (Func<int, int>)Delegate.CreateDelegate( typeof(Func<int, int>), this, tp.GetMethod("myMethod_2"));

              Text += "    "+asmF2(5).ToString(); 

              test1(this, 2);
              test2(this, 3);

              const int CNT = 500000000;
              for(var i=0; i<500000000; i++);

              var w = Stopwatch.StartNew();
             
             
              w.Restart();             
              for(var i=0; i<CNT; i++)  asmF1(3);
              var ts1 = w.ElapsedMilliseconds;

              w.Restart();             
              for(var i=0; i<CNT; i++)  asmF2(3);
              var ts2 = w.ElapsedMilliseconds;

              w.Restart();             
              for(var i=0; i<CNT; i++)  test1(this, 3);
              var ts3 = w.ElapsedMilliseconds;

              w.Restart();             
              for(var i=0; i<CNT; i++)  test2(this, 3);
              var ts4 = w.ElapsedMilliseconds;


              Text = "T1: {0:n2}/sec  T2: {1:n2}/sec  T3: {2:n2}/sec  T4: {3:n2}/sec".Args
                      (
                         CNT / (ts1 / 1000d),
                         CNT / (ts2 / 1000d),
                         CNT / (ts3 / 1000d),
                         CNT / (ts4 / 1000d)
                      );
          }

          private void btnDates_Click(object sender, EventArgs e)
          {
            const int CNT = 1000000;
            var w = Stopwatch.StartNew();
            for(var i=0; i<CNT;i++)
            {
             var n = DateTime.Now;
            }

            var ts1 = w.ElapsedMilliseconds;

            w.Restart();

            for(var i=0; i<CNT;i++)
            {
             var n = DateTime.UtcNow;
            }

            var ts2 = w.ElapsedMilliseconds;

            Text = Text = "T1: {0:n2}/sec  T2: {1:n2}/sec".Args
                      (
                         CNT / (ts1 / 1000d),
                         CNT / (ts2 / 1000d)
                      );

          }

          private void btnObjRef_Click(object sender, EventArgs e)
          {
            const int CNT = 8;
            const int SEARCHES = 8000000;

            


            var rnds = new int[800];
            for(var i=0; i<rnds.Length; i++)
              rnds[i] = ExternalRandomGenerator.Instance.NextScaledRandomInteger(CNT / 2, CNT-1);

            var lst = new Perzon[CNT];
            var dict = new Dictionary<object, int>(128, ReferenceEqualityComparer<object>.Instance);
            for(var i=0; i<lst.Length; i++)
            {
              var person = new Perzon();
              lst[i] = (person);
              dict.Add(person, i);
            }

            var found1 = 0;
            var found2 = 0;

            System.Threading.Thread.SpinWait(250333000);

            var sw = Stopwatch.StartNew();
            for(var j=0; j<SEARCHES; j++)
            {
             var key = this;//WORST CASe, key is never found //lst[rnds[j%rnds.Length]];
             for(var i=0; i<lst.Length; i++)
              if ( object.ReferenceEquals(key, lst[i]))
              {
                found1++;
                break;//FOUND!!!
              }
            }  

            var time1 = sw.ElapsedMilliseconds;

            sw.Restart();
           
           for(var j=0; j<SEARCHES; j++)
            {
             var key = this;//WORST CASe, key is never found //lst[rnds[j%rnds.Length]];
             int idx;
             if (dict.TryGetValue(key, out idx)) found2++; //FOUND!
            } 

            var time2 = sw.ElapsedMilliseconds;

            var summary=
@"
   Elements: {0}  Searched: {1}
   -------------------------------------
   Linear search found {2} in {3} ms at {4:n2} ops/sec
   Dict search found   {5} in {6} ms {7:n2} ops/sec


".Args(
 CNT, SEARCHES,
 found1, time1, SEARCHES / (time1 / 1000d),
 found2, time2, SEARCHES / (time2 / 1000d)
);
     MessageBox.Show( summary );


          }

          private void btnABsSpeed_Click(object sender, EventArgs e)
          {
             System.Threading.Thread.SpinWait(100000000);
             const int CNT = 100000000;
             var sw = Stopwatch.StartNew();
             
             long sum = 0;
             for(var i=0; i<CNT; i++)
               sum += (Math.Abs(i) % 3);

             var t1 = sw.ElapsedMilliseconds;
             sw.Restart();

             long sum2 = 0;
             for(var i=0; i<CNT; i++)
               sum2 += ((i & 0x7fffffff) % 3);

             var t2 = sw.ElapsedMilliseconds;
             sw.Restart();

             long sum3 = 0;
             for(var i=0; i<CNT; i++)
               sum3 += ((i < 0? -i : i) % 3);
             var t3 = sw.ElapsedMilliseconds;

             var msg =
@"
Did {0}:
--------------------------------------
Math.Abs() {1:n0} ms at @ {2:n0} ops/sec
Bit &&     {3:n0} ms at @ {4:n0} ops/sec
IF         {5:n0} ms at @ {6:n0} ops/sec".Args(CNT, 
                  t1, CNT / (t1 / 1000d),
                  t2, CNT / (t2 / 1000d),
                  t3, CNT / (t3 / 1000d)  );

            MessageBox.Show( msg );

          }




    }

    [DataContract(IsReference=true)]
    [Serializable]
    public class Library
    {
      [DataMember]public string Name;
      [DataMember]public int Int1;
      [DataMember]public DateTime Date;
      [DataMember]public string Watcher;
      [DataMember]public Guid RequestID;
      [DataMember]public object[] Args;
      [DataMember]public byte[] Bin;

      [DataMember]public List<Book> Books;
    }
    
    [DataContract(IsReference=true)]
    [Serializable]
    public class Book
    {
      public Book()
      {
        Year = -1000;
      }
      [DataMember]public Perzon Author;
      [DataMember]public string Title;
      [DataMember]public int Year;

      [DataMember]public object[] Attributes1;
      [DataMember]public object[] Attributes2;

      [DataMember]public byte[] BinData;
      [DataMember]public object ObjData;


      [DataMember]public int I1;
      [DataMember]public int I2;
      [DataMember]public int I3;

      [DataMember]public bool B1;
      [DataMember]public bool B2;
      [DataMember]public bool B3;

      [DataMember]public string S1;
      [DataMember]public string S2;
      [DataMember]public string S3;

      [DataMember]public decimal Dec1;
      [DataMember]public decimal Dec2;
      [DataMember]public decimal Dec3;

      [DataMember]public double Dbl1;
      [DataMember]public double Dbl2;
      [DataMember]public double Dbl3;

      [DataMember]public DateTime Dt1;
      [DataMember]public DateTime Dt2;
      [DataMember]public DateTime Dt3;

    }
    
    [DataContract(IsReference=true)]
    [Serializable]
    public class Perzon
    {
      [DataMember]public string FirstName;
      [DataMember]public string MiddleName;
      [DataMember]public string LastName;

      [DataMember]public Perzon Parent;
 
      [DataMember]public int Age1;
      [DataMember]public int Age2;
      [DataMember]public int? Age3;
      [DataMember]public int? Age4;
 
      [DataMember]public double Salary1;
      [DataMember]public double? Salary2;

      [DataMember]public Guid ID1;
      [DataMember]public Guid? ID2;
      [DataMember]public Guid? ID3;

  //    [DataMember]byte[] Buffer = new byte[1024];
  
      [DataMember]public List<string> Names1; 
      [DataMember]public List<string> Names2;        
      
      [DataMember]public int O1;// = 1;
      [DataMember]public bool O2;// = true;
      [DataMember]public DateTime O3;// = DateTime.UtcNow;
      [DataMember]public TimeSpan O4;// = TimeSpan.FromHours(12);
      [DataMember]public decimal O5;// = 123.23M;
  
    }


  
    [DataContract]
    [Serializable] public class TradingRec
    {
        [DataMember] public string Symbol;
        [DataMember] public int Volume;
        [DataMember] public long Bet;
        [DataMember] public long Price;

         public static TradingRec Build()
         {
           return new TradingRec
           {
            Symbol = NFX.Parsing.NaturalTextGenerator.GenerateFirstName(),
            Volume = ExternalRandomGenerator.Instance.NextScaledRandomInteger(-25000, 25000),
            Bet = ExternalRandomGenerator.Instance.NextScaledRandomInteger(-250000, 250000) * 10000L,
            Price = ExternalRandomGenerator.Instance.NextScaledRandomInteger(0, 1000000) * 10000L
           };
         }
    } 






   [Serializable, DataContract]
public partial class Game
{
    [DataMember]
    public bool Finished { get; set; }
    [DataMember]
    public Guid GameGUID { get; set; }
    [DataMember]
    public long GameID { get; set; }
    [DataMember]
    public bool GameSetup { get; set; }
    [DataMember]
    public Nullable<int> MaximumCardsInDeck { get; set; }
    [DataMember]
    public Player Player { get; set; }
    [DataMember]
    public Player Player1 { get; set; }
    [DataMember]
    public bool Player1Connected { get; set; }
    [DataMember]
    public bool Player1EnvironmentSetup { get; set; }
    [DataMember]
    public long Player1ID { get; set; }
    [DataMember]
    public int Player1Won { get; set; }
    [DataMember]
    public bool Player2Connected { get; set; }
    [DataMember]
    public bool Player2EnvironmentSetup { get; set; }
    [DataMember]
    public long Player2ID { get; set; }
    [DataMember]
    public int Player2Won { get; set; }
    [DataMember]
    public int Round { get; set; }
    [DataMember]
    public Nullable<int> RoundsToWin { get; set; }
    [DataMember]
    public bool Started { get; set; }
    [DataMember]
    public string StateXML { get; set; }
    [DataMember]
    public Nullable<DateTime> TimeEnded { get; set; }
    [DataMember]
    public Nullable<int> TimeLimitPerTurn { get; set; }
    [DataMember]
    public byte[] TimeStamp { get; set; }
    [DataMember]
    public Nullable<DateTime> TimeStarted { get; set; }
}
[Serializable, DataContract]
public class Player
{
    [DataMember]
    public string Name { get; set; }
}


}
