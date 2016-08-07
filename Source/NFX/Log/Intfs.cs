/*<FILE_LICENSE>
* NFX (.NET Framework Extension) Unistack Library
* Copyright 2003-2014 Dmitriy Khmaladze, IT Adapter Inc / 2015-2016 Aum Code LLC
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

using NFX.Time;
using NFX.Environment;
using NFX.ApplicationModel;
using NFX.Instrumentation;

namespace NFX.Log
{
      /// <summary>
      /// Describes entity capable of being written log information to
      /// </summary>
      public interface ILog : IApplicationComponent, ILocalizedTimeProvider
      {
         void Write(Message msg);
         void Write(Message msg, bool urgent);
         void Write(MessageType type, string text, string topic = null, string from = null);
         void Write(MessageType type, string text, bool urgent, string topic = null, string from = null);


         Message LastWarning { get;}
         Message LastError { get;}
         Message LastCatastrophy { get;}
      }


      /// <summary>
      /// Describes entity capable of being written log information to
      /// </summary>
      public interface ILogImplementation : ILog, IDisposable, IConfigurable, IInstrumentable
      {

      }



}
