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
using System.IO;
using System.Net;
using System.Threading;

using NFX.Log;
using NFX.Collections;
using NFX.Environment;
using NFX.ServiceModel;

namespace NFX.Wave
{
  /// <summary>
  /// Represents a default dispatcher that dispatches WorkContext calls on the same thread that calls Dispatch(work).
  /// May extend this class to implement custom dispatchers, i.e. the once that maintain their won work queue/worker threads
  /// </summary>
  public class WorkDispatcher : Service<WaveServer>
  {
      public WorkDispatcher(WaveServer director) : base(director)
      {

      }

      protected override void Destructor()
      {
        base.Destructor();
        foreach(var filter in Filters) filter.Dispose();
        foreach(var handler in Handlers) handler.Dispose();
      }


      #region Fields

        private OrderedRegistry<WorkFilter> m_Filters = new OrderedRegistry<WorkFilter>();
        private OrderedRegistry<WorkHandler> m_Handlers = new OrderedRegistry<WorkHandler>();

      #endregion

      #region Properties

        /// <summary>
        /// Returns ordered registry of filters
        /// </summary>
        public IRegistry<WorkFilter> Filters { get { return m_Filters;}}


        /// <summary>
        /// Returns ordered registry of handlers
        /// </summary>
        public IRegistry<WorkHandler> Handlers { get { return m_Handlers;}}

      #endregion

      #region Public

        /// <summary>
        /// Registers filter and returns true if the named instance has not been registered yet
        /// Note: it is possible to call this method on active server that is - inject filters while serving requests
        /// </summary>
        public bool RegisterFilter(WorkFilter filter)
        {
          if (filter==null) return false;
          if (filter.Dispatcher!=this)
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
          if (filter.Dispatcher!=this)
            throw new WaveException(StringConsts.WRONG_DISPATCHER_FILTER_UNREGISTRATION_ERROR.Args(filter));

          return m_Filters.Unregister(filter);
        }

        /// <summary>
        /// Registers handler and returns true if the named instance has not been registered yet
        /// Note: it is possible to call this method on active server that is - inject handlers while serving requests
        /// </summary>
        public bool RegisterHandler(WorkHandler handler)
        {
          if (handler==null) return false;
          if (handler.Dispatcher!=this)
            throw new WaveException(StringConsts.WRONG_DISPATCHER_HANDLER_REGISTRATION_ERROR.Args(handler));

          return m_Handlers.Register(handler);
        }

        /// <summary>
        /// Unregisters handler and returns true if the named instance has been removed
        /// Note: it is possible to call this method on active server that is - remove handlers while serving requests
        /// </summary>
        public bool UnRegisterHandler(WorkHandler handler)
        {
          if (handler==null) return false;
          if (handler.Dispatcher!=this)
            throw new WaveException(StringConsts.WRONG_DISPATCHER_HANDLER_UNREGISTRATION_ERROR.Args(handler));

          return m_Handlers.Unregister(handler);
        }





        /// <summary>
        /// Dispatches work into appropriate handler passing through filters.
        /// The default implementation processes requests on the calling thread and disposes WorkContext deterministically
        /// </summary>
        public virtual void Dispatch(WorkContext work)
        {
          WorkFilter filter = null;
          WorkHandler handler = null;
          try
          {
            var filters = m_Filters.OrderedValues.ToList().AsReadOnly();//captures context
            filter = filters.FirstOrDefault();
            if (filter!=null)
             filter.FilterWork(work, filters, 0);
            else
             InvokeHandler(work, out handler);
          }
          catch(Exception error)
          {
            work.LastError = error;
            HandleException(work, filter, handler, error);
          }
          finally
          {
            try
            {
               if (!work.NoDefaultAutoClose)
                  work.Dispose();
            }
            catch(Exception error)
            {
                HandleException(null, null, null, error);
            }
          }
        }

        /// <summary>
        /// Finds appropriate handler and invokes it. Returns the appropriate handler or null if work was aborted or already handled.
        /// Throws if appropriate handler was not found
        /// </summary>
        public virtual void InvokeHandler(WorkContext work, out WorkHandler handler)
        {
          if (!work.Aborted && !work.Handled)
          {
             handler = GetWorkHandler(work);

             if (handler==null)
              throw new WaveException(StringConsts.NO_HANDLER_FOR_WORK_ERROR.Args(work.About));

             handler.FilterAndHandleWork(work);
             return;
          }
          handler = null;
        }

        /// <summary>
        /// Finds the most appropriate work handler to do the work.
        /// The default implementation finds first handler with matching URI pattern or null
        /// </summary>
        public virtual WorkHandler GetWorkHandler(WorkContext work)
        {
          return m_Handlers.OrderedValues.FirstOrDefault(handler => handler.MakeMatch(work));
        }

        /// <summary>
        /// Handles processing exception - this implementation uses server-wide behavior.
        /// All parameters except ERROR can be null - which indicates error that happened during WorkContext dispose
        /// </summary>
        public virtual void HandleException(WorkContext work, WorkFilter filter, WorkHandler handler,  Exception error)
        {
          ComponentDirector.HandleException(work, filter, handler, error);
        }

      #endregion

      #region Protected

        protected override void DoConfigure(IConfigSectionNode node)
        {
          base.DoConfigure(node);

          m_Filters = new OrderedRegistry<WorkFilter>();//clear existing
          foreach(var fNode in node.Children.Where(cn=>cn.IsSameName(WorkFilter.CONFIG_FILTER_SECTION)))
            if(!m_Filters.Register( FactoryUtils.Make<WorkFilter>(fNode, args: new object[] {this, fNode})))
             throw new WaveException(StringConsts.CONFIG_DUPLICATE_FILTER_NAME_ERROR.Args(fNode.AttrByName(Configuration.CONFIG_NAME_ATTR).Value));

          m_Handlers = new OrderedRegistry<WorkHandler>();//clear existing
          foreach(var hNode in node.Children.Where(cn=>cn.IsSameName(WorkHandler.CONFIG_HANDLER_SECTION)))
            if(!m_Handlers.Register( FactoryUtils.Make<WorkHandler>(hNode, args: new object[] {this, hNode})))
             throw new WaveException(StringConsts.CONFIG_DUPLICATE_HANDLER_NAME_ERROR.Args(hNode.AttrByName(Configuration.CONFIG_NAME_ATTR).Value));

        }

      #endregion

  }




}
