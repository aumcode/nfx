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
