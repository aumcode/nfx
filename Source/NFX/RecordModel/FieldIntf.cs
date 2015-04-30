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


/* NFX by ITAdapter
 * Originated: 2008.03
 * Revision: NFX 1.0  2011.02.06
 */
using System;
using System.Collections.Specialized;
using System.Collections.Generic;
using System.Text;
using System.ComponentModel;

using NFX.DataAccess;

namespace NFX.RecordModel
{
    /// <summary>
    /// Stipulates field contract with value mutability only(metadata read-only),  used for field expositions from records
    /// </summary>
    public interface IField : IComparable, IComparable<Field> 
    {
       /// <summary>
       /// Providers access to owner record that has this field declared
       /// </summary>
       Record Record { get; }
    
       /// <summary>
       /// Determines whether field is persisted in a storage (such as a database) or loaded from storage or both.
       /// Non-stored fields are most commonly used in client-side functionality like temp-calculated values etc.
       /// </summary>
       StoreFlag StoreFlag { get; }
       
       /// <summary>
       /// Returns database/storage field name
       /// </summary>
       string FieldName { get; }
       
       /// <summary>
       /// Provides meaningful field description
       /// </summary>
       string Description { get; }

       /// <summary>
       /// Provides a hint - an example of data stored by field, i.e. may be displayed as watermark
       /// </summary>
       string Hint { get; }
       
       /// <summary>
       /// Determines whether field applies (can be interacted at all).
       /// Usually this property is acted upon  by an application business rules
       /// </summary>
       bool Applicable{ get; }
      
       /// <summary>
       /// Determines whether field is shown to the user through views
       /// </summary>
       bool Visible { get; }
      
       /// <summary>
       /// Determines whether field is enabled for user actions
       /// </summary>
       bool Enabled { get; }
      
       /// <summary>
       /// Determines whether field is available for data input
       /// </summary>
       bool Readonly { get; }

       /// <summary>
       /// Determines whether field applies (can be interacted at all).
       /// Usually this property is acted upon  by an application business rules.
       /// Returns true when both field and it's parent record are Applicable.
       /// </summary>
       bool IsApplicable{ get; }
      
       /// <summary>
       /// Determines whether field is shown to the user through views.
       /// Returns true when both field and it's parent record are Visible.
       /// </summary>
       bool IsVisible { get; }
      
       /// <summary>
       /// Determines whether field is enabled for user actions.
       /// Returns true when both field and it's parent record are Enabled.
       /// </summary>
       bool IsEnabled { get; }
      
       /// <summary>
       /// Determines whether field is available for data input.
       /// Returns true when either field or it's parent record is Readonly.
       /// </summary>
       bool IsReadonly { get; }

       /// <summary>
       /// Indicates whether this field's value can be set right now per current field's state 
       /// </summary>
       bool IsMutable { get; }

       /// <summary>
       /// Indicates whether this field's value can be set from attached view 
       /// </summary>
       bool IsGUIMutable { get; }


       /// <summary>
       /// Indicates whether field was validated. This flag is reset by field changes
       /// </summary>
       bool Validated { get; }

       /// <summary>
       /// Indicates whether field is valid
       /// </summary>
       bool Valid { get; }

       
       /// <summary>
       /// Returns exception which surfaced during last field validation
       /// </summary>
       Exception ValidationException { get; }
       
       /// <summary>
       /// Returns logical order of field within its record
       /// </summary>
       int LogicalOrder { get ;}
       
       /// <summary>
       /// Returns field notes (similar to MS XL cell comments)
       /// </summary>
       string Note { get;}
       
       /// <summary>
       /// Indicates whether field is marked
       /// </summary>
       bool Marked { get; }



       /// <summary>
       /// Suggested display width for GUI
       /// </summary>
       int DisplayWidth { get; }




       /// <summary>
       /// Returns type of field data entry
       /// </summary>
       DataEntryType DataEntryType { get; }
       
       /// <summary>
       /// Indicates whether field has value
       /// </summary>
       bool HasValue { get; }

       /// <summary>
       /// Indicates whether field has default value
       /// </summary>
       bool HasDefaultValue { get; }
       
       /// <summary>
       /// Indicates whether field must have value in order to validate
       /// </summary>
       bool Required { get; }

       /// <summary>
       /// Indicates whether a field is a(part of) key field
       /// </summary>
       bool KeyField { get; }

       /// <summary>
       /// Indicates that field value has changed
       /// </summary>
       bool Modified { get; }

       /// <summary>
       /// Indicates that field value has changed through user action in GUI
       /// </summary>
       bool GUIModified { get; }

       /// <summary>
       /// Indicates that the field value is calculated
       /// </summary>
       bool Calculated { get; }

       /// <summary>
       /// Indicates whether field calculation formula can be overridden
       /// </summary>
       bool AllowCalculationOverride { get; }

       /// <summary>
       /// Returns calculation formula error
       /// </summary>
       Exception CalculationError { get; }

       /// <summary>
       /// Indicates whether calculation formula was overridden
       /// </summary>
       bool Overridden { get; }

       /// <summary>
       /// Resets calculated value override
       /// </summary>
       void ResetOverridden();

       object ValueAsObject { get; set; }
       int ValueAsInt { get; set; }
       long ValueAsLong { get; set; }
       short ValueAsShort { get; set; }
       decimal ValueAsDecimal { get;  set; }
       DateTime ValueAsDateTime  { get;  set;}
       TimeSpan ValueAsTimeSpan  { get; set;}
       float ValueAsFloat { get; set;}
       double ValueAsDouble { get; set;}
       string ValueAsString { get; set; }
       string ValueAsDisplayString { get; }
       bool ValueAsBool { get; set; }

      
       /// <summary>
       /// Reverts field value to the one it had before change was made
       /// </summary>
       void Revert();

       /// <summary>
       /// Clears field value
       /// </summary>
       void Clear();
       
       /// <summary>
       /// Resets value to default one (if field has such a value)
       /// </summary>
       void Default();

       /// <summary>
       /// Causes field validation
       /// </summary>
       void Validate();
    }


    /// <summary>
    /// Stipulates field contract with value mutability only (metadata read-only),  used for field expositions from records
    /// </summary>
    public interface IField<T> : IField
    {
       /// <summary>
       /// Typed field value
       /// </summary>
       T Value { get; set; }
    }

}