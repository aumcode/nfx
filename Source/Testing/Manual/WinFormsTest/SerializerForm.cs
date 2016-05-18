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
using System.IO;
using System.Runtime.Serialization.Formatters.Binary;



using NFX.WinForms;
using NFX.Serialization.Slim;

using BusinessLogic;
using System.Diagnostics;
using NFX;
using NFX.DataAccess;
using NFX.DataAccess.Distributed;
using NFX.DataAccess.CRUD;


namespace WinFormsTest
{
    public partial class SerializerForm : Form
    {
        public SerializerForm()
        {
            InitializeComponent();
        }

        private void button1_Click(object sender, EventArgs e)
        {  
           var Frank = new Person
                          {
                            Name = "Frank Frankfurter",
                            Age = 99,
                            IsHappy = true,
                            MarriageDate = App.LocalizedTime,

                            Father = new Person { Name = "Zaxar Mai", Age = 44},

                            Type = typeof(System.Reflection.Assembly)
                          };

           var Marry = new Person
                          {
                            Name = "Marry Morra",
                            Age = 44,
                            IsHappy = false,
                            MarriageDate = App.LocalizedTime,

                            Father = Frank.Father,

                            Type = typeof(System.Diagnostics.Stopwatch)
                            //Buffer = new byte[] {10,13,65,65,65,65,65,65,65,65,66,66,66,66,66,66,66,66},
                            //Chars = new char[] {'i', ' ', 'a', 'm', ' ','!'}
                          };


           var Dodik = new Person
                          {
                            Name = "Dodik Kutzhenbukher",
                            Age = 12,
                            IsHappy = true,
                            MarriageDate = App.LocalizedTime.AddDays(-100),
                            Father = Frank,
                            Mother = Marry
                          };

          Marry.Mother = Dodik;//Cycle




        //Frank.Father.Items["Absukov"] = 123;
        //Frank.Father.Items["Bosurxevich"] = true;
        //Frank.Father.Items["Corshunovich"] = "ya est tot kto mojet byt";
        //Frank.Father.Items["Zmejukka"] = " do svazi!";
    

        //Dodik.IntArray = new int[10,10,10];

        //Dodik.IntArray[5,3,4] = 67;
        //Dodik.IntArray[9,9,9] = 65;


        //Dodik.ObjArray = new object[10];
        //for(int i=0; i<Dodik.ObjArray.Length; i++)
        // Dodik.ObjArray[i] = new Person{ Name = "Chelovek-"+(i%4).ToString(), Age = 1, Father = Dodik, IsHappy = true, MarriageDate = App.TimeSource.UTCNow};

        //for(int i=0; i<10; i++)
        // Dodik.Relatives.Add( new Person{ Name = "Solovei-"+(i%4).ToString(), Age = 1, Father = Dodik, IsHappy = true, MarriageDate = App.TimeSource.UTCNow} );

        //for(int i=0; i<1000; i++)
        // Dodik.Numbers.Add( 10000-i );
        

        var tr = new TypeRegistry(TypeRegistry.CommonCollectionTypes,
                                  TypeRegistry.BoxedCommonTypes, 
                                  TypeRegistry.BoxedCommonNullableTypes);

        tr.Add(typeof(Person));
        tr.Add(typeof(Person2));
        tr.Add(typeof(Person[]));
        tr.Add(typeof(System.Collections.Generic.List<Person>));
        tr.Add(typeof(System.Drawing.Point));
        tr.Add(typeof(int[]));
        tr.Add(typeof(int[,,]));
                


    var data = make();

         var clk = Stopwatch.StartNew();
        // for(var i=1; i<1000; i++)
           using (var fs = new FileStream(@"c:\NFX\SLIM.slim", FileMode.Create))
              {
                var s = new SlimSerializer(tr);
                
                for(var i=1; i<1000; i++)
                {
                  s.Serialize(fs,  data);//Dodik);
                }
              }
         Text = clk.ElapsedMilliseconds.ToString();

         clk.Restart();
           using (var fs = new FileStream(@"c:\NFX\FORMATTER.bin", FileMode.Create))
              {
                var bf = new BinaryFormatter();
                
                for(var i=1; i<1000; i++)
                {
                 bf.Serialize(fs, data);//Dodik);
                }
              }
         Text += "        Binary formatter: " + clk.ElapsedMilliseconds.ToString();


        }

        private void button2_Click(object sender, EventArgs e)
        {
             var tr = new TypeRegistry(
                                  TypeRegistry.CommonCollectionTypes,
                                  TypeRegistry.BoxedCommonTypes, 
                                  TypeRegistry.BoxedCommonNullableTypes);
             
             tr.Add(typeof(Person2));
             tr.Add(typeof(System.Drawing.Point));
             tr.Add(typeof(TimeSpan));
             tr.Add(typeof(Kozel));
             
              var p1 = make();



              using (var ms = new FileStream(@"c:\NFX\PERSON2.slim", FileMode.Create) )//new MemoryStream())
              {
                var s = new SlimSerializer(tr);
                             
                s.Serialize(ms, p1);

                var clk = Stopwatch.StartNew();
               
                for(var i=1; i<4000; i++)
                {
                  ms.Seek(0, SeekOrigin.Begin);
                  var p2 = s.Deserialize(ms);
                }
                Text = clk.ElapsedMilliseconds.ToString();

                //Text = p2.Name;
              }

              //BINARY formatterr
             
              using (var ms = new FileStream(@"c:\NFX\PERSON2.bin", FileMode.Create) ) //new MemoryStream())
              {
                var s = new BinaryFormatter();
                
                s.Serialize(ms, p1);

                var clk = Stopwatch.StartNew();
               
                for(var i=1; i<4000; i++)
                {
                  ms.Seek(0, SeekOrigin.Begin);
                  var p2 = s.Deserialize(ms);
                }
                Text += "        Binary formatter: " + clk.ElapsedMilliseconds.ToString();
              }
        }


        int cnt;
        private Person2 make()
        {
              if (cnt>2) return null;
              cnt++;
              var p1 = new Person2{ Name = "Kumir Da", Age = 22 , PointA = new Point(25,36)};

              //p1.IntArray = new int[3];
              //p1.IntArray[0] = 11;
              //p1.IntArray[1] = 22;
              //p1.IntArray[2] = 33;

              
             // p1.StringArray = new string[] {"aaa", "bovin", "zomama"};

              //p1.StringArray2D = new string[2,3];
              //    p1.StringArray2D[1,1] = "Kolya";
              //    p1.StringArray2D[0,2] = "Sima";

              //p1.IntArray3D = new int[2,2,4];
              //    p1.IntArray3D[0,0,0] = 180;
              //    p1.IntArray3D[1,1,3] = -11;

              p1.ObjArray = new object[150];
              p1.ObjArray[12] = 12;
              p1.ObjArray[13] = true;
              p1.ObjArray[14] = App.LocalizedTime;



              p1.IntList.Add(12);
              p1.IntList.Add(19);
              p1.IntList.Add(-1890);
              p1.IntList.Add(0);
              p1.IntList.Add(991);


              p1.Data1 = "ya loshad!";
              p1.Data2 = TimeSpan.FromHours(12);

              p1.Kozel1.Certified = true;
              p1.Kozel1.KozelGrade = 789;
              p1.Kozel1.Owner = new Person2{ Name = "Kozlopas", Age = 48 };

              p1.Kozel2 = new Kozel{ Certified = false, KozelGrade = 2, Owner = new Person2{Name="Soloviev", Age = 98}};


              p1.Father = new Person2{ Name = "Dodik Butcer", Age = 120 };
              p1.Mother = new Person2{ Name = "Feiga Svinarenko", Age = 95, IsHappy = true, MarriageDate = App.LocalizedTime };

              p1.Father.Mother = new Person2{Name = "Zlata Butcer", Age = 230, Father = p1};
              p1.Sibling1 = new Person2{Name = "Cathy", Age = 17, IsHappy = true, MarriageDate = App.LocalizedTime, Data1 = 12};
              p1.Sibling2 = new Person2{Name = "Jimmy", Age = 34, IsHappy = true, MarriageDate = App.LocalizedTime, Data1 = 212};
              p1.Sibling3 = new Person2{Name = "Bob", Age = 7,    IsHappy = true, MarriageDate = App.LocalizedTime, Data1 = 32};
              p1.Sibling4 = new Person2{Name = "Karl", Age = 21,  IsHappy = true, MarriageDate = App.LocalizedTime, Data1 = -432};
              p1.Sibling5 = new Person2{Name = "Oleg", Age = 27,  IsHappy = true, MarriageDate = App.LocalizedTime, Data1 = 552};


              p1.Sibling1.Data1 = make();
              p1.Sibling1.Data2 = p1;
              p1.Sibling1.Father = new Person2{Name="David"};
              p1.Sibling1.Father.Data1 = 123;
              p1.Sibling1.Father.Data2 = TimeSpan.FromDays(12.2);
              p1.Sibling1.Father.Data1 = "aaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaa!";
              p1.Sibling1.Father.Data2 = "bbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbb";

              cnt--;
              return p1;
        }

        private void button3_Click(object sender, EventArgs e)
        {
          const int CNT = 1000;

          var lst = new TwitterMsg[CNT];
          for(var i=0; i<CNT; i++)
           lst[i] = ( new TwitterMsg
           {
             ID = new GDID(0, (ulong)i),
             AuthorID = new GDID(0, (ulong)i*251),
             Banned = i%45==0,
             Rating = i%5,
             ResponseToMsg = new GDID(0, (ulong)i),
             MsgText = NFX.Parsing.NaturalTextGenerator.Generate(0),    
             When = DateTime.Now,
             ri_Deleted = false,
             ri_Host = "Zukini1234",
             ri_Version = DateTime.Now 
           });

           var tr = new TypeRegistry(
                                  TypeRegistry.CommonCollectionTypes,
                                  TypeRegistry.BoxedCommonTypes, 
                                  TypeRegistry.BoxedCommonNullableTypes);
           tr.Add( typeof(GDID));
           tr.Add( typeof(TwitterMsg));


           var clk = Stopwatch.StartNew();
           using (var fs = new FileStream(@"c:\NFX\SLIM.slim", FileMode.Create, FileAccess.Write, FileShare.None, 1024*1024))
           {
                var s = new SlimSerializer(tr);

                s.Serialize(fs,  lst);
           }
           Text = "SLIM took {0} ms ".Args(clk.ElapsedMilliseconds);

          clk.Restart();
           using (var fs = new FileStream(@"c:\NFX\FORMATTER.bin", FileMode.Create, FileAccess.Write, FileShare.None, 1024*1024))
           {
             var bf = new BinaryFormatter();
             bf.Serialize(fs,  lst);
           }
           Text += "        Binary formatter took {0}ms ".Args(clk.ElapsedMilliseconds);

        }


    }

   


    [Serializable]
    public class Person2  : System.Runtime.Serialization.IDeserializationCallback
    {
       public string Name;
       public int Age;
       public DateTime MarriageDate;
       public bool IsHappy;

       public int[] IntArray;
       public string[] StringArray;
       public string[,] StringArray2D;
       public int[,,] IntArray3D;

       public List<int> IntList = new List<int>();

       public object[] ObjArray;

       public Person2 Father;
       public Person2 Mother;

       public Person2 Sibling1;
       public Person2 Sibling2;
       public Person2 Sibling3;
       public Person2 Sibling4;
       public Person2 Sibling5;
       

       public object Data1;
       public object Data2;


       public Type Type;
       
       public Point PointA;
       public Point PointB;
       public Point PointC;
       public TimeSpan SpanA;
       public TimeSpan SpanB;
       public TimeSpan SpanC;

       public double D1;
       public double D2;
       public double D3;

       public double? nD1;
       public double? nD2;
       public double? nD3;

       public decimal Salary1;
       public decimal Salary2;
       public decimal Salary3;

       public Kozel Kozel1;
       public object Kozel2;

       public void OnDeserialization(object sender)
       {
          // MessageBox.Show("Deserialized "+ Name);
       }
    }



    [Serializable]
    public struct Kozel
    {
      public int KozelGrade;
      public bool Certified;
      public Person2 Owner;
    }






     [Serializable]
    public class Person
    {
       public string Name;
       public int Age;
       public DateTime MarriageDate;
       public bool IsHappy;

       public Person Mother;
       public Person Father;
       
       public Type Type;
       
       public Point PointA;
       public Point PointB;
       public Point PointC;
       public TimeSpan SpanA;
       public TimeSpan SpanB;
       public TimeSpan SpanC;

       public double D1;
       public double D2;
       public double D3;

       public decimal Salary1;
       public decimal Salary2;
       public decimal Salary3;
                                     

                          
       //public byte[] Buffer;
       //public char[] Chars;

       //public Dictionary<object, object> Items = new Dictionary<object,object>();

       //public List<int> Numbers = new List<int>();
       //public List<Person> Relatives = new List<Person>();

       //public object[] ObjArray;
       //public int[,,] IntArray;

       //public PatientRecord Record = PatientRecord.Make<PatientRecord>();
    }


    [Serializable]
    public class TwitterMsg : TypedRow
    {
      
      [Field]public GDID ID{get; set;}
      [Field]public GDID ResponseToMsg{get; set;}
      [Field]public GDID AuthorID{get; set;}
      [Field]public DateTime When{get; set;}
      [Field]public string MsgText{get; set;}
      [Field]public bool Banned{get; set;}
      [Field]public int Rating{get; set;}

      [Field]public DateTime ri_Version{get;set;}
      [Field]public string ri_Host{get;set;}
      [Field]public bool ri_Deleted{get;set;}


      //private Person m_Person = new Person();

    }


  




}
