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

namespace NFX.RecordModel
{
  [Serializable]
  public class RecordModelException : System.Exception
  {
    public RecordModelException()
    {
    }

    public RecordModelException(string message)
      : base(message)
    {
    }

    public RecordModelException(string message, Exception inner)
      : base(message, inner)
    {
    }

    protected RecordModelException(SerializationInfo info, StreamingContext context)
      : base(info, context)
    {
    }
  }

  [Serializable]
  public class ModelValidationException : RecordModelException
  {

    public ModelBase ErroredModel { get; set; }

    public ModelValidationException()
    {
    }

    public ModelValidationException(string message)
      : base(message)
    {
    }

    public ModelValidationException(string message, ModelBase erroredModel)
      : base(message)
    {
      ErroredModel = erroredModel;
    }


    public ModelValidationException(string message, Exception inner)
      : base(message, inner)
    {
    }

    protected ModelValidationException(SerializationInfo info, StreamingContext context)
      : base(info, context)
    {
    }
  }


  [Serializable]
  public class RequiredValidationException : ModelValidationException
  {

    public RequiredValidationException()
    {
    }

    public RequiredValidationException(string message)
      : base(message)
    {
    }

    public RequiredValidationException(string message, ModelBase source)
      : base(message, source)
    {
     
    }


    public RequiredValidationException(string message, Exception inner)
      : base(message, inner)
    {
    }

    protected RequiredValidationException(SerializationInfo info, StreamingContext context)
      : base(info, context)
    {
    }
  }


  [Serializable]
  public class SizeValidationException : ModelValidationException
  {

    public SizeValidationException()
    {
    }

    public SizeValidationException(string message)
      : base(message)
    {
    }

    public SizeValidationException(string message, ModelBase source)
      : base(message, source)
    {

    }


    public SizeValidationException(string message, Exception inner)
      : base(message, inner)
    {
    }

    protected SizeValidationException(SerializationInfo info, StreamingContext context)
      : base(info, context)
    {
    }
  }
  
  [Serializable]
  public class RegExpValidationException : ModelValidationException
  {

    public RegExpValidationException()
    {
    }

    public RegExpValidationException(string message)
      : base(message)
    {
    }

    public RegExpValidationException(string message, ModelBase source)
      : base(message, source)
    {

    }


    public RegExpValidationException(string message, Exception inner)
      : base(message, inner)
    {
    }

    protected RegExpValidationException(SerializationInfo info, StreamingContext context)
      : base(info, context)
    {
    }
  }
  

  [Serializable]
  public class MinMaxValidationException : ModelValidationException
  {

    public MinMaxValidationException()
    {
    }

    public MinMaxValidationException(string message)
      : base(message)
    {
    }

    public MinMaxValidationException(string message, ModelBase source)
      : base(message, source)
    {

    }


    public MinMaxValidationException(string message, Exception inner)
      : base(message, inner)
    {
    }

    protected MinMaxValidationException(SerializationInfo info, StreamingContext context)
      : base(info, context)
    {
    }
  }


  [Serializable]
  public class LookupValidationException : ModelValidationException
  {

    public LookupValidationException()
    {
    }

    public LookupValidationException(string message)
      : base(message)
    {
    }

    public LookupValidationException(string message, ModelBase source)
      : base(message, source)
    {

    }


    public LookupValidationException(string message, Exception inner)
      : base(message, inner)
    {
    }

    protected LookupValidationException(SerializationInfo info, StreamingContext context)
      : base(info, context)
    {
    }
  }
  
  
  
}
