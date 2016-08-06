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

using System.Windows.Forms;
using System.Drawing;

using NFX.WinForms;

namespace NFX.WinForms.Elements
{
  /// <summary>
  /// Represents a group of radio buttons
  /// </summary>
  public class RadioGroupElement : Element
  {
    #region .ctor

    public RadioGroupElement(ElementHostControl host)
      : base(host)
    {
    }

    #endregion


    #region Private Fields
      private List<RadioButtonElement> m_RadioList = new List<RadioButtonElement>();
      private Dictionary<object, object> m_Items = new Dictionary<object, object>();
      private int m_UpdateCount;

      private int m_ButtonVSpacing = 8;
      private Padding m_Padding = new Padding(2,2,2,2);

      private RadioButtonElement m_CheckedElement;
    #endregion


    #region Properties
      /// <summary>
      /// Returns selected item or null
      /// </summary>
      public RadioButtonElement CheckedElement
      {
        get
         {
           return m_CheckedElement;
         }
      }


      /// <summary>
      /// Returns nesting depth of BeginUpdate() call
      /// </summary>
      public int UpdateCount
      {
        get {return m_UpdateCount;}
      }


      public int ButtonVSpacing
      {
        get { return m_ButtonVSpacing; }
        set
        {
          BeginUpdate();
           m_ButtonVSpacing = value;
          EndUpdate();
        }
      }

      public Padding Padding
      {
        get { return m_Padding; }
        set
        {
          BeginUpdate();
           m_Padding = value;
          EndUpdate();
        }
      }


    #endregion


     #region Events

      public event EventHandler CheckedChanged;
     #endregion


    #region Public

       public void BeginUpdate()
       {
         m_UpdateCount++;
       }


       public void EndUpdate()
       {
         if (m_UpdateCount==0)
           throw new WFormsException(StringConsts.END_UPDATE_MISMATCH_ERROR);

          m_UpdateCount--;

         if (m_UpdateCount==0)
            rebuild();
       }


       public void AddItem(object key, object value)
       {
         BeginUpdate();
          m_Items.Add(key, value);
         EndUpdate();
       }

       public void RemoveItem(object key, object value)
       {
         BeginUpdate();
          m_Items.Remove(key);
         EndUpdate();
       }

       public void ClearItems()
       {
         BeginUpdate();
          m_Items.Clear();
         EndUpdate();
       }


      /// <summary>
      /// Tries to find a button with specified key and checks it. Returns true if button could be found
      /// </summary>
       public bool CheckButtonWithKey(object key)
       {
         m_CheckedElement = null;

         foreach (RadioButtonElement other in m_RadioList)
           other.Checked = false;

         foreach (RadioButtonElement rbt in m_RadioList)
           if (equal(rbt.Key, key))
           {
             rbt.Checked = true;
             m_CheckedElement = rbt;

             OnCheckedChanged(EventArgs.Empty);

             return true;
           }

         OnCheckedChanged(EventArgs.Empty);
         return false;
       }


    #endregion


    #region Protected

       protected override void Destructor()
       {
         deleteAllButtons();
         base.Destructor();
       }


      protected internal override void Paint(Graphics gr)
      {
        //BaseApplication.Theme.PartRenderer.RadioGroup(gr, Region, MouseIsOver, FieldControlContext);
      }


      protected override void ZOrderChanged()
      { //bring all "child" radio buttons to top
        foreach(RadioButtonElement rbt in m_RadioList)
        {
         rbt.ZOrder = this.ZOrder + 1;
         foreach (Element elm in rbt.OwnedElements)
          elm.ZOrder = this.ZOrder + 1;
        }
        base.ZOrderChanged();
      }

      protected override void VisibleChanged()
      {
        foreach (RadioButtonElement rbt in m_RadioList)
        {
          rbt.Visible = this.Visible;
          foreach (Element elm in rbt.OwnedElements)
            elm.Visible = this.Visible;
        }

        base.VisibleChanged();
      }

      protected override void EnabledChanged()
      {
        foreach (RadioButtonElement rbt in m_RadioList)
        {
          rbt.Enabled = this.Enabled;
          foreach (Element elm in rbt.OwnedElements)
            elm.Enabled = this.Enabled;
        }

        base.EnabledChanged();
      }

      protected override void MouseTransparentChanged()
      {
        foreach (RadioButtonElement rbt in m_RadioList)
        {
          rbt.MouseTransparent = this.MouseTransparent;
          foreach (Element elm in rbt.OwnedElements)
            elm.MouseTransparent = this.MouseTransparent;
        }

        base.MouseTransparentChanged();
      }



      protected override void RegionChanged()
      {
        BeginUpdate();
        EndUpdate();

        base.RegionChanged();
      }

      protected virtual void OnCheckedChanged(EventArgs e)
      {
        if (CheckedChanged != null) CheckedChanged(this, e);
      }

      protected override void FieldControlContextChanged()
      {
        //base.FieldControlContextChanged();

        foreach( RadioButtonElement rbt in m_RadioList)
        {
          rbt.FieldControlContext = FieldControlContext;
          foreach( Element elm in rbt.OwnedElements)
             elm.FieldControlContext = FieldControlContext;
        }
      }


    #endregion


    #region Private Utils

         private const string BUTTON_TAG = "Button";

         private bool equal(object o1, object o2)
         {
           if ((o1==null)||(o2==null))
              return false;

           if (o1.GetType() != o2.GetType())
              return false;

           if (o1 is IComparable)
            return ((IComparable) o1).CompareTo(o2) == 0;

           return false;
         }


         private void deleteAllButtons()
         {
           m_CheckedElement = null;

           foreach (RadioButtonElement rbt in m_RadioList)
             rbt.Dispose();

           m_RadioList.Clear();
         }


         private void rebuild()
         {
           object wasCheckedKey = null;

           if (m_CheckedElement!=null)
             wasCheckedKey = m_CheckedElement.Key;

           m_CheckedElement = null;

           deleteAllButtons();

           if (m_Items.Count==0) return;

           int fh = Host.CurrentFontHeight;

           int clientHeight = Region.Height - Padding.Vertical;
           int rowHeight = fh + m_ButtonVSpacing;
           int rowCount = clientHeight / rowHeight;

           //see if last row fits without trailing spacing
           if ((clientHeight-(rowCount*rowHeight))>=fh) rowCount++;

           if (rowCount<1)
            rowCount =1;

           int colCount = (int)(m_Items.Count / rowCount) + (((m_Items.Count % rowCount)>0)? 1 : 0);

           int colWidth = (int)((Region.Width- Padding.Horizontal) / colCount);

           RadioButtonElement btn;
           TextLabelElement lbl;

           int x = Region.Left+m_Padding.Left;


           IDictionaryEnumerator enm = m_Items.GetEnumerator();

           try
           {
             for (int col = 1; col <= colCount; col++)
             {

               int y = Region.Top + m_Padding.Top;
               for (int row = 1; row <= rowCount; row++)
               {
                  if (!enm.MoveNext()) return;
                  m_RadioList.Add(btn = new RadioButtonElement(Host));
                  btn.Region = new Rectangle(x, y, fh, fh);
                  btn.ZOrder = ZOrder + 1;
                  btn.MouseClick += buttonClick;
                  btn.Key = enm.Key;
                  btn.OwnedElements.Add(lbl = new TextLabelElement(Host));

                  if (wasCheckedKey!=null)
                     if (enm.Key==wasCheckedKey)
                     {
                       btn.Checked = true;
                       m_CheckedElement = btn;
                     }

                  lbl.Region = new Rectangle(x + fh, y, colWidth - fh, fh);
                  lbl.ZOrder = btn.ZOrder;
                  lbl.Text = (enm.Value != null)? " "+enm.Value.ToString() : string.Empty;
                  lbl.MouseClick += buttonClick;
                  lbl.Tags[BUTTON_TAG] = btn;

                  y += fh + m_ButtonVSpacing;
               }
               x+= colWidth;
             }
           }
           finally
           {
              FieldControlContextChanged();
           }
         }

         //assumption: sender is either radio button or label with named tag pointing to corresponding button
         private void buttonClick(object sender, EventArgs e)
         {
           if (sender==null) return; //safeguard

           RadioButtonElement rbt = sender as RadioButtonElement;

           if (rbt==null) //it is a text label, not button
           {
             rbt = (sender as TextLabelElement).Tags[BUTTON_TAG] as RadioButtonElement;
           }


           rbt.Checked = !rbt.Checked;

           if (rbt.Checked)
           {
             foreach (RadioButtonElement other in m_RadioList)
                if (other!=rbt) other.Checked = false;

             m_CheckedElement = rbt;
           }
           else
             m_CheckedElement = null;

           OnCheckedChanged(EventArgs.Empty);
         }

    #endregion

  }

}
