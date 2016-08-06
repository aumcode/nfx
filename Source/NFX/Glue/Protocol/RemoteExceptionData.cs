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

namespace NFX.Glue.Protocol
{
    /// <summary>
    /// Marshalls exception details
    /// </summary>
    [Serializable]
    public sealed class RemoteExceptionData
    {

       /// <summary>
       /// Initializes instance form local exception
       /// </summary>
       public RemoteExceptionData(Exception error)
       {
         var tp = error.GetType();
         m_TypeName = tp.FullName;
         m_Message = error.Message;
         if (error is NFXException)
          m_Code = ((NFXException)error).Code;

         m_ApplicationName = ExecutionContext.Application.Name;

         m_Source = error.Source;
         m_StackTrace = error.StackTrace;

         if (error.InnerException!=null)
          m_InnerException = new RemoteExceptionData(error.InnerException);
       }


       private string m_TypeName;
       private string m_Message;
       private int m_Code;
       private string m_ApplicationName;
       private string m_Source;
       private string m_StackTrace;
       private RemoteExceptionData m_InnerException;

       /// <summary>
       /// Returns the name of remote exception type
       /// </summary>
       public string TypeName { get {return m_TypeName ?? CoreConsts.UNKNOWN;} }

       /// <summary>
       /// Returns the message of remote exception
       /// </summary>
       public string Message { get {return m_Message ?? string.Empty;} }

       /// <summary>
       /// Returns the code of remote NFX exception
       /// </summary>
       public int Code { get {return m_Code;} }


       /// <summary>
       /// Name of the object that caused the error
       /// </summary>
       public string Source { get {return m_Source ?? string.Empty;} }

       /// <summary>
       /// Returns stack trace
       /// </summary>
       public string StackTrace { get {return m_StackTrace ?? string.Empty;} }


       /// <summary>
       /// Returns the name of remote application
       /// </summary>
       public string ApplicationName { get {return m_ApplicationName ?? CoreConsts.UNKNOWN;} }


       /// <summary>
       /// Returns the inner remote exception if any
       /// </summary>
       public RemoteExceptionData InnerException { get {return m_InnerException;} }


       public override string ToString()
       {
           return string.Format("[{0}:{1}:{2}] {3}", TypeName, Code, ApplicationName, Message);
       }

    }
}
