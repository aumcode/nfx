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

using NFX.WinForms.Controls;

using NFX.WinForms.Elements;

namespace NFX.WinForms.Views
{

  /// <summary>
  /// FieldView control provides field controller WinForms view implementation
  /// </summary>
  [Designer(typeof(Design.FieldViewDesigner))]
  [DefaultProperty("AttachToFieldName")]
  public class FieldView : FieldContextControl
  {
    #region CONSTS
      private const int VERTICAL_PADDING = 1;
      private const int HORIZONTAL_PADDING = 1;
      private const int ELEMENT_HSPACING = 2;
      private const int ELEMENT_VSPACING = 2;
      private const int CAPTION_HINDENT = 120;
      private const string DEFAULT_FIELD_CAPTION = "Field Caption";
      
    #endregion
  
  
    #region .ctor
      public FieldView() : base ()
      {
        m_CaptionElement = new TextLabelElement(this);
        m_CaptionElement.FieldControlContext = this;
        m_CaptionElement.Text = DEFAULT_FIELD_CAPTION;
        
        m_SymbolElement = new SymbolElement(this);
        m_SymbolElement.FieldControlContext = this;
        m_SymbolElement.SymbolType = SymbolType.Diamond;
        m_SymbolElement.Visible    = false;
      }

      protected override void Dispose(bool disposing)
      {
        base.Dispose(disposing);
        HideErrorBalloon();//20120523DKh
        destroyDataEntryElement();
      }
     
    #endregion
    
    #region Private Fields
    
      private TextLabelElement  m_CaptionElement;
      private Element           m_DataEntryElement;
      private SymbolElement     m_SymbolElement;
      private InternalTextBox   m_InternalTextBox;
             
      private ControlType       m_ControlType;
      private ControlType       m_ActualControlType  = ControlType.None;
              
      private CaptionPlacement  m_CaptionPlacement;
      private StringAlignment   m_CaptionAlignment;
      private int               m_CaptionHIndent     = CAPTION_HINDENT;

      private int               m_LineCount          = 1;
      private int               m_ElementHSpacing    = ELEMENT_HSPACING;
      private int               m_ElementVSpacing    = ELEMENT_VSPACING;

      private string            m_DisplayFormat      = string.Empty;
              
      private TextHAlignment    m_TextHAlignment     = TextHAlignment.Controller;
              
      private bool              m_ComboButtonVisible = true;
              
    #endregion
    
    #region Properties
      
      /// <summary>
      /// Determines data-entry control type. When property is set to "Auto", FieldView will automatically determine
      ///  the most appropriate control type based on information supplied by attached controller
      /// </summary>
      [Category(StringConsts.VIEW_CATEGORY),
       Description(
       "Determines data-entry control type. When property is set to \"Auto\", FieldView will automatically determine"+
       " the most appropriate control type based on information supplied by attached controller"),
       DefaultValue(ControlType.Auto)]
      public ControlType ControlType
      {
         get { return m_ControlType; }
         set 
         {
           m_ControlType = value;
           rebuild();
         }
      }

      /// <summary>
      /// Actual data-entry control type. Property value is infered from field when "ControlType"  is "Auto"
      /// </summary>
      [Browsable(false)]
      public ControlType ActualControlType
      {
        get { return m_ActualControlType; }
      }

      /// <summary>
      /// Determines where caption label is placed relative to entry control
      /// </summary>
      [Category(StringConsts.VIEW_CATEGORY),
       Description("Determines where caption label is placed relative to entry control"),
       DefaultValue(CaptionPlacement.Top)]
      public CaptionPlacement CaptionPlacement
      {
        get { return m_CaptionPlacement; }
        set
        {
          m_CaptionPlacement = value;
          SetBounds(Left, Top, Width, 0); //recalc height
          Invalidate();
        }
      }

      /// <summary>
      /// Determines how caption label text is aligned
      /// </summary>
      [Category(StringConsts.VIEW_CATEGORY),
       Description("Determines how caption label text is aligned"),
       DefaultValue(StringAlignment.Near)]
      public StringAlignment CaptionAlignment
      {
        get { return m_CaptionAlignment; }
        set
        {
          m_CaptionAlignment = value;
          LayoutElements();
          Invalidate();
        }
      }


      /// <summary>
      /// Determines how text is aligned in text and combo box controls
      /// </summary>
      [Category(StringConsts.VIEW_CATEGORY),
       Description("Determines how text is aligned in text and combo box controls"),
       DefaultValue(TextHAlignment.Controller)]
      public TextHAlignment TextHAlignment
      {
        get { return m_TextHAlignment; }
        set
        {
          m_TextHAlignment = value;
          setTextBoxHAlignment(m_TextHAlignment);
        }
      }



      /// <summary>
      /// Determines how many rows of text should be occupied by entry control
      /// </summary>
      [Category(StringConsts.VIEW_CATEGORY),
       Description("Determines how many rows of text should be occupied by entry control"),
       DefaultValue(1)]
      public int LineCount
      {
        get { return m_LineCount; }
        set
        {
          m_LineCount = value<1? 1 : value;
          SetBounds(Left, Top, Width, 0); //recalc height
        }
      }


      /// <summary>
      /// Indicates whether caption is visible
      /// </summary>
      [Category(StringConsts.VIEW_CATEGORY),
       Description("Indicates whether caption is visible"),
       DefaultValue(true)]
      public bool CaptionVisible
      {
        get { return m_CaptionElement.Visible; }
        set
        {
          m_CaptionElement.Visible = value;
          Invalidate();
        }
      }


      /// <summary>
      /// Horizontal spacing between elements
      /// </summary>
      [Category(StringConsts.VIEW_CATEGORY),
       Description("Horizontal spacing between elements"),
       DefaultValue(ELEMENT_HSPACING)]
      public int ElementHSpacing
      {
        get { return m_ElementHSpacing; }
        set
        {
          m_ElementHSpacing = value;
          LayoutElements();
          Invalidate();
        }
      }

      /// <summary>
      /// Vertical spacing between elements
      /// </summary>
      [Category(StringConsts.VIEW_CATEGORY),
       Description("Vertical spacing between elements"),
       DefaultValue(ELEMENT_VSPACING)]
      public int ElementVSpacing
      {
        get { return m_ElementVSpacing; }
        set
        {
          m_ElementVSpacing = value;
          SetBounds(Left, Top, Width, 0); //recalc height
        }
      }

      /// <summary>
      /// Horizontal caption indent - a space between control edge and data-entry element reserved for caption
      /// </summary>
      [Category(StringConsts.VIEW_CATEGORY),
       Description("Horizontal caption indent - a space between control edge and data-entry element reserved for caption"),
       DefaultValue(CAPTION_HINDENT)]
      public int CaptionHIndent
      {
        get { return m_CaptionHIndent; }
        set
        {
          m_CaptionHIndent = value;
          LayoutElements();
          Invalidate();
        }
      }


      /// <summary>
      /// Provides display format string. This property when set overrides display format of attached field controller
      /// </summary>
      [Category(StringConsts.VIEW_CATEGORY),
      Description("Provides display format string. This property when set overrides display format of attached field controller"),
      DefaultValue("")]
      public string DisplayFormat
      {
        get { return m_DisplayFormat; }
        set
        {
          m_DisplayFormat = value ?? string.Empty;
        }
      }

      /// <summary>
      /// Indicates whether combo button is visible
      /// </summary>
      [Category(StringConsts.VIEW_CATEGORY),
      Description("Indicates whether combo button is visible"),
      DefaultValue(true)]
      public bool ComboButtonVisible
      {
        get 
        {
          return m_ComboButtonVisible; 
        }
        set
        {
          m_ComboButtonVisible = value;
          
          if (m_DataEntryElement!=null)
           if (m_DataEntryElement is ComboBoxElement)
            ((ComboBoxElement)m_DataEntryElement).Button.Visible = value;
          
          LayoutElements();  
        }
      }

      /// <summary>
      /// Indicates whether control is able to display multiple lines of text
      /// </summary>
      [Browsable(false)]
      public override bool Multiline { get { return m_LineCount>1; } }

    /// <summary>
    /// Determines character casing
    /// </summary>
    [Category(StringConsts.VIEW_CATEGORY),
     Description("Determines character casing")]
    public CharacterCasing CharCase { get; set; }

    #endregion
 
 
 
    
    #region Public


      public override bool Focused
      {
        get
        {
          Form form = FindForm();
          if (form != null)
          {
              if (m_InternalTextBox != null)
                return form.ActiveControl == m_InternalTextBox;
              else
                return form.ActiveControl == this;
          }
          else
          {
              if (m_InternalTextBox != null)
                return m_InternalTextBox.Focused;
              else
                return base.Focused;
          }  
        }
      }
     
     
     public override Size MinimumSize
     {
       get
       {
         return base.MinimumSize;
       }
       set
       {
         base.MinimumSize = new Size(value.Width, 0);
       }
     }


     public override Size MaximumSize
     {
       get
       {
         return base.MaximumSize;
       }
       set
       {
         base.MaximumSize = new Size(value.Width, 0);
       }
     }
    
    
     /// <summary>
     /// Invokes an appropriate lookup which can be a drop down box, popup form or other custom lookup method
     ///  as defined by an attached field controller
     /// </summary>
     public bool Lookup()
     {
            if ((!IsEnabled) || 
                (!IsApplicable) || 
                (Field == null) || 
                (this.Readonly)) return false;

             bool result = false;
             
             ComboBoxLookupForm frmHost = null;
             
             try
             {                           //todo need to re-write this
                   /*   need to redo this
                   result = Field.Lookup(
                          delegate(bool needHost, NFX.Forms.Lookup.Coordinates needSize)
                          {
                              if (needHost)
                              {
                                frmHost = new ComboBoxLookupForm();
                                
                                if (needSize.Specified) //try to size lookup dropdown
                                {
                                  int hostW = (int) needSize.X;
                                  int hostH = (int) needSize.Y;
                                  
                                  Rectangle screenBounds = Screen.GetBounds(this);
                                  
                                  if (hostW > screenBounds.Width / 2) hostW = screenBounds.Width / 2;
                                  if (hostH > screenBounds.Height / 2) hostH = screenBounds.Height / 2;  
                                  
                                  frmHost.SetBounds(0,0, hostW, hostH, BoundsSpecified.Size);
                                }
                                
                                
                                return new NFX.Forms.Lookup.HostInfo(
                                             frmHost,
                                             delegate () 
                                             { 
                                               return frmHost.ShowDropdown(this);
                                             },
                                             new NFX.Forms.Lookup.Coordinates(false, 0,0,null)
                                             );
                              }
                              else
                              {
                                return new NFX.Forms.Lookup.HostInfo(
                                             new NFX.Forms.Lookup.Coordinates(false, 0, 0,null),
                                             new NFX.Forms.Lookup.Coordinates(false, 0, 0,null)
                                             );
                              }
                               
                          }
                   
                );//lookup 
                   */
             }//try   
             finally
             {
                if (frmHost!=null) frmHost.Dispose();
             }
             
            return result;  
     }//Lookup



     /// <summary>
     /// Takes controls value from attached controller 
     /// </summary>
     public void TakeValue()
     {
        if (Field==null) return;

        bool has = Field.HasValue;


        m_TakeValueLatch = true;
        try
        {
            if (m_InternalTextBox != null)
            {
              if (has)
              {
                if (string.IsNullOrEmpty(m_DisplayFormat))
                  m_InternalTextBox.Text = Field.ValueAsDisplayString;
                else
                {
                  object val = Field.ValueAsObject;
                  m_InternalTextBox.Text = (val != null) ?
                                           string.Format("{0:" + m_DisplayFormat + "}", val) :
                                           string.Empty;
                }  
              }   
              else
                 m_InternalTextBox.Text = string.Empty;  
                 
            }//txt
            else
            if (m_ActualControlType == ControlType.Check)
            {
              string val = Field.ValueAsString.Trim().ToUpperInvariant();
               
              ((CheckBoxElement)m_DataEntryElement).Checked = ((val == "TRUE") || (val == "YES") || (val == "OK"));
            }
            else
            if (m_ActualControlType == ControlType.Radio)
            {
              RadioGroupElement rgrp = (RadioGroupElement)m_DataEntryElement;
              rgrp.CheckButtonWithKey(Field.ValueAsString);
            }
            else
            if (m_DataEntryElement is TextLabelElement)
            {
              ((TextLabelElement)m_DataEntryElement).Text = Field.ValueAsDisplayString;
            }
        }
        finally
        {
          m_TakeValueLatch = false;
        }    
     }
    
     private bool m_TakeValueLatch;
        
     /// <summary>
     /// Commits controls value into attached controller 
     /// </summary>
     public void CommitValue()
     {
         if (m_TakeValueLatch) return;
     
         if (m_InternalTextBox!=null)
         {
            if (m_FieldBinding.Field!=null)
            {
               if (m_FieldBinding.Field is StringField)
                m_FieldBinding.SetValueFromGUI(m_InternalTextBox.Text);
               else
               { 
                 string val = m_InternalTextBox.Text.Trim();
                 if (val.Length==0)
                   ClearValue();
                 else 
                   m_FieldBinding.SetValueFromGUI(val);
               }
            }
         }//txt
         else
         if (m_ActualControlType == ControlType.Check)
         {
           m_FieldBinding.SetValueFromGUI(((CheckBoxElement)m_DataEntryElement).Checked);
         }
         else
         if (m_ActualControlType == ControlType.Radio)
         {
           RadioGroupElement rgrp = (RadioGroupElement)m_DataEntryElement;
           
           object val = string.Empty;
           
           if (rgrp.CheckedElement!=null)
             if (rgrp.CheckedElement.Key != null)
                 val = rgrp.CheckedElement.Key; 
           
           m_FieldBinding.SetValueFromGUI(val);
         }
     }
    
     /// <summary>
     /// Hides error balloon if it is shown
     /// </summary>
     public void HideErrorBalloon()
      {
        if (m_ErrorBalloon != null)
        {
          m_ErrorBalloon.DisposeOnFadeOut = true;
          m_ErrorBalloon.FadeOut();
          m_ErrorBalloon.Beating = true;
          m_ErrorBalloon.DetachAnchoredControl();
          m_ErrorBalloon = null;
        }
      }
    
    
    #endregion
 
 
    
    
    #region Protected

     protected override FieldBinding CreateFieldBinding()
     {
       return new fieldViewBinding(this);
     }

     protected override bool CallDefaultPaintImplementation
     {
       get { return false; }
     }

     protected override void ReadonlyChanged()
     {
       applyReadonly();
     }
     
     
     internal Point comboLookupFormAnchorPoint()
     {
       if (m_DataEntryElement==null)
         return this.Location;
       else
       {
         Point pnt = Location;
         pnt.Offset(m_DataEntryElement.Left, m_DataEntryElement.Top);
       
         return pnt;
       }
     }

     internal int comboLookupFormElementHeight()
     {
       if (m_DataEntryElement == null)
         return 0;
       else
         return m_DataEntryElement.Height;
     }

     /// <summary>
     /// Returns height of the control taking into consideration font sizes and margins
     /// </summary>
     protected virtual int GetControlHeight()
     {
       int captionHeight = 
         (m_CaptionPlacement == CaptionPlacement.Top || m_CaptionPlacement == CaptionPlacement.Bottom)?
          FontHeight + m_ElementVSpacing : 0;

       return GetDataEntryElementHeight() + captionHeight + Padding.Vertical;   
     }


     /// <summary>
     /// Returns height of data entry element
     /// </summary>
     protected virtual int GetDataEntryElementHeight()
     {
       int adornmentsHeight = 0;
       
       switch (m_ActualControlType)
       {
         case ControlType.Check:
         {
           adornmentsHeight = BaseApplication.Theme.PartRenderer.CheckBoxMetrics.Vertical + m_ElementVSpacing * 2;
           break;
         }

         case ControlType.Radio:
         {
           adornmentsHeight = m_ElementVSpacing * 2; //this is padding of RadioGroup
           break;
         }
         
         default:
         {
           adornmentsHeight = BaseApplication.Theme.PartRenderer.TextBoxMetrics.Vertical;
           break;
         }
       
       }
       
       return (FontHeight * m_LineCount) + (m_ElementVSpacing * (m_LineCount - 1)) +  adornmentsHeight;
     }

//------------------------------------------------------------------------------------------------
          #region Layout Elements

               /// <summary>
               /// Perfoms layout of all elements a control(host) consists of
               /// </summary>
               protected virtual void LayoutElements()
               {
                 Themes.IPartRenderer renderer = BaseApplication.Theme.PartRenderer; 
                 
                 int fh = FontHeight;
                 
                 bool symbol = Required || IsReadonly;
                 
                                                   
                 Rectangle captionRegion = new Rectangle();
                 Rectangle dataEntryRegion = new Rectangle();
                 
                 switch (m_CaptionPlacement)
                 {
                   case CaptionPlacement.Left:
                       {
                         captionRegion = new Rectangle(
                            Padding.Left,
                            (Height / 2) - (fh / 2),
                            m_CaptionHIndent - m_ElementHSpacing,
                            fh
                          );

                         dataEntryRegion = new Rectangle(
                           Padding.Left + m_CaptionHIndent,
                           Padding.Top,
                           Width - m_CaptionHIndent - Padding.Horizontal,
                           GetDataEntryElementHeight());
                         
                         break;  
                       }
                   
                   case CaptionPlacement.Right:
                       {
                         captionRegion = new Rectangle(
                            Width - Padding.Right - m_CaptionHIndent + m_ElementHSpacing,
                            (Height / 2) - (fh / 2),
                            m_CaptionHIndent - m_ElementHSpacing,
                            fh
                          );

                         dataEntryRegion = new Rectangle(
                          Padding.Left,
                          Padding.Top,
                          Width - m_CaptionHIndent - Padding.Horizontal,
                          GetDataEntryElementHeight());

                         break;
                       }
                   
                   
                   case CaptionPlacement.Bottom:
                       {
                         captionRegion = new Rectangle(
                            Padding.Left,
                            Height - Padding.Bottom - fh,
                            Width - Padding.Horizontal,
                            fh
                          );


                         dataEntryRegion = new Rectangle(
                           Padding.Left,
                           Padding.Top,
                           Width - Padding.Horizontal,
                           GetDataEntryElementHeight());

                         break;
                       }
                   
                   default: /* and Top*/
                       {
                         captionRegion = new Rectangle(
                            Padding.Left,
                            Padding.Top,
                            Width - Padding.Horizontal,
                            fh
                          );
                          
                         dataEntryRegion = new Rectangle(
                            Padding.Left,
                            Padding.Top + fh + m_ElementVSpacing,
                            Width - Padding.Horizontal,
                            GetDataEntryElementHeight());
                        
                         
                         break;
                       }
                 }

                 //place caption and symbol
                 if (symbol)
                 {
                   int symW = 6;
                   int symH = 6;
                   
                   int symX;
                   int symY = (captionRegion.Top + captionRegion.Height / 2) - (symH / 2);
                   
                   
                   if (m_CaptionAlignment==StringAlignment.Far)
                   {                                     
                     symX = captionRegion.Right - symW;
                     captionRegion.Width -= symW + m_ElementHSpacing;
                   }
                   else
                   {
                     symX = captionRegion.Left;
                     if (m_CaptionAlignment!=StringAlignment.Center)
                     {
                         int delta = symW + m_ElementVSpacing;
                         captionRegion.X += delta;
                         captionRegion.Width -= delta;
                     }
                   }
                   
                   m_SymbolElement.Region = new Rectangle(symX, symY, symW, symH);
                   m_SymbolElement.Visible = true;
                 }
                 else
                   m_SymbolElement.Visible = false;
                   
                 m_CaptionElement.Region = captionRegion;
                 m_CaptionElement.Alignment = m_CaptionAlignment;                 
                 
                 
                 if (m_DataEntryElement!=null)
                 {
                   if (m_DataEntryElement is CheckBoxElement)
                   { //adjust region for checkbox
                     Point midPoint = new Point(
                         dataEntryRegion.Left + dataEntryRegion.Width/2,
                         dataEntryRegion.Top + dataEntryRegion.Height/2);
                         
                     Padding cbm = renderer.CheckBoxMetrics;
                     
                     dataEntryRegion = new Rectangle(midPoint.X - fh/2 - cbm.Left,
                                                     midPoint.Y - fh/2 - cbm.Top, 
                                                     fh + cbm.Right,
                                                     fh + cbm.Bottom); 
                   }
                                      
                   if (m_DataEntryElement is RadioGroupElement)
                   {
                     RadioGroupElement rgrp = (RadioGroupElement)m_DataEntryElement;
                     rgrp.BeginUpdate();
                      rgrp.ButtonVSpacing = m_ElementVSpacing;
                      rgrp.Padding = new Padding(m_ElementHSpacing, m_ElementVSpacing, m_ElementHSpacing, m_ElementVSpacing);
                      rgrp.Region = dataEntryRegion;
                     rgrp.EndUpdate();
                    
                     if (rgrp.UpdateCount>0)
                        rgrp.EndUpdate(); //rebuilds RadioGroup
                   }
                   else
                     m_DataEntryElement.Region = dataEntryRegion;  
                 }
                 
                 if (m_InternalTextBox != null)
                 {
               
                       if (Utils.IsControlDesignerHosted(this))
                         m_InternalTextBox.SetBounds(-1000,-1000,1,1);//adornment painter fix
                       else 
                       {
                           Padding tbm = renderer.FocusedTextBoxMetrics;
                           
                           Rectangle itbRect = new Rectangle(dataEntryRegion.Left + tbm.Left,
                                                       dataEntryRegion.Top + tbm.Top,
                                                       dataEntryRegion.Width - tbm.Horizontal,
                                                       dataEntryRegion.Height - tbm.Vertical);  
                           
                           if (m_DataEntryElement is ComboBoxElement && m_ComboButtonVisible)
                           {
                             itbRect.Width = ((int) renderer.ComboTextBoxWidthPercent * dataEntryRegion.Width) - tbm.Horizontal;
                             int btnLeft = ((ComboBoxElement)m_DataEntryElement).ButtonRegion.Left - 1;
                             
                             if ((itbRect.Left+itbRect.Width) > btnLeft)
                              itbRect.Width = btnLeft - itbRect.Left;
                             
                           }
                           
                           m_InternalTextBox.SetBounds(
                                itbRect.Left,
                                itbRect.Top,
                                itbRect.Width,
                                itbRect.Height);
                       }
                          
                        if (m_LineCount > 1)
                        {
                          m_InternalTextBox.ScrollBars = ScrollBars.Vertical;
                          m_InternalTextBox.WordWrap = true;
                        }
                        else
                        {
                          m_InternalTextBox.ScrollBars = ScrollBars.None;   
                          m_InternalTextBox.WordWrap = false;
                        }
                        m_InternalTextBox.CharacterCasing = CharCase;
                        m_InternalTextBox.BackColor = renderer.TextBoxBackgroundColor;       
                 }//m_InternalTextBox!=null
                 
               }//LayoutElements()
               
          #endregion
//------------------------------------------------------------------------------------------------

     protected override void ScaleCore(float dx, float dy)
     {
       m_ElementHSpacing = (int)(m_ElementHSpacing * dx);
       m_ElementVSpacing = (int)(m_ElementVSpacing * dy);
       m_CaptionHIndent = (int)(m_ElementVSpacing * dx);

       base.ScaleCore(dx, dy);
     }
     
     
     protected override void SetBoundsCore(int x, int y, int width, int height, BoundsSpecified specified)
     {
       base.SetBoundsCore(x, y, width, GetControlHeight(), specified | BoundsSpecified.Height);
       LayoutElements();
     }


     protected override void OnFontChanged(EventArgs e)
     {
       base.OnFontChanged(e);
       SetBounds(Left, Top, Width, 0);
     }

     protected override void OnParentFontChanged(EventArgs e)
     {
       base.OnParentFontChanged(e);
       SetBounds(Left, Top, Width, 0);
     }

     protected override void OnPaddingChanged(EventArgs e)
     {
       base.OnPaddingChanged(e);
       SetBounds(Left, Top, Width, 0);
     }

     protected override Padding DefaultPadding
     {
       get
       {
         return new Padding(HORIZONTAL_PADDING, VERTICAL_PADDING,
                            HORIZONTAL_PADDING, VERTICAL_PADDING);
       }
     }

     private Balloon m_ErrorBalloon;

     protected override void OnEnter(EventArgs e)
     {
       checkAndShowErrorBalloon();
                             
       base.OnEnter(e);
      
       if (m_InternalTextBox != null)
       {
         m_InternalTextBox.Modified = false;
         m_InternalTextBox.Show();
         if (m_InternalTextBox.CanFocus)
           BeginInvoke(new MethodInvoker(
                 delegate 
                 {
                   m_InternalTextBox.Focus();
                   m_InternalTextBox.SelectAll();
                   Refresh();
                 }
                 )); 
          
       }
       else
        BeginInvoke(new MethodInvoker(delegate { Refresh(); }) );
     }

     protected override void OnLeave(EventArgs e)
     {
       base.OnLeave(e);

       if (m_InternalTextBox != null)
       {
         m_InternalTextBox.InvalidateControlImage();
         m_InternalTextBox.Hide();
         
         if (m_InternalTextBox.Modified)
         {
           CommitValue();//from text box
           m_InternalTextBox.Modified = false;                
         }   
       }

       BeginInvoke(new MethodInvoker(delegate { Refresh(); }) );
       
       HideErrorBalloon();
       
     }


     protected override void OnKeyDown(KeyEventArgs e)
     {
       base.OnKeyDown(e);
       
       if (m_ActualControlType== ControlType.Check)
       {
         CheckBoxElement chk =  (CheckBoxElement)m_DataEntryElement;
         if (e.KeyCode == Keys.Space && !IsReadonly)
               chk.Checked = !chk.Checked;
       }
       
       if (m_InternalTextBox==null)
       {
           if (e.KeyCode == Keys.Return)
             focusNextSibling();
           else
           if (e.KeyCode == Keys.Back && e.Control && !IsReadonly)
           {
             e.Handled = true;
             e.SuppressKeyPress = true;
             ClearValue();
           }
       }  
     }
    
    
    #endregion
 
 
    
    #region Private Utils


      private void focusNextSibling()
      { 
        if (Parent!=null)
          Parent.SelectNextControl(this, true, true, true, true);
      }

      private void setInteractability()
      {
         if (!Utils.IsControlDesignerHosted(this))
         {
          ((Control)this).Visible = IsVisible;
          ((Control)this).Enabled = IsEnabled && IsApplicable;
         } 
         applyReadonly();

         bool wasSymbol = m_SymbolElement.Visible;
         bool isSymbol = Required || IsReadonly;
         
         if (isSymbol)
         {
             if (IsReadonly)
             {
               m_SymbolElement.SymbolType = SymbolType.Diamond;
               m_SymbolElement.BrushColor = Color.Blue;
               m_SymbolElement.PenColor = Color.Navy;
             }
             else
             {
               m_SymbolElement.SymbolType = SymbolType.TriangleRight;
               m_SymbolElement.BrushColor = Color.Red;
               m_SymbolElement.PenColor = Color.Maroon;
             } 
             
             
             m_SymbolElement.Visible = true;
         }
         else
           m_SymbolElement.Visible = false;
         
         if (wasSymbol != isSymbol)
            LayoutElements();
            
         if (m_InternalTextBox!=null)
          m_InternalTextBox.InvalidateControlImage();   
            
      }
           
      private void applyReadonly()
      {
        if (m_InternalTextBox!=null)
          m_InternalTextBox.ReadOnly = (Field.DataEntryType == DataEntryType.Lookup) || (this.IsReadonly);
        else
        {
          if (m_DataEntryElement != null)//make data entry-element read-only
              m_DataEntryElement.Enabled = !IsReadonly;
        
        }  
      }
      
      
      private void rebuild()
      {
        destroyDataEntryElement();

        setInteractability();

        if (Field != null)
        { //BIND
          if (Field.DataEntryType==DataEntryType.None)
           m_ActualControlType = ControlType.None;
          else 
          {
              if (m_ControlType == ControlType.Auto)
                m_ActualControlType = inferControlType();
              else
                m_ActualControlType = m_ControlType;
          }  

          m_CaptionElement.Text = Field.Description;
          if (Field.LineCount > 1)
            m_LineCount = Field.LineCount;
// TODO: Replace with lookup drill-down-navigation in future          
m_CaptionElement.IsHyperlink = true;

          createDataEntryElement();
        }
        else
        { //UNBIND
          m_ActualControlType = ControlType.None;
          m_CaptionElement.Text = DEFAULT_FIELD_CAPTION;
          m_CaptionElement.IsHyperlink = false;
        }

        TakeValue();
        SetBounds(Left, Top, Width, 0); //recalc height
      }
      
      
      private void createDataEntryElement()
      {
         switch (m_ActualControlType)
         {
           case ControlType.Check:
            {
                CheckBoxElement chk = new CheckBoxElement(this);
                
                chk.CheckedChanged += 
                        delegate(object sender, EventArgs e)
                        {
                          CommitValue();
                        };
                
                m_DataEntryElement = chk;
                break;
            }
           case ControlType.Combo:
            {
                makeTextBox();
                
                ComboBoxElement cmb = new ComboBoxElement(this, m_InternalTextBox);
                cmb.ButtonClick +=
                                delegate (object sender, EventArgs e)
                                {
                                  Lookup();                                
                                };
                                
                cmb.Button.Visible = m_ComboButtonVisible;                
                
                m_DataEntryElement = cmb;
                break;
            }
           case ControlType.Radio:
            {
                RadioGroupElement rgrp = new RadioGroupElement(this);

                rgrp.CheckedChanged +=
                              delegate(object sender, EventArgs e)
                              {
                                CommitValue();
                              };
                
                if (Field != null)
                  if (Field.LookupType==LookupType.Dictionary)
                    {
                      
                      rgrp.BeginUpdate();
                        foreach(string key in Field.LookupDictionary.Keys)
                          rgrp.AddItem(key, Field.LookupDictionary[key]);
                      rgrp.EndUpdate();
                    }
                //commented 2011.02.25    
                //if (rgrp.UpdateCount==0)
                //        rgrp.BeginUpdate();

                m_DataEntryElement = rgrp;
                break;
            }
           case ControlType.Text:
            {
                makeTextBox();
                
                             
                m_DataEntryElement = new TextBoxElement(this, m_InternalTextBox);
                break;
            }
           
           default:
           {
             m_DataEntryElement = new TextLabelElement(this);
             break;
           }
         }//switch

         m_DataEntryElement.FieldControlContext = this;
      }
      
      private void makeTextBox()
      {
              m_InternalTextBox = new InternalTextBox();
              m_InternalTextBox.Parent = this;
              m_InternalTextBox.Visible = false;
              m_InternalTextBox.Multiline = true; //otherwise id does not paint
              m_InternalTextBox.WordWrap = false;
              m_InternalTextBox.TabStop = false;

              setTextBoxHAlignment(m_TextHAlignment);             
             
             
              if (Field!=null)
              {
               applyReadonly();
               
               if (Field is StringField)
               {
                 int size = ((StringField)Field).Size;
                 if (size>0) 
                   m_InternalTextBox.MaxLength = size;
                  
                 if (((StringField) Field).Password)
                 {
                    m_InternalTextBox.PasswordChar = '*';
                    m_InternalTextBox.Multiline = false; //otherwise "*" dont show 
                 }   
               }
               
              } 

              m_InternalTextBox.Validating +=
                 delegate(object sender, CancelEventArgs e)
                 {
                   if (Field!=null)
                   {
                     if (Field.Validated && Field.KeepInErroredField && !(Field.ValidationException is RequiredValidationException))
                     {
                       e.Cancel = !Field.Valid;
                       checkAndShowErrorBalloon();
                     }
                   }
                 };

              m_InternalTextBox.KeyDown += 
                delegate(object sender, KeyEventArgs e)
                {
                    if (
                        (e.KeyCode==Keys.Return) &&
                        (m_LineCount==1 || (m_LineCount>1 && e.Control))
                       )  //emulate single-line textbox
                    {
                      e.Handled = true;
                      e.SuppressKeyPress = true;
                      
                      if ((m_ActualControlType == ControlType.Combo))
                       if (
                           (
                             (!HasValue) && (m_InternalTextBox.Text.Trim()==string.Empty) 
                           ) || (e.Shift)
                          )
                        if (!Lookup()) return;
                      
                      focusNextSibling();
                    }
                    else
                    if (e.KeyCode==Keys.Back && e.Control && !IsReadonly)
                    {
                      e.Handled = true;
                      e.SuppressKeyPress = true;
                      m_InternalTextBox.Modified = false;
                      ClearValue();  
                    }
                    else
                    if (e.KeyCode==Keys.Escape)
                      m_InternalTextBox.Undo();
                                        
                };

      }//makeTextBox

      


      

      private void destroyDataEntryElement()
      {
        if (m_DataEntryElement != null)
        {
          m_DataEntryElement.Dispose();
          m_DataEntryElement = null;
        }

        if (m_InternalTextBox != null)
        {
          m_InternalTextBox.Dispose();
          m_InternalTextBox = null;
        }


      }


      private ControlType inferControlType()
      {
        if (Field == null) 
            return ControlType.None;
       
        if (Field.DataEntryType==DataEntryType.None) 
            return ControlType.None;

        if ((Field.DataEntryType == DataEntryType.Lookup) ||
            (Field.DataEntryType == DataEntryType.DirectEntryOrLookup) ||
            (Field.DataEntryType == DataEntryType.DirectEntryOrLookupWithValidation))
        {
          if ((m_LineCount > 1) &&
              (Field.LookupDictionary.Count > 0) &&
              (Field.LookupType == LookupType.Dictionary))
            return ControlType.Radio;
          else
            return ControlType.Combo;
        }

        if (Field is BoolField)
          return ControlType.Check;

        if ((Field is DataTableField) || (Field is DataSetField))
          return ControlType.Grid;

        return ControlType.Text;
      }
    
    
      private void checkAndShowErrorBalloon()
      {
        bool error = Validated && !Valid;

        if (error)
        {
          string errTxt = "Unspecified";
          if (Field != null)
            if (Field.ValidationException != null)
              errTxt = NFX.Parsing.Utils.MakeSentenceLines(Field.ValidationException.Message);
          if (m_ErrorBalloon == null)
            m_ErrorBalloon = new Balloon(
                                 string.Format(StringConsts.ERROR_BALLOON_TEXT, m_CaptionElement.Text, errTxt),
                                 Font,
                                 this,
                                 32,
                                 Geometry.MapDirection.NorthWest,
                                 Color.Red);
          m_ErrorBalloon.FadeIn();
        }
        else 
         HideErrorBalloon();
      }
    
      


      private void setTextBoxHAlignment(TextHAlignment alignment)
      {
        if (m_InternalTextBox==null) return;
        
        switch (alignment)
        {
               case TextHAlignment.LiteralLeft:
               {
                  m_InternalTextBox.RightToLeft = RightToLeft.No;
                  m_InternalTextBox.TextAlign = HorizontalAlignment.Left;
                  break;
               }
               case TextHAlignment.LiteralRight:
               {
                  m_InternalTextBox.RightToLeft = RightToLeft.No;
                  m_InternalTextBox.TextAlign = HorizontalAlignment.Right;
                  break;
               }
               case TextHAlignment.LiteralCenter:
               {
                  m_InternalTextBox.RightToLeft = RightToLeft.No;
                  m_InternalTextBox.TextAlign = HorizontalAlignment.Center;
                  break;
               }

               case TextHAlignment.Left:
               {
                 m_InternalTextBox.RightToLeft = RightToLeft.Inherit;
                 m_InternalTextBox.TextAlign = HorizontalAlignment.Left;   
                 break;
               }

               case TextHAlignment.Right:
               {
                 m_InternalTextBox.RightToLeft = RightToLeft.Inherit;
                 m_InternalTextBox.TextAlign = HorizontalAlignment.Right;
                 break;
               }

               case TextHAlignment.Center:
               {
                 m_InternalTextBox.RightToLeft = RightToLeft.Inherit;
                 m_InternalTextBox.TextAlign = HorizontalAlignment.Center;
                 break;
               }  
               
               default: //controller
               {
                 if (Field!=null)
                 {
                   setTextBoxHAlignment((TextHAlignment)Field.DisplayTextHAlignment);
                 }
                 break;
               }
        }
        
        
      }
    
    #endregion

     //#################################################################################################################    
     #region Private FieldContextControl Binding Class

     private class fieldViewBinding : FieldBinding<FieldView>
     {
       public fieldViewBinding(FieldView owner)
         : base(owner)
       {
       }


       private bool needsData;
       private bool needsLayout;
       private bool needsInteractability;
       private bool needsRebuild;
       

       //need to write notification types to latches because multiple notifications of the same type are possible
       protected override void ProcessNotification(Notification notification)
       {
         base.ProcessNotification(notification);


         if (notification is DataChangeNotification)  needsData = true;   
         else
         if (notification is InteractabilityChangeNotification) needsInteractability = true;
         else
         if (notification is PresentationChangeNotification) needsLayout = true;
         else
         if (notification is DataEntryTypeChangeNotification) needsRebuild = true;
         else
         if (notification is ValidationNotification) needsLayout = true;         
         
       }

       //analyse latches and perform required work
       protected override void ProcessNotificationsFinished()
       {
         base.ProcessNotificationsFinished();

         if (needsInteractability)
         {
           Owner.setInteractability();
         }

         if (needsLayout)
         {
           Owner.m_CaptionElement.Text = Field.Description;
           Owner.LineCount = Field.LineCount;
           Owner.LayoutElements();
         }

         if (needsRebuild)
         {
           Owner.rebuild();
         }

         if (needsData)
         {
           Owner.TakeValue();
           if (Owner.Focused)
             Owner.checkAndShowErrorBalloon();

         }

         Owner.Invalidate(); 

         needsData = false;
         needsLayout = false;
         needsInteractability = false;
         needsRebuild = false;

       }

       protected override void BindingStatusChanged()
       {
         base.BindingStatusChanged();

         Owner.rebuild();
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
