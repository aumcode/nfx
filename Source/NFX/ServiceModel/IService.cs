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
using System.Text;

using NFX.Environment;

namespace NFX.ServiceModel
{
    /// <summary>
    /// Defines abstraction for an entity that is controlled by Start/Stop commands and has a status
    /// </summary>
    public interface IService : INamed, IConfigurable
    {
      /// <summary>
      /// Current service status
      /// </summary>
      ControlStatus Status { get;}

      /// <summary>
      /// Returns true when service is active or about to become active.
      /// Check in service implementation loops/threads/tasks
      /// </summary>
      bool Running { get;}

      /// <summary>
      /// Blocking call that starts the service instance
      /// </summary>
      void Start();

      /// <summary>
      /// Non-blocking call that initiates the stopping of the service
      /// </summary>
      void SignalStop();

      /// <summary>
      /// Non-blocking call that returns true when the service instance has completely stopped after SignalStop()
      /// </summary>
      bool CheckForCompleteStop();

      /// <summary>
      /// Blocks execution of current thread until this service has completely stopped
      /// </summary>
      void WaitForCompleteStop();
    }
}
