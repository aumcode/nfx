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


/* NFX by ITAdapter
 * Originated: 2008.03
 * Revision: NFX 1.0  2011.02.06
 */
using System.Collections.Generic;

namespace NFX.RecordModel
{
  
  /// <summary>
  /// Represents view binding. This interface is used by model classes which are unaware of concrete binding
  ///  types allocated by views
  /// </summary>
  public interface IBinding
  {
      bool Attached { get; }
      IView View { get; }
      void Detach(); 
      void Notify(Notification notification);
      void NotificationsFinished();
  }


  /// <summary>
  /// Represents model-attachable view
  /// </summary>
  public interface IView
  {
    IBinding ControllerBinding { get; }
  }


  internal class Bindings : List<IBinding>
  {
  
  
  
    public void RegisterBinding(IBinding binding)
    {
       if (!Contains(binding))
        Add(binding);
    }

    public void UnRegisterBinding(IBinding binding)
    {
      Remove(binding);
    }
    
  
    public void Broadcast(Notification notification)
    {
     foreach(IBinding b in this)
               b.Notify(notification);
    } 
  
  
  
  }

}