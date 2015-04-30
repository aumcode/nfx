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

namespace NFX.RecordModel
{
  /// <summary>
  /// Determines interactability characteristic 
  /// </summary>
  public enum InteractabilityType {Applicable, Visible, Enabled, Readonly}
  
  
  /// <summary>
  /// Represents a state for controllers 
  /// </summary>
  public enum DataState {Uninitialized = 0, Initializing, Viewing, Creating, Editing }

  /// <summary>
  /// Represents a state for storage operation such as Load() Save and Delete()
  /// </summary>
  public enum StorageOperation { None = 0, Loading, Saving, Deleting }
  
  /// <summary>
  /// Represents a type change of change made to the record 
  /// </summary>
  public enum ChangeType { None=0, Created, Edited, Deleted }


  /// <summary>
  /// Describes the ways a user can perform data entry through model-attached view
  /// </summary>
  public enum DataEntryType
  {

    /// <summary>
    /// No data entry is allowed, an item is display-only
    /// </summary>
    None = -1,
    
    /// <summary>
    /// A user can enter value directly into view control
    /// </summary>
    DirectEntry = 0,

    /// <summary>
    /// A user can either enter value directly into view control or select value from a lookup
    /// </summary>
    DirectEntryOrLookup,

    /// <summary>
    /// A user can either enter value directly into view control or select value from a lookup
    /// A value must exist in dictionary if one is defined
    /// </summary>
    DirectEntryOrLookupWithValidation,

    /// <summary>
    /// A user can only select value from lookup
    /// </summary>
    Lookup
  }


  /// <summary>
  /// Describes types of lookup
  /// </summary>
  public enum LookupType
  { 
    /// <summary>
    /// Lookup values come from LookupDictionary property
    /// </summary>
    Dictionary = 0, 
    
    /// <summary>
    /// Lookup values come from lookup command property
    /// </summary>
    Command, 
    
    /// <summary>
    /// A value is looked-up in a calendar control
    /// </summary>
    Calendar,
    
    /// <summary>
    /// A value is looked-up in a calendar control with time
    /// </summary>
    CalendarWithTime,
    
    /// <summary>
    /// A custom event is invoked, developers are responsible for implementing custom lookup solution
    /// </summary>
    Custom 
  
  };


  /// <summary>
  /// Describes formatting applied to attached view controls
  /// </summary>
  public enum DataEntryFormat
  {
    /// <summary>
    /// No formatting is performed, content is kept as-is
    /// </summary>
    AsIs = 0,

    /// <summary>
    /// Phone numbers
    /// </summary>
    Phone,

    /// <summary>
    /// aka ZIP codes in US
    /// </summary>
    PostalCode,

    /// <summary>
    /// Format as currency
    /// </summary>
    Currency,

    /// <summary>
    /// Date without time
    /// </summary>
    Date,

    /// <summary>
    /// A time without date
    /// </summary>
    Time,

    /// <summary>
    /// Date with time
    /// </summary>
    DateTime,
    
    /// <summary>
    /// An event is invoked. Developers are responsible for custom formatting
    /// </summary>
    Custom   

  };



  /// <summary>
  /// Types of text alignment for text and combo box views
  /// Supports both RTL sensitive and insensitive modes
  /// </summary>
  public enum DisplayTextHAlignment
  {
    Left = 0,
    Center,
    Right,
    LiteralLeft = 128,
    LiteralCenter,
    LiteralRight 
  }


 





}