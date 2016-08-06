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
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;

namespace NFX.ServiceModel
{

         /// <summary>
         /// Defines how intervals should be handled. Sliding means that every time message is posted into in queue
         ///  interval starts to count from scratch, periodic counts from the first message post
         /// </summary>
         public enum WorkItemAggregationIntervalKind { Sliding, Periodic }

         /// <summary>
         /// Represents an item that may be posted into WorkItemAggregatorService queue
         /// </summary>
         public interface IAggregatableWorkItem
         {
           object AggregationKey { get; }
           WorkItemAggregationIntervalKind AggregationIntervalKind { get; }
         }




  /// <summary>
  /// Aggregates same/equal (as defined by item's IAggregatableWorkItem.AggregationKey) work items posted into queue to limit
  ///  the penetration of duplicate work items in destination queue
  /// </summary>
  public class WorkItemAggregatorService<TContext> : Service, IWorkQueue<TContext> where TContext : class
  {





     #region CONSTS

      private const string THREAD_NAME = "WorkItemAggregatorService.Thread for ";
      private const int THREAD_GRANULARITY_MSEC = 10;

      private const int DEFAULT_POST_INTERVAL_MSEC = 1000;
      private const int MIN_POST_INTERVAL_MSEC = 20;
      private const int MAX_POST_INTERVAL_MSEC = 60*60*1000;
     #endregion


     #region .ctor/.dctor
       public WorkItemAggregatorService(IWorkQueue<TContext> destination) : base(null)
       {
          m_Queue = new WorkQueue<TContext>(destination.Context);
          m_DestinationQueue = destination;
       }

       protected override void Destructor()
       {
         base.Destructor();
       }
     #endregion

     #region Private Fields
       private WorkQueue<TContext> m_Queue;
       private IWorkQueue<TContext> m_DestinationQueue;
       private Thread m_Thread;

       private int m_DestinationPostInterval = DEFAULT_POST_INTERVAL_MSEC;
     #endregion

     #region Properties
       /// <summary>
       /// Returns the destination queue
       /// </summary>
       public IWorkQueue<TContext> DestinationQueue
       {
         get { return m_DestinationQueue; }
       }

       /// <summary>
       /// Defines an interval measured in milliseconds for posting into destination queue
       /// </summary>
       public int DestinationPostInterval
       {
         get { return m_DestinationPostInterval; }

         set
         {
           if (value<MIN_POST_INTERVAL_MSEC) value = MIN_POST_INTERVAL_MSEC;
           else
            if (value>MAX_POST_INTERVAL_MSEC) value = MAX_POST_INTERVAL_MSEC;

           m_DestinationPostInterval = value;
         }
       }


     #endregion

     #region IWorkQueue<TContext> Members

       /// <summary>
       /// Posts work item in the service. This item may not be posted into destination if the same item was already recently posted
       ///  into destination queue, however if no same work item will come within timeout, then this item will eventually post
       /// </summary>
       public void PostItem(IWorkItem<TContext> work)
       {
         if (work==null) return;
         if (Status != ControlStatus.Active) return; //alas message lost
         if (! (work is IAggregatableWorkItem))
          throw new NFXException(StringConsts.WORK_ITEM_NOT_AGGREGATABLE_ERROR);

         m_Queue.PostItem(work);
       }

       public long ProcessedSuccessCount
       {
         get { return m_Queue.ProcessedSuccessCount; }
       }

       public long ProcessedFailureCount
       {
         get { return m_Queue.ProcessedFailureCount; }
       }

       public TContext Context
       {
         get { return m_Queue.Context; }
       }

     #endregion


     #region Protected
        protected override void DoStart()
        {
          base.DoStart();
          m_Thread = new Thread(threadSpin);
          m_Thread.Name = THREAD_NAME + Name;
          m_Thread.Start();
        }

        protected override void DoSignalStop()
        {
          base.DoSignalStop();
        }

        protected override void DoWaitForCompleteStop()
        {
          base.DoWaitForCompleteStop();
          m_Thread.Join();
          m_Thread = null;
        }

     #endregion


     #region .pvt

        private class workEntry
        {
           public IWorkItem<TContext> Work;
           public DateTime Created;
        }


        private void threadSpin()
        {
          var registry = new Dictionary<object, workEntry>();

          while (Running)
          {
            var wasWork = false;

            for(int cnt=0; cnt<100 && Running; cnt++)
            {//read everything up to limit from in queue into dictionary
              var item = m_Queue.FetchDueItem();
              if (item==null) break;

              wasWork = true;

              var aitem = (IAggregatableWorkItem)item;
              var key = aitem.AggregationKey;
              var kind = aitem.AggregationIntervalKind;

              workEntry entry;
              if (!registry.TryGetValue(key, out entry))
               registry.Add(key, new workEntry{ Work = item, Created = App.LocalizedTime});
              else
              {
               if (kind== WorkItemAggregationIntervalKind.Sliding)  entry.Created = App.LocalizedTime;
              }
            }

            var now = App.LocalizedTime;
            var regs = registry.ToList();
            foreach(var reg in regs)
            {
             if ((now - reg.Value.Created).TotalMilliseconds > m_DestinationPostInterval)
             {
               m_DestinationQueue.PostItem(reg.Value.Work); //repost into destination queue
               registry.Remove(reg.Key);//and remove from here
             }
            }

            if (!wasWork)
             Thread.Sleep(THREAD_GRANULARITY_MSEC);
          }//while

        }

     #endregion

  }
}
