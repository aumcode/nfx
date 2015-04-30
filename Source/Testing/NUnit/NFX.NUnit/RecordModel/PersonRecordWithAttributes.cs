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

using NFX.DataAccess;
using NFX.RecordModel;

namespace NFX.NUnit.RecordModel
{
    /// <summary>
    /// Record for testing configured using attributes
    /// </summary>
    [RecordDef(TableName = "TBL_TESTPERSON", StoreFlag = StoreFlag.LoadAndStore, NoBuildCrosscheks = true)]
    [ClientScript]
    public class PersonRecordWithAttributes : NFX.RecordModel.Record
    {
#pragma warning disable 0649
       
        [FieldDef(Name = "PERSON_NAME", Required = true, Size = 50)]
        [ClientScript]
        [ClientScript( Text="inline script", Type = "testing")]
        [ClientScript( Text="inline script for zizikaka", Technology="zizikaka")]
        [ClientScript( Text="inline testing script for zizikaka", Technology="zizikaka", Type = "testing")]
        private StringField m_fldName;       public IField<string> fldName { get{ return m_fldName;} }


        [FieldDef(Name = "SEX",
                  Required = true,
                  Size = 1,
                  DataEntryType = DataEntryType.DirectEntryOrLookupWithValidation,
                  LookupType = LookupType.Dictionary,
                  LookupDictionary = new string[] { "M,Male", "F,Female", "U,Unspecified" }
                 )]
        private StringField m_fldSex;        public IField<string> fldSex { get{ return m_fldSex;} }



        [FieldDef(Name = "RATING",
                  Description = "Personal Rating",  
                  MinMaxChecking = true,
                  MinValue = 0,
                  MaxValue = 10  
                 )]
        private IntField    m_fldRating;     public IField<int> fldRating { get{ return m_fldRating;} }



        [FieldDef(Name = "IS_REGISTERED", Required = true)]
        private BoolField   m_fldRegistered; public IField<bool> fldRegistered { get{ return m_fldRegistered;} }

        
        [FieldDef(Name = "DOB")]
        private DateTimeField m_fldDOB;      public IField<DateTime> fldDOB { get{ return m_fldDOB;} }


        [FieldDef(Name = "PAYROLL_VOLUME",
                  Required = true,
                  MinMaxChecking = true,
                  MinValue = 0,
                  MaxValue = 100000000
                 )]
        private DecimalField m_fldSalary;         public IField<decimal> fldSalary { get{ return m_fldSalary;} }



        [FieldDef(Name = "DECADE_SALARY",
                  StoreFlag = StoreFlag.None,
                  DataEntryType = DataEntryType.None,
                  Calculated = true,
                  Formula = "850-10 +(::PAYROLL_VOLUME * 10)"
                 )]
        private DecimalField m_fldDecadeSalary;   public IField<decimal> fldDecadeSalary { get{ return m_fldDecadeSalary;} }


        [FieldDef(Name = "SCORE")]
        private DoubleField  m_fldScore;            public IField<double> fldScore { get{ return m_fldScore;} }


        [FieldDef(Name = "MOVIES", Required = true)]
        private ListField<string> m_fldMovieNames;  public IField<ListOf<string>> fldMovieNames { get{ return m_fldMovieNames;} }


        [FieldDef(Name = "BUSINESS_CODE", StoreFlag = StoreFlag.None, DataEntryType = DataEntryType.None)]
        private StringField m_fldBusinessCode;  public IField<string> fldBusinessCode { get{ return m_fldBusinessCode;} }

#pragma warning restore 0649


        protected override void AttachEvents()
        {
            //DOB is required for registered persons
            m_fldRegistered.FieldDataChanged += new FieldDataChangeEventHandler(
                                                     (s, a) => m_fldDOB.Required = m_fldRegistered
                                                   );
        }

        protected internal override void OnFieldDataChanged(Field sender, FieldDataChangeEventArgs args)
        {
            base.OnFieldDataChanged(sender, args);

            //recalculate business code according to this business rule
            if (m_fldBusinessCode.IsMutable)
            {            
                //example of implicit value property access
                int hash = m_fldRating + 10 + (int)Math.Truncate(m_fldScore * 2D);
                string str = "Rating " + m_fldRating + " for " + m_fldName;

                m_fldBusinessCode.Value =
                 string.Format("[{0}] {1}-{2} {3}",
                   m_fldName.GetHashCode() + hash,
                   m_fldDOB.HasValue ? m_fldDOB : App.LocalizedTime,
                   ~m_fldRegistered,
                   str
                 );

            }

        }



    }
}
