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
using System.Linq;
using System.Text;
using System.Threading;

namespace NFX.ServiceModel
{

  /// <summary>
  /// Defines contract for work queue that work items can be posted to
  /// </summary>
  public interface IWorkQueue<TContext> where TContext : class
  {
     /// <summary>
     /// Posts work item into the queue in natural queue order (at the end of the queue)
     /// </summary>
     void PostItem(IWorkItem<TContext> work);

     long ProcessedSuccessCount{get;}
     long ProcessedFailureCount{get;}

     TContext Context { get; }
  }


  /// <summary>
  /// Maintains a queue of pending work - every WorkItem posting in the queue will be executed by the host of the queue.
  /// WorkQueues are useful for coordination of complex data/event flows in multi-threaded/service applications
  /// </summary>
  public class WorkQueue<TContext> : IWorkQueue<TContext> where TContext : class
  {

     /// <summary>
     /// Creates an instance of work queue in particular context
     /// </summary>
     public WorkQueue(TContext context)
     {
       m_Context = context;
     }


     /// <summary>
     /// Creates an instance of work queue in particular context
     ///  with specific woprk item post filter
     /// </summary>
     public WorkQueue(TContext context, PostItemFilter<TContext> filter)
     {
       m_Context = context;
       m_Filter = filter;
     }


     private TContext m_Context;
     private PostItemFilter<TContext> m_Filter;
     private WorkItemList<TContext> m_WorkList = new WorkItemList<TContext>();
     private long m_ProcessedSuccessCount;
     private long m_ProcessedFailureCount;



     /// <summary>
     /// Returns context that work is processed in
     /// </summary>
     public TContext Context
     {
       get { return m_Context; }
     }


     /// <summary>
     /// Returns pending number of work items
     /// </summary>
     public int PendingCount
     {
       get
       {
         lock(m_WorkList)
          return m_WorkList.Count;
       }
     }

     /// <summary>
     /// Returns total number of work items processed without errors by this queue since its creation
     /// </summary>
     public long ProcessedSuccessCount
     {
       get
       {
          return Interlocked.Read(ref m_ProcessedSuccessCount);
       }
     }

     /// <summary>
     /// Returns total number of work items processed with errors by this queue since its creation
     /// </summary>
     public long ProcessedFailureCount
     {
       get
       {
          return Interlocked.Read(ref m_ProcessedFailureCount);
       }
     }

     /// <summary>
     /// Returns total number of work items processed with or without errors by this queue since its creation
     /// </summary>
     public long ProcessedTotalCount
     {
       get
       {
          return ProcessedSuccessCount + ProcessedFailureCount;
       }
     }



     /// <summary>
     /// Posts work item into the queue in natural queue order (at the end of the queue)
     /// </summary>
     public void PostItem(IWorkItem<TContext> work)
     {
      if (m_Filter!=null)
       work = m_Filter(work);

      if (work!=null)
        lock(m_WorkList)
          m_WorkList.AddLast(work);
     }


     /// <summary>
     /// Takes due item off the queue without executing it and returns it, or returns null when queue is empty.
     /// </summary>
     /// <returns></returns>
     public IWorkItem<TContext> FetchDueItem()
     {
       IWorkItem<TContext> work;
       lock(m_WorkList)
       {
         var head = m_WorkList.First;
         if (head==null) return null;//nothing left to do
         work = head.Value;
         m_WorkList.RemoveFirst();
       }

       return work;
     }


     /// <summary>
     /// Processes item in normal queue order (the item that is due to be processed). Returns true when there was an item in the queue.
     /// This method does not leak exceptions from work performance unless they are re-thrown by particular work item WorkFailed(error)
     /// </summary>
     public bool ProcessDueItem()
     {
       var work = FetchDueItem();

       if (work==null) return false;//nothing left to do


       var workPerformed = false;
       try
       {
         work.PerformWork(m_Context);

         workPerformed = true;

         work.WorkSucceeded();

         Interlocked.Increment(ref m_ProcessedSuccessCount);
       }
       catch(Exception error)
       {
         Interlocked.Increment(ref m_ProcessedFailureCount);
         work.WorkFailed(workPerformed, error);
       }

       return true;
     }

  }


  /// <summary>
  /// A filter delegate that gets called within PostItem before adding work to this queue.
  /// This is useful for re-routing work to some other queue/s when needed.
  /// Return null if work item is going to be processed by some other queue that this delegate should post into.
  /// Keep in mind that this delegate is invoked by posters thread
  /// </summary>
  public delegate IWorkItem<TContext> PostItemFilter<TContext>(IWorkItem<TContext> work) where TContext : class;


  internal class WorkItemList<TContext> : LinkedList<IWorkItem<TContext>> where TContext : class
  {}

}
