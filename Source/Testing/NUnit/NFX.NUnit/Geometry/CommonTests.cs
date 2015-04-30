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
using System;
using System.IO;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Drawing;
using NUnit.Framework;

using NFX.Geometry;

namespace NFX.NUnit.Geometry
{
  [TestFixture]
  public class CommonTests
  {
    [TestCase]
    public void MapDirection()
    {
      var dir1 = NFX.Geometry.MapDirection.East;
      var dir2 = NFX.Geometry.MapDirection.North;
      var dir3 = NFX.Geometry.MapDirection.East;
      Assert.AreEqual("East", dir1.ToString());
      Assert.AreEqual("North", dir2.ToString());

      Assert.AreNotEqual(dir1, dir2);
      Assert.AreNotEqual(dir1, dir2);
      Assert.AreEqual(dir1, dir3);
    }


    [TestCase]
    public void Distance()
    {
      Assert.AreEqual(100, CartesianUtils.Distance(0, 0, 100, 0));
      Assert.AreEqual(100, CartesianUtils.Distance(0, 0, 0, 100));
      Assert.AreEqual(141421D, Math.Floor(1000F * CartesianUtils.Distance(0F, 0F, 100F, 100F)));
      Assert.AreEqual(141421D, Math.Floor(1000F * CartesianUtils.Distance(100F, 0F, 0F, 100F)));
      Assert.AreEqual(141421D, Math.Floor(1000F * CartesianUtils.Distance(100F, 100F, 0F, 0F)));
    }

    [TestCase]
    public void DistancePoints()
    {
      Assert.AreEqual(100, CartesianUtils.Distance(new Point(0, 0), new Point(100, 0)));
      Assert.AreEqual(100, CartesianUtils.Distance(new Point(0, 0), new Point(0, 100)));
      Assert.AreEqual(141421D, Math.Floor(1000F * CartesianUtils.Distance(new PointF(0, 0), new PointF(100, 100))));
      Assert.AreEqual(141421D, Math.Floor(1000F * CartesianUtils.Distance(new PointF(100, 0), new PointF(0, 100))));
      Assert.AreEqual(141421D, Math.Floor(1000F * CartesianUtils.Distance(new PointF(100, 100), new PointF(0, 0))));
    }

    [TestCase]
    public void RadToDeg()
    {
      Assert.AreEqual(180D, Math.PI.ToDeg());
      Assert.AreEqual(360D, (Math.PI * 2D).ToDeg());
    }

    [TestCase]
    public void DegToRad()
    {
      Assert.AreEqual(157, Math.Floor(90D.ToRad() * 100));
      Assert.AreEqual(314, Math.Floor(180D.ToRad() * 100));
    }

    [TestCase]
    public void AzimuthRad()
    {
      Assert.AreEqual(157D, Math.Floor(CartesianUtils.AzimuthRad(0, 0, 0, 100) * 100));
      Assert.AreEqual(0D, Math.Floor(CartesianUtils.AzimuthRad(0, 0, 100, 0) * 100));
    }

    [TestCase]
    public void AzimuthDeg()
    {
      Assert.AreEqual(90D, Math.Floor(CartesianUtils.AzimuthDeg(0, 0, 0, 100)));
      Assert.AreEqual(0D, Math.Floor(CartesianUtils.AzimuthDeg(0, 0, 100, 0)));
    }

    [TestCase]
    public void AzimuthOfRadix()
    {
      Assert.AreEqual(3, CartesianUtils.AzimuthOfRadix(0, 0,   0, -100, 4));
      Assert.AreEqual(0, CartesianUtils.AzimuthOfRadix(0, 0, 100, -100, 4));
      Assert.AreEqual(1, CartesianUtils.AzimuthOfRadix(0, 0, 100, 100, 4));
      Assert.AreEqual(2, CartesianUtils.AzimuthOfRadix(0, 0, -100, 100, 4));
    }

    [TestCase]
    public void WrapAngle()
    {
      Assert.AreEqual(314D, Math.Floor(CartesianUtils.WrapAngle(0D, Math.PI * 3D) * 100D));
      Assert.AreEqual(314D, Math.Floor(CartesianUtils.WrapAngle(Math.PI, Math.PI * 2D) * 100D));
      Assert.AreEqual(157D, Math.Floor(CartesianUtils.WrapAngle(Math.PI, -Math.PI / 2D) * 100D));
      Assert.AreEqual(314D + 157D, Math.Floor(CartesianUtils.WrapAngle(Math.PI, Math.PI / 2D) * 100D));
    }

    [TestCase]
    public void MapDirectionToAngle()
    {
      Assert.AreEqual(157D, Math.Floor(CartesianUtils.MapDirectionToAngle(NFX.Geometry.MapDirection.North) * 100D));
      Assert.AreEqual(314D + 157D, Math.Floor(CartesianUtils.MapDirectionToAngle(NFX.Geometry.MapDirection.South) * 100D));
      Assert.AreEqual(0D, Math.Floor(CartesianUtils.MapDirectionToAngle(NFX.Geometry.MapDirection.East) * 100D));
      Assert.AreEqual(314D, Math.Floor(CartesianUtils.MapDirectionToAngle(NFX.Geometry.MapDirection.West) * 100D));

      Assert.AreEqual(157D - 79D, Math.Floor(CartesianUtils.MapDirectionToAngle(NFX.Geometry.MapDirection.NorthEast) * 100D));
      Assert.AreEqual(157D + 78D, Math.Floor(CartesianUtils.MapDirectionToAngle(NFX.Geometry.MapDirection.NorthWest) * 100D));

      Assert.AreEqual(314D + 157D + 78D, Math.Floor(CartesianUtils.MapDirectionToAngle(NFX.Geometry.MapDirection.SouthEast) * 100D));
      Assert.AreEqual(314D + 157D - 79D, Math.Floor(CartesianUtils.MapDirectionToAngle(NFX.Geometry.MapDirection.SouthWest) * 100D));
    }

    [TestCase]
    public void AngleToMapDirection()
    {
      Assert.AreEqual(NFX.Geometry.MapDirection.North, 
        CartesianUtils.AngleToMapDirection(CartesianUtils.MapDirectionToAngle(NFX.Geometry.MapDirection.North)));
      Assert.AreEqual(NFX.Geometry.MapDirection.South,
        CartesianUtils.AngleToMapDirection(CartesianUtils.MapDirectionToAngle(NFX.Geometry.MapDirection.South)));
      Assert.AreEqual(NFX.Geometry.MapDirection.East,
        CartesianUtils.AngleToMapDirection(CartesianUtils.MapDirectionToAngle(NFX.Geometry.MapDirection.East)));
      Assert.AreEqual(NFX.Geometry.MapDirection.West,
        CartesianUtils.AngleToMapDirection(CartesianUtils.MapDirectionToAngle(NFX.Geometry.MapDirection.West)));

      Assert.AreEqual(NFX.Geometry.MapDirection.NorthEast,
        CartesianUtils.AngleToMapDirection(CartesianUtils.MapDirectionToAngle(NFX.Geometry.MapDirection.NorthEast)));
      Assert.AreEqual(NFX.Geometry.MapDirection.NorthWest,
        CartesianUtils.AngleToMapDirection(CartesianUtils.MapDirectionToAngle(NFX.Geometry.MapDirection.NorthWest)));

      Assert.AreEqual(NFX.Geometry.MapDirection.SouthEast,
        CartesianUtils.AngleToMapDirection(CartesianUtils.MapDirectionToAngle(NFX.Geometry.MapDirection.SouthEast)));
      Assert.AreEqual(NFX.Geometry.MapDirection.SouthWest,
        CartesianUtils.AngleToMapDirection(CartesianUtils.MapDirectionToAngle(NFX.Geometry.MapDirection.SouthWest)));
    }

    [TestCase]
    public void PerimeterViolationArea()
    {
      Assert.AreEqual(0, CartesianUtils.CalculatePerimeterViolationArea(new Rectangle(0, 0, 100, 100), new Rectangle(0, 0, 100, 100)));
      Assert.AreEqual(100, CartesianUtils.CalculatePerimeterViolationArea(new Rectangle(0, 0, 100, 100), new Rectangle(1, 0, 100, 100)));
      Assert.AreEqual(100, CartesianUtils.CalculatePerimeterViolationArea(new Rectangle(0, 0, 100, 100), new Rectangle(-1, 0, 100, 100)));
      Assert.AreEqual(20*100, CartesianUtils.CalculatePerimeterViolationArea(new Rectangle(0, 0, 100, 100), new Rectangle(-10, -10, 100, 100)));
    }

    [TestCase]
    public void FindRayFromRectangleCenterSideIntersection1()
    {
      var ray = CartesianUtils.FindRayFromRectangleCenterSideIntersection(new Rectangle(-50, -50, 100, 100), 0D);
      Assert.AreEqual(50, ray.X);
      Assert.AreEqual(0, ray.Y);
    }

    [TestCase]
    public void FindRayFromRectangleCenterSideIntersection2()
    {
      var ray = CartesianUtils.FindRayFromRectangleCenterSideIntersection(new Rectangle(-50, -50, 100, 100), Math.PI / 2);
      Assert.AreEqual(0, ray.X);
      Assert.AreEqual(50, ray.Y);
    }

    [TestCase]
    public void FindRayFromRectangleCenterSideIntersection3()
    {
      var ray = CartesianUtils.FindRayFromRectangleCenterSideIntersection(new Rectangle(-50, -50, 100, 100), Math.PI);
      Assert.AreEqual(-50, ray.X);
      Assert.AreEqual(0, ray.Y);
    }

    [TestCase]
    public void FindRayFromRectangleCenterSideIntersection4()
    {
      var ray = CartesianUtils.FindRayFromRectangleCenterSideIntersection(new Rectangle(-50, -50, 100, 100), Math.PI + Math.PI / 2);
      Assert.AreEqual(0, ray.X);
      Assert.AreEqual(-50, ray.Y);
    }

    [TestCase]
    public void PointToPolarPoint()
    {
      var center = new Point(0, 0);
      var pnt = new Point(150, 0);

      var polar = new PolarPoint(center, pnt);

      Assert.AreEqual(150D, polar.R);
      Assert.AreEqual(0D, polar.Theta);
    }

    [TestCase]
    public void PolarPointInvalid()
    {
      try
      {
        var pp = new PolarPoint(100, 789);
        Assert.Fail();
      }
      catch (NFXException ex)
      {
        Assert.IsTrue(ex.Message.Contains("angle"));
      }
    }

    [TestCase]
    public void PolarPointRadius()
    {
      var polar = new PolarPoint(100D, Math.PI);
      Assert.AreEqual(100D, polar.R);

      polar.R = 125D;
      Assert.AreEqual(125D, polar.R);
    }

    [TestCase]
    public void PolarPointTheta()
    {
      var polar = new PolarPoint(100D, Math.PI);
      Assert.AreEqual(314D, Math.Floor(polar.Theta * 100D));

      polar.Theta = 1.18D;
      Assert.AreEqual(118D, Math.Floor(polar.Theta * 100D));
    }

    [TestCase]
    public void PolarPointToPoint()
    {
      var polar = new PolarPoint(100D, Math.PI / 4);
      var decart = polar.Point;

      Assert.AreEqual(70D, Math.Floor((double)decart.X));
      Assert.AreEqual(70D, Math.Floor((double)decart.Y));
    }

    [TestCase]
    public void PolarPointIsEqual()
    {
      var polar1 = new PolarPoint(100D, 1.2D);
      var polar2 = new PolarPoint(10D, 2.17D);
      var polar3 = new PolarPoint(100D, 1.2D);

      Assert.AreNotEqual(polar1, polar2);
      Assert.AreEqual(polar1, polar3);
    }

    [TestCase]
    public void VectorizeBalloon()
    {
      var rect = new Rectangle( -100, -100, 200, 200);
      var target = new Point(0, 300);
      var lagSweep = Math.PI / 16D;
      var points = VectorUtils.VectorizeBalloon(rect, target, lagSweep);

      foreach (var p in points)
        Console.WriteLine(p.ToString());

      Assert.AreEqual(7, points.Length);

      var expectedPoints = new int[,] { { 100, -100 }, { -100, -100 }, { -100, 100 }, { -9, 100 }, { 0, 300 }, { 9, 100 }, { 100, 100 } };
      for (int i = 0; i < points.Length; i++)
      {
        Assert.AreEqual(expectedPoints[i, 0], points[i].X);
        Assert.AreEqual(expectedPoints[i, 1], points[i].Y);
      }
    }
  }
}
