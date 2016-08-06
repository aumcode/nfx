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

using NFX.Environment;

namespace NFX.WinForms.Controls.GridKit
{


  /// <summary>
  /// Represents grid column definition
  /// </summary>
  public abstract class Column : DisposableObject, IConfigurable, IConfigurationPersistent
  {

      #region CONSTS
       public const int    MIN_MIN_WIDTH        = 1;

       public const string CONFIG_WIDTH_ATTR    = "width";
       public const string CONFIG_SORT_ATTR     = "sort";
       public const string CONFIG_VISIBLE_ATTR  = "visible";

     #endregion

      protected Column(Grid grid, string id, int fieldIndex)
      {
        m_Grid = grid;
        m_ID = id ?? Guid.NewGuid().ToString();
        m_FieldIndex = fieldIndex;
        m_Grid.RegisterColumn(this);
        m_Style = new Style(grid, grid.Style);
        m_HeaderStyle = new Style(grid, grid.HeaderStyle);
        m_SelectedStyle = new Style(grid, grid.SelectedStyle);
      }

      protected override void Destructor()
      {
        m_Grid.UnregisterColumn(this);
        base.Destructor();
      }


      #region Private Fiels

         private Grid m_Grid;

         private string m_ID;

         private string m_Description;

         private int m_FieldIndex;

         private string m_Title;

         private int m_Width;
         private int m_MinWidth = MIN_MIN_WIDTH;

         private Style m_Style;
         private Style m_HeaderStyle;
         private Style m_SelectedStyle;


         private string m_FormatString;

         private bool m_HasCellSelection;

         private bool m_Visible = true;


         private bool m_SortingAllowed;
         private SortDirection m_SortDirection;

      #endregion


      #region Pub Fields

        /// <summary>
        /// Handy field for attaching some business-related object to a column
        /// </summary>
        public object ArbitraryData;

      #endregion


      #region Events

        /// <summary>
        /// Occurs when user select a cell that belongs to this column
        /// </summary>
        public event CellSelectionEventHandler CellSelection;

      #endregion

      #region Properties

         public Grid Grid
         {
           get { return m_Grid; }
         }

         /// <summary>
         /// Provides unique column ID
         /// </summary>
         public string ID
         {
           get { return m_ID; }
         }


         /// <summary>
         /// Provides column description
         /// </summary>
         public string Description
         {
           get { return m_Description??string.Empty;}
           set { m_Description = value;}
         }

         /// <summary>
         /// Returns field index - this may be handy for associating column in array position so
         /// column accessors may read data from list/array by index for faster response times
         /// </summary>
         public int FieldIndex
         {
           get { return m_FieldIndex;}
         }

         public abstract Type DataType { get; }

         /// <summary>
         /// Provides the title of the column. If title not set the column ID is returned
         /// </summary>
         public string Title
         {
           get { return m_Title ?? ID;}
           set
           {
             if (m_Title==value) return;
             m_Title = value;
             m_Grid.columnAttributesChanged(this);
           }
         }


         /// <summary>
         /// Gets/sets columns width
         /// </summary>
         public int Width
         {
           get { return m_Width;}
           set
           {
             if (value<m_MinWidth) value = m_MinWidth;

             if (m_Width==value) return;
             m_Width = value;
             m_Grid.columnAttributesChanged(this);
           }
         }


         /// <summary>
         /// Gets/sets columns minimum width
         /// </summary>
         public int MinWidth
         {
           get { return m_MinWidth;}
           set
           {
             if (value<MIN_MIN_WIDTH) value = MIN_MIN_WIDTH;

             if (m_MinWidth==value) return;
             m_MinWidth = value;

             if (m_Width<m_MinWidth)
               Width = MinWidth;//this may trigger grid rebuild
           }
         }


         /// <summary>
         /// Provides column formatting string
         /// </summary>
         public string FormatString
         {
           get { return m_FormatString ?? string.Empty;}
           set
           {
             if (m_FormatString==value) return;
             m_FormatString = value;
             m_Grid.columnAttributesChanged(this);
           }
         }

         /// <summary>
         /// Determines whether column is shown in grid
         /// </summary>
         public bool Visible
         {
           get { return m_Visible;}
           set
           {
             if (m_Visible==value) return;
             m_Visible = value;
             m_Grid.columnAttributesChanged(this);
           }
         }


         /// <summary>
         /// Determines whether this column may be sorted by
         /// </summary>
         public bool SortingAllowed
         {
           get { return m_SortingAllowed; }
           set
           {
             if (m_SortingAllowed==value) return;
             m_SortingAllowed = value;
             m_Grid.columnAttributesChanged(this);
           }
         }


         /// <summary>
         /// Returns/Sets sort direction for this column.
         /// Grid and column sorting must be allowed otherwise setting this property has no effect and get
         ///  always returns SortDirection.None
         /// </summary>
         public SortDirection SortDirection
         {
           get
           {
            if (m_Grid.SortingAllowed && m_SortingAllowed)  return m_SortDirection;
            return SortDirection.None;
           }
           set
           {
             if (!m_Grid.SortingAllowed || !m_SortingAllowed) return;
             if (m_SortDirection==value) return;
             m_SortDirection = value;

             m_Grid.columnSortChanged(this);
           }
         }




         /// <summary>
         /// Indicartes whether this column has selected cell
         /// </summary>
         public bool HasCellSelection
         {
           get { return m_HasCellSelection;}
         }


         /// <summary>
         /// Returns style for data cells in this column
         /// </summary>
         public Style Style
         {
           get { return m_Style;}
         }

         /// <summary>
         /// Returns style for header cell in this column
         /// </summary>
         public Style HeaderStyle
         {
           get { return m_HeaderStyle;}
         }

         /// <summary>
         /// Returns style for cell in this column for selected row
         /// </summary>
         public Style SelectedStyle
         {
           get { return m_SelectedStyle;}
         }



      #endregion

      #region Public Methods

        public abstract bool HasValueInRow(object row);

        public abstract object GetValueFromRow(object row);

        public abstract string GetCommentFromRow(object row);


        /// <summary>
        /// Override to create instance of cell view specific to particular column and row
        /// </summary>
        public virtual CellElement MakeCellElementInstance(object row)
        {
          return new CellElement(m_Grid.m_CellView, row, this);
        }

        // IConfigurable Members
        public void Configure(IConfigSectionNode node)
        {
          Grid.BeginBatchChange();
          try
          {
            Width   = node.AttrByName(CONFIG_WIDTH_ATTR).ValueAsInt(m_Width);
            Visible = node.AttrByName(CONFIG_VISIBLE_ATTR).ValueAsBool(m_Visible);

            if (SortingAllowed)
              SortDirection = node.AttrByName(CONFIG_SORT_ATTR).ValueAsEnum(SortDirection.None);
          }
          finally
          {
            Grid.EndBatchChange();
          }
        }

        // IConfigurationPersistent Members
        public void PersistConfiguration(ConfigSectionNode node)
        {
          var cn = node.AddChildNode(Grid.CONFIG_COLUMN_SECTION);
          cn.AddAttributeNode(Grid.CONFIG_ID_ATTR, m_ID);
          cn.AddAttributeNode(CONFIG_WIDTH_ATTR,   m_Width);
          cn.AddAttributeNode(CONFIG_VISIBLE_ATTR, m_Visible);

          if (SortingAllowed)
            cn.AddAttributeNode(CONFIG_SORT_ATTR, m_SortDirection);
        }


        /// <summary>
        /// Converts/formats cell object value as string so it can be painted. This implementation relies on FormatString.
        /// Row instanced is also passed so formatting may be done per particular row state
        /// Override to perform cell-specific conversions/formatting
        /// </summary>
        public virtual string RepresentValueAsString(object row, object value)
        {
           if (string.IsNullOrEmpty(m_FormatString))
                return value.ToString();

          return string.Format(FormatString, value);
        }

        /// <summary>
        /// Dispatches appropriate events and performs row selection in the grid
        /// </summary>
        public virtual void DispatchCellSelection(CellElement cell)
        {
          foreach(var col in m_Grid.m_Columns)
            col.m_HasCellSelection = false;

          if (m_Grid.CellSelectionAllowed)
          {
            m_HasCellSelection = true;
            OnCellSelection(m_Grid.SelectedCell, cell);
          }

          m_Grid.DispatchCellSelection(cell);
        }


        /// <summary>
        /// Repositions this column in place of other
        /// </summary>
        public void RepositionTo(Column other)
        {
          m_Grid.RepositionColumn(this, other);
        }

      #endregion


      #region Protected

        /// <summary>
        /// Invokes event
        /// </summary>
        protected virtual void OnCellSelection(CellElement oldCell, CellElement newCell)
        {
          if (CellSelection!=null) CellSelection(oldCell, newCell);
        }

      #endregion
  }





  /// <summary>
  /// Event handler that gets data for columns from rows
  /// </summary>
  public delegate TValue GetValueHandler<TRow, TValue>(Column<TRow, TValue> column, TRow row);

  /// <summary>
  /// Event handler that determine whether row has data for this column
  /// </summary>
  public delegate bool GetHasValueHandler<TRow, TValue>(Column<TRow, TValue> column, TRow row);


  /// <summary>
  /// Event handler that gets comment value(if any) for the cell
  /// </summary>
  public delegate string GetCommentHandler<TRow>(Column column, TRow row);

  /// <summary>
  /// Represents typed grid column definition
  /// </summary>
  public class Column<TRow, TValue> : Column
  {
     #region .ctor
      public Column(Grid grid, string id, int idx) : base(grid, id, idx)
      {
      }

      public Column(Grid grid, string id) : base(grid, id, 0)
      {
      }

     #endregion


     #region Properties

         public override Type DataType
         {
           get { return typeof(TValue); }
         }


     #endregion

     #region Events

        /// <summary>
        /// Invoked to extract data value from row
        /// </summary>
        public GetValueHandler<TRow, TValue> GetValue;

        /// <summary>
        /// Invoked to determine whether a row has data for this column
        /// </summary>
        public GetHasValueHandler<TRow, TValue> GetHasValue;

        /// <summary>
        /// Invoked to get comment value(if any) for the cell
        /// </summary>
        public GetCommentHandler<TRow> GetComment;

     #endregion

     #region Public Methods

        /// <summary>
        /// Returns value for specified row. Base implementation calls event handler
        /// </summary>
        public virtual TValue GetValueFromRow(TRow row)
        {
          if (GetValue!=null)
            return GetValue(this, row);
          else
            return default(TValue);
        }

        /// <summary>
        /// Returns true when specified row has value. Base implementation calls event handler
        /// </summary>
        public virtual bool HasValueInRow(TRow row)
        {
          if (GetHasValue!=null)
            return GetHasValue(this, row);
          else
            return false;
        }

        /// <summary>
        /// Returns comment for row
        /// </summary>
        public virtual string GetCommentFromRow(TRow row)
        {
          if (GetComment!=null)
            return GetComment(this, row);
          else
            return null;
        }


        public override bool HasValueInRow(object row)
        {
          if (row==null) return false;
          if (row is TRow)
           return this.HasValueInRow((TRow)row);
          else
           return false;
        }

        public override object GetValueFromRow(object row)
        {
          if (row==null) return null;
          if (row is TRow)
           return this.GetValueFromRow((TRow)row);
          else
           return null;
        }

       public override string GetCommentFromRow(object row)
       {
         if (row==null || row is TRow)
           return this.GetCommentFromRow((TRow)row);
         else
           return null;
       }


     #endregion
  }


  internal class ColumnList : List<Column>{}


}
