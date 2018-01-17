/*<FILE_LICENSE>
* NFX (.NET Framework Extension) Unistack Library
* Copyright 2003-2018 Agnicore Inc. portions ITAdapter Corp. Inc.
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
using System.Threading.Tasks;

namespace NFX.DataAccess.CRUD
{

  /// <summary>
  /// Establishes a thead-static context which surrounds CRUD operations. You can derive your own classes, the .ctor must be chained.
  /// The context can be nested. A call to .ctor must be balanced with .Dispose().
  /// This is needed to pass some out-of-band information in some special cases to CRUD operations without changing
  /// the caller interface, i.e. to swap database connection string.
  /// This class IS NOT THREAD SAFE, so in cases of async operations, the context captures extra parameters ONLY for initial ASYNC INVOCATION, that is-
  ///  a true ASYNC implementation must pass the reference along the task execution line (in which case the object may be already Disposed but usable for property access
  /// </summary>
  public class CRUDOperationCallContext : DisposableObject
  {
      [ThreadStatic]
      private static Stack<CRUDOperationCallContext> ts_Instances;

      /// <summary>
      /// Returns the current set CRUDOperationCallContext or null
      /// </summary>
      public static CRUDOperationCallContext Current
      {
        get
        {
          return ts_Instances!=null && ts_Instances.Count>0 ? ts_Instances.Peek() : null;
        }
      }

      public CRUDOperationCallContext()
      {
        if (ts_Instances==null)
           ts_Instances = new Stack<CRUDOperationCallContext>();

        ts_Instances.Push(this);
      }

      protected override void Destructor()
      {
        if (ts_Instances.Count>0)
        {
          if (ts_Instances.Pop()==this) return;
        }

        throw new CRUDException(StringConsts.CRUD_OPERATION_CALL_CONTEXT_SCOPE_MISMATCH_ERROR);
      }


      /// <summary>
      /// Used to override store's default database connection string
      /// </summary>
      public string ConnectString{ get; set;}

      /// <summary>
      /// Used to override store's default database name - used by some stores, others take db name form connect string
      /// </summary>
      public string DatabaseName{ get; set;}
  }

}
