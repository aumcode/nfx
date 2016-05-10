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
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.ComponentModel;

using System.Windows.Forms;
using System.Drawing;
using System.Drawing.Drawing2D;


using NFX.Environment;
using NFX.WinForms.Elements;

namespace NFX.WinForms.Controls.GridKit
{
  /// <summary>
  /// Implements a Grid control base. DataRowSource is a list of row objects - if it changes in separate thread then it is row's responsibility to synchronize
  ///  accessors as this grid makes no assumptions about thread safety
  /// </summary>
  public class Grid : Control, IConfigurable, IConfigurationPersistent
  {
     #region CONSTS
       public const string CONFIG_GRID_SECTION = "grid";
       public const string CONFIG_COLUMN_SECTION = "column";
       public const string CONFIG_ID_ATTR = "id";
       
       public const int MIN_ROW_HEIGHT = 1;
       public const int DEFAULT_ROW_HEIGHT = 22;
       
       public const int REBUILD_TIMEOUT_MS = 100;
     #endregion
  
  
     #region .ctor

       public Grid() : base ()
       {
         //SetStyle(ControlStyles.OptimizedDoubleBuffer, true); //the CellView is already dbl buffered
         SetStyle(ControlStyles.UserPaint, true);
         SetStyle(ControlStyles.AllPaintingInWmPaint, true); //no ERASE BACKGROUND
         
         SetStyle(ControlStyles.UserMouse, true);

         base.DoubleBuffered = true;
         
         UpdateStyles();
         
         m_ForeBrush = new SolidBrush(ForeColor);
         m_Style = new Style(this, null);
         m_HeaderStyle = new Style(this, null);
         m_SelectedStyle = new Style(this, null);
         
         BuildDefaultStyle(m_Style);
         BuildDefaultHeaderStyle(m_HeaderStyle);
         BuildDefaultSelectedStyle(m_SelectedStyle);

         ColumnHidingAllowed = true;
                  
         m_CellView = new CellView() {   Parent = this , TabStop = false};

         m_CellView.MouseClick       += (_, e) => this.OnMouseClick(e);
         m_CellView.MouseDoubleClick += (_, e) => this.OnMouseDoubleClick(e);
         m_CellView.MouseDown        += (_, e) => this.OnMouseDown(e);
         m_CellView.MouseUp          += (_, e) => this.OnMouseUp(e);
         m_CellView.MouseEnter       += (_, e) => this.OnMouseEnter(e);
         m_CellView.MouseLeave       += (_, e) => this.OnMouseLeave(e);
         m_CellView.MouseHover       += (_, e) => this.OnMouseHover(e);
         m_CellView.MouseMove        += (_, e) => this.OnMouseMove(e);
         m_CellView.Click            += (_, e) => this.OnClick(e);
         
         
         m_HScrollBar = new HScrollBar() {   Parent = this, TabStop = false, Minimum=0, SmallChange=1, LargeChange =1  };
         m_HScrollBar.ValueChanged += m_HScrollBar_Scroll;
                                        
         m_VScrollBar = new VScrollBar() {   Parent = this, TabStop = false, Minimum=0, SmallChange=1   };                                          
         m_VScrollBar.ValueChanged += m_VScrollBar_Scroll;   
         
         m_DataSourceChangedNotificationTimer = new Timer();
         m_DataSourceChangedNotificationTimer.Interval = REBUILD_TIMEOUT_MS;
         m_DataSourceChangedNotificationTimer.Enabled = false;
         m_DataSourceChangedNotificationTimer.Tick += new EventHandler((o, e) =>
                                                       {
                                                        if (m_RepositioningColumn!=null || m_ResizingColumn!=null ) return;//while grid is being manipulated dont flush timer just yet
                                                        m_DataSourceChangedNotificationTimer.Stop(); 
                                                        notifyDataSourceChanged(m_DataSourceChangedNotificationRow);
                                                        m_DataSourceChangedNotificationRow = null; 
                                                       });
       }

       protected override void Dispose(bool disposing)
       {
         base.Dispose(disposing);
         m_Disposed = true;
       }

       

    #endregion


    #region Private Fields
  
      private bool m_Disposed;
  
      private string m_ID;
      
      internal HScrollBar m_HScrollBar;
      internal VScrollBar m_VScrollBar;
      
      internal CellView m_CellView;   
      
      private bool m_ReadOnly;
      private bool m_MultiSelect;
      private bool m_MultiSelectWithCtrl = true;

      private Brush m_ForeBrush;
      private Style m_Style;
      private Style m_HeaderStyle;
      private Style m_SelectedStyle;
      
      private IList m_DataRowSource;
      
      private bool m_CellSelectionAllowed;
      private bool m_ColumnResizeAllowed = true;
      private bool m_ColumnRepositionAllowed = true;
      private bool m_SortingAllowed = true;
      
      
      private CellElement m_SelectedCell;
      private List<object> m_SelectedRowsList = new List<object>();
      
      internal ColumnList m_Columns = new ColumnList();
      
      
      private RowMap m_RowMap = new RowMap();
       
      private int m_OrigDataRowHeight = 0;
      private int m_DataRowHeight     = DEFAULT_ROW_HEIGHT;
      private int m_HeaderRowHeight   = DEFAULT_ROW_HEIGHT;      
      
      
      internal CommentElement m_CommentElement;
      
      private Timer m_DataSourceChangedNotificationTimer;
      private object m_DataSourceChangedNotificationRow;                                
  
      private int m_BatchChangeCounter;
      private bool m_BatchChangeNeedsRebuild;
      
      internal Column m_RepositioningColumn;
      internal Column m_ResizingColumn;
  
    #endregion


    #region Properties
    
    
      /// <summary>
      /// Specifies ID for this grid instance. IDs are needed for grid config persistence
      /// </summary>
      public string ID
      {
        get { return m_ID ?? string.Empty; }
        set { m_ID = value;}
      }
    
    
      /// <summary>
      /// Determines whether grid data can be modified
      /// </summary>
      public bool ReadOnly
      {
        get { return m_ReadOnly; }
        set { m_ReadOnly = value;}
      }
      
      /// <summary>
      /// Determines whether grid supports selection of more than one row
      /// </summary>
      public bool MultiSelect
      {
        get { return m_MultiSelect; }
        set 
        {           
          if (m_MultiSelect!=value)
          {
            m_MultiSelect = value;
            UnSelectAllRows();
          }  
        }
      }
      
      
      /// <summary>
      /// Determines whether grid requires control to be pressed for multi-select
      /// </summary>
      public bool MultiSelectWithCtrl
      {
        get { return m_MultiSelectWithCtrl; }
        set 
        {
            m_MultiSelectWithCtrl = value;
        }
      }
      
      
      /// <summary>
      /// Determines whether grid supports cell selection. Cell selection only works when Multiselect=false
      /// </summary>
      public bool CellSelectionAllowed
      {
        get { return m_CellSelectionAllowed; }
        set 
        {
            m_CellSelectionAllowed = value;
            m_SelectedCell = null;
        }
      }
      
      /// <summary>
      /// Determines whether user can resize columns
      /// </summary>
      public bool ColumnResizeAllowed
      {
        get { return m_ColumnResizeAllowed; }
        set 
        {
            m_ColumnResizeAllowed = value;
        }
      }
      
      /// <summary>
      /// Determines whether user can reposition columns
      /// </summary>
      public bool ColumnRepositionAllowed
      {
        get { return m_ColumnRepositionAllowed; }
        set 
        {
            m_ColumnRepositionAllowed = value;
        }
      }
      
      /// <summary>
      /// Determines whether user can hide columns
      /// </summary>
      public bool ColumnHidingAllowed { get; set; }
      
      /// <summary>
      /// Determines whether user can sort data by clicking on column headers
      /// </summary>
      public bool SortingAllowed
      {
        get { return m_SortingAllowed; }
        set 
        {
           if (m_SortingAllowed!=value)
           {
            m_SortingAllowed = value;
            rebuildAllCells();
           } 
        }
      }

      /// <summary>
      /// When true Ctrl+A key will select all rows
      /// </summary>
      public bool SelectAllRowsAllowed { get; set; }

      /// <summary>
      /// Returns a column being repositioned
      /// </summary>
      public Column RepositioningColumn
      {
        get { return m_RepositioningColumn; }
      }
      
      /// <summary>
      /// Returns a column being resized
      /// </summary>
      public Column ResizingColumn
      {
        get { return m_ResizingColumn; }
      }
      
      /// <summary>
      /// Indicates whether user is changing layout of columns or rows such as size or column positions
      /// </summary>
      public bool UserChangingLayout
      {
        get { return m_RepositioningColumn!=null || m_ResizingColumn!=null;}
      }
      
      
      /// <summary>
      /// Returns the last cell that was selected in the grid  
      /// </summary>
      [Browsable(false)]
      public CellElement SelectedCell { get { return m_SelectedCell; } }
      
      /// <summary>
      /// Gets/sets the text color of the grid.
      /// </summary>
      public new Color ForeColor
      { 
        get { return base.ForeColor; }
        set { base.ForeColor = value; m_ForeBrush = new SolidBrush(value); }
      }

      [Browsable(false)]
      internal Brush ForeBrush { get { return m_ForeBrush; } }

      /// <summary>
      /// Returns a style object for all data cells in the grid 
      /// </summary>
      //[Browsable(false)]
      public Style Style
      {
        get { return m_Style; }
        set { m_Style = value; }
      }
      
      /// <summary>
      /// Returns a style object for all header cells in the grid
      /// </summary>
      //[Browsable(false)]
      public Style HeaderStyle
      {
        get { return m_HeaderStyle; }
        set { m_HeaderStyle = value; }
      }
      
      
      /// <summary>
      /// Returns a style object for all data cells in the grid which are in selected rows
      /// </summary>
      //[Browsable(false)]
      public Style SelectedStyle
      {
        get { return m_SelectedStyle; }
        set { m_SelectedStyle = value; }
      }
      
      
      /// <summary>
      /// Provides default height for all data rows
      /// </summary>
      public int DataRowHeight
      {
        get { return m_DataRowHeight; }
        set
        {
          if (m_DataRowHeight!=value)
          {
            if (m_OrigDataRowHeight == 0)
              m_OrigDataRowHeight = m_DataRowHeight;
            m_DataRowHeight= value<MIN_ROW_HEIGHT ? MIN_ROW_HEIGHT : value;
            rebuildAllCells();
          }
        }
      }
      
      /// <summary>
      /// Provides default height for header row (column captions)
      /// </summary>
      public int HeaderRowHeight
      {
        get { return m_HeaderRowHeight; }
        set
        {
          if (m_HeaderRowHeight!=value)
          {
            m_HeaderRowHeight= value<MIN_ROW_HEIGHT ? MIN_ROW_HEIGHT : value;
            rebuildAllCells();
          }
        }
      }
      
      /// <summary>
      /// Enumerates selected rows
      /// </summary>
      [Browsable(false)]
      public IEnumerable<object> SelectedRows
      {
        get { return m_SelectedRowsList; }
      }
      
      
      /// <summary>
      /// Binds grid to a source of rows
      /// </summary>
      [Browsable(false)]
      public IList DataRowSource
      {
        get { return m_DataRowSource; }
        set 
        {
          m_DataRowSource = value;
          dataRowSourceChanged();
        }
      }

      /// <summary>
      /// Accesses columns
      /// </summary>
      public IList<Column> Columns
      {
        get { return m_Columns.AsReadOnly();}
      }
    
    
      /// <summary>
      /// Returns reference to cellview - an area where cells are displayed
      /// </summary>
      [Browsable(false)] 
      public CellView CellView
      {
        get { return m_CellView; }
      }
    
    #endregion 
    
    
    #region Events
    
     /// <summary>
     /// Occurs after data row source has changed and grid displays whole different dataset
     /// </summary>
     public event EventHandler DataRowSourceChanged;
     
     /// <summary>
     /// Occurs after rows have been selected/unselected
     /// </summary>
     public event EventHandler RowSelectionChanged;
     
     /// <summary>
     /// Occurs after columns have been added/deleted
     /// </summary>
     public event EventHandler ColumnsChanged;
     
     /// <summary>
     /// Occurs after columns attributes changed such as visibility,width, titile etc..
     /// </summary>
     public event ColumnAttributesChangedEventHandler ColumnAttributesChanged;
     
     /// <summary>
     /// Occurs after columns sort changed 
     /// </summary>
     public event ColumnAttributesChangedEventHandler ColumnSortChanged;
     
     
     /// <summary>
     /// Occurs when external data source changes and notifies the grid
     /// </summary>
     public event NotifyDataSourceChangedEventHandler NotifyDataSourceChanged;
     
     /// <summary>
     /// Occurs when user selects the cell
     /// </summary>
     public event CellSelectionEventHandler CellSelection;
     
    #endregion
    
    
    #region Public Methods
    
        
      /// <summary>
      /// Notifies grid that data/metadata(such as comment) has changed for specified row, and row has to be redrawn if it is visible. 
      /// If row is null the grid is rebuilt. Pass null when rows where added or deleted to/from grid data source
      /// Returns true if grid was rebuilt when row was visible or null.
      /// This method is NOT THREAD SAFE and runs in GUI thread.
      /// This method does call aggregation, that is - when this method is called many times within timespan it will rebuild grid only once at the end.
      /// </summary>
      public bool SendDataSourceChangedNotification(object row)
      {
         if (row==null || m_RowMap.HasRow(row))
         {
           if (m_DataSourceChangedNotificationRow==null)
            m_DataSourceChangedNotificationRow = row;
           else
            m_DataSourceChangedNotificationRow = null;
           
           m_DataSourceChangedNotificationTimer.Start();
           return true;
         }
         return false;
      }
      
      
      /// <summary>
      /// Adds row to selection. If row is not in grid's source rowset then it will never be highlighted in grid (because it never shows in grid then)
      /// </summary>
      public void SelectRow(object row)
      {                      
        if (!IsRowSelected(row))
        {
          if (!m_MultiSelect) m_SelectedRowsList.Clear();
          m_SelectedRowsList.Add(row);
          rowSelectionChanged();
        }
      }
      
      /// <summary>
      /// Deletes row from selection. Returns true if row was selected
      /// </summary>
      public bool UnSelectRow(object row)
      {
        var result = m_SelectedRowsList.Remove(row);
        if (result)
          rowSelectionChanged();
        return result;
      }
      
      /// <summary>
      /// Returns true if row is selected in grid
      /// </summary>
      public bool IsRowSelected(object row)
      {
        return m_SelectedRowsList.Contains(row);
      }
      
      /// <summary>
      /// Selects all rows.
      /// The method will not select any rows if SelectAllRowsAllowed is false.
      /// <returns>Number of rows selected</returns>
      /// </summary>
      public int SelectAllRows()
      {
        if (!SelectAllRowsAllowed)
          return 0;

        m_SelectedRowsList.Clear();

        for (int i=0; i < m_DataRowSource.Count; ++i)
          m_SelectedRowsList.Add(m_DataRowSource[i]);
       
        if (m_SelectedRowsList.Count > 0) 
          rowSelectionChanged();

        return m_SelectedRowsList.Count;
      }
      
      /// <summary>
      /// Unselects all rows
      /// </summary>
      public void UnSelectAllRows()
      {
        var cnt = m_SelectedRowsList.Count;
        m_SelectedRowsList.Clear();
       
        if (cnt>0) 
          rowSelectionChanged();
      }
      
      /// <summary>
      /// Tries to find a column by its' id or returns null
      /// </summary>
      public Column TryFindColumnByID(string id)
      {
        return m_Columns.FirstOrDefault(n => string.Equals(n.ID, id, StringComparison.OrdinalIgnoreCase));
      }
    
    
      // IConfigurable Members
      /// <summary>
      /// Reads column configuration from config node that should contain [grid] sub elemnt/s. The id attribute should either match or be absent
      /// </summary>
      public void Configure(IConfigSectionNode node) //should point to node that has grids
      {
         if (node==null) return;
         
         node = findSubNodeForThisGrid(node);
         
         if (node==null || !node.Exists) return;
         
         BeginBatchChange();
         try
         {
             var idx = 0;
             foreach (ConfigSectionNode cnode in 
                      node.Children.Where(n => string.Equals(n.Name, CONFIG_COLUMN_SECTION, StringComparison.OrdinalIgnoreCase)))
             {
                var col = TryFindColumnByID(cnode.AttrByName(CONFIG_ID_ATTR).Value);
                if (col!=null)
                {
                  col.Configure(cnode);
                  m_Columns.Remove(col);
                  m_Columns.Insert(idx, col);
                  idx++;
                }  
             }
             
            columnsChanged();
         }
         finally
         {
           EndBatchChange();
         }
      }

      // IConfigurationPersistent Members
      /// <summary>
      /// Persists column configuration to config node. [grid] subnode will be created under specified node pr reused if one already exists
      /// </summary>
      public void PersistConfiguration(ConfigSectionNode node)
      {
         if (node==null) return;
         
         ConfigSectionNode gnode = findSubNodeForThisGrid(node) as ConfigSectionNode; //see if node for this grid already exists

         if (gnode!=null)//delete with all column defs that are different now
           gnode.Delete();
         
         
         gnode = node.AddChildNode(CONFIG_GRID_SECTION);
         
         if (!string.IsNullOrEmpty(m_ID))
           gnode.AddAttributeNode(CONFIG_ID_ATTR, ID);
         
         foreach(var col in m_Columns)
           col.PersistConfiguration(gnode);
      }
    
    
      /// <summary>
      /// Returns height for a row, if row==null then header row assumed (where column titles are displayed).
      /// This implementation returns Data/Header RowHeight properties. Override to do conditional row measurements depending on row instance 
      /// </summary>
      public virtual int GetRowHeight(object row)
      {                                             
        return row==null ? m_HeaderRowHeight : m_DataRowHeight;
      }
    
    
      /// <summary>
      /// Builds default style for data cells in non-selected rows
      /// </summary>
      public virtual void BuildDefaultStyle(Style style)
      {
      }
      
      /// <summary>
      /// Builds default style for header row cells - column titles
      /// </summary>
      public virtual void BuildDefaultHeaderStyle(Style style)
      {
         style.HAlignment = HAlignment.Center;
         style.BorderLeft = new LineStyle(){ Color = Color.White };
         style.BorderTop = new LineStyle(){ Color = Color.White };
         style.BorderBottom = new LineStyle(){ Color = Color.FromArgb(100,100,100) };
         style.BGKind = BGKind.HorizontalGradient;
         style.BGColor = Color.White;
         style.BGColor2 = Color.Silver;
      }
      
      /// <summary>
      /// Builds default style for data cells in selected rows
      /// </summary>
      public virtual void BuildDefaultSelectedStyle(Style style)
      {
       //  style.HAlignment = HAlignment.Center;
         style.BorderTop = new LineStyle(){ Color = Color.Transparent };
         style.BorderBottom = new LineStyle(){ Color = Color.Silver};
         style.BGKind = BGKind.BackwardDiagonalGradient;//.VerticalGradient;
         style.BGColor = Color.FromArgb(199,199,255);
         style.BGColor2 = Color.FromArgb(250,250,255);
      }
      
      
      /// <summary>
      /// Dispatches appropriate events and performs row selection in the grid
      /// </summary>
      public virtual void DispatchCellSelection(CellElement cell)
      {
        if (cell.Row!=null)
        {
          if (!m_MultiSelect)
          {
            if ((Control.ModifierKeys & Keys.Control) > 0)
              UnSelectRow(cell.Row);
            else
              SelectRow(cell.Row);
          }
          else //multiselect
          {
            if ((Control.ModifierKeys & Keys.Shift) > 0)
            {
              if (!SelectedRows.Any())
              {
                SelectRow(cell.Row);
                return;
              }

              var first = m_RowMap.FindIndex(re => re.Row == SelectedRows.First());
              var last  = m_RowMap.FindIndex(re => re.Row == SelectedRows.Last());
              var cur   = m_RowMap.FindIndex(re => re.Row == cell.Row);

              if (cur < 0)
                return;

              UnSelectAllRows();

              if (last <= cur)
                for (var i = last; i <= cur; ++i)
                  SelectRow(m_RowMap[i].Row);
              else if (cur <= first)
                for (var i = cur; i <= first; ++i)
                  SelectRow(m_RowMap[i].Row);
            }
            else if (!m_MultiSelectWithCtrl || (Control.ModifierKeys & Keys.Control) > 0)
            {
              if (IsRowSelected(cell.Row))
                UnSelectRow(cell.Row);
              else
                SelectRow(cell.Row);
            }
            else
            {
              UnSelectAllRows();
              SelectRow(cell.Row);
            }
          }
        }
        
        if (m_CellSelectionAllowed)
          try
          {
            rebuildAllCells(true);//must force the rebuild or event will be invalid
            OnCellSelection(m_SelectedCell, cell);
          }finally{
            m_SelectedCell = cell;
          } 
      } 
      
      
      /// <summary>
      /// Marks the beginning of the series of changes that are going to be performed on the grid so
      ///  the grid does not have to be rebuild unnecessarily many times.
      /// The complementary method is EndBatchChange() that does rebuild the grid IF it is needed.
      /// These methods respect call nesting, so only the last call to EndBatchChange() would result in rebuild
      /// </summary>
      public void BeginBatchChange()
      {
        m_BatchChangeCounter++;
      }
      
      /// <summary>
      /// Complementary method for BeginBatchChange()
      /// </summary>
      public void EndBatchChange()
      {
        if (m_BatchChangeCounter>0) m_BatchChangeCounter--;
        
        if (m_BatchChangeCounter==0 && m_BatchChangeNeedsRebuild)
        {
          rebuildAllCells();
        }
      }
      
      
      /// <summary>
      /// Repositions one column to the position of another
      /// </summary>
      public void RepositionColumn(Column column, Column other)
      {
        if (!m_ColumnRepositionAllowed) return;
        
        var ci = m_Columns.IndexOf(column);
        var oi = m_Columns.IndexOf(other);
        
        if (ci>=0 && oi>=0)
        {
          m_Columns.Remove(column);
          m_Columns.Insert(oi, column);
        }
        
        columnsChanged();
      }
    
      /// <summary>
      /// Copies selection to clipboard
      /// </summary>
      public virtual void CopyToClipboard()
      {
        var result =new StringBuilder();
        
        if (m_CellSelectionAllowed && m_SelectedCell!=null)
        {
          var val = m_SelectedCell.Value;
          if (val!=null)
           result.Append(m_SelectedCell.RepresentValueAsString(val));
        }
        else
        {
          foreach(var row in m_SelectedRowsList)
          {
            var rowCells = m_CellView.Elements.Where(e=>(e is CellElement) && ((CellElement)e).Row == row).Cast<CellElement>().ToList();
            foreach(var col in m_Columns.Where(c=>c.Visible))
            {
              var cell = rowCells.FirstOrDefault(cl=>cl.Column==col);
              if (cell==null) continue;
              var val = cell.Value;
              if (val!=null)
              {
                result.Append(cell.RepresentValueAsString(val));
                result.Append("\t");
              }  
            }
            result.AppendLine();
          }
        } 
        
        
        System.Windows.Forms.Clipboard.SetText(result.ToString());
      }

      /// <summary>
      /// Reset data row hight to its original value it had prior to changing with assignment to DataRowHeight
      /// </summary>
      public void ResetRowHeight()
      {
        if (m_OrigDataRowHeight != 0)
          DataRowHeight = m_OrigDataRowHeight;
      }

    #endregion
    
    
    #region Protected
    
       internal void RegisterColumn(Column column)
       {
         if (m_RepositioningColumn!=null)
          throw new WFormsException(StringConsts.COLUMN_ARE_REPOSITIONING_GRID_ERROR);
         
         
         if (m_Columns.FirstOrDefault(col=>string.Equals(col.ID, column.ID, StringComparison.InvariantCultureIgnoreCase))!=null)
          throw new WFormsException(StringConsts.COLUMN_ID_EXISTS_GRID_ERROR + column.ID);
          
         m_Columns.Add(column);
         columnsChanged();
       }
       
       internal void UnregisterColumn(Column column)
       {
         if (m_RepositioningColumn!=null)
          throw new WFormsException(StringConsts.COLUMN_ARE_REPOSITIONING_GRID_ERROR);
          
         if (m_Columns.Remove(column))
           columnsChanged();
       }
       
       
       /// <summary>
       /// Performs layout of sub controls, such as element host, scroll bars etc.. 
       /// </summary>
       protected virtual void LayoutSubControls()
       {
         int vw = m_VScrollBar.Width;
         int hh = m_HScrollBar.Height;
       
         m_VScrollBar.SetBounds(Width - vw - Padding.Right, Padding.Top, vw, Height - hh - Padding.Vertical);     
         m_HScrollBar.SetBounds(Padding.Left, Height - hh - Padding.Bottom, Width - vw - Padding.Horizontal, hh);   
         
         var newWidth = m_VScrollBar.Left - 1 - Padding.Left;
         var newHeight = m_HScrollBar.Top - 1 - Padding.Top;
         var needRebuild = newHeight != m_CellView.Height;
         m_CellView.SetBounds(Padding.Left, Padding.Top, newWidth, newHeight);   
         
         if (needRebuild) rebuildAllCells();
       }
       
       protected override void SetBoundsCore(int x, int y, int width, int height, BoundsSpecified specified)
       {
         base.SetBoundsCore(x, y, width, height, specified);
         LayoutSubControls();
       }
       
       protected override void OnPaddingChanged(EventArgs e)
       {
         base.OnPaddingChanged(e);
         LayoutSubControls();
       }
  
  
       protected virtual void OnRowSelectionChanged(EventArgs e)
       {
         if (RowSelectionChanged!=null) RowSelectionChanged(this, e);
       }
       
       protected virtual void OnDataRowSourceChanged(EventArgs e)
       {
         if (DataRowSourceChanged!=null) DataRowSourceChanged(this, e);
       }
       
       protected virtual void OnColumnsChanged(EventArgs e)
       {
         if (ColumnsChanged!=null) ColumnsChanged(this, e);
       }
       
       protected virtual void OnColumnAttributesChanged(Column column)
       {
         if (ColumnAttributesChanged!=null) ColumnAttributesChanged(column);
       }
       
       protected virtual void OnColumnSortChanged(Column column)
       {
         if (ColumnSortChanged!=null) ColumnSortChanged(column);
       }

       protected virtual void OnNotifyDataSourceChanged(NotifyDataSourceChangedEventArgs e)
       {
        if (NotifyDataSourceChanged!=null) NotifyDataSourceChanged(this, e);
       }

           
       /// <summary>
       /// Invokes cell selection  event
       /// </summary>
       protected virtual void OnCellSelection(CellElement oldCell, CellElement newCell)
       {
         if (CellSelection!=null) CellSelection(oldCell, newCell);
       }

       protected void ContextMenuOpening(object sender, CancelEventArgs e)
       {
         // Don't open context menu on the header row or on non-clickable region of the grid
         var cms = sender as ContextMenuStrip;
         var mousepos = MousePosition;
         if (cms == null) return;

         var relpos = PointToClient(mousepos);
         var ele    = m_CellView.GetClickableElementAt(relpos);
         if (ele   == null || (ele is ColumnResizeElement) ||
            (ele is CellElement) && ((CellElement)ele).Row == null)
           e.Cancel = true;
       }

       protected override void OnContextMenuStripChanged(EventArgs e)
       {
         if (ContextMenuStrip != null)
         {
           ContextMenuStrip.Opening -= ContextMenuOpening;
           ContextMenuStrip.Opening += ContextMenuOpening;
         }
         base.OnContextMenuStripChanged(e);
       }

       protected override void OnMouseWheel(MouseEventArgs e)
       {
         var alt = (Control.ModifierKeys&Keys.Alt) >0;
         var ctrl = (Control.ModifierKeys&Keys.Control) >0;
         var shift = (Control.ModifierKeys&Keys.Shift) >0;
        
        
         if (ctrl)//zoom
         {
           m_CellView.UpdateZoom( m_CellView.Zoom + Math.Sign(e.Delta)* 0.1f);
           rebuildAllCells();
         }
         else  if (shift)//stretch data rows
         {
           DataRowHeight = Math.Max(DEFAULT_ROW_HEIGHT, DataRowHeight - 3*Math.Sign(e.Delta));
           rebuildAllCells(); 
         }
          else //scroll
         {
           var v = m_VScrollBar.Value;
                                     
           v -= Math.Sign(e.Delta)* (alt ? 1 : m_RowMap.Count -1);
           
           
           if (v>m_VScrollBar.Maximum-2)
            v = m_VScrollBar.Maximum-2; 

           if (v<m_VScrollBar.Minimum)
            v = m_VScrollBar.Minimum;
             
           m_VScrollBar.Value = v;  
         }                                   
         base.OnMouseWheel(e);
       }


       //protected override bool ProcessKeyPreview(ref Message m)
       //{
       //  if (m.Msg == (int)NFX.WinApi.Messages.WM_KEYDOWN)
       //  {
       //   var e = new KeyEventArgs(((Keys)((int)((long)m.WParam))) | ModifierKeys);
          
          
          
       //   if (e.KeyCode==Keys.Up)
       //   {
       //     MessageBox.Show("Up pressed and eaten");
       //     return true;
       //   }else if (e.KeyCode==Keys.Down)
       //   {
       //     MessageBox.Show("Down pressed and eaten");
       //     return true;
       //   }          
          
       //  }
       //  return base.ProcessKeyPreview(ref m); 
       //}
                      
       protected override bool ProcessDialogKey(Keys keyData)
       {
         if (keyData==(Keys.C | Keys.Control))
         {
           CopyToClipboard();
           return true;
         }
         
         if (keyData==Keys.Up)
         {
           if (m_SelectedRowsList.Count == 1)
           {
             var row = m_SelectedRowsList[0];
             var idx = m_RowMap.FindIndex(r => r.Row == row);
             if (idx == 1 && m_VScrollBar.Value > 0)
             {
               m_VScrollBar.Value -= 1;
               idx = m_RowMap.FindIndex(r => r.Row == row);
             }

             if (idx > 1)
             {
               UnSelectRow(row);
               SelectRow(m_RowMap[idx - 1].Row);
             }
           }
           else if (m_VScrollBar.Value>0)
             m_VScrollBar.Value-=1;
           return true;
         }
         
         if (keyData==Keys.Down)
         {
           if (m_SelectedRowsList.Count == 1)
           {
             var row = m_SelectedRowsList[0];
             var idx = m_RowMap.FindIndex(r => r.Row == row);
             if (idx == m_RowMap.Count-2 && m_VScrollBar.Value+idx+1 < m_VScrollBar.Maximum)
             {
               m_VScrollBar.Value += 1;
               idx = m_RowMap.FindIndex(r => r.Row == row);
             }

             if (idx > 0 && idx < m_RowMap.Count-1)
             {
               UnSelectRow(row);
               SelectRow(m_RowMap[idx + 1].Row);
             }
           }
           else if (m_VScrollBar.Value<m_VScrollBar.Maximum)
             m_VScrollBar.Value += 1;
           return true;
         }
         
         if (keyData==Keys.PageUp)
         {
           var v = m_VScrollBar.Value - m_RowMap.Count;
           if (v<0) v = 0;
           m_VScrollBar.Value = v;
           if (m_SelectedRowsList.Count == 1 && m_RowMap.Count > 1)
           {
             UnSelectRow(m_SelectedRowsList[0]);
             SelectRow(m_RowMap[1].Row);
           }
           return true;
         }
         
         if (keyData==Keys.PageDown)
         {
           if (m_VScrollBar.Maximum>2)
           {
             var v = m_VScrollBar.Value + m_RowMap.Count;
             if (v>m_VScrollBar.Maximum-1) v = m_VScrollBar.Maximum-1;
             m_VScrollBar.Value = v;
           }

           if (m_SelectedRowsList.Count == 1 && m_RowMap.Count > 1)
           {
             UnSelectRow(m_SelectedRowsList[0]);
             SelectRow(m_RowMap[1].Row);
           }
           return true;
         }
         
         if (keyData==Keys.Left)
         {
           if (m_HScrollBar.Value>0)
             m_HScrollBar.Value-=1;
           return true;
         }
         
         if (keyData==Keys.Right)
         {
           if (m_HScrollBar.Value<m_HScrollBar.Maximum)
             m_HScrollBar.Value+=1;
           return true;
         }
         
         if (keyData==Keys.Home)
         {
           m_VScrollBar.Value=0;
           return true;
         }
         
         if (keyData==Keys.End)
         {
           if (m_VScrollBar.Maximum > 2)
             m_VScrollBar.Value = m_VScrollBar.Maximum - 2;
           return true;
         }

         if (keyData == (Keys.A | Keys.Control))
         {
           if ((keyData & Keys.Shift) == Keys.Shift)
             UnSelectAllRows();
           else
             SelectAllRows();

           return true;
         }

         if (keyData == (Keys.Multiply | Keys.Control))
         {
           CellView.Zoom = 1.0f;
           ResetRowHeight();
           return true;
         }

         if ((keyData & (Keys.Add | Keys.Control)) == (Keys.Add | Keys.Control))
         {
           if ((keyData & Keys.Shift) == Keys.Shift)
             DataRowHeight += 3;
           else
             CellView.Zoom *= 1.1f;
           return true;
         }

         if ((keyData & (Keys.Subtract | Keys.Control)) == (Keys.Subtract | Keys.Control))
         {
           if ((keyData & Keys.Shift) == Keys.Shift)
             DataRowHeight = Math.Max(DEFAULT_ROW_HEIGHT, DataRowHeight - 3);
           else
             CellView.Zoom *= 0.9f;
           return true;
         }

         return base.ProcessDialogKey(keyData);
       }

    #endregion
    
    #region .pvt  .impl
       
       internal void rebuildAllCells()
       {
         rebuildAllCells(false);
       }
       
       private void rebuildAllCells(bool force)//true to force rebuild inspite of Begin/End Change 
       {
         if (!force && m_BatchChangeCounter>0)
         {
           m_BatchChangeNeedsRebuild = true;
           return;
         }
         
         try
         {
           if (m_CellView==null) return;
              
           if (m_Disposed) return;
              
           //forget all previously selected cells
           m_SelectedCell = null;
              
           m_CommentElement = null;
              
           m_CellView.DeleteAllElements();
           buildCells();
         }
         finally
         {
           if (!force)
             m_BatchChangeNeedsRebuild = false;
         }
       }
       
       
       private void m_HScrollBar_Scroll(object sender, EventArgs e)
       {
    //     m_CellView.ScrollElementsBy(10, 10,   m_HScrollBar.Value, 0, true);
         
         rebuildAllCells();
       }
       
       private void m_VScrollBar_Scroll(object sender, EventArgs e)
       {
    //     m_CellView.ScrollElementsBy(10, 10,  0, - m_HeaderRowHeight, true);
    // return;
     
         rebuildAllCells();    
       }
    
    
       private void rowSelectionChanged()
       {
         BeginBatchChange();
         try
         {
           rebuildAllCells();
           OnRowSelectionChanged(EventArgs.Empty);
         }
         finally
         {
           EndBatchChange();
         }
       }
       
       private void notifyDataSourceChanged(object row)
       {
         BeginBatchChange();
         try
         {
           updateScrollBarsMinMaxFromRowSource();
           rebuildAllCells();
           OnNotifyDataSourceChanged( new NotifyDataSourceChangedEventArgs(row));
         }
         finally
         {
           EndBatchChange();
         }  
       }
       
       private void dataRowSourceChanged()
       {
         BeginBatchChange();
         try
         {
           updateScrollBarsMinMaxFromRowSource();
           rebuildAllCells();
           OnDataRowSourceChanged(EventArgs.Empty);
         }
         finally
         {
           EndBatchChange();
         }
       }
       
       private void updateScrollBarsMinMaxFromRowSource()
       {
         IList list = m_DataRowSource;
         var max =  list!=null? list.Count : 0;
         
         if (m_VScrollBar.Value>max) m_VScrollBar.Value = max;
         
         m_VScrollBar.Maximum =  max;
       }
       
                      
       private void columnsChanged()
       {
         BeginBatchChange();
         try
         {           
           adjustHScrollBar(); 
             
           rebuildAllCells();
           OnColumnsChanged(EventArgs.Empty);
         }
         finally
         {
           EndBatchChange();
         }  
       }
       
       internal void columnAttributesChanged(Column column)
       {
         BeginBatchChange();
         try
         {           
           adjustHScrollBar(); 
             
           rebuildAllCells();
           OnColumnAttributesChanged(column);
         }
         finally
         {
           EndBatchChange();
         }  
       }
       
       internal void columnSortChanged(Column column)
       {
         BeginBatchChange();
         try
         {           
           rebuildAllCells();
           OnColumnSortChanged(column);
         }
         finally
         {
           EndBatchChange();
         }  
       }
       
       
       
       private void adjustHScrollBar()
       {
           var cnt = m_Columns.Count(c=>c.Visible);
           m_HScrollBar.Enabled = cnt>0;
           
           var max = 0;
           if (cnt>0)
            max =  cnt - 1;
            
           if (m_HScrollBar.Value>max)
             m_HScrollBar.Value = max;
            
           m_HScrollBar.Maximum = max; 
       }
    
    
       private IConfigSectionNode findSubNodeForThisGrid(IConfigSectionNode root)
       {
         if (string.IsNullOrEmpty(m_ID))
           return root.Children.FirstOrDefault(n => string.Equals(n.Name, CONFIG_GRID_SECTION, StringComparison.OrdinalIgnoreCase) && 
                                                    !n.AttrByName(CONFIG_ID_ATTR).Exists); //take first grid that has no ID assigned
         else 
           return root.Children.FirstOrDefault(n => string.Equals(n.Name, CONFIG_GRID_SECTION, StringComparison.OrdinalIgnoreCase)  &&
                                                    string.Equals(n.AttrByName(CONFIG_ID_ATTR).Value, ID, StringComparison.OrdinalIgnoreCase));
       }
       
       
       
       private void buildCells()
       {
         float zoom = m_CellView.Zoom;
         
         var y = 0;
         var rowHeight = GetRowHeight(null);
         
         m_RowMap.Clear();
         m_RowMap.Add( new RowMapEntry{ Row = null, Height = rowHeight, Top = y} );        
         
         for(int i=m_HScrollBar.Value,x=0; i<m_Columns.Count; i++)
         {
             var col = m_Columns[i];
             if (!col.Visible) continue;
             var celm = col.MakeCellElementInstance(null); //header
             celm.Region = new Rectangle(x, y, col.Width, rowHeight);
             celm.Visible = true;
             x+=col.Width+1;
         }
         
         y += rowHeight+1; 
                  
         
         if (m_DataRowSource==null) return;
         var idx = m_VScrollBar.Value;

         while( idx < m_DataRowSource.Count && (int)(y*zoom)<m_CellView.Height)
         {
           var row = m_DataRowSource[idx];
           rowHeight = GetRowHeight(row);
           m_RowMap.Add( new RowMapEntry{ Row = row, Height = rowHeight, Top = y} ); 
           
           for(int i=m_HScrollBar.Value,x=0; i<m_Columns.Count; i++)
           {
             var col = m_Columns[i];
             if (!col.Visible) continue;
             
             var celm = col.MakeCellElementInstance(row);
             celm.Region = new Rectangle(x, y, col.Width, rowHeight);
             celm.Visible = true;
             x+=col.Width+1;
             
             //var cbe = new CheckBoxElement(m_CellView);
             //cbe.Region = new Rectangle(x,y, 12,12);
             //cbe.Visible = true;
             //cbe.ZOrder = 10;
             //cbe.Checked = true;
           } 
           idx++;
           y+= rowHeight+1;
         }
         
       }
    
    #endregion
  }
  
  
  
 
  
  
}
