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
using System.Reflection;


using NFX.Security;
using NFX.Glue;
using NFX.Glue.Protocol;

namespace BusinessLogic
{
    [Serializable]
    public class TextInfoHeader : Header
    {
      public string Text;
      public string Info;
    }
    
    
    [Glued]
    //[SultanPermission( 1000 )]
    [AuthenticationSupport]
    public interface IJokeContract
    {
      [AdHocPermission("/TestPermissions/Space/Flight", "Echo", AccessLevel.VIEW_CHANGE)]
      [SultanPermission(9)]
      string Echo(string text);
      
      /// <summary>
      /// Echo without permissions
      /// </summary>
      string UnsecureEcho(string text);

      
      [ArgsMarshalling(typeof(RequestMsg_IJokeContract_UnsecEchoMar))]
      string UnsecEchoMar(string text);


      string SimpleWorkAny(string s, int i1, int i2, bool b, double d);

      [ArgsMarshalling(typeof(RequestMsg_IJokeContract_SimpleWorkMar))]
      string SimpleWorkMar(string s, int i1, int i2, bool b, double d);


      /// <summary>
      /// Emulates database work that returns dummy data and pauses for some interval emulating blocking backend access
      /// </summary>
      object DBWork(string id, int recCount, int waitMs);

      [OneWay] 
      //[SultanPermission(AccessLevel.VIEW)]
      void Notify(string text);

      object ObjectWork(object dummy);
    }

    public sealed class RequestMsg_IJokeContract_UnsecEchoMar : RequestMsg
    {
        public RequestMsg_IJokeContract_UnsecEchoMar(MethodInfo method, Guid? instance) : base(method, instance){}
        public RequestMsg_IJokeContract_UnsecEchoMar(TypeSpec contract, MethodSpec method, bool oneWay, Guid? instance) : base(contract, method, oneWay, instance){}

        public string MethodArg_0_text;
    }

    public sealed class RequestMsg_IJokeContract_SimpleWorkMar : RequestMsg
    {
        public RequestMsg_IJokeContract_SimpleWorkMar(MethodInfo method, Guid? instance) : base(method, instance){}
        public RequestMsg_IJokeContract_SimpleWorkMar(TypeSpec contract, MethodSpec method, bool oneWay, Guid? instance) : base(contract, method, oneWay, instance){}

        public string MethodArg_0_s;
        public int    MethodArg_1_i1;
        public int    MethodArg_2_i2;
        public bool   MethodArg_3_b;
        public double MethodArg_4_d;
    }



    [Glued]
    [LifeCycle(ServerInstanceMode.Stateful, 20000)]
    [AuthenticationSupport]
    public interface IJokeCalculator
    {
       [Constructor]
       void Init(int value);
       
       [SultanPermission( 250 )]
       int Add(int value);
       int Sub(int value);

       [Destructor]
       int Done();
      
    }




}
