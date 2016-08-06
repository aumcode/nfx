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

namespace NFX.Templatization
{
    /// <summary>
    /// Defines an entity that a template can be rendered into.
    /// Templates are not necessarily text-based, consequently data is supplied as objects
    /// </summary>
    public interface IRenderingTarget
    {

      /// <summary>
      /// Encodes an object per underlying target specification. For example, a Http-related target may
      ///  encode strings using HttpEncoder. If particular target does not support encoding then this method should just return the argument unmodified
      /// </summary>
      object Encode(object value);


      /// <summary>
      /// Writes a generic object into target. Templates are not necessarily text-based, consequently this method takes an object argument
      /// </summary>
      void Write(object value);

      /// <summary>
      /// Flushes writes into underlying target implementation. If target does not support buffering then this call does nothing
      /// </summary>
      void Flush();

    }
}
