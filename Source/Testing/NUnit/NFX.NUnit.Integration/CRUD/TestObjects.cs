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
using NFX.DataAccess.CRUD;
using NFX.DataAccess.Distributed;

namespace NFX.NUnit.Integration.CRUD
{
    [Serializable]
    public class Patient : TypedRow
    {
        public Patient() {}


        [Field(storeFlag: StoreFlag.None)]
        public string Marker {get; set;}

        [Field(required: true, key: true)]
        public long COUNTER {get; set;}

        [Field(required: true)]
        public string SSN {get; set;}

        [Field(required: true, backendName: "fname")]
        public string First_Name {get; set;}
        
        [Field(required: true, backendName: "lname")]
        public string Last_Name {get; set;}
        
        [Field(required: true)]
        public DateTime DOB {get; set;}

        
        [Field()]
        public string Address1 {get; set;}
        [Field()]
        public string Address2 {get; set;}
        [Field()]
        public string City {get; set;}
        [Field()]
        public string State {get; set;}
        [Field()]
        public string Zip {get; set;}


        [Field()]
        public long C_DOCTOR {get; set;}
        [Field(storeFlag: StoreFlag.OnlyLoad)]
        public string Doctor_Phone {get; set;}
        [Field(storeFlag: StoreFlag.OnlyLoad)]
        public string Doctor_ID {get; set;}

        [Field()]
        public string Phone {get; set;}

        [Field()]
        public string Years_In_Service {get; set;}

        [Field()]
        public decimal Amount {get; set;}

        [Field()]
        public string Note {get; set;}
    }



    [Table(name: "tbl_Patient")] //different class write to the same table as Patient above
    [Serializable]
    public class SuperPatient : Patient
    {
        public SuperPatient() {}

        [Field(storeFlag: StoreFlag.OnlyLoad)]
        public bool Superman {get; set;}

    }


    public class Types : TypedRow
    {
      [Field(key: true)]
      public GDID GDID{ get; set;} 

      [Field]
      public string Screen_Name{ get; set;} 

      [Field]
      public string String_Name{ get; set;}
      
      [Field]
      public string Char_Name{ get; set;} 

      [Field]
      public bool? Bool_Char{ get; set;} 

      [Field]
      public bool? Bool_Bool{ get; set;}
      
      [Field]
      public decimal? Amount{ get; set;}
      
      
      [Field]
      public DateTime? DOB{ get; set;} 

      [Field]
      public int? Age{ get; set;} 
    }


    public class FullGDID : TypedRow
    {
      [Field]
      public GDID GDID{ get; set;} 
 
      [Field]
      public GDID VARGDID{ get; set;} 


      [Field]
      public string String_Name{ get; set;}
    }


    [Serializable]
    [Table(name: "tbl_tuple")]
    public class TupleData : TypedRow
    {
        public TupleData() {}

        [Field(required: true, key: true)]
        public long COUNTER {get; set;}

        [Field(required: true)]
        public string DATA {get; set;}
    }

}
