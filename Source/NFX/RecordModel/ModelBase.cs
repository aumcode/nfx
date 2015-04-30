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
using System.ComponentModel.Design.Serialization;
using System.CodeDom;
using System.Runtime.Serialization;

using System.Text;
using System.Linq;

using NFX.Environment;
using NFX.DataAccess;
using NFX.RecordModel.DataAccess;

namespace NFX.RecordModel
{
  /// <summary>
  /// Provides an abstract base for Record and Field model classes
  /// </summary>
  [Serializable]
  public abstract class ModelBase : IControllableInteractions, ISupportInitialize, IDeserializationCallback
  {
  
    #region Private Fields
      
      //indicates that instance has been constructed and ready to be used
      internal bool m_Constructed = true; 
      
      
      //What owns this instance
      //this is purposely object to bypass MS designer problems
      private object m_Owner = null; 
           
      //What this instance owns
      private List<ModelBase> m_OwnedItems;
      
      private DataState m_State; 
      
      private StorageOperation m_StorageOperation;
      
      private ChangeType  m_LastPostedChange;
      
      private string m_PermissionNamespace; 
      private string m_PermissionName;
      
      private StoreFlag m_StoreFlag;  
      
            
      private bool m_Applicable = true;
      private bool m_Visible = true;
      private bool m_Enabled = true;
      private bool m_Readonly;
      
      protected bool m_Validated;

      private bool m_ThrowOnInitValidation = true;
             
      private bool m_DeferredValidation;   
      private Exception m_ValidationException; internal void __setValException(Exception error) {  m_ValidationException = error; }
     
      private int m_DisableBindingsLevel;

      private bool m_SupportsNotificationBinding;


    #endregion
  
    #region .ctor
      
      protected ModelBase()
      {
        
      }

    #endregion

    #region Properties

    
    /// <summary>
    /// Determines whether this model supports sending notifications to bindings
    /// </summary>
      public virtual bool SupportsNotificationBinding
      {
          get { return m_SupportsNotificationBinding; }
          set { m_SupportsNotificationBinding = value; }
      }
    
    /// <summary>
    /// Indicates whether instance was created and internal data structures constructed
    /// </summary>
    public bool Constructed
    {
      get
      {
        if (m_Owner==null) return m_Constructed;
        if (!(m_Owner is ModelBase)) return m_Constructed;
        
        return ((ModelBase)m_Owner).Constructed;
      }
    }

        
    /// <summary>
    /// Provides access to an owner object
    /// </summary>
    [Browsable(false)]
    [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
    public object Owner
    {
      get 
      {
        return m_Owner;
      }
      set
      {
        if (m_Owner == value) return;
        
        
         if ((value != null) && (m_Owner != null) && (m_Owner != value))
          throw new RecordModelException(StringConsts.OWNER_BINDING_ERROR);
   
        if (value==null)
           ReleaseOwner();
        else   
        { 
         //before setting m_Owner, try to register with this's owner - this may fail because of naming, etc...
         if (value is ModelBase)
           ((ModelBase)value).addOwnedItem(this);
         
         m_Owner = value;
        }
      }
    }

   
    /// <summary>
    /// Provides access to items owned by this model instance
    /// </summary>
    [Browsable(false)]
    [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
    public IEnumerable<ModelBase> OwnedItems
    {
      get
      {
        if (m_OwnedItems==null) yield break;
        
        foreach(var item in m_OwnedItems)
         yield return item;
      }
     
    }

    /// <summary>
    /// Returns data state of this model
    /// </summary>
    [Browsable(false)]
    public virtual DataState State
    {
      get
      {
        return m_State;
      }

    }

    /// <summary>
    /// Returns ongoing data storage operation
    /// </summary>
    [Browsable(false)]
    public virtual StorageOperation StorageOperation
    {
      get
      {
        return m_StorageOperation;
      }

    }


    /// <summary>
    /// Returns a type of the last change posted to model
    /// </summary>
    [Browsable(false)]
    public virtual ChangeType LastPostedChange
    {
      get
      {
        return m_LastPostedChange;
      }

    }
    
    
    /// <summary>
    /// Returns the key that this model was loaded or saved with or null
    /// </summary>
    [Browsable(false)]
    public virtual IDataStoreKey DataStoreKey
    {
      get
      {
        return m_DataStoreKey;
      }
    }
    

    
    /// <summary>
    /// Establishes permission namespace for an item. Items owned by this item resolve thier permission names through 
    ///  owner's permission namespace, i.e. if permission namespace is set to "CustomerPaymentRecord" in a record,
    ///  then all record's fields will lookup their permission names according to this pattern:
    ///     "CustomerPaymentRecord.[Field.PermissionName]"
    /// </summary>
     [Category(CoreConsts.SECURITY_CATEGORY),
     Description(
      @"Establishes permission namespace for an item. Owned items resolve thier permission names through 
        owner's permission namespace, i.e. if permission namespace is set to ""CustomerPaymentRecord"" in a record,
        then all record's fields will lookup their permission names according to this pattern:
         ""CustomerPaymentRecord.[Field.PermissionName]"""),
     DefaultValue("")]
    public string PermissionNamespace
    {
       get { return m_PermissionNamespace ?? string.Empty; }
       set 
       {
         m_PermissionNamespace = value;
         
         OnSecurityChanged(EventArgs.Empty);
       }
    }

     /// <summary>
     /// Establishes permission name for an item. When checking permissions, items concatenate 
     /// their owner's permission namespace name with their own permission name to resolve full permission name
     ///  through security provider
     /// </summary>
     [Category(CoreConsts.SECURITY_CATEGORY),
     Description(
      @"Establishes permission name for an item. When checking permissions, items concatenate 
        their owner's permission namespace name with their own permission name to resolve full permission name
        through security provider"),
     DefaultValue("")]
    public string PermissionName
    {
       get { return m_PermissionName ?? string.Empty; }
       set
       {
         m_PermissionName = value;
         
         OnSecurityChanged(EventArgs.Empty);
       }
    }



    /// <summary>
    /// Determines whether this entity should be persisted in a storage (such as a database) or loaded from storage or both.
    /// Non-stored entities are most commonly used in client-side functionality like temp-calculated values etc.
    /// </summary>
    [Category(CoreConsts.DATA_CATEGORY),
     Description(
      @"Determines whether this entity should be persisted in a storage (such as a database) or loaded from storage or both.
        Non-stored entities are most commonly used in client-side functionality like temp-calculated values etc."),
     DefaultValue(StoreFlag.LoadAndStore)]
    public StoreFlag StoreFlag 
    {
      get {return m_StoreFlag;}
      set { m_StoreFlag = value; }
    }


    /// <summary>
    /// Specifies current nesting level of DisableBindings() call
    /// </summary>
    [Browsable(false)]
    public int DisableBindingsLevel
    {
      get { return m_DisableBindingsLevel; }
    }


    /// <summary>
    /// Provides access to change notifications pending on this model instance.
    /// Notifications are purged after final EnableBindings() call
    /// </summary>
    [Browsable(false)]
    public IEnumerable<Notification> ChangeNotifications
    {
      get 
      {
       return m_ChangeNotifications; 
      }
    }


    #region IControllableInteractions Members

    
    [Category(CoreConsts.INTERACTIONS_CATEGORY),
     Description("Determines whether an item applies (can be interacted at all). Usually this property is acted upon  by an application business rules"),
     DefaultValue(true)]
    public bool Applicable
    {
      get
      {
        return m_Applicable;
      }
      set
      { 
        if (!Constructed)
        {
          m_Applicable = value;
          return;
        }
      
        if (m_Applicable!=value)
        {
          bool old = m_Applicable;
          
          OnInteractabilityChanging(new InteractabilityChangeEventArgs(InteractabilityType.Applicable, old, value)); 
        
          DisableBindings();
           m_Applicable = value;
           AddNotification(new InteractabilityChangeNotification(this));
          EnableBindings();

          OnInteractabilityChanged(new InteractabilityChangeEventArgs(InteractabilityType.Applicable, old, value)); 
        }
      }
    }

    [Category(CoreConsts.INTERACTIONS_CATEGORY),
     Description("Determines whether an item is shown to the user"),
     DefaultValue(true)]
    public bool Visible
    {
      get
      {
        return m_Visible;
      }
      set
      {
        if (!Constructed)
        {
          m_Visible = value;
          return;
        }
        
        if (m_Visible!=value)
        {
          bool old = m_Visible;
          
          OnInteractabilityChanging(new InteractabilityChangeEventArgs(InteractabilityType.Visible, old, value));
        
          DisableBindings(); 
           m_Visible = value;
           AddNotification(new InteractabilityChangeNotification(this));
          EnableBindings();

          OnInteractabilityChanged(new InteractabilityChangeEventArgs(InteractabilityType.Visible, old, value));
        }
      }
    }

    [Category(CoreConsts.INTERACTIONS_CATEGORY),
     Description("Determines whether an item is enabled for user actions"),
     DefaultValue(true)]
    public bool Enabled
    {
      get
      {
        return m_Enabled;
      }
      set
      {
        if (!Constructed)
        {
          m_Enabled = value;
          return;
        }
        
        if (m_Enabled!=value)
        {
         bool old = m_Enabled;

         OnInteractabilityChanging(new InteractabilityChangeEventArgs(InteractabilityType.Enabled, old, value));
         
         DisableBindings(); 
          m_Enabled = value;
          AddNotification(new InteractabilityChangeNotification(this));
         EnableBindings();

         OnInteractabilityChanged(new InteractabilityChangeEventArgs(InteractabilityType.Enabled, old, value));
        }
      }
    }

    [Category(CoreConsts.INTERACTIONS_CATEGORY),
     Description("Determines whether an item is available for data input"),
     DefaultValue(false)]
    public bool Readonly
    {
      get
      {
        return m_Readonly;
      }
      set
      {
        if (!Constructed)
        {
          m_Readonly = value;
          return;
        }
        
        if (m_Readonly!=value)
        {
          bool old = m_Readonly;

          OnInteractabilityChanging(new InteractabilityChangeEventArgs(InteractabilityType.Readonly, old, value));

          DisableBindings(); 
            m_Readonly = value;
            AddNotification(new InteractabilityChangeNotification(this));
          EnableBindings();

          OnInteractabilityChanged(new InteractabilityChangeEventArgs(InteractabilityType.Readonly, old, value));
        }
      }
    }

    [Browsable(false)]
    public bool IsApplicable
    {
      get
      { 
        if ((m_Owner==null) || !(m_Owner is ModelBase))
          return m_Applicable; 
        else  
          return m_Applicable && ((ModelBase)m_Owner).IsApplicable;
      }
    }

    [Browsable(false)]
    public bool IsVisible
    {
      get
      {
        if ((m_Owner == null) || !(m_Owner is ModelBase))
          return m_Visible;
        else
          return m_Visible && ((ModelBase)m_Owner).IsVisible;
      }
    }

    [Browsable(false)]
    public bool IsEnabled
    {
      get
      {
        if ((m_Owner == null) || !(m_Owner is ModelBase))
          return m_Enabled;
        else
        {
          ModelBase ownr = (ModelBase)m_Owner;
          return m_Enabled && ownr.IsEnabled && (State == DataState.Creating || State == DataState.Editing); 
        }  
      }
    }

    [Browsable(false)]
    public bool IsReadonly
    {
      get
      {
        if ((m_Owner == null) || !(m_Owner is ModelBase))
          return m_Readonly;
        else
          return m_Readonly || ((ModelBase)m_Owner).IsReadonly;
      }
    }


    /// <summary>
    /// Indicates whether this item was validated. "Validated" is set to false upon any change made to records.
    /// When field validation is deferred until record "Post()"
    /// validation on fields is not performed upon field change.
    /// Not to confuse with "Valid", a field may be validated but not valid because it contains bad value.
    /// </summary>
    [Browsable(false)]
    public bool Validated
    {
      get
      {
        return m_Validated;
      }
    }

    /// <summary>
    /// Indicates whether this item was validated and contains no errors.
    /// "Validated" is set to false upon any change made to records, so is "Valid".
    /// When field validation is deferred until record "Post()"
    /// validation on fields is not performed upon field change.
    /// Not to confuse with "Validated", a field must be validated to be valid.
    /// </summary>
    [Browsable(false)]
    public bool Valid
    {
      get
      {
        return m_Validated && m_ValidationException == null;
      }
    }

    /// <summary>
    /// Provides access to an error (if any) that happened during last field validation
    /// </summary>
    [Browsable(false)]
    public Exception ValidationException
    {
      get
      {
        return m_ValidationException;
      }
    }


    /// <summary>
    /// Indicates whether to throw an exception when model is in invalid state upon init
    /// </summary>
    [Browsable(false)]
    public bool ThrowOnInitValidation
    {
      get { return m_ThrowOnInitValidation;  }
      set { m_ThrowOnInitValidation = value; }
    }



    /// <summary>
    /// Indicates whether this item's validation is delayed until data is posted
    /// </summary>
    [Category(CoreConsts.VALIDATION_CATEGORY),
     Description("Indicates whether this item's validation is delayed until data is posted"),
     DefaultValue(false)]
    public bool DeferredValidation
    {
      get
      {
        return m_DeferredValidation;
      }
      set
      {
        m_DeferredValidation = value;
      }
    }


    #endregion    

   

    //#region ITags Members

    ///// <summary>
    ///// General-purpose tags
    ///// </summary>
    //[Browsable(false)]
    //public Dictionary<string, object> Tags
    //{
    //  get { return m_Tags; }
    //}

    //#endregion

    #endregion

    #region Events
      /// <summary>
      /// Invoked after security permission name changes
      /// </summary>
    [field: NonSerialized]
      public event EventHandler SecurityChanged;

    [field: NonSerialized]
      public event InteractabilityChangeEventHandler InteractabilityChanging;

    [field: NonSerialized]
      public event InteractabilityChangeEventHandler InteractabilityChanged;

    [field: NonSerialized]
      public event StateChangeEventHandler StateChanging;
    [field: NonSerialized]
      public event StateChangeEventHandler StateChanged;


    [field: NonSerialized]
      public event EventHandler Reverting;

    [field: NonSerialized]
      public event EventHandler Reverted;

    [field: NonSerialized]
      public event EventHandler ReleasedOwner;

      /// <summary>
      /// Invoked before validation starts. Exceptions are not caught
      /// </summary>
    [field: NonSerialized]
      public event EventHandler PreValidation;
      
      /// <summary>
      /// Invoked during validation. Any exception thrown is caught into ValidationException (and Valid property)
      /// </summary>
    [field: NonSerialized]
      public event EventHandler Validation;
      
      /// <summary>
      /// Invoked after all validation is done regardless of the result. Exceptions are not caught
      /// </summary>
    [field: NonSerialized] 
      public event EventHandler PostValidation;


     /// <summary>
      /// Invoked to get client validation script for specific technology
      /// </summary>
    [field: NonSerialized] 
      public event GetClientScriptsEventHandler GetClientScriptsEvent;
      
    #endregion

    #region Protected Event Dispatchers
     
      protected void OnSecurityChanged(EventArgs args)
      {
         if (SecurityChanged!=null) SecurityChanged(this, args);
      }

      protected void OnInteractabilityChanging(InteractabilityChangeEventArgs args)
      {
        if (InteractabilityChanging != null) InteractabilityChanging(this, args);
      }

      protected void OnInteractabilityChanged(InteractabilityChangeEventArgs args)
      {
        if (InteractabilityChanged != null) InteractabilityChanged(this, args);
      }

      protected void OnStateChanging(StateChangeEventArgs args)
      {
        if (StateChanging != null) StateChanging(this, args);
      }

      protected void OnStateChanged(StateChangeEventArgs args)
      {
        if (StateChanged != null) StateChanged(this, args);
      }

      protected void OnReverting(EventArgs args)
      {
        if (Reverting != null) Reverting(this, args);
      }

      protected void OnReverted(EventArgs args)
      {
        if (Reverted != null) Reverted(this, args);
      }

      protected void OnReleasedOwner(EventArgs args)
      {
        if (ReleasedOwner != null) ReleasedOwner(this, args);
      }

      protected void OnPreValidation(EventArgs args)
      {
        if (PreValidation != null) PreValidation(this, args);
      }

      protected void OnValidation(EventArgs args)
      {
        if (Validation != null) Validation(this, args);
      }

      protected void OnPostValidation(EventArgs args)
      {
        if (PostValidation != null) PostValidation(this, args);
      }
    
    #endregion 
     
     
    #region Pub Methods


     


    /// <summary>
    /// Initializes an instance for data loading and loads data if store is not null from external source.
    /// </summary>
    public void Load(IModelDataStore store, IDataStoreKey key, params object[] extra)
    {
      BeginLoad(store, key, extra);
      EndLoad();
    }
    
    
    
    /// <summary>
    /// Initializes an instance for data loading and loads data if store is not null from external source.
    /// This call has to be followed by EndLoad()
    /// </summary>
    public void BeginLoad(IModelDataStore store, IDataStoreKey key, params object[] extra)
    {
      CheckLoadAllowed();
      
      
      setState(DataState.Initializing);
      
      m_StorageOperation = StorageOperation.Loading;
      try
      {      
        DoCreate();
        
        if (store!=null)
          DoLoad(store, key, extra);
      }
      finally
      {
        m_StorageOperation = StorageOperation.None;
      }    
       
      m_DataStoreKey = key; 
    }

    /// <summary>
    /// Finalized data load operation from external source. This call has to be preceded by BeginLoad()
    /// </summary>
    public void EndLoad()
    {
      if (m_State != DataState.Initializing)
        throw new RecordModelException(StringConsts.NOT_LOADING_STATE_ERROR + " EndLoad()");

      Validate();//20100919
     
      if (!Valid)
        throw new ModelValidationException(StringConsts.NOT_VALID_ON_LOAD_ERROR, this);
            
      DoEndLoad();
        
      setState(DataState.Viewing);
      m_LastPostedChange = ChangeType.None;
    }

    /// <summary>
    /// Sets this instance to Uninitialized state when Load did not succeed. Call to abort failed loads
    /// </summary>
    public void CancelLoad()
    {
      if (m_State != DataState.Initializing)
        throw new RecordModelException(StringConsts.NOT_LOADING_STATE_ERROR + " CancelLoad()");

      DoCancelLoad();
     
      setState(DataState.Uninitialized);
      m_LastPostedChange = ChangeType.None;
    }


    
    private DataState statePriorToCreate;
    private IDataStoreKey keyPriorToCreate;
    
    /// <summary>
    /// Create a new instance of data, such as new record, or new field value
    /// </summary>
    public void Create()
    {
      if (State == DataState.Creating) return;
      
      CheckCreateAllowed();

      statePriorToCreate = m_State;
      setState(DataState.Initializing);
      
      keyPriorToCreate = m_DataStoreKey;
      m_DataStoreKey = null;
     
      CreatePreEditStateCopy();
      
      
      DisableBindings();
      try
      {
        DoCreate(); 
        AddNotification(new InteractabilityChangeNotification(this));
      }
      finally
      {
        setState(DataState.Creating);
        EnableBindings();
      }
    }

    /// <summary>
    /// Modified item's existing data
    /// </summary>
    public void Edit()
    {
      if (State == DataState.Editing) return;
      
      CheckEditAllowed();

      setState(DataState.Editing);
      
      CreatePreEditStateCopy();
      

      DisableBindings();
      try
      {
        DoEdit();
        AddNotification(new InteractabilityChangeNotification(this));
      }
      finally
      {
        EnableBindings();
      }
    }

    /// <summary>
    /// Performs validations and posts changes permanently
    /// </summary>
    public void Post()
    {
      if (State == DataState.Viewing) return;
    
      CheckPostAllowed();
      
      DisableBindings();
      try
      {
      
        Validate();
       
        if (!Valid)
          throw new ModelValidationException(StringConsts.NOT_VALID_ON_POST_ERROR, this);

              
        DoPost();

        DropPreEditStateCopy();
        
        AddNotification(new InteractabilityChangeNotification(this));
        
        
        m_LastPostedChange = (State==DataState.Creating) ? ChangeType.Created : ChangeType.Edited;
        
        setState(DataState.Viewing);
      }
      finally
      {  
        EnableBindings();
      }
    }

    /// <summary>
    /// Cancels creation/modification of data
    /// </summary>
    public void Cancel()
    {
      if (State == DataState.Viewing) return;
      
      CheckCancelAllowed();
    
      if ((State != DataState.Creating) && (State != DataState.Editing))
        throw new RecordModelException(StringConsts.NOT_CREATE_EDIT_STATE_ERROR + " Cancel()");


      DisableBindings();  
      try
      {  
        RevertToPreEditState();
        DropPreEditStateCopy();
        
        DoCancel();
        
        AddNotification(new InteractabilityChangeNotification(this));
        AddNotification(new DataChangeNotification(this));

        if (m_State == DataState.Creating)
        {
             setState(statePriorToCreate);
             m_DataStoreKey = keyPriorToCreate;
        }
        else 
             setState(DataState.Viewing);     
      }
      finally
      {  
        EnableBindings();
      }
    }

    /// <summary>
    /// Saves data into external store under this instance's DataStoreKey
    /// </summary>
    public void Save(IModelDataStore store)
    {
      Save(store, DataStoreKey);
    }
    
    
    /// <summary>
    /// Saves data into external store using specified key. If key is null then provider may elect to build key from fields with KeyField attribute set to true
    /// </summary>
    public void Save(IModelDataStore store, IDataStoreKey key, params object[] extra)
    {
      CheckSaveAllowed();
      
      m_StorageOperation = StorageOperation.Saving;
      try
      {
         DoSave(store, key ?? this.DataStoreKey, extra);
         m_DataStoreKey = key;
      }
      finally
      {
         m_StorageOperation = StorageOperation.None;
      }
      
    }

    /// <summary>
    /// Deletes data from external store under this instance's DataStoreKey.
    /// After this call model state is DataState.Uninitialized
    /// </summary>
    public void Delete(IModelDataStore store)
    {
      Delete(store, DataStoreKey);
    }


    /// <summary>
    /// Deletes data from external store with specified key. When key is omitted instance's DataStoreKey is used.
    /// After this call model state is DataState.Uninitialized
    /// </summary>
    public void Delete(IModelDataStore store, IDataStoreKey key, params object[] extra)
    {
      CheckDeleteAllowed();


      m_StorageOperation = StorageOperation.Deleting;
      try
      {
       DoDelete(store, key ?? this.DataStoreKey, extra);
      }
      finally
      {
        m_StorageOperation = StorageOperation.None;
      }
      
      m_DataStoreKey = null;
      setState(DataState.Uninitialized);
      m_LastPostedChange = ChangeType.Deleted;
    }



    /// <summary>
    /// Reverts data to the state it was in before editing/creation was performed. This method is used for modifications cancelation
    /// </summary>
    public void Revert()
    {
      if (State!=DataState.Creating  &&  State!=DataState.Editing)
        throw new RecordModelException(StringConsts.NOT_CREATE_EDIT_STATE_ERROR);
     
      OnReverting(EventArgs.Empty);
      
      RevertToPreEditState();
     
      DisableBindings();
       AddNotification(new DataChangeNotification(this));
      EnableBindings();

      OnReverted(EventArgs.Empty);
     
    }
   

    /// <summary>
    /// Disassociates an item with its current owner effectively rendering owning entity unaware of this item
    /// </summary>
    public void ReleaseOwner()
    {
      if (m_Owner!=null)
      {
        if (m_Owner is ModelBase)
         ((ModelBase)m_Owner).removeOwnedItem(this);
        m_Owner = null;
        
        OnReleasedOwner(EventArgs.Empty);
      }
    }

    /// <summary>
    /// Validates an item for correct data and sets "Validated" and "Valid" properties
    /// </summary>
    public void Validate()
    {
      OnPreValidation(EventArgs.Empty);
      
      bool couldNotValidate = false;
      
      m_Validated = false;
      m_ValidationException = null;
      
      try
      {
        PerformValidation(ref couldNotValidate);
        OnValidation(EventArgs.Empty);
      }
      catch(Exception error)
      {
        m_ValidationException = error;
      }
      
      m_Validated = !couldNotValidate;
      
      DisableBindings();
       AddNotification(new ValidationNotification(this));
      EnableBindings();

      OnPostValidation(EventArgs.Empty);
    }


    /// <summary>
    /// Stops any notifications broadcast to subscribed bindings from this and all owned instances
    /// </summary>
    public void DisableBindings()
    {
      if (!Constructed) return;
      if (!SupportsNotificationBinding) return;
     
      m_DisableBindingsLevel++;
      
      //disable all owned bindings
      if (m_OwnedItems!=null)
        foreach (ModelBase child in m_OwnedItems)
            child.DisableBindings();
    }
    
    /// <summary>
    /// Resumes all notifications broadcast to subscribed bindings from this and all owned instances
    /// </summary>
    public void EnableBindings()
    {
      if (!Constructed) return;
      if (!SupportsNotificationBinding) return;
      
      if (m_DisableBindingsLevel<=0) //it will never be <0
        throw new RecordModelException(string.Format(StringConsts.ENABLE_BINDINGS_ERROR, GetType().ToString()));
     
      
      m_DisableBindingsLevel--;
      
      if (m_DisableBindingsLevel==0)
      {
       //send all notifications to bindings from this
       try
       {
         if (m_Bindings!=null)
           foreach(IBinding binding in m_Bindings)
           {
             foreach(Notification notification in m_ChangeNotifications)
               binding.Notify(notification);
               
             binding.NotificationsFinished(); 
           } 
       }
       finally
       {
         //add this notifications to children, THUS
         //making ALL owned items receive parents notification
         if (m_OwnedItems!=null)
          foreach (ModelBase child in m_OwnedItems)
            child.m_ChangeNotifications.MergeNotifications(m_ChangeNotifications);
           
         m_ChangeNotifications.ClearNotifications();
       }  
       
      }//if level==0

      //enable all owned bindings (sending their notifications)
      if (m_OwnedItems!=null)
       foreach (ModelBase child in m_OwnedItems)
         child.EnableBindings();
    }
    
    
    /// <summary>
    /// Attaches event handlers or re-attaches event handlers back after model was resurrected from non-volatile store
    /// </summary>
    public void AttachEventHandlers()
    {
      AttachEvents();
    }

    /// <summary>
    /// Invokes model's public method which is decorated with callable attribute
    /// </summary>
    /// <param name="name">Method name</param>
    /// <param name="parameters">Parameters</param>
    public void InvokeCallableMethod(string name, object[] parameters)
    {
      var mi = GetType().GetMethod(name);
      if (mi == null)
        throw new RecordModelException(string.Format(StringConsts.MODEL_METHOD_NOT_FOUND_ERROR, name));

      if (!mi.IsPublic)
        throw new RecordModelException(string.Format(StringConsts.MODEL_METHOD_NOT_FOUND_ERROR, name));

      var attrs = mi.GetCustomAttributes(typeof(CallableMethodAttribute), false);
      
      if (attrs.Length<1)
        throw new RecordModelException(string.Format(StringConsts.MODEL_METHOD_NOT_FOUND_ERROR, name));
      
      
      try
      {
       if (parameters==null) parameters = new object[0];
       mi.Invoke(this, parameters);
      }
      catch(Exception error)
      {
        throw new RecordModelException(StringConsts.MODEL_METHOD_ERROR + error.Message, error);
      } 
    }


    /// <summary>
    /// Returns client scripts, such as JavaScript for particular client-implementation technology.
    /// For example, client frameworks/technologies of a different kind may connect to the same record model.
    /// This implementation probes event first, and if it is set then invokes it, otherwise it uses ClientScript attribute to return its text if it is set, 
    /// otherwise script text from resource is returned. 
    /// Override to provide client script programmatically.
    /// </summary>
    /// <param name="technology">The name of client-side technology that requests client script, i.e. 'nfx'</param>
    public virtual ClientScripts GetClientScripts(string technology)
    {
      var scripts = OnGetClientScriptsEvent(technology);
      if (scripts!=null) return scripts;
      
      var tp = GetType();
      return ClientScriptAttribute.GetScripts(tp, 
                                              null, 
                                              tp.GetCustomAttributes(typeof(ClientScriptAttribute), false)
                                                .Cast<ClientScriptAttribute>(),
                                              technology);
    }



    #endregion
  
    #region ISupportInitialize Members

     public virtual void BeginInit()
     {
        setState(DataState.Initializing);
     }

     public virtual void EndInit()
     {
       if (m_State != DataState.Initializing)
        throw new RecordModelException(StringConsts.NOT_LOADING_STATE_ERROR + " EndInit()");

        Validate();
      
        
        
        if (m_ThrowOnInitValidation)
          if (!Valid)
            throw new ModelValidationException(StringConsts.NOT_VALID_ON_LOAD_ERROR, this);
      
            
       setState(DataState.Viewing);
       m_LastPostedChange = ChangeType.None;
     }

    #endregion

     #region IDeserializationCallback Members

       public void OnDeserialization(object sender)
       {
         m_Bindings = new Bindings();
         AttachEventHandlers();
       }

     #endregion



    
    #region Protected
     
     [NonSerialized]
     internal Bindings m_Bindings = new Bindings();
    
     internal ChangeNotifications m_ChangeNotifications = new ChangeNotifications();

     protected IDataStoreKey m_DataStoreKey;



     protected internal void AddNotification(Notification notification)
     {
         if (SupportsNotificationBinding)
             this.m_ChangeNotifications.AddNotification(notification);
     }


     /// <summary>
     /// Sets model properties from definition attributes 
     /// </summary>
     protected internal virtual void ApplyDefinitionAttributes(IEnumerable<object> attributes)
     {
     }
     
     
     /// <summary>
     /// Override to attach/re-attach events after model gets resurrected from storage
     /// </summary>
     protected virtual void AttachEvents()
     {
     
     }


     /// <summary>
     /// Creates a copy of items data state. This is needed for record editing
     ///  so Cancel method may revert to saved copy that was made before any edits were performed
     /// </summary>
     protected internal virtual void CreatePreEditStateCopy()
     {
       //nothing to create at this level
     }
     
     /// <summary>
     /// Deletes a copy of data as it is not needed any more after successful post
     /// </summary>
     protected internal virtual void DropPreEditStateCopy()
     {
       //nothing to drop at this level
     }
     
     /// <summary>
     /// Reverts this instance to pre edit state by returning to private state values
     /// </summary>
     protected internal virtual void RevertToPreEditState()
     {
       m_ValidationException = null;
       m_Validated = false;
     }


     /// <summary>
     /// Checks and throws an exception if Create() operation is not allowed
     /// </summary>
     protected virtual void CheckCreateAllowed()
     {
        if (State!=DataState.Viewing && State!=DataState.Uninitialized)
          throw new RecordModelException(string.Format(StringConsts.MODEL_OPERATION_NOT_ALLOWED_ERROR, State, "Create"));
     }
     
     /// <summary>
     /// Override to implement functionality performed upon Create() 
     /// </summary>
     protected internal virtual void DoCreate()
     {
     
     }

     /// <summary>
     /// Checks and throws an exception if Edit() operation is not allowed
     /// </summary>
     protected virtual void CheckEditAllowed()
     {
       if (State != DataState.Viewing)
         throw new RecordModelException(string.Format(StringConsts.MODEL_OPERATION_NOT_ALLOWED_ERROR, State, "Edit"));
     }


     /// <summary>
     /// Override to implement functionality performed upon Edit() 
     /// </summary>
     protected internal virtual void DoEdit()
     {

     }

     /// <summary>
     /// Checks and throws an exception if Post() operation is not allowed
     /// </summary>
     protected virtual void CheckPostAllowed()
     {
       if (State != DataState.Creating && State != DataState.Editing)
         throw new RecordModelException(string.Format(StringConsts.MODEL_OPERATION_NOT_ALLOWED_ERROR, State, "Post"));
     }


     /// <summary>
     /// Override to implement functionality performed upon Post() 
     /// </summary>
     protected internal virtual void DoPost()
     {

     }


     /// <summary>
     /// Checks and throws an exception if Cancel() operation is not allowed
     /// </summary>
     protected virtual void CheckCancelAllowed()
     {
       if (State != DataState.Creating && State != DataState.Editing)
         throw new RecordModelException(string.Format(StringConsts.MODEL_OPERATION_NOT_ALLOWED_ERROR, State, "Cancel"));
     }
     
     /// <summary>
     /// Override to implement functionality performed upon Cancel() 
     /// </summary>
     protected internal virtual void DoCancel()
     {
     
     }

     /// <summary>
     /// Checks and throws an exception if BeginLoad() operation is not allowed
     /// </summary>
     protected virtual void CheckLoadAllowed()
     {
       if ((m_State != DataState.Uninitialized) && (m_State != DataState.Viewing))
         throw new RecordModelException(StringConsts.NOT_UNINIT_VIEW_STATE_ERROR + " BeginLoad()");
         
       if (StorageOperation != StorageOperation.None)
         throw new RecordModelException(StringConsts.STORAGE_OPERATION_IN_PROGRESS_ERROR);  
     }

     /// <summary>
     /// Override to implement functionality performed upon BeginLoad() 
     /// </summary>
     protected internal virtual void DoLoad(IModelDataStore store, IDataStoreKey key, params object[] extra)
     {
       store.Load(this, key, extra);
     }

     /// <summary>
     /// Override to implement functionality performed upon EndLoad() after data has been loaded AND validated
     /// </summary>
     protected internal virtual void DoEndLoad()
     {
     }

     /// <summary>
     /// Override to implement functionality performed upon CancelLoad() after load has not succeeded
     /// </summary>
     protected internal virtual void DoCancelLoad()
     {
     }


     /// <summary>
     /// Checks and throws an exception if Save() operation is not allowed
     /// </summary>
     protected virtual void CheckSaveAllowed()
     {
       if (State != DataState.Viewing)
         throw new RecordModelException(StringConsts.NOT_APPROPRIATE_STATE_FOR_SAVE_ERROR); 
       
       if (StorageOperation != StorageOperation.None)
         throw new RecordModelException(StringConsts.STORAGE_OPERATION_IN_PROGRESS_ERROR);
     }

     /// <summary>
     /// Override to implement functionality performed upon Save() 
     /// </summary>
     protected virtual void DoSave(IModelDataStore store, IDataStoreKey key, params object[] extra)
     {
        store.Save(this, key, extra);
     }



     /// <summary>
     /// Checks and throws an exception if Save() operation is not allowed
     /// </summary>
     protected virtual void CheckDeleteAllowed()
     {
       if (State != DataState.Viewing)
         throw new RecordModelException(StringConsts.NOT_APPROPRIATE_STATE_FOR_DELETE_ERROR);
         
       if (StorageOperation != StorageOperation.None)
         throw new RecordModelException(StringConsts.STORAGE_OPERATION_IN_PROGRESS_ERROR);  
     }

     /// <summary>
     /// Override to implement functionality performed upon Delete() 
     /// </summary>
     protected virtual void DoDelete(IModelDataStore store, IDataStoreKey key, params object[] extra)
     {
       store.Delete(this, key, extra);
     }
     
     
     /// <summary>
     /// Performs item validation, throwing exception when data is not valid
     /// </summary>
     /// <param name="couldNotValidate">
     /// Indicates that validation could not be performed. This may happen when
     /// an item is being validated prematurely, i.e. when an item's validation
     /// needs to cross-check its value with some other item that has not yet been 
     /// entered. This value is ignored on final validations caused by record posts
     /// as items must be validated then
     /// </param>
     protected abstract void PerformValidation(ref bool couldNotValidate);
     
     
     /// <summary>
     /// Called before owned item is added to owner collection. Override this method
     /// to perform checks before item actually added, such as: ensure unique item identification among siblings, etc.
     /// Throw exception to prohibit addition
     /// </summary>
     protected virtual void BeforeOwnedItemAddition(ModelBase addingItem)
     {
      //base implementation does nothing
      //does not require unneeded implementation in derived classes as it will be blank most of the time
     }
     
     /// <summary>
     /// Called after an item was added to owning object. Exceptions are handled
     /// </summary>
     protected virtual void AfterOwnedItemAddition(ModelBase addedItem)
     {
       //base implementation does nothing
       //does not require unneeded implementation in derived classes as it will be blank most of the time
     }
     
     /// <summary>
     /// Called before an item is about to be removed from owning context.
     /// Throw exception to prohibit deletion
     /// </summary>
     protected virtual void BeforeOwnedItemRemoval(ModelBase removingItem)
     {
       //base implementation does nothing
       //does not require unneeded implementation in derived classes as it will be blank most of the time
     }
     
     /// <summary>
     /// Called after an item was removed from owning context. Exceptions are handled
     /// </summary>
     protected virtual void AfterOwnedItemRemoval(ModelBase removedItem)
     {
       //base implementation does nothing
       //does not require unneeded implementation in derived classes as it will be blank most of the time
     }

     /// <summary>
     /// Invokes event
     /// </summary>
     protected ClientScripts OnGetClientScriptsEvent(string technology)
     {
       if (GetClientScriptsEvent!=null)
         return GetClientScriptsEvent(this, technology);

       return null;
     }

    
    #endregion
    
    #region Private Utils
    
     private void setState(DataState newState)
     {
       if (m_State!=newState)
       {
         DataState old = m_State;
        
         OnStateChanging(new StateChangeEventArgs(old, newState));
        
          m_State = newState;
        
         OnStateChanged(new StateChangeEventArgs(old, newState));
       }
     }
    
    
     private void addOwnedItem(ModelBase anItem)
     {
       //20130228 Dkh Lazy create
       if (m_OwnedItems==null)
          m_OwnedItems = new List<ModelBase>(32);//average field count per record - experimentaly this uses less ram while allocating 1000000+ records

       //check for dbl-registration
       this._CheckForDuplicateOwnedItemRegistration(anItem);
       
       //we expect exceptions (when applicable) that may "spoil" addition of owned items
       // i.e. can't add item with duplicate logical name etc.. this is implemented in derived classes
       BeforeOwnedItemAddition(anItem); 
       m_OwnedItems.Add(anItem);
       try { AfterOwnedItemAddition(anItem); } catch {}
     }
    
     protected virtual void _CheckForDuplicateOwnedItemRegistration(ModelBase anItem)
     {
        if (m_OwnedItems.Contains(anItem, ReferenceEqualityComparer<ModelBase>.Instance))
          throw new RecordModelException(StringConsts.ITEM_ALREADY_EXISTS_ERROR);
     }


     private void removeOwnedItem(ModelBase anItem)
     {
       if (m_OwnedItems == null) return;

       if (m_OwnedItems.Contains(anItem))
       {
         BeforeOwnedItemRemoval(anItem);
         m_OwnedItems.Remove(anItem);
         try { AfterOwnedItemRemoval(anItem); } catch {}
       }
     }


     private void dropPreEditStateCopy()
     {
       DropPreEditStateCopy();

       if (m_OwnedItems!=null)
        foreach (ModelBase child in m_OwnedItems)
          child.dropPreEditStateCopy();
     }
     
     
     
     internal void RegisterBinding(IBinding binding)
     {
       if (m_Bindings!=null)
         m_Bindings.RegisterBinding(binding);
     }

     internal void UnRegisterBinding(IBinding binding)
     {
       if (m_Bindings != null)
         m_Bindings.UnRegisterBinding(binding);
     }
      
    #endregion

    
  }


  
  
}
