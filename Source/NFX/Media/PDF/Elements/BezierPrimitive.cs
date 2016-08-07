/*<FILE_LICENSE>
* NFX (.NET Framework Extension) Unistack Library
* Copyright 2003-2014 Dmitriy Khmaladze, IT Adapter Inc / 2015-2016 Aum Code LLC
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
  /// PDF Bezier curve primitive as a part of path
  /// </summary>
  public class BezierPrimitive : PathPrimitive
  {
    private const string BEZIER_TO_FORMAT = "{0} {1} {2} {3} {4} {5} c";

    public BezierPrimitive()
    {
    }

    public BezierPrimitive(float firstControlX, float firstControlY, float secondControlX, float secondControlY, float endX, float endY)
    {
      FirstControlX = firstControlX;
      FirstControlY = firstControlY;
      SecondControlX = secondControlX;
      SecondControlY = secondControlY;
      EndX = endX;
      EndY = endY;
    }

    /// <summary>
    /// X coordinate of Bezier's curve first control point
    /// </summary>
    public float FirstControlX { get; set; }

    /// <summary>
    /// Y coordinate of Bezier's curve first control point
    /// </summary>
    public float FirstControlY { get; set; }

    /// <summary>
    /// X coordinate of Bezier's curve second control point
    /// </summary>
    public float SecondControlX { get; set; }

    /// <summary>
    /// Y coordinate of Bezier's curve second control point
    /// </summary>
    public float SecondControlY { get; set; }

    /// <summary>
    /// X coordinate of Bezier's curve last point
    /// </summary>
    public float EndX { get; set; }

    /// <summary>
    /// Y coordinate of Bezier's curve last point
    /// </summary>
    public float EndY { get; set; }

    /// <summary>
    /// Returns PDF string representation on the line primitive
    /// </summary>
    /// <returns></returns>
    public override string ToPdfString()
    {
      return BEZIER_TO_FORMAT.Args(TextAdapter.FormatFloat(FirstControlX),
                                   TextAdapter.FormatFloat(FirstControlY),
                                   TextAdapter.FormatFloat(SecondControlX),
                                   TextAdapter.FormatFloat(SecondControlY),
                                   TextAdapter.FormatFloat(EndX),
                                   TextAdapter.FormatFloat(EndY));
    }
  }
}