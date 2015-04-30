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

namespace NFX.RecordModel
{

  /// <summary>
  /// Provides component model description for properties of type Field
  /// </summary>
  public class FieldPropertyDescriptor : PropertyDescriptor
  {
    public FieldPropertyDescriptor(Field field) : base(field.Description, null)  
    {                           
      m_RecordType = field.Record.GetType();
      m_FieldDataType = field.GetFieldDataType();
      m_FieldName = field.FieldName;
    }
    
    private Type m_RecordType;
    private Type m_FieldDataType;
    private string m_FieldName;
    
    private Field GetField(object component)
    {
      return (component as Record)[m_FieldName];
    }
    
    
    public override bool CanResetValue(object component)
    {
      return GetField(component).IsGUIMutable;
    }


    public override Type ComponentType
    {
      get { return  m_RecordType; }
    }

    public override object GetValue(object component)
    {          
      var fld = GetField(component);
      
      if (!fld.HasValue) return DBNull.Value;
      
      if (fld is StringField)
       return fld.ValueAsDisplayString;
      else 
       return fld.ValueAsObject;
    }

    public override bool IsReadOnly
    {
      get { return false;  }
    }

    public override Type PropertyType
    {
      get { return m_FieldDataType; }
    }

    public override void ResetValue(object component)
    {
      GetField(component).Revert();
    }

    public override void SetValue(object component, object value)
    {
      GetField(component).SetValueFromGUI(value, null);
    }

    public override bool ShouldSerializeValue(object component)
    {
      return false;
    }

    public override AttributeCollection Attributes
    {
      get                     
      {                       
        Attribute[] attr = new Attribute[base.Attributes.Count + 1];

        base.Attributes.CopyTo(attr, 0);
        attr[attr.Length - 1] = new RefreshPropertiesAttribute(RefreshProperties.All);

        return new AttributeCollection(attr);
      }
    }
  }
}
