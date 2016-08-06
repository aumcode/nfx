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

using NFX;
using NFX.Environment;

namespace NFX.Wave.Handlers
{
  /// <summary>
  /// Dispatched work to sub-handlers just like dispatcher does
  /// </summary>
  public sealed class CompositeHandler : WorkHandler
  {
    #region .ctor
         public CompositeHandler(WorkDispatcher dispatcher, string name, int order, WorkMatch match) : base(dispatcher, name, order, match)
         {

         }

         public CompositeHandler(WorkDispatcher dispatcher, IConfigSectionNode confNode) : base(dispatcher, confNode)
         {
            foreach(var hNode in confNode.Children.Where(cn=>cn.IsSameName(WorkHandler.CONFIG_HANDLER_SECTION)))
            {
             var sub = FactoryUtils.Make<WorkHandler>(hNode, args: new object[] {dispatcher, hNode});
             sub.___setParentHandler( this );
             sub.__setComponentDirector( this );
             if(!m_Handlers.Register(sub))
              throw new WaveException(StringConsts.CONFIG_DUPLICATE_HANDLER_NAME_ERROR.Args(hNode.AttrByName(Configuration.CONFIG_NAME_ATTR).Value));
            }

         }
    #endregion


    #region Fields

         private OrderedRegistry<WorkHandler> m_Handlers = new OrderedRegistry<WorkHandler>();

    #endregion

    #region Properties

        /// <summary>
        /// Returns ordered registry of handlers
        /// </summary>
        public IRegistry<WorkHandler> Handlers { get { return m_Handlers;}}

    #endregion

    #region Public
       /// <summary>
        /// Registers handler and returns true if the named instance has not been registered yet
        /// Note: it is possible to call this method on active server that is - inject handlers while serving requests
        /// </summary>
        public bool RegisterHandler(WorkHandler handler)
        {
          if (handler==null) return false;
          if (handler.Dispatcher!=this.Dispatcher)
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
          if (handler.Dispatcher!=this.Dispatcher)
            throw new WaveException(StringConsts.WRONG_DISPATCHER_HANDLER_UNREGISTRATION_ERROR.Args(handler));

          return m_Handlers.Unregister(handler);
        }

    #endregion

    #region Protected

      protected override void DoHandleWork(WorkContext work)
      {
        var subHandler = m_Handlers.OrderedValues.FirstOrDefault(handler => handler.MakeMatch(work));

        if (subHandler==null)
              throw new WaveException(StringConsts.NO_HANDLER_FOR_WORK_ERROR.Args(work.About));

        subHandler.FilterAndHandleWork(work);
      }

    #endregion

  }

}
