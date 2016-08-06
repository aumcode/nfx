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
using System.Text;
using System.Collections.Generic;
using System.Runtime.Serialization;

namespace NFX.DataAccess.Distributed
{
      /// <summary>
      /// Thrown by distributed data access classes
      /// </summary>
      [Serializable]
      public class DistributedDataAccessException : DataAccessException
      {
            public DistributedDataAccessException():base()
            {
            }

            public DistributedDataAccessException(string message)
              : base(message)
            {
            }

            public DistributedDataAccessException(string message, Exception inner)
              : base(message, inner)
            {
            }

            protected DistributedDataAccessException(SerializationInfo info, StreamingContext context)
              : base(info, context)
            {
            }
      }

      /// <summary>
      /// Thrown by distributed data access classes when parcel serialization problems happen
      /// </summary>
      [Serializable]
      public class DistributedDataParcelSerializationException : DistributedDataAccessException
      {
            public DistributedDataParcelSerializationException()
            {
            }

            public DistributedDataParcelSerializationException(string message)
              : base(message)
            {
            }

            public DistributedDataParcelSerializationException(string message, Exception inner)
              : base(message, inner)
            {
            }

            protected DistributedDataParcelSerializationException(SerializationInfo info, StreamingContext context)
              : base(info, context)
            {
            }
      }


      /// <summary>
      /// Thrown by distributed data access classes  to indicate some data validation error
      /// </summary>
      [Serializable]
      public class ParcelValidationException : DistributedDataAccessException
      {
            public ParcelValidationException()
            {
            }

            public ParcelValidationException(string message)
              : base(message)
            {
            }

            public ParcelValidationException(string message, Exception inner)
              : base(message, inner)
            {
            }

            protected ParcelValidationException(SerializationInfo info, StreamingContext context)
              : base(info, context)
            {
            }
      }

      /// <summary>
      /// Thrown by Parcel.Seal() method trying to ensure parcel consistency before it gets sealed
      /// </summary>
      [Serializable]
      public class ParcelSealValidationException : ParcelValidationException
      {
            private ParcelSealValidationException(string msg) : base(msg) { }

            public static ParcelSealValidationException ForErrors(string parcelName, IEnumerable<Exception> errors)
            {
                var sb = new StringBuilder();

                sb.AppendLine( StringConsts.DISTRIBUTED_DATA_PARCEL_SEAL_VALIDATION_ERROR.Args(parcelName) );
                foreach(var error in errors)
                 sb.AppendLine( " * " + error.ToMessageWithType() );

                return new ParcelSealValidationException( sb.ToString() );
            }



            protected ParcelSealValidationException(SerializationInfo info, StreamingContext context)
              : base(info, context)
            {
            }
      }





 }