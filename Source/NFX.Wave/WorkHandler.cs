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
using System.Linq;
using System.Text;

using NFX.ApplicationModel;
using NFX.Environment;
using NFX.IO.Net.Gate;

namespace NFX.Wave
{

  /// <summary>
  /// Represents a base for all work handlers. Handlers are final work execution destination
  /// </summary>
  public abstract class WorkHandler : ApplicationComponent, INamed, IOrdered
  {
      public const string CONFIG_HANDLER_SECTION = "handler";

      protected WorkHandler(WorkDispatcher dispatcher, string name, int order, WorkMatch match) : base(dispatcher)
      {
        if (dispatcher==null)
         throw new WaveException(StringConsts.ARGUMENT_ERROR + GetType().FullName+".ctor(dispatcher==null|empty)");

        if (name.IsNullOrWhiteSpace())
          name = "{0}({1})".Args(GetType().FullName, Guid.NewGuid());

        m_Name = name;
        m_Dispatcher = dispatcher;
        m_Server = dispatcher.ComponentDirector;
        m_Order = order;
        if (match!=null)
         m_Matches.Register(match);
      }


      protected WorkHandler(WorkDispatcher dispatcher, IConfigSectionNode confNode) : base(dispatcher)
      {
        if (confNode==null||dispatcher==null)
         throw new WaveException(StringConsts.ARGUMENT_ERROR + GetType().FullName+".ctor(dispatcher|confNode==null|empty)");

        m_Dispatcher = dispatcher;
        m_Server = dispatcher.ComponentDirector;
        m_Name = confNode.AttrByName(Configuration.CONFIG_NAME_ATTR).Value;
        m_Order = confNode.AttrByName(Configuration.CONFIG_ORDER_ATTR).ValueAsInt(0);
        if (m_Name.IsNullOrWhiteSpace())
         m_Name = "{0}({1})".Args(GetType().FullName, Guid.NewGuid());


        foreach(var cn in confNode.Children.Where(cn=>cn.IsSameName(WorkFilter.CONFIG_FILTER_SECTION)))
          if(!m_Filters.Register( FactoryUtils.Make<WorkFilter>(cn, typeof(WorkFilter), args: new object[]{ this, cn })) )
            throw new WaveException(StringConsts.CONFIG_HANDLER_DUPLICATE_FILTER_NAME_ERROR.Args(cn.AttrByName(Configuration.CONFIG_NAME_ATTR).Value));

        foreach(var cn in confNode.Children.Where(cn=>cn.IsSameName(WorkMatch.CONFIG_MATCH_SECTION)))
          if(!m_Matches.Register( FactoryUtils.Make<WorkMatch>(cn, typeof(WorkMatch), args: new object[]{ cn })) )
            throw new WaveException(StringConsts.CONFIG_HANDLER_DUPLICATE_MATCH_NAME_ERROR.Args(cn.AttrByName(Configuration.CONFIG_NAME_ATTR).Value));
      }

      protected override void Destructor()
      {
        base.Destructor();
        foreach(var filter in Filters) filter.Dispose();
      }


      private string m_Name;
      private int m_Order;
      private WorkDispatcher m_Dispatcher;
      private WorkHandler m_ParentHandler; internal void ___setParentHandler(WorkHandler parent) { m_ParentHandler = parent;}
      private WaveServer m_Server;
      private OrderedRegistry<WorkMatch> m_Matches = new OrderedRegistry<WorkMatch>();
      private OrderedRegistry<WorkFilter> m_Filters = new OrderedRegistry<WorkFilter>();

      /// <summary>
      /// Returns the handler instance name
      /// </summary>
      public string Name { get{ return m_Name;}}

      /// <summary>
      /// Returns the handler order in handler registry. Order is used for URI pattern matching
      /// </summary>
      public int Order { get{ return m_Order;}}

      /// <summary>
      /// Returns matches used by this handler. May change the registry at runtime (inject/remove matches)
      /// </summary>
      public OrderedRegistry<WorkMatch> Matches { get{ return m_Matches;}}

      /// <summary>
      /// Returns ordered registry of filters
      /// </summary>
      public IRegistry<WorkFilter> Filters { get { return m_Filters;}}

      /// <summary>
      /// Returns the server that this handler works under
      /// </summary>
      public WaveServer Server { get{ return m_Server;}}

      /// <summary>
      /// Returns the dispatcher that this handler works under
      /// </summary>
      public WorkDispatcher Dispatcher { get{ return m_Dispatcher;}}

      /// <summary>
      /// Returns parent handler that this handler is under or null
      /// </summary>
      public WorkHandler ParentHandler{ get{return m_ParentHandler;}}

      /// <summary>
      /// Returns network gate that handler implementation may use to set business variables or null
      /// </summary>
      public INetGate NetGate { get{ return m_Dispatcher.ComponentDirector.Gate;}}


      /// <summary>
      /// Registers filter and returns true if the named instance has not been registered yet
      /// Note: it is possible to call this method on active server that is - inject filters while serving requests
      /// </summary>
      public bool RegisterFilter(WorkFilter filter)
      {
        if (filter==null) return false;
        if (filter.Dispatcher!=this.Dispatcher)
          throw new WaveException(StringConsts.WRONG_DISPATCHER_FILTER_REGISTRATION_ERROR.Args(filter));

        return m_Filters.Register(filter);
      }

      /// <summary>
      /// Unregisters filter and returns true if the named instance has been removed
      /// Note: it is possible to call this method on active server that is - remove filters while serving requests
      /// </summary>
      public bool UnRegisterFilter(WorkFilter filter)
      {
        if (filter==null) return false;
        if (filter.Dispatcher!=this.Dispatcher)
          throw new WaveException(StringConsts.WRONG_DISPATCHER_FILTER_UNREGISTRATION_ERROR.Args(filter));

        return m_Filters.Unregister(filter);
      }


      /// <summary>
      /// Handles the work -  first involes all filters then calls HandleWork to do actual processing
      /// Note: This method is re-entrant by multiple threads
      /// </summary>
      public void FilterAndHandleWork(WorkContext work)
      {
        var filters = m_Filters.OrderedValues.ToList().AsReadOnly();//captures context
        var filter = filters.FirstOrDefault();
        if (filter!=null)
           filter.FilterWork(work, filters, 0);
        else
           HandleWork(work);
      }

      /// <summary>
      /// Handles the work - i.e. renders a template page or calls MVC controller method.
      /// This method does not pass through handler's filters
      /// Note: This method is re-entrant by multiple threads
      /// </summary>
      public void HandleWork(WorkContext work)
      {
        try
        {
          work.m_Handler = this;
          DoHandleWork(work);
        }
        finally
        {
          work.m_Handled = true;
        }
      }


      public override string ToString()
      {
        return "{0}('{1}',#{2},{3})".Args(GetType().FullName, m_Name, m_Order, m_Matches.Count);
      }

      /// <summary>
      /// Returns true when the particular work request matches the pattern match of this handler.
      /// Also sets the WorkHandler's MatchedVars property filled with matched values
      /// </summary>
      public virtual bool MakeMatch(WorkContext work)
      {
        foreach(var match in m_Matches.OrderedValues)
        {
          var matched = match.Make(work);
          if (matched!=null)
          {
             work.___SetWorkMatch(match, matched);
             return true;
          }
        }
        return false;
      }


      /// <summary>
      /// Override to handle the work - i.e. render a template page or call MVC controller method.
      /// Note: This method is re-entrant by multiple threads
      /// </summary>
      protected abstract void DoHandleWork(WorkContext work);



  }
}
