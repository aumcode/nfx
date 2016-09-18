/*<FILE_LICENSE>
* NFX (.NET Framework Extension) Unistack Library
* Copyright 2003-2016 IT Adapter Inc.
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
using System.Threading.Tasks;

using NFX;
using NFX.DataAccess.CRUD;
using NFX.Security;
using NFX.Serialization.JSON;
using NFX.Wave.MVC;
using NFX.Parsing;

namespace WaveTestSite.Controllers
{
  public class LoadTester: Controller
  {
    #region Nested classes

      public enum PatientStatus { Admitted, Discharged};


      public class Patient: TypedRow
      {
        [Field] public int ID { get; set; }
        [Field] public string Name { get; set; }

        [Field] public PatientStatus Status { get; set;}

        [Field] public DateTime AdmissionDate { get; set; }
        [Field] public DateTime? DischargeDate { get; set; }

        [Field] public decimal Balance { get; set;}
        [Field] public byte[] BinData{ get; set;}
      }

      public class Doctor: TypedRow
      {
        [Field] public int ID { get; set; }
        [Field] public string Name { get; set; }
        [Field] public bool Certified { get; set; }
        [Field] public List<Patient> Patients { get; set; }
      }
        
    #endregion

    [Action]
    public string Echo(string input)
    {
      return "from server, received: "+input;
    }

    [Action]
    public object Doctors(int count = 1, int sleepMs=0, int minPatientCount=0, int maxPatientCount=10)
    {
    //  WorkContext.Response.Buffered = false;

      var result = new List<Doctor>();

      for(var i=0; i<count; i++)
      {
        var doctor = new Doctor
        {
          ID = i,
           Certified = i%2==0,
            Name = "Doctor # "+i, //NaturalTextGenerator.Generate(15),
             Patients = new List<Patient>()
        };

        var pcount = ExternalRandomGenerator.Instance.NextScaledRandomInteger(minPatientCount, maxPatientCount);
        for (var j=0; j<pcount; j++)
        {
          var patient = new Patient
          {
             ID = i*1000+j,
              AdmissionDate = DateTime.Now,
               Balance = j*123.11m,
                DischargeDate = i%3==0?(DateTime?)null:DateTime.Now,
                  Name = "Patient # "+j,
                   Status = i%7==0?PatientStatus.Admitted:PatientStatus.Discharged,
          };

          patient.BinData = new byte[ExternalRandomGenerator.Instance.NextScaledRandomInteger(0,7)];
          for(var k=0; k<patient.BinData.Length; k++)
           patient.BinData[k] = (byte)ExternalRandomGenerator.Instance.NextScaledRandomInteger(0, 255);

          doctor.Patients.Add(patient);

        }

        if (sleepMs>0)//Simulate some kind of DB access
           System.Threading.Thread.Sleep(ExternalRandomGenerator.Instance.NextScaledRandomInteger(0, sleepMs));

        result.Add(doctor);
      }


      return result;
    }

   

    //protected override System.Reflection.MethodInfo FindMatchingAction(NFX.Wave.WorkContext work, string action, out object[] args)
    //{
    //  return base.FindMatchingAction(work, action, out args);
    //}

    //multipart (byte array as well)
    //public object RowSet(different data types: decimal, bool, float, double, DateTime, TimeSpan and their Nullable versions)
    //public object RowSet(JSONDataMap row, int a, string b)
    //public object RowSet(int a, string b, JSONDataMap row)
    //public object RowSet(TestRow row, int a, string b)
    //match{is-local=false}
  }
}
