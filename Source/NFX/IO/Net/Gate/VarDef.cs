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

using NFX.Environment;

namespace NFX.IO.Net.Gate
{

    /// <summary>
    /// Provides variable definition - the name and parameters how fast a variable decays - loses its value towards 0 when it gets deleted
    /// </summary>
    public class VarDef : INamed
    {
         public const int DEFAULT_DECAY_BY = 1;
         public const int DEFAULT_INTERVAL_SEC = 10;


         public VarDef(string name)
         {
            if (name.IsNullOrWhiteSpace())
              throw new NetGateException(StringConsts.NETGATE_VARDEF_NAME_EMPTY_CTOR_ERROR);
            m_Name = name;
         }

         public VarDef(IConfigSectionNode node)
                  : this(node.NonNull(text: "VarDef.ctor(node==null)").AttrByName(Configuration.CONFIG_NAME_ATTR).Value)
         {
           ConfigAttribute.Apply(this, node);
         }



         [Config]
         private string m_Name;
         private int m_DecayBy     = DEFAULT_DECAY_BY;
         private int m_IntervalSec = DEFAULT_INTERVAL_SEC;


         public string Name { get{ return m_Name;}}

         [Config]
         public int DecayBy
         {
           get{ return m_DecayBy;}
           set
           {
              value = Math.Abs(value);
              m_DecayBy = value<1 ? 1 : value;
           }
         }

         [Config(Default=DEFAULT_INTERVAL_SEC)]
         public int IntervalSec
         {
            get{ return m_IntervalSec;}
            set
            {
              m_IntervalSec = value<1 ? 1 : value;
            }
         }
    }
}
