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
using System.Collections.Generic;
using System.Linq;
using System.Text;


namespace NFX.RecordModel
{


    /// <summary>
    /// Provides an abstract base for bindings
    /// </summary>
    public abstract class BindingBase : DisposableObject, IBinding
    {
        #region .ctor
            
            private BindingBase()
            {
            }

            /// <summary>
            /// Creates an instance of binding, setting an owner reference to an instance of a concrete GUI class
            /// that is allocating this binding and what this binding provides services for
            /// </summary>
            public BindingBase(object owner)
            {
                m_Owner = owner;
            }

        #endregion


        #region Private Fields

              private object m_Owner;

              private string m_RecordTypeName = string.Empty;

        #endregion


        #region Properties

          public object Owner
          {
              get { return m_Owner; }
          }


          public IView View
          {
            get { return Owner as IView; }
          }
          

          /// <summary>
          /// Indicates whether binding is active/attached
          /// </summary>
          public bool Attached
          {
              get { return m_Attached; }
          }

          /// <summary>
          /// Indicates whether binding was created in design-time and connected to temporary record instance
          /// </summary>
          public bool DesignTimeSurrogate
          {
              get { return !string.IsNullOrEmpty(m_RecordTypeName); }
          }

          /// <summary>
          /// Represents a record this binding is attached to
          /// </summary>
          public Record Record
          {
              get { return m_Record; }
          }

          /// <summary>
          /// Provides fully-qualified record class type name which is used in design-time to extract record's metadata
          ///  like field names, types etc. A name may take a form of: "Medical.PatientRecord, BusinessObjects" where
          ///   first part before coma is a namespace-qualified record type name followed by an assembly name after coma
          /// </summary>
          public string RecordTypeName
          {
              get { return m_RecordTypeName; }
          }

        #endregion


        #region Public




          /// <summary>
          /// Sets value from view binding. This method is used to commit data from view controls into fields
          /// </summary>
          public abstract void SetValueFromGUI(object value);


          /// <summary>
          /// Attaches to a surrogate record instance. This is an internal framework design-time only method. 
          /// Throws exceptions. Not intended to be called by developers
          /// </summary>
          public void AttachDesignTimeSurrogateRecord(string recordTypeName)
          {
            if (!DesignTime) return; 
            
            if (m_Attached) Detach();
            
            if (!string.IsNullOrEmpty(recordTypeName))
            {
                      Type t = Type.GetType(recordTypeName);
                   
                      if (t != null)
                      {
                            Record rec = Record.Make(t);
                            if (rec!=null)
                            {
                              rec._markAsSurrogate(); 
                              m_Record = rec;
                              m_RecordTypeName = recordTypeName;
                              m_Record.RegisterBinding(this);
                              m_Attached = true;
                            }
                      }
            }
            
            if (!m_Attached)
              throw new RecordModelException(StringConsts.SURROGATE_LOAD_ERROR);
            
            BindingStatusChanged();
          }
          
          
          /// <summary>
          /// Unbinds a binding from whatever entity it is bound to
          /// </summary>
          public void Detach()
          {
             if (m_Attached)
             {
                DetachBinding();
                m_Attached = false;
                m_RecordTypeName = string.Empty;
                m_Record = null;
                
                BindingStatusChanged();
             }
          }

          /// <summary>
          /// Notifies binding with a notification message of a certain type
          /// </summary>
          public void Notify(Notification notification)
          {
              ProcessNotification(notification);
          }

          /// <summary>
          /// Notifies binding that all notifications have been sent and it is time for control to take appropriate actions.
          /// This method may be useful for controls that should wait with any repaints/layout changes until all
          ///  notifications have arrived i.e.: "DataChange" may arrive first then other notification
          ///  may arrive that can require full control repaint once again. By implementing protected ProcessNotificationsFinished()
          ///  control's binding can respond to different notification kinds post-factum when a more intelligent
          ///   composition/painting decision can be made.
          /// </summary>
          public void NotificationsFinished()
          {
              ProcessNotificationsFinished();
          }


        #endregion


        #region Protected

            /// <summary>
            /// A reference to attached record
            /// </summary>
            internal Record m_Record;

            /// <summary>
            /// Indicates whether binding is attached
            /// </summary>
            internal bool m_Attached;


            /// <summary>
            /// Destroys an instance of binding. Overrides DisposableObject.Destructor()
            /// </summary>
            protected override void Destructor()
            {
              Detach();
              base.Destructor();
            }
            
            
            
            /// <summary>
            /// Determines whether owner is being designed
            /// </summary>
            protected abstract bool DesignTime { get; }



            /// <summary>
            /// Unbinds a binding from whatever entity it is bound to 
            /// </summary>
            protected virtual void DetachBinding()
            {
                if (m_Record != null)
                    m_Record.UnRegisterBinding(this);
            }

            /// <summary>
            /// Override in derived binding class to take action when all notifications have been sent and it is time for control to take appropriate actions.
            /// This method may be useful for controls that should wait with any repaints/layout changes until all
            ///  notifications have arrived i.e.: "DataChange" may arrive first then other notification
            ///  may arrive that can require full control repaint once again. By implementing ProcessNotificationsFinished()
            ///  control's binding can respond to different notification kinds post-factum when a more intelligent
            ///   composition/painting decision can be made.
            /// </summary>
            protected virtual void ProcessNotification(Notification notification)
            {

            }

            /// <summary>
            /// Called when all messages arrived
            /// </summary>
            protected virtual void ProcessNotificationsFinished()
            {

            }


            /// <summary>
            /// Override in derived classes to take action after binding was attached or detached
            /// </summary>
            protected virtual void BindingStatusChanged()
            {
            
            }


        #endregion


        #region Private Utils
       
        #endregion


    }









}
