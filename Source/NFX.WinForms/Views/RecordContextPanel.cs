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
  /// Provides record-bindable panel functionality
  /// </summary>
  [Designer(typeof(Design.RecordContextPanelDesigner))]
  [DefaultProperty("AttachToRecord")]
  public class RecordContextPanel : Panel, IView, IRecordContext,IParentRecordAttachable, ISurrogateRecordTypeNameProvider, ISupportInitialize
  {
    
    #region .ctor
     public RecordContextPanel()
     {
     
      m_RecordBinding = new panelBinding(this);
     }
    
    #endregion
    
    
    #region Private Fields
    
     private RecordBinding<RecordContextPanel> m_RecordBinding;

     private Record m_AttachToRecord;
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
     /// Specifies a record this control will be connected with
     /// </summary>
     [Category(StringConsts.CONTROLLER_BINDING_CATEGORY),
      Description(
       @"Specifies a record this control will be connected with"),
      DefaultValue(null),
      RefreshProperties(RefreshProperties.All)]
     public Record AttachToRecord
     {
       get
       {
         return m_AttachToRecord;
       }
       set
       {
         m_AttachToRecord = value;
         m_AttachToRecordSurrogateTypeName = string.Empty;

         TryAttachModel();

         if (Utils.IsControlDesignerHosted(this))
           Refresh();
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
         if (value == null)
           value = string.Empty;
         else
           value = value.Trim();
         
         m_AttachToRecordSurrogateTypeName = value;

         if (Utils.IsControlDesignerHosted(this))
         {
           if (value.Length > 0)
             try
             {
               AttachModel(null);
             }
             catch (Exception error)
             {
               MessageBox.Show(string.Format(StringConsts.SURROGATE_CREATE_ERROR, value, error.Message), "RecordContextPanel Designer");
             }
           else
             DetachModel();
             
          Refresh();   
         }
       }
     }

     /// <summary>
     /// Controller field this control is connected to. May be null
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
     /// Determines whether panel is attached to its parent record
     /// </summary>
     [Browsable(false)]
     public bool AttachToParentRecord
     {
       get
       {
         return m_AttachToRecord == null;
       }
     }

    #endregion
    
    
    #region Public

     /// <summary>
     /// Tries to attach control to model referenced by AttachToRecord property 
     /// </summary>
     public bool TryAttachModel()
     {
       try
       {
         AttachModel();
         return true;
       }
       catch
       {
         return false;
       }
     }


     /// <summary>
     /// Attaches control to model referenced by AttachToRecord property 
     /// </summary>
     public void AttachModel()
     {
       try
       {
           if (m_RecordBinding.Attached) m_RecordBinding.Detach();
           
           
           if (Utils.IsControlDesignerHosted(this) && (m_AttachToRecord == null))
           {    
                 try
                 {
                   m_RecordBinding.AttachDesignTimeSurrogateRecord(this.m_AttachToRecordSurrogateTypeName);
                 }
                 catch { }
           }      
           else
           { 
              m_RecordBinding.Attach(m_AttachToRecord);
           }
       }
       finally
       {
         Utils.AttachChildControlsToParentRecord(this);
       }
     }

     /// <summary>
     /// Attaches control to a particular record model 
     /// </summary>
     public void AttachModel(Record record)
     {
       m_AttachToRecord = record;
       AttachModel();
     }


     /// <summary>
     /// Detaches model. Does nothing if not attached
     /// </summary>
     public void DetachModel()
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
   
       
    
    #region Private Panel Binding Class

    private class panelBinding : RecordBinding<RecordContextPanel>
    {
      public panelBinding(RecordContextPanel owner) : base(owner)
      {
      }

      protected override bool DesignTime
      {
        get
        {
          return Utils.IsControlDesignerHosted(Owner);
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
