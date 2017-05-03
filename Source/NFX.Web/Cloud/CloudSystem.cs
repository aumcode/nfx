using System;
using System.Collections.Generic;
using System.Linq;

using NFX;
using NFX.ServiceModel;
using NFX.Environment;
using NFX.Log;
using NFX.ApplicationModel;

namespace NFX.Web.Cloud
{
  public abstract class CloudSystem : ServiceWithInstrumentationBase<object>, IWebClientCaller, ICloudSystemImplementation
  {
    #region CONSTS
    public const string CONFIG_CLOUD_SECTION = "cloud";
    public const string CONFIG_SYSTEM_SECTION = "system";
    public const string CONFIG_HOST_TEMPLATE_SECTION = "host-template";
    public const string CONFIG_AUTO_START_ATTR = "auto-start";

    private const string LOG_TOPIC = "Cloud.System";

    private const MessageType DEFAULT_LOG_LEVEL = MessageType.Warning;

    private static readonly TimeSpan INSTR_INTERVAL = TimeSpan.FromMilliseconds(4015);
    #endregion

    #region STATIC
    private static Registry<CloudSystem> s_Instances = new Registry<CloudSystem>();
    public static IRegistry<ICloudSystem> Instances { get { return s_Instances; } }

    public static void AutoStart()
    {
      App.Instance.RegisterAppFinishNotifiable(Finisher.Instance);

      foreach (var mNode in App.ConfigRoot[WebSettings.CONFIG_WEBSETTINGS_SECTION][CONFIG_CLOUD_SECTION]
                               .Children
                               .Where(cn => cn.IsSameName(CONFIG_SYSTEM_SECTION)))
      {
        if (!mNode.AttrByName(CONFIG_AUTO_START_ATTR).ValueAsBool()) continue;

        var manager = FactoryUtils.MakeAndConfigure<CloudSystem>(mNode, args: new object[] { null, mNode });

        manager.Start();
      }
    }

    public static TSystem Make<TSystem>(string name, IConfigSectionNode node)
      where TSystem : CloudSystem
    { return FactoryUtils.MakeAndConfigure<TSystem>(node, typeof(TSystem), new object[] { name, node }); }

    public static TSystem Make<TSystem>(string name, string cfgStr, string format = Configuration.CONFIG_LACONIC_FORMAT)
      where TSystem : CloudSystem
    { return Make<TSystem>(name, Configuration.ProviderLoadFromString(cfgStr, format).Root); }
    #endregion

    #region Inner
    private class Finisher : IApplicationFinishNotifiable
    {
      internal static readonly Finisher Instance = new Finisher();

      public string Name { get { return GetType().FullName; } }

      public void ApplicationFinishBeforeCleanup(IApplication application)
      {
        foreach (var manager in s_Instances)
          manager.WaitForCompleteStop();
      }

      public void ApplicationFinishAfterCleanup(IApplication application) { }
    }
    #endregion

    #region .ctor
    protected CloudSystem(string name, IConfigSectionNode node): this(name, node, null) { }

    protected CloudSystem(string name, IConfigSectionNode node, object director): base(director)
    {
      LogLevel = DEFAULT_LOG_LEVEL;

      if (node != null)
        name = node.AttrByName(Configuration.CONFIG_NAME_ATTR).Value;

      if (name.IsNullOrWhiteSpace()) name = GetType().Name;

      Name = name;

      Sessions = new List<CloudSession>();
    }

    protected override void Destructor()
    {
      DisposeAndNull(ref m_InstrumentationEvent);
      base.Destructor();
    }
    #endregion

    #region Fields
    private IConfigSectionNode m_DefaultSessionConnParamsCfg;
    private CloudConnectionParameters m_DefaultSessionConnectParams;

    private int m_WebServiceCallTimeoutMs;

    private bool m_InstrumentationEnabled;
    private Time.Event m_InstrumentationEvent;

    private Registry<CloudTemplate> m_Templates = new Registry<CloudTemplate>();
    #endregion

    #region Properties
    protected internal readonly List<CloudSession> Sessions;
    public IRegistry<CloudTemplate> Templates { get { return m_Templates; } }

    [Config(Default = false)]
    [ExternalParameter(CoreConsts.EXT_PARAM_GROUP_INSTRUMENTATION, CoreConsts.EXT_PARAM_GROUP_PAY)]
    public override bool InstrumentationEnabled
    {
      get { return m_InstrumentationEnabled; }
      set
      {
        m_InstrumentationEnabled = value;
        if (m_InstrumentationEvent == null)
        {
          if (!value) return;
          m_InstrumentationEvent = new NFX.Time.Event(App.EventTimer, null, e => AcceptManagerVisit(this, e.LocalizedTime), INSTR_INTERVAL);
        }
        else
        {
          if (value) return;
          DisposeAndNull(ref m_InstrumentationEvent);
        }
      }
    }

    [Config("default-session-connect-params")]
    public IConfigSectionNode DefaultSessionConnectParamsCfg
    {
      get { return m_DefaultSessionConnParamsCfg; }
      set
      {
        m_DefaultSessionConnectParams = MakeDefaultSessionConnectParams(value);
        m_DefaultSessionConnParamsCfg = value;
      }
    }

    /// <summary>
    /// Specifies the log level for operations performed by Pay System.
    /// </summary>
    [Config(Default = DEFAULT_LOG_LEVEL)]
    [ExternalParameter(CoreConsts.EXT_PARAM_GROUP_PAY)]
    public MessageType LogLevel { get; set; }

    [Config(Default = 20000)]
    public int WebServiceCallTimeoutMs
    {
      get { return m_WebServiceCallTimeoutMs; }
      set { m_WebServiceCallTimeoutMs = value < 0 ? 0 : value; }
    }

    [Config(Default = true)]
    public bool KeepAlive { get; set; }

    [Config(Default = true)]
    public bool Pipelined { get; set; }
    #endregion

    #region Public
    public CloudSession StartSession(CloudConnectionParameters cParams = null) { return DoStartSession(cParams ?? DefaultSessionConnectParams); }
    public void Deploy(CloudSession session, string id, CloudTemplate template, IConfigSectionNode customData) { DoDeploy(session, id, template, customData); }
    #endregion

    #region Protected
    protected CloudConnectionParameters DefaultSessionConnectParams { get { return m_DefaultSessionConnectParams; } }

    protected abstract CloudConnectionParameters MakeDefaultSessionConnectParams(IConfigSectionNode paramsSection);
    protected internal abstract CloudTemplate MakeTemplate(IConfigSectionNode node);

    protected abstract CloudSession DoStartSession(CloudConnectionParameters cParams);
    protected abstract void DoDeploy(CloudSession session, string id, CloudTemplate template, IConfigSectionNode customData);

    protected override void DoConfigure(IConfigSectionNode node)
    {
      if (node == null)
      {
        var sNode = App.ConfigRoot[WebSettings.CONFIG_WEBSETTINGS_SECTION][CONFIG_CLOUD_SECTION];
        if (!sNode.Exists) return;

        node = sNode.Children.FirstOrDefault(cn => cn.IsSameName(CONFIG_SYSTEM_SECTION) && cn.IsSameNameAttr(Name));

        if (node == null)
          node = sNode.Children.FirstOrDefault(cn => cn.IsSameNameAttr(CONFIG_SYSTEM_SECTION) && cn.AttrByName(Configuration.CONFIG_NAME_ATTR).Value.IsNullOrWhiteSpace());

        if (node == null) return;
      }

      ConfigAttribute.Apply(this, node);

      foreach (var hNode in node.Children
                                .Where(cn => cn.IsSameName(CONFIG_HOST_TEMPLATE_SECTION)))
      {
        var host = MakeTemplate(hNode);

        if (!m_Templates.Register(host))
          throw new CloudException(StringConsts.CLOUD_SYSTEM_DUPLICATE_HOST_ERROR.Args(GetType().FullName, Name));
      }
    }

    protected override void DoStart()
    {
      if (!s_Instances.Register(this))
        throw new CloudException(StringConsts.CLOUD_SYSTEM_DUPLICATE_NAME_ERROR.Args(GetType().FullName, Name));
    }

    protected override void DoWaitForCompleteStop()
    {
      s_Instances.Unregister(this);
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
    #endregion
  }
}
