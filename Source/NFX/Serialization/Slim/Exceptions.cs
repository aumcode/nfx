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
using System.Runtime.Serialization;
using System.Collections.Generic;

namespace NFX.Serialization.Slim
{

  /// <summary>
  /// Base exception thrown by the Slim serialization format
  /// </summary>
  [Serializable]
  public class SlimException : NFXSerializationException
  {
    public SlimException()
    {
    }

    public SlimException(string message)
      : base(message)
    {
    }

    public SlimException(string message, Exception inner)
      : base(message, inner)
    {
    }

    protected SlimException(SerializationInfo info, StreamingContext context)
      : base(info, context)
    {

    }

  }


  /// <summary>
  /// Base exception thrown by the Slim when serializing objects
  /// </summary>
  [Serializable]
  public class SlimSerializationException : SlimException
  {
    public SlimSerializationException()
    {
    }

    public SlimSerializationException(string message)
      : base(message)
    {
    }

    public SlimSerializationException(string message, Exception inner)
      : base(message, inner)
    {
    }

    protected SlimSerializationException(SerializationInfo info, StreamingContext context)
      : base(info, context)
    {

    }

  }


  /// <summary>
  /// Base exception thrown by the Slim when deserializing objects
  /// </summary>
  [Serializable]
  public class SlimDeserializationException : SlimException
  {
    public SlimDeserializationException()
    {
    }

    public SlimDeserializationException(string message)
      : base(message)
    {
    }

    public SlimDeserializationException(string message, Exception inner)
      : base(message, inner)
    {
    }

    protected SlimDeserializationException(SerializationInfo info, StreamingContext context)
      : base(info, context)
    {

    }

  }



  /// <summary>
  /// Thrown by type registry when supplied type handle is invalid/not found
  /// </summary>
  [Serializable]
  public class SlimInvalidTypeHandleException : SlimException
  {
    public SlimInvalidTypeHandleException()
    {
    }

    public SlimInvalidTypeHandleException(string message)
      : base(message)
    {
    }

    public SlimInvalidTypeHandleException(string message, Exception inner)
      : base(message, inner)
    {
    }

    protected SlimInvalidTypeHandleException(SerializationInfo info, StreamingContext context)
      : base(info, context)
    {

    }
  }

  /// <summary>
  /// Thrown when a type is decoreted with SlimSerializationProhibitedAttribute
  /// </summary>
  [Serializable]
  public class SlimSerializationProhibitedException : SlimException
  {
    protected SlimSerializationProhibitedException()
    {
    }

    public SlimSerializationProhibitedException(Type type)
      : base(StringConsts.SLIM_SER_PROHIBIT_ERROR.Args( type!=null ? type.FullName : StringConsts.NULL_STRING,
                                                        typeof(SlimSerializationProhibitedAttribute).Name ))
    {
    }

    protected SlimSerializationProhibitedException(SerializationInfo info, StreamingContext context)
      : base(info, context)
    {

    }
  }

}