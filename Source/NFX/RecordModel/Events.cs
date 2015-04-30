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

namespace NFX.RecordModel
{

  

  public class InteractabilityChangeEventArgs
  {
       public readonly InteractabilityType InteractabilityType;
       public readonly bool OldValue;
       public readonly bool NewValue;
       
       public InteractabilityChangeEventArgs(InteractabilityType type, bool oldVal, bool newVal)
       {
         InteractabilityType = type;
         OldValue = oldVal;
         NewValue = newVal;
       }
  }

  public delegate void InteractabilityChangeEventHandler(ModelBase sender, InteractabilityChangeEventArgs args);

  public class StateChangeEventArgs
  {
      public readonly DataState OldValue;
      public readonly DataState NewValue;

      public StateChangeEventArgs(DataState oldVal, DataState newVal)
      {
        OldValue = oldVal;
        NewValue = newVal;
      }
  }

  public delegate void StateChangeEventHandler(ModelBase sender, StateChangeEventArgs args);
  
  
  
  public class FieldDataChangeEventArgs
  {
    public readonly object NewValue;
    public readonly bool HasNewValue;
    
    /// <summary>
    /// Represents a GUI control binding (if any) that this field's value is being changed from.
    /// NULL when change was not caused by manual user entry
    /// </summary>
    public readonly IBinding GUIBinding;
    
    /// <summary>
    /// Indicates whether an event was caused by user GUI action 
    /// </summary>
    public bool FromGUI 
    {
      get { return GUIBinding != null; }
    }

    public FieldDataChangeEventArgs(object newVal, bool hasVal, IBinding binding)
    {
      NewValue = newVal;      
      HasNewValue = hasVal;
      GUIBinding = binding;
    }
  }

  public delegate void FieldDataChangeEventHandler(Field sender, FieldDataChangeEventArgs args);



  /// <summary>
  /// Event handler invoked to get client scripts for specific technology
  /// </summary>
  public delegate ClientScripts GetClientScriptsEventHandler(ModelBase sender, string technologyFilter);

}
