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
using System.Threading;
using System.Linq;

using NFX.DataAccess;

namespace NFX.RecordModel
{

  

  /// <summary> Represents fields that store reference types</summary>
  [Serializable]
  public abstract class TypedReferenceTypeField<TDataType> : TypedField<TDataType>
  where TDataType : class
  {
    #region .ctors
      public TypedReferenceTypeField() : base()
      {

      }
    #endregion

    #region Properties

    [Browsable(false)]
    [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
    public override bool HasValue
    {
      get { return this.m_Value != null; }
    }


    #endregion
    
    #region Pub Methods
    
    public override void Clear()
    {
      OnFieldDataChanging(new FieldDataChangeEventArgs(null, false, __sourceBinding));
      
       this.Value = null;

      OnFieldDataChanged(new FieldDataChangeEventArgs(null, false, __sourceBinding));
    }
    
    #endregion
   
  }

  /// <summary> Represents fields that store value types</summary>
  [Serializable]
  public abstract class TypedValueTypeField<TDataType> : TypedField<TDataType>
  where TDataType : struct
  {
    #region .ctors
      public TypedValueTypeField() : base()
      {


      }
        
    #endregion


    #region Private Fields
    private bool m_HasValue;
    
    private bool m_PreEdit_HasValue;
    
    #endregion

    #region Properties

    [Browsable(false)]
    [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
    public override bool HasValue
    {
      get { return m_HasValue || (Calculated && !string.IsNullOrEmpty(Formula) && !Overridden); }
    }


    #endregion

    #region Pub Methods


    public override void Default()
    {
      if (!HasDefaultValue) return;
      
      if (CompareFieldValues(Value, DefaultValue)!=0)
       base.Default();
      else //20120521 DKh Default() did not work on value types when def value was the same as default(TYPE)
      {
        OnFieldDataChanging(new FieldDataChangeEventArgs( default(TDataType), false, __sourceBinding));
        
        m_HasValue = true;

        OnFieldDataChanged(new FieldDataChangeEventArgs( default(TDataType), false, __sourceBinding));
      } 
    }
    
    public override void Clear()
    {
      OnFieldDataChanging(new FieldDataChangeEventArgs( default(TDataType), false, __sourceBinding));
      
      try
      {
          _clearLatch = true;
         
         this.Value = default(TDataType);
         m_HasValue = false;
      } 
      finally
      {
         _clearLatch = false;
      }

      OnFieldDataChanged(new FieldDataChangeEventArgs( default(TDataType), false, __sourceBinding));
    }

    #endregion
    
    #region Protected

    private bool _clearLatch;
    /// <summary>
    /// Internal framework method not intended to be called by developers
    /// </summary>
    internal override void _setValueTypeHasValue()
    {
      if (!_clearLatch)
         m_HasValue = true;
    }

    protected internal override void CreatePreEditStateCopy()
    {
      base.CreatePreEditStateCopy();
      m_PreEdit_HasValue = m_HasValue;
    }


    protected internal override void DropPreEditStateCopy()
    {
      base.DropPreEditStateCopy();
      // nothing to drop at this level
    }


    protected internal override void RevertToPreEditState()
    {
      base.RevertToPreEditState();
      m_HasValue = m_PreEdit_HasValue;
    }
    
    
    #endregion
  }


  //DKh 20130328 - What is this needed for?
  ///////////// <summary> Represents fields that store value types</summary>
  //////////[Serializable]
  //////////public class ComparableValueTypeField<TDataType> : TypedValueTypeField<TDataType>
  //////////where TDataType : struct,IComparable
  //////////{
  //////////    public ComparableValueTypeField() : base()
  //////////    {
  //////////    }
        
  //////////    protected override int CompareFieldValues(TDataType v1, TDataType v2)
  //////////    {
  //////////      return v1.CompareTo(v2);
  //////////    }
  //////////}







  /// <summary> Represents fields that store numerical value types such as integers, doubles, etc.</summary>
  [Serializable]
  public abstract class NumericalField<TDataType> : TypedValueTypeField<TDataType>
  where TDataType : struct
  {
    #region .ctors
      public NumericalField()
        : base()
      {


      }
     
    #endregion


    #region Private Fields
      private TDataType m_MinValue;

      private TDataType m_MaxValue;
    
      private bool m_MinMaxChecking; 
    #endregion

    #region Properties

    /// <summary>
    /// Minimum permitted field value
    /// </summary>
    [Category(CoreConsts.VALIDATION_CATEGORY),
    Description("Minimum permitted field value"),
    DefaultValue(0)]
    public TDataType MinValue
    {
      get { return m_MinValue; }
      set
      {
        m_MinValue = value;
        if (Constructed) Validate();
      }
    }

    /// <summary>
    /// Maximum permitted field value
    /// </summary>
    [Category(CoreConsts.VALIDATION_CATEGORY),
    Description("Maximum permitted field value"),
    DefaultValue(0)]
    public TDataType MaxValue
    {
      get { return m_MaxValue; }
      set
      {
        m_MaxValue = value;
        if (Constructed) Validate();
      }
    }

    /// <summary>
    /// Determines whether field value is validated against min/max boundaries
    /// </summary>
    [Category(CoreConsts.VALIDATION_CATEGORY),
    Description("Determines whether field value is validated against min/max boundaries"),
    DefaultValue(false)]
    public bool MinMaxChecking
    {
      get { return m_MinMaxChecking; }
      set
      {
        m_MinMaxChecking = value;
        if (Constructed && value) 
          Validate();
      }
    }

    #endregion

    #region Pub Methods

    #endregion

    #region Protected
        protected internal override void ApplyDefinitionAttributes(IEnumerable<object> attributes)
        {
            base.ApplyDefinitionAttributes(attributes);

            var attr = attributes.FirstOrDefault(a => a.GetType() == typeof(FieldDefAttribute)) as FieldDefAttribute;
            if (attr == null) return;

            this.MinMaxChecking = attr.MinMaxChecking;
        }
    #endregion
  }








  /// <summary> Represents fields that store objects </summary>
  [Serializable]
  public class ObjectField : TypedReferenceTypeField<object>, IField<object>
  {
    #region .ctors
    public ObjectField()
      : base()
    {


    }
     
    #endregion

    protected override int CompareFieldValues(object v1, object v2)
    { //v1 and v2 are never null here

      if (object.ReferenceEquals(v1, v2)) return 0;

      IComparable cmp = v1 as IComparable;
      if (cmp != null)
        return cmp.CompareTo(v2);
      else
        return -1; // no particular comparison possible 
    }
  }


  /// <summary> Represents fields that store generic objects </summary>
  [Serializable]     //20110111 DKh
  public class ObjectField<T> : TypedReferenceTypeField<T>, IField<T> where T : class
  {
    #region .ctors
    public ObjectField()
      : base()
    {


    }
     
    #endregion

    protected override int CompareFieldValues(T v1, T v2)
    { //v1 and v2 are never null here

      if (object.ReferenceEquals(v1, v2)) return 0;

      IComparable cmp = v1 as IComparable;
      if (cmp != null)
        return cmp.CompareTo(v2);
      else
        return -1; // no particular comparison possible 
    }
  }



  /// <summary> Represents fields that store string </summary>
  [Serializable]
  public class StringField : TypedReferenceTypeField<string>, IField<string>
  {
    #region .ctors
    public StringField()
      : base()
    {


    }

   
    #endregion


    #region Private Fields

    private int m_Size = 0;
    private DataEntryFormat m_DataEntryFormat;
    private CharCase m_CharCase;
    
    private bool m_Password;
    
    //20120522 DKh
    private string m_FormatRegExp;
    private string m_FormatRegExpDescription;

    #endregion


    #region Properties
    
    /// <summary>
    /// Imposes a limit on string field length in characters, 0 = Unlimited
    /// </summary>
    [Category(CoreConsts.VALIDATION_CATEGORY),
    Description("Imposes a limit on string field length in charecters, 0 = Unlimited"),
    DefaultValue(0)]
    public int Size
    {
      get { return m_Size; }
      set
      {
        m_Size = (value < 0) ? 0 : value;
        m_Validated = false;

        if (Constructed)
        {
          DisableBindings();
           AddNotification(new ValidationNotification(this));
          EnableBindings();
        }  
      }
    }
    
    
    /// <summary>
    /// Imposes a regular expression validation on this field
    /// </summary>
    [Category(CoreConsts.VALIDATION_CATEGORY),
    Description("Imposes a regular expression validation on this field")
    ]
    public string FormatRegExp//20120522 DKh
    {
      get { return m_FormatRegExp ?? string.Empty; }
      set
      {
        if (m_FormatRegExp==value) return;
        
        m_FormatRegExp = value;
        m_Validated = false;

        if (Constructed)
        {
          DisableBindings();
            AddNotification(new ValidationNotification(this));
          EnableBindings();
        }  
      }
    }
    
    /// <summary>
    /// Provides description for format regular expression
    /// </summary>
    [Category(CoreConsts.VALIDATION_CATEGORY),
    Description("Provides description for format regular expression")
    ]
    public string FormatRegExpDescription//20120522 DKh
    {
      get { return m_FormatRegExpDescription ?? string.Empty; }
      set
      {
        m_FormatRegExpDescription = value;
      }
    }
    
    

    /// <summary>
    /// Determines formatting/masking applied during data entry
    /// </summary>
    [Category(CoreConsts.VALIDATION_CATEGORY),
     Description("Determines formatting/masking applied during data entry"),
     DefaultValue(DataEntryFormat.AsIs)]
    public DataEntryFormat DataEntryFormat
    {
      get
      {
        return m_DataEntryFormat;
      }
      set
      {
        m_DataEntryFormat = value;
        
        if (Constructed)
        {
          DisableBindings();
           AddNotification(new DataEntryTypeChangeNotification(this));
          EnableBindings();
        }  
      }
    }

    /// <summary>
    /// Determines data entry character casing for string fields
    /// </summary>
    [Category(CoreConsts.VALIDATION_CATEGORY),
     Description("Determines data entry character casing for string fields"),
     DefaultValue(CharCase.AsIs)]
    public CharCase CharCase
    {
      get
      {
        return m_CharCase;
      }
      set
      {
        m_CharCase = value;
        m_Validated = false;
       
        if (Constructed)
        {
          DisableBindings();
            AddNotification(new DataEntryTypeChangeNotification(this));
          EnableBindings(); 
        }  
      }
    }
    

    /// <summary>
    /// Determines whether field stores a password value
    /// </summary>
    [Category(CoreConsts.PRESENTATION_CATEGORY),
     Description("Determines whether field stores a password value"),
     DefaultValue(false)]
    public bool Password
    {
      get
      {
        return m_Password;
      }
      set
      {
        m_Password = value;

        if (Constructed)
        {
          DisableBindings();
           AddNotification(new DataEntryTypeChangeNotification(this));
          EnableBindings();
        }  
      }
    }



    #endregion


    #region Protected


    protected override void SetInternalValue(ref string value)
    {
      if (value != null)
      {
        switch (m_CharCase)
        {
          case CharCase.Lower:
            {
              value = value.ToLower();
              break;
            }
          case CharCase.Upper:
            {
              value = value.ToUpper();
              break;
            }
        }
      }  
     
      base.SetInternalValue(ref value);
    }
    

    protected override void SetValue(object value, bool fromGUI)
    {
      if (value != null)      
         value = value.ToString();
      
      base.SetValue(value, fromGUI);
    }


    protected override int CompareFieldValues(string v1, string v2)
    {
      return string.Compare(v1, v2);
    }


    protected override void PerformValidation(ref bool couldNotValidate)
    {
      base.PerformValidation(ref couldNotValidate);
      
      if (m_Size>0)
      {
        if (m_Value!=null)
         if (m_Value.Length>m_Size)
          throw new SizeValidationException(string.Format(StringConsts.SIZE_FIELD_ERROR, Description, m_Size), this);
      }
      
      //20120522 DKh
      if (m_Value!=null && !string.IsNullOrEmpty(m_FormatRegExp))
      {
        if (!System.Text.RegularExpressions.Regex.IsMatch(m_Value, m_FormatRegExp))
         throw new RegExpValidationException(
                      string.Format(StringConsts.REGEXP_FIELD_ERROR,
                                    Description,
                                    FormatRegExpDescription.Length==0?m_FormatRegExp:m_FormatRegExpDescription
                                    ), this);
      }
      
    }


    protected override object PreProcessValueFromGUI(object value)
    {
      if ((value == null) || (!(value is string)))
        return base.PreProcessValueFromGUI(value);
      else
      {
        string str = value as string;
        try
        {
          if (str!=null)//20120519
           if (str.Trim()==string.Empty) return null; //20120519       
         
          return GUIDataParser.NormalizeEnteredString(
                                              str, 
                                              m_DataEntryFormat, 
                                              Thread.CurrentThread.CurrentUICulture
                                              ); 
        }
        catch
        {
          return str;
        } 
      }//else 
    }

    protected internal override void ApplyDefinitionAttributes(IEnumerable<object> attributes)
    {
        base.ApplyDefinitionAttributes(attributes);

        var attr = attributes.FirstOrDefault(a => a.GetType() == typeof(FieldDefAttribute)) as FieldDefAttribute;
        if (attr == null) return;

        this.Size = attr.Size;
    }


    
    #endregion
    
  }





  /// <summary> Represents fields that store dataset </summary>
  [Serializable]
  public class DataSetField : TypedReferenceTypeField<DataSet>, IField<DataSet>
  {
    #region .ctors
    public DataSetField()
      : base()
    {


    }
        
    #endregion

    protected override int CompareFieldValues(DataSet v1, DataSet v2)
    { //v1 and v2 are never null here

      if (object.ReferenceEquals(v1, v2))
        return 0;
      else
        return -1; // no particular comparison possible 
    }
  }


  /// <summary> Represents fields that store datatable </summary>
  [Serializable]
  public class DataTableField : TypedReferenceTypeField<DataTable>, IField<DataTable>
  {
    #region .ctors
    public DataTableField()
      : base()
    {


    }

   
    #endregion

    protected override int CompareFieldValues(DataTable v1, DataTable v2)
    { //v1 and v2 are never null here

      if (object.ReferenceEquals(v1, v2))
        return 0;
      else
        return -1; // no particular comparison possible 
    }
  }




  /// <summary> Represents fields that store integers </summary>
  [Serializable]
  public class IntField : NumericalField<int>, IField<int>
  {
    #region .ctors
    public IntField()
      : base()
    {


    }

    
    #endregion

    protected override int CompareFieldValues(int v1, int v2)
    {
      if (v1 == v2) return 0;
      if (v1 < v2) return -1;
      return +1;
    }

    protected override void PerformValidation(ref bool couldNotValidate)
    {
      base.PerformValidation(ref couldNotValidate);

      if (HasValue && MinMaxChecking)
      {
        if ((m_Value < MinValue)||(m_Value>MaxValue))
            throw new MinMaxValidationException(string.Format(StringConsts.MINMAX_FIELD_ERROR, Description, MinValue, MaxValue), this);
      }
    }

    protected override void SetValue(object value, bool fromGUI)
    {
      if (value != null)
        if (value is string)
          value = int.Parse(value as string);

      base.SetValue(value, fromGUI);
    }



    protected internal override void ApplyDefinitionAttributes(IEnumerable<object> attributes)
    {
            base.ApplyDefinitionAttributes(attributes);

            var attr = attributes.FirstOrDefault(a => a.GetType() == typeof(FieldDefAttribute)) as FieldDefAttribute;
            if (attr == null) return;

            if (attr.MinValue!=null)
              this.MinValue = (int)attr.MinValue;

            if (attr.MaxValue!=null)
             this.MaxValue = (int)attr.MaxValue;
   }

  }

  /// <summary> Represents fields that store long integers </summary>
  [Serializable]
  public class LongField : NumericalField<long>, IField<long>
  {
    #region .ctors
    public LongField()
      : base()
    {


    }

    
    #endregion

    protected override int CompareFieldValues(long v1, long v2)
    {
      if (v1 == v2) return 0;
      if (v1 < v2) return -1;
      return +1;
    }
    protected override void PerformValidation(ref bool couldNotValidate)
    {
      base.PerformValidation(ref couldNotValidate);

      if (HasValue && MinMaxChecking)
      {
        if ((m_Value < MinValue) || (m_Value > MaxValue))
          throw new MinMaxValidationException(string.Format(StringConsts.MINMAX_FIELD_ERROR, Description, MinValue, MaxValue), this);
      }
    }

    protected override void SetValue(object value, bool fromGUI)
    {
      if (value != null)
        if (value is string)
          value = long.Parse(value as string);

      base.SetValue(value, fromGUI);
    }

    protected internal override void ApplyDefinitionAttributes(IEnumerable<object> attributes)
    {
        base.ApplyDefinitionAttributes(attributes);

        var attr = attributes.FirstOrDefault(a => a.GetType() == typeof(FieldDefAttribute)) as FieldDefAttribute;
        if (attr == null) return;

        if (attr.MinValue != null)
            this.MinValue = (long)attr.MinValue;

        if (attr.MaxValue != null)
            this.MaxValue = (long)attr.MaxValue;
    }
    
  }


  /// <summary> Represents fields that store short integers </summary>
  [Serializable]
  public class ShortField : NumericalField<short>, IField<short>
  {
    #region .ctors
    public ShortField()
      : base()
    {


    }

    
    #endregion

    protected override int CompareFieldValues(short v1, short v2)
    {
      if (v1 == v2) return 0;
      if (v1 < v2) return -1;
      return +1;
    }
    
    protected override void PerformValidation(ref bool couldNotValidate)
    {
      base.PerformValidation(ref couldNotValidate);

      if (HasValue && MinMaxChecking)
      {
        if ((m_Value < MinValue) || (m_Value > MaxValue))
          throw new MinMaxValidationException(string.Format(StringConsts.MINMAX_FIELD_ERROR, Description, MinValue, MaxValue), this);
      }
    }

    protected override void SetValue(object value, bool fromGUI)
    {
      if (value != null)
        if (value is string)
          value = short.Parse(value as string);

      base.SetValue(value, fromGUI);
    }

    protected internal override void ApplyDefinitionAttributes(IEnumerable<object> attributes)
    {
        base.ApplyDefinitionAttributes(attributes);

        var attr = attributes.FirstOrDefault(a => a.GetType() == typeof(FieldDefAttribute)) as FieldDefAttribute;
        if (attr == null) return;

        if (attr.MinValue != null)
            this.MinValue = (short)attr.MinValue;

        if (attr.MaxValue != null)
            this.MaxValue = (short)attr.MaxValue;
    }
    
    
  }


  /// <summary> Represents fields that store decimals </summary>
  [Serializable]
  public class DecimalField : NumericalField<decimal>, IField<decimal>
  {
    #region .ctors
    public DecimalField()
      : base()
    {


    }

    
    #endregion

    protected override int CompareFieldValues(decimal v1, decimal v2)
    {
      if (v1 == v2) return 0;
      if (v1 < v2) return -1;
      return +1;
    }

    protected override void PerformValidation(ref bool couldNotValidate)
    {
      base.PerformValidation(ref couldNotValidate);

      if (HasValue && MinMaxChecking)
      {
        if ((m_Value < MinValue) || (m_Value > MaxValue))
          throw new MinMaxValidationException(string.Format(StringConsts.MINMAX_FIELD_ERROR, Description, MinValue, MaxValue), this);
      }
    }

    protected override void SetValue(object value, bool fromGUI)
    {
      if (value != null)
        if (value is string)
          value = decimal.Parse(value as string);

      base.SetValue(value, fromGUI);
    }

    protected override object PreProcessValueFromGUI(object value)
    {
      if ((value==null)||(!(value is string)))
         return base.PreProcessValueFromGUI(value);
      else
      {
        string str = value as string;
        str = str.Replace("$", string.Empty); 
        return str;
      }  
    }

    protected internal override void ApplyDefinitionAttributes(IEnumerable<object> attributes)
    {
        base.ApplyDefinitionAttributes(attributes);

        var attr = attributes.FirstOrDefault(a => a.GetType() == typeof(FieldDefAttribute)) as FieldDefAttribute;
        if (attr == null) return;

        if (attr.MinValue != null)
            this.MinValue = Convert.ToDecimal(attr.MinValue);
                                                   
        if (attr.MaxValue != null)
            this.MaxValue = Convert.ToDecimal(attr.MaxValue);
    }
    
  }


  /// <summary> Represents fields that store date and time </summary>
  [Serializable]
  public class DateTimeField : TypedValueTypeField<DateTime>, IField<DateTime>
  {
    #region .ctors
    public DateTimeField()
      : base()
    {


    }

    
    #endregion

    protected override int CompareFieldValues(DateTime v1, DateTime v2)
    {
      if (v1 == v2) return 0;
      if (v1 < v2) return -1;
      return +1;
    }

    protected override void SetValue(object value, bool fromGUI)
    {
      if (value != null)
        if (value is string)
           value = DateTime.Parse(value as string);

      base.SetValue(value, fromGUI);
    }
    
  }


  /// <summary> Represents fields that store time span </summary>
  [Serializable]
  public class TimeSpanField : TypedValueTypeField<TimeSpan>, IField<TimeSpan>
  {
    #region .ctors
    public TimeSpanField()
      : base()
    {


    }

    
    #endregion

    protected override int CompareFieldValues(TimeSpan v1, TimeSpan v2)
    {
      if (v1 == v2) return 0;
      if (v1 < v2) return -1;
      return +1;
    }

    protected override void SetValue(object value, bool fromGUI)
    {
      if (value != null)
        if (value is string)
          value = TimeSpan.Parse(value as string);

      base.SetValue(value, fromGUI);
    }
    
  }


  /// <summary> Represents fields that store double </summary>
  [Serializable]
  public class DoubleField : NumericalField<double>, IField<double>
  {
    #region .ctors
    public DoubleField()
      : base()
    {


    }

    
    #endregion

    protected override int CompareFieldValues(double v1, double v2)
    {
      if (v1 == v2) return 0;
      if (v1 < v2) return -1;
      return +1;
    }

    protected override void PerformValidation(ref bool couldNotValidate)
    {
      base.PerformValidation(ref couldNotValidate);

      if (HasValue && MinMaxChecking)
      {
        if ((m_Value < MinValue) || (m_Value > MaxValue))
          throw new MinMaxValidationException(string.Format(StringConsts.MINMAX_FIELD_ERROR, Description, MinValue, MaxValue), this);
      }
    }

    protected override void SetValue(object value, bool fromGUI)
    {
      if (value != null)
        if (value is string)
          value = double.Parse(value as string);

      base.SetValue(value, fromGUI);
    }

    protected internal override void ApplyDefinitionAttributes(IEnumerable<object> attributes)
    {
        base.ApplyDefinitionAttributes(attributes);

        var attr = attributes.FirstOrDefault(a => a.GetType() == typeof(FieldDefAttribute)) as FieldDefAttribute;
        if (attr == null) return;

        if (attr.MinValue != null)
            this.MinValue = (double)attr.MinValue;

        if (attr.MaxValue != null)
            this.MaxValue = (double)attr.MaxValue;
    }
    
  }

  /// <summary> Represents fields that store float </summary>
  [Serializable]
  public class FloatField : NumericalField<float>, IField<float>
  {
    #region .ctors
    public FloatField()
      : base()
    {


    }

    
    #endregion

    protected override int CompareFieldValues(float v1, float v2)
    {
      if (v1 == v2) return 0;
      if (v1 < v2) return -1;
      return +1;
    }

    protected override void PerformValidation(ref bool couldNotValidate)
    {
      base.PerformValidation(ref couldNotValidate);

      if (HasValue && MinMaxChecking)
      {
        if ((m_Value < MinValue) || (m_Value > MaxValue))
          throw new MinMaxValidationException(string.Format(StringConsts.MINMAX_FIELD_ERROR, Description, MinValue, MaxValue), this);
      }
    }

    protected override void SetValue(object value, bool fromGUI)
    {
      if (value != null)
        if (value is string)
          value = float.Parse(value as string);

      base.SetValue(value, fromGUI);
    }


    protected internal override void ApplyDefinitionAttributes(IEnumerable<object> attributes)
    {
        base.ApplyDefinitionAttributes(attributes);

        var attr = attributes.FirstOrDefault(a => a.GetType() == typeof(FieldDefAttribute)) as FieldDefAttribute;
        if (attr == null) return;

        if (attr.MinValue != null)
            this.MinValue = (float)attr.MinValue;

        if (attr.MaxValue != null)
            this.MaxValue = (float)attr.MaxValue;
    }
    
  }



  /// <summary> Represents fields that store boolean </summary>
  [Serializable]
  public class BoolField : TypedValueTypeField<bool>, IField<bool>
  {
    #region .ctors
    public BoolField()
      : base()
    {


    }

    
    #endregion

    protected override int CompareFieldValues(bool v1, bool v2)
    {
      if (v1 == v2) return 0;
      if ((v1 == false) && (v2 == true)) return -1;
      return +1;
    }

    protected override void SetValue(object value, bool fromGUI)
    {
      if (value != null)
      {
        if (value is string)
        {
          var svalue = value as string;
        
          if (svalue=="1") value = true;
          else
            if (svalue == "0") value = false;
            else
             value = bool.Parse(svalue);
        }
        else
        if (value is short)
        {
          value = (short)value != 0;
        }
        else
        if (value is int)
        {
          value = (int)value != 0;
        }
        else
        if (value is long)
        {
          value = (long)value != 0;
        }
      }
      base.SetValue(value, fromGUI);
    }
    
  }


  /// <summary> Represents fields that store Guid structs </summary>
  [Serializable]
  public class GuidField : TypedValueTypeField<Guid>, IField<Guid>
  {
    #region .ctors
        public GuidField(): base()
        {


        }
    #endregion

    protected override int CompareFieldValues(Guid v1, Guid v2)
    {
      return v1.CompareTo(v2);
    }

    protected override void SetValue(object value, bool fromGUI)
    {
      if (value != null)
        if (value is string)
          value = Guid.Parse(value as string);

      base.SetValue(value, fromGUI);
    }
    
  }
 

}