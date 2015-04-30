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
 * Originated: 2008.03
 * Revision: NFX 1.0  2011.02.06
 */
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace NFX.RecordModel
{
    /// <summary>
  /// Interfaces an item with changeable interaction behavior such as: applicability, visibility, being enabled, readonly etc.
  /// </summary>
  public interface IControllableInteractions
  {
    /// <summary>
    /// Determines whether an item applies (can be interacted at all). Usually this property is
    /// acted upon  by an application business rules
    /// </summary>
    bool Applicable { get; set; }
    
    /// <summary>
    /// Determines whether an item is shown to the user. 
    /// Presentation method of an item is dependent on an implementation of a particular class.
    /// For non-visual model/controller classes this property cascades down to attached view devices.
    /// </summary>
    bool Visible    { get; set; }
    
    /// <summary>
    /// Determines whether an item is enabled for user actions.
    /// Presentation method of an item is dependent on an implementation of a particular class.
    /// For non-visual model/controller classes this property cascades down to attached view devices.
    /// </summary>
    bool Enabled    { get; set; }
    
    /// <summary>
    /// Determines whether an item is available for data input.
    /// Presentation method of an item is dependent on an implementation of a particular class.
    /// For non-visual model/controller classes this property cascades down to attached view devices.
    /// </summary>
    bool Readonly   { get; set; }



    /// <summary>
    /// Determines whether an item is really applicable. 
    /// Item applicability is governed by parent context and may differ from instance's Applicable property 
    /// <see>Applicable</see>
    /// </summary>
    bool IsApplicable { get; }

    /// <summary>
    /// Determines whether an item is really visible. 
    /// Item visibility is governed by parent context and may differ from instance's Visible property 
    /// <see>Visible</see>
    /// </summary>
    bool IsVisible { get; }

    /// <summary>
    /// Determines whether an item is really enabled. 
    /// Item enabled state is governed by parent context and may differ from instance's Enabled property 
    /// <see>Enabled</see>
    /// </summary>
    bool IsEnabled { get; }

    /// <summary>
    /// Determines whether an item is really readonly. 
    /// Item readonly state is governed by parent context and may differ from instance's Readonly property 
    /// <see>Readonly</see>
    /// </summary>
    bool IsReadonly { get; }
    
    
  }
}
