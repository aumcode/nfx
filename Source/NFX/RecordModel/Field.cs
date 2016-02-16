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
using System.Linq;
using System.ComponentModel;

using NFX.Parsing;
using System.Reflection;

namespace NFX.RecordModel
{
   
  /// <summary>
  /// Provides base field-management model functionality. View controls are bound to record fields 
  /// </summary>
  [DefaultProperty("FieldName")]
  [Serializable]
  public abstract class Field : ModelBase, IComparable, IComparable<Field>
  {
    
    #region STATIC

      public static readonly Dictionary<Type, Func<Field>> TYPE_FIELD_MAP = new Dictionary<Type,Func<Field>>
      {
        {typeof(byte), () => new IntField(){ MinMaxChecking = true, MinValue = byte.MinValue, MaxValue = byte.MaxValue}},
        {typeof(sbyte), () => new IntField(){ MinMaxChecking = true, MinValue = sbyte.MinValue, MaxValue = sbyte.MaxValue}},
        {typeof(short), () => new ShortField()},
        {typeof(ushort), () => new IntField(){ MinMaxChecking = true, MinValue = ushort.MinValue, MaxValue = ushort.MaxValue}},
        {typeof(int), () => new IntField()},
        {typeof(uint), () => new LongField(){ MinMaxChecking = true, MinValue = uint.MinValue, MaxValue = uint.MaxValue}},
        {typeof(long), () => new LongField()},
        {typeof(ulong), () => new LongField()},
        {typeof(char), () => new StringField()},
        {typeof(string), () => new StringField()},
        {typeof(bool), () => new BoolField()},
        {typeof(decimal), () => new DecimalField()},
        {typeof(float), () => new DoubleField()},
        {typeof(double), () => new DoubleField()},
        {typeof(DateTime), () => new DateTimeField()},
        {typeof(TimeSpan), () => new TimeSpanField()},
        {typeof(Guid), () => new GuidField()},
        {typeof(byte[]), () => new ObjectField<byte[]>()},
        {typeof(Record), () => new ObjectField<Record>()}
      };


      public static Field MakeFieldOfType(Type type)
      {
        //Take care of Nullable<T>
        if (type.IsGenericType && type.GetGenericTypeDefinition() == typeof(Nullable<>))
         type = type.GetGenericArguments()[0];

        Func<Field> f;
        if (!TYPE_FIELD_MAP.TryGetValue(type, out f))
         throw new RecordModelException(StringConsts.FIELD_TYPE_MAP_ERROR.Args(type.FullName));

        return f();
      }

    
    #endregion
    
    #region .ctors
      public Field() : base()
      {    
       
      }
    #endregion
     
  
    #region Private fields
   
     private string m_FieldName;
     private string m_Description;
     private string m_Hint;
     private int m_DisplayWidth;
     private bool m_KeepInErroredField;
     private string m_Note;
     

     private bool m_Required;
     private bool m_KeyField;

     protected bool m_Calculated;
     private bool m_AllowCalculationOverride;
     private string m_Formula;

     [NonSerialized] private Evaluator m_formulaEvaluator;

     private bool m_Modified;   protected void _setModified() { m_Modified = true;} 
     private bool m_GUIModified;
     protected bool m_Overridden;
     private bool m_PreEdit_Overridden;
     
     private int m_LogicalOrder;
     
     private Exception m_CalculationError;
     private Exception m_PreEdit_CalculationError;
     
     private bool m_Marked;
     
     private bool m_HasDefaultValue;
     
     private DataEntryType m_DataEntryType;
     private LookupType m_LookupType;
     private LookupDictionary m_LookupDictionary;
     private string m_LookupCommand;
     
     private string m_DisplayFormat; 
     private DisplayTextHAlignment m_DisplayTextHAlignment;
     
    #endregion


    #region Properties


     public override bool SupportsNotificationBinding
     {
         get
         {
             var rec = Record;
             if (rec==null)
               return base.SupportsNotificationBinding;
             else
               return rec.SupportsNotificationBinding; 
         }
         set
         {
             base.SupportsNotificationBinding = value;
         }
     }



     /// <summary>
     /// Providers access to owner record
     /// </summary>
     [Browsable(false)]
     [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
     public Record Record
     {
       get
       {
         return Owner as Record;
       }
     }


     /// <summary>
     /// Represents field state. When fields are owned by records, field's states are determined by owner record
     /// </summary>
     [Browsable(false)]
     [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
     public override DataState State
     {
       get
       {
         if (Record!=null)
          return Record.State;
         else 
          return base.State;
       }

     }

     /// <summary>
     /// Returns ongoing storage operation. When fields are owned by records, field's states are determined by owner record
     /// </summary>
     [Browsable(false)]
     [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
     public override StorageOperation StorageOperation
     {
       get
       {
         if (Record != null)
           return Record.StorageOperation;
         else
           return base.StorageOperation;
       }

     }

     /// <summary>
     /// Returns a type of the last change posted to model
     /// </summary>
     [Browsable(false)]
     [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
     public override ChangeType LastPostedChange
     {
       get
       {
         if (Record != null)
           return Record.LastPostedChange;
         else
           return base.LastPostedChange;
       }

     }


    
          
     /// <summary>
     /// Unique data field name, most often used for underlying database column name identification
     /// Not to be confused with design-time component "Name" property
     /// </summary>
     [Category(CoreConsts.DATA_CATEGORY),
     Description("Unique data field name, most often used for underlying database column name identification"),
     RefreshProperties(RefreshProperties.All)]
     public string FieldName
     {
      get { return m_FieldName ?? string.Empty; }
      set 
      {
       if ((value!=null) && (Record!=null))
       {
         if (!Record.NoBuildCrosschecks)
          if (Record.fieldNameAlreadyTaken(this, value))
           throw new RecordModelException(string.Format(StringConsts.DUPLICATE_FIELD_NAME_ERROR, value));
       }
       
       
       string oldName = m_FieldName;
       
       m_FieldName = value ?? string.Empty;
       
       //Design-time help for developers
       if (string.IsNullOrEmpty(m_Description)||(m_Description==oldName))
          m_Description = m_FieldName.ParseFieldNameToDescription(true);
                          
      }
     }

     /// <summary>
     /// Provides meaningful field description
     /// </summary>
     [Category(CoreConsts.PRESENTATION_CATEGORY),
     Description("Provides meaningful field description"),
     DefaultValue("")]
     public string Description
     {
       get { return m_Description ?? string.Empty; }
       set
       {
         if (!Constructed)
         {
           m_Description = value;
           return;
         }
         
         if (m_Description!=value)
         {
           DisableBindings();
              m_Description = value;
              AddNotification(new PresentationChangeNotification(this));
           EnableBindings();
         }
       }
     }


     /// <summary>
     /// Provides a hint - an example of data stored by field, i.e. may be displayed as watermark
     /// </summary>
     [Category(CoreConsts.PRESENTATION_CATEGORY),
     Description("Provides a hint - an example of data stored by field, i.e. may be displayed as watermark"),
     DefaultValue("")]
     public string Hint
     {
       get { return m_Hint ?? string.Empty; }
       set
       {
         if (!Constructed)
         {
           m_Hint = value;
           return;
         }
         
         if (m_Hint!=value)
         {
           DisableBindings();
              m_Hint = value;
              AddNotification(new PresentationChangeNotification(this));
           EnableBindings();
         }
       }
     }


     /// <summary>
     /// Provides suggested default width for GUI
     /// </summary>
     [Category(CoreConsts.PRESENTATION_CATEGORY),
     Description("Provides suggested default width for GUI"),
     DefaultValue(0)]
     public int DisplayWidth
     {
       get { return m_DisplayWidth; }
       set
       {
         if (!Constructed)
         {
           m_DisplayWidth = value;
           return;
         }
         
         
         if (m_DisplayWidth!=value)
         {
            DisableBindings();
               m_DisplayWidth = value;
               AddNotification(new PresentationChangeNotification(this));
            EnableBindings(); 
         }
       }
     }

     /// <summary>
     /// Provides suggested horizontal text alignment for attached view
     /// </summary>
     [Category(CoreConsts.PRESENTATION_CATEGORY),
     Description("PProvides suggested horizontal text allignment for attached view"),
     DefaultValue(DisplayTextHAlignment.Left)]
     public DisplayTextHAlignment DisplayTextHAlignment
     {
       get { return m_DisplayTextHAlignment; }
       set
       {
         if (!Constructed)
         {
           m_DisplayTextHAlignment = value;
           return;
         }
         
         if (m_DisplayTextHAlignment!=value)
         {
           DisableBindings();
             m_DisplayTextHAlignment = value;
             AddNotification(new PresentationChangeNotification(this));
           EnableBindings();
         }
       }
     }


     /// <summary>
     /// Provides display format string
     /// </summary>
     [Category(CoreConsts.PRESENTATION_CATEGORY),
     Description("Provides display format string"),
     DefaultValue("")]
     public string DisplayFormat
     {
       get { return m_DisplayFormat ?? string.Empty; }
       set
       {
         if (value!=null) value = value.Trim();
         
         if (!Constructed)
         {
           m_DisplayFormat = value;
           return;
         }
         
         if (m_DisplayFormat!=value)
         {
           DisableBindings();
               m_DisplayFormat = value;
               AddNotification(new PresentationChangeNotification(this));
           EnableBindings();    
         }
       }
     }

     /// <summary>
     /// Indicates whether a user should be kept(not allowed to tab-out) in field-attached control when data is incorrect
     /// </summary>
     [Category(CoreConsts.PRESENTATION_CATEGORY),
     Description("Indicates whether a user should be kept(not allowed to tab-out) in field-attached control when data is incorrect"),
     DefaultValue(false)]
     public bool KeepInErroredField
     {
       get { return m_KeepInErroredField; }
       set
       {
         if (!Constructed)
         {
           m_KeepInErroredField = value;
           return;
         }
         
         if (m_KeepInErroredField!=value)
         {
            DisableBindings();
               m_KeepInErroredField = value;
               AddNotification(new PresentationChangeNotification(this));
            EnableBindings(); 
         }
       }
     }

     /// <summary>
     /// Defines filed's logical order which may coinside with tab order, this is a hint for automated GUI builders
     /// </summary>
     [Category(CoreConsts.PRESENTATION_CATEGORY),
     Description("Defines filed's logical order which may coinside with tab order, this is a hint for automated GUI builders"),
     DefaultValue(0)]
     public int LogicalOrder
     {
       get { return m_LogicalOrder; }
       set
       {
         if (!Constructed)
         {
           m_LogicalOrder = value;
           return;
         }
         
         if (m_LogicalOrder!=value)
         {
          DisableBindings();
             m_LogicalOrder = value;
             AddNotification(new PresentationChangeNotification(this));
          EnableBindings();
         }
       }
     }
  
     /// <summary>
     /// Attaches a note text to the field (i.e. similar to XL cell comments)
     /// </summary>
     [Description("Attaches a note text to the field (i.e. similar to XL cell comments)")]
     [DefaultValue("")]
     public string Note
     {
       get { return m_Note ?? string.Empty; }
       set
       {
         if (!Constructed)
         {
           m_Note = value;
           return;
         }
         
         if (m_Note!=value)
         {
           DisableBindings();
             m_Note = value;
             AddNotification(new PresentationChangeNotification(this));
           EnableBindings();
         }
       }
     }

     /// <summary>
     /// Indicates whether a field was marked. This property is usually used to implement user-selectable fields 
     /// </summary>
     [Category(CoreConsts.PRESENTATION_CATEGORY),
     Description("Indicates whether a field was marked. This property is usually used to implement user-selectable fields"),
     DefaultValue(false)]
     public bool Marked
     {
       get { return m_Marked; }
       set
       {
         if (!Constructed)
         {
           m_Marked = value;
           return;
         }
         if (m_Marked!=value)
         {
           DisableBindings();
             m_Marked = value;
             AddNotification(new PresentationChangeNotification(this));
           EnableBindings();
         }
       }
     }


     /// <summary>
     /// Provides a list of lookup items. Each strings line portion before comma(if any) is treated as an item key.
     /// Example:  "LPN, LPN Nurse" will be parsed as Key="LPN", Description="LPN Nurse"
     /// </summary>
     [Category(CoreConsts.DATA_CATEGORY),
      Description("Provides a list of lookup items. Each strings line portion before comma(if any) is treated as an item key"),
      DesignerSerializationVisibility(DesignerSerializationVisibility.Content),
      Editor("System.Windows.Forms.Design.StringCollectionEditor, System.Design",
         "System.Drawing.Design.UITypeEditor, System.Drawing")]
     public LookupDictionary LookupDictionary
     {
       get
       {
         //20120328 DKh added lazy creation
         if (m_LookupDictionary==null)
          m_LookupDictionary = new LookupDictionary(this);

         return m_LookupDictionary;
       }
     }

     /// <summary>
     /// Determines the ways a user can perform data entry through model-attached view
     /// </summary>
     [Category(CoreConsts.DATA_CATEGORY),
      Description("Determines the ways a user can perform data entry through model-attached view"),
      DefaultValue(DataEntryType.DirectEntry)]
     public DataEntryType DataEntryType
     {
       get
       {
         return m_DataEntryType;
       }
       set
       {
         if (!Constructed)
         {
           m_DataEntryType = value;
           return;
         }
         if (m_DataEntryType!=value)
         {
           DisableBindings();
             m_DataEntryType = value;
             AddNotification(new DataEntryTypeChangeNotification(this));
           EnableBindings();
         }
       }
     }


     /// <summary>
     /// Specifies lookup command text
     /// </summary>
     [Category(CoreConsts.DATA_CATEGORY),
      Description("Specifies lookup command text"),
      DefaultValue("")]
     public string LookupCommand
     {
       get
       {
         return m_LookupCommand ?? string.Empty;
       }
       set
       {
         if (!Constructed)
         {
           m_LookupCommand = value;
           return;
         }
         if (m_LookupCommand!=value)
         {
           DisableBindings();
               m_LookupCommand = value;
               AddNotification(new DataEntryTypeChangeNotification(this));
           EnableBindings();
         }
       }
     }



     /// <summary>
     ///  Determines a type of value lookup used
     /// </summary>
     [Category(CoreConsts.DATA_CATEGORY),
      Description("Determines a type of value lookup used"),
      DefaultValue(LookupType.Dictionary)]
     public LookupType LookupType
     {
       get
       {
         return m_LookupType;
       }
       set
       {
         if (!Constructed)
         {
           m_LookupType = value;
           return;
         }
         if (m_LookupType!=value)
         {
          DisableBindings();
             m_LookupType = value;
             AddNotification(new DataEntryTypeChangeNotification(this));
          EnableBindings();
         }
       }
     }
     
     
     

     /// <summary>
     /// Indicates whether field contains any value 
     /// </summary>
     [Browsable(false)]
     [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
     public abstract bool HasValue
     {
       get;
     }

     /// <summary>
     /// Indicates whether a field has a default value
     /// </summary>
     [Category(CoreConsts.DATA_CATEGORY),
      Description("Indicates whether a field has a default value"),
      DefaultValue(false)]
     public bool HasDefaultValue
     {
       get
       {
         return m_HasDefaultValue;
       }
       set
       {
         m_HasDefaultValue = value;
       }

     }
     
     ///// <summary>
     ///// Indicates whether Lookup() method was called and the field is awaiting lookup result (that might never arrive)
     ///// </summary>
     //[Browsable(false)]
     //[DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
     //public bool LookupIsPending
     //{
     //  get { return m_LookupIsPending; }
     //}

     ///// <summary>
     ///// Returns an object reference passed to currently-pending result after Lookup() call
     ///// </summary>
     //[Browsable(false)]
     //[DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
     //public object LookupArgument
     //{
     //  get { return m_LookupArgument;}
     //}
     



     /// <summary>
     /// Indicates whether field must have a value 
     /// </summary>
     [Category(CoreConsts.VALIDATION_CATEGORY)] 
     [Description("Indicates whether field must have a value")]
     [DefaultValue(false)]
     public bool Required
     {
       get { return m_Required; }
       set
       {
         if (!Constructed)
         {
           m_Required = value;
           return;
         }
         if (m_Required!=value)
         {
           DisableBindings();
                 m_Validated = false;
                 m_Required = value;
                 AddNotification(new DataEntryTypeChangeNotification(this));
           EnableBindings();      
         }
       }
     }

     /// <summary>
     /// Indicates whether field is a(part of) key 
     /// </summary>
     [Category(CoreConsts.VALIDATION_CATEGORY)] 
     [Description("Indicates whether field is a(part of) key")]
     [DefaultValue(false)]
     public bool KeyField
     {
       get { return m_KeyField; }
       set
       {
         if (!Constructed)
         {
           m_KeyField = value;
           return;
         }
         
         if (m_KeyField!=value)
         {
           DisableBindings();
                 m_Validated = false;
                 m_KeyField = value;
                 AddNotification(new DataEntryTypeChangeNotification(this));
           EnableBindings();      
         }
       }
     }
     

     /// <summary>
     /// Indicates whether field value was modified
     /// </summary>
     [Browsable(false)]
     [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
     public bool Modified
     {
       get { return m_Modified; }
     }

     /// <summary>
     /// Indicates whether field value was modified from any of bound GUI controls.
     /// This property allows to identify fields that have been changed by users
     /// </summary>
     [Browsable(false)]
     [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
     public bool GUIModified
     {
       get { return m_GUIModified; }
     }

     /// <summary>
     /// Indicates whether this field calculation is enabled
     /// </summary>
     [Category(CoreConsts.DATA_CATEGORY),
      Description("Indicates whether this field value is obtained from formula calculation"),
      DefaultValue(false)]
     public bool Calculated
     {
       get { return m_Calculated; }
       set 
       {
         if (!Constructed)
         {
           m_Calculated = value;
           return;
         }
         
         if (m_Calculated!=value)
         {
           DisableBindings();
             m_Calculated = value;
             AddNotification(new DataEntryTypeChangeNotification(this));
           EnableBindings();  
         }  
       }
     }


     /// <summary>
     /// Indicates whether this field calculation formula may be overridden by literal value
     /// </summary>
     [Category(CoreConsts.DATA_CATEGORY),
     Description("Indicates whether this field calculation formula may be overridden by literal value"),
     DefaultValue(false)]
     public bool AllowCalculationOverride
     {
       get { return m_AllowCalculationOverride; }
       set
       {
         if (!Constructed)
         {
           m_AllowCalculationOverride = value;
           return;
         }
         
         if (m_AllowCalculationOverride!=value)
         {
           DisableBindings();
             m_AllowCalculationOverride = value;
             AddNotification(new DataEntryTypeChangeNotification(this));
           EnableBindings();
         }   
       }
     }

     /// <summary>
     /// Calculation formula
     /// </summary>
     [Category(CoreConsts.DATA_CATEGORY),
     Description("Calculation formula"),
     DefaultValue("")]
     public string Formula
     {
       get 
       {
        return m_Formula ?? string.Empty;
       }
       set
       {
         if (m_Formula != value)
         {
          m_Formula = value;
          m_formulaEvaluator = null;
          
          if (Constructed)
           AddNotification(new DataEntryTypeChangeNotification(this));
         }
       }
     }



     /// <summary>
     /// References last raised exception during field formula calculation
     /// </summary>
     [Browsable(false)]
     [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
     public Exception CalculationError
     {
       get { return m_CalculationError; }
     }

     /// <summary>
     /// Indicates whether this field is calculated but value forcefuly overridden 
     /// </summary>
     [Browsable(false)]
     [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
     public bool Overridden
     {
       get { return m_Overridden; }
     }



     /// <summary>
     /// Indicates whether this field's value can be set right now per current field's state 
     /// </summary>
     [Browsable(false)]
     [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
     public bool IsMutable
     {
       get 
       {
         switch (State)
         {
            case DataState.Initializing: break;  
            case DataState.Viewing: return false;
      
            case DataState.Creating:
            case DataState.Editing: 
            {
              if (IsReadonly)
                   return false;

              if ((Calculated) && (!AllowCalculationOverride))
                   return false;

              break;
            }

            default:
             return false;
         }
      
         switch(StorageOperation)
         {
          case StorageOperation.Saving: return false;
          case StorageOperation.Deleting: return false;
         }

         return true;
       }
     }


      /// <summary>
     /// Indicates whether this field's value can be set right now from the attached GUI view 
     /// </summary>
     [Browsable(false)]
     [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
     public bool IsGUIMutable
     {
       get 
       {
          if (this.State == DataState.Editing)
           return IsMutable && !KeyField;
          else
           return IsMutable;
       }
     }






     
     #region Value Accessors 
     /// <summary>
     /// Provides access to field data as object 
     /// </summary>
     [Browsable(false)]
     [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
     public object ValueAsObject
     {
       get { return GetValue(); }
       set { SetValue(value, false); }
     }


     [Browsable(false)]
     [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
     public int ValueAsInt
     {
       get
       {
         int rslt = 0;

         object val = GetValue();

         if (val != null)
           try
           {
             if (val is string)
               rslt = Int32.Parse(val as string);
             else
               rslt = Convert.ToInt32(val);
           }
           catch (Exception err)
           {
             throw new RecordModelException(string.Format(StringConsts.INVALID_FIELD_ACCESSOR_ERROR, FieldName, "ValueAsInt"), err);
           }

         return rslt;
       }
       set { SetValue(value, false); }
     }


     [Browsable(false)]
     [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
     public long ValueAsLong
     {
       get 
       {
        long rslt = 0;
       
        object val = GetValue();
       
        if (val != null)
         try
          {
            if (val is string)
             rslt = Int64.Parse(val as string);
            else 
             rslt = Convert.ToInt64(val);
          } 
          catch(Exception err)
          {
            throw new RecordModelException(string.Format(StringConsts.INVALID_FIELD_ACCESSOR_ERROR, FieldName, "ValueAsLong"), err);
          }
       
        return rslt;
       }
       set { SetValue(value, false); }
     }


     [Browsable(false)]
     [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
     public short ValueAsShort
     {
       get
       {
         short rslt = 0;

         object val = GetValue();

         if (val != null)
           try
           {
             if (val is string)
               rslt = Int16.Parse(val as string);
             else
               rslt = Convert.ToInt16(val);
           }
           catch (Exception err)
           {
             throw new RecordModelException(string.Format(StringConsts.INVALID_FIELD_ACCESSOR_ERROR, FieldName, "ValueAsShort"), err);
           }

         return rslt;
       }
       set { SetValue(value, false); }
     }


     [Browsable(false)]
     [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
     public decimal ValueAsDecimal
     {
       get
       {
         decimal rslt = 0;

         object val = GetValue();

         if (val != null)
           try
           {
             if (val is string)
               rslt = Decimal.Parse(val as string);
             else
               rslt = Convert.ToDecimal(val);
           }
           catch (Exception err)
           {
             throw new RecordModelException(string.Format(StringConsts.INVALID_FIELD_ACCESSOR_ERROR, FieldName, "ValueAsDecimal"), err);
           }

         return rslt;
       }
       set { SetValue(value, false); }
     }


     [Browsable(false)]
     [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
     public DateTime ValueAsDateTime
     {
       get
       {
         DateTime rslt = DateTime.MinValue;

         object val = GetValue();

         if (val != null)
           try
           {
             if (val is string)
               rslt = DateTime.Parse(val as string);
             else
               rslt = Convert.ToDateTime(val);
           }
           catch (Exception err)
           {
             throw new RecordModelException(string.Format(StringConsts.INVALID_FIELD_ACCESSOR_ERROR, FieldName, "ValueAsDateTime"), err);
           }

         return rslt;
       }
       set { SetValue(value, false); }
     }


     [Browsable(false)]
     [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
     public TimeSpan ValueAsTimeSpan
     {
       get
       {
         TimeSpan rslt = TimeSpan.Zero;

         object val = GetValue();

         if (val != null)
           try
           {
             if (val is string)
               rslt = TimeSpan.Parse(val as string);
             else
               rslt = (TimeSpan)val;
           }
           catch (Exception err)
           {
             throw new RecordModelException(string.Format(StringConsts.INVALID_FIELD_ACCESSOR_ERROR, FieldName, "ValueAsTimeSpan"), err);
           }

         return rslt;
       }
       set { SetValue(value, false); }
     }


     [Browsable(false)]
     [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
     public float ValueAsFloat
     {
       get
       {
         float rslt = 0;

         object val = GetValue();

         if (val != null)
           try
           {
             if (val is string)
               rslt = Single.Parse(val as string);
             else
               rslt = Convert.ToSingle(val);
           }
           catch (Exception err)
           {
             throw new RecordModelException(string.Format(StringConsts.INVALID_FIELD_ACCESSOR_ERROR, FieldName, "ValueAsFloat"), err);
           }

         return rslt;
       }
       set { SetValue(value, false); }
     }


     [Browsable(false)]
     [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
     public double ValueAsDouble
     {
       get
       {
         double rslt = 0;

         object val = GetValue();

         if (val != null)
           try
           {
             if (val is string)
               rslt = Double.Parse(val as string);
             else
               rslt = Convert.ToDouble(val);
           }
           catch (Exception err)
           {
             throw new RecordModelException(string.Format(StringConsts.INVALID_FIELD_ACCESSOR_ERROR, FieldName, "ValueAsDouble"), err);
           }

         return rslt;
       }
       set { SetValue(value, false); }
     }


     [Browsable(false)]
     [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
     public string ValueAsString
     {
       get 
       {
        object v = GetValue();
        return v != null ? v.ToString() : null;
       }
       set { SetValue(value, false); }
     }

     /// <summary>
     /// Gets field value as formatted string according to DisplayFormat property
     /// </summary>
     [Browsable(false)]
     [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
     public string ValueAsDisplayString
     {
       get
       {
         if (!HasValue) return string.Empty;
         
         object v = GetValue();

         if (v==null) return string.Empty;
         if (m_DisplayFormat.IsNullOrWhiteSpace()) return v.ToString();

         try
         {
           if (m_DisplayFormat.StartsWith("{0"))
            return m_DisplayFormat.Args(v);
           else
            return "{0:"+m_DisplayFormat+"}".Args(v);
         }
         catch(Exception error)
         {
           return "Error in DisplayFormat('{0}'): {1}".Args(m_DisplayFormat, error.ToMessageWithType());
         }
       }
     }



     [Browsable(false)]
     [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
     public bool ValueAsBool
     {
       get
       {
         bool rslt = false;

         object val = GetValue();

         if (val != null)
           try
           {
             if (val is string)
               rslt = Boolean.Parse(val as string);
             else
               rslt = Convert.ToBoolean(val);
           }
           catch (Exception err)
           {
             throw new RecordModelException(string.Format(StringConsts.INVALID_FIELD_ACCESSOR_ERROR, FieldName, "ValueAsBool"), err);
           }

         return rslt;
       }
       set { SetValue(value, false); }
     }
     
     #endregion

    #endregion


    #region Pub Methods

         /// <summary>
         /// Internal framework member not intended to be used by developers
         /// </summary>
         internal IBinding  __sourceBinding;
         
         
         /// <summary>
         /// Sets value from view binding. This method is used to commit data from view controls into fields
         /// </summary>
         public void SetValueFromGUI(object value, IBinding binding)
         {
            try
            {        
              __sourceBinding = binding;
              
              SetValue(PreProcessValueFromGUI(value), true);
            }
            catch(Exception error)
            {
              m_Validated = true;
              __setValException(error);
            }
            finally
            {
              __sourceBinding = null;
            }
              
            m_GUIModified = true;          
         }

         /// <summary>
         /// Clears value from view binding. This method is used to commit data from view controls into fields
         /// </summary>
         public void ClearValueFromGUI(IBinding binding)
         {
           try
           {
             __sourceBinding = binding;

             Clear();
           }
           catch (Exception error)
           {
             m_Validated = true;
             __setValException(error);
           }
           finally
           {
             __sourceBinding = null;
           }

           m_GUIModified = true;
         }
         
         

         /// <summary>
         /// Clears field value effectively setting HasValue to false
         /// </summary>
         public abstract void Clear();


         /// <summary>
         /// Sets field value to default value
         /// </summary>
         public abstract void Default();
         


         /// <summary>
         /// Resets field modification flag to false
         /// </summary>
         public void ResetModified()
         {
           m_Modified = false;
         }

         /// <summary>
         /// Resets field GUI modified flag to false
         /// </summary>
         public void ResetGUIModified()
         {
           m_GUIModified = false;
         }

         /// <summary>
         /// Resets calculation override flag to false
         /// </summary>
         public void ResetOverridden()
         {
          m_Overridden = false;
         }

         /// <summary>
         /// Resets validation flag
         /// </summary>
         public void ResetValidated()
         {
           m_Validated = false;
         }


         /// <summary>
         /// Returns a type of data stored by field
         /// </summary>
         public abstract Type GetFieldDataType();


         /// <summary>
         /// Returns client scripts, such as JavaScript for particular client-implementation technology.
         /// For example, client frameworks/technologies of a different kind may connect to the same record model.
         /// This implementation probes field-level event first, and if it is set then invokes it, otherwise it uses ClientScript attribute 
         /// from field-declaration level within record to return its text if it is set, 
         /// otherwise script text from resource is returned. 
         /// Override to provide client script programmatically
         /// </summary>
         /// <param name="technology">The name of client-side technology that requests client script, i.e. 'nfx'</param>
         public override ClientScripts GetClientScripts(string technology)
         {
            if (this.Record==null) return base.GetClientScripts(technology);

            var scripts = OnGetClientScriptsEvent(technology);
            if (scripts!=null) return scripts;
            
            var tr = Record.GetType();
            var finf = tr.GetFields(BindingFlags.NonPublic |
                                    BindingFlags.Public |
                                    BindingFlags.Instance).FirstOrDefault(fi => object.ReferenceEquals(fi.GetValue(Record), this));

            if (finf==null) return base.GetClientScripts(technology);
          
            var atrs = finf.GetCustomAttributes(typeof(ClientScriptAttribute), false).Cast<ClientScriptAttribute>();
      
            return ClientScriptAttribute.GetScripts(tr, finf.Name, atrs, technology); 
         }


    #endregion

    
    #region Events
     
     /// <summary>
     /// Event fired before field's value is about to change. Throw exceptions to abort the change
     /// </summary>
     [Category(CoreConsts.CHANGE_EVENTS_CATEGORY),
     Description(@"Event fired before field's value is about to change. Throw exceptions to abort the change")]
     [field: NonSerialized]
     public event FieldDataChangeEventHandler FieldDataChanging;
     
     /// <summary>
     /// Event fired after field's value has changed
     /// </summary>
     [Category(CoreConsts.CHANGE_EVENTS_CATEGORY),
     Description(@"Event fired after field's value has changed")]
     [field: NonSerialized]
     public event FieldDataChangeEventHandler FieldDataChanged;
     
    #endregion
   
   
    #region Protected
   
     protected abstract object GetValue();
     protected abstract void SetValue(object value, bool fromGUI);


     /// <summary>
    /// Internal framework method not intended to be called by developers
    /// </summary>
    internal virtual void _setValueTypeHasValue()
    {
    }


     protected internal void EnsureCanModifyValue()
     {
           
      switch (State)
      {
       case DataState.Initializing: { break; }  //anyting is allowed in this state
       case DataState.Viewing: { throw new RecordModelException(string.Format(StringConsts.VIEW_MODIFY_ERROR, FieldName)); }
      
       case DataState.Creating:
       case DataState.Editing: 
        {
          if (IsReadonly)
            throw new RecordModelException(string.Format(StringConsts.READONLY_FIELD_ERROR, FieldName));

          if ((Calculated) && (!AllowCalculationOverride))
            throw new RecordModelException(string.Format(StringConsts.CALC_FIELD_CHANGE_ERROR, FieldName));
          break;
        }

       default:
         throw new RecordModelException(StringConsts.UNINITIALIZED_STATE_ERROR);
      }
      
      switch(StorageOperation)
      {
        case StorageOperation.Saving: { throw new RecordModelException(string.Format(StringConsts.SAVE_MODIFY_ERROR, FieldName)); }
        case StorageOperation.Deleting: { throw new RecordModelException(string.Format(StringConsts.DELETE_MODIFY_ERROR, FieldName)); }
      }
        
     }


     protected Evaluator GetFormulaEvaluator()
     {
        if (string.IsNullOrEmpty(m_Formula)) return null;
          
        if (m_formulaEvaluator!=null) return m_formulaEvaluator;

        m_formulaEvaluator = new Evaluator(m_Formula);  
        m_formulaEvaluator.OnIdentifierLookup += formulaIdentifierLookupHandler;

        return m_formulaEvaluator;
     }


     protected internal override void DoCreate()
     {
       base.DoCreate();

       Clear();
       Default();
      
       ResetGUIModified();
       ResetModified();
       ResetOverridden();
       ResetValidated();
     }


     protected internal override void DoEdit()
     {
       base.DoEdit();

       ResetGUIModified();
       ResetModified();
       ResetValidated();
     }

     protected internal override void DoEndLoad()
     {
       base.DoEndLoad();

       ResetGUIModified();
       ResetModified();
       ResetOverridden();
     }



     protected override void CheckCreateAllowed()
     {
       base.CheckCreateAllowed();
       
       if (Record != null)
         throw new RecordModelException(string.Format(StringConsts.FIELD_OPERATION_NOT_ALLOWED_ERROR, m_FieldName, "Create"));
     }

     protected override void CheckEditAllowed()
     {
       base.CheckEditAllowed();
     
       if (Record != null)
         throw new RecordModelException(string.Format(StringConsts.FIELD_OPERATION_NOT_ALLOWED_ERROR, m_FieldName, "Edit"));
     }

     protected override void CheckPostAllowed()
     {
       base.CheckPostAllowed();
     
       if (Record != null)
         throw new RecordModelException(string.Format(StringConsts.FIELD_OPERATION_NOT_ALLOWED_ERROR, m_FieldName, "Post"));
     }

     protected override void CheckCancelAllowed()
     {
       base.CheckCancelAllowed();
     
       if (Record != null)
         throw new RecordModelException(string.Format(StringConsts.FIELD_OPERATION_NOT_ALLOWED_ERROR, m_FieldName, "Cancel"));
     }


     protected override void CheckLoadAllowed()
     {
       base.CheckLoadAllowed();

       if (Record != null)
         throw new RecordModelException(string.Format(StringConsts.FIELD_OPERATION_NOT_ALLOWED_ERROR, m_FieldName, "Load"));
     }

     protected override void CheckSaveAllowed()
     {
       base.CheckSaveAllowed();

       if (Record != null)
         throw new RecordModelException(string.Format(StringConsts.FIELD_OPERATION_NOT_ALLOWED_ERROR, m_FieldName, "Save"));
     }

     protected override void CheckDeleteAllowed()
     {
       base.CheckDeleteAllowed();

       if (Record != null)
         throw new RecordModelException(string.Format(StringConsts.FIELD_OPERATION_NOT_ALLOWED_ERROR, m_FieldName, "Delete"));
     }


     protected override void PerformValidation(ref bool couldNotValidate)
     {
       if (Required && !HasValue)
         throw new RequiredValidationException(string.Format(StringConsts.REQUIRED_FIELD_ERROR, string.IsNullOrEmpty(Description)?FieldName:Description), this);
         
       if (
           (m_DataEntryType == DataEntryType.Lookup || m_DataEntryType == DataEntryType.DirectEntryOrLookupWithValidation) && 
           (m_LookupType== LookupType.Dictionary) &&
           (HasValue)
          )
          if (!LookupDictionary.FindKey(ValueAsString.Trim()))
            throw new LookupValidationException(string.Format(StringConsts.LOOKUP_DICTIONARY_FIELD_ERROR, Description), this);
     }

     protected internal override void CreatePreEditStateCopy()
     {
       base.CreatePreEditStateCopy();

       m_PreEdit_Overridden = m_Overridden;
       m_PreEdit_CalculationError = m_CalculationError;
     }

     protected internal override void DropPreEditStateCopy()
     {
       base.DropPreEditStateCopy();
                        
       //nothing to cleanup at this level
     }
     
     protected internal override void RevertToPreEditState()
     {
          base.RevertToPreEditState();
     
          m_Overridden = m_PreEdit_Overridden;
          m_CalculationError = m_PreEdit_CalculationError;
     }
   
   
     /// <summary>
     /// Invoked before value gets written into field buff. Override to take actions like re-parsing strings etc..
     /// </summary>
     protected virtual object PreProcessValueFromGUI(object value)
     {
       return value;
     }


     /// <summary>
     /// Triggers field data change event for field and owner record.
     /// Do not forget to call base implementation when overriding
     /// </summary>
     protected internal virtual void OnFieldDataChanging(FieldDataChangeEventArgs args)
     {
       if (FieldDataChanging != null)
             FieldDataChanging(this, args);
             
       if (Record!=null)
         Record.OnFieldDataChanging(this, args);      
     }

     /// <summary>
     /// Triggers field data change event for field and owner record.
     /// Do not forget to call base implementation when overriding
     /// </summary>
     protected internal virtual void OnFieldDataChanged(FieldDataChangeEventArgs args)
     {
       if (FieldDataChanged != null)
             FieldDataChanged(this, args);
             
       if (Record!=null)
         Record.OnFieldDataChanged(this, args);      
     }



     protected internal override void ApplyDefinitionAttributes(IEnumerable<object> attributes)
     {
         var attr = attributes.FirstOrDefault(a => a.GetType() == typeof(FieldDefAttribute)) as FieldDefAttribute;
         if (attr == null) return;

         this.FieldName = attr.Name;
         this.Description = attr.Description;
         this.StoreFlag = attr.StoreFlag;
         this.Required = attr.Required;
         this.Calculated = attr.Calculated;
         this.Formula = attr.Formula;
         this.DataEntryType = attr.DataEntryType;
         this.LookupType = attr.LookupType;

         if (attr.LookupDictionary!=null)
          this.LookupDictionary.AddRange(attr.LookupDictionary);

         this.Marked = attr.Marked;         
     }


     
   
    #endregion



    #region Private Utils

     
     
     
     private string formulaIdentifierLookupHandler(string ident)
     {
       if (string.IsNullOrEmpty(ident)) return ident;
       
       string fName = ident.Trim();
       if ((fName.Length > CoreConsts.FORMULA_FIELD_NAME_PREFIX.Length) &&
           (fName.StartsWith(CoreConsts.FORMULA_FIELD_NAME_PREFIX)))
       {
         fName = fName.Substring(CoreConsts.FORMULA_FIELD_NAME_PREFIX.Length);
         if (Record!=null)
         {
           var fld = Record[fName];

           if (fld==this)
            throw new RecordModelException(StringConsts.RECURSIVE_FORMULA_FIELD_ERROR + fName);
           if (fld.Calculated)
            throw new RecordModelException(StringConsts.FORMULA_CALCULATED_ANOTHER_FIELD_ERROR + fName);

           return fld.ValueAsString;
         }
         else
           return string.Empty;
       } 
     
       return ident;
     }


    #endregion

 
 
     #region Object Overrides
         public override string ToString()
         {
           object val = GetValue();
            
           return string.Format("<{0}> = \"{1}\"",
                    string.IsNullOrEmpty(FieldName) ? StringConsts.UNKNOWN_STRING : FieldName,
                    ((val != null) && HasValue) ? val.ToString() : StringConsts.NULL_STRING);
       
         }

         public override int GetHashCode()
         {
           var val = GetValue();
           var vhc =  val!=null ? val.GetHashCode() : 0;

           return FieldName.GetHashCode() + vhc;
         }

         public override bool Equals(object obj)
         {
             return CompareTo(obj) == 0;
         }
            
     #endregion


     #region IComparable,IComparable<Field> Members

         public int CompareTo(object obj)
         {
           return CompareTo(obj as Field);
         }

         public virtual int CompareTo(Field obj)
         {
           throw new RecordModelException(StringConsts.FIELD_COMPARISON_NOT_IMPLEMENTED_ERROR);
         }

     #endregion
  }


    
}
