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
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace NFX.RecordModel
{

  /// <summary>
  /// Represents a list of notifications
  /// </summary>
  [Serializable]
  internal class ChangeNotifications : IEnumerable<Notification>  //20130228 DKh - hid the List
  {

    private List<Notification> m_List = new List<Notification>();
    
    
    /// <summary>
    /// Add notification to the list ensuring notification-type uniqueness
    /// </summary>
    public void AddNotification(Notification notification)
    {
      Notification foundExisting = null;
      
      foreach(Notification existing in this)
       if (object.ReferenceEquals(existing.GetType(),notification.GetType()))
       {
        foundExisting = existing;
        break;
       }
       
      if (foundExisting==null)
      {
       m_List.Add(notification);
      }
    }

    public void ClearNotifications()
    {
      m_List.Clear();
    }

    /// <summary>
    /// Combines notifications with other source ensuring notification-type uniqueness
    /// </summary>
    public void MergeNotifications(ChangeNotifications otherNotifications)
    {
       foreach (Notification other in otherNotifications)
        this.AddNotification(other);
    }


    public IEnumerator<Notification> GetEnumerator()
    {
        return m_List.GetEnumerator();
    }

    IEnumerator IEnumerable.GetEnumerator()
    {
        return m_List.GetEnumerator();
    }
  }



  /// <summary>
  /// Represents a notification message sent from controllers to views through bindings
  /// </summary>
  [Serializable]
  public class Notification
  {
    #region .ctor
      private Notification()
      {
      }
      
      
      public Notification(ModelBase sender)
      {
        m_Sender = sender;
      }
    #endregion 
    
    #region Private Fields
     private ModelBase m_Sender;
    #endregion
    
    #region Properties
    
     public ModelBase Sender
     {
       get { return m_Sender; }
     }
    
    #endregion
    
  }


  [Serializable]
  public class DataChangeNotification : Notification 
  {
   public DataChangeNotification(ModelBase sender) : base(sender) { }  
  }
  
  [Serializable]
  public class FieldDataChangeNotification : DataChangeNotification 
  {
   public FieldDataChangeNotification(ModelBase sender) : base(sender) { }  
  } 


  /// <summary>
  /// Sent when Enabled/Visible/Readonly/Applicable change
  /// </summary>
  [Serializable]
  public class InteractabilityChangeNotification : Notification 
  {
   public InteractabilityChangeNotification(ModelBase sender) : base(sender) { }  
  }

  /// <summary>
  /// Sent when Description, Marked and other change that require presentation change and view repaint
  /// </summary>
  [Serializable]
  public class PresentationChangeNotification : Notification
  {
    public PresentationChangeNotification(ModelBase sender) : base(sender) { } 
  }

  /// <summary>
  /// Sent when data-entry method changes and attached view needs to be rebuilt
  /// </summary>
  [Serializable]
  public class DataEntryTypeChangeNotification : Notification
  {
    public DataEntryTypeChangeNotification(ModelBase sender) : base(sender) { }
  }


  /// <summary>
  /// Sent when model was validated, model may or may not be Valid after validation
  /// </summary>
  [Serializable]
  public class ValidationNotification : Notification
  {
    public ValidationNotification(ModelBase sender) : base(sender) { }
  }
  
}
