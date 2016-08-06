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
  /// Represents a point with polar coordinates
  /// </summary>
  public struct PolarPoint
  {

    #region .ctor

      /// <summary>
      /// Initializes polar coordinates
      /// </summary>
      public PolarPoint(double r, double theta)
      {
        m_R = r;
        m_Theta = 0;
        Theta = theta;
      }

      /// <summary>
      /// Initializes polar coordinates from 2-d cartesian coordinates
      /// </summary>
      public PolarPoint(Point center, Point point)
      {
        this = CartesianUtils.PointToPolarPoint(center, point);
      }

      /// <summary>
      /// Initializes polar coordinates from 2-d cartesian coordinates of 'x1, y1, x2, y2' format
      /// </summary>
      public PolarPoint(int x1, int y1, int x2, int y2)
      {
        this = CartesianUtils.VectorToPolarPoint(x1, y1, x2, y2);
      }

    #endregion

    #region Private Fields
      private double m_R;
      private double m_Theta;

    #endregion


    #region Properties
      /// <summary>
      /// R coordinate component which is coordinate distance from point of coordinates origin
      /// </summary>
      public double R
      {
        get { return m_R; }
        set { m_R = value; }
      }


      /// <summary>
      /// Angular azimuth coordinate component. An angle must be between 0 and 2Pi.
      /// Note: Due to screen Y coordinate going from top to bottom (in usual orientation)
      ///  Theta angle may be reversed, that is - be positive in the lower half coordinate plane.
      /// Please refer to:
      ///  http://en.wikipedia.org/wiki/Polar_coordinates
      /// </summary>
      public double Theta
      {
        get { return m_Theta; }
        set
        {
          if ((value < 0) || (value > Math.PI * 2))
            throw new NFXException("Invalid polar coordinates angle");
          m_Theta = value;
        }
      }


      /// <summary>
      /// Returns polar coordinate converted to 2-d cartesian coordinates.
      /// Coordinates are relative to 0,0 of the angle base vertex
      /// </summary>
      public Point Point
      {
        get
        {
          int x = (int)(m_R * Math.Cos(m_Theta));
          int y = (int)(m_R * Math.Sin(m_Theta));
          return new Point(x, y);
        }
      }
    #endregion



    #region Operators
      public static bool operator ==(PolarPoint left, PolarPoint right)
      {
        return (left.m_R == right.m_R) && (left.m_Theta == right.m_Theta);
      }

      public static bool operator !=(PolarPoint left, PolarPoint right)
      {
        return (left.m_R != right.m_R) || (left.m_Theta != right.m_Theta);
      }
    #endregion


    #region Object overrides
      public override bool Equals(object obj)
      {
        if (obj is PolarPoint)
         return this==((PolarPoint)obj);
        else
         return false;
      }

      public override int GetHashCode()
      {
        return m_R.GetHashCode() + m_Theta.GetHashCode();
      }

      public override string ToString()
      {
        return string.Format("Distance: {0}; Angle: {1} rad.", m_R, m_Theta);
      }


    #endregion

  }


}