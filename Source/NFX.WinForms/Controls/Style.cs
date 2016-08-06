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
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using System.Drawing;
using System.Drawing.Drawing2D;

namespace NFX.WinForms.Controls
{
  /// <summary>
  /// Defines a style used for painting and layout
  /// </summary>
  [TypeConverter(typeof(ExpandableObjectConverter))]
  [Themable]
  public class Style
  {
      #region .ctor

         /// <summary>
         /// Creates a style in a context of control and parent. If an attribute is not defined in this instance it is tried to be fetched from parent.
         /// If parent is not available then control context is used if it is not null for some properties like Font and BGColor. Both context and parent may be null
         /// </summary>
         public Style(Control contextCtl, Style parent)
         {
           m_ContextControl = contextCtl;
           m_Parent = parent;
         }

      #endregion

      #region Private Fields

        private Control m_ContextControl;
        private Style m_Parent;

        private HAlignment? m_HAlignment;

        private BGKind?     m_BGKind;
        private Color?      m_BGColor;
        private Color?      m_BGColor2;
        private Color?      m_BGHatchColor;
        private HatchStyle? m_BGHatchStyle;

        private Padding?    m_Padding;


        private LineStyle? m_BorderLeft;
        private LineStyle? m_BorderRight;
        private LineStyle? m_BorderTop;
        private LineStyle? m_BorderBottom;

        private Color? m_ForeColor;
        private Brush  m_ForeBrush;
        private Pen    m_ForePen;
        private Brush  m_BackBrush;

        private Font   m_Font;

      #endregion


      #region Properties

        /// <summary>
        /// Returns a context/control that this style is in. May be null
        /// </summary>
        [Browsable(false)]
        public Control ControlContext
        {
          get { return m_ContextControl; }
        }


        /// <summary>
        /// Returns parent style definition or null
        /// </summary>
        [Browsable(false)]
        public Style Parent
        {
          get { return m_Parent; }
          internal set { m_Parent = value;}
        }





        /// <summary>
        /// Defines text horizontal alignment. Call corresponding Clear method to delete attribute in this instance
        /// </summary>
        [Description("Defines text horizontal alignment")]
        public HAlignment HAlignment
        {
          get
          {
            if (m_HAlignment.HasValue) return m_HAlignment.Value;
            if (m_Parent!=null) return m_Parent.HAlignment;
            return HAlignment.Left;
          }
          set {m_HAlignment = value;  notify(); }
        }

        public void ClearHAlignment(){ m_HAlignment = null; notify(); }




        /// <summary>
        /// Defines background kind. Call corresponding Clear method to delete attribute in this instance
        /// </summary>
        [Description("Defines background kind")]
        public BGKind BGKind
        {
          get
          {
            if (m_BGKind.HasValue) return m_BGKind.Value;
            if (m_Parent!=null) return m_Parent.BGKind;
            return BGKind.Solid;
          }
          set {m_BGKind = value; notify(); }
        }

        public void ClearBGKind(){ m_BGKind = null; notify(); }


        /// <summary>
        /// Defines background color. Call corresponding Clear method to delete attribute in this instance
        /// </summary>
        [Description("Defines background color")]
        public Color BGColor
        {
          get
          {
            if (m_BGColor.HasValue) return m_BGColor.Value;
            if (m_Parent!=null)     return m_Parent.BGColor;
            return (m_ContextControl==null) ? SystemColors.Window : m_ContextControl.BackColor;
          }
          set { m_BGColor = value; m_BackBrush = new SolidBrush(value); notify(); }
        }

        public void ClearBGColor(){ m_BGColor = null; m_BackBrush = null; notify(); }

        [Browsable(false)]
        public Brush BackBrush
        {
          get
          {
            if (m_BackBrush!=null)    return m_BackBrush;
            if (m_Parent!=null)       return m_Parent.BackBrush;
            return SystemBrushes.Window;
          }
        }

        /// <summary>
        /// Defines second background color used by gradients. Call corresponding Clear method to delete attribute in this instance
        /// </summary>
        [Description("Defines second background color used by gradients")]
        public Color BGColor2
        {
          get
          {
            if (m_BGColor2.HasValue) return m_BGColor2.Value;
            if (m_Parent!=null) return m_Parent.BGColor2;
            return Color.White;
          }
          set {m_BGColor2 = value; notify(); }
        }

        public void ClearBGColor2(){ m_BGColor2 = null; notify(); }



        /// <summary>
        /// Defines background hatch color. Call corresponding Clear method to delete attribute in this instance
        /// </summary>
        [Description("Defines background hatch color")]
        public Color BGHatchColor
        {
          get
          {
            if (m_BGHatchColor.HasValue) return m_BGHatchColor.Value;
            if (m_Parent!=null) return m_Parent.BGHatchColor;
            return BGColor;
          }
          set {m_BGHatchColor = value; notify(); }
        }

        public void ClearBGHatchColor(){ m_BGHatchColor = null; notify(); }


        /// <summary>
        /// Defines background hatch style. Call corresponding Clear method to delete attribute in this instance
        /// </summary>
        [Description("Defines background hatch style")]
        public HatchStyle BGHatchStyle
        {
          get
          {
            if (m_BGHatchStyle.HasValue) return m_BGHatchStyle.Value;
            if (m_Parent!=null) return m_Parent.BGHatchStyle;
            return HatchStyle.SmallGrid;
          }
          set {m_BGHatchStyle = value; notify(); }
        }

        public void ClearBGHatchStyle(){ m_BGHatchStyle = null; notify(); }






        /// <summary>
        /// Defines padding - an extra space inside of container. Call corresponding Clear method to delete attribute in this instance
        /// </summary>
        [Description("Defines padding - an extra space inside of container")]
        public Padding Padding
        {
          get
          {
            if (m_Padding.HasValue) return m_Padding.Value;
            if (m_Parent!=null) return m_Parent.Padding;
            return new Padding(4);
          }
          set {m_Padding = value; notify(); }
        }

        public void ClearPadding(){ m_Padding = null; notify(); }




        /// <summary>
        /// Defines left border style. Call corresponding Clear method to delete attribute in this instance
        /// </summary>
        [Description("Defines left border style")]
        public LineStyle BorderLeft
        {
          get
          {
            if (m_BorderLeft.HasValue) return m_BorderLeft.Value;
            if (m_Parent!=null) return m_Parent.BorderLeft;
            return LineStyle.DefaultNone;
          }
          set {m_BorderLeft = value; notify(); }
        }

        public void ClearBorderLeft(){ m_BorderLeft = null; notify(); }


        /// <summary>
        /// Defines right border style. Call corresponding Clear method to delete attribute in this instance
        /// </summary>
        [Description("Defines right border style")]
        public LineStyle BorderRight
        {
          get
          {
            if (m_BorderRight.HasValue) return m_BorderRight.Value;
            if (m_Parent!=null) return m_Parent.BorderRight;
            return LineStyle.DefaultGray;
          }
          set {m_BorderRight = value; notify(); }
        }

        public void ClearBorderRight(){ m_BorderRight = null; notify(); }



        /// <summary>
        /// Defines top border style. Call corresponding Clear method to delete attribute in this instance
        /// </summary>
        [Description("Defines top border style")]
        public LineStyle BorderTop
        {
          get
          {
            if (m_BorderTop.HasValue) return m_BorderTop.Value;
            if (m_Parent!=null) return m_Parent.BorderTop;
            return LineStyle.DefaultNone;
          }
          set {m_BorderTop = value; notify(); }
        }

        public void ClearBorderTop(){ m_BorderTop = null; notify(); }


        /// <summary>
        /// Defines bottom border style. Call corresponding Clear method to delete attribute in this instance
        /// </summary>
        [Description("Defines bottom border style")]
        public LineStyle BorderBottom
        {
          get
          {
            if (m_BorderBottom.HasValue) return m_BorderBottom.Value;
            if (m_Parent!=null) return m_Parent.BorderBottom;
            return LineStyle.DefaultGray;
          }
          set {m_BorderBottom = value; notify(); }
        }

        public void ClearBorderBottom(){ m_BorderBottom = null; notify(); }

        /// <summary>
        /// Defines foreground color. Call corresponding Clear method to delete attribute in this instance
        /// </summary>
        [Description("Defines foreground color")]
        public Color ForeColor
        {
          get
          {
            if (m_ForeColor.HasValue) return m_ForeColor.Value;
            if (m_Parent!=null)       return m_Parent.ForeColor;
            return SystemColors.WindowText;
          }
          set { m_ForeColor = value; m_ForeBrush = new SolidBrush(value); m_ForePen = new Pen(value); notify(); }
        }

        public void ClearForeColor()
        {
          if (m_ForeBrush != null) m_ForeBrush.Dispose();
          if (m_ForePen   != null) m_ForePen.Dispose();
          m_ForeColor      = null;
          m_ForeBrush      = null;
          m_ForePen        = null;
          notify();
        }

        [Browsable(false)]
        public Brush ForeBrush
        {
          get
          {
            if (m_ForeBrush != null) return m_ForeBrush;
            if (m_Parent    != null) return m_Parent.ForeBrush;
            return SystemBrushes.WindowText;
          }
        }

        [Browsable(false)]
        public Pen ForePen
        {
          get
          {
            if (m_ForePen != null) return m_ForePen;
            if (m_Parent  != null) return m_Parent.ForePen;
            return SystemPens.WindowText;
          }
        }

        /// <summary>
        /// Defines font information. Null is never returned but may be set so it resets attribute in this instance
        /// </summary>
        public Font Font
        {
          get
          {
            if (m_Font!=null) return m_Font;
            if (m_Parent!=null) return m_Parent.Font;
            if (m_ContextControl!=null)
             return m_ContextControl.Font;
            else
             return new Font("Verdana", 10f);
          }
          set { m_Font = value;notify(); }
        }

      #endregion

      #region Public Methods

        /// <summary>
        /// Copies attributes from another style
        /// </summary>
        public void Assign(Style from)
        {
           m_HAlignment = from.m_HAlignment;
           m_BGKind = from.m_BGKind;
           m_BGColor = from.m_BGColor;
           m_BGColor2 = from.m_BGColor2;
           m_BGHatchColor = from.m_BGHatchColor;
           m_BGHatchStyle = from.m_BGHatchStyle;
           m_Padding = from.m_Padding;
           m_BorderLeft = from.m_BorderLeft;
           m_BorderRight = from.m_BorderRight;
           m_BorderTop = from.m_BorderTop;
           m_BorderBottom = from.m_BorderBottom;
           m_Font = from.m_Font;
       }

      #endregion


      #region .pvt .impl

        private void notify()
        {
          if (m_ContextControl!=null)
           m_ContextControl.Invalidate(true);
        }

      #endregion

  }

  /// <summary>
  /// Defines a style for line
  /// </summary>
  [TypeConverter(typeof(ExpandableObjectConverter))]
  [Themable]
  public struct LineStyle
  {
    private Color m_Color;
    private Brush m_Brush;

    /// <summary>
    /// Specifies type of dashing used
    /// </summary>
    public DashStyle DashStyle { get; set; }

    /// <summary>
    /// Specifies line color
    /// </summary>
    public Color Color
    {
      get { return m_Color; }
      set { m_Color = value; m_Brush = new SolidBrush(value); }
    }

    [Browsable(false)]
    public Brush Brush { get { return m_Brush; } }

    /// <summary>
    /// Specifies line width
    /// </summary>
    [Description("Specifies line width")]
    public float Width { get; set; }

    public static LineStyle DefaultNone
    {
      get
      {
        return new LineStyle()
        {
          DashStyle = DashStyle.Solid,
          Color = Color.Transparent,
          Width = 1.0f
        };
      }
    }

    public static LineStyle DefaultGray
    {
      get
      {
        return new LineStyle()
        {
          DashStyle = DashStyle.Solid,
          Color = Color.DarkGray,
          Width = 1.0f
        };
      }
    }
  }

  /// <summary>
  /// Indicates that the class/struct/field/prop is themable
  /// </summary>
  [AttributeUsage(AttributeTargets.Class  |
                  AttributeTargets.Struct |
                  AttributeTargets.Field  |
                  AttributeTargets.Property)]
  public class ThemableAttribute : Attribute
  {
  }
}
