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

namespace NFX
{
  /// <summary>
  /// Describes an entity that can request some hosting container to end its lifetime by calling End() method
  /// </summary>
  public interface IEndableInstance
  {
     /// <summary>
     /// Indicates whether this instance was requested to be ended and will get destoyed by the hosting container
     /// </summary>
     bool IsEnded { get; }

     /// <summary>
     /// Requests container that hosts.runs this entity to end its instance
     /// </summary>
     void End();
  }
}
