/*<FILE_LICENSE>
* NFX (.NET Framework Extension) Unistack Library
* Copyright 2003-2017 ITAdapter Corp. Inc.
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
using BusinessLogic;

namespace TestServer.Glue
{
    /// <summary>
    /// Provides IJokeContract implementation used for testing
    /// </summary>
    public class JokeServer : IJokeContract
    {
        public string Echo(string text)
        {
          
          //StringBuilder sb = null;
          //TextInfoHeader th = null;
          //foreach(var h in NFX.Glue.ServerCallContext.Request.Headers)
          //{
          //  if (sb==null) sb = new StringBuilder();
          //  if (h is TextInfoHeader) th = (TextInfoHeader)h;
          //  sb.Append(h.ToString());
          //  sb.Append(" ");
          //}
         
         
          //ExecutionContext.Application.Log.Write(MessageType.Info, "MarazmServer.Echo. Headers: " + sb.ToString(), from: text + (th==null?string.Empty : th.Text + th.Info));
          
          
          //NFX.Glue.ServerCallContext.ResponseHeaders.Add( new MyHeader());
          return "Server echoed " + text;// + "' on "+App.LocalizedTime.ToString(); 
        }

        public string UnsecureEcho(string text)
        {
          return "Server echoed " + text;
        }

        public string UnsecEchoMar(string text)
        {
          return "Server echoed " + text;
        }

        public object DBWork(string id, int recCount, int waitMs)
        {
          var result = new SimplePersonRow[recCount];// new List<SimplePersonRow>();

          if (recCount<0) recCount = 0;
          for(var i=0; i<recCount; i++)
          {
           result[i] = new SimplePersonRow
            {
              ID = new NFX.DataAccess.Distributed.GDID(0, (ulong) i),
              Age = i,
               Name = "abuxazn"+i,// NFX.Parsing.NaturalTextGenerator.Generate(10),
                Date = DateTime.Now,
                 Bool1 = i % 18 ==0,
                Str1 = "jsaudhasuhdasiuhduhd", // NFX.Parsing.NaturalTextGenerator.Generate(25),
                Str2 = "dsadas sdas ",//NFX.Parsing.NaturalTextGenerator.Generate(25),
                Salary = 1234d * i
            };
          }

          //emulate DB Access
          if (waitMs>0)
           System.Threading.Thread.Sleep(ExternalRandomGenerator.Instance.NextScaledRandomInteger(waitMs, waitMs));

          return result;
        }
        //{
        //  var result = new List<object[]>();

        //  if (recCount<0) recCount = 0;
        //  for(var i=0; i<recCount; i++)
        //  {
        //    var rec = new object[8];
        //    rec[0] = id;
        //    rec[1] = i;
        //    rec[2] = NFX.Parsing.NaturalTextGenerator.Generate(10);
        //    rec[3] = i % 18 ==0;
        //    rec[4] = NFX.Parsing.NaturalTextGenerator.Generate(25);
        //    rec[5] = NFX.Parsing.NaturalTextGenerator.Generate(25);
        //    rec[6] = DateTime.Now;
        //    rec[7] = 1234d * i;
        //    result.Add( rec );
        //  }

        //  //emulate DB Access
        //  if (waitMs>0)
        //   System.Threading.Thread.Sleep(ExternalRandomGenerator.Instance.NextScaledRandomInteger(waitMs, waitMs));

        //  return result;
        //}


        public void Notify(string text)
        {
            NotifyEvent.Happened();
            //ExecutionContext.Application.Log.Write(MessageType.Info, from: "MarazmServer.Notify", text: text);  
        }

        public object ObjectWork(object dummy)
        {
            if (dummy is string)
                dummy = dummy.ToString() + " server work done on " + App.LocalizedTime.ToString();
            return dummy;
        }


        public string SimpleWorkAny(string s, int i1, int i2, bool b, double d)
        {
          return s + i1.ToString() + i2.ToString() + b.ToString() + d.ToString();
        }

        public string SimpleWorkMar(string s, int i1, int i2, bool b, double d)
        {
          return s + i1.ToString() + i2.ToString() + b.ToString() + d.ToString();
        }

    }
}
