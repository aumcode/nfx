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
using System.ComponentModel.Design;
using System.Linq;
using System.Text;
using System.Runtime.Serialization;

using NFX.DataAccess;
using NFX.RecordModel.DataAccess;
using NFX.ApplicationModel;

namespace NFX.RecordModel
{
  /// <summary>
  /// Provides base record-management model functionality. View controls are bound to records 
  /// </summary>
  [Serializable]
  public abstract class Record : ModelBase, ICustomTypeDescriptor
  {
    #region STATIC

      /// <summary>
      /// Main method for creation of new record instances which is faster than Make&lt;T&gt; 
      /// </summary>
      /// <typeparam name="T">A instance of record to build</typeparam>
      /// <returns>New record instance initialized with default state</returns>
      public static T Build<T>(T instance) where T : Record
      {
          var result = instance;
          result.ctor_MakeRecordInstanceFromScratch();
          return result;
      }



      /// <summary>
      /// Main method for creation of new record instances. 
      /// 2013 Update: for performance use Build(T) instead
      /// </summary>
      /// <typeparam name="T">A type of record to create</typeparam>
      /// <returns>New record instance initialized with default state</returns>
      public static T Make<T>() where T : Record, new()
      {
       //This is slow:
          //http://www.csharp-architect.com/post/2009/06/11/Generic-new-T%28%29-Performance-Issue-and-a-Solution.aspx
          //http://msmvps.com/blogs/jon_skeet/archive/2011/08/22/optimization-and-generics-part-1-the-new-constraint.aspx
        var result = new T();
        result.ctor_MakeRecordInstanceFromScratch();
        return result;
      }

      /// <summary>
      /// Main method for creation of new record instances
      /// </summary>
      /// <param name="type">A type of record to create</param>
      /// <returns>New record instance initialized with default state</returns>
      public static Record Make(Type type)
      {
        if (type==null)
          throw new RecordModelException(StringConsts.INVALID_RECORD_TYPE_ERROR + StringConsts.NULL_STRING);
        
        var result = Activator.CreateInstance(type) as Record;
        
        if (result==null)
          throw new RecordModelException(StringConsts.INVALID_RECORD_TYPE_ERROR + type.ToString());
        
        result.ctor_MakeRecordInstanceFromScratch();
        return result;
      }

      /// <summary>
      /// Main method for creation of new record instances
      /// </summary>
      /// <param name="typeName">A type name of record to create</param>
      /// <returns>New record instance initialized with default state</returns>
      public static Record Make(string typeName)
      {
        if (string.IsNullOrEmpty(typeName))
          throw new RecordModelException(StringConsts.INVALID_RECORD_TYPE_ERROR + StringConsts.NULL_STRING);
        
        var type = Type.GetType(typeName);

        if (type == null)
          throw new RecordModelException(StringConsts.INVALID_RECORD_TYPE_ERROR + typeName);
        
        return Make(type);
      }
    
   
    #endregion
   
    #region .ctor
      protected Record() : base()
      {
      
      }        
    #endregion
    
    #region Private fields
     [NonSerialized]
     private bool m_DesignTimeSurrogate;  internal void _markAsSurrogate() { m_DesignTimeSurrogate = true; }
     
     private bool m_FieldValidationSuspended = true;//20130228 Dkh added default TRUE
     
     private bool m_CaseSensitiveFieldBinding;
     
     private string m_RecordName;
     
     private string m_TableName;

          
     /// <summary>
     /// Suppresses record crosschecks like duplicate field name detection so record construction speed increases significantly.
     /// Set to true in ConstructFields override while setting field names, or use attribute on Record-derived class
     /// </summary>
     [NonSerialized]
     protected internal bool NoBuildCrosschecks;
     
     
    #endregion

    #region Properties


         
     
     /// <summary>
     /// Provides a user-meaningful name for the record
     /// </summary>
     [Description("Provides a user-meaningful name for the record"),
     DefaultValue("")]
     public string RecordName
     {
       get { return m_RecordName ?? string.Empty; } 
       set { m_RecordName = value; }
     }


     /// <summary>
     /// Provides table name that this record comes/goes from/to
     /// </summary>
     [Description("Provides table name that this record comes/goes from/to"),
     DefaultValue("")]
     public string TableName
     {
       get { return m_TableName ?? string.Empty; }
       set { m_TableName = value; }
     }




     /// <summary>
     /// Indicates that this record instance was created in design-time from string type name and does not
     ///  represent a real record instance
     /// </summary>
     [Browsable(false)]
     public bool DesignTimeSurrogate
     {
       get { return m_DesignTimeSurrogate; }
     }
     
     /// <summary>
     /// Provides access to fields by name. Exception is thrown if a field with such a name does not exist in this record
     /// </summary>
     public Field this[string fName]
     {
       get 
       {
         if (string.IsNullOrEmpty(fName))
           throw new RecordModelException(StringConsts.INVALID_ARGUMENT_ERROR+": Record[fName] - fName is blank or null");
        
         Field fld = FindFieldByName(fName);
         
         if (fld==null)
           throw new RecordModelException(string.Format(StringConsts.FIELD_NAME_NOT_FOUND_ERROR, fName));
         
         return fld;
         
       }
     }
    
     /// <summary>
     /// Provides access to all fields that a record contains
     /// </summary>
     [Browsable(false)]
     public IEnumerable<Field> Fields
     {
       get 
       {      
         return OwnedItems.Where((item => item is Field)).Cast<Field>();
       }
     }


     /// <summary>
     /// Provides access to all record fields that have been modified
     /// </summary>
     [Browsable(false)]
     public IEnumerable<Field> ModifiedFields
     {
       get
       {
         return Fields.Where(fld => fld.Modified);
       }
     }


     /// <summary>
     /// Provides access to all record fields that have been modified through GUI
     /// </summary>
     [Browsable(false)]
     public IEnumerable<Field> GUIModifiedFields
     {
       get
       {
         return Fields.Where(fld => fld.GUIModified);
       }
     }

     /// <summary>
     /// Returns true when record contains modified fields
     /// </summary>
     [Browsable(false)]
     public bool HasModifiedFields
     {
       get
       {
         return ModifiedFields.FirstOrDefault() != null;
       }
     }


     /// <summary>
     /// Returns true when record contains fields modified from GUI
     /// </summary>
     [Browsable(false)]
     public bool HasGUIModifiedFields
     {
       get
       {
         return GUIModifiedFields.FirstOrDefault() != null;
       }
     }



     /// <summary>
     /// Provides access to all record fields that either have not validated yet or could not be validated 
     /// </summary>
     [Browsable(false)]
     public IEnumerable<Field> InvalidFields
     {
       get
       {
         return Fields.Where(fld => !fld.Valid);
       }
     }
     
     /// <summary>
     /// Provides access to all record fields that have validated but not valid 
     /// </summary>
     [Browsable(false)]
     public IEnumerable<Field> InvalidValidatedFields
     {
       get
       {
         return Fields.Where(fld => fld.Validated && !fld.Valid);
       }
     }

     /// <summary>
     /// Provides access to all record fields that have passed validation
     /// </summary>
     [Browsable(false)]
     public IEnumerable<Field> ValidFields
     {
       get
       {     
         return Fields.Where(fld => fld.Valid);
       }
     }

     /// <summary>
     /// Returns validation exception messages for all fields 
     /// </summary>
     [Browsable(false)]
     public IEnumerable<string> FieldValidationExceptionMessages
     {
       get
       {     
         return Fields.Where(fld => fld.ValidationException!=null).Select<Field, string>(fld => fld.ValidationException.Message);
       }
     }    

     /// <summary>
     /// Returns validation exception messages concatenated into one string 
     /// </summary>
     [Browsable(false)]
     public string AllValidationExceptionMessages
     {
       get
       {     
         var sb = new StringBuilder();
         
         if (this.ValidationException!=null)
          sb.AppendLine("* " + this.ValidationException.Message);

         foreach(var msg in FieldValidationExceptionMessages)
          sb.AppendLine("* " + msg);

         return sb.ToString();
       }
     }


     /// <summary>
     /// Indicates whether validation was suspended for all fields until record post
     /// </summary>
     [Browsable(false)]
     [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
     [DefaultValue(true)]
     public bool FieldValidationSuspended
     {
      get
      {
        return m_FieldValidationSuspended;
      }
      set
      {
        m_FieldValidationSuspended = value;
      }
     }
     
     /// <summary>
     /// Controls whether field search is or is not case sensitive
     /// </summary>
     [Category(CoreConsts.DATA_CATEGORY),
     Description("Controls whether field search is or is not case sensitive"),
     DefaultValue(false)]
     public bool CaseSensitiveFieldBinding
     {
      get { return m_CaseSensitiveFieldBinding; }
      set { m_CaseSensitiveFieldBinding = value; }
     }


    #endregion
   
    #region Pub Methods
     
     

    /// <summary>
    /// Initializes an instance for data loading and loads data from Application model data store.
    /// </summary>
    public void Load(IDataStoreKey key, params object[] extra)
    {
      base.Load(App.DataStore as IModelDataStore, key, extra);
    }

    /// <summary>
    /// Saves data into Application model data store under this instance's DataStoreKey
    /// </summary>
    public void Save()
    {
      base.Save(App.DataStore as IModelDataStore);
    }


    /// <summary>
    /// Saves data into Application model data store using specified key. If key is null then provider may elect to build key from fields with KeyField attribute set to true
    /// </summary>
    public void Save(IDataStoreKey key, params object[] extra)
    {
      base.Save(App.DataStore as IModelDataStore, key, extra);
    }


    /// <summary>
    /// Deletes data from Application model data store under this instance's DataStoreKey.
    /// After this call model state is DataState.Uninitialized
    /// </summary>
    public void Delete()
    {
      base.Delete(App.DataStore as IModelDataStore);
    }


    /// <summary>
    /// Deletes data from Application model data with specified key. When key is omitted instance's DataStoreKey is used.
    /// After this call model state is DataState.Uninitialized
    /// </summary>
    public void Delete(IDataStoreKey key, params object[] extra)
    {
      base.Delete(App.DataStore as IModelDataStore, key, extra);
    }

     
     /// <summary>
     /// Outputs log message to application log
     /// </summary>
     public void Log(Log.Message message)
     {
       App.Log.Write(message);
     }
     
     /// <summary>
     /// Outputs log message to application log
     /// </summary>
     public void Log(Log.MessageType type, string message)
     {
       Log(new Log.Message
                    {
                      Text = message ?? string.Empty,
                      Type = type,
                      From = this.RecordName,
                      Topic = CoreConsts.RECORD_TOPIC,
                      Parameters = this.TableName
                    }
          );          
     }
     
     
     /// <summary>
     /// Tries to find a field by name or returns null if such named field was not found
     /// </summary>
     public Field FindFieldByName(string fName)
     {
       fName = fName ?? string.Empty;
       
       if (!m_CaseSensitiveFieldBinding) fName = fName.ToUpper();

       Field result = Fields.FirstOrDefault((item => string.Equals(item.FieldName, fName)));
       
       return result;
     }
     
     
     /// <summary>
     /// Resets Modified flag to false for all fields
     /// </summary>
     public void ResetModified()
     {
       foreach(Field fld in Fields)
         fld.ResetModified();
     }

     /// <summary>
     /// Resets Modified flag to false for all fields
     /// </summary>
     public void ResetGUIModified()
     {
       foreach (Field fld in Fields)
         fld.ResetGUIModified();
     }


    #endregion

     #region Events
       /// <summary>
       /// Event fired before field's value is about to change. Throw exceptions to abort the change
       /// </summary>
       [Category(CoreConsts.CHANGE_EVENTS_CATEGORY),
       Description(@"Event fired before field's value is about to change. Throw exceptions to abort the change")]
       [field:NonSerialized]
       public event FieldDataChangeEventHandler FieldDataChanging;
       
       /// <summary>
       /// Event fired after field's value has changed
       /// </summary>
       [Category(CoreConsts.CHANGE_EVENTS_CATEGORY),
       Description(@"Event fired after field's value has changed")]
       [field:NonSerialized]
       public event FieldDataChangeEventHandler FieldDataChanged;
    #endregion



    #region Protected

      /// <summary>
      /// Normally .ctors should never be overridden.
      /// This is because records may be rehydrated from some storage (i.e. stream) and only default cotr will be called.
      /// However, for some records that are not serializable, when parametrized .ctors are needed
      ///  this method should be called as the last .ctor line
      /// </summary>
      protected void ctor_MakeRecordInstanceFromScratch()
      {
        m_Constructed = false;
         this.ApplyDefinitionAttributes(null);//20130217 Dkh added
         ConstructFields();
         AttachEvents();
        m_Constructed = true;
      }


      protected override void _CheckForDuplicateOwnedItemRegistration(ModelBase anItem)
      {
          if (!NoBuildCrosschecks)
           base._CheckForDuplicateOwnedItemRegistration(anItem);
      }
     
     
     /// <summary>
     /// Performs record field construction/allocation. Override to build fields in code
     /// </summary>
     protected virtual void ConstructFields()
     {
       FieldDefAttribute.BuildAndDefineFields(this);
     }

                   
                   
                     private volatile static Dictionary<Type, RecordDefAttribute> s_defs = new Dictionary<Type,RecordDefAttribute>(); 



     protected internal override void ApplyDefinitionAttributes(IEnumerable<object> attributes)
     {
        Type recType = GetType();
        RecordDefAttribute attr = null;
        
        if (!s_defs.TryGetValue(recType, out attr)) 
        {
         if (attributes==null)
           attributes = GetType().GetCustomAttributes(false);

         attr = attributes.FirstOrDefault(a => a.GetType() == typeof(RecordDefAttribute)) as RecordDefAttribute;
         var newDefs = new Dictionary<Type,RecordDefAttribute>(s_defs);
         
         if (!newDefs.ContainsKey(recType))//other thread may have already added
          newDefs.Add(recType, attr);
         s_defs = newDefs;//atomic ref thread safe
        }

       if (attr == null) return;

       this.RecordName = attr.Name;
       this.TableName = attr.TableName;
       this.StoreFlag = attr.StoreFlag;
       this.FieldValidationSuspended = attr.FieldValidationSuspended;
       this.NoBuildCrosschecks = attr.NoBuildCrosscheks; 
       this.SupportsNotificationBinding = attr.SupportsNotificationBinding;      
     }

     
     protected internal override void CreatePreEditStateCopy()
     {
       base.CreatePreEditStateCopy();
      
       foreach (Field fld in Fields)
         fld.CreatePreEditStateCopy();
     }

     protected internal override void DropPreEditStateCopy()
     {
       base.DropPreEditStateCopy();

       foreach (Field fld in Fields)
         fld.DropPreEditStateCopy();
     }

     protected internal override void RevertToPreEditState()
     {
       base.RevertToPreEditState();
      
       foreach (Field fld in Fields)
         if (!fld.IsReadonly &&
              fld.IsApplicable &&
             (!fld.Calculated || (fld.Calculated && fld.AllowCalculationOverride))
            )
           fld.RevertToPreEditState();
     }


     protected internal override void DoCreate()
     {
        bool wasSuspended = m_FieldValidationSuspended;
        
        m_FieldValidationSuspended = true;
        try
        {
        
          foreach(Field fld in Fields)
            fld.DoCreate();
        }
        finally
        {
          m_FieldValidationSuspended = wasSuspended;
        }
        
     }

     protected internal override void DoEdit()
     {
       bool wasSuspended = m_FieldValidationSuspended;

       m_FieldValidationSuspended = true;
       try
       {

         foreach (Field fld in Fields)
            fld.DoEdit();
       }
       finally
       {
         m_FieldValidationSuspended = wasSuspended;
       }

     }




     protected internal override void DoEndLoad()
     {
       base.DoEndLoad();

       foreach (Field fld in Fields)
        fld.DoEndLoad();
     }

     protected internal override void DoCancelLoad()
     {
       base.DoCancelLoad();

       foreach (Field fld in Fields)
        fld.DoCancelLoad();
     }
     
     
     

     protected override void PerformValidation(ref bool couldNotValidate)
     {
        foreach(Field fld in Fields)
          fld.Validate();  //validate ALL filds first

        foreach (Field fld in Fields) //then trow first exception (if any) 
         if (fld.ValidationException!=null)
           throw fld.ValidationException;                         
     }


     protected override void BeforeOwnedItemAddition(ModelBase addingItem)
     {
       if ((addingItem!=null) && (addingItem is Field))
       {
         Field fld = (Field)addingItem;
         
         if (fieldNameAlreadyTaken(fld, fld.FieldName))
           throw new RecordModelException(string.Format(StringConsts.DUPLICATE_FIELD_NAME_ERROR, fld.FieldName));
            
       }
     }
     
     
     protected internal virtual void OnFieldDataChanging(Field sender, FieldDataChangeEventArgs args)
     {                                
       if (FieldDataChanging != null)
             FieldDataChanging(sender, args);
     }

     protected internal virtual void OnFieldDataChanged(Field sender, FieldDataChangeEventArgs args)
     {
       if (FieldDataChanged != null)
             FieldDataChanged(sender, args);
     } 

    #endregion

     
    #region Private Utils


      internal bool fieldNameAlreadyTaken(Field fld, string newName)
      {     
        if (string.IsNullOrEmpty(newName)) return false;
        
        Field f = Fields.FirstOrDefault(item => (item != fld) && (string.Equals(item.FieldName, newName)));
        
        return f != null;
      }


      


    #endregion




    #region ICustomTypeDescriptor Members

        /// <summary>
        /// Implements ICustomTypeDescriptor used by component designers
        /// </summary>
        public AttributeCollection GetAttributes()
        {
          return TypeDescriptor.GetAttributes(this, true);
        }

        /// <summary>
        /// Implements ICustomTypeDescriptor used by component designers
        /// </summary>
        public string GetClassName()
        {
          return TypeDescriptor.GetClassName(this, true);
        }
        
        /// <summary>
        /// Implements ICustomTypeDescriptor used by component designers
        /// </summary>
        public string GetComponentName()
        {
          return TypeDescriptor.GetComponentName(this, true);
        }

        /// <summary>
        /// Implements ICustomTypeDescriptor used by component designers
        /// </summary>
        public TypeConverter GetConverter()
        {
          return TypeDescriptor.GetConverter(this, true);
        }

        /// <summary>
        /// Implements ICustomTypeDescriptor used by component designers
        /// </summary>
        public EventDescriptor GetDefaultEvent()
        {
          return TypeDescriptor.GetDefaultEvent(this, true);
        }

        /// <summary>
        /// Implements ICustomTypeDescriptor used by component designers
        /// </summary>
        public PropertyDescriptor GetDefaultProperty()
        {
          return TypeDescriptor.GetDefaultProperty(this, true);
        }

        /// <summary>
        /// Implements ICustomTypeDescriptor used by component designers
        /// </summary>
        public object GetEditor(Type editorBaseType)
        {
          return TypeDescriptor.GetEditor(this, editorBaseType, true);
        }

        /// <summary>
        /// Implements ICustomTypeDescriptor used by component designers
        /// </summary>
        public EventDescriptorCollection GetEvents(Attribute[] attributes)
        {
          return TypeDescriptor.GetEvents(this, attributes, true);
        }

        /// <summary>
        /// Implements ICustomTypeDescriptor used by component designers
        /// </summary>
        public EventDescriptorCollection GetEvents()
        {
          return TypeDescriptor.GetEvents(this, true);
        }

        /// <summary>
        /// Implements ICustomTypeDescriptor used by component designers
        /// </summary>
        public PropertyDescriptorCollection GetProperties(Attribute[] attributes)
        {
          var result = new PropertyDescriptorCollection(null);
          
          foreach(Field fld in Fields)
            if (!string.IsNullOrEmpty(fld.FieldName))
                result.Add(new FieldPropertyDescriptor(fld));
           
          return result; 
        }

        /// <summary>
        /// Implements ICustomTypeDescriptor used by component designers
        /// </summary>
        public PropertyDescriptorCollection GetProperties()
        {
          return this.GetProperties(null);
        }

        /// <summary>
        /// Implements ICustomTypeDescriptor used by component designers
        /// </summary>
        public object GetPropertyOwner(PropertyDescriptor pd)
        {
          return this;
        }

    #endregion
    
    
  }//Record
  
  
  
  
  
}
