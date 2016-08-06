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

using NFX.Environment;

namespace NFX.Glue.Tools
{
    /// <summary>
    /// Generates code from glue contracts
    /// </summary>
    public abstract class GluecCompiler
    {

        protected GluecCompiler(Assembly asm)
        {
           m_Assembly = asm;
        }


        protected Assembly m_Assembly;
        private List<string> m_NamespaceFilter;
        private string m_OutPath;
        private bool m_FilePerContract;


        [Config("$out")]
        public string OutPath
        {
          get { return m_OutPath ?? string.Empty; }
          set { m_OutPath = value;}
        }

        [Config("$fpc", true)]
        public bool FilePerContract
        {
          get { return m_FilePerContract;}
          set { m_FilePerContract = value;}
        }

        [Config("$ns")]
        public string NamespaceFilter
        {
          get { return (m_NamespaceFilter==null || m_NamespaceFilter.Count==0)
                       ? string.Empty
                       : m_NamespaceFilter.Aggregate(new StringBuilder(), (a, s) => a.Append(";" + s), a => a.ToString().Remove(0,1)); }
          set
          {
             if (value==null)
              m_NamespaceFilter = null;
             else
              m_NamespaceFilter = value.Split(';')
                     .Select( ns => ns.Trim())
                     .Where( ns => !string.IsNullOrEmpty(ns)).ToList();
          }
        }


        public IEnumerable<string> Namespaces
        {
          get
          {
            if (m_NamespaceFilter==null || m_NamespaceFilter.Count==0)
             return m_Assembly.GetTypes().Select(t => t.Namespace).Distinct();

            return m_NamespaceFilter;
          }
        }


        public abstract void Compile();

    }
}
