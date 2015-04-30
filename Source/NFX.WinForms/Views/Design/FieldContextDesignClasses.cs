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
using System.Drawing.Drawing2D;
using System.Drawing.Design;
using System.Windows.Forms;
using System.Windows.Forms.Design;

using NFX.RecordModel;

using NFX.WinForms.Elements;


namespace NFX.WinForms.Views.Design
{

  /// <summary>
  /// Custom designer for FieldContextControl and derived classes
  /// </summary>
  public class FieldContextControlDesigner : ControlDesigner
  {

    public override void Initialize(IComponent component)
    {
      base.Initialize(component);
      IComponentChangeService svc = GetService(typeof(IComponentChangeService)) as IComponentChangeService;
      if (svc != null)
      {
        svc.ComponentRemoving += doComponentRemoving;
      }
    }

    protected override void Dispose(bool disposing)
    {
      IComponentChangeService svc = GetService(typeof(IComponentChangeService)) as IComponentChangeService;
      if (svc != null)
      {
        svc.ComponentRemoving -= doComponentRemoving;
      }
      base.Dispose(disposing);
    }
    
    
    private void doComponentRemoving(object sender, ComponentEventArgs e)
    {
      if (e.Component is ModelBase)
      {
        FieldContextControl ctl = Control as FieldContextControl;
        if (ctl!=null)
        {
          if (e.Component == ctl.AttachToRecord) ctl.AttachToRecord = null;
           else
            if (e.Component == ctl.AttachToField) ctl.AttachToField = null;
        }
      }
    }

       
    protected override void OnPaintAdornments(PaintEventArgs pe)
    {
      base.OnPaintAdornments(pe);

      if (Control != null)
      {
        FieldContextControl ctl = Control as FieldContextControl;
        if (ctl!=null)
        {
          string str;
          Brush brsh = Brushes.Red;
          
          if (ctl.AttachToField != null)
           str = "->Field("+ctl.AttachToField.FieldName+")";
          else  
          { 
             if (ctl.Record!=null)
              str = ctl.Record.RecordName;
             else 
              str = "?"; 
             
             str += ".[\""+ctl.AttachToFieldName+"\"]";

             if (ctl.AttachToParentRecord)
             {
                 brsh = Brushes.Navy;
                 str = "≈>" + str;
             }
          }

          
          using (Font f = new Font("Tahoma", 6, FontStyle.Bold))
            using (Brush backBrush =
                    new LinearGradientBrush(new Rectangle(0,0,100,14),
                       Color.FromArgb(0x80, Color.White),
                       Color.FromArgb(10, Color.Orange),90))
                 Utils.DrawStringWithBackground(pe.Graphics, str, f, brsh, backBrush, 0, 0);      
          
        }//context!=null
      }
    }

    protected override void PreFilterProperties(System.Collections.IDictionary properties)
    {
      base.PreFilterProperties(properties);
      properties.Remove("Text");
      properties.Remove("Dock");
      
    }


  } //designer class







  public class FieldContextFieldNameEditor : UITypeEditor
  {
    private IWindowsFormsEditorService editorService;


    public override UITypeEditorEditStyle GetEditStyle(ITypeDescriptorContext context)
    {
      return UITypeEditorEditStyle.DropDown;
    }

    private void ItemSelected(object sender, EventArgs e)
    {
      if (editorService != null)
      {
        editorService.CloseDropDown();
      }
    }


    public override object EditValue(ITypeDescriptorContext context,
                                      IServiceProvider provider, object value)
    {
      editorService = provider.GetService(typeof(IWindowsFormsEditorService)) as IWindowsFormsEditorService;

      if (editorService == null) return value;

      FieldContextControl control = context.Instance as FieldContextControl;


      ListBox aListBox = new ListBox();


      IEnumerable<string> fieldNames = null;
      
      
      
      if (control != null)
      {
        if (control.Record != null)
          fieldNames = control.Record.Fields.Select<Field, string>(item => item.FieldName);
        else
        if (control.AttachToRecord != null)
          fieldNames = control.AttachToRecord.Fields.Select<Field, string>(item => item.FieldName);
        else
        {
          Record rec = Utils.FindParentRecord(control);//try to get from parent
          
          if (rec==null) //try to create surrogate
          {
              string typeName = Utils.FindParentRecordSurrogateTypeName(control);
              
              if (typeName!=null)
              {
                 using (DesignerRecordBinding b = new DesignerRecordBinding(control))
                     try
                     {   
                        b.AttachDesignTimeSurrogateRecord(typeName);
                        if (b.Record == null) throw new WFormsException("Could not create surrogate record class");
                        fieldNames = b.Record.Fields.Select<Field, string>(item => item.FieldName);
                     }
                     catch (Exception error)
                     {
                       MessageBox.Show( 
                          string.Format(string.Format(StringConsts.SURROGATE_CREATE_ERROR, typeName, error.Message), 
                                        "Field Lookup Designer"
                                        ));
                     } 
              } 
          }
          else
            fieldNames = rec.Fields.Select<Field, string>(item => item.FieldName);
        }   
      }

      int itemIdx = 0;
      
      if (fieldNames != null)
      {
        foreach(string s in fieldNames)
        {
          aListBox.Items.Add(s);
          if ((control!=null)&&(control.AttachToFieldName==s))
            aListBox.SelectedIndex = itemIdx;
          
          itemIdx++;  
        }
      }
      else 
      {
        aListBox.Items.Add(string.Empty);
        aListBox.SelectedIndex = 0;
      }  

      
           
      aListBox.SelectedIndexChanged +=
           new EventHandler(ItemSelected);

      editorService.DropDownControl(aListBox);

     
      if (aListBox.SelectedItem != null) 
        return aListBox.SelectedItem.ToString();
      else
        return string.Empty;
    }

  }












}