using System;
using System.Runtime.Serialization;

using NFX.Web.Cloud;

namespace NFX.Azure
{
  /// <summary>
  /// Thrown to indicate azure related problems (server side)
  /// </summary>
  [Serializable]
  public class AzureException : CloudException
  {
    public AzureException() : base() { }
    public AzureException(string message) : base(message) { }
    public AzureException(string message, Exception inner) : base(message, inner) { }
    protected AzureException(SerializationInfo info, StreamingContext context) : base(info, context) { }
  }
}
