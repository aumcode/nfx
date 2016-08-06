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

namespace NFX.DataAccess.Cache
{

    /// <summary>
    /// Provides cache table capacity classifications
    /// </summary>
    public enum TableCapacity
    {
        /// <summary>
        /// 25,111 * 7 = 175,777 items * 64(8/ptr + 40/cache rec + 12/obj over) = 11.25 Mb just for storing empty cache items
        /// </summary>
        Default = 0,

        /// <summary>
        /// 753,001 * 7 = 5,271,007 items * 64(8/ptr + 40/cache rec + 12/obj over) = 337.3 Mb just for storing empty cache items
        /// </summary>
        Large,

        /// <summary>
        /// 3,337,333 * 7 = 23,361,331 items * 64(8/ptr + 40/cache rec + 12/obj over) = 1.39 Gb just for storing empty cache items
        /// </summary>
        XLarge
    }



    /// <summary>
    /// Provides config options for cache tables
    /// </summary>
    public sealed class TableOptions : INamed, IConfigurable
    {
        internal TableOptions()
        {

        }


        public TableOptions(string name, TableCapacity capacity, int lockCount = 0, int maxAgeSec = 0, bool parallelSweep = false)
        {
            var bc = 0;
            var rp = 0;

            switch(capacity)
            {
                case TableCapacity.Large:   { bc =   753001; rp =  7;  break;}
                case TableCapacity.XLarge:  { bc =  3337333; rp =  7;  break;}
                default: break;
            }

            ctor(name, bc, rp, lockCount, maxAgeSec, parallelSweep);
        }

        public TableOptions(string name, int bucketCount, int recPerPage, int lockCount = 0, int maxAgeSec = 0, bool parallelSweep = false)
        {
            ctor(name, bucketCount, recPerPage, lockCount, maxAgeSec, parallelSweep);
        }

        private void ctor(string name, int bucketCount, int recPerPage, int lockCount, int maxAgeSec, bool parallelSweep)
        {
            if (name.IsNullOrWhiteSpace())
                throw new NFXException(StringConsts.ARGUMENT_ERROR + "TableOptions.ctor(name=null|Empty)");

            m_Name = name;
            m_BucketCount = bucketCount;
            m_RecPerPage = recPerPage;
            m_LockCount = lockCount;
            m_MaxAgeSec = maxAgeSec;
            m_ParallelSweep = parallelSweep;
        }


        [Config]private string m_Name;
        [Config]private int    m_BucketCount;
        [Config]private int    m_RecPerPage;
        [Config]private int    m_LockCount;
        [Config]private int    m_MaxAgeSec;
        [Config]private bool   m_ParallelSweep;


        public string Name       { get{ return m_Name;} }
        public int    BucketCount{ get{ return m_BucketCount;} }
        public int    RecPerPage { get{ return m_RecPerPage;} }
        public int    LockCount  { get{ return m_LockCount;} }
        public int    MaxAgeSec  { get{ return m_MaxAgeSec;} }
        public bool   ParallelSweep { get{ return m_ParallelSweep;} }

        void IConfigurable.Configure(IConfigSectionNode node)
        {
            if (node!=null && node.AttrByName(Configuration.CONFIG_NAME_ATTR).Value.IsNullOrWhiteSpace())
                throw new NFXException(StringConsts.ARGUMENT_ERROR + "TableOptions.configure(name=null|Empty)");

            ConfigAttribute.Apply(this, node);
        }
    }
}
