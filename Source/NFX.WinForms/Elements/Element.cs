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

using System.Windows.Forms;
using System.Drawing;

namespace NFX.WinForms.Elements
{


  /// <summary>
  /// Represents an element to be drawn in a host control
  /// </summary>
  public abstract class Element : DisposableObject
  {
     #region .ctor

       private Element()
       {
       }

       protected Element(ElementHostControl host)
       {
        m_Host = host;
        m_Host.AddElement(this);
       }

     #endregion


     #region Private Fields
        internal ElementHostControl m_Host;

        private Dictionary<string, object> m_Tags;

        private bool m_Visible = true;
        private bool m_Enabled = true;
        private int m_ZOrder;

        private bool m_MouseTransparent;
        private Rectangle m_Region;

        private ElementList m_OwnedElements;

        private IFieldControlContext m_FieldControlContext;
     #endregion


     #region Properties


        /// <summary>
        /// References host control
        /// </summary>
        public ElementHostControl Host
        {
          get { return m_Host; }
        }


        /// <summary>
        /// Returns a list of elements owned by this element. Owned elements are deleted when this element is deleted.
        /// Element ownership does not affect element drawing
        /// </summary>
        public ElementList OwnedElements
        {
          get
          {
            if (m_OwnedElements==null)
              m_OwnedElements = new ElementList();

            return m_OwnedElements;
          }
        }



        /// <summary>
        /// References control context for which element painting is done, may be null.
        /// Part renderers may need control context to paint additional features like changed borders (when mofified etc.).
        /// </summary>
        public IFieldControlContext FieldControlContext
        {
          get { return m_FieldControlContext; }
          set
          {
            m_FieldControlContext = value;
            FieldControlContextChanged();
            Invalidate();
          }
        }


        /// <summary>
        /// Indicates whether an element is displayed
        /// </summary>
        public int ZOrder
        {
          get { return m_ZOrder; }
          set
          {
            if (m_ZOrder!=value)
            {
                m_ZOrder = value;
                ZOrderChanged();
                Invalidate();
            }
          }
        }


       /// <summary>
       /// Indicates whether an element is displayed
       /// </summary>
       public bool Visible
       {
         get {return m_Visible; }
         set
         {
           if (m_Visible!=value)
           {
            m_Visible = value;
            VisibleChanged();
            Invalidate();
           }
         }
       }

       /// <summary>
       /// Indicates whether an element can receive events
       /// </summary>
       public bool Enabled
       {
         get { return m_Enabled; }
         set
         {
           if (m_Enabled!=value)
           {
             m_Enabled = value;
             EnabledChanged();
             Invalidate();
           }
         }
       }

       /// <summary>
       /// Indicates whether an element does not receive any mouse events
       /// </summary>
       public bool MouseTransparent
       {
         get { return m_MouseTransparent; }
         set
         {
           if (m_MouseTransparent!=value)
           {
             m_MouseTransparent = value;
             MouseTransparentChanged();
           }
         }
       }


       /// <summary>
       /// Provides graphical region- coordinates of the element without zoom. Even when host container is zoomed it has no effect on Region. Compare to DisplayRegion
       /// </summary>
       public Rectangle Region
       {
         get { return m_Region; }
         set
         {
           m_Region = value;
           RegionChanged();
           Invalidate();
         }
       }

       /// <summary>
       /// Provides graphical region- coordinates of the element in zoomed container. These coordinates are different from Region when Zoom!=1
       ///  because Region returns "normal"/non-zoomed coordinates. Mouse event handlers must use DisplayRegion because mouse is in screen coordinates
       /// </summary>
       public Rectangle DisplayRegion
       {
         get
         {
           var zoom = m_Host!=null? m_Host.Zoom : 1f;

           if (zoom==1f) return Region;

           var result = m_Region;

           result.X = (int)(result.X * zoom);
           result.Y = (int)(result.Y * zoom);
           result.Width = (int)(result.Width * zoom);
           result.Height = (int)(result.Height * zoom);
           return result;
         }
       }



       /// <summary>
       /// Shortcut property to element's region
       /// </summary>
       public int Left
       {
        get { return m_Region.Left; }
        set
        {
          m_Region.X = value;
          RegionChanged();
          Invalidate();
        }
       }

       /// <summary>
       /// Shortcut property to element's region
       /// </summary>
       public int Top
       {
         get { return m_Region.Top; }
         set
         {
           m_Region.Y = value;
           RegionChanged();
           Invalidate();
         }
       }

       /// <summary>
       /// Shortcut property to element's region
       /// </summary>
       public int Width
       {
         get { return m_Region.Width; }
         set
         {
           m_Region.Width = value;
           RegionChanged();
           Invalidate();
         }
       }

       /// <summary>
       /// Shortcut property to element's region
       /// </summary>
       public int Height
       {
         get { return m_Region.Height; }
         set
         {
           m_Region.Height = value;
           RegionChanged();
           Invalidate();
         }
       }


       /// <summary>
       /// Returns true if mouse is currently over the element
       /// </summary>
       public bool MouseIsOver
       {
         get { return m_Host.MouseEnteredElement == this; }
       }


       public Dictionary<string, object> Tags
       {
         get
         {
          if (m_Tags==null) m_Tags = new Dictionary<string,object>();
          return m_Tags;
         }
       }

     #endregion


     #region Events

      public event MouseEventHandler MouseClick;
      public event MouseEventHandler MouseDoubleClick;
      public event MouseEventHandler MouseDown;
      public event MouseEventHandler MouseUp;
      public event MouseEventHandler MouseMove;
      public event MouseEventHandler MouseWheel;
      public event EventHandler MouseEnter;
      public event EventHandler MouseLeave;

      public event MouseEventHandler MouseDragStart;
      public event MouseEventHandler MouseDrag;
      public event MouseEventHandler MouseDragRelease;
      public event EventHandler MouseDragCancel;

     #endregion

     #region Public

      /// <summary>
      /// Repaints this particular element instance
      /// </summary>
      public void Repaint()
      {
        using(Graphics gr = m_Host.CreateGraphics())
        {
         Paint(gr);
        }
      }

      /// <summary>
      /// Invalidates space occupied by this element.
      /// This causes repaint of any elements overlapped by elements region
      /// </summary>
      public void Invalidate()
      {
        const int SAFE_MARGIN = 2;//repaint a bit more than windows asks

        Rectangle rgn = DisplayRegion;
        rgn.Inflate(SAFE_MARGIN, SAFE_MARGIN);
        m_Host.Invalidate(rgn);
      }


      /// <summary>
      /// Updates element location by delta x/y without causing invalidate
      /// </summary>
      public void UpdateLocation(int dx, int dy)
      {
        m_Region.Offset(dx, dy);
      }

     #endregion


     #region Protected

         protected override void Destructor()
         {
           if (m_Host != null)
            m_Host.RemoveElement(this);

           if (m_OwnedElements!=null)
            foreach(Element elm in m_OwnedElements)
             elm.Dispose();

           base.Destructor();
         }


         protected virtual void FieldControlContextChanged()
         {
         }

         protected internal virtual void Paint(Graphics gr)
         {

         }

         /// <summary>
         /// Override to take action after element's Z-order changed
         /// </summary>
         protected virtual void ZOrderChanged()
         {

         }


         /// <summary>
         /// Override to take action after element's region changed
         /// </summary>
         protected virtual void RegionChanged()
         {

         }

         /// <summary>
         /// Override to take action after element's visibility change
         /// </summary>
         protected virtual void VisibleChanged()
         {

         }

         /// <summary>
         /// Override to take action after element's enabled state change
         /// </summary>
         protected virtual void EnabledChanged()
         {

         }

         /// <summary>
         /// Override to take action after element's mouse transparency change
         /// </summary>
         protected virtual void MouseTransparentChanged()
         {

         }


       #region Mouse Events

         protected internal virtual void OnMouseClick(MouseEventArgs e)
         {
           if (MouseClick!=null) MouseClick(this, e);
         }

         protected internal virtual void OnMouseDoubleClick(MouseEventArgs e)
         {
           if (MouseDoubleClick != null) MouseDoubleClick(this, e);
         }

         protected internal virtual void OnMouseDown(MouseEventArgs e)
         {
           if (MouseDown != null) MouseDown(this, e);
         }

         protected internal virtual void OnMouseUp(MouseEventArgs e)
         {
           if (MouseUp != null) MouseUp(this, e);
         }

         protected internal virtual void OnMouseMove(MouseEventArgs e)
         {
           if (MouseMove != null) MouseMove(this, e);
         }

         protected internal virtual void OnMouseWheel(MouseEventArgs e)
         {
           if (MouseWheel != null) MouseWheel(this, e);
         }

         protected internal virtual void OnMouseEnter(EventArgs e)
         {
           if (MouseEnter != null) MouseEnter(this, e);
         }

         protected internal virtual void OnMouseLeave(EventArgs e)
         {
           if (MouseLeave != null) MouseLeave(this, e);
         }


         protected internal virtual void OnMouseDragStart(MouseEventArgs e)
         {
           if (MouseDragStart !=null) MouseDragStart(this, e);
         }

         protected internal virtual void OnMouseDrag(MouseEventArgs e)
         {
           if (MouseDrag !=null) MouseDrag(this, e);
         }

         protected internal virtual void OnMouseDragRelease(MouseEventArgs e)
         {
           if (MouseDragRelease !=null) MouseDragRelease(this, e);
         }

         protected internal virtual void OnMouseDragCancel(EventArgs e)
         {
           if (MouseDragCancel !=null) MouseDragCancel(this, e);
         }


       #endregion

     #endregion


     #region Private Utils


     #endregion



  }


  /// <summary>
  /// Represents a list of elements
  /// </summary>
  public class ElementList : List<Element> { }


}
