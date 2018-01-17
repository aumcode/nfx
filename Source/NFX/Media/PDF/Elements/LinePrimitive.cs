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
using NFX.Media.PDF.Text;

namespace NFX.Media.PDF.Elements
{
  /// <summary>
  /// PDF line primitive as a part of path
  /// </summary>
  public class LinePrimitive : PathPrimitive
  {
    private const string LINE_TO_FORMAT = "{0} {1} l";

    public LinePrimitive()
    {
    }

    public LinePrimitive(float endX, float endY)
    {
      EndX = endX;
      EndY = endY;
    }

    /// <summary>
    /// Line's end X coordinate
    /// </summary>
    public float EndX { get; set; }

    /// <summary>
    /// Line's end Y coordinate
    /// </summary>
    public float EndY { get; set; }

    /// <summary>
    /// Returns PDF string representation on the line primitive
    /// </summary>
    /// <returns></returns>
    public override string ToPdfString()
    {
      return LINE_TO_FORMAT.Args(TextAdapter.FormatFloat(EndX), TextAdapter.FormatFloat(EndY));
    }
  }
}