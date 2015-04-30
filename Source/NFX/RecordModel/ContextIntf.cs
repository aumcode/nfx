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
  /// Describes an entity which can function in a context of a particular record
  /// </summary>
  public interface IRecordContext
  {
    Record Record { get; }
  }


  /// <summary>
  /// Describes an entity that has design-time record type name property 
  /// </summary>
  public interface ISurrogateRecordTypeNameProvider
  {
    string AttachToRecordSurrogateTypeName { get; }
  }
  

  /// <summary>
  /// Describes an entity which takes its record from parent entity, i.e. field view control
  ///  takes AttachToRecord property from it's containing form or panel
  /// </summary>
  public interface IParentRecordAttachable
  {
    /// <summary>
    /// Indicates whether an item should automatically attach to it's parent record
    /// </summary>
    bool AttachToParentRecord { get; }
    
    /// <summary>
    /// Attaches an item to a record provided by its logical parent
    /// </summary>
    bool TryAttachModel();
  }


  /// <summary>
  /// Describes an entity which can function in a context of a particular record's field
  /// </summary>
  public interface IRecordFieldContext : IRecordContext
  {
    /// <summary>
    /// Defines what field by reference this binding is connected to. 
    /// </summary>
    Field Field { get;  }
  }

}