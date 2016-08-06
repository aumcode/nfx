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
 * Originated: 2006.01
 * Revision: NFX 0.2  2009.02.10
 */
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Text;

namespace NFX.Geometry
{
  /// <summary>
  /// Provides helper methods dealing with vector graphics
  /// </summary>
  public static class VectorUtils
  {


    /// <summary>
    /// Calculates callout balloon vertexes suitable for curve drawing
    /// </summary>
    /// <param name="body">Balloon body coordinates</param>
    /// <param name="target">A point of balloon leg attachment</param>
    /// <param name="legSweep">Length of balloon leg attachment breach at balloon body edge, expressed in radians (arc length). A value such as PI/16 yields good results</param>
    /// <returns>An array of vertex points</returns>
    public static Point[] VectorizeBalloon(Rectangle body, Point target, double legSweep)
    {

      List<Point> result = new List<Point>();

      Point center = new Point(body.Left + body.Width / 2, body.Top + body.Height / 2);
      PolarPoint trg = new PolarPoint(center, target);

      Point legStart = CartesianUtils.FindRayFromRectangleCenterSideIntersection(
                                 body,
                                 CartesianUtils.WrapAngle(trg.Theta, -legSweep / 2));

      Point legEnd = CartesianUtils.FindRayFromRectangleCenterSideIntersection(
                                 body,
                                 CartesianUtils.WrapAngle(trg.Theta, +legSweep / 2));

      result.Add(new Point(body.Left, body.Top));

      result.Add(new Point(body.Right, body.Top));

      result.Add(new Point(body.Right, body.Bottom));

      result.Add(new Point(body.Left, body.Bottom));

      result.Add(legStart);
      result.Add(target);
      result.Add(legEnd);

      //reorder points by azimuth so the curve can close and look like a balloon
      result.Sort(delegate(Point p1, Point p2)
      {
        PolarPoint pp1 = new PolarPoint(center, p1);
        PolarPoint pp2 = new PolarPoint(center, p2);

        if (pp1.Theta > pp2.Theta)
          return -1;
        else
          return +1;
      }
                  );

      return result.ToArray();
    }


    /// <summary>
    /// Inflates balloon body and target point
    /// </summary>
    public static void InflateBalloon(ref Rectangle body, ref Point target, int deltaBody, int deltaTarget)
    {
      Point center = new Point(body.Left + body.Width / 2, body.Top + body.Height / 2);
      PolarPoint pp = new PolarPoint(center, target);

      body.Inflate(deltaBody, deltaBody);

      pp.R += deltaTarget;

      target = pp.Point;
      target.Offset(center.X, center.Y);
    }

    /// <summary>
    /// Inflates balloon body and target point
    /// </summary>
    public static void InflateBalloon(ref Rectangle body, ref Point target, int delta)
    {
      InflateBalloon(ref body, ref target, delta, delta);
    }



  }
}
