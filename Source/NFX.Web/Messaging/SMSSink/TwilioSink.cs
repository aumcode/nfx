using System;
using System.Collections.Generic;
using System.Linq;

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

    private const string URI_MESSAGES = "https://api.twilio.com/2010-04-01/Accounts/{0}/Messages.json";
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

    [Config]
    public string AccountSid { get; set; }
    [Config]
    public string AuthToken { get; set; }
    [Config]
    public string From { get; set; }

    public override MsgChannels SupportedChannels
    {
      get { return MsgChannels.SMS; }
    }

    #endregion

    #region Protected

    protected override bool DoSendMsg(Message msg)
    {
      if (msg == null || !msg.AddressToBuilder.All.Any())
        throw new WebException("TwilioSink.DoSendMsg(msg==null)");
      
      if (msg.Body.IsNullOrEmpty())
        throw new WebException("TwilioSink.DoSendMsg(msg.body==null");

      var addressTo = msg.AddressToBuilder.GetMatchesForChannels(SupportedChannelNames).ToList();

      var from = Uri.EscapeDataString(From);
      var body = Uri.EscapeDataString(msg.Body);
      var auth = new HttpBasicAuthenticationHelper(AccountSid, AuthToken);

      var wasSent = false;
      foreach (var addressee in addressTo)
      {
        var to = Uri.EscapeDataString(addressee.ChannelAddress);
        try
        {
          var wasSentNow = doCall(from, to, body, auth);
          wasSent |= wasSentNow;
        }
        catch (Exception error)
        {
          var et = error.ToMessageWithType();
          Log(MessageType.Error, "{0}.DoSendMsg(msg): {1}".Args(this.GetType().FullName, et), et);
        }
      }
      return wasSent;
    }

    #endregion

    #region .pvt

    private bool doCall(string from, string to, string body, HttpBasicAuthenticationHelper auth)
    {
      var request = new WebClient.RequestParams(this)
      {
        Method = HTTPRequestMethod.POST,
        ContentType = ContentType.FORM_URL_ENCODED,
        BodyParameters = new Dictionary<string, string>
        {
          { "From", from },
          { "To",   to },
          { "Body", body }
        }
      };

      auth.AddAuthHeader(ref request);

      var response = WebClient.GetJson(new Uri(URI_MESSAGES.Args(AccountSid)), request);

      if (response[ERROR_MESSAGE].AsString().IsNotNullOrEmpty()) return false;
      return true;
    }

    #endregion
  }
}
