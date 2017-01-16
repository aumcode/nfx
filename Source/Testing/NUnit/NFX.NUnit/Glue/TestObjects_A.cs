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


using NFX.Glue;
using NFX.Glue.Protocol;

namespace NFX.NUnit.Glue
{
   
        [Glued]
        public interface ITestContractA
        {
                     string Method1(int x);
            [OneWay] void Method2(int x);

                     int Sleeper(int sleepMs);
        }

        public class TestServerA : ITestContractA
        {
            internal static int s_Accumulator;


            public string Method1(int x)
            {
                s_Accumulator += x;
                return s_Accumulator.ToString();  
            }

            public void Method2(int x)
            {
                s_Accumulator += x;
            }

            public int Sleeper(int sleepMs)
            {
               System.Threading.Thread.Sleep(sleepMs);
               return sleepMs;
            }
        }

 
  ///<summary>
  /// Client for glued contract NFX.NUnit.Glue.ITestContractA server.
  /// Each contract method has synchronous and asynchronous versions, the later denoted by 'Async_' prefix.
  /// May inject client-level inspectors here like so:
  ///   client.MsgInspectors.Register( new YOUR_CLIENT_INSPECTOR_TYPE());
  ///</summary>
  public class TestContractAClient : ClientEndPoint, NFX.NUnit.Glue.ITestContractA
  {

  #region Static Members

     private static TypeSpec s_ts_CONTRACT;
     private static MethodSpec @s_ms_Method1_0;
     private static MethodSpec @s_ms_Method2_1;
     private static MethodSpec @s_ms_Sleeper_2;

     //static .ctor
     static TestContractAClient()
     {
         var t = typeof(NFX.NUnit.Glue.ITestContractA);
         s_ts_CONTRACT = new TypeSpec(t);
         @s_ms_Method1_0 = new MethodSpec(t.GetMethod("Method1", new Type[]{ typeof(@System.@Int32) }));
         @s_ms_Method2_1 = new MethodSpec(t.GetMethod("Method2", new Type[]{ typeof(@System.@Int32) }));
         @s_ms_Sleeper_2 = new MethodSpec(t.GetMethod("Sleeper", new Type[]{ typeof(@System.@Int32) }));
     }
  #endregion

  #region .ctor
     public TestContractAClient(string node, Binding binding = null) : base(node, binding) { ctor(); }
     public TestContractAClient(Node node, Binding binding = null) : base(node, binding) { ctor(); }
     public TestContractAClient(IGlue glue, string node, Binding binding = null) : base(glue, node, binding) { ctor(); }
     public TestContractAClient(IGlue glue, Node node, Binding binding = null) : base(glue, node, binding) { ctor(); }

     //common instance .ctor body
     private void ctor()
     {

     }

  #endregion

     public override Type Contract
     {
       get { return typeof(NFX.NUnit.Glue.ITestContractA); }
     }



  #region Contract Methods

         ///<summary>
         /// Synchronous invoker for  'NFX.NUnit.Glue.ITestContractA.Method1'.
         /// This is a two-way call per contract specification, meaning - the server sends the result back either
         ///  returning '@System.@String' or WrappedExceptionData instance.
         /// ClientCallException is thrown if the call could not be placed in the outgoing queue.
         /// RemoteException is thrown if the server generated exception during method execution.
         ///</summary>
         public @System.@String @Method1(@System.@Int32  @x)
         {
            var call = Async_Method1(@x);
            return call.GetValue<@System.@String>();
         }

         ///<summary>
         /// Asynchronous invoker for  'NFX.NUnit.Glue.ITestContractA.Method1'.
         /// This is a two-way call per contract specification, meaning - the server sends the result back either
         ///  returning no exception or WrappedExceptionData instance.
         /// CallSlot is returned that can be queried for CallStatus, ResponseMsg and result.
         ///</summary>
         public CallSlot Async_Method1(@System.@Int32  @x)
         {
            var request = new RequestAnyMsg(s_ts_CONTRACT, @s_ms_Method1_0, false, RemoteInstance, new object[]{@x});
            return DispatchCall(request);
         }



         ///<summary>
         /// Synchronous invoker for  'NFX.NUnit.Glue.ITestContractA.Method2'.
         /// This is a one-way call per contract specification, meaning - the server sends no acknowledgement of this call receipt and
         /// there is no result that server could return back to the caller.
         /// ClientCallException is thrown if the call could not be placed in the outgoing queue.
         ///</summary>
         public void @Method2(@System.@Int32  @x)
         {
            var call = Async_Method2(@x);
            if (call.CallStatus != CallStatus.Dispatched)
                throw new ClientCallException(call.CallStatus, "Call failed: 'TestContractAClient.Method2'");
         }

         ///<summary>
         /// Asynchronous invoker for  'NFX.NUnit.Glue.ITestContractA.Method2'.
         /// This is a one-way call per contract specification, meaning - the server sends no acknowledgement of this call receipt and
         /// there is no result that server could return back to the caller.
         /// CallSlot is returned that can be queried for CallStatus, ResponseMsg.
         ///</summary>
         public CallSlot Async_Method2(@System.@Int32  @x)
         {
            var request = new RequestAnyMsg(s_ts_CONTRACT, @s_ms_Method2_1, true, RemoteInstance, new object[]{@x});
            return DispatchCall(request);
         }


         public int @Sleeper(@System.@Int32  @sleepMs)
         {
            var call = Async_Sleeper(@sleepMs);
            return call.GetValue<@System.@Int32>();
         }

         public CallSlot Async_Sleeper(@System.@Int32  @sleepMs)
         {
            var request = new RequestAnyMsg(s_ts_CONTRACT, @s_ms_Sleeper_2, false, RemoteInstance, new object[]{@sleepMs});
            return DispatchCall(request);
         }


  #endregion

  }//class

        
}
