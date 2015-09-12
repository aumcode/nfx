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

using NFX.ApplicationModel;
using NFX.DataAccess.CRUD;
using NFX.DataAccess.Distributed;

namespace NFX.NUnit.DataAccess
{
   
    [Serializable]
    [Table(targetName: "SPARTA_SYSTEM", name: "dimperson")]
    public class Person : TypedRow
    {
        public Person() {}

        [Field(required: true, key: true)]
        public string ID {get; set;}

        [Field]
        public string FirstName {get; set;}
        
        [Field(required: true)]
        public string LastName {get; set;}

        [Field(valueList: "GOOD,BAD,UGLY")]
        public string Classification {get; set;}
        
        [Field(required: true, min: "01/01/1900")]
        [Field(targetName: "SPARTA_SYSTEM", required: true, backendName: "brthdt", min: "01/01/1800")]
        public DateTime DOB {get; set;}

        [Field]
        [Field(targetName: "ORACLE", backendName: "empl_yrs")]
        [Field(targetName: "SPARTA_SYSTEM", backendName: "tenure")]
        public int? YearsWithCompany {get; set;}

        [Field(required: true)]
        public int? YearsInSpace {get; set;}


        [Field(min: 0d, max: "1000000" )]
        public decimal Amount {get; set;}

        [Field( maxLength: 25)]
        public string Description {get; set;}

        [Field]
        public bool GoodPerson {get; set;}

        [Field]
        public double LuckRatio {get; set;}
    }


    [Serializable]
    public class WithCompositeKey : TypedRow
    {
        public WithCompositeKey() {}

        [Field(required: true, key: true)]
        public string ID {get; set;}

        [Field(required: true, key: true)]
        public DateTime StartDate {get; set;}

        [Field(required: true)]
        public string Description {get; set;}
        
    }

    [Serializable]
    public class HistoryItem : TypedRow
    {
        public HistoryItem() {}

        [Field(required: true, key: true)]
        public string ID {get; set;}

        [Field(required: true, key: true)]
        public DateTime StartDate {get; set;}

        [Field(required: true)]
        public string Description {get; set;}

        public override Exception Validate(string targetName)
        {
          var error = base.Validate(targetName);
          if (error!=null) return error;

          if (!Description.Contains("Chaplin")) 
            return new CRUDFieldValidationException("Chaplin is required in description", "Description");

          return null;
        }
        
    }


    [Serializable]
    public class PersonWithNesting : Person
    {
        public PersonWithNesting() {}

        [Field(required: true)]
        public List<HistoryItem> History1 {get; set;}

        [Field(required: true)]
        public HistoryItem[] History2 {get; set;}

        [Field]
        public HistoryItem LatestHistory { get ; set;}
    }


    [DataParcel(schemaName: "Testing", areaName: "Testing", replicationChannel: "General")]
    public class PeopleNamesParcel : Parcel<List<string>>
    {
      public PeopleNamesParcel(GDID id, List<string> data) : base(id, data)
      {
      }

      public override bool ReadOnly
      {
        get { return false; }
      }

      protected override void DoValidate(IBank bank)
      {
        if (!Payload.Contains("Aroyan")) m_ValidationExceptions.Add(new ParcelValidationException ("Aroyan must be present") );
      }
    }

       public enum HumanStatus{Ok, NotOk, Unknown}

       public class Human : TypedRow
       { 
         [Field]public GDID ID{ get; set;}
         [Field]public string Name{ get; set;}
         [Field]public DateTime DOB{ get; set;}
         [Field]public HumanStatus Status{ get; set;}
         
         [Field]public Human Father{ get; set;}
         [Field]public Human Mother{ get; set;}

         [Field]public string Addr1{ get; set;}
         [Field]public string Addr2{ get; set;}
         [Field]public string AddrCity{ get; set;}
         [Field]public string AddrState{ get; set;}
         [Field]public string AddrZip{ get; set;}
       }



    [DataParcel(schemaName: "Testing", areaName: "Testing", replicationChannel: "General")]
    public class HumanParcel : Parcel<Human>
    {
      public HumanParcel(GDID id, Human data) : base(id, data)
      {

      }

      public override bool ReadOnly  {   get { return false; }   }
      protected override void DoValidate(IBank bank){}
      
    }

    
    //used only for test
    public class FakeNOPBank : IBank
    {

      public static readonly FakeNOPBank Instance = new FakeNOPBank();
      
      
      public IDistributedDataStore DataStore {get { return null;}}
     

      public ISchema Schema {get { return null;}}

      public IRegistry<IAreaInstance> Areas {get { return null;}}


      public string Description { get {return "NOP";}}

      public string GetDescription(string culture) { return "NOP";}

      public NFX.DataAccess.IGDIDProvider IDGenerator {get { return null;}}

      public IReplicationVersionInfo GenerateReplicationVersionInfo(Parcel parcel) { return null;}


      public UInt64 ObjectToShardingID(object key)
      {
        return NFX.DataAccess.Cache.ComplexKeyHashingStrategy.DefaultComplexKeyToCacheKey(key);
      }


      public Parcel Load(Type tParcel, GDID id, object shardingId = null, DataVeracity veracity = DataVeracity.Maximum, DataCaching cacheOpt = DataCaching.LatestData, int? cacheMaxAgeSec = null, ISession session = null)
      {
        throw new NotImplementedException();
      }

      public T Load<T>(GDID id, object shardingId = null, DataVeracity veracity = DataVeracity.Maximum, DataCaching cacheOpt = DataCaching.LatestData, int? cacheMaxAgeSec = null, ISession session = null) where T : Parcel
      {
        throw new NotImplementedException();
      }

      public System.Threading.Tasks.Task<Parcel> LoadAsync(Type tParcel, GDID id,object shardingId = null, DataVeracity veracity = DataVeracity.Maximum, DataCaching cacheOpt = DataCaching.LatestData, int? cacheMaxAgeSec = null, ISession session = null)
      {
        throw new NotImplementedException();
      }

      public System.Threading.Tasks.Task<T> LoadAsync<T>(GDID id,object shardingId = null, DataVeracity veracity = DataVeracity.Maximum, DataCaching cacheOpt = DataCaching.LatestData, int? cacheMaxAgeSec = null, ISession session = null) where T : Parcel
      {
        throw new NotImplementedException();
      }

      public object Query(Command command, DataVeracity veracity = DataVeracity.Maximum, DataCaching cacheOpt = DataCaching.LatestData, int? cacheMaxAgeSec = null, ISession session = null)
      {
        throw new NotImplementedException();
      }

      public System.Threading.Tasks.Task<object> QueryAsync(Command command, DataVeracity veracity = DataVeracity.Maximum, DataCaching cacheOpt = DataCaching.LatestData, int? cacheMaxAgeSec = null, ISession session = null)
      {
        throw new NotImplementedException();
      }

      public void Save(Parcel parcel, DataCaching cacheOpt = DataCaching.Everywhere, int? cachePriority = null, int? cacheMaxAgeSec = null, DateTime? cacheAbsoluteExpirationUTC = null, ISession session = null)
      {
        throw new NotImplementedException();
      }

      public System.Threading.Tasks.Task SaveAsync(Parcel parcel, DataCaching cacheOpt = DataCaching.Everywhere, int? cachePriority = null, int? cacheMaxAgeSec = null, DateTime? cacheAbsoluteExpirationUTC = null, ISession session = null)
      {
        throw new NotImplementedException();
      }


      public bool Remove(Type tParcel, GDID id, object shardingId = null, ISession session = null)
      {
        throw new NotImplementedException();
      }

      public bool Remove<T>(GDID id, object shardingId = null, ISession session = null) where T : Parcel
      {
        throw new NotImplementedException();
      }


      public System.Threading.Tasks.Task<bool> RemoveAsync(Type tParcel, GDID id, object shardingId = null, ISession session = null)
      {
        throw new NotImplementedException();
      }

      public System.Threading.Tasks.Task<bool> RemoveAsync<T>(GDID id, object shardingId = null, ISession session = null) where T : Parcel
      {
        throw new NotImplementedException();
      }

      public string Name
      {
        get { throw new NotImplementedException(); }
      }
    }

}
