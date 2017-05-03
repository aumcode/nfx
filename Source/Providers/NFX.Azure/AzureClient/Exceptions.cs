using System;
using System.Runtime.Serialization;

using NFX.Azure;

namespace NFX.AzureClient
{
  /// <summary>
  /// Thrown to indicate azure related problems (server side)
  /// </summary>
  [Serializable]
  public class AzureClientException : AzureException
  {
    public AzureClientException() : base() { }
    public AzureClientException(string message) : base(message) { }
    public AzureClientException(string message, Exception inner) : base(message, inner) { }
    protected AzureClientException(SerializationInfo info, StreamingContext context) : base(info, context) { }
  }
}
