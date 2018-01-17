/*<FILE_LICENSE>
* NFX (.NET Framework Extension) Unistack Library
* Copyright 2003-2018 Agnicore Inc. portions ITAdapter Corp. Inc.
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
using System.Linq;
using System.Net;

using NFX.Environment;
using NFX.Log;

namespace NFX.Web.Messaging
{
  /// <summary>
  /// Implements mailer sink using Mailgun service
  /// </summary>
  public sealed class MailgunMailerSink : MessageSink, IWebClientCaller
  {
    #region Consts

    private const string API_USER_NAME = "api";

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

    private const string MAIL_PARAM_REPLY_TO = "h:Reply-To";

    // Mailgun specific parameters. Reffer to https://documentation.mailgun.com/en/latest/api-sending.html#sending
    private const string API_PARAM_TAG = "o:tag";
    private const string API_PARAM_CAMPAIGN = "o:campaign";
    private const string API_PARAM_DKIM_ENABLED = "o:dkim";
    private const string API_PARAM_DELIVERYTIME = "o:deliverytime";
    private const string API_PARAM_TESTMODE = "o:testmode";
    private const string API_PARAM_TRACKING = "o:tracking";
    private const string API_PARAM_TRACKING_CLICKS = "o:tracking-clicks";
    private const string API_PARAM_TRACKING_OPENS = "o:tracking-opens";

    #endregion

    #region Inner

    private class MessageAddresses
    {
      public string From { get; set; }
      public string To   { get; set; }
      public string CC   { get; set; }
      public string BCC  { get; set; }
      public string ReplyTo  { get; set; }
    }

    #endregion

    #region .ctor

    public MailgunMailerSink(MessageService director)
      : base(director)
    {
    }

    #endregion

    #region Private Fields

    private int m_WebServiceCallTimeoutMs;

    #endregion

    #region Properties

    [Config] public string AuthorizationKey { get; set; }
    [Config] public string Domain { get; set; }
    [Config] public string DefaultFromAddress { get; set; }
    [Config] public string DefaultFromName { get; set; }
    [Config] public bool TestMode { get; set; }
    [Config] public bool DKIM { get; set; }

    public Uri ServiceUrl { get { return new Uri("{0}/{1}/messages".Args(BASE_SERVICE_URL, Domain)); } }

    #endregion

    #region Protected

    protected override void DoConfigure(IConfigSectionNode node)
    {
      ConfigAttribute.Apply(this, node);
    }

    /// <summary>
    /// MessageSink DoSendMsg implementation
    /// </summary>
    /// <param name="msg">Message</param>
    protected override bool DoSendMsg(Message msg)
    {
      if (msg == null || !msg.AddressToBuilder.All.Any())
        return false;

      var sent = false;

      try
      {
        var addresses = new MessageAddresses();

        var addressFrom = msg.AddressFromBuilder.GetFirstOrDefaultMatchForChannels(SupportedChannelNames);
        addresses.From = addressFrom.Assigned ? fmtEmail(addressFrom.Name, addressFrom.ChannelAddress)
                                              : fmtEmail(DefaultFromName, DefaultFromAddress);

        var addressTo = msg.AddressToBuilder.GetMatchesForChannels(SupportedChannelNames).ToList();
        addresses.To = string.Join(", ", addressTo.Select(a => fmtEmail(a.Name, a.ChannelAddress)).ToArray());

        var addressCC = msg.AddressCCBuilder.GetMatchesForChannels(SupportedChannelNames);
       addresses.CC = string.Join(", ", addressCC.Select(a => fmtEmail(a.Name, a.ChannelAddress)).ToArray());

        var addressBCC = msg.AddressBCCBuilder.GetMatchesForChannels(SupportedChannelNames);
        addresses.BCC = string.Join(", ", addressBCC.Select(a => fmtEmail(a.Name, a.ChannelAddress)).ToArray());

        var addressReplyTo = msg.AddressReplyToBuilder.GetMatchesForChannels(SupportedChannelNames);
       addresses.ReplyTo = string.Join(", ", addressReplyTo.Select(a => fmtEmail(a.Name, a.ChannelAddress)).ToArray());

        //20170921 opan, dkh: fix too long content for Uri encoding - switch to multipart
        //if (msg.Attachments == null)
        //  doSendWithoutAttachments(msg, addresses);
        //else
        doSendMultipart(msg, addresses);
        sent = true;
      }
      catch (Exception error)
      {
        var et = error.ToMessageWithType();
        Log(MessageType.Error, "{0}.DoSendMsg(msg): {1}".Args(this.GetType().FullName, et), et);
      }

      return sent;
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

    public override MsgChannels SupportedChannels
    {
      get
      {
        return MsgChannels.EMail;
      }
    }
    #endregion

    #region .pvt. impl.

      //private void doSendWithoutAttachments(Message msg, MessageAddresses addresses)
      //{
      //  var request = new WebClient.RequestParams(this)
      //  {
      //    Method = HTTPRequestMethod.POST,
      //    BodyParameters = new Dictionary<string, string>()
      //  };

      //  var auth = new HttpBasicAuthenticationHelper(API_USER_NAME, AuthorizationKey);
      //  auth.AddAuthHeader(ref request);

      //  addParameter(request.BodyParameters, MAIL_PARAM_FROM, addresses.From);
      //  addParameter(request.BodyParameters, MAIL_PARAM_TO, addresses.To);

      //  if (addresses.CC.IsNotNullOrWhiteSpace())
      //      addParameter(request.BodyParameters, MAIL_PARAM_CC, addresses.CC);

      //  if (addresses.BCC.IsNotNullOrWhiteSpace())
      //    addParameter(request.BodyParameters, MAIL_PARAM_BCC, addresses.BCC);

      //  if (addresses.ReplyTo.IsNotNullOrWhiteSpace())
      //    addParameter(request.BodyParameters, MAIL_PARAM_REPLY_TO, addresses.ReplyTo);

      //  if (msg.Subject.IsNotNullOrWhiteSpace())
      //    addParameter(request.BodyParameters, MAIL_PARAM_SUBJECT, msg.Subject);

      //  if (msg.Body.IsNotNullOrWhiteSpace())
      //    addParameter(request.BodyParameters, MAIL_PARAM_TEXT, msg.Body);

      //  if (msg.RichBody.IsNotNullOrWhiteSpace())
      //    addParameter(request.BodyParameters, MAIL_PARAM_HTML, msg.RichBody);

      //  if (TestMode)
      //    request.BodyParameters.Add(API_PARAM_TESTMODE, "yes");

      //  WebClient.GetString(ServiceUrl, request);
      //}

      private void doSendMultipart(Message msg, MessageAddresses addresses)
      {
        Multipart.Part part;

        var parts = new List<Multipart.Part>();

        part = new Multipart.Part(MAIL_PARAM_FROM)
        {
          ContentType = ContentType.TEXT,
          Content = addresses.From
        };
        parts.Add(part);

        part = new Multipart.Part(MAIL_PARAM_TO)
        {
          ContentType = ContentType.TEXT,
          Content = addresses.To
        };
        parts.Add(part);

        if (addresses.CC.IsNotNullOrWhiteSpace())
        {
          part = new Multipart.Part(MAIL_PARAM_CC)
          {
            ContentType = ContentType.TEXT,
            Content = addresses.CC
          };
          parts.Add(part);
        }

        if (addresses.BCC.IsNotNullOrWhiteSpace())
        {
          part = new Multipart.Part(MAIL_PARAM_BCC)
          {
            ContentType = ContentType.TEXT,
            Content = addresses.BCC
          };
          parts.Add(part);
        }

        if (addresses.ReplyTo.IsNotNullOrWhiteSpace())
        {
          part = new Multipart.Part(MAIL_PARAM_REPLY_TO)
          {
            ContentType = ContentType.TEXT,
            Content = addresses.ReplyTo
          };
          parts.Add(part);
        }

        part = new Multipart.Part(MAIL_PARAM_SUBJECT)
        {
          ContentType = ContentType.TEXT,
          Content = msg.Subject
        };
        parts.Add(part);

        part = new Multipart.Part(MAIL_PARAM_TEXT)
        {
          ContentType = ContentType.TEXT,
          Content = msg.Body
        };
        parts.Add(part);

        part = new Multipart.Part(MAIL_PARAM_HTML)
        {
          ContentType = ContentType.HTML,
          Content = msg.RichBody
        };
        parts.Add(part);

        if (TestMode)
        {
          part = new Multipart.Part(API_PARAM_TESTMODE)
          {
            ContentType = ContentType.TEXT,
            Content = "yes"
          };
          parts.Add(part);
        }

        if (msg.Attachments!=null)
          foreach (var attachment in msg.Attachments.Where(a => a.Content != null))
          {
            part = new Multipart.Part(MAIL_PARAM_ATTACHMENT)
            {
              ContentType = attachment.ContentType,
              Content = attachment.Content,
              FileName = attachment.Name
            };
            parts.Add(part);
          }

        var mp = new Multipart(parts);
        var enc = mp.Encode();

        var req = WebRequest.CreateHttp(ServiceUrl);
        req.Method = "POST";
        req.ContentType = ContentType.FORM_MULTIPART_ENCODED_BOUNDARY.Args(enc.Boundary);
        req.ContentLength = enc.Length;
        var auth = new HttpBasicAuthenticationHelper(API_USER_NAME, AuthorizationKey);
        auth.AddAuthHeader(req);

        using (var reqs = req.GetRequestStream())
        {
          reqs.Write(enc.Buffer, 0, (int)enc.Length);
          using (var resp = req.GetResponse()) { }
        }
      }

      private string fmtEmail(string name, string addr)
      {
        return "{0} <{1}>".Args(name, addr);
      }

      //private void addParameter(IDictionary<string, string> parameters, string name, string value)
      //{
      //  if (parameters == null) return;
      //  if (name.IsNullOrWhiteSpace() || value.IsNullOrWhiteSpace()) return;

      //  parameters.Add(name, Uri.EscapeDataString(value));
      //}

    #endregion
  }
}
