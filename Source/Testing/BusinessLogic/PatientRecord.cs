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

using NFX;
using NFX.Inventorization;
using NFX.RecordModel;
using NFX.ApplicationModel;
using NFX.Security;


namespace BusinessLogic
{



/*
 PostgreSQL:
create table TBL_PATIENT
(
  PATIENT_ID  VARCHAR(12) NOT NULL PRIMARY KEY,
  FIRST_NAME  VARCHAR(50) NOT NULL,
  LAST_NAME  VARCHAR(50) NOT NULL,
  PHONE_CONTACT  VARCHAR(25),
  DOB  timestamp NOT NULL,
  ADM_DATE  timestamp,
  MONTHLY_COST  money,
  ATTITUDE_CLASSIFICATION CHAR(3) NOT NULL,
  IS_RELIGIOUS boolean not null
)
*/



  
  [Serializable]
  [Inventory(Tiers=SystemTiers.DBServer, 
             Concerns=SystemConcerns.Testing,
             Technology="MongoDB",
             Schema="NFX.Testing")]
  [RecordDef(SupportsNotificationBinding=true, NoBuildCrosscheks=false)]
  [ClientScript( ResourceName = "ClientValidation.Patient.@tech@.js")]
  public class PatientRecord : Record
  {  
     
     private StringField m_fldID;

     [ClientScript] //this is equivalent to [ClientScript(ResourceName="PatientRecord.m_fldFirstName.@tech@.js")]
     private StringField m_fldFirstName;
     private StringField m_fldLastName;
     private StringField m_fldContactPhone;
     private DateTimeField m_fldDOB;
     
     [ClientScript(Type = ClientScripts.VALIDATION_TYPE, 
                   ResourceName = "ClientValidation.Patient.AdmDate.@tech@.js")]
     private DateTimeField m_fldAdmissionDate;
     
     private DecimalField m_fldMonthlyCost;
     private StringField m_fldClassification;
     
     
     [ClientScript(
        Type = ClientScripts.VALIDATION_TYPE,
        Text = " if (fldReligious.Value && fldMonthlyCost.Value>10000) throw 'Cant be religious and charge that much' ",
        Technology = "nfx")]
     private BoolField m_fldReligious;
     

     public IField<string> fldID { get { return m_fldID; } }
     public IField<string> fldFirstName { get {return m_fldFirstName;} }
     public IField<string> fldLastName { get { return m_fldLastName; } }
     public IField<string> fldContactPhone { get { return m_fldContactPhone; } }
     public IField<DateTime> fldDOB { get { return m_fldDOB; } }
     public IField<DateTime> fldAdmissionDate { get { return m_fldAdmissionDate; } }
     public IField<Decimal> fldMonthlyCost { get { return m_fldMonthlyCost; } }
     public IField<string> fldClassification { get { return m_fldClassification; } }
     public IField<bool> fldReligious { get { return m_fldReligious; } }
     


     protected override void ConstructFields()
     {                                
       TableName = "TBL_PATIENT";

       m_fldID = new StringField()
       {        
         Owner = this,
         FieldName = "PATIENT_ID",
         Description = "ID",
         Size = 12,
         Required = true,
         KeyField = true
       };
       
       m_fldFirstName = new StringField(){
          Owner = this,
          FieldName = "FIRST_NAME",
          Description = "First Name",
          Size = 50,
          Required = true
       };

       m_fldLastName = new StringField()
       {
         Owner = this,
         FieldName = "LAST_NAME",
         Description = "Last Name",
         Size = 50,
         Required = true
       };

       m_fldContactPhone = new StringField()
       {
         Owner = this,  
         FieldName = "PHONE_CONTACT",
         Description = "Contact Phone#",
         DataEntryFormat = DataEntryFormat.Phone,
         Size = 25
       };
       
       m_fldDOB = new DateTimeField()
       {
         Owner = this,
         FieldName = "DOB",
         Description = "Date of Birth",
         Required = true  
       };

       m_fldAdmissionDate = new DateTimeField()
       {
         Owner = this,
         FieldName = "ADM_DATE",
         Description = "Admission Date"
       };

       m_fldMonthlyCost = new DecimalField()
       {
         Owner = this,
         FieldName = "MONTHLY_COST",
         Description = "Monthly Cost",
         MinValue = 0,
         DisplayFormat = "C"
       };

       m_fldClassification = new StringField()
       {
         Owner = this,
         FieldName = "ATTITUDE_CLASSIFICATION",
         Description = "Classification",
         Required = true,
         Size = 3,
         LookupType = LookupType.Dictionary,
         DataEntryType = DataEntryType.Lookup,
         
         HasDefaultValue = true,
         DefaultValue = "BUD"
       };

       m_fldClassification.LookupDictionary.Add("MRX, Marxist",
                                                     "CAP, Capitalist",
                                                     "NEH, Nehilist",
                                                     "BUD, Buddhist");

       m_fldReligious = new BoolField()
       {
         Owner = this,
         FieldName = "IS_RELIGIOUS",
         Description = "Religious",
         Required = true,
         DefaultValue = false,
         HasDefaultValue = true
       };
       
     }
     
     const string ABSURDIST = "ABS, Absurdist";

     protected override void AttachEvents()
     {              
       base.AttachEvents();                  

       m_fldFirstName.FieldDataChanged += (s, a) => m_fldLastName.Value = m_fldFirstName.Value + ", ";

       m_fldMonthlyCost.FieldDataChanged += (s, a) => m_fldAdmissionDate.Marked = m_fldMonthlyCost.Value > 10000;

       m_fldReligious.FieldDataChanged += (s, a) =>
       {       
         if (m_fldReligious.Value)
           m_fldClassification.LookupDictionary.Include(ABSURDIST);
         else
           m_fldClassification.LookupDictionary.Exclude(ABSURDIST);
       };
                 
       m_fldContactPhone.Validation += (s, e) =>
              { 
                if (ExecutionContext.Session.User.Status != UserStatus.Administrator)
                 if (m_fldContactPhone.HasValue)
                   if (!m_fldContactPhone.Value.StartsWith("(555)")) throw new ModelValidationException("May only call area code 555");
              };
     }

     

  
  }
  
  
}
