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
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Runtime.Serialization;

using NFX.Log;
using NFX.Environment;


namespace NFX.ServiceModel
{

  /// <summary>
  /// Child service entry as managed by CompositeServiceHost class
  /// </summary>
  public struct ChildService : INamed, IOrdered
  {
    public ChildService(Service svc, int order, bool abortStart)
    {
      m_Service = svc;
      m_Order = order;
      m_AbortStart = abortStart;
    }

    private Service m_Service;
    private int m_Order;
    private bool m_AbortStart;

    public Service Service { get { return m_Service;}}
    public string Name { get { return m_Service.Name;}}
    public int Order { get { return m_Order;}}
    public bool AbortStart { get {return m_AbortStart;}}

    public override string ToString()
    {
      return "{0}Service({1})[{2}]".Args(AbortStart ? "Abortable" : "", Name, Order);
    }

  }



  /// <summary>
  /// Represents a service that contains other child services.
  /// Start/Stop commands translate into child sub-commands.
  /// This class is used to host other services in various job/background process hosts
  /// </summary>
  public class CompositeServiceHost : Service
  {
    #region CONSTS
    public const string CONFIG_SERVICE_HOST_SECTION = "service-host";
    public const string CONFIG_SERVICE_SECTION = "service";
    public const string CONFIG_ABORT_START_ATTR = "abort-start";
    public const string CONFIG_IGNORE_THIS_SERVICE_ATTR = "ignore-this-service";
    #endregion

    #region .ctor
    public CompositeServiceHost(object director) : base(director)
    {

    }

    protected override void Destructor()
    {
      base.Destructor();
      foreach (var csvc in m_Services.Where(s => s.Service.ComponentDirector == this))
        try
        {
          csvc.Service.Dispose();
        }
        catch (Exception error)
        {
          log(MessageType.Error, "Destructor('{0}')".Args(csvc), error.ToMessageWithType(), error);
        }
    }
    #endregion

    #region Fields
    private OrderedRegistry<ChildService> m_Services = new OrderedRegistry<ChildService>();
    #endregion

    #region Properties

    /// <summary>
    /// Returns service registry where services can be looked-up by name
    /// </summary>
    public IRegistry<ChildService> ChildServices { get { return m_Services; } }

    /// <summary>
    /// Returns services ordered by their order property
    /// </summary>
    public IEnumerable<ChildService> OrderedChildServices { get { return m_Services.OrderedValues; } }

    #endregion

    #region Public

    /// <summary>
    /// Returns true if child service was registered, false if it was already registered prior tp this call.
    /// The method may only be called on stopped (this) service
    /// </summary>
    public bool RegisterService(Service service, int order, bool abortStart)
    {
      this.CheckServiceInactive();
      var csvc = new ChildService(service, order, abortStart);
      return m_Services.Register(csvc);
    }

    /// <summary>
    /// Returns true if child service was unregistered, false if it did not exist.
    /// The method may only be called on stopped (this) service
    /// </summary>
    public bool UnregisterService(Service service)
    {
      this.CheckServiceInactive();
      var csvc = new ChildService(service, 0, false);
      return m_Services.Unregister(csvc);
    }

    #endregion


    #region Protected
    protected override void DoConfigure(IConfigSectionNode node)
    {
      if (node == null)
        node = App.ConfigRoot[CONFIG_SERVICE_HOST_SECTION];

      foreach (var snode in node.Children
                                .Where(cn => cn.IsSameName(CONFIG_SERVICE_SECTION))
                                .OrderBy(cn => cn.AttrByName(Configuration.CONFIG_ORDER_ATTR).ValueAsInt(0)))//the order here is needed so that child services get CREATED in order,
                                                                                                             // not only launched in order
      {
        var ignored = snode.AttrByName(CONFIG_IGNORE_THIS_SERVICE_ATTR).ValueAsBool(false);
        if (ignored)
        {
          log(MessageType.Warning, "DoConfigure()", "Service {0} is ignored".Args(snode.AttrByName("name").Value));
          continue;
        }

        var svc = FactoryUtils.MakeAndConfigure<Service>(snode, args: new object[] { this });
        var abort = snode.AttrByName(CONFIG_ABORT_START_ATTR).ValueAsBool(true);
        RegisterService(svc, snode.AttrByName(Configuration.CONFIG_ORDER_ATTR).ValueAsInt(0), abort);
      }
    }

    protected override void DoStart()
    {
      var started = new List<Service>();

      foreach (var csvc in m_Services.OrderedValues)
        try
        {
          csvc.Service.Start();
          started.Add(csvc.Service);
        }
        catch (Exception error)
        {
          var guid = log(MessageType.CatastrophicError, "DoStart('{0}')".Args(csvc), error.ToMessageWithType(), error);

          if (csvc.AbortStart)
          {
            this.AbortStart();
            foreach (var service in started)
              try
              {
                service.WaitForCompleteStop();
              }
              catch (Exception ex)
              {
                log(MessageType.CriticalAlert, "DoStart('{0}').WaitForCompleteStop('{1}')".Args(csvc, service.GetType().Name), ex.ToMessageWithType(), ex, guid);
              }
            throw new SvcHostException(StringConsts.SERVICE_COMPOSITE_CHILD_START_ABORT_ERROR.Args(csvc, error.ToMessageWithType()), error);
          }
        }
    }

    protected override void DoSignalStop()
    {
      foreach (var csvc in m_Services.OrderedValues.Reverse())
        try
        {
          csvc.Service.SignalStop();
        }
        catch (Exception error)
        {
          log(MessageType.Error, "DoSignalStop('{0}')".Args(csvc), error.ToMessageWithType(), error);
        }
    }

    protected override void DoWaitForCompleteStop()
    {
      foreach (var csvc in m_Services.OrderedValues.Reverse())
        try
        {
          csvc.Service.WaitForCompleteStop();
        }
        catch (Exception error)
        {
          log(MessageType.Error, "DoWaitForCompleteStop('{0}')".Args(csvc), error.ToMessageWithType(), error);
        }
    }

    protected override bool DoCheckForCompleteStop()
    {
      foreach (var csvc in m_Services.OrderedValues)
        try
        {
          if (!csvc.Service.CheckForCompleteStop()) return false;
        }
        catch (Exception error)
        {
          log(MessageType.Error, "DoCheckForCompleteStop('{0}')".Args(csvc), error.ToMessageWithType(), error);
        }
      return true;
    }

    protected override void DoAcceptManagerVisit(object manager, DateTime managerNow)
    {
      foreach (var csvc in m_Services.OrderedValues)
        try
        {
          csvc.Service.AcceptManagerVisit(manager, managerNow);
        }
        catch (Exception error)
        {
          log(MessageType.Error, "DoAcceptManagerVisit('{0}')".Args(csvc), error.ToMessageWithType(), error);
        }
    }

    #endregion

    #region .pvt

    private Guid log(MessageType type,
                     string from,
                     string message,
                     Exception error = null,
                     Guid? relatedMessageID = null,
                     string parameters = null)
    {
      var logMessage = new Message
      {
        Topic = StringConsts.SVCAPPLICATION_TOPIC,
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


  /// <summary>
  ///Thrown by CompositeServiceHost
  /// </summary>
  [Serializable]
  public class SvcHostException : NFXException
  {
    public SvcHostException() { }
    public SvcHostException(string message) : base(message) { }
    public SvcHostException(string message, Exception inner) : base(message, inner) { }
    protected SvcHostException(SerializationInfo info, StreamingContext context) : base(info, context) { }
  }
}
