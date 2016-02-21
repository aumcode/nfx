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
using System.Threading.Tasks;

using NFX.Environment;
using NFX.ApplicationModel;
using NFX.ServiceModel;

namespace NFX.Web.EMail
{

  public enum SendPriority { Urgent = 0, Normal=1, BelowNormal=2,        Slowest = BelowNormal} 


  /// <summary>
  /// Describes an entity that can send EMails
  /// </summary>
  public interface IMailer : IApplicationComponent
  {
    void SendMsg(MailMsg msg, SendPriority? handlingPriority = null);
  }

  public interface IMailerImplementation : IMailer, IConfigurable, IService, IApplicationFinishNotifiable 
  {

  }

  /// <summary>
  /// Represents an email msg that needs to be sent
  /// </summary>
  [Serializable]
  public class MailMsg
  {
    public SendPriority Priority { get; set;}
    public string FROMAddress{get;set;}
    public string FROMName{get;set;}

    public string TOAddress{get;set;}
    public string TOName{get;set;}
    public string CC{get;set;}
    public string BCC{get;set;}

    public string Subject{get;set;}
    
    /// <summary>
    /// Plain/text body
    /// </summary>
    public string Body{get;set;}

    /// <summary>
    /// HTML-formatted body
    /// </summary>
    public string HTMLBody{get; set;}

  }

}
