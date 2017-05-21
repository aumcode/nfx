/*<FILE_LICENSE>
* NFX (.NET Framework Extension) Unistack Library
* Copyright 2003-2017 ITAdapter Corp. Inc.
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
 * Originated: 2006.01
 * Revision: NFX 0.3  2009.10.12
 */
using System;
using System.Runtime.Serialization;

using NFX.ApplicationModel;
using NFX.Serialization.BSON;

namespace NFX
{
  /// <summary>
  /// Base exception thrown by the framework
  /// </summary>
  [Serializable]
  public class NFXException : Exception
  {
    public const string CODE_FLD_NAME = "NE-C";

    public NFXException() {}
    public NFXException(string message) : base(message) {}
    public NFXException(string message, Exception inner) : base(message, inner) {}
    protected NFXException(SerializationInfo info, StreamingContext context) : base(info, context) { Code = info.GetInt32(CODE_FLD_NAME); }

    /// <summary>
    /// Provides general-purpose error code
    /// </summary>
    public int Code { get; set; }

    public override void GetObjectData(SerializationInfo info, StreamingContext context)
    {
      if (info == null)
        throw new ArgumentNullException("info", GetType().Name + ".GetObjectData(info=null)");
      info.AddValue(CODE_FLD_NAME, Code);
      base.GetObjectData(info, context);
    }
  }


  /// <summary>
  /// Thrown by Debug class to indicate assertion failures
  /// </summary>
  [Serializable]
  public sealed class DebugAssertionException : NFXException
  {
    public const string FROM_FLD_NAME = "DAE-F";

    public DebugAssertionException(string from = null) { m_From = from; }
    public DebugAssertionException(string message, string from = null) : base(message) { m_From = from; }
    public DebugAssertionException(SerializationInfo info, StreamingContext context) : base(info, context)
    {
      m_From = info.GetString(FROM_FLD_NAME);
    }

    private string m_From;

    public string From { get { return m_From ?? string.Empty; } }

    public override void GetObjectData(SerializationInfo info, StreamingContext context)
    {
      if (info == null)
        throw new NFXException(StringConsts.ARGUMENT_ERROR + GetType().Name + ".GetObjectData(info=null)");
      info.AddValue(FROM_FLD_NAME, m_From);
      base.GetObjectData(info, context);
    }
  }


  /// <summary>
  /// Thrown by Aver class to indicate averment failures
  /// </summary>
  [Serializable]
  public sealed class AvermentException : NFXException
  {
    public const string FROM_FLD_NAME = "AE-F";

    public AvermentException(string from = null) { m_From = from; }

    public AvermentException(string message, string from = null) : base((from.IsNullOrWhiteSpace() ? "" : "from '{0}' ".Args(from)) + message)
    {
      m_From = from;
    }

    public AvermentException(SerializationInfo info, StreamingContext context) : base(info, context)
    {
      m_From = info.GetString(FROM_FLD_NAME);
    }

    private string m_From;

    public string From { get { return m_From ?? string.Empty; } }

    public override void GetObjectData(SerializationInfo info, StreamingContext context)
    {
      if (info == null)
        throw new NFXException(StringConsts.ARGUMENT_ERROR + GetType().Name + ".GetObjectData(info=null)");
      info.AddValue(FROM_FLD_NAME, m_From);
      base.GetObjectData(info, context);
    }
  }

  /// <summary>
  /// Provides textual portable data about this exception which will be used in wrapped exception.
  /// Wrapped exceptions are used to marshall non serializable exceptions
  /// </summary>
  public interface IWrappedExceptionDataSource
  {
    /// <summary>
    /// Gets portable textual representation of exception data for inclusion in wrapped exception
    /// </summary>
    string GetWrappedData();
  }

  /// <summary>
  /// Marshalls exception details
  /// </summary>
  [Serializable]
  [BSONSerializable("A339F46F-6637-4396-B148-094BAFFB4BE6")]
  public sealed class WrappedExceptionData : IBSONSerializable, IBSONDeserializable
  {

    internal WrappedExceptionData(){}

    /// <summary>
    /// Initializes instance form local exception
    /// </summary>
    public WrappedExceptionData(Exception error, bool captureStack = true)
    {
      if (error==null) throw new NFXException(StringConsts.ARGUMENT_ERROR+"WrappedExceptionData.ctor(error=null)");

      var tp = error.GetType();
      m_TypeName = tp.FullName;
      m_Message = error.Message;
      if (error is NFXException)
        m_Code = ((NFXException)error).Code;

      m_ApplicationName = ExecutionContext.Application.Name;

      m_Source = error.Source;
      if (captureStack)
        m_StackTrace = error.StackTrace;

      if (error.InnerException != null)
        m_InnerException = new WrappedExceptionData(error.InnerException);

      var source = error as IWrappedExceptionDataSource;
      if (source != null)
        m_WrappedData = source.GetWrappedData();
    }

    private string m_TypeName;
    private string m_Message;
    private int m_Code;
    private string m_ApplicationName;
    private string m_Source;
    private string m_StackTrace;
    private string m_WrappedData;
    private WrappedExceptionData m_InnerException;

    /// <summary>
    /// Returns the name of remote exception type
    /// </summary>
    public string TypeName { get { return m_TypeName ?? CoreConsts.UNKNOWN; } }

    /// <summary>
    /// Returns the message of remote exception
    /// </summary>
    public string Message { get { return m_Message ?? string.Empty; } }

    /// <summary>
    /// Returns the code of remote NFX exception
    /// </summary>
    public int Code { get { return m_Code; } }

    /// <summary>
    /// Name of the object that caused the error
    /// </summary>
    public string Source { get { return m_Source ?? string.Empty; } }

    /// <summary>
    /// Returns stack trace
    /// </summary>
    public string StackTrace { get { return m_StackTrace ?? string.Empty; } }

    /// <summary>
    /// Returns the name of remote application
    /// </summary>
    public string ApplicationName { get { return m_ApplicationName ?? CoreConsts.UNKNOWN; } }

    /// <summary>
    /// Returns wrapped date from IWrappedDataSource
    /// </summary>
    public string WrappedData { get { return m_WrappedData; } }

    /// <summary>
    /// Returns the inner remote exception if any
    /// </summary>
    public WrappedExceptionData InnerException { get { return m_InnerException; } }

    public override string ToString()
    {
      return string.Format("[{0}:{1}:{2}] {3}", TypeName, Code, ApplicationName, Message);
    }

    public void SerializeToBSON(BSONSerializer serializer, BSONDocument doc, IBSONSerializable parent, ref object context)
    {
      serializer.AddTypeIDField(doc, parent, this, context);

      doc.Set( new BSONStringElement("tname", TypeName))
         .Set( new BSONStringElement("msg",   Message))
         .Set( new BSONInt32Element ("code",  Code))
         .Set( new BSONStringElement("app",   ApplicationName))
         .Set( new BSONStringElement("src",   Source))
         .Set( new BSONStringElement("trace", StackTrace));

      if (WrappedData!=null)
        doc.Set( new BSONStringElement("wdata", WrappedData));

      if (m_InnerException==null) return;

      doc.Set( new BSONDocumentElement("inner", serializer.Serialize(m_InnerException, parent: this)) );
    }

    public bool IsKnownTypeForBSONDeserialization(Type type)
    {
      return type==typeof(WrappedExceptionData);
    }

    public void DeserializeFromBSON(BSONSerializer serializer, BSONDocument doc, ref object context)
    {
      m_TypeName        = doc.TryGetObjectValueOf("tname").AsString();
      m_Message         = doc.TryGetObjectValueOf("msg").AsString();
      m_Code            = doc.TryGetObjectValueOf("code").AsInt();
      m_ApplicationName = doc.TryGetObjectValueOf("app").AsString();
      m_Source          = doc.TryGetObjectValueOf("src").AsString();
      m_StackTrace      = doc.TryGetObjectValueOf("trace").AsString();
      m_WrappedData     = doc.TryGetObjectValueOf("wdata").AsString();

      var iv = doc["inner"] as BSONDocumentElement;
      if (iv==null) return;

      m_InnerException = new WrappedExceptionData();
      serializer.Deserialize(iv.Value, m_InnerException);
    }
  }

  /// <summary>
  /// Represents exception that contains data about causing exception with all of it's chain
  /// </summary>
  [Serializable]
  [BSONSerializable("A43ABD0D-22B2-4012-8A24-280A038FD943")]
  public sealed class WrappedException : NFXException, IBSONSerializable, IBSONDeserializable
  {
    public const string WRAPPED_FLD_NAME = "WE-W";

    /// <summary>
    /// Returns an exception wrapped into WrappedException. If the exception is already wrapped, it is returned as-is
    /// </summary>
    public static WrappedException ForException(Exception root, bool captureStack = true)
    {
      if (root==null) return null;

      var we = root as WrappedException;
      if (we==null)
       we = new WrappedException( new WrappedExceptionData(root, captureStack) );

      return we;
    }

    public static WrappedException MakeFromBSON(BSONSerializer serializer, BSONDocument doc)
    {
      var wrp = doc["wrp"] as BSONDocumentElement;
      var result = wrp==null ? new WrappedException() : new WrappedException(wrp.Value.TryGetObjectValueOf("msg").AsString());
      serializer.Deserialize(doc, result);
      return result;
    }

    internal WrappedException() {}
    internal WrappedException(string msg): base(msg){}


    public WrappedException(WrappedExceptionData data) : base(data.Message) { m_Wrapped = data; }
    public WrappedException(string message, WrappedExceptionData data) : base(message) { m_Wrapped = data; }

    internal WrappedException(SerializationInfo info, StreamingContext context) : base(info, context)
    {
      m_Wrapped = (WrappedExceptionData)info.GetValue(WRAPPED_FLD_NAME, typeof(WrappedExceptionData));
    }

    private WrappedExceptionData m_Wrapped;

    /// <summary>
    /// Returns wrapped exception data
    /// </summary>
    public WrappedExceptionData Wrapped { get { return m_Wrapped; } }

    public override void GetObjectData(SerializationInfo info, StreamingContext context)
    {
      if (info == null)
        throw new NFXException(StringConsts.ARGUMENT_ERROR + GetType().Name + ".GetObjectData(info=null)");
      info.AddValue(WRAPPED_FLD_NAME, m_Wrapped);
      base.GetObjectData(info, context);
    }

    public void SerializeToBSON(BSONSerializer serializer, BSONDocument doc, IBSONSerializable parent, ref object context)
    {
      serializer.AddTypeIDField(doc, parent, this, context);
      doc.Set( new BSONDocumentElement("wrp", serializer.Serialize(m_Wrapped, parent: this)));
    }

    public bool IsKnownTypeForBSONDeserialization(Type type)
    {
      return type==typeof(WrappedExceptionData);
    }

    public void DeserializeFromBSON(BSONSerializer serializer, BSONDocument doc, ref object context)
    {
      var iv = doc["wrp"] as BSONDocumentElement;
      if (iv==null) return;

      m_Wrapped = new WrappedExceptionData();
      serializer.Deserialize(iv.Value, m_Wrapped);
    }
  }
}