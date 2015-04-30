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

using System.ComponentModel;
using System.ComponentModel.Design;
using System.Windows.Forms;
using System.Windows.Forms.Design;

using NFX.RecordModel;

namespace NFX.WinForms.Views
{
  
  /// <summary>
  /// Provides record-bindable form functionality 
  /// </summary>
  public class RecordContextForm : Form, IView, IRecordContext, ISurrogateRecordTypeNameProvider, ISupportInitialize
  {
    
     #region .ctor
     public RecordContextForm()
     {
     
      m_RecordBinding = new formBinding(this);
     }
    
    #endregion


       
    
    #region Private Fields
  
     private RecordBinding<RecordContextForm> m_RecordBinding;
     private string m_AttachToRecordSurrogateTypeName;
     
    #endregion
    
    #region Properties


     /// <summary>
     /// Provides access to internal controller binding. Binding implementations depend on concrete view control 
     /// </summary>
     [Browsable(false)]
     public IBinding ControllerBinding
     {
       get { return m_RecordBinding; }
     }
     
     
     
     /// <summary>
     /// Indicates whether control is attached to a controller
     /// </summary>
     [Browsable(false)]
     public bool ControllerAttached
     {
       get { return m_RecordBinding.Attached; }
     }
    

     /// <summary>
     /// Represents a record this form control is attached to
     /// </summary>
     [Browsable(false)]
     public Record Record
     {
       get
       {
         return m_RecordBinding.Record;
       }
     }

     /// <summary>
     /// A fully-qualified type name including declaring assembly name, i.e.:
     ///  "Namespace1.Type1, TestClassesAssembly"
     /// This property is used only in design time 
     /// </summary>
     [Category(StringConsts.CONTROLLER_BINDING_CATEGORY),
      Description(
       @"A fully-qualified type name including declaring assembly name, i.e.:
         ""Namespace1.Type1, TestClassesAssembly""
         This property is used only in design time"),
      DefaultValue(""),
      DesignOnly(true),
      RefreshProperties(RefreshProperties.All)]
     public string AttachToRecordSurrogateTypeName
     {
       get
       {
         return m_AttachToRecordSurrogateTypeName;
       }
       set
       {
         if (value==null)
           value = string.Empty;
         else
           value = value.Trim();
             
         m_AttachToRecordSurrogateTypeName = value;
         
         if (Utils.IsControlDesignerHosted(this))
         {
            if (value.Length>0)
                try
                {
                 AttachController(null);
                }
                catch(Exception error)
                {
                  MessageBox.Show(string.Format(StringConsts.SURROGATE_CREATE_ERROR, value, error.Message), "RecordContextForm Designer");
                }
             else
              DetachController();   
         }  
       }
     }
     
     
    #endregion


     #region Public

     
     /// <summary>
     /// Attaches form to record controller  
     /// </summary>
     public void AttachController(Record record)
     {
       if (m_RecordBinding.Attached) m_RecordBinding.Detach();

       if (record!=null)              
        m_RecordBinding.Attach(record);
       else
        m_RecordBinding.AttachDesignTimeSurrogateRecord(m_AttachToRecordSurrogateTypeName); 

       Utils.AttachChildControlsToParentRecord(this);
     }


     /// <summary>
     /// Detaches form from controller. Does nothing if not attached
     /// </summary>
     public void DetachController()
     {
       m_RecordBinding.Detach();
       Utils.AttachChildControlsToParentRecord(this);
       
       Refresh();
     }



       #region ISupportInitialize Members

            public void BeginInit()
            {
            }

            public void EndInit()
            {
              Utils.AttachChildControlsToParentRecord(this);
            }

      #endregion
    
    #endregion


     #region Protected

     protected override void Dispose(bool disposing)
       {
         m_RecordBinding.Dispose();

         base.Dispose(disposing);
       }

     #endregion 


    #region Private Form Binding Class

    private class formBinding : RecordBinding<RecordContextForm>
    {
      public formBinding(RecordContextForm owner)
        : base(owner)
      {
      }

      protected override bool DesignTime
      {
        get
        {
          return Utils.IsControlDesignerHosted(Owner); ;
        }
      }

      protected override void ProcessNotificationsFinished()
      {
        base.ProcessNotificationsFinished();
        Owner.Invalidate();
      }
      
    }


    #endregion



    
  }
}
