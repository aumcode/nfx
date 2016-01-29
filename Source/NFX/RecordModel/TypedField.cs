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
using System.ComponentModel;
using System.Text;
using System.Data;
using System.Linq;

namespace NFX.RecordModel
{
  
  /// <summary>
  /// Provides base typed field model functionality. View controls are bound to record fields 
  /// </summary>
  [Serializable]
  public abstract class TypedField<TDataType> : Field
  {
    #region .ctors
      public TypedField() : base()
      {
       
       
      }

    #endregion


    #region Implicit Conversion
        
        
        /// <summary>
        /// Value property shortcut operator
        /// </summary>
        public static TDataType operator ~(TypedField<TDataType> fld) 
        {
          return fld.Value;
        }
        
        
        //public static implicit operator TDataType(TypedField<TDataType> fld) 
        //{
        //    return fld.Value;
        //}

        public static implicit operator bool(TypedField<TDataType> fld) 
        {
            return fld.ValueAsBool;
        }

        public static implicit operator DateTime(TypedField<TDataType> fld) 
        {
            return fld.ValueAsDateTime;
        }

        public static implicit operator decimal(TypedField<TDataType> fld) 
        {
            return fld.ValueAsDecimal;
        }

        public static implicit operator double(TypedField<TDataType> fld) 
        {
            return fld.ValueAsDouble;
        }

        public static implicit operator float(TypedField<TDataType> fld) 
        {
            return fld.ValueAsFloat;
        }

        public static implicit operator int(TypedField<TDataType> fld) 
        {
            return fld.ValueAsInt;
        }

        public static implicit operator long(TypedField<TDataType> fld) 
        {
            return fld.ValueAsLong;
        }

        public static implicit operator short(TypedField<TDataType> fld) 
        {
            return fld.ValueAsShort;
        }

        public static implicit operator string(TypedField<TDataType> fld) 
        {
            return fld.ValueAsString;
        }

        public static implicit operator TimeSpan(TypedField<TDataType> fld) 
        {
            return fld.ValueAsTimeSpan;
        }

    #endregion



    #region Private Fields
    
    
        
    #endregion




    #region Properties
    /// <summary>
    /// Field's data value
    /// </summary>
    [Browsable(false)]
    [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
    public TDataType Value
    {
      get
      {
       if (Calculated && !string.IsNullOrEmpty(Formula) && !Overridden)
       {
         var result = GetFormulaEvaluator().Evaluate();
         return (TDataType)System.Convert.ChangeType(result, typeof(TDataType));
       }
      
      
       if (!HasValue) 
         return default(TDataType);
       else 
         return m_Value; 
      }
      set
      {   
        EnsureCanModifyValue();
        
        if (CompareFieldValues(Value, value)!=0) 
        {
          DisableBindings();
          try
          {
            OnFieldDataChanging(new FieldDataChangeEventArgs(value, true, __sourceBinding));
            
               SetInternalValue(ref value);
            
            OnFieldDataChanged(new FieldDataChangeEventArgs(value, true, __sourceBinding)); 

          }
          finally
          {
            EnableBindings();
          } 
        }
        
      }

    }


    /// <summary>
    /// Field's default data value
    /// </summary>
    [Category(CoreConsts.DATA_CATEGORY),
     Description("Field's default data value")]
    public TDataType DefaultValue
    {
      get
      {
        return m_DefaultValue;
      }
      set
      {
        m_DefaultValue = value;
      }

    }

    
    
   
   
    #endregion

    
    #region Pub Methods
    
      /// <summary>
      /// Sets field to its default value
      /// </summary>
      public override void Default()
      {
        if (HasDefaultValue)
          Value = m_DefaultValue; 
      }

      /// <summary>
      /// Returns type of data stored by field
      /// </summary>
      public override Type GetFieldDataType()
      {
        return typeof(TDataType);
      }
       
    #endregion

    #region Events

    #endregion

    #region Protected

    protected TDataType m_Value;
    protected TDataType m_PreEdit_Value;
    protected TDataType m_DefaultValue;
    

    protected abstract int CompareFieldValues(TDataType v1, TDataType v2);




    protected override object GetValue()
    {
      return Value;
    }
    
    
    /// <summary>
    /// Performs actual write of value into field's memory.
    /// Always call default parent implementation when overriding
    /// </summary>
    protected virtual void SetInternalValue(ref TDataType value)
    {
      __internalSetValue(value);
    }

    protected override void SetValue(object value, bool fromGUI)
    {
      if (value==null) //20100921 when assigning ValueAsObject = null for val type fields has to Clear the contents
        if (typeof(TDataType).IsValueType)
        {
           Clear();
           return;
        }
      
      TDataType v;
      try
      {         
        v = (TDataType)value;
      }
      catch (System.Exception)
      {
        throw new ModelValidationException(StringConsts.VALUE_ASSIGNMENT_ERROR, this);
      }

      Value = v;
    }


    protected internal override void CreatePreEditStateCopy()
    {
      base.CreatePreEditStateCopy();
      m_PreEdit_Value = m_Value;
    }


    protected internal override void DropPreEditStateCopy()
    {
      base.DropPreEditStateCopy();
      // nothing to drop at this level
    }


    protected internal override void RevertToPreEditState()
    {
      base.RevertToPreEditState();
      __internalSetValue(m_PreEdit_Value);//DKh 20110707 was Value = m_PreEdit_Value
    }
    
    
    

    protected internal override void ApplyDefinitionAttributes(IEnumerable<object> attributes)
    {
        base.ApplyDefinitionAttributes(attributes);

        var attr = attributes.FirstOrDefault(a => a.GetType() == typeof(FieldDefAttribute)) as FieldDefAttribute;
        if (attr == null) return;

        if (attr.DefaultValue != null)
        {
            this.DefaultValue = (TDataType)attr.DefaultValue;
            this.HasDefaultValue = true;
        }
    }
    

    #endregion


    #region IComparable Members

        public override int CompareTo(Field obj)
        {
          if (obj==null) return +1;

          var fld2 = obj as TypedField<TDataType>;
      
          if (fld2==null) return +1;

          if (!HasValue)
          {
            if (!fld2.HasValue) return 0;
            else return -1;
          }
          else
          {     
            if (fld2.HasValue) 
               return CompareFieldValues(Value, fld2.Value);
            else
               return +1;   
           
          }  
        }

    #endregion



    #region Private Utils
     
     
     
     
     
     internal void __internalSetValue(TDataType value)
     {
       m_Validated = false;
    
       if (m_Calculated)
         m_Overridden = true;

       _setModified();
       
       m_Value = value;
       _setValueTypeHasValue();
       AddNotification(new FieldDataChangeNotification(this));
       
       if (Record != null)
       {
         //reset validation in all calculated fiels because their formula might depend on this value
         foreach (Field fld in Record.Fields)
           if (fld.Calculated && fld!=this) 
                     fld.ResetValidated();
         
         if (Record.FieldValidationSuspended) return;

       }  
         
       if (!DeferredValidation) Validate();
     }
   
    #endregion
    
  }
  
  
  
  
  
}
