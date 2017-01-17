/*<FILE_LICENSE>
* NFX (.NET Framework Extension) Unistack Library
* Copyright 2003-2017 ITAdapter Corp. Inc.
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

using NFX.Environment;

namespace NFX.Serialization.CSV
{
  /// <summary>
  /// Specifies how row should be written in CSV.
  /// Use CSVWritingOptions.Default static property for typical options
  /// </summary>
  public class CSVWritingOptions : IConfigurable
  {
    private static CSVWritingOptions s_Default = new CSVWritingOptions();

    private static CSVWritingOptions s_AllFields = new CSVWritingOptions { LoadAllFields = true };

    private static CSVWritingOptions s_NoHeader = new CSVWritingOptions { IncludeHeader = false };

    public CSVWritingOptions()
    {
    }

    public CSVWritingOptions(CSVWritingOptions other)
    {
      if (other == null) return;

      this.FieldDelimiter = other.FieldDelimiter;
      this.NullValue      = other.NullValue;
      this.LoadAllFields  = other.LoadAllFields;
      this.IncludeHeader     = other.IncludeHeader;
    }

    /// <summary>
    /// Writes CSV with comma as field delimiter, empty string for null values,
    /// skipping nonUI/non-stored fields, including field names
    /// </summary>
    public static CSVWritingOptions Default { get { return s_Default; } }

    /// <summary>
    /// Writes all fields with comma as field delimiter, empty string for null values,
    /// including field names
    /// </summary>
    public static CSVWritingOptions AllFields { get { return s_AllFields; } }

    /// <summary>
    /// Writes CSV with comma as field delimiter, empty string for null values,
    ///  skipping nonUI/non-stored fields, but without field names
    /// </summary>
    public static CSVWritingOptions NoHeader { get { return s_NoHeader; } }

    /// <summary>
    /// Specifies field delimiter, comma is default
    /// </summary>
    [Config]
    public char FieldDelimiter = ',';

    /// <summary>
    /// Specifies string that will be used for null values
    /// </summary>
    [Config]
    public string NullValue = string.Empty;

    /// <summary>
    /// Indicates if nonUI/non-stored fields must be loaded
    /// </summary>
    [Config]
    public bool LoadAllFields = false;

    /// <summary>
    /// Indicates if field names must be included in result
    /// </summary>
    [Config]
    public bool IncludeHeader = true;

    public void Configure(IConfigSectionNode node)
    {
      ConfigAttribute.Apply(this, node);
    }
  }
}
