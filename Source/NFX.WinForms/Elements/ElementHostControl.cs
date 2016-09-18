/*<FILE_LICENSE>
* NFX (.NET Framework Extension) Unistack Library
* Copyright 2003-2016 IT Adapter Inc.
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

using System.Windows.Forms;
using System.Drawing;

namespace NFX.WinForms.Elements
{
  /// <summary>
  /// Defines a sequence of element enumeration
  /// </summary>
  public enum ElementZOrder {
    /// <summary>
    /// This is used for mouse events, top -> bottom
    /// </summary>
    TopToBottom, 
    
    
    /// <summary>
    /// Used for painting, bottom -> top
    /// </summary>
    BottomToTop
    }
  
  /// <summary>
  /// Represents a host control for lightweight control-like elements. Elements can receive mouse input
  /// however they do not support keyboard focus
  /// </summary>
  public class ElementHostControl : Control
  {
    #region CONSTS
      public const float MIN_ZOOM = 0.05f;
    
    #endregion
    
    
    #region .ctor

       public ElementHostControl() : base ()
       {
         SetStyle(ControlStyles.OptimizedDoubleBuffer, true);
         SetStyle(ControlStyles.UserPaint, true);
         SetStyle(ControlStyles.AllPaintingInWmPaint, true);
         
         SetStyle(ControlStyles.UserMouse, true);

         
         UpdateStyles();
       }

    #endregion


    #region Private Fields
     private ElementList m_ElementList = new ElementList();
     private Element m_MouseEnteredElement;
     private Element m_MouseDragElement;
     private Element m_LastMouseDownElement;
     private MouseEventArgs m_LastMouseDownEventArgs;
     
     private float m_Zoom = 1f;
    #endregion


    #region Properties
      /// <summary>
      /// Provides access to hosted elements
      /// </summary>
      [Browsable(false)]
      public IEnumerable<Element> Elements
      {
        get { return m_ElementList; }
      }


      /// <summary>
      /// Returns mouse-entered element or null
      /// </summary>
      [Browsable(false)]
      public Element MouseEnteredElement
      {
        get { return m_MouseEnteredElement; }
      }


      /// <summary>
      /// Returns pixel height of current font
      /// </summary>
      [Browsable(false)]
      public int CurrentFontHeight
      {
        get { return FontHeight; }
      }
      
      
      /// <summary>
      /// Provides access to zoom coefficient
      /// </summary>
      [Browsable(false)]
      public float Zoom
      {
        get { return m_Zoom; }
        set 
        {
          if (UpdateZoom(value))
           Invalidate();
        }
      }

    #endregion


    #region Public

      /// <summary>
      /// Returns list of elements inside specified region. Elements are returned in registration order
      /// </summary>
      public IEnumerable<Element> GetElementsInRegion(Rectangle region)
      {
        foreach(Element element in m_ElementList)
         if (region.IntersectsWith(element.DisplayRegion))
           yield return element;
      }
      
      /// <summary>
      /// Returns list of elements that are completely outside of region
      /// </summary>
      public IEnumerable<Element> GetElementsOutOfRegion(Rectangle region)
      {
        foreach(Element element in m_ElementList)
         if (!region.IntersectsWith(element.DisplayRegion))
           yield return element;
      }
      

      /// <summary>
      /// Returns list of elements inside specified region. Elements are returned in registration order
      /// </summary>
      public IEnumerable<Element> GetElementsInRegion(Rectangle region, ElementZOrder order)
      {
        if (order == ElementZOrder.BottomToTop)
           return GetElementsInRegion(region).OrderBy(item => item.ZOrder);
        else
           return GetElementsInRegion(region).OrderBy(item => -item.ZOrder); 
      }

      /// <summary>
      /// Finds an element (if any) which is clickable at a certain point
      /// </summary>
      public Element GetClickableElementAt(Point point)
      {
        foreach (Element elm in GetElementsInRegion(new Rectangle(point, new Size(1,1)), ElementZOrder.TopToBottom))
        {
          if (elm.Visible)
          {
            if (!elm.Enabled) break;
            if (!elm.MouseTransparent) return elm;
          }
        }
        
        return null;
      }
      
      
      /// <summary>
      /// Scrolls elements by delta x/y and destroys those elements that are completely out of the viewport.
      /// </summary>
      public void ScrollElementsBy(int offsetx, int offsety, int dx, int dy, bool destroy)    //todo dorabotat!
      {
        using(var gr = this.CreateGraphics())
        {
          var origin = this.PointToScreen(new Point(offsetx, offsety));
          gr.SetClip(new Rectangle(offsetx, offsety, Width-offsetx, Height-offsety));
          gr.CopyFromScreen(origin, new Point(offsetx+dx, offsety+dy), new Size(Width - offsetx, Height-offsety), CopyPixelOperation.SourceCopy);
                                               
          foreach(var element in m_ElementList) 
            element.UpdateLocation(dx, dy);
            
          if (destroy)
          {
            var lst = GetElementsOutOfRegion(new Rectangle(0, 0, Width, Height)).ToList();
            foreach(var element in lst)
             element.Dispose();  
          }  
          
          //todo need to find elemnts that are partially out of region and invalidate them because their visibility partially changed
          
        }//graphics
      }
      
      /// <summary>
      /// Deletes all elements efficiently. Calls Dispose() for each element 
      /// </summary>
      public void DeleteAllElements(Func<Element, bool> keepFilter = null)
      {
        releaseMouseDragElement(null);
        releaseMouseEnteredElement();
        
        
        m_LastMouseDownElement = null;
        
        ElementList kept = null;

        foreach(var elm in m_ElementList)
        {
          if (keepFilter!=null && keepFilter(elm))
          {
            if (kept==null) kept = new ElementList();
            kept.Add(elm);
            continue;
          } 
          elm.m_Host = null;//prevent elements from unregistering with host because it is slower than host.list.Clear()
          elm.Dispose();
        }  
        
        m_ElementList.Clear();

        if (kept!=null)
         m_ElementList = kept;
        
        Invalidate();
      }
      
      
      /// <summary>
      /// Updates zoom value without invalidation of the whole control, returns true if invalidation is needed(value changed) 
      /// </summary>
      public bool UpdateZoom(float value)
      {
         if (value<=MIN_ZOOM) value = MIN_ZOOM;
          if (m_Zoom!=value)
          {
            m_Zoom = value;
            return true;
          }
          return false;
      }

    #endregion


    #region Protected
      
      
         #region Mouse Events

            protected override void OnMouseClick(MouseEventArgs e)
            {
              Element element = GetClickableElementAt(e.Location);
              
              if (element!=null)
                 element.OnMouseClick(e);
              
              base.OnMouseClick(e);
            }

            protected override void OnMouseDoubleClick(MouseEventArgs e)
            {
              Element element = GetClickableElementAt(e.Location);

              if (element != null)
                  element.OnMouseDoubleClick(e);
              
              base.OnMouseDoubleClick(e);
            }

            protected override void OnMouseDown(MouseEventArgs e)
            {
              Element element = GetClickableElementAt(e.Location);

              if (element != null)
              {
                element.OnMouseDown(e);
                
                if (m_MouseDragElement != element)//safeguard
                   releaseMouseDragElement(e);
                
                m_LastMouseDownElement = element;
                m_LastMouseDownEventArgs = e;
              }  
              
              base.OnMouseDown(e);
            }
            
            protected override void OnMouseUp(MouseEventArgs e)
            {
              m_LastMouseDownElement = null;
              
              Element element = GetClickableElementAt(e.Location);

              if (element != null)
                element.OnMouseUp(e);
                
              releaseMouseDragElement(e);  

              base.OnMouseDown(e);
            }

            protected override void OnMouseMove(MouseEventArgs e)
            {
              Element element = GetClickableElementAt(e.Location);

              if (m_MouseEnteredElement != element)
                releaseMouseEnteredElement();

              if (element != null)
              {         
                element.OnMouseMove(e);

                if (m_MouseEnteredElement != element)
                {
                  element.OnMouseEnter(e);
                  m_MouseEnteredElement = element;
                }  
              }  
              
              //start dragging
              if (m_LastMouseDownElement!=null && m_MouseDragElement==null)
              {
                m_MouseDragElement = m_LastMouseDownElement;
                m_LastMouseDownElement.OnMouseDragStart(m_LastMouseDownEventArgs);
              }  
              
              if (m_MouseDragElement!=null)
               m_MouseDragElement.OnMouseDrag(e);
                
              base.OnMouseMove(e);
              
            }

            protected override void OnMouseWheel(MouseEventArgs e)
            {
              Element element = GetClickableElementAt(e.Location);

              if (element != null)
                element.OnMouseWheel(e);
                
              base.OnMouseWheel(e);
            }

            
            
            protected override void OnMouseLeave(EventArgs e)
            {
              releaseMouseEnteredElement();
              releaseMouseDragElement(null);
              m_LastMouseDownElement = null;
              base.OnMouseLeave(e);
            }
            
            
         
         #endregion
      
      
      
      protected override void OnPaint(PaintEventArgs e)
      {
        const int SAFE_MARGIN = 2;//repaint a bit more than windows asks
        
        if (m_Zoom!=0f)
         e.Graphics.ScaleTransform(m_Zoom, m_Zoom); 
        
        Rectangle rgn = e.ClipRectangle;
        rgn.Inflate(SAFE_MARGIN, SAFE_MARGIN);//make a bit bigger
        
        foreach(Element element in GetElementsInRegion(rgn, ElementZOrder.BottomToTop))
         if (element.Visible)
         {
            element.Paint(e.Graphics);
        //    e.Graphics.DrawRectangle(Pens.SlateBlue, element.Region);//for debugging
         }  
       //  else
       //     e.Graphics.DrawRectangle(Pens.Orange, element.Region);//for debugging

        base.OnPaint(e);
      }

    
      
    
    #endregion


    #region Private Utils
      internal void AddElement(Element element)
      {
        m_ElementList.Add(element);
        Invalidate(element.Region);
      }
     
     internal void RemoveElement(Element element)
     {
      try
      {
        if (m_MouseDragElement==element)  
          releaseMouseDragElement(null);
        
        if (m_MouseEnteredElement==element)
          releaseMouseEnteredElement();
      }
      finally
      {  
        m_LastMouseDownElement = null;
        
        m_ElementList.Remove(element);
        
        Invalidate(element.Region);
      }
     }

     private void releaseMouseEnteredElement()
     {
       if (m_MouseEnteredElement!=null)
       {
          m_MouseEnteredElement.OnMouseLeave(EventArgs.Empty);
          m_MouseEnteredElement = null ;
       }
     }
     
     private void releaseMouseDragElement(MouseEventArgs e)
     {
       if (m_MouseDragElement!=null)
       {
          if (e!=null)
           m_MouseDragElement.OnMouseDragRelease(e);
          else
           m_MouseDragElement.OnMouseDragCancel(EventArgs.Empty); 
          m_MouseDragElement = null ;
       }
     }


    #endregion

  }
  
}
