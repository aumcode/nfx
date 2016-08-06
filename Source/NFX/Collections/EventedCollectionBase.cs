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

namespace NFX.Collections
{


    /// <summary>
    /// Specifies the phase of the event i.e. before/after
    /// </summary>
    public enum EventPhase { Before, After}

    /// <summary>
    /// Event handler for list changes
    /// </summary>
    public delegate bool EventedCollectionGetReadOnlyHandler<TContext>(EventedCollectionBase<TContext> collection);


    /// <summary>
    /// Provides base implementation for some evented collections
    /// </summary>
    public abstract class EventedCollectionBase<TContext>
    {
        protected EventedCollectionBase()
        {

        }

        protected EventedCollectionBase(TContext context, bool contextReadOnly)
        {
            m_ContextReadOnly = contextReadOnly;
            m_Context = context;
        }


        private bool m_ContextReadOnly;
        private TContext m_Context;



        /// <summary>
        /// Returns true to indicate that Context property can not be set (was injected in .ctor only
        /// </summary>
        public bool ContextReadOnly
        {
            get { return m_ContextReadOnly;}
        }

        /// <summary>
        /// Context that this structure works in
        /// </summary>
        public TContext Context
        {
            get { return m_Context; }
            set
            {
                if (m_ContextReadOnly)
                 throw new NFXException(StringConsts.INVALID_OPERATION_ERROR + this.GetType().FullName+".Context.set()");
                m_Context = value;
            }
        }

        [field:NonSerialized] public EventedCollectionGetReadOnlyHandler<TContext> GetReadOnlyEvent;

        /// <summary>
        /// Indicates whether collection can be modified
        /// </summary>
        public bool IsReadOnly
        {
            get { return GetReadOnlyEvent!=null ? GetReadOnlyEvent(this) : true; }
        }


        public void CheckReadOnly()
        {
           if (IsReadOnly)
            throw new NFXException(StringConsts.READONLY_COLLECTION_MUTATION_ERROR + this.GetType().FullName+".CheckReadOnly()");
        }

    }
}
