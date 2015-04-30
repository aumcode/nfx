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
using System.ComponentModel.Design.Serialization;

using System.Drawing;
using System.Drawing.Design;
using System.Windows.Forms;
using System.Windows.Forms.Design;

using NFX.RecordModel;

using NFX.WinForms.Elements;

namespace NFX.WinForms.Views
{
  /// <summary>
  /// Provides field controller attachable base view control functionality
  /// </summary>
  [Designer(typeof(Design.FieldContextControlDesigner))]
  [DefaultProperty("AttachToFieldName")]
  public class FieldContextControl : ElementHostControl,
                                      IView,
                                      IRecordFieldContext,
                                      IParentRecordAttachable,
                                      IFieldControlContext,
                                      ISupportInitialize
  {
     #region CONSTS
      private const float DEFAULT_MARKER_INTENSITY = 0.8f;
      
    #endregion
       
    
    #region .ctor
     public FieldContextControl() : base ()
     {
      m_FieldBinding = CreateFieldBinding(); 
      
      
      
     }
    
    #endregion


     #region Private Fields
       private bool m_Initializing;
       
       private bool m_Visible = true;
       private bool m_Enabled = true; 
       private bool m_Applicable = true;
       private bool m_Readonly;
       
       
       private Field m_AttachToField;
       private Record m_AttachToRecord;
       private string m_AttachToFieldName;
       private bool m_AttachToParentRecord = true;

       private Color m_MarkerColor = Color.Yellow;
       private float m_MarkerIntensity = DEFAULT_MARKER_INTENSITY;


     #endregion


       #region Properties

      /// <summary>
      /// Indicates whether control is being initialized by a call to BeginInit()
      /// </summary>
      public bool Initializing
      {
        get { return m_Initializing; }
      }

      
      
      /// <summary>
      /// Provides access to internal controller binding. Binding implementations depend on concrete view control 
      /// </summary>
      [Browsable(false)]
      public IBinding ControllerBinding
      {
        get { return m_FieldBinding; }
      }
     
     
      /// <summary>
      /// Indicates whether control is attached to model
      /// </summary>
      [Browsable(false)]
      public bool ModelAttached
      {
       get { return m_FieldBinding.Attached; }
      }

        /// <summary>
        /// Determines whether control auto-attaches to a record specified in parent control
        /// </summary>
        [Category(StringConsts.CONTROLLER_BINDING_CATEGORY),
         Description(
          @"Determines whether control auto-attaches to a record specified in parent control"),
         DefaultValue(true),
         RefreshProperties(RefreshProperties.All)]
        public bool AttachToParentRecord
        {
          get
          {
            return m_AttachToParentRecord;
          }
          set
          {
            m_AttachToParentRecord = value;
            
            TryAttachModel();

            if (Utils.IsControlDesignerHosted(this)) 
                  Refresh();
          }
        }


        /// <summary>
        /// Specifies a field this control will be connected with
        /// </summary>
        [Category(StringConsts.CONTROLLER_BINDING_CATEGORY),
         Description(
          @"Specifies a field this control will be connected with"),
         DefaultValue(null),
         RefreshProperties(RefreshProperties.All)]
        public Field AttachToField
        {
          get
          {
            return m_AttachToField;
          }
          set
          {
            m_AttachToField = value;
            m_AttachToRecord = null;
            m_AttachToFieldName = null;

            TryAttachModel();

            if (Utils.IsControlDesignerHosted(this))
              Refresh();
          }
        }



        /// <summary>
        /// Specifies a record this control will be connected with
        /// </summary>
        [Category(StringConsts.CONTROLLER_BINDING_CATEGORY),
         Description(
          @"Specifies a record this control will be connected with"),
         RefreshProperties(RefreshProperties.All)]
        public Record AttachToRecord
        {
          get
          {
            if (m_AttachToRecord!=null)
             if (m_AttachToRecord.DesignTimeSurrogate)
               return null;

            return m_AttachToRecord;
          }
          set
          {
            m_AttachToRecord = value;
            m_AttachToField = null;

            TryAttachModel();

            if (Utils.IsControlDesignerHosted(this)) //redraw design-time artifacts
              Refresh();
          }
        }
        private bool ShouldSerializeAttachToRecord() { return !m_AttachToParentRecord && m_AttachToRecord!=null; }
    
    
      /// <summary>
      /// Specifies field by name this control will be connected with
      /// </summary>
       [Category(StringConsts.CONTROLLER_BINDING_CATEGORY),
        Description(
         @"Specifies field by name this control will be connected with"),
        DefaultValue(""),
        RefreshProperties(RefreshProperties.All),
        Editor(typeof(Design.FieldContextFieldNameEditor),typeof(UITypeEditor))]
      public string AttachToFieldName
      {
        get
        {
          return m_AttachToFieldName ?? string.Empty;
        }
        set
        {
          m_AttachToFieldName = value;
          m_AttachToField = null;
         
          TryAttachModel();

          if (Utils.IsControlDesignerHosted(this))
            Refresh(); 
        }
      }


       /// <summary>
       /// Controller field this control is connected to. May be null
       /// </summary>
       [Browsable(false)]
       public Field Field
       {
         get
         {
           return m_FieldBinding.Field;
         }
       }

       /// <summary>
       /// Controller record this control is connected to. May be null
       /// </summary>
       [Browsable(false)]
       public Record Record
       {
         get
         {            
           return m_FieldBinding.Record;
         }
       }
       
       
    /// <summary>
    /// Indicates whether this field was modified
    /// </summary>
    [Browsable(false)]
    public bool Modified
    {
      get 
      {
        if (Field!=null)
         return Field.Modified;
        else 
         return false; 
      }
    }

    /// <summary>
    /// Indicates whether this field was modified from GUI
    /// </summary>
    [Browsable(false)]
    public bool GUIModified
    {
      get
      {
        if (Field != null)
          return Field.GUIModified;
        else
          return false;
      }
    }

    /// <summary>
    /// Indicates whether field's data is valid
    /// </summary>
    [Browsable(false)]
    public bool Valid
    {
      get
      {
        if (Field != null)
          return Field.Valid;
        else
          return false;
      }
    }

    /// <summary>
    /// Indicates whether field's data was validated
    /// </summary>
    [Browsable(false)]
    public new bool Validated
    {
      get
      {
        if (Field != null)
          return Field.Validated;
        else
          return false;
      }
    }


    /// <summary>
    /// Indicates whether field was marked
    /// </summary>
    [Browsable(false)]
    public bool Marked
    {
      get
      {
        if (Field != null)
          return Field.Marked;
        else
          return false;
      }
    }

    /// <summary>
    /// Indicates whether field value is required
    /// </summary>
    [Browsable(false)]
    public bool Required
    {
      get
      {
        if (Field != null)
          return Field.Required;
        else
          return false;
      }
    }


    /// <summary>
    /// Indicates whether field value is readonly
    /// </summary>
    [Browsable(false)]
    public bool ReadonlyField
    {
      get
      {
        if (Field != null)
          return Field.Readonly;
        else
          return false;
      }
    }
    

    /// <summary>
    /// Determines whether this control is visible
    /// </summary>
    [Category(StringConsts.INTERACTIONS_CATEGORY),
     Description("Determines whether control is shown"),
     DefaultValue(true)]
    public new bool Visible
    {
      get
      {
        return m_Visible;
      }
      set
      {
        m_Visible = value;
        base.Visible = IsVisible;
      }
    }


    /// <summary>
    /// Determines whether this control is enabled
    /// </summary>
    [Category(StringConsts.INTERACTIONS_CATEGORY),
     Description("Determines whether control is enabled"),
     DefaultValue(true)]
    public new bool Enabled
    {
      get
      {
        return m_Enabled;
      }
      set
      {
        m_Enabled = value;
        base.Enabled = IsEnabled;
      }
    }
    

    /// <summary>
    /// Determines whether this control applies (can be interacted at all). Usually this property is
    /// acted upon  by an application business rules
    /// </summary>
    [Category(StringConsts.INTERACTIONS_CATEGORY),
     Description("Determines whether control applies (can be interacted at all). Usually this property is acted upon  by an application business rules"),
     DefaultValue(true)]
    public bool Applicable
    {
      get
      {
        return m_Applicable;
      }
      set
      {
        m_Applicable = value;
        base.Enabled = IsEnabled && m_Applicable;
      }
    }

    /// <summary>
    /// Determines whether control is available for data input.
    /// </summary>
    [Category(StringConsts.INTERACTIONS_CATEGORY),
     Description("Determines whether control is available for data input"),
     DefaultValue(false)]
    [Browsable(false)]
    public bool Readonly
    {
      get
      {
        return m_Readonly;
      }
      set
      {
        m_Readonly = value;
        ReadonlyChanged();
      }
    }

    /// <summary>
    /// Determines whether control is really applicable. 
    /// Item applicability is governed by parent context and may differ from instance's Applicable property 
    /// <see>Applicable</see>
    /// </summary>
    [Browsable(false)]
    public bool IsApplicable
    {
      get
      { 
        if (Field==null)
           return m_Applicable;
        else 
           return Field.IsApplicable && m_Applicable;   
      }
    }

    /// <summary>
    /// Determines whether control is really visible. 
    /// Item visibility is governed by parent context and may differ from instance's Visible property 
    /// <see>Visible</see>
    /// </summary>
    [Browsable(false)]
    public bool IsVisible
    {
      get
      {
        if (Field == null)
          return Visible;
        else
          return Field.IsVisible && m_Visible;
      }
    }

    /// <summary>
    /// Determines whether control is really enabled. 
    /// Item enabled state is governed by parent context and may differ from instance's Enabled property 
    /// <see>Enabled</see>
    /// </summary>
    [Browsable(false)]
    public bool IsEnabled
    {
      get
      {
        if (Field == null)
          return Enabled;
        else
          return Field.IsEnabled && m_Enabled;
      }
    }

    /// <summary>
    /// Determines whether control is really readonly. 
    /// Item readonly state is governed by parent context and may differ from instance's Readonly property 
    /// <see>Readonly</see>
    /// </summary>
    [Browsable(false)]
    public bool IsReadonly
    {
      get
      {
        if (Field == null)
          return m_Readonly;
        else
          return !Field.IsGUIMutable || m_Readonly; //20120525 DKh
          //return Field.IsReadonly || m_Readonly || Field.IsGUIMutable;// "OR" because readonly is a restrictive clause
      }
    }



    /// <summary>
    /// Indicates whether control is able to display multiple lines of text
    /// </summary>
    [Browsable(false)]
    public virtual bool Multiline
    {
      get
      {
        return true;
      }
    }

    /// <summary>
    /// Indicates whether a field this control is attached to has a value (aka. DB NULL/NOT NULL)
    /// </summary>
    [Browsable(false)]
    public virtual bool HasValue
    {
      get
      {
        if (Field == null)
          return false;
        else
          return Field.HasValue;
      }
    }
    

    #endregion

    #region Public

      /// <summary>
      /// Starts control initialization during which controller attachment is suspended 
      /// </summary>
      public void BeginInit()
      {
         m_Initializing = true;
      }
      
      /// <summary>
      /// Finishes control initialization and resumes controller attachment
      /// </summary>
      public void EndInit()
      {
          m_Initializing = false;
          TryAttachModel();
      }



      /// <summary>
      /// Tries to attach control to controller referenced by AttachToField property or if AttachToField is blank, by AttachToRecord/AttachToFieldName properties 
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
      /// Attaches control to controller referenced by AttachToField property or if AttachToField is blank, by AttachToRecord/AttachToFieldName properties 
      /// </summary>
      public void AttachModel()
      {
        if (m_Initializing) return;

        if (m_FieldBinding.Attached) m_FieldBinding.Detach();
          
         if (m_AttachToParentRecord)
         {
           m_AttachToRecord = Utils.FindParentRecord(this);
           
           if (m_AttachToRecord == null)
             return; //prevent exception
         } 

         if (m_AttachToField != null)
           m_FieldBinding.Attach(m_AttachToField);
         else 
           m_FieldBinding.Attach(m_AttachToRecord, m_AttachToFieldName);
           
      }
      
      /// <summary>
      /// Attaches control to a particular field controller 
      /// </summary>
      public void AttachModel(Field field)
      {
        m_AttachToField = field;
       AttachModel();
      }

      /// <summary>
      /// Attaches control to particular field controller identified by FieldName belonging to referenced Record instance
      /// </summary>
      public void AttachModel(Record record, string fieldName)
      {
        m_AttachToRecord = record;
        m_AttachToFieldName = fieldName;
        AttachModel();
      }
      
      /// <summary>
      /// Detaches controller. Does nothing if not attached
      /// </summary>
      public void DetachModel()
      {
       m_FieldBinding.Detach();
       Invalidate();
      }
      
      /// <summary> 
      /// Clears value of attached field effectively setting HasValue to false.
      /// Does nothing if no field is currently attached
      /// </summary>
      public void ClearValue()
      {
        m_FieldBinding.ClearValue();
      }
      
      
      
      /// <summary>
      /// Specifies color for text highlights when field is marked
      /// </summary>
      [Category(StringConsts.VIEW_CATEGORY),
      Description("Specifies color for text highlights when field is marked")]
      public Color MarkerColor
      {
        get
        {
          return m_MarkerColor;
        }
        set
        {
          m_MarkerColor = value;
          Invalidate();
        }
      }

      /// <summary>
      /// Specifies intenisty (marker pressure) for text highlights when field is marked
      /// </summary>
      [Category(StringConsts.VIEW_CATEGORY),
      Description("Specifies intenisty (marker pressure) for text highlights when field is marked"),
      DefaultValue(DEFAULT_MARKER_INTENSITY)]
      public float MarkerIntensity
      {
        get
        {
          return m_MarkerIntensity;
        }
        set
        {
          if (value<0) value = 0;
          else
          if (value>1) value = 1;
        
          m_MarkerIntensity = value;
          Invalidate();
        }
      }
      
      
    
    #endregion

    #region Protected

    protected internal FieldBinding m_FieldBinding;


    /// <summary>
    /// Template field binding factory method 
    /// </summary>
    protected virtual FieldBinding CreateFieldBinding()
    {
       return new controlBinding(this);
    }   
    
    protected override void Dispose(bool disposing)
    {
      m_FieldBinding.Dispose();

      base.Dispose(disposing);
    }


    protected override void OnPaint(PaintEventArgs e)
    {
      DefaultOnPaintImplementation(e);
      base.OnPaint(e);
    }


    /// <summary>
    /// Internal framework property, return false to provide custom control painting
    /// </summary>
    protected virtual bool CallDefaultPaintImplementation
    {
      get { return true; }
    }
    
    /// <summary>
    /// Override to take action when Readonly property changes
    /// </summary>
    protected virtual void ReadonlyChanged()
    {
    
    }


    /// <summary>
    /// Provides default painitng implementation
    /// </summary>
    protected void DefaultOnPaintImplementation(PaintEventArgs e)
    {
      if (!CallDefaultPaintImplementation) return;
      
      string content = "- Controller not attached -";
      string valError = string.Empty;
      bool isValid = false;
      Brush brush = Brushes.Black;

      Field fld = m_FieldBinding.Field;

      if (fld!=null)
      {
        try
        {
          content = fld.ToString();
          isValid = fld.Valid;
          if ((!isValid) && (fld.ValidationException != null))
            valError = fld.ValidationException.Message;
        }
        catch (Exception err)
        {
          content = err.Message;
          brush = Brushes.Red;
        }
      }

      BaseApplication.Theme.PartRenderer.Text(e.Graphics,
                                             ClientRectangle,
                                             false, 
                                             Font, 
                                             Brushes.Black, 
                                             this,
                                             content,
                                             StringAlignment.Center);

      if (isValid)
      {
        //TODO: Replace with validation part later
        Point[] points =
             {
                 new Point(1,  8),
                 new Point(5,  14),
                 new Point(10, 2)
             };

        e.Graphics.DrawLines(Pens.Lime, points);
      }
      else
      {
        using (Font fnt = new Font("Courier", 6, FontStyle.Bold))
        {
          BaseApplication.Theme.PartRenderer.Text(e.Graphics,
                                            ClientRectangle,
                                            false,
                                            fnt,
                                            Brushes.Black,
                                            this,
                                            valError,
                                            StringAlignment.Far);
        }
      }

    }

   
    

    #endregion
    
    
    
//#################################################################################################################    
       #region Private FieldContextControl Binding Class

                          private class controlBinding : FieldBinding<FieldContextControl>
                          {
                            public controlBinding(FieldContextControl owner)
                              : base(owner)
                            {
                            }

                           
                            protected override void ProcessNotificationsFinished()
                            {
                              base.ProcessNotificationsFinished();

                              //dont care about change type in this class, just always repaint whole control
                              Owner.Invalidate();
                            }

                            protected override void BindingStatusChanged()
                            {
                              base.BindingStatusChanged();
                              
                              Owner.Invalidate();
                            }
                            

                            protected override bool DesignTime
                            {
                              get 
                              {
                                return Utils.IsControlDesignerHosted(Owner);                              
                              }
                            }
                            
                          }


      #endregion
//#################################################################################################################        


                          
  }
  
 
  
}
