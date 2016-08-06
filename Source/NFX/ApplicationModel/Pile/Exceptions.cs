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


/* NFX by ITAdapter
 * Originated: 2008.03
 * Revision: NFX 1.0  2011.02.06
 */
using System;
using System.Runtime.Serialization;

namespace NFX.ApplicationModel.Pile
{


      /// <summary>
      /// Thrown by pile memory manager
      /// </summary>
      [Serializable]
      public class PileException : NFXException
      {


            public PileException()
            {

            }

            public PileException(string message) : base(message)
            {
            }

            public PileException(string message, Exception inner) : base(message, inner)
            {
            }

            protected PileException(SerializationInfo info, StreamingContext context)
              : base(info, context)
            {
            }
      }


      /// <summary>
      /// Thrown by pile memory manager when a supplied PilePointer is not pointing to a valid buffer
      /// </summary>
      [Serializable]
      public class PileAccessViolationException : PileException
      {


            public PileAccessViolationException()
            {

            }

            public PileAccessViolationException(string message) : base(message)
            {
            }

            public PileAccessViolationException(string message, Exception inner) : base(message, inner)
            {
            }

            protected PileAccessViolationException(SerializationInfo info, StreamingContext context)
              : base(info, context)
            {
            }
      }

      /// <summary>
      /// Thrown by pile memory manager when there is not anough room in the pile to perform the requested operation
      /// </summary>
      [Serializable]
      public class PileOutOfSpaceException : PileException
      {


            public PileOutOfSpaceException()
            {

            }

            public PileOutOfSpaceException(string message) : base(message)
            {
            }

            public PileOutOfSpaceException(string message, Exception inner) : base(message, inner)
            {
            }

            protected PileOutOfSpaceException(SerializationInfo info, StreamingContext context)
              : base(info, context)
            {
            }
      }


      /// <summary>
      /// Thrown by pile cache
      /// </summary>
      [Serializable]
      public class PileCacheException : PileException
      {


            public PileCacheException()
            {

            }

            public PileCacheException(string message) : base(message)
            {
            }

            public PileCacheException(string message, Exception inner) : base(message, inner)
            {
            }

            protected PileCacheException(SerializationInfo info, StreamingContext context)
              : base(info, context)
            {
            }
      }

 }