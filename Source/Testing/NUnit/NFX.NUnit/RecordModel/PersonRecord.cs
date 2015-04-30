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
    /// Record for testing
    /// </summary>
    public class PersonRecord : NFX.RecordModel.Record
    {
        private StringField m_fldName;       public IField<string> fldName { get{ return m_fldName;} }
        private StringField m_fldSex;        public IField<string> fldSex { get{ return m_fldSex;} }
        private IntField    m_fldRating;     public IField<int> fldRating { get{ return m_fldRating;} }
        private BoolField   m_fldRegistered; public IField<bool> fldRegistered { get{ return m_fldRegistered;} }
        private DateTimeField m_fldDOB;      public IField<DateTime> fldDOB { get{ return m_fldDOB;} }

        private DecimalField m_fldSalary;         public IField<decimal> fldSalary { get{ return m_fldSalary;} }
        private DecimalField m_fldDecadeSalary;   public IField<decimal> fldDecadeSalary { get{ return m_fldDecadeSalary;} }
        
        private DoubleField  m_fldScore;            public IField<double> fldScore { get{ return m_fldScore;} }
        private ListField<string> m_fldMovieNames;  public IField<ListOf<string>> fldMovieNames { get{ return m_fldMovieNames;} }
        
        //this is demoing calculated field
        private StringField m_fldBusinessCode;  public IField<string> fldBusinessCode { get{ return m_fldBusinessCode;} }
        
        protected override void ConstructFields()
        {
           TableName = "TBL_TESTPERSON"; 
           StoreFlag = StoreFlag.LoadAndStore; 
           
           m_fldName = new StringField()
           {
             Owner = this,
             FieldName = "PERSON_NAME",
             Required = true,
             Size = 50
           };
           
           m_fldSex = new StringField()
           {
             Owner = this,
             FieldName = "SEX",
             Required = true,
             Size = 1,
             DataEntryType = DataEntryType.DirectEntryOrLookupWithValidation,
             LookupType = LookupType.Dictionary
           };
           m_fldSex.LookupDictionary.AddRange(new string[]{"M,Male", "F,Female", "U,Unspecified"});

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

           m_fldSalary = new DecimalField()
           {
             Owner = this,
             FieldName = "PAYROLL_VOLUME",
             MinMaxChecking = true,
             MinValue = 0M,
             MaxValue = Decimal.MaxValue
           };

           //calculated field returns salary * 10 years
           m_fldDecadeSalary = new DecimalField()
           {
             Owner = this,
             FieldName = "DECADE_SALARY",
             StoreFlag = StoreFlag.None,
             DataEntryType = DataEntryType.None,
             Calculated = true,
             Formula = "850-10 +(::PAYROLL_VOLUME * 10)"
           };

           m_fldScore = new DoubleField()
           {
             Owner = this,
             FieldName = "SCORE"
           };


           m_fldMovieNames = new ListField<string>()
           {
             Owner = this,
             FieldName = "MOVIES",
             Required = true
           };


           //ths is business logic-driven computed column for GUI
           m_fldBusinessCode = new StringField()
           {
             Owner = this,
             FieldName = "BUSINESS_CODE",
             StoreFlag = StoreFlag.None,
             DataEntryType = DataEntryType.None
           };

            
        }

        protected override void PerformValidation(ref bool couldNotValidate)
        {
            base.PerformValidation(ref couldNotValidate);

        }


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
