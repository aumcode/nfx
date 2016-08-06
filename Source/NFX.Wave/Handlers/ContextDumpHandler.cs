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
using System.IO;

using NFX.Web;
using NFX.Environment;

namespace NFX.Wave.Handlers
{
  /// <summary>
  /// Dumps WorkContext status - used for debugging purposes
  /// </summary>
  public class ContextDumpHandler : WorkHandler
  {
     protected ContextDumpHandler(WorkDispatcher dispatcher, string name, int order, WorkMatch match) : base(dispatcher, name, order, match){}
     protected ContextDumpHandler(WorkDispatcher dispatcher, IConfigSectionNode confNode) : base(dispatcher, confNode){}

     protected override void DoHandleWork(WorkContext work)
     {
        var dump = new{
          About = work.About,

          Server = new
          {
             Name = work.Server.Name,
             Type = work.Server.GetType().FullName,
             LocalTime = work.Server.LocalizedTime,
             Environment = work.Server.EnvironmentName,
             KernelHttpQueueLimit = work.Server.KernelHttpQueueLimit,
             ParallelAccepts = work.Server.ParallelAccepts,
             ParallelWorks = work.Server.ParallelWorks,
             Prefixes = work.Server.Prefixes,
             Dispatcher = work.Server.Dispatcher.Name,
             ShowDumpMatches = work.Server.ShowDumpMatches.OrderedValues.Select(m=>m.Name).ToList(),
             LogMatches = work.Server.LogMatches.OrderedValues.Select(m=>m.Name).ToList(),
          },//server
          Request = new
          {
             AcceptTypes = work.Request.AcceptTypes,
             ContentEncoding = work.Request.ContentEncoding!= null?work.Request.ContentEncoding.EncodingName : SysConsts.NULL_STRING,
             ContentLength = work.Request.ContentLength64,
             ContentType  = work.Request.ContentType,
             Cookies = work.Request.Cookies.Count,
             HasEntityBody = work.Request.HasEntityBody,
             Headers = collToList( work.Request.Headers),
             Method = work.Request.HttpMethod,
             IsAuthenticated = work.Request.IsAuthenticated,
             IsLocal = work.Request.IsLocal,
             IsSecure = work.Request.IsSecureConnection,
             KeepAlive = work.Request.KeepAlive,
             LocalEndPoint = work.Request.LocalEndPoint.ToString(),
             ProtoVersion = work.Request.ProtocolVersion.ToString(),
             Query = collToList(work.Request.QueryString),
             RawURL = work.Request.RawUrl,
             RemoteEndPoint = work.Request.RemoteEndPoint.ToString(),
             UserAgent = work.Request.UserAgent,
             UserHostAddress = work.Request.UserHostAddress,
             UserHostName = work.Request.UserHostName,
             UserLanguages = work.Request.UserLanguages,
          },
          Dispatcher = new
          {
             Filters = work.Server.Dispatcher.Filters.Select(f=>f.ToString()),
             Handlers = work.Server.Dispatcher.Handlers.Select(h=>h.ToString()),

          },

          Session = work.Session==null ? SysConsts.NULL_STRING : work.Session.GetType().FullName,

          Handled = work.Handled,
          Aborted = work.Aborted,
          NoDefaultAutoClose = work.NoDefaultAutoClose,
          Items = work.Items.Select(kvp=>kvp.Key),
          GeoEntity = work.GeoEntity!=null?work.GeoEntity.ToString() : SysConsts.NULL_STRING
        };



        work.Response.ContentType = ContentType.JSON;
        work.Response.WriteJSON(dump, Serialization.JSON.JSONWritingOptions.PrettyPrint);
     }

     private List<string> collToList(System.Collections.Specialized.NameValueCollection collection)
     {
        var result = new List<string>();
        for(var i=0; i<collection.Count; i++)
          result.Add("{0} = {1}".Args(collection.GetKey(i), collection[i]));

        return result;
     }

  }
}
