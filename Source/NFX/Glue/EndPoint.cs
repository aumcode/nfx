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
using NFX.Environment;

namespace NFX.Glue
{

    /// <summary>
    /// Abstarction of server and client endpoints. And endpoint is a logically-connected entity per: ABC rule - Address/Binding/Contract(s)
    /// </summary>
    public abstract class EndPoint : DisposableObject
    {

        protected EndPoint(IGlue glue) //used by conf
        {
           if (glue == null)
                glue = ExecutionContext.Application.Glue;

            m_Glue = (IGlueImplementation)glue;
        }

        protected EndPoint(IGlue glue, Node node, Binding binding)
        {
            if (glue == null)
                glue = ExecutionContext.Application.Glue;

            m_Glue = (IGlueImplementation)glue;

            m_Node = node;
            m_Binding = binding ?? m_Glue.GetNodeBinding(node);
        }

        protected IGlueImplementation m_Glue;



        protected Node m_Node;    //[A]ddress
        protected Binding m_Binding; //[B]inding




        /// <summary>
        /// References glue that this endpoint works under
        /// </summary>
        public IGlue Glue { get { return m_Glue; } }


        /// <summary>
        /// Returns a node of this endpoint. "A" component of the "ABC" rule
        /// </summary>
        public Node Node { get { return m_Node; } }

        /// <summary>
        /// Returns a binding of this endpoint. "B" component of the "ABC" rule
        /// </summary>
        public Binding Binding { get { return m_Binding; } }


    }



}
