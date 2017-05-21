using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using NFX.Environment;
using NFX.Log;

namespace NFX.Web.Messaging
{
  /// <summary>
  /// Sending SMS with Twilio REST API
  /// </summary>
  public sealed class TwilioSink : MessageSink, IWebClientCaller
  {
    #region CONSTS

    private const string URI_MESSAGES= "https://api.twilio.com/2010-04-01/Accounts/{0}/Messages.json";

    private const string HDR_AUTHORIZATION = "Authorization";
    private const string HDR_AUTHORIZATION_TOKEN = "Basic {0}";

    private const string ERROR_MESSAGE = "error_message";

    #endregion

    public TwilioSink(MessageService director) : base(director)
    {
    }

    #region IWebClientCaller

    private int m_WebServiceCallTimeoutMs;

    [Config(Default = false)]
    public bool KeepAlive { get; set; }

    [Config(Default = true)]
    public bool Pipelined { get; set; }

    [Config(Default = 20000)]
    public int WebServiceCallTimeoutMs
    {
      get { return m_WebServiceCallTimeoutMs; }
      set { m_WebServiceCallTimeoutMs = value < 0 ? 0 : value; }
    }

    #endregion

    #region Properties

    [Config] public string AccountSid { get; set; }
    [Config] public string AuthToken { get; set; }
    [Config] public string From { get; set; }

    public string AuthorizationKey
    {
      get { return Convert.ToBase64String(Encoding.UTF8.GetBytes("{0}:{1}".Args(AccountSid, AuthToken))); }
    }

    public override MsgChannels SupportedChannels
    {
      get { return MsgChannels.SMS; }
    }

    #endregion

    protected override bool DoSendMsg(Message msg)
    {
      if (msg == null || msg.TOAddress.IsNullOrWhiteSpace())
        throw new WebException("TwilioSink.DoSendMsg(msg==null)");

      if (msg.TOAddress.IsNullOrWhiteSpace())
        throw new WebException("TwilioSink.DoSendMsg(msg.TOAddress==null)");

      if (msg.Body.IsNullOrEmpty())
        throw new WebException("TwilioSink.DoSendMsg(msg.body==null");

      var addressees =
        msg.MessageAddress.All.Where(a => SupportedChannelNames.Any(n => n.EqualsOrdIgnoreCase(a.ChannelName)));

      var from = Uri.EscapeDataString(From);
      var body = Uri.EscapeDataString(msg.Body);
      var auth = HDR_AUTHORIZATION_TOKEN.Args(AuthorizationKey);
      var sent = false;
      foreach (var addressee in addressees)
      {
        var to = Uri.EscapeDataString(addressee.ChannelAddress);
        try
        {
          sent = sent || doCall(from, to, body, auth);
        }
        catch (Exception error)
        {
          var et = error.ToMessageWithType();
          Log(MessageType.Error, "{0}.DoSendMsg(msg): {1}".Args(this.GetType().FullName, et), et);
        }
      }
      return sent;
    }


    private bool doCall(string from, string to, string body, string auth)
    {
      var request = new WebClient.RequestParams(this)
      {
        Method = HTTPRequestMethod.POST,
        ContentType = ContentType.FORM_URL_ENCODED,
        Headers = new Dictionary<string, string>
        {
          { HDR_AUTHORIZATION, auth }
        },
        BodyParameters = new Dictionary<string, string>
        {
          { "From", from },
          { "To",   to },
          { "Body", body }
        }
      };
      var response = WebClient.GetJson(new Uri(URI_MESSAGES.Args(AccountSid)), request);

      if (response[ERROR_MESSAGE].AsString().IsNotNullOrEmpty()) return false;
      return true;
    }
  }
}
