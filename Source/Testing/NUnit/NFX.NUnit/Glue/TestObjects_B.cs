/*<FILE_LICENSE>
* NFX (.NET Framework Extension) Unistack Library
* Copyright 2003-2018 Agnicore Inc. portions ITAdapter Corp. Inc.
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
using System.Reflection;


using NFX.Glue;
using NFX.Glue.Protocol;

namespace NFX.NUnit.Glue
{
        [Serializable]
        public class PersonData
        {
            public ulong ID {get; set;}
            public string FirstName {get; set;}
            public string LastName {get; set;}
            public bool Certified {get; set;}
            public decimal Salary {get; set;}
        }


        [Glued]
        [LifeCycle(ServerInstanceMode.AutoConstructedStateful)]
        public interface ITestContractB
        {
            void SetPersonOfTheDay(PersonData person);
            PersonData GetPersonOfTheDay();

            //these groupof method tests overloaded calls
            string GetName();
            string GetName(int id);
            string GetName(int id, DateTime? startDate);
            
            [ArgsMarshalling(typeof(RequestMsg_ITestContractB_GetNameMar))]
            string GetNameMar(int id);

            //overloaded calls returning complex ref type with generic sig
            List<PersonData> GetPersonalData(int[] ids);
            List<PersonData> GetPersonalData(int[] ids, bool onlyCertified, decimal maxSalary);

            //more complex overloaded convoluted type
            Dictionary<DateTime?, List<PersonData>> GetDailyStatuses(int count);

            [Destructor]
            string SummarizeAndFinish();
        }

            public sealed class RequestMsg_ITestContractB_GetNameMar : RequestMsg
            {
                public RequestMsg_ITestContractB_GetNameMar(MethodInfo method, Guid? instance) : base(method, instance){}
                public RequestMsg_ITestContractB_GetNameMar(TypeSpec contract, MethodSpec method, bool oneWay, Guid? instance) : base(contract, method, oneWay, instance){}

                public int MethodArg_0_id;
            }

        [ThreadSafe] //<------------- This isntance will NOT be locked by Glue runtime
        public class TestServerB_ThreadSafe : ITestContractB
        {
            private PersonData m_PersonOfTheDay;
            
            public void SetPersonOfTheDay(PersonData person)
            {
                m_PersonOfTheDay = person;
            }

            public PersonData GetPersonOfTheDay()
            {
                return m_PersonOfTheDay;
            }


            public string GetName()
            {
                return "Felix";
            }

            public string GetName(int id)
            {
                return "Felix{0}".Args(id);
            }

            public string GetNameMar(int id)
            {
                return "Felix{0}".Args(id);
            }

            public string GetName(int id, DateTime? startDate)
            {
                return "Felix{0}{1}".Args(id, startDate ?? new DateTime(1980,1,1, 14,00,00));
            }

            public List<PersonData> GetPersonalData(int[] ids)
            {
                var result = new List<PersonData>();

                if (ids!=null)
                  foreach(var id in ids)
                  {
                    result.Add( new PersonData { ID = (ulong)id, FirstName = "Oleg{0}".Args(id), LastName = "Popov{0}".Args(id) } );
                  }

                return result;
            }

            public List<PersonData> GetPersonalData(int[] ids, bool onlyCertified, decimal maxSalary)
            {
                var result = new List<PersonData>();

                if (ids!=null)
                  foreach(var id in ids)
                  {
                    result.Add( new PersonData { ID = (ulong)id, FirstName = "Oleg{0}".Args(id), LastName = "Popov{0}".Args(id), Certified = onlyCertified, Salary = maxSalary } );
                  }

                return result;
            }

            //NOTE to Serge: this test allocates much larger response that fails to come back
            //via ASYNC binding
            public Dictionary<DateTime?, List<PersonData>> GetDailyStatuses(int count)
            {
                var result = new Dictionary<DateTime?, List<PersonData>>();
                
                for(int i=0; i<count; i++)
                {
                    var dt = new DateTime(1980,1,1).AddSeconds(i);
                    var lst = new List<PersonData>();

                    for(var j=0; j<100; j++)
                    {
                        lst.Add( new PersonData { ID = (ulong)j, FirstName = "Oleg{0}".Args(j), LastName = "Popov{0}".Args(j), Certified = true, Salary = 1000*j } );
                    }
                    result[dt] = lst;
                }
                return result;
            }


            public string SummarizeAndFinish()
            {
                var result = "That is all!";

                if (m_PersonOfTheDay==null)
                {
                    result+= " but no person of the day was set";
                }
                else
                {
                    result+= " for the person " + m_PersonOfTheDay.LastName;
                }

                return result;
            }

           
        }

        //this isntance WILL be locked by Glue runtime because it is not marked as [ThreadSafe]
        public class TestServerB_NotThreadSafe : ITestContractB
        {
            private PersonData m_PersonOfTheDay;
            
            public void SetPersonOfTheDay(PersonData person)
            {
                m_PersonOfTheDay = person;
            }

            public PersonData GetPersonOfTheDay()
            {
                return m_PersonOfTheDay;
            }


            public string GetName()
            {
                return "Felix";
            }

            public string GetName(int id)
            {
                return "Felix{0}".Args(id);
            }

            public string GetNameMar(int id)
            {
                return "Felix{0}".Args(id);
            }

            public string GetName(int id, DateTime? startDate)
            {
                return "Felix{0}{1}".Args(id, startDate ?? new DateTime(1980,1,1, 14,00,00));
            }

            public List<PersonData> GetPersonalData(int[] ids)
            {
                var result = new List<PersonData>();

                if (ids!=null)
                  foreach(var id in ids)
                  {
                    result.Add( new PersonData { ID = (ulong)id, FirstName = "Oleg{0}".Args(id), LastName = "Popov{0}".Args(id) } );
                  }

                return result;
            }

            public List<PersonData> GetPersonalData(int[] ids, bool onlyCertified, decimal maxSalary)
            {
                var result = new List<PersonData>();

                if (ids!=null)
                  foreach(var id in ids)
                  {
                    result.Add( new PersonData { ID = (ulong)id, FirstName = "Oleg{0}".Args(id), LastName = "Popov{0}".Args(id), Certified = onlyCertified, Salary = maxSalary } );
                  }

                return result;
            }

            public Dictionary<DateTime?, List<PersonData>> GetDailyStatuses(int count)
            {
                var result = new Dictionary<DateTime?, List<PersonData>>();
                
                for(int i=0; i<count; i++)
                {
                    var dt = new DateTime(1980,1,1).AddSeconds(i);
                    var lst = new List<PersonData>();

                    for(var j=0; j<100; j++)
                    {
                        lst.Add( new PersonData { ID = (ulong)j, FirstName = "Oleg{0}".Args(j), LastName = "Popov{0}".Args(j), Certified = true, Salary = 1000*j } );
                    }
                    result[dt] = lst;
                }
                return result;
            }


            public string SummarizeAndFinish()
            {
                var result = "That is all!";

                if (m_PersonOfTheDay==null)
                {
                    result+= " but no person of the day was set";
                }
                else
                {
                    result+= " for the person " + m_PersonOfTheDay.LastName;
                }

                return result;
            }

           
        }

 //=====================================================================================================================================================
  ///<summary>
  /// Client for glued contract NFX.NUnit.Glue.ITestContractB server.
  /// Each contract method has synchronous and asynchronous versions, the later denoted by 'Async_' prefix.
  /// May inject client-level inspectors here like so:
  ///   client.MsgInspectors.Register( new YOUR_CLIENT_INSPECTOR_TYPE());
  ///</summary>
  public class TestContractBClient : ClientEndPoint, NFX.NUnit.Glue.ITestContractB
  {

  #region Static Members

     private static TypeSpec s_ts_CONTRACT;
     private static MethodSpec @s_ms_SetPersonOfTheDay_0;
     private static MethodSpec @s_ms_GetPersonOfTheDay_1;
     private static MethodSpec @s_ms_GetName_2;
     private static MethodSpec @s_ms_GetName_3;
     private static MethodSpec @s_ms_GetName_4;
     private static MethodSpec @s_ms_GetNameMar_100;
     private static MethodSpec @s_ms_GetPersonalData_5;
     private static MethodSpec @s_ms_GetPersonalData_6;
     private static MethodSpec @s_ms_GetDailyStatuses_7;
     private static MethodSpec @s_ms_SummarizeAndFinish_8;

     //static .ctor
     static TestContractBClient()
     {
         var t = typeof(NFX.NUnit.Glue.ITestContractB);
         s_ts_CONTRACT = new TypeSpec(t);
         @s_ms_SetPersonOfTheDay_0 = new MethodSpec(t.GetMethod("SetPersonOfTheDay", new Type[]{ typeof(@NFX.@NUnit.@Glue.@PersonData) }));
         @s_ms_GetPersonOfTheDay_1 = new MethodSpec(t.GetMethod("GetPersonOfTheDay", new Type[]{  }));
         @s_ms_GetName_2 = new MethodSpec(t.GetMethod("GetName", new Type[]{  }));
         @s_ms_GetName_3 = new MethodSpec(t.GetMethod("GetName", new Type[]{ typeof(@System.@Int32) }));
         @s_ms_GetName_4 = new MethodSpec(t.GetMethod("GetName", new Type[]{ typeof(@System.@Int32), typeof(@System.@Nullable<@System.@DateTime>) }));
         @s_ms_GetNameMar_100 = new MethodSpec(t.GetMethod("GetNameMar", new Type[]{ typeof(@System.@Int32) }));
         @s_ms_GetPersonalData_5 = new MethodSpec(t.GetMethod("GetPersonalData", new Type[]{ typeof(@System.@Int32[]) }));
         @s_ms_GetPersonalData_6 = new MethodSpec(t.GetMethod("GetPersonalData", new Type[]{ typeof(@System.@Int32[]), typeof(@System.@Boolean), typeof(@System.@Decimal) }));
         @s_ms_GetDailyStatuses_7 = new MethodSpec(t.GetMethod("GetDailyStatuses", new Type[]{ typeof(@System.@Int32) }));
         @s_ms_SummarizeAndFinish_8 = new MethodSpec(t.GetMethod("SummarizeAndFinish", new Type[]{  }));
     }
  #endregion

  #region .ctor
     public TestContractBClient(string node, Binding binding = null) : base(node, binding) { ctor(); }
     public TestContractBClient(Node node, Binding binding = null) : base(node, binding) { ctor(); }
     public TestContractBClient(IGlue glue, string node, Binding binding = null) : base(glue, node, binding) { ctor(); }
     public TestContractBClient(IGlue glue, Node node, Binding binding = null) : base(glue, node, binding) { ctor(); }

     //common instance .ctor body
     private void ctor()
     {

     }

  #endregion

     public override Type Contract
     {
       get { return typeof(NFX.NUnit.Glue.ITestContractB); }
     }



  #region Contract Methods

         ///<summary>
         /// Synchronous invoker for  'NFX.NUnit.Glue.ITestContractB.SetPersonOfTheDay'.
         /// This is a two-way call per contract specification, meaning - the server sends the result back either
         ///  returning no exception or WrappedExceptionData instance.
         /// ClientCallException is thrown if the call could not be placed in the outgoing queue.
         /// RemoteException is thrown if the server generated exception during method execution.
         ///</summary>
         public void @SetPersonOfTheDay(@NFX.@NUnit.@Glue.@PersonData  @person)
         {
            var call = Async_SetPersonOfTheDay(@person);
            call.CheckVoidValue();
         }

         ///<summary>
         /// Asynchronous invoker for  'NFX.NUnit.Glue.ITestContractB.SetPersonOfTheDay'.
         /// This is a two-way call per contract specification, meaning - the server sends the result back either
         ///  returning no exception or WrappedExceptionData instance.
         /// CallSlot is returned that can be queried for CallStatus, ResponseMsg and result.
         ///</summary>
         public CallSlot Async_SetPersonOfTheDay(@NFX.@NUnit.@Glue.@PersonData  @person)
         {
            var request = new RequestAnyMsg(s_ts_CONTRACT, @s_ms_SetPersonOfTheDay_0, false, RemoteInstance, new object[]{@person});
            return DispatchCall(request);
         }



         ///<summary>
         /// Synchronous invoker for  'NFX.NUnit.Glue.ITestContractB.GetPersonOfTheDay'.
         /// This is a two-way call per contract specification, meaning - the server sends the result back either
         ///  returning '@NFX.@NUnit.@Glue.@PersonData' or WrappedExceptionData instance.
         /// ClientCallException is thrown if the call could not be placed in the outgoing queue.
         /// RemoteException is thrown if the server generated exception during method execution.
         ///</summary>
         public @NFX.@NUnit.@Glue.@PersonData @GetPersonOfTheDay()
         {
            var call = Async_GetPersonOfTheDay();
            return call.GetValue<@NFX.@NUnit.@Glue.@PersonData>();
         }

         ///<summary>
         /// Asynchronous invoker for  'NFX.NUnit.Glue.ITestContractB.GetPersonOfTheDay'.
         /// This is a two-way call per contract specification, meaning - the server sends the result back either
         ///  returning no exception or WrappedExceptionData instance.
         /// CallSlot is returned that can be queried for CallStatus, ResponseMsg and result.
         ///</summary>
         public CallSlot Async_GetPersonOfTheDay()
         {
            var request = new RequestAnyMsg(s_ts_CONTRACT, @s_ms_GetPersonOfTheDay_1, false, RemoteInstance, new object[]{});
            return DispatchCall(request);
         }



         ///<summary>
         /// Synchronous invoker for  'NFX.NUnit.Glue.ITestContractB.GetName'.
         /// This is a two-way call per contract specification, meaning - the server sends the result back either
         ///  returning '@System.@String' or WrappedExceptionData instance.
         /// ClientCallException is thrown if the call could not be placed in the outgoing queue.
         /// RemoteException is thrown if the server generated exception during method execution.
         ///</summary>
         public @System.@String @GetName()
         {
            var call = Async_GetName();
            return call.GetValue<@System.@String>();
         }

         ///<summary>
         /// Asynchronous invoker for  'NFX.NUnit.Glue.ITestContractB.GetName'.
         /// This is a two-way call per contract specification, meaning - the server sends the result back either
         ///  returning no exception or WrappedExceptionData instance.
         /// CallSlot is returned that can be queried for CallStatus, ResponseMsg and result.
         ///</summary>
         public CallSlot Async_GetName()
         {
            var request = new RequestAnyMsg(s_ts_CONTRACT, @s_ms_GetName_2, false, RemoteInstance, new object[]{});
            return DispatchCall(request);
         }



         ///<summary>
         /// Synchronous invoker for  'NFX.NUnit.Glue.ITestContractB.GetName'.
         /// This is a two-way call per contract specification, meaning - the server sends the result back either
         ///  returning '@System.@String' or WrappedExceptionData instance.
         /// ClientCallException is thrown if the call could not be placed in the outgoing queue.
         /// RemoteException is thrown if the server generated exception during method execution.
         ///</summary>
         public @System.@String @GetName(@System.@Int32  @id)
         {
            var call = Async_GetName(@id);
            return call.GetValue<@System.@String>();
         }

         ///<summary>
         /// Asynchronous invoker for  'NFX.NUnit.Glue.ITestContractB.GetName'.
         /// This is a two-way call per contract specification, meaning - the server sends the result back either
         ///  returning no exception or WrappedExceptionData instance.
         /// CallSlot is returned that can be queried for CallStatus, ResponseMsg and result.
         ///</summary>
         public CallSlot Async_GetName(@System.@Int32  @id)
         {
            var request = new RequestAnyMsg(s_ts_CONTRACT, @s_ms_GetName_3, false, RemoteInstance, new object[]{@id});
            return DispatchCall(request);
         }




          ///<summary>
         /// Synchronous invoker for  'NFX.NUnit.Glue.ITestContractB.GetName'.
         /// This is a two-way call per contract specification, meaning - the server sends the result back either
         ///  returning '@System.@String' or WrappedExceptionData instance.
         /// ClientCallException is thrown if the call could not be placed in the outgoing queue.
         /// RemoteException is thrown if the server generated exception during method execution.
         ///</summary>
         public @System.@String @GetNameMar(@System.@Int32  @id)
         {
            var call = Async_GetNameMar(@id);
            return call.GetValue<@System.@String>();
         }

         ///<summary>
         /// Asynchronous invoker for  'NFX.NUnit.Glue.ITestContractB.GetName'.
         /// This is a two-way call per contract specification, meaning - the server sends the result back either
         ///  returning no exception or WrappedExceptionData instance.
         /// CallSlot is returned that can be queried for CallStatus, ResponseMsg and result.
         ///</summary>
         public CallSlot Async_GetNameMar(@System.@Int32  @id)
         {
            var request = new RequestMsg_ITestContractB_GetNameMar(s_ts_CONTRACT, @s_ms_GetNameMar_100, false, RemoteInstance)
            {
              MethodArg_0_id = id
            };
            return DispatchCall(request);
         }





         ///<summary>
         /// Synchronous invoker for  'NFX.NUnit.Glue.ITestContractB.GetName'.
         /// This is a two-way call per contract specification, meaning - the server sends the result back either
         ///  returning '@System.@String' or WrappedExceptionData instance.
         /// ClientCallException is thrown if the call could not be placed in the outgoing queue.
         /// RemoteException is thrown if the server generated exception during method execution.
         ///</summary>
         public @System.@String @GetName(@System.@Int32  @id, @System.@Nullable<@System.@DateTime>  @startDate)
         {
            var call = Async_GetName(@id, @startDate);
            return call.GetValue<@System.@String>();
         }

         ///<summary>
         /// Asynchronous invoker for  'NFX.NUnit.Glue.ITestContractB.GetName'.
         /// This is a two-way call per contract specification, meaning - the server sends the result back either
         ///  returning no exception or WrappedExceptionData instance.
         /// CallSlot is returned that can be queried for CallStatus, ResponseMsg and result.
         ///</summary>
         public CallSlot Async_GetName(@System.@Int32  @id, @System.@Nullable<@System.@DateTime>  @startDate)
         {
            var request = new RequestAnyMsg(s_ts_CONTRACT, @s_ms_GetName_4, false, RemoteInstance, new object[]{@id, @startDate});
            return DispatchCall(request);
         }



         ///<summary>
         /// Synchronous invoker for  'NFX.NUnit.Glue.ITestContractB.GetPersonalData'.
         /// This is a two-way call per contract specification, meaning - the server sends the result back either
         ///  returning '@System.@Collections.@Generic.@List<@NFX.@NUnit.@Glue.@PersonData>' or WrappedExceptionData instance.
         /// ClientCallException is thrown if the call could not be placed in the outgoing queue.
         /// RemoteException is thrown if the server generated exception during method execution.
         ///</summary>
         public @System.@Collections.@Generic.@List<@NFX.@NUnit.@Glue.@PersonData> @GetPersonalData(@System.@Int32[]  @ids)
         {
            var call = Async_GetPersonalData(@ids);
            return call.GetValue<@System.@Collections.@Generic.@List<@NFX.@NUnit.@Glue.@PersonData>>();
         }

         ///<summary>
         /// Asynchronous invoker for  'NFX.NUnit.Glue.ITestContractB.GetPersonalData'.
         /// This is a two-way call per contract specification, meaning - the server sends the result back either
         ///  returning no exception or WrappedExceptionData instance.
         /// CallSlot is returned that can be queried for CallStatus, ResponseMsg and result.
         ///</summary>
         public CallSlot Async_GetPersonalData(@System.@Int32[]  @ids)
         {
            var request = new RequestAnyMsg(s_ts_CONTRACT, @s_ms_GetPersonalData_5, false, RemoteInstance, new object[]{@ids});
            return DispatchCall(request);
         }



         ///<summary>
         /// Synchronous invoker for  'NFX.NUnit.Glue.ITestContractB.GetPersonalData'.
         /// This is a two-way call per contract specification, meaning - the server sends the result back either
         ///  returning '@System.@Collections.@Generic.@List<@NFX.@NUnit.@Glue.@PersonData>' or WrappedExceptionData instance.
         /// ClientCallException is thrown if the call could not be placed in the outgoing queue.
         /// RemoteException is thrown if the server generated exception during method execution.
         ///</summary>
         public @System.@Collections.@Generic.@List<@NFX.@NUnit.@Glue.@PersonData> @GetPersonalData(@System.@Int32[]  @ids, @System.@Boolean  @onlyCertified, @System.@Decimal  @maxSalary)
         {
            var call = Async_GetPersonalData(@ids, @onlyCertified, @maxSalary);
            return call.GetValue<@System.@Collections.@Generic.@List<@NFX.@NUnit.@Glue.@PersonData>>();
         }

         ///<summary>
         /// Asynchronous invoker for  'NFX.NUnit.Glue.ITestContractB.GetPersonalData'.
         /// This is a two-way call per contract specification, meaning - the server sends the result back either
         ///  returning no exception or WrappedExceptionData instance.
         /// CallSlot is returned that can be queried for CallStatus, ResponseMsg and result.
         ///</summary>
         public CallSlot Async_GetPersonalData(@System.@Int32[]  @ids, @System.@Boolean  @onlyCertified, @System.@Decimal  @maxSalary)
         {
            var request = new RequestAnyMsg(s_ts_CONTRACT, @s_ms_GetPersonalData_6, false, RemoteInstance, new object[]{@ids, @onlyCertified, @maxSalary});
            return DispatchCall(request);
         }



         ///<summary>
         /// Synchronous invoker for  'NFX.NUnit.Glue.ITestContractB.GetDailyStatuses'.
         /// This is a two-way call per contract specification, meaning - the server sends the result back either
         ///  returning '@System.@Collections.@Generic.@Dictionary<@System.@Nullable<@System.@DateTime>, @System.@Collections.@Generic.@List<@NFX.@NUnit.@Glue.@PersonData>>' or WrappedExceptionData instance.
         /// ClientCallException is thrown if the call could not be placed in the outgoing queue.
         /// RemoteException is thrown if the server generated exception during method execution.
         ///</summary>
         public @System.@Collections.@Generic.@Dictionary<@System.@Nullable<@System.@DateTime>, @System.@Collections.@Generic.@List<@NFX.@NUnit.@Glue.@PersonData>> @GetDailyStatuses(@System.@Int32  @count)
         {
            var call = Async_GetDailyStatuses(@count);
            return call.GetValue<@System.@Collections.@Generic.@Dictionary<@System.@Nullable<@System.@DateTime>, @System.@Collections.@Generic.@List<@NFX.@NUnit.@Glue.@PersonData>>>();
         }

         ///<summary>
         /// Asynchronous invoker for  'NFX.NUnit.Glue.ITestContractB.GetDailyStatuses'.
         /// This is a two-way call per contract specification, meaning - the server sends the result back either
         ///  returning no exception or WrappedExceptionData instance.
         /// CallSlot is returned that can be queried for CallStatus, ResponseMsg and result.
         ///</summary>
         public CallSlot Async_GetDailyStatuses(@System.@Int32  @count)
         {
            var request = new RequestAnyMsg(s_ts_CONTRACT, @s_ms_GetDailyStatuses_7, false, RemoteInstance, new object[]{@count});
            return DispatchCall(request);
         }



         ///<summary>
         /// Synchronous invoker for  'NFX.NUnit.Glue.ITestContractB.SummarizeAndFinish'.
         /// This is a two-way call per contract specification, meaning - the server sends the result back either
         ///  returning '@System.@String' or WrappedExceptionData instance.
         /// ClientCallException is thrown if the call could not be placed in the outgoing queue.
         /// RemoteException is thrown if the server generated exception during method execution.
         ///</summary>
         public @System.@String @SummarizeAndFinish()
         {
            var call = Async_SummarizeAndFinish();
            return call.GetValue<@System.@String>();
         }

         ///<summary>
         /// Asynchronous invoker for  'NFX.NUnit.Glue.ITestContractB.SummarizeAndFinish'.
         /// This is a two-way call per contract specification, meaning - the server sends the result back either
         ///  returning no exception or WrappedExceptionData instance.
         /// CallSlot is returned that can be queried for CallStatus, ResponseMsg and result.
         ///</summary>
         public CallSlot Async_SummarizeAndFinish()
         {
            var request = new RequestAnyMsg(s_ts_CONTRACT, @s_ms_SummarizeAndFinish_8, false, RemoteInstance, new object[]{});
            return DispatchCall(request);
         }


  #endregion

  }//class
}//namespace

