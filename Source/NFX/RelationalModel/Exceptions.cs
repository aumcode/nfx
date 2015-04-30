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
using System.Runtime.Serialization;
using System.Text;

namespace NFX.RelationalModel
{
      /// <summary>
      /// Base exception thrown by the Relational-* framework
      /// </summary>
      [Serializable]
      public class RelationalException : NFXException
      {
        public RelationalException()
        {
        }

        public RelationalException(string message)
          : base(message)
        {
        }

        public RelationalException(string message, Exception inner)
          : base(message, inner)
        {
        }

        protected RelationalException(SerializationInfo info, StreamingContext context)
          : base(info, context)
        {

        }

      }


      /// <summary>
      /// Thrown by relational schema
      /// </summary>
      [Serializable]
      public class SchemaException : RelationalException
      {

        public SchemaException(string message) : base(message)
        {

        }

        public SchemaException(string message, Exception inner) : base(message, inner)
        {

        }

      }

       /// <summary>
      /// Thrown by relational schema compiler
      /// </summary>
      [Serializable]
      public class CompilerException : RelationalException
      {

        public CompilerException(string message) : base(message)
        {

        }

        public CompilerException(string message, Exception inner) : base(message, inner)
        {

        }

      }


      /// <summary>
      /// Thrown by relational schema compiler while processing the source schema
      /// </summary>
      [Serializable]
      public class SchemaCompilationException : SchemaException
      {

        /// <summary>
        /// Returns node that issued compilation error
        /// </summary>
        public readonly string NodePath;


        public override string Message
        {
            get
            {
                return "Exception {0} at node '{1}'".Args(base.Message, NodePath ?? CoreConsts.UNKNOWN);
            }
        }

        public SchemaCompilationException(string nodePath, string message) : base(message)
        {
            NodePath = nodePath;
        }

        public SchemaCompilationException(string nodePath, string message, Exception inner) : base(message, inner)
        {
            NodePath = nodePath;
        }

      }



}
