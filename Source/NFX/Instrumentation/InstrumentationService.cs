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
using System.Collections.Concurrent;
using System.Linq;
using System.Text;
using System.Threading;

using NFX.Environment;
using NFX.ServiceModel;
using NFX.ApplicationModel;
using NFX.Log;

namespace NFX.Instrumentation
{
    /// <summary>
    /// Implements IInstrumentation. This service aggregates data by type,source and sends result into provider
    /// </summary>
    [ConfigMacroContext]
    public sealed class InstrumentationService : ServiceWithInstrumentationBase<object>, IInstrumentationImplementation
    {
        #region CONSTS
            private const int MIN_INTERVAL_MSEC = 500;
            private const int DEFAULT_INTERVAL_MSEC = 7000;

            private const string THREAD_NAME = "InstrumentationSvc Thread";


            public const string CONFIG_PROVIDER_SECTION = "provider";


            public const int DEFAULT_MAX_REC_COUNT = 1 * 1024 * 1024;
            public const int MINIMUM_MAX_REC_COUNT = 1024;
            public const int MAXIMUM_MAX_REC_COUNT = 256 * 1024 * 1024;

            public const int DEFAULT_RESULT_BUFFER_SIZE = 128 * 1024;
            public const int MAX_RESULT_BUFFER_SIZE = 2 * 1024 * 1024;// 250 msg * 12/min = 3,000/min * 60 min = 180,000/hr * 12 hrs = 2,160,000

        #endregion

        #region .ctor

            /// <summary>
            /// Creates a instrumentation service instance
            /// </summary>
            public InstrumentationService() : base(null)
            {
            }

            /// <summary>
            /// Creates a instrumentation service instance
            /// </summary>
            public InstrumentationService(object director) : base(director)
            {
            }


        #endregion


        #region Private Fields

            private int m_ProcessingIntervalMS = DEFAULT_INTERVAL_MSEC;

            private int m_OSInstrumentationIntervalMS;

            private bool m_SelfInstrumented;

            private Thread m_Thread;

            private InstrumentationProvider m_Provider;

            private AutoResetEvent m_Trigger = new AutoResetEvent(false);

            private TypeBucketedData m_TypeBucketed;

            private int m_RecordCount;

            private int m_MaxRecordCount = DEFAULT_MAX_REC_COUNT;

            private Datum[] m_ResultBuffer;
            private int m_ResultBufferIndex = 0;
            private int m_ResultBufferSize = DEFAULT_RESULT_BUFFER_SIZE;

        #endregion

        #region Properties

            public override string ComponentCommonName { get { return "instr"; }}

            public bool Enabled
            {
                get { return true; }
            }

            /// <summary>
            /// Returns true to indicate that instrumentation does not have any space left to record more data
            /// </summary>
            public bool Overflown
            {
                get { return m_RecordCount>m_MaxRecordCount;}
            }


            /// <summary>
            /// References provider that persists instrumentation data
            /// </summary>
            public InstrumentationProvider Provider
            {
              get { return m_Provider; }
              set
              {
                ensureInactive("InstrumentationService.Provider.set()");

                m_Provider = value;
              }
            }

            /// <summary>
            /// Specifies how often aggregation is performed
            /// </summary>
            [Config("$processing-interval-ms|$interval-ms", DEFAULT_INTERVAL_MSEC)]
            [ExternalParameter]
            public int ProcessingIntervalMS
            {
              get { return m_ProcessingIntervalMS; }
              set
              {
                if (value < MIN_INTERVAL_MSEC) value = MIN_INTERVAL_MSEC;
                m_ProcessingIntervalMS = value;
              }
            }

            /// <summary>
            /// Specifies how often OS instrumentation such as CPU and RAM is sampled.
            /// Value of zero disables OS sampling
            /// </summary>
            [Config("$os-interval-ms|$os-instrumentation-interval-ms")]
            [ExternalParameter]
            public int OSInstrumentationIntervalMS
            {
              get { return m_OSInstrumentationIntervalMS; }
              set
              {
                if (value < 0) value = 0;
                m_OSInstrumentationIntervalMS = value;
              }
            }

            /// <summary>
            /// When true, outputs instrumentation data about the self (how many datum buffers, etc.)
            /// </summary>
            [Config("$self-instrumented|$instrument-self|$instrumented", false)]
            public bool SelfInstrumented
            {
              get { return m_SelfInstrumented; }
              set { m_SelfInstrumented = value;}
            }

            /// <summary>
            /// Shortcut to SelfInstrumented, implements IInstrumentable
            /// </summary>
            [Config(Default=false)]
            [ExternalParameter]
            public override bool InstrumentationEnabled
            {
              get { return this.SelfInstrumented;}
              set { SelfInstrumented = value;}
            }

            /// <summary>
            /// Returns current record count in the instance
            /// </summary>
            public int RecordCount { get{ return m_RecordCount;}}

            /// <summary>
            /// Gets/Sets the maximum record count that this instance can store
            /// </summary>
            [Config(null, DEFAULT_MAX_REC_COUNT)]
            [ExternalParameter]
            public int MaxRecordCount
            {
              get{ return m_MaxRecordCount;}
              set
              {
                if (value<MINIMUM_MAX_REC_COUNT) value = MINIMUM_MAX_REC_COUNT;
                else
                if (value>MAXIMUM_MAX_REC_COUNT) value = MAXIMUM_MAX_REC_COUNT;

                m_MaxRecordCount = value;
              }
            }

            /// <summary>
            /// Returns the size of the ring buffer where result (aggregated) instrumentation records are kept in memory.
            /// The maximum buffer capacity is returned, not how many results have been buffered so far.
            ///  If this property is less than or equal to zero then result buffering in memory is disabled.
            ///  This property can be set only on a stopped service
            /// </summary>
            [Config(null, DEFAULT_RESULT_BUFFER_SIZE)]
            public int ResultBufferSize
            {
              get { return m_ResultBufferSize;}
              set
              {
                CheckServiceInactive();
                if (value>MAX_RESULT_BUFFER_SIZE) value = MAX_RESULT_BUFFER_SIZE;
                m_ResultBufferSize = value;
              }
            }

            /// <summary>
            /// Enumerates distinct types of Datum ever recorded in the instance. This property may be used to build
            ///  UIs for instrumentation, i.e. datum type tree. Returned data is NOT ORDERED
            /// </summary>
            public IEnumerable<Type> DataTypes
            {
              get
              {
                var bucketed = m_TypeBucketed;
                if (bucketed == null) return Enumerable.Empty<Type>();
                return bucketed.Keys.ToArray();
              }
            }

        #endregion


        #region Public

            /// <summary>
            /// Records instrumentation datum
            /// </summary>
            public void Record(Datum datum)
            {
              if (Status != ControlStatus.Active) return;
              if (datum==null) return;
              if (Overflown) return;

              var t = datum.GetType();

              var srcBucketed = m_TypeBucketed.GetOrAdd(t, (tp) => new SrcBucketedData());

              if (srcBucketed.DefaultDatum==null)
               srcBucketed.DefaultDatum = datum;

              var bag = srcBucketed.GetOrAdd(datum.Source, (src) => new DatumBag());

              bag.Add(datum);
              Interlocked.Increment(ref m_RecordCount);
            }

            /// <summary>
            /// Returns the specified number of samples from the ring result buffer in the near-chronological order,
            /// meaning that data is already sorted by time MOST of the TIME, however sorting is NOT GUARANTEED for all
            ///  result records returned as enumeration is a lazy procedure that does not make copies/take locks.
            /// The enumeration is empty if ResultBufferSize is less or equal to zero entries.
            /// If count is less or equal to zero then the system returns all results available.
            /// </summary>
            public IEnumerable<Datum> GetBufferedResults(int count=0)
            {
              var data = m_ResultBuffer;//thread-safe copy
              if (data==null) yield break;

              var curIdx = m_ResultBufferIndex;
              if (curIdx>=data.Length) curIdx = 0;

              var idx = curIdx + 1;
              if (count>0)
              {
                if (count>m_ResultBufferSize) count = m_ResultBufferSize;

                if (curIdx>=count)
                  idx = curIdx - count;
                else
                  idx = (data.Length-1) - ((count-1) - curIdx);
              }
              //else dump all


              if (idx>=curIdx)//capture the tail first
              {
                for(;idx<data.Length;idx++)
                {
                   var datum = data[idx];
                   if (datum!=null) yield return datum;
                }
                idx = 0;
              }

              for(;idx<curIdx;idx++)
              {
                 var datum = data[idx];
                 if (datum!=null) yield return datum;
              }
            }

            /// <summary>
            /// Returns samples starting around the the specified UTCdate in the near-chronological order,
            /// meaning that data is already sorted by time MOST of the TIME, however sorting is NOT GUARANTEED for all
            ///  result records returned as enumeration is a lazy procedure that does not make copies/take locks.
            /// The enumeration is empty if ResultBufferSize is less or equal to zero entries
            /// </summary>
            public IEnumerable<Datum> GetBufferedResultsSince(DateTime utcStart)
            {
              const int COARSE_SEC_SAMPLES = 50;
              const int FINE_SEC_SAMPLES = 10;

              var data = m_ResultBuffer;//thread-safe copy
              if (data==null) yield break;

              var curIdx = m_ResultBufferIndex;
              if (curIdx>=data.Length) curIdx = 0;

              var taking = false;
              var idx = curIdx;
              //capture the tail first
              for(int pass=0;pass<2;pass++)//0=tail, 1=head, 2...exit
              {
                while(idx< (pass==0 ? data.Length : curIdx))
                {
                  var datum = data[idx];
                  if (datum==null)
                  {
                    idx += taking ? 1 : COARSE_SEC_SAMPLES;
                    continue;
                  }

                  if (taking)
                  {
                    if (datum.UTCTime >= utcStart)
                     yield return datum;
                    idx++;
                    continue;
                  }

                  if (datum.UTCTime>=utcStart)
                  {
                    taking = true;
                    continue;
                  }

                  var span = (int)(utcStart - datum.UTCTime).TotalSeconds;//can not be negative because of prior if
                  var offset = span<5 ? 1 : span<10 ? FINE_SEC_SAMPLES : span * COARSE_SEC_SAMPLES;
                  idx += offset;
                }//while
                idx=0;
              }//for

            }


            /// <summary>
            /// Enumerates sources per Datum type ever recorded by the instance. This property may be used to build
            ///  UIs for instrumentation, i.e. datum type tree. Returned data is NOT ORDERED
            /// </summary>
            public IEnumerable<string> GetDatumTypeSources(Type datumType, out Datum defaultInstance)
            {
               var tBucketed = m_TypeBucketed;
               if (datumType!=null && tBucketed!=null)
               {
                 SrcBucketedData srcBucketed = null;
                 if (tBucketed.TryGetValue(datumType, out srcBucketed))
                 {
                   defaultInstance = srcBucketed.DefaultDatum;
                   return srcBucketed.Keys.ToArray();
                 }
               }
               defaultInstance = null;
               return Enumerable.Empty<string>();
            }

        #endregion


        #region Protected


            protected override void DoConfigure(NFX.Environment.IConfigSectionNode node)
            {
              try
              {
                base.DoConfigure(node);

                m_Provider = FactoryUtils.MakeAndConfigure(node[CONFIG_PROVIDER_SECTION]) as InstrumentationProvider;

                if (m_Provider == null)
                  throw new NFXException("Provider is null");

                m_Provider.__setComponentDirector(this);
              }
              catch (Exception error)
              {
                throw new NFXException(StringConsts.INSTRUMENTATIONSVC_PROVIDER_CONFIG_ERROR + error.Message, error);
              }
            }



            protected override void DoStart()
            {
              log(MessageType.Info, "Entering DoStart()", null);

              try
              {

                //pre-flight checks
                if (m_Provider == null)
                  throw new NFXException(StringConsts.SERVICE_INVALID_STATE + "InstrumentationService.DoStart(Provider=null)");


                m_BufferOldestDatumUTC = null;

                m_TypeBucketed = new TypeBucketedData();

                m_Provider.Start();

                m_ResultBufferIndex = 0;
                if (m_ResultBufferSize>0)
                  m_ResultBuffer = new Datum[m_ResultBufferSize];


                m_Thread = new Thread(threadSpin);
                m_Thread.Name = THREAD_NAME;
                m_Thread.IsBackground = false;

                m_Thread.Start();
              }
              catch (Exception error)
              {
                AbortStart();

                if (m_Thread != null)
                {
                  try {m_Thread.Join();} catch{}
                  m_Thread = null;
                }

                m_ResultBuffer = null;

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

                m_TypeBucketed = null;
                m_ResultBuffer = null;
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
                            Topic = CoreConsts.INSTRUMENTATIONSVC_TOPIC,
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
                            if (m_SelfInstrumented)
                             instrumentSelf();

                            write();

                            if (m_OSInstrumentationIntervalMS <= 0)
                              m_Trigger.WaitOne(m_ProcessingIntervalMS);
                            else
                              instrumentOS();
                          }//while

                          write();
                     }
                     catch(Exception e)
                     {
                         log(MessageType.Emergency, " threadSpin() leaked exception", e.Message);
                     }

                     log(MessageType.Info, "Exiting threadSpin()", null);
                }

                //adds data that described this very instance
                private void instrumentSelf()
                {
                   var rc = this.RecordCount;
                   Self.RecordCount.Record( rc );
                   Self.RecordLoad.Record( (int)Math.Round( 100d *  (rc / (double)m_MaxRecordCount)) );//cant be 0
                   Self.ProcessingInterval.Record( m_ProcessingIntervalMS );


                   var buf = m_ResultBuffer;
                   if (buf!=null)
                   {
                     if (m_BufferOldestDatumUTC.HasValue)
                     {
                       var ageSec = (int)(App.TimeSource.UTCNow - m_BufferOldestDatumUTC.Value).TotalSeconds;
                       Self.BufferMaxAge.Record( ageSec );
                     }
                   }
                }


                // Pauses up to m_ProcessingIntervalMS
                private void instrumentOS()
                {
                  const int MIN_SAMPLING_RATE_MS = 500;

                  var samplingRate = m_OSInstrumentationIntervalMS;
                  if (samplingRate < MIN_SAMPLING_RATE_MS) samplingRate = MIN_SAMPLING_RATE_MS;
                  if (samplingRate > m_ProcessingIntervalMS) samplingRate = m_ProcessingIntervalMS;
                  var remainder = m_ProcessingIntervalMS % samplingRate;

                  var count = m_ProcessingIntervalMS / samplingRate;
                  for (int i = 0; i < count; i++)
                  {
                    NFX.OS.Instrumentation.CPUUsage.Record(NFX.OS.Computer.CurrentProcessorUsagePct);
                    NFX.OS.Instrumentation.RAMUsage.Record(NFX.OS.Computer.GetMemoryStatus().LoadPct);
                    NFX.OS.Instrumentation.AvailableRAM.Record(NFX.OS.Computer.CurrentAvailableMemoryMb);
                    m_Trigger.WaitOne(samplingRate);
                  }

                  if (remainder > 20) m_Trigger.WaitOne(remainder);
                }


                private void write()
                {
                  foreach(var tvp in m_TypeBucketed)
                  {
                    if (!App.Available) break;
                    foreach(var svp in tvp.Value)
                    {
                      if (!App.Available) break;

                      var bag = svp.Value;
                      Datum datum = null;
                      if (bag.TryPeek(out datum))
                      {
                        Datum aggregated = null;

                        try
                        {
                           var lst = new List<Datum>();
                           Datum elm;
                           while(bag.TryTake(out elm))
                           {
                             lst.Add(elm);
                             Interlocked.Decrement(ref m_RecordCount);
                           }

                           aggregated = datum.Aggregate(lst);
                        }
                        catch(Exception error)
                        {
                          var et = error.ToMessageWithType();
                          log(MessageType.Error, string.Format("{0}.Aggregate(IEnumerable<Datum>) leaked {1}",
                                                                datum.GetType().FullName,
                                                                et), et);
                        }

                        try
                        {
                          if (aggregated!=null)
                            m_Provider.Write(aggregated);
                        }
                        catch(Exception error)
                        {
                          var et = error.ToMessageWithType();
                          log(MessageType.CatastrophicError, string.Format("{0}.Write(datum) leaked {1}",
                                                                m_Provider.GetType().FullName,
                                                                et), et);
                        }

                        if (aggregated!=null) bufferResult(aggregated);

                      }//if
                    }
                  }
                }


                private DateTime? m_BufferOldestDatumUTC;

                //revise
                private void bufferResult(Datum result)
                {
                  var data = m_ResultBuffer;
                  if (data==null || result==null) return;

                  var idx = Interlocked.Increment(ref m_ResultBufferIndex);

                  while(idx>=data.Length)//while needed in case VolatileWrite is not properly implemented on platform and does not write-through cache right away
                  {
                    Thread.VolatileWrite(ref m_ResultBufferIndex, -1);
                    idx = Interlocked.Increment(ref m_ResultBufferIndex);
                  }

                  if (m_BufferOldestDatumUTC.HasValue)
                  {
                    var existing = data[idx];
                    if (existing!=null)
                      m_BufferOldestDatumUTC = existing.UTCTime;
                  }
                  else
                   m_BufferOldestDatumUTC = result.UTCTime;

                  data[idx] = result;
                }

        #endregion



    }



    /// <summary>
    /// Internal concurrent dictionary used for instrumentation data aggregation
    /// </summary>
    internal class TypeBucketedData : ConcurrentDictionary<Type, SrcBucketedData> {}


    /// <summary>
    /// Internal concurrent dictionary used for instrumentation data aggregation
    /// </summary>
    internal class SrcBucketedData : ConcurrentDictionary<string, DatumBag>
    {
      internal Datum DefaultDatum;
    }


    /// <summary>
    /// Internal concurrent bag used for instrumentation data aggregation
    /// </summary>
    internal class DatumBag : ConcurrentBag<Datum> {}
}
