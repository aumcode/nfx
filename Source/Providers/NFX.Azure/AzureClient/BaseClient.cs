using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using NFX.Web;
using NFX.Log;
using NFX.Serialization.JSON;

namespace NFX.AzureClient
{
  public class BaseClient
  {
    protected static readonly JSONWritingOptions JSON = new JSONWritingOptions()
    {
      MapSkipNulls = true,
      RowsAsMap = true,
      RowMapTargetName = "*"
    };

    protected const string HDR_AUTHORIZATION = "Authorization";

    private const MessageType DEFAULT_LOG_LEVEL = MessageType.Warning;
    private const string LOG_TOPIC = "AzureClient";

    public BaseClient(IWebClientCaller caller, string host, string subscriptionId, AzureOAuth2Token token)
    {
      Caller = caller;
      Host = new Uri(host);
      Token = token;
      SubscriptionId = subscriptionId;
      LogLevel = DEFAULT_LOG_LEVEL;
    }

    public readonly IWebClientCaller Caller;
    public readonly Uri Host;
    public readonly AzureOAuth2Token Token;
    public readonly string SubscriptionId;
    public MessageType LogLevel { get; set; }

    public TResource GetResource<TResource>(string path)
      where TResource : Resource
    {
      var logID = Log(MessageType.Info, "getResource()", "Get " + typeof(TResource).Name);

      try
      {
        var request = new WebClient.RequestParams(Caller)
        {
          Method = HTTPRequestMethod.GET,
          Headers = new Dictionary<string, string>
          { { HDR_AUTHORIZATION, Token.AuthorizationHeader } }
        };

        var response = WebClient.GetJson(new Uri(Host, path), request);
        var result = JSONReader.ToRow<TResource>(response, nameBinding: JSONReader.NameBinding.ByBackendName("*"));
        result.Validate();
        return result;
      }
      catch (Exception error)
      {
        Log(MessageType.Error, "getResource()", error.Message, error, relatedMessageID: logID);
        throw error;
      }
    }

    public void PutResource<TResource, TResult>(string path, TResource networkInterface, out TResult result)
      where TResource : Resource
      where TResult : Resource
    {
      var logID = Log(MessageType.Info, "putResource()", "Get " + typeof(TResource).Name);

      try
      {
        var request = new WebClient.RequestParams(Caller)
        {
          Method = HTTPRequestMethod.PUT,
          ContentType = ContentType.JSON,
          Headers = new Dictionary<string, string>
          { { HDR_AUTHORIZATION, Token.AuthorizationHeader } },
          Body = networkInterface.ToJSON(JSON)
        };

        var response = WebClient.GetJson(new Uri(Host, path), request);
        result = JSONReader.ToRow<TResult>(response, nameBinding: JSONReader.NameBinding.ByBackendName("*"));
        result.Validate();
      }
      catch (Exception error)
      {
        Log(MessageType.Error, "putResource()", error.Message, error, relatedMessageID: logID);
        throw error;
      }
    }


    protected Guid Log(MessageType type,
               string from,
               string message,
               Exception error = null,
               Guid? relatedMessageID = null,
               string parameters = null)
    {
      if (type < LogLevel) return Guid.Empty;

      var logMessage = new Message
      {
        Topic = LOG_TOPIC,
        Text = message ?? string.Empty,
        Type = type,
        From = "{0}.{1}".Args(this.GetType().Name, from),
        Exception = error,
        Parameters = parameters
      };
      if (relatedMessageID.HasValue) logMessage.RelatedTo = relatedMessageID.Value;

      App.Log.Write(logMessage);

      return logMessage.Guid;
    }
  }
}
