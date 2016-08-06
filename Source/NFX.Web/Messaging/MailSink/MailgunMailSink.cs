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
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Threading;
using NFX.Environment;
using NFX.Financial;
using NFX.Instrumentation;
using NFX.Log;
using NFX.Serialization.JSON;

namespace NFX.Web.Messaging
{
  /// <summary>
  /// Implements mailer sink using Mailgun service
  /// </summary>
  public sealed class MailgunMailerSink : MessageSink, IWebClientCaller
  {
    #region Consts

    public const string CONFIG_FALLBACKSINK_SECTION = "fallback-sink";

    //Mail standard parameters
    private const string BASE_SERVICE_URL = "https://api.mailgun.net/v3";
    private const string MAIL_PARAM_FROM = "from";
    private const string MAIL_PARAM_TO = "to";
    private const string MAIL_PARAM_CC = "cc";
    private const string MAIL_PARAM_BCC = "bcc";
    private const string MAIL_PARAM_SUBJECT = "subject";
    private const string MAIL_PARAM_TEXT = "text";
    private const string MAIL_PARAM_HTML = "html";
    private const string MAIL_PARAM_ATTACHMENT = "attachment";
    private const string MAIL_PARAM_INLINE = "inline";

    // Mailgun specific parameters. Reffer to https://documentation.mailgun.com/api-sending.html#sending
    private const string API_PARAM_TAG = "o:tag";
    private const string API_PARAM_CAMPAIGN = "o:campaign";
    private const string API_PARAM_DKIM_ENABLED = "o:dkim";
    private const string API_PARAM_DELIVERYTIME = "o:deliverytime";
    private const string API_PARAM_TESTMODE = "o:testmode";
    private const string API_PARAM_TRACKING = "o:tracking";
    private const string API_PARAM_TRACKING_CLICKS = "o:tracking-clicks";
    private const string API_PARAM_TRACKING_OPENS = "o:tracking-opens";

    #endregion

    #region .ctor

    public MailgunMailerSink(MessageService director)
      : base(director)
    {
    }

    #endregion

    #region Private Fields

    private int m_WebServiceCallTimeoutMs;
    private MessageSink m_FallbackSink;

    #endregion

    #region Properties

    [Config]
    public string AuthorizationKey { get; set; }

    [Config]
    public string Domain { get; set; }

    [Config]
    public string DefaultFromAddress { get; set; }

    [Config]
    public string DefaultFromName { get; set; }

    [Config]
    public bool TestMode { get; set; }

    [Config]
    public IMessageSink FallbackSink
    {
      get { return m_FallbackSink; }
      set
      {
        m_FallbackSink = value as MessageSink;
      }
    }
    #endregion

    #region Protected
    protected override void DoConfigure(Environment.IConfigSectionNode node)
    {
      base.DoConfigure(node);
      m_FallbackSink = FactoryUtils.MakeAndConfigure<MessageSink>(node[CONFIG_FALLBACKSINK_SECTION], typeof(NOPMessageSink), args: new object[] { this.ComponentDirector });
    }
    protected override void DoStart()
    {
      base.DoStart();
      if (m_FallbackSink != null)
        m_FallbackSink.Start();
    }

    /// <summary>
    /// MessageSink DoSendMsg implementation
    /// </summary>
    /// <param name="msg">Message</param>
    protected override void DoSendMsg(Message msg)
    {
      if (msg == null || msg.TOAddress.IsNullOrWhiteSpace()) return;

      var request = new WebClient.RequestParams()
      {
        Caller = this,
        Method = HTTPRequestMethod.POST,
        Uri = new Uri(ServiceUrl),
        Headers = new Dictionary<string, string>(),
        BodyParameters = new Dictionary<string, string>(),
        UName = "api",
        UPwd = AuthorizationKey
      };

      var fromAddress = "{0} <{1}>".Args(DefaultFromName, DefaultFromAddress);
      if (msg.FROMAddress.IsNotNullOrWhiteSpace())
      {
        fromAddress = "{0} <{1}>".Args(msg.FROMName, msg.FROMAddress);
      }

      addParameter(request.BodyParameters, MAIL_PARAM_FROM, fromAddress);
      addParameter(request.BodyParameters, MAIL_PARAM_TO, "{0} <{1}>".Args(msg.TOName, msg.TOAddress));
      addParameter(request.BodyParameters, MAIL_PARAM_CC, msg.CC);
      addParameter(request.BodyParameters, MAIL_PARAM_BCC, msg.BCC);
      addParameter(request.BodyParameters, MAIL_PARAM_SUBJECT, msg.Subject);
      addParameter(request.BodyParameters, MAIL_PARAM_TEXT, msg.Body);
      addParameter(request.BodyParameters, MAIL_PARAM_HTML, msg.HTMLBody);

      if (TestMode)
      {
        request.BodyParameters.Add(API_PARAM_TESTMODE, "Yes");
      }
      try
      {
        var result = WebClient.GetJsonAsDynamic(request);
      }
      catch (Exception e)
      {
        if (m_FallbackSink != null)
          m_FallbackSink.SendMsg(msg);
      }

    }
    #endregion

    #region IWebClientCaller

    [Config(Default = 20000)]
    public int WebServiceCallTimeoutMs
    {
      get { return m_WebServiceCallTimeoutMs; }
      set { m_WebServiceCallTimeoutMs = value < 0 ? 0 : value; }
    }

    [Config(Default = false)]
    public bool KeepAlive { get; set; }

    [Config(Default = false)]
    public bool Pipelined { get; set; }

    public string ServiceUrl
    {
      get
      {
        return "{0}/{1}/messages".Args(BASE_SERVICE_URL, Domain);
      }
    }
    #endregion

    #region .pvt. impl.
    private void addParameter(IDictionary<string, string> parameters, string name, string value)
    {
      if (parameters == null) return;
      if (name.IsNullOrWhiteSpace() || value.IsNullOrWhiteSpace()) return;

      parameters.Add(name, Uri.EscapeDataString(value));
    }
    #endregion
  }
}
