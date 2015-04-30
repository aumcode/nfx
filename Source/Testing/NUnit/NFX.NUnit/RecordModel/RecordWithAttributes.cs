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
using System.Text;
using System.Collections.Generic;
using System.Linq;
using NUnit.Framework;


using NFX.RecordModel;
using R=NFX.RecordModel.Record;

namespace NFX.NUnit.RecordModel
{
    [TestFixture]   
    public class RecordWithAttributes
    {
        [TestCase]
        public void BasicCreateSetPostGet()
        {
          var rec = makeValidPostedPerson();
          
          Assert.AreEqual("John Frank Bookman", rec.fldName.Value);
          Assert.AreEqual(10, rec.fldScore.Value);
          Assert.AreEqual("M", rec.fldSex.Value);
          Assert.AreEqual(75000M, rec.fldSalary.Value);
          Assert.AreEqual(DateTime.Parse("June 18, 1955"), rec.fldDOB.Value);
          Assert.AreEqual(true, rec.fldRegistered.Value);
          Assert.AreEqual("Titanic", rec.fldMovieNames.Value[0]);
        }


        [TestCase]
        public void AccessFieldByName()
        {
          var rec = makeValidPostedPerson();
          
          Assert.AreEqual("John Frank Bookman", rec["PERSON_NAME"].ValueAsString);
          Assert.AreEqual(10, rec["SCORE"].ValueAsInt);
        }

        [TestCase]
        public void CalculatedField()
        {
          var rec = makeValidPostedPerson();
          
          Assert.AreEqual(850-10+(rec.fldSalary.Value*10), rec.fldDecadeSalary.Value);

          //the cast is needed because AlloCalcOverride is read-only through pub contract
          ((DecimalField)rec.fldDecadeSalary).AllowCalculationOverride = true;

          rec.Edit();
           rec.fldDecadeSalary.Value = 900;
          rec.Post();
          //we have overridden the formula, so now we should get what we have written-in directly
          Assert.AreEqual(900, rec.fldDecadeSalary.Value);

          rec.Edit();
           rec.fldDecadeSalary.ResetOverridden();
          rec.Post();
          //again, after reset formula kicked in and yields older result
          Assert.AreEqual(850-10+(rec.fldSalary.Value*10), rec.fldDecadeSalary.Value);

        }


        [TestCase]
        [ExpectedException(typeof(ModelValidationException))]
        public void LookupDictionaryValidationError()
        {
          var rec = makeValidPostedPerson();
          
          rec.Edit();
           rec.fldSex.Value = "F";
          rec.Post();

          Assert.AreEqual("F", rec.fldSex.Value); //was M now F and this is OK

          rec.FieldValidationSuspended = false;//make sure that fields validate upon assignment
          rec.Edit();
           rec.fldSex.Value = "z";//ERROR!!! however not thrown until rec.Post()
           //error gets written to local field-level property
           Assert.AreEqual(typeof(LookupValidationException), rec.fldSex.ValidationException.GetType());
            
          rec.Post(); //and this WILL throw as record can not be posted in invalid state
        }
       
       
       
        [TestCase]
        [ExpectedException(typeof(RecordModelException))]
        public void StateExceptionOnMutation()
        {
          var rec = R.Make<PersonRecord>();

          rec.fldName.Value = "aaa"; //cant write to non-init record
        }

        [TestCase]
        [ExpectedException(typeof(ModelValidationException))]
        public void ValidationExceptionOnPost()
        {
          var rec = R.Make<PersonRecord>();
          rec.Create();
          rec.fldRegistered.Value = true; 
          rec.Post();//rest of required fields are blank
        }


        [TestCase]
        public void RecalcOfDependentFields()
        {
          var rec = makeValidPostedPerson();
          
          var v1 = rec.fldBusinessCode.Value;
          
          rec.Edit();
           rec.fldName.Value = "Some Other Name";

          Assert.AreNotEqual(v1, rec.fldBusinessCode.Value);

          rec.Post();

          Assert.AreNotEqual(v1, rec.fldBusinessCode.Value);
        }


        [TestCase]
        public void FieldReverting()
        {
          var rec = makeValidPostedPerson();
          
          rec.Edit();

          rec.fldName.Value = "New Value";

          Assert.AreEqual("New Value", rec.fldName.Value);

          rec.fldName.Revert();

          Assert.AreEqual("John Frank Bookman", rec.fldName.Value);
        }

        [TestCase]
        public void RecordReverting()
        {
          var rec = makeValidPostedPerson();
          
          rec.Edit();

          rec.fldName.Value = "New Value";

          Assert.AreEqual("New Value", rec.fldName.Value);

          rec.Revert();//revert values of ALL fields, record fields can still be changed

          Assert.AreEqual("John Frank Bookman", rec.fldName.Value);
        }


        [TestCase]
        public void RecordCancel()
        {
          var rec = makeValidPostedPerson();
          
          rec.Edit();

          rec.fldName.Value = "New Value";

          Assert.AreEqual("New Value", rec.fldName.Value);

          rec.Cancel(); //cancel changes to whole record, after this call no mutation possible

          Assert.AreEqual("John Frank Bookman", rec.fldName.Value);
        }

        [TestCase]
        public void RecordCancelListOfTField()
        {
          var rec = makeValidPostedPerson();
          
          rec.Edit();

          Assert.AreEqual(1, rec.fldMovieNames.Value.Count);

          rec.fldMovieNames.Value.Add("Another Movie");
          rec.fldMovieNames.Value.Add("Another Movie2");
          rec.fldMovieNames.Value.Add("Another Movie3");

          Assert.AreEqual(4, rec.fldMovieNames.Value.Count);

          rec.Cancel(); //cancel changes to whole record

          Assert.AreEqual(1, rec.fldMovieNames.Value.Count);
        }


        [TestCase]
        public void DefaultValues()
        {
           var rec = R.Make<PersonRecord>();

           //this is black magic, never cast fields in normal use cases
           var fldName = (StringField)rec.fldName;
           fldName.DefaultValue = "Hello!";
           fldName.HasDefaultValue = true;         

           var fldRegistered = (BoolField)rec.fldRegistered;
           fldRegistered.DefaultValue = false;
           fldRegistered.HasDefaultValue = true;

           rec.Create();//this should default field values

           Assert.AreEqual("Hello!", ~fldName);
           Assert.IsTrue(rec.fldName.HasValue);

           Assert.IsFalse(~fldRegistered);
           Assert.IsTrue(rec.fldName.HasValue);

        }


        [TestCase]
        public void ClearValues()
        {
           var rec = makeValidPerson();

           Assert.IsTrue(rec.fldName.HasValue);

           rec.fldName.Clear();

           Assert.IsFalse(rec.fldName.HasValue);

           //now test LsitOf<T> field
           Assert.IsTrue(rec.fldMovieNames.Value.Count >0);

           rec.fldMovieNames.Clear();

           Assert.IsFalse(rec.fldMovieNames.Value.Count >0);

        }

        [TestCase]
        [ExpectedException(typeof(RecordModelException))]
        public void ListOfTFieldImmutability()
        {
           var rec = R.Make<PersonRecord>();
           
           rec.fldMovieNames.Value.Add("cant go");
        }



        [TestCase]
        [ExpectedException(typeof(RecordModelException))]
        public void ListOfTFieldAssignError()
        {
          var rec = makeValidPostedPerson();
          var rec2 = makeValidPostedPerson();
         
          rec.Edit();

          rec.fldMovieNames.Value = rec2.fldMovieNames.Value; //cant assign lists
        }


        [TestCase]
        public void ListOfTFieldReverting()
        {
          var rec = makeValidPostedPerson();
          
          rec.Edit();

          Assert.AreEqual(1, rec.fldMovieNames.Value.Count);

          rec.fldMovieNames.Value.Add("Movie 1");
          rec.fldMovieNames.Value.Add("Movie 2");

          Assert.AreEqual(3, rec.fldMovieNames.Value.Count);

          rec.fldMovieNames.Revert();

          Assert.AreEqual(1, rec.fldMovieNames.Value.Count);
        }




        [TestCase]
        public void MeasureAllocationSpeed()
        {
            const int TOTAL = 1000;
            var lst = new List<R>(TOTAL);

            var clock = System.Diagnostics.Stopwatch.StartNew();
            
            for(int i=0; i<TOTAL; i++)
              lst.Add(makeValidPostedPerson());
              
            clock.Stop();
            Console.WriteLine(string.Format("$$$$$$>  Allocated {0} PersonRecordWithAttributes instances in {1} ms", TOTAL, clock.ElapsedMilliseconds));
        }


        [TestCase]
        public void GetRecordClientScripts()
        {
            var rec = makeValidPostedPerson();

            var scripts = rec.GetClientScripts("test-tech");
            Assert.AreEqual("This is record validation script text for test technology", scripts[ClientScripts.VALIDATION_TYPE]);
        }

        [TestCase]
        public void GetFieldClientScripts()
        {
            var rec = makeValidPostedPerson();
           
            var scripts = ((Field)rec.fldName).GetClientScripts("test-tech");

            Assert.AreEqual("This is field validation script text for test technology", scripts[ClientScripts.VALIDATION_TYPE]);
            Assert.AreEqual("inline script", scripts["testing"]);
        }

        [TestCase]
        public void GetFieldClientScriptsForDifferentTechnology()
        {
            var rec = makeValidPostedPerson();
           
            var scripts = ((Field)rec.fldName).GetClientScripts("zizikaka");

            Assert.AreEqual("inline script for zizikaka", scripts[ClientScripts.VALIDATION_TYPE]);
            Assert.AreEqual("inline testing script for zizikaka", scripts["testing"]);
        }

        
        private PersonRecordWithAttributes makeValidPostedPerson()
        {
          var rec = makeValidPerson();
                   
          rec.Post();

          return rec;
        }

        private PersonRecordWithAttributes makeValidPerson()
        {
          var rec = R.Make<PersonRecordWithAttributes>();

              rec.Create();
                rec.fldName.Value = "John Frank Bookman";
                rec.fldScore.Value = 10;
                rec.fldSex.Value = "M";
                rec.fldSalary.Value = 75000M;
                rec.fldRegistered.Value = true;
                rec.fldDOB.Value = DateTime.Parse("June 18, 1955");
                rec.fldMovieNames.Value.Add("Titanic");
          
          return rec;
        }

    }
}
