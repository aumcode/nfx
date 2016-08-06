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

namespace NFX.ServiceModel
{

  /// <summary>
  /// Defines a base for items executable by WorkQueue
  /// </summary>
  public interface IWorkItem<TContext> where TContext : class
  {
    /// <summary>
    /// Invoked on an item to perform actual work. For example: repaint grid from changed data source, refresh file, send email etc...
    /// </summary>
    void PerformWork(TContext context);

    /// <summary>
    /// Invoked after successfull work execution - when no exception happened
    /// </summary>
    void WorkSucceeded();


    /// <summary>
    /// Invoked when either work execution or work success method threw an exception and did not succeed
    /// </summary>
    /// <param name="workPerformed">When true indicates that PerformWork() worked without exception but exception happened later</param>
    /// <param name="error">Exception instance</param>
    void WorkFailed(bool workPerformed, Exception error);

  }


}
