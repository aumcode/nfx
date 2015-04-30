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
using System.Drawing;

using NFX.RecordModel;

namespace NFX.WinForms
{
  /// <summary>
  /// Describes an entity, usually a control-derivative, that provides field-bound view properties.
  ///  <para>This interface is mostly used for theme's parts rendering as parts painting may depend on control state
  ///  such as "valid/invalid","modified" etc.  </para>
  /// <para> Usage of this interface is a better solution rather than passing those characteristics to every part rendering function
  ///  because the later is much less flexible.</para> 
  ///  <para>When new property needs to be added in future,
  ///  only interface and implementing classes need to be changed, 
  ///  thus existing rendering code will not break and may elect to completely ignore new context characteristics. </para>
  /// </summary>
  public interface IFieldControlContext : IControllableInteractions
  {
    bool Modified { get; }
    bool GUIModified { get; }
    bool Valid { get; }
    bool Validated { get; }
    bool Marked { get; }
    bool Multiline { get; }
    bool Focused { get;}
    bool Required { get;}
    Field Field  { get;}
    Color MarkerColor { get; }
    float MarkerIntensity { get; }
  }
  
}
