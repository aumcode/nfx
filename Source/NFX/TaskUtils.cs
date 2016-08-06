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
 * Revision: NFX 1.0  2011.01.31
 */
using System;
using System.Threading.Tasks;

namespace NFX
{
  public static class TaskUtils
  {
    /// <summary>
    /// Chains task 'first' with 'next' if first is completed, not cancelled and not faulted.
    /// Returns task that completes when 'next' completes
    /// </summary>
    public static Task OnOk(this Task first, Action next, TaskContinuationOptions options = TaskContinuationOptions.ExecuteSynchronously)
    {
      var tcs = new TaskCompletionSource<object>();

      first.ContinueWith(_ =>
      {
        if (first.IsCanceled)
        {
          tcs.TrySetCanceled();
        }
        else if (first.IsFaulted)
        {
          tcs.TrySetException(first.Exception.InnerExceptions);
        }
        else
        {
          try
          {
            next();
            tcs.TrySetResult(null);
          }
          catch (Exception ex)
          {
            tcs.TrySetException(ex);
          }
        }
      }, options);

      return tcs.Task;
    }

    /// <summary>
    /// Chains task 'first' with 'next' passing result of 'first' to 'next' if first is completed, not cancelled and not faulted.
    /// Returns task that completes when 'next' completes
    /// </summary>
    public static Task OnOk<T1>(this Task<T1> first, Action<T1> next, TaskContinuationOptions options = TaskContinuationOptions.ExecuteSynchronously)
    {
      var tcs = new TaskCompletionSource<object>();

      first.ContinueWith(_ =>
      {
          if (first.IsFaulted)
        {
          tcs.TrySetException(first.Exception.InnerExceptions);
        }
        else if (first.IsCanceled)
        {
          tcs.TrySetCanceled();
        }
        else
        {
          try
          {
            next(first.Result);
            tcs.TrySetResult(null);
          }
          catch (Exception ex)
          {
            tcs.TrySetException(ex);
          }
        }
      }, options);

      return tcs.Task;
    }

    /// <summary>
    /// Chains task 'first' with task returned by 'next' passing result of 'first' to 'next' if first is completed, not cancelled and not faulted.
    /// Returns task that completes after task returned by 'next' completes
    /// </summary>
    public static Task OnOk<T1>(this Task<T1> first, Func<T1, Task> next,
                                TaskContinuationOptions firstOptions = TaskContinuationOptions.ExecuteSynchronously,
                                TaskContinuationOptions nextOptions = TaskContinuationOptions.ExecuteSynchronously)
    {
      var tcs = new TaskCompletionSource<object>();

      first.ContinueWith(_ =>
      {
        if (first.IsFaulted)
          tcs.TrySetException(first.Exception.InnerExceptions);
        else if (first.IsCanceled)
          tcs.TrySetCanceled();
        else
        {
          try
          {
            var t = next(first.Result);
            if (t == null)
              tcs.TrySetException( new NFXException(StringConsts.CANNOT_RETURN_NULL_ERROR + typeof(TaskUtils).FullName + ".Then" ));
            else
            {
              t.ContinueWith(__ =>
              {
                if (t.IsFaulted)
                  tcs.TrySetException(first.Exception.InnerExceptions);
                else if (t.IsCanceled)
                  tcs.TrySetCanceled();
                else
                  tcs.TrySetResult(null);
              }, nextOptions);
            }
          }
          catch (Exception ex)
          {
            tcs.TrySetException(ex);
          }
        }
      }, firstOptions);

      return tcs.Task;
    }

    /// <summary>
    /// Chains task 'first' with task returned by 'next' if first is completed, not cancelled and not faulted.
    /// Returns task that completes after task returned by 'next' completes with result from 'next' task
    /// </summary>
    public static Task<T1> OnOk<T1>(this Task first, Func<Task<T1>> next,
                                TaskContinuationOptions firstOptions = TaskContinuationOptions.ExecuteSynchronously,
                                TaskContinuationOptions nextOptions = TaskContinuationOptions.ExecuteSynchronously)
    {
      var tcs = new TaskCompletionSource<T1>();

      first.ContinueWith(_ =>
      {
        if (first.IsFaulted)
          tcs.TrySetException(first.Exception.InnerExceptions);
        else if (first.IsCanceled)
          tcs.TrySetCanceled();
        else
        {
          try
          {
            var t = next();
            if (t == null)
              tcs.TrySetException( new NFXException(StringConsts.CANNOT_RETURN_NULL_ERROR + typeof(TaskUtils).FullName + ".Then" ));
            else
            {
              t.ContinueWith(__ =>
              {
                if (t.IsFaulted)
                  tcs.TrySetException(first.Exception.InnerExceptions);
                else if (t.IsCanceled)
                  tcs.TrySetCanceled();
                else
                  tcs.TrySetResult(t.Result);
              }, nextOptions);
            }
          }
          catch (Exception ex)
          {
            tcs.TrySetException(ex);
          }
        }
      }, firstOptions);

      return tcs.Task;
    }

    /// <summary>
    /// Chains task 'first' with task returned by 'next' passing result of 'first' to 'next' if first is completed, not cancelled and not faulted.
    /// Returns task that completes after task returned by 'next' completes with result from 'next' task
    /// </summary>
    public static Task<T2> OnOk<T1, T2>(this Task<T1> first, Func<T1, Task<T2>> next,
                                        TaskContinuationOptions firstOptions = TaskContinuationOptions.ExecuteSynchronously,
                                        TaskContinuationOptions nextOptions = TaskContinuationOptions.ExecuteSynchronously)
    {
      var tcs = new TaskCompletionSource<T2>();

      first.ContinueWith( _ =>
      {
        if (first.IsFaulted)
          tcs.TrySetException(first.Exception.InnerExceptions);
        else if (first.IsCanceled)
          tcs.TrySetCanceled();
        else
        {
          try
          {
            var t = next(first.Result);
            if (t == null)
              tcs.TrySetException( new NFXException(StringConsts.CANNOT_RETURN_NULL_ERROR + typeof(TaskUtils).FullName + ".Then" ));
            else
            {
              t.ContinueWith( __ =>
              {
                if (t.IsFaulted)
                  tcs.TrySetException(first.Exception.InnerExceptions);
                else if (t.IsCanceled)
                  tcs.TrySetCanceled();
                else
                  tcs.TrySetResult(t.Result);
              }, nextOptions);
            }
          }
          catch (Exception ex)
          {
            tcs.TrySetException(ex);
          }
        }
      }, firstOptions);

      return tcs.Task;
    }

    /// <summary>
    /// Registers action executed if task was faulted or cancelled
    /// </summary>
    public static Task OnError(this Task task, Action handler)
    {
      var tcs = new TaskCompletionSource<object>();

      task.ContinueWith(_ => {

        if (task.IsFaulted || task.IsCanceled)
        {
          try
          {
            handler();
          }
          catch (Exception ex)
          {
            tcs.TrySetException(ex);
            return;
          }
        }

        if (task.IsFaulted)
          tcs.TrySetException(task.Exception.InnerExceptions);
        else if (task.IsCanceled)
          tcs.TrySetCanceled();
        else
          tcs.TrySetResult(null);

      }, TaskContinuationOptions.ExecuteSynchronously);

      return tcs.Task;
    }

    /// <summary>
    /// Registers action executed disregarding task state
    /// </summary>
    public static Task OnOkOrError(this Task task,  Action<Task> handler)
    {
      var tcs = new TaskCompletionSource<object>();

      task.ContinueWith(_ => {

        try
        {
          handler(task);
        }
        catch (Exception ex)
        {
          tcs.TrySetException(ex);
          return;
        }

        if (task.IsFaulted)
          tcs.TrySetException(task.Exception.InnerExceptions);
        else if (task.IsCanceled)
          tcs.TrySetCanceled();
        else
          tcs.TrySetResult(null);

      }, TaskContinuationOptions.ExecuteSynchronously);

      return tcs.Task;
    }

    /// <summary>
    /// Registers action executed disregarding task state
    /// </summary>
    public static Task<T> OnOkOrError<T>(this Task<T> task, Action<Task> handler)
    {
      var tcs = new TaskCompletionSource<T>();

      task.ContinueWith(_ => {

        try
        {
          handler(task);
        }
        catch (Exception ex)
        {
          tcs.TrySetException(ex);
          return;
        }

        if (task.IsFaulted)
          tcs.TrySetException(task.Exception.InnerExceptions);
        else if (task.IsCanceled)
          tcs.TrySetCanceled();
        else
          tcs.TrySetResult(task.Result);

      }, TaskContinuationOptions.ExecuteSynchronously);

      return tcs.Task;
    }

    /// <summary>
    /// Non-generic version of <see cref="AsCompletedTask{T}(Func{T})"/>
    /// </summary>
    /// <remarks>
    /// Because there is no non-generic <see cref="System.Threading.Tasks.TaskCompletionSource{T}"/> version
    /// generic version typed by <see cref="System.Object"/> is used (<see cref="System.Threading.Tasks.Task{T}"/> inherits from <see cref="System.Threading.Tasks.Task"/>)
    /// </remarks>
    public static Task AsCompletedTask(this Action act)
    {
      return AsCompletedTask<object>(() =>
      {
        act();
        return null;
      }
      );
    }

    /// <summary>
    /// Returns task completed from a synchronous functor
    /// </summary>
    public static Task<T> AsCompletedTask<T>(this Func<T> func)
    {
      var tcs = new TaskCompletionSource<T>();
      try
      {
        tcs.SetResult(func());
      }
      catch (Exception ex)
      {
        tcs.SetException(ex);
      }

      return tcs.Task;
    }
  }
}
