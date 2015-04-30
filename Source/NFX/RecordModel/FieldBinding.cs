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
using System.Collections.Generic;
using System.Linq;
using System.Text;


namespace NFX.RecordModel
{
  
  /// <summary>
  /// Provides an abstract base for field view bindings. Concrete binding implementations are defined by 
  /// actual presentation classes that may be (but certainly not limited to) derivatives of standard 
  /// controls such as WinForms.Textbox, ASPNet.TextBox etc. A field binding derives from record binding and 
  /// serves two purposes: 
  ///  links a control with a record instance and field name (hence the name)
  ///  and allows control to react to changes in model
  /// </summary>
  public abstract class FieldBinding : BindingBase, IRecordFieldContext
  {
    #region .ctor

     /// <summary>
     /// Creates an instance of binding, setting an owner reference to an instance of a concrete GUI class
     /// that is allocating this binding and what this binding provides services for.
     /// </summary>
     public FieldBinding(object owner) : base (owner)
     {
      
     }

    #endregion


    #region Private Fields
     
     private Field m_Field;

    #endregion


    #region Properties

     

     /// <summary>
     /// Represents a field this binding is attached to
     /// </summary>
     public Field Field
     {
        get 
        { 
          return m_Field; 
        }
     }

    #endregion


    #region Public

       /// <summary>
       /// Sets value from view binding. This method is used to commit data from view controls into fields
       /// </summary>
       public override void SetValueFromGUI(object value)
       {
         if (m_Field!=null)
           m_Field.SetValueFromGUI(value, this);
       }
       
       /// <summary>
       /// Clears field value effectively setting HasValue to false
       /// </summary>
       public void ClearValue()
       {
         if (m_Field!=null)
           m_Field.ClearValueFromGUI(this);
       }
       
       
       /// <summary>
       /// Attaches binding to a particular field
       /// </summary>
       public void Attach(Field field)
       {
         if (field==null)
          throw new RecordModelException(string.Format(StringConsts.INVALID_ARGUMENT_ERROR, "Attach(field=null)"));
         
         if (Attached) Detach();
         m_Field = field;
         m_Field.RegisterBinding(this);
         m_Record = field.Record;//this may be null if field is not inside record context
         m_Attached = true;

         BindingStatusChanged();
       }

       /// <summary>
       /// Attaches binding to a particular field by name in a particular record
       /// </summary>
       public void Attach(Record record, string fieldName)
       {
         if (record == null)
           throw new RecordModelException(string.Format(StringConsts.INVALID_ARGUMENT_ERROR, "Attach(record=null, )"));

         if (string.IsNullOrEmpty(fieldName))
           throw new RecordModelException(string.Format(StringConsts.INVALID_ARGUMENT_ERROR, "Attach(record, fieldName=null|empty)"));  

         if (Attached) Detach();
         
         //this may throw exception if field with such a name was not found
         m_Field = record[fieldName];
         m_Field.RegisterBinding(this);
         m_Record = record;
         m_Attached = true;
         
         BindingStatusChanged();
       }
     
    #endregion


    #region Protected

     protected override void DetachBinding()
     {
       if (m_Field != null)
       {
         m_Field.UnRegisterBinding(this);
         m_Field = null;
       }
       base.DetachBinding();
     }

    #endregion


    #region Private Utils
    
    #endregion


  }


  public abstract class FieldBinding<TOwnerType> : FieldBinding
  where TOwnerType : class
  {
    public FieldBinding(TOwnerType owner)
      : base(owner)
    {
    }

    public new TOwnerType Owner
    {
      get { return base.Owner as TOwnerType; }
    }

  }
  
  
  
  
  
}
