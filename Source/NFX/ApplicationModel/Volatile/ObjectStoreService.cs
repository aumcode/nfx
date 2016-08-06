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
 * Originated: 2006.01
 * Revision: NFX 1.0  2011.02.03
 */
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Diagnostics;

using NFX.ServiceModel;
using NFX.Log;
using NFX.Environment;

namespace NFX.ApplicationModel.Volatile
{

  /// <summary>
  /// Implements service that stores object in proccess's memory, asynchronously saving objects to external non-volatile storage
  /// upon change and synchronously saving objects upon service stop. This service is useful for scenarious like ASP.NET
  /// volatile domain that can be torn down at any time.
  /// Note for ASP.NET uses: the key difference of this approach from .NET session state management is the fact that this service never blocks
  ///  object CheckIn() operations as backing store is being updated asynchronously.
  /// This class is thread-safe unless specified otherwise on a property/method level
  /// </summary>
  [ConfigMacroContext]
  public class ObjectStoreService : ServiceWithInstrumentationBase<object>, IObjectStoreImplementation
  {
    #region CONSTS

        public const int DEFAULT_OBJECT_LEFESPAN_MS = 1 * //hr;
                                                      60 * //min
                                                      60 * //sec
                                                      1000; //msec

        public const int MIN_OBJECT_LIFESPAN_MS = 1000;

        public const string CONFIG_GUID_ATTR = "guid";

        public const string CONFIG_BUCKET_COUNT_ATTR = "bucket-count";
        public const int DEFAULT_BUCKET_COUNT = 1024;
        public const int MAX_BUCKET_COUNT = 0xffff;

        public const string CONFIG_PROVIDER_SECT = "provider";
        public const string CONFIG_OBJECT_LIFE_SPAN_MS_ATTR = "object-life-span-ms";

        public const int MUST_ACQUIRE_INTERVAL_MS = 3000;
    #endregion


    #region .ctor

        /// <summary>
        /// Creates instance of the store service
        /// </summary>
        public ObjectStoreService() : this(new Guid())
        {
        }


        /// <summary>
        /// Creates instance of the store service with the state identified by "storeGUID". Refer to "StoreGUID" property.
        /// </summary>
        public ObjectStoreService(Guid storeGUID) : base(null)
        {
          m_StoreGUID = storeGUID;
        }

    #endregion


    #region Private Fields

        private int m_ObjectLifeSpanMS = DEFAULT_OBJECT_LEFESPAN_MS;


        private Guid m_StoreGUID;

        private ObjectStoreProvider m_Provider;


        private Thread m_Thread;
        private AutoResetEvent m_Trigger = new AutoResetEvent(false);

        private int m_BucketCount = DEFAULT_BUCKET_COUNT;
        private List<Bucket> m_Buckets;

        private bool m_InstrumentationEnabled;

    #endregion


    #region Properties

        public override string ComponentCommonName { get { return "objstore"; }}


        /// <summary>
        /// Returns unique identifier that identifies this particular store.
        /// This ID is used to load store's state from external medium upon start-up.
        /// One may think of this ID as of a "pointer/handle" that survives phisical object destroy/create cycle
        /// </summary>
        public Guid StoreGUID
        {
          get { return m_StoreGUID; }

          set
          {
            ensureInactive("ObjectStoreService.StoreGUID.set()");
            m_StoreGUID = value;
          }
        }

        /// <summary>
        /// Implements IInstrumentable
        /// </summary>
        [Config(Default=false)]
        [ExternalParameter(CoreConsts.EXT_PARAM_GROUP_OBJSTORE, CoreConsts.EXT_PARAM_GROUP_INSTRUMENTATION)]
        public override bool InstrumentationEnabled
        {
          get { return m_InstrumentationEnabled;}
          set { m_InstrumentationEnabled = value;}
        }

        /// <summary>
        /// Specifies how many buckets objects are kept in. Increasing this number improves thread concurrency
        /// </summary>
        public int BucketCount
        {
          get { return m_BucketCount;}
          set
          {
            ensureInactive("ObjectStoreService.BucketCount.set()");
            if (value<1) value = 1;
            if (value>MAX_BUCKET_COUNT) value = MAX_BUCKET_COUNT;
            m_BucketCount = value;
          }
        }


        /// <summary>
        /// Specifies how long objects live without being touched before becoming evicted from the list
        /// </summary>
        [ExternalParameter(CoreConsts.EXT_PARAM_GROUP_OBJSTORE)]
        public int ObjectLifeSpanMS
        {
          get { return m_ObjectLifeSpanMS; }
          set
          {
            if (value < MIN_OBJECT_LIFESPAN_MS) value = MIN_OBJECT_LIFESPAN_MS;
            m_ObjectLifeSpanMS = value;
          }
        }

        /// <summary>
        /// References provider that persists objects
        /// </summary>
        public ObjectStoreProvider Provider
        {
          get { return m_Provider; }
          set
          {
            ensureInactive("ObjectStoreService.Provider.set()");

            m_Provider = value;
          }
        }

    #endregion


    #region Public



    /// <summary>
    /// Retrieves an object reference from the store identified by the "key" or returns null if such object does not exist.
    /// Object is not going to be persisted as this method provides logical read-only access. If touch=true then object timestamp is updated
    /// </summary>
    public object Fetch(Guid key, bool touch = false)
    {
      if (this.Status != ControlStatus.Active) return null;

      var bucket = getBucket(key);

      ObjectStoreEntry entry = null;

      lock (bucket)
        if (!bucket.TryGetValue(key, out entry)) return null;

      lock (entry)
      {
        if (entry.Status == ObjectStoreEntryStatus.Deleted) return null;
        if (touch)
           entry.LastTime = App.LocalizedTime;
        return entry.Value;
      }
    }



    /// <summary>
    /// Retrieves an object reference from the store identified by the "key" or returns null if such object does not exist.
    /// Object is not going to be persisted until it is checked back in the store using the same number of calls to CheckIn() for the same GUID.
    /// </summary>
    public object CheckOut(Guid key)
    {
      if (this.Status != ControlStatus.Active) return null;

      var bucket = getBucket(key);

      ObjectStoreEntry entry = null;

      lock (bucket)
        if (!bucket.TryGetValue(key, out entry)) return null;

      lock (entry)
      {
        if (entry.Status == ObjectStoreEntryStatus.Deleted) return null;
        entry.Status = ObjectStoreEntryStatus.CheckedOut;
        entry.CheckoutCount++;
        entry.LastTime = App.LocalizedTime;
        return entry.Value;
      }
    }

    /// <summary>
    /// Reverts object state to Normal after the call to Checkout. This way the changes (if any) are not going to be persisted.
    /// Returns true if object was found and checkout canceled. Keep in mind: this method CAN NOT revert inner object state
    ///  to its original state if it was changed, it only unmarks object as changed.
    /// This method is reentrant just like the Checkout is
    /// </summary>
    public bool UndoCheckout(Guid key)
    {
      if (this.Status != ControlStatus.Active) return false;

      var bucket = getBucket(key);

      ObjectStoreEntry entry = null;

      lock (bucket)
        if (!bucket.TryGetValue(key, out entry)) return false;

      lock (entry)
      {
        if (entry.Status == ObjectStoreEntryStatus.Deleted) return false;

        if (entry.CheckoutCount>0)
           entry.CheckoutCount--;

        if (entry.CheckoutCount==0)
          entry.Status = ObjectStoreEntryStatus.Normal;
        return true;
      }
    }


    /// <summary>
    /// Puts an object reference "value" into store identified by the "key".
    /// The object is written in the provider when call count to this method equals to CheckOut()
    /// </summary>
    public void CheckIn(Guid key, object value, int msTimeout = 0)
    {
      if (Status != ControlStatus.Active) return;

      if (value==null)
      {
        Delete(key);
        return;
      }

      var bucket = getBucket(key);

      ObjectStoreEntry entry = null;
      bool isnew = false;

      lock (bucket)
      {
        if (!bucket.TryGetValue(key, out entry))
        {
          isnew = true;
          entry = new ObjectStoreEntry();
          entry.Key = key;
          entry.Status = ObjectStoreEntryStatus.ChekedIn;
          entry.LastTime = App.LocalizedTime;
          entry.MsTimeout = msTimeout;
          entry.Value = value;

          bucket.Add(key, entry);
        }
      }

      if (!isnew)
        lock (entry)
        {
          if (entry.Status == ObjectStoreEntryStatus.Deleted) return;

          if (entry.CheckoutCount>0)
           entry.CheckoutCount--;

          if (entry.CheckoutCount==0)
            entry.Status = ObjectStoreEntryStatus.ChekedIn;

          entry.LastTime = App.LocalizedTime;
          entry.MsTimeout = msTimeout;
          entry.Value = value;
        }

    }

    /// <summary>
    /// Puts an object reference "value" into store identified by the "oldKey" under the "newKey".
    /// If oldKey was not checked in, then checks-in under new key as normally would
    /// </summary>
    public void CheckInUnderNewKey(Guid oldKey, Guid newKey, object value, int msTimeout = 0)
    {
      if (Status != ControlStatus.Active) return;

      if (value==null)
      {
        Delete(oldKey);
        return;
      }

      var bucket = getBucket(oldKey);

      ObjectStoreEntry entry = null;

      lock (bucket)
        if (!bucket.TryGetValue(oldKey, out entry)) entry = null;

      if (entry!=null)
        lock (entry)
        {
          entry.Value = null;
          entry.Status = ObjectStoreEntryStatus.Deleted;
          entry.LastTime = App.LocalizedTime;
        }

      CheckIn(newKey, value, msTimeout);
    }


    /// <summary>
    /// Puts an object back into store identified by the "key".
    /// The object is written in the provider when call count to this method equals to CheckOut().
    /// Returns true if object with such id exists and was checked-in
    /// </summary>
    public bool CheckIn(Guid key, int msTimeout = 0)
    {
      if (Status != ControlStatus.Active) return false;

      var bucket = getBucket(key);

      ObjectStoreEntry entry = null;

      lock (bucket)
        if (!bucket.TryGetValue(key, out entry))
          return false;

        lock (entry)
        {
          if (entry.Status == ObjectStoreEntryStatus.Deleted) return false;

          if (entry.CheckoutCount>0)
          {
           entry.CheckoutCount--;
           if (entry.CheckoutCount==0)
            entry.Status = ObjectStoreEntryStatus.ChekedIn;

          }
          entry.LastTime = App.LocalizedTime;
          entry.MsTimeout = msTimeout;
        }

      return true;
    }


    /// <summary>
    /// Deletes object identified by key. Returns true when object was found and marked for deletion
    /// </summary>
    public bool Delete(Guid key)
    {
      if (Status != ControlStatus.Active) return false;


      var bucket = getBucket(key);


      ObjectStoreEntry entry = null;

      lock (bucket)
        if (!bucket.TryGetValue(key, out entry)) return false;

      lock (entry)
      {
        if (entry.Status == ObjectStoreEntryStatus.Deleted) return false;
        entry.Status = ObjectStoreEntryStatus.Deleted;
        entry.LastTime = App.LocalizedTime;
      }


      return true;
    }

    #endregion


    #region Protected

        protected override void DoConfigure(NFX.Environment.IConfigSectionNode node)
        {
          try
          {
            base.DoConfigure(node);

            var sguid = node.AttrByName(CONFIG_GUID_ATTR).ValueAsString();
            if (!string.IsNullOrEmpty(sguid))
              StoreGUID = new Guid(sguid);


            m_Provider = FactoryUtils.MakeAndConfigure(node[CONFIG_PROVIDER_SECT]) as ObjectStoreProvider;

            if (m_Provider == null)
              throw new NFXException("Provider is null");

            m_Provider.__setComponentDirector(this);

            ObjectLifeSpanMS = node.AttrByName(CONFIG_OBJECT_LIFE_SPAN_MS_ATTR).ValueAsInt(DEFAULT_OBJECT_LEFESPAN_MS);

            BucketCount = node.AttrByName(CONFIG_BUCKET_COUNT_ATTR).ValueAsInt(DEFAULT_BUCKET_COUNT);

          }
          catch (Exception error)
          {
            throw new NFXException(StringConsts.OBJSTORESVC_PROVIDER_CONFIG_ERROR + error.Message, error);
          }
        }


    protected override void DoStart()
    {
      log(MessageType.Info, "Entering DoStart()", null);

      try
      {

        //pre-flight checks
        if (m_Provider == null)
          throw new NFXException(StringConsts.SERVICE_INVALID_STATE + "ObjectStoreService.DoStart(Provider=null)");

        m_Provider.Start();

        m_Buckets = new List<Bucket>(m_BucketCount);
        for(var i=0;i<m_BucketCount; i++)
         m_Buckets.Add(new Bucket());

        base.DoStart();

        var clock = Stopwatch.StartNew();
        var now = App.LocalizedTime;
        var all = m_Provider.LoadAll();

        log(MessageType.Info, "Prepared object list to load in " + clock.Elapsed, null);

        var cnt = 0;
        foreach (var entry in all)
        {
          entry.Status = ObjectStoreEntryStatus.Normal;
          entry.LastTime = now;

          var bucket = getBucket(entry.Key);
          bucket.Add(entry.Key, entry);

          cnt++;
        }
        log(MessageType.Info, string.Format("DoStart() has loaded {0} objects in {1} ", cnt, clock.Elapsed ), null);


        m_Thread = new Thread(threadSpin);
        m_Thread.Name = "ObjectStoreService Thread";
        m_Thread.IsBackground = false;

        m_Thread.Start();
      }
      catch (Exception error)
      {
        AbortStart();

        if (m_Thread != null)
        {
          m_Thread.Join();
          m_Thread = null;
        }

        log(MessageType.CatastrophicError, "DoStart() exception: " + error.Message, null);
        throw error;
      }

      log(MessageType.Info, "Exiting DoStart()", null);
    }

    protected override void DoSignalStop()
    {
      log(MessageType.Info, "Entering DoSignalStop()", null);

      try
      {
        base.DoSignalStop();

        m_Trigger.Set();

        //m_Provider should not be touched here
      }
      catch (Exception error)
      {
        log(MessageType.CatastrophicError, "DoSignalStop() exception: " + error.Message, null);
        throw error;
      }

      log(MessageType.Info, "Exiting DoSignalStop()", null);
    }

    protected override void DoWaitForCompleteStop()
    {
      log(MessageType.Info, "Entering DoWaitForCompleteStop()", null);

      try
      {
        base.DoWaitForCompleteStop();

        m_Thread.Join();
        m_Thread = null;

        m_Provider.WaitForCompleteStop();

        m_Buckets = null;
      }
      catch (Exception error)
      {
        log(MessageType.CatastrophicError, "DoWaitForCompleteStop() exception: " + error.Message, null);
        throw error;
      }

      log(MessageType.Info, "Exiting DoWaitForCompleteStop()", null);
    }


    #endregion


    #region .pvt. impl.


                private Bucket getBucket(Guid key)
                {
                  var idx = (key.GetHashCode() & CoreConsts.ABS_HASH_MASK) % m_BucketCount;
                  return m_Buckets[idx];
                }


                private void ensureInactive(string msg)
                {
                  if (Status != ControlStatus.Inactive)
                          throw new NFXException(StringConsts.SERVICE_INVALID_STATE + msg);
                }


                private void log(MessageType type, string message, string parameters)
                {
                  App.Log.Write(
                          new Log.Message
                          {
                            Text = message ?? string.Empty,
                            Type = type,
                            From = this.Name,
                            Topic = CoreConsts.OBJSTORESVC_TOPIC,
                            Parameters = parameters ?? string.Empty
                          }
                        );
                }


        private void threadSpin()
        {
             try
             {
                  while (Running)
                  {
                    visit(false);   //todo need to make sure that this visit() never leaks anything otherwise thread may crash the whole service
                    m_Trigger.WaitOne(2000);
                  }//while

                  visit(true);
             }
             catch(Exception e)
             {
                 log(MessageType.Emergency, " threadSpin() leaked exception", e.Message);
             }

             log(MessageType.Info, "Exiting threadSpin()", null);
        }


        public void visit(bool stopping)
        {
          var now = App.LocalizedTime;
          for(var i=0; i< m_BucketCount; i++)
          {
            var bucket = m_Buckets[i];
            if (stopping || bucket.LastAcquire.AddMilliseconds(MUST_ACQUIRE_INTERVAL_MS)<now)
                  lock(bucket)
                    write(bucket);
            else
                  if (Monitor.TryEnter(bucket))
                  try
                  {
                    write(bucket);
                  }
                  finally
                  {
                    Monitor.Exit(bucket);
                  }
          }//for
        }


        private void write(Bucket bucket)  //bucket is locked already
        {
          var now = App.LocalizedTime;

          var removed = new Lazy<List<Guid>>(false);

          foreach (var pair in bucket)
          {
            var entry = pair.Value;

            lock(entry)
            {
                //evict expired object and delete evicted or marked for deletion
                if (
                    (
                     (entry.Status == ObjectStoreEntryStatus.Normal  ||
                      entry.Status == ObjectStoreEntryStatus.ChekedIn     //checked-in but not written yet
                     )
                      &&
                     entry.LastTime.AddMilliseconds( (entry.MsTimeout > 0) ? entry.MsTimeout : m_ObjectLifeSpanMS ) < now
                    ) ||
                    (entry.Status == ObjectStoreEntryStatus.Deleted)
                   )
                {
                  var wasWritten = entry.Status == ObjectStoreEntryStatus.Normal || entry.Status == ObjectStoreEntryStatus.Deleted;
                  entry.Status = ObjectStoreEntryStatus.Deleted;//needed for Normal objects that have just expired

                  removed.Value.Add(entry.Key);

                  if (wasWritten)
                  { //delete form disk only if it was already written(normal)
                    try
                    {
                      m_Provider.Delete(entry);
                    }
                    catch (Exception error)
                    {
                      log(MessageType.CatastrophicError, "Provider error in .Delete(entry): " + error.Message, null);
                    }
                  }

                  try
                  {
                    if (entry.Value!=null && entry.Value is IDisposable)
                      ((IDisposable)entry.Value).Dispose();
                  }
                  catch (Exception error)
                  {
                    log(MessageType.Error, "Exception from evicted object IDisposable.Dispose(): " + error.Message, null);
                  }

                  continue;
                }

                if (entry.Status == ObjectStoreEntryStatus.ChekedIn)
                {

                  try
                  {
                    m_Provider.Write(entry);
                  }
                  catch (Exception error)
                  {
                    log(MessageType.CatastrophicError, "Provider error in .Write(entry): " + error.Message, null);
                  }


                  entry.Status = ObjectStoreEntryStatus.Normal;
                }

            }//lock entry
          }//for


          if (removed.IsValueCreated)
          {
            foreach(var key in removed.Value)
             bucket.Remove(key);
            log(MessageType.Info, string.Format("Removed {0} objects", removed.Value.Count), null);
          }


          bucket.LastAcquire = App.LocalizedTime;
        }



    #endregion

  }


}
