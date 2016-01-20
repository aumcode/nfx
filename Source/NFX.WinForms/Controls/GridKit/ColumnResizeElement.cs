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
using System.Drawing.Drawing2D;
using System.Drawing.Text;

using NFX.WinForms.Elements;

namespace NFX.WinForms.Controls.GridKit
{

  /// <summary>
  /// Represents a cell in a grid
  /// </summary>
  internal class ColumnResizeElement : Element
  {
      #region .ctor
     
       internal ColumnResizeElement(CellView host, CellElement headerCell) : base(host)
       {
         m_HeaderCell = headerCell;
         m_Grid = m_HeaderCell.Column.Grid; 
       }
     
     #endregion
     
     #region Private Fields
       
       private CellElement m_HeaderCell;
       private Grid m_Grid; 
       
     #endregion
     
     
     #region Protected/Private

       protected internal override void Paint(Graphics gr)
       {
         if (MouseIsOver && m_Grid.ColumnResizeAllowed &&  m_Grid.m_RepositioningColumn==null)
           using(var br =  new LinearGradientBrush(Region, Color.Blue, Color.White, LinearGradientMode.ForwardDiagonal))
           {
             gr.FillRectangle(br, Region); 
           }  
       }

       protected internal override void OnMouseEnter(EventArgs e)
       {
         if (m_Grid.ColumnResizeAllowed &&  m_Grid.m_RepositioningColumn==null)
           Invalidate();
        
         base.OnMouseEnter(e);
       }
       
       protected internal override void OnMouseLeave(EventArgs e)
       {
         if (m_Grid.ColumnResizeAllowed &&  m_Grid.m_RepositioningColumn==null)
           Invalidate();
         base.OnMouseEnter(e);
       }
       
       
       private int m_GrabX;
       private Rectangle m_InvertRect;
       

       protected internal override void OnMouseDragStart(MouseEventArgs e)
       {
         if (!m_Grid.ColumnResizeAllowed) return;
         
         m_Grid.m_ResizingColumn = m_HeaderCell.Column;
         m_GrabX = e.X;
         m_InvertRect = new Rectangle();
         base.OnMouseDragStart(e);
       }
       
       protected internal override void OnMouseDrag(MouseEventArgs e)
       {
         base.OnMouseDrag(e);
         if (m_Grid.m_ResizingColumn==null) return;
         
         var rect = DisplayRegion;
         rect.Offset(e.X - m_GrabX, 0);
         rect.X = Math.Max(0, rect.X);
         rect.X = Math.Min(m_Grid.CellView.Right, rect.X);
         rect.Y = 1;
         rect.Height = m_Host.Height -1;
         invert();
         m_InvertRect = m_Host.RectangleToScreen(rect);
         ControlPaint.FillReversibleRectangle(m_InvertRect, Color.Beige);
       }
       
       protected internal override void OnMouseDragRelease(MouseEventArgs e)
       {
         base.OnMouseDragRelease(e);
         
         if (m_Grid.m_ResizingColumn!=null)
         {
           m_Grid.m_ResizingColumn = null;
           {
             var oldW = m_HeaderCell.Column.Width;
             var newW = oldW + (int)((e.X - m_GrabX) / m_Host.Zoom);
             newW = Math.Max(5, newW);
             var maxW = m_Grid.Width - m_HeaderCell.Left - m_Grid.m_VScrollBar.Width - 2;

             if (newW>maxW) newW = maxW;
             
             if (oldW!=newW) 
               m_HeaderCell.Column.Width = newW;
             else
               m_Host.Invalidate();//refresh artifacts in case width did not change becaus of constraints
           }
           //else
           
           //invert();
         }
       }
       
       protected internal override void OnMouseDragCancel(EventArgs e)
       {
         m_Grid.m_ResizingColumn = null;
         invert();
         base.OnMouseDragCancel(e);
       }
       
       private void invert()
       {
          if (m_InvertRect.X!=0 && m_InvertRect.Y!=0)
           ControlPaint.FillReversibleRectangle(m_InvertRect, Color.Beige);
       }
     
     #endregion
  }
}
