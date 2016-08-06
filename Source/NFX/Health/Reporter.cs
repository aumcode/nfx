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

namespace NFX.Health
{
    /// <summary>
    /// Base for all reporters - entities that output check list state into TextWriter
    /// </summary>
    public abstract class Reporter
    {
       protected Reporter(CheckList list)
       {
         m_List = list;
       }

       private CheckList m_List;


       public CheckList CheckList
       {
         get { return m_List; }
       }


       public abstract void Report(TextWriter writer);

    }
}
