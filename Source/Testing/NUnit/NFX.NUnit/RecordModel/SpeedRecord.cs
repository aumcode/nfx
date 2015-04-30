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
using System.Linq;
using System.Text;
using NUnit.Framework;

using NFX.DataAccess;
using NFX.RecordModel;

namespace NFX.NUnit.RecordModel
{
   
   [TestFixture]   
    public class RecordPerformance
    {
       [TestCase]
       public void MeasureAllocationSpeed_withMake()
       {
           const int TOTAL = 10000;
           var lst = new List<SpeedRecord>(TOTAL);

           var clock = System.Diagnostics.Stopwatch.StartNew();

           for (int i = 0; i < TOTAL; i++)
           {
               var rec = NFX.RecordModel.Record.Make<SpeedRecord>(); 
               rec.Create();
                   rec.fldName.Value = "Some Name";
                   rec.fldScore.Value = 10;
                   rec.fldRegistered.Value = true;
                   rec.fldDOB.Value = App.LocalizedTime;
               rec.Post();

               lst.Add(rec);
           }

           clock.Stop();
           Console.WriteLine(string.Format("$$$$$$>  Allocated {0} SpeedRecord instances in {1} ms", TOTAL, clock.ElapsedMilliseconds));
       }

       [TestCase]
       public void MeasureAllocationSpeed_withBuild()
       {
           const int TOTAL = 10000;
           var lst = new List<SpeedRecord>(TOTAL);

           var clock = System.Diagnostics.Stopwatch.StartNew();

           for (int i = 0; i < TOTAL; i++)
           {
               var rec = NFX.RecordModel.Record.Build( new SpeedRecord());
               rec.Create();
                   rec.fldName.Value = "Some Name";
                   rec.fldScore.Value = 10;
                   rec.fldRegistered.Value = true;
                   rec.fldDOB.Value = App.LocalizedTime;
               rec.Post();

               lst.Add(rec);
           }

           clock.Stop();
           Console.WriteLine(string.Format("$$$$$$>  Allocated {0} SpeedRecord instances in {1} ms", TOTAL, clock.ElapsedMilliseconds));
       }

    }
   
   
   
   
   
    /// <summary>
    /// Record for testing
    /// </summary>
  //  [RecordDef(NoBuildCrosscheks = true, SupportsNotificationBinding = false)]
    public class SpeedRecord : NFX.RecordModel.Record
    {
        private StringField m_fldName;       public IField<string> fldName { get{ return m_fldName;} }
        private IntField    m_fldRating;     public IField<int> fldRating { get{ return m_fldRating;} }
        private BoolField   m_fldRegistered; public IField<bool> fldRegistered { get{ return m_fldRegistered;} }
        private DateTimeField m_fldDOB;      public IField<DateTime> fldDOB { get{ return m_fldDOB;} }
        private DoubleField  m_fldScore;     public IField<double> fldScore { get{ return m_fldScore;} }

        private StringField m_fldData1; public IField<string> fldData1 { get { return m_fldData1; } }
        private StringField m_fldData2; public IField<string> fldData2 { get { return m_fldData2; } }
        private StringField m_fldData3; public IField<string> fldData3 { get { return m_fldData3; } }
        private IntField m_fldData4; public IField<int> fldData4 { get { return m_fldData4; } }
        private IntField m_fldData5; public IField<int> fldData5 { get { return m_fldData5; } }
        private IntField m_fldData6; public IField<int> fldData6 { get { return m_fldData6; } }
        private BoolField m_fldData7; public IField<bool> fldData7 { get { return m_fldData7; } }
        private BoolField m_fldData8; public IField<bool> fldData8 { get { return m_fldData8; } }
        private BoolField m_fldData9; public IField<bool> fldData9 { get { return m_fldData9; } }
        private DoubleField m_fldData10; public IField<double> fldData10 { get { return m_fldData10; } }
        

        
        
        protected override void ConstructFields()
        {
           SupportsNotificationBinding = false;
           NoBuildCrosschecks = true;
           FieldValidationSuspended = true;

           TableName = "TBL_TESTPERSON"; 
           StoreFlag = StoreFlag.LoadAndStore; 
         
           m_fldName = new StringField()
           {
             Owner = this,
             FieldName = "PERSON_NAME",
             Required = true,
             Size = 50
           };


           m_fldRating = new IntField()
           {
               Owner = this,
               FieldName = "RATING",
               MinMaxChecking = true,
               MinValue = 0,
               MaxValue = 10
           };

           m_fldRegistered = new BoolField()
           {
               Owner = this,
               FieldName = "IS_REGISTERED",
               Required = true
           };

           m_fldDOB = new DateTimeField()
           {
               Owner = this,
               FieldName = "DOB"
           };


           m_fldScore = new DoubleField()
           {
               Owner = this,
               FieldName = "SCORE"
           };


           m_fldData1 = new StringField()
           {
               Owner = this,
               FieldName = "Data1"
           };

           m_fldData2 = new StringField()
           {
               Owner = this,
               FieldName = "Data2"
           };

           m_fldData3 = new StringField()
           {
               Owner = this,
               FieldName = "Data3"
           };

           m_fldData4 = new IntField()
           {
               Owner = this,
               FieldName = "Data4"
           };

           m_fldData5 = new IntField()
           {
               Owner = this,
               FieldName = "Data5"
           };

           m_fldData6 = new IntField()
           {
               Owner = this,
               FieldName = "Data6"
           };

           m_fldData7 = new BoolField()
           {
               Owner = this,
               FieldName = "Data7"
           };

           m_fldData8 = new BoolField()
           {
               Owner = this,
               FieldName = "Data8"
           };

           m_fldData9 = new BoolField()
           {
               Owner = this,
               FieldName = "Data9"
           };

           m_fldData10 = new DoubleField()
           {
               Owner = this,
               FieldName = "Data10"
           };




        }


    }
}
