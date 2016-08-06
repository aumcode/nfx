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

namespace NFX.CodeAnalysis.JSON
{
    /// <summary>
    /// Represents JSONDataParser result
    /// </summary>
    public sealed class JSONData : ObjectResultAnalysisContext<object>
    {

        public JSONData(IAnalysisContext context = null, MessageList messages = null, bool throwErrors = false):
                                               base(context, messages ?? new MessageList(), throwErrors)
        {

        }


        public override Language Language
        {
            get { return JSONLanguage.Instance; }
        }

        public override string MessageCodeToString(int code)
        {
            return ((JSONMsgCode)code).ToString();
        }

        internal void setData(object data)
        {
            m_ResultObject = data;
        }
    }
}
