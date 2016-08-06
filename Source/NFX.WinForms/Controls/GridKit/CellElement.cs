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
  public class CellElement : Element
  {
    public const int RESIZE_WIDTH = 5;

    #region .ctor

    protected internal CellElement(CellView host, object row, Column column) : base(host)
    {
      m_Row = row;
      m_Column = column;
      m_Grid = column.Grid;

      m_Style = new Style(null, getBaseStyle());

      if (row == null) //if header cell
      {
        m_ColumnResizeElement = new ColumnResizeElement(host, this);
        m_ColumnResizeElement.ZOrder = 1;
      }
    }

    #endregion

    #region Private Fields

    private object m_Row;
    private Column m_Column;
    private Style m_Style;
    private Grid m_Grid;
    private ColumnResizeElement m_ColumnResizeElement;

    private object m_Value;

    #endregion

    #region Properties

    /// <summary>
    /// References row that this cell is for, null if header row
    /// </summary>
    public object Row
    {
      get { return m_Row; }
    }


    /// <summary>
    /// References column that this cell is for
    /// </summary>
    public Column Column
    {
      get { return m_Column; }
    }


    /// <summary>
    /// References grid
    /// </summary>
    public Grid Grid
    {
      get { return m_Grid; }
    }


    /// <summary>
    /// Returns a style for this cell. Cell style inherit from column style, which in turn inherits from grid style.
    /// Changing cell style does not automatically repaint the element
    /// </summary>
    public Style Style
    {
      get { return m_Style; }
    }


    /// <summary>
    /// Returns value displayed in this cell or null.
    /// You can call RepresentValueAsString(Value) to get a string representation of this cell
    /// </summary>
    public object Value
    {
      get { return m_Value; }
    }

    #endregion

    #region Protected

    protected override void RegionChanged()
    {
      base.RegionChanged();
      if (m_ColumnResizeElement != null)
      {
        m_ColumnResizeElement.Region = new Rectangle(Region.Right - RESIZE_WIDTH,
                                                     Region.Top + 1, RESIZE_WIDTH,
                                                     Region.Height - 2);
      }
    }


    protected internal override void Paint(Graphics gr)
    {
      //base.Paint(gr);
      PaintBackground(gr);
      PaintBorders(gr);

      if (m_Row == null) //paint header row
      {
        PaintValue(gr, m_Column.Title);
        PaintSortingArrows(gr, m_Column.SortDirection);
        return;
      }

      if (!m_Column.HasValueInRow(m_Row)) return; //nothing else to paint
      m_Value = m_Column.GetValueFromRow(m_Row);
      if (m_Value == null) return;

      PaintValue(gr, m_Value);
    }

    protected virtual void PaintBackground(Graphics gr)
    {
      using (var brush = getBrush(m_Style))
      {
        gr.FillRectangle(brush, Region);
      }
    }

    protected virtual void PaintBorders(Graphics gr)
    {
      using (var pen = getPen(m_Style.BorderLeft))
      {
        gr.DrawLine(pen, Region.X, Region.Y, Region.X, Region.Bottom);
      }

      using (var pen = getPen(m_Style.BorderRight))
      {
        gr.DrawLine(pen, Region.Right, Region.Y, Region.Right, Region.Bottom);
      }

      using (var pen = getPen(m_Style.BorderTop))
      {
        gr.DrawLine(pen, Region.Left, Region.Y, Region.Right, Region.Y);
      }

      using (var pen = getPen(m_Style.BorderBottom))
      {
        gr.DrawLine(pen, Region.Left, Region.Bottom, Region.Right, Region.Bottom);
      }
    }

    protected virtual void PaintSortingArrows(Graphics gr, SortDirection sort)
    {
      if (sort == SortDirection.None) return;

      const int dx = 4;
      const int dy = 4;

      var up = sort == SortDirection.Up;

      var cx = Region.Left + (int)(Region.Width/2);
      var y = up ? Region.Top + 1 : Region.Bottom - 1;

      var points = new Point[3];
      points[0] = up ? new Point(cx - dx, y + dy) : new Point(cx - dx, y - dy);
      points[1] = new Point(cx, y);
      points[2] = up ? new Point(cx + dx, y + dy) : new Point(cx + dx, y - dy);

      gr.FillPolygon(Brushes.Blue, points, FillMode.Alternate);
    }


    protected virtual void PaintValue(Graphics gr, object value)
    {
      using (StringFormat fmt = new StringFormat())
      {
        var ha = m_Style.HAlignment;
        if (ha == HAlignment.Left || ha == HAlignment.Right)
          fmt.FormatFlags &= ~StringFormatFlags.DirectionRightToLeft;

        switch (ha)
        {
          case HAlignment.Left:
            fmt.Alignment = StringAlignment.Near;
            break;
          case HAlignment.Center:
            fmt.Alignment = StringAlignment.Center;
            break;
          case HAlignment.Right:
            fmt.Alignment = StringAlignment.Far;
            break;
          case HAlignment.Near:
            fmt.Alignment = StringAlignment.Near;
            break;
          case HAlignment.Far:
            fmt.Alignment = StringAlignment.Far;
            break;
          default:
            fmt.Alignment = StringAlignment.Near;
            break;
        }

        var reg = Region;
        reg.X += m_Style.Padding.Left;
        reg.Y += m_Style.Padding.Top;
        reg.Width -= m_Style.Padding.Horizontal;
        reg.Height -= m_Style.Padding.Vertical;

        var str = RepresentValueAsString(value);

        gr.DrawString(str, m_Style.Font, m_Style.ForeBrush, reg, fmt);
      }
    }

    /// <summary>
    /// Converts/formats cell object value as string so it can be painted. This implementation relies on column to convert the value.
    /// Override to perform cell-specific conversions/formatting
    /// </summary>
    public virtual string RepresentValueAsString(object value)
    {
      return m_Column.RepresentValueAsString(m_Row, value);
    }

    protected internal override void OnMouseClick(MouseEventArgs e)
    {
      if (m_Row == null) //header cell
        switch (e.Button)
        {
          case MouseButtons.Left:
            if (!m_Grid.SortingAllowed || !m_Column.SortingAllowed ||
                m_Grid.m_RepositioningColumn != null ||
                m_Grid.m_ResizingColumn != null)
              return;

            var dir = m_Column.SortDirection;
            dir++;
            if (dir > SortDirection.LAST) dir = SortDirection.FIRST;
            m_Column.SortDirection = dir;
            break;
          case MouseButtons.Right:
            return;
        }
      base.OnMouseClick(e);
    }

    protected internal override void OnMouseDown(System.Windows.Forms.MouseEventArgs e)
    {
      if (e.Button == MouseButtons.Left)
        DispatchSelection();
      base.OnMouseDown(e);
    }

    protected internal override void OnMouseEnter(EventArgs e)
    {
      base.OnMouseEnter(e);

      if (m_Grid.m_RepositioningColumn != null || m_Grid.m_ResizingColumn != null) return;

      var comment = m_Column.GetCommentFromRow(m_Row);

      if (!string.IsNullOrEmpty(comment))
      {
        var elm = m_Grid.m_CommentElement;
        if (elm == null)
        {
          elm = new CommentElement(m_Host as CellView);
          m_Grid.m_CommentElement = elm;
        }
        var reg = Region;

        var w = (int)(comment.Length*8*m_Host.Zoom);
        reg.Width = 50 + (w%350);
        reg.Height = 15 + (15*(w/reg.Width)) + 4;

        if (reg.Right + reg.Width > m_Host.Width/m_Host.Zoom)
          reg.X += -reg.Width + 2;
        else
          reg.X += Region.Width + 2;

        if (reg.Bottom + reg.Height > m_Host.Height/m_Host.Zoom)
          reg.Y += -reg.Height + 2;
        else
          reg.Y += Region.Height + 2;


        elm.Region = reg;
        elm.ZOrder = 10;
        elm.Text = comment;
        elm.Visible = true;
      }
    }

    protected internal override void OnMouseLeave(EventArgs e)
    {
      if (m_Grid.m_CommentElement != null) m_Grid.m_CommentElement.Visible = false;
      base.OnMouseLeave(e);
    }

    private int m_GrabX;
    private Rectangle m_InvertRect;


    protected internal override void OnMouseDragStart(MouseEventArgs e)
    {
      base.OnMouseDragStart(e);

      if (m_Row != null || e.Button != MouseButtons.Left) return; //only for header cells
      if (!m_Grid.ColumnRepositionAllowed) return;

      m_Grid.m_RepositioningColumn = m_Column;
      m_GrabX = e.X;
      m_InvertRect = new Rectangle();
    }

    protected internal override void OnMouseDrag(MouseEventArgs e)
    {
      base.OnMouseDrag(e);
      if (m_Grid.m_RepositioningColumn == null) return;

      var rect = DisplayRegion;
      rect.Offset(e.X - m_GrabX, 0);
      rect.X = Math.Max(0, rect.X);
      rect.X = Math.Min(m_Grid.CellView.Right - (int)(m_Grid.m_RepositioningColumn.Width*m_Host.Zoom), rect.X);
      rect.Y = 1;
      rect.Height = m_Host.Height - 1;
      invert();
      m_InvertRect = m_Host.RectangleToScreen(rect);
      ControlPaint.FillReversibleRectangle(m_InvertRect, Color.Beige);
    }

    protected internal override void OnMouseDragRelease(MouseEventArgs e)
    {
      base.OnMouseDragRelease(e);
      m_InvertRect = new Rectangle();
      if (m_Grid.m_RepositioningColumn == null) return;

      m_Grid.m_RepositioningColumn = null;
      var elm = m_Host.GetClickableElementAt(new Point(e.X, e.Y)) as CellElement;
      if (elm != null)
        m_Column.RepositionTo(elm.m_Column);
      else if (e.X >= m_Host.Elements.Where(c => c is ColumnResizeElement)
                            .Max(c => c.DisplayRegion.X + c.DisplayRegion.Width))
        m_Column.RepositionTo(m_Grid.Columns.LastOrDefault());
      else if (e.X <= 0)
        m_Column.RepositionTo(m_Grid.Columns.FirstOrDefault());
      else
        invert();
      m_Grid.CellView.Invalidate();
    }

    protected internal override void OnMouseDragCancel(EventArgs e)
    {
      m_Grid.m_RepositioningColumn = null;
      invert();
      base.OnMouseDragCancel(e);
    }


    /// <summary>
    /// Dispatches action that this cell element was selected
    /// </summary>
    protected virtual void DispatchSelection()
    {
      m_Column.DispatchCellSelection(this);
    }

    #endregion

    #region .pvt .impl

    private void invert()
    {
      if (m_InvertRect.X != 0 && m_InvertRect.Y != 0)
        ControlPaint.FillReversibleRectangle(m_InvertRect, Color.Beige);
    }


    private Style getBaseStyle()
    {
      var baseStyle = m_Column.HeaderStyle;
      if (m_Row != null)
      {
        var grid = m_Grid;
        var columnSelectionStyled = grid.MultiSelect || !grid.CellSelectionAllowed ||
                                    m_Column.HasCellSelection;
        if (grid.IsRowSelected(m_Row) && columnSelectionStyled)
        {
          baseStyle = m_Column.SelectedStyle;
        }
        else
          baseStyle = m_Column.Style;
      }
      return baseStyle;
    }

    private Pen getPen(LineStyle style)
    {
      var result = new Pen(style.Color);
      result.Width = style.Width;
      result.DashStyle = style.DashStyle;
      return result;
    }

    private Brush getBrush(Style style)
    {
      switch (style.BGKind)
      {
        case BGKind.Hatched:
        {
          var result = new HatchBrush(style.BGHatchStyle, style.BGHatchColor,
                                      style.BGColor);
          return result;
        }

        case BGKind.VerticalGradient:
        {
          var result = new LinearGradientBrush(Region, style.BGColor, style.BGColor2,
                                               LinearGradientMode.Vertical);
          return result;
        }
        case BGKind.HorizontalGradient:
        {
          var result = new LinearGradientBrush(Region, style.BGColor, style.BGColor2,
                                               LinearGradientMode.Horizontal);
          return result;
        }
        case BGKind.ForwardDiagonalGradient:
        {
          var result = new LinearGradientBrush(Region, style.BGColor, style.BGColor2,
                                               LinearGradientMode.ForwardDiagonal);
          return result;
        }
        case BGKind.BackwardDiagonalGradient:
        {
          var result = new LinearGradientBrush(Region, style.BGColor, style.BGColor2,
                                               LinearGradientMode.BackwardDiagonal);
          return result;
        }

        case BGKind.Solid:
        default:
        {
          var result = new SolidBrush(style.BGColor);
          return result;
        }
      }
    }

    #endregion
  }
}
