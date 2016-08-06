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
    private Grid        m_Grid;

    #endregion

    #region Protected/Private

    /*
    protected internal override void Paint(Graphics gr)
    {
      if (MouseIsOver && m_Grid.ColumnResizeAllowed &&  m_Grid.m_RepositioningColumn==null)
        using(var br =  new LinearGradientBrush(Region, Color.Blue, Color.White, LinearGradientMode.ForwardDiagonal))
        {
          gr.FillRectangle(br, Region);
        }
    }
    */

    protected internal override void OnMouseClick(MouseEventArgs e)
    {
      // Right click on the right edge of a column - open the column picker
      if (e.Button == MouseButtons.Right)
      {
        if (!m_Grid.Columns.Any() || !m_Grid.ColumnHidingAllowed)
          return;

        var longestText     = new string('W', m_Grid.Columns.Max(c => c.Title.Length));
        var textSize        = TextRenderer.MeasureText(longestText, m_Grid.Font);
        int preferredHeight = (m_Grid.Columns.Count * textSize.Height) + 7;
        var checkedListBox  = new CheckedListBox()
        {
          CheckOnClick = true,
          Height       = (preferredHeight < 350) ? preferredHeight : 350,
          Width        = Math.Min(textSize.Width, m_Grid.Width),
        };
        checkedListBox.ItemCheck += (sender, ea) =>
        {
          var column = m_Grid.Columns.FirstOrDefault(c => c.Title.Equals(checkedListBox.Items[ea.Index].ToString()));
          if (column == null) return;
          if (m_Grid.Columns.Count(c => c.Visible) > 1 || ea.NewValue == CheckState.Checked)
            column.Visible = ea.NewValue == CheckState.Checked;
          else
            ea.NewValue = ea.CurrentValue;
        };

        foreach (var c in m_Grid.Columns)
          checkedListBox.Items.Add(c.Title, c.Visible);

        var controlHost = new ToolStripControlHost(checkedListBox)
        {
          Padding  = Padding.Empty,
          Margin   = Padding.Empty,
          AutoSize = false
        };

        // The following permits handling of custom actions such as applying themes
        // to the newly created control:
        m_Grid.Controls.Add(checkedListBox);

        var popup = new ToolStripDropDown() {Padding = Padding.Empty};
        popup.Items.Add(controlHost);
        popup.Disposed += (sender, args) => m_Grid.Controls.Remove(checkedListBox); // This removes checkedListBox from m_Grid.Controls

        popup.Show(m_Grid.PointToScreen(e.Location));
      }

      base.OnMouseClick(e);
    }

    protected internal override void OnMouseDoubleClick(MouseEventArgs e)
    {
      // Double click resizes the column
      if (e.Button == MouseButtons.Left)
      {
        var wid = m_Grid.CellView.Elements
                        .Where(ce => ce.GetType() == typeof(CellElement))
                        .Cast<CellElement>()
                        .Where(ce => ReferenceEquals(ce.Column, m_HeaderCell.Column))
                        .Max(ce => ce.Value == null
                          ? 0
                          : TextRenderer.MeasureText(ce.RepresentValueAsString(ce.Value),
                                                     ce.Host.Font).Width +
                            ce.Style.Padding.Horizontal
                        );
        m_HeaderCell.Column.Width = Math.Max(wid, 10);
      }
      base.OnMouseDoubleClick(e);
    }

    protected internal override void OnMouseEnter(EventArgs e)
    {
      if (m_Grid.ColumnResizeAllowed && m_Grid.m_RepositioningColumn == null)
      {
        m_Grid.Cursor = Cursors.VSplit;
        Invalidate();
      }

      base.OnMouseEnter(e);
    }

    protected internal override void OnMouseLeave(EventArgs e)
    {
      if (m_Grid.ColumnResizeAllowed && m_Grid.m_RepositioningColumn == null)
        Invalidate();
      m_Grid.Cursor = Cursors.Default;
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
      if (m_Grid.m_ResizingColumn == null) return;

      var rect = DisplayRegion;
      rect.Offset(e.X - m_GrabX, 0);
      rect.X = Math.Max(0, rect.X);
      rect.X = Math.Min(m_Grid.CellView.Right, rect.X);
      rect.Y = 1;
      rect.Height = m_Host.Height - 1;
      invert();
      m_InvertRect = m_Host.RectangleToScreen(rect);
      ControlPaint.FillReversibleRectangle(m_InvertRect, Color.Beige);
    }

    protected internal override void OnMouseDragRelease(MouseEventArgs e)
    {
      base.OnMouseDragRelease(e);

      if (m_Grid.m_ResizingColumn != null)
      {
        m_Grid.m_ResizingColumn = null;
        {
          var oldW = m_HeaderCell.Column.Width;
          var newW = oldW + (int)((e.X - m_GrabX)/m_Host.Zoom);
          newW = Math.Max(5, newW);
          var maxW = m_Grid.Width - m_HeaderCell.Left - m_Grid.m_VScrollBar.Width - 2;

          if (newW > maxW) newW = maxW;

          if (oldW != newW)
            m_HeaderCell.Column.Width = newW;
          else
            m_Host.Invalidate();
              //refresh artifacts in case width did not change becaus of constraints
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
      if (m_InvertRect.X != 0 && m_InvertRect.Y != 0)
        ControlPaint.FillReversibleRectangle(m_InvertRect, Color.Beige);
    }

    #endregion
  }
}
