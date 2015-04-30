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


namespace NFX.WinForms.Views
{

  /// <summary>
  /// Types of data-entry control
  /// </summary>
  public enum ControlType
  {
    None = -1,
    Auto = 0,
    Text,
    Combo,
    Radio,
    Check,
    Grid  
  }


  /// <summary>
  /// Determines where field caption is placed relative to data entry area
  /// </summary>
  public enum CaptionPlacement
  {
    Top = 0, Bottom, Left, Right
  }

  /// <summary>
  /// Types of text alignment for text and combo boxes.
  /// Supports both RTL sensitive and insensitive modes
  /// </summary>
  public enum TextHAlignment
  {
    Controller = -1,
    Left = 0,
    Center,
    Right,
    LiteralLeft = 128,
    LiteralCenter,
    LiteralRight
  }


}