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
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NFX.Geometry
{
  /// <summary>
  /// Provides support for Latitude/Longitude logic
  /// </summary>
  public struct LatLng : INamed
  {
    public const double MIN_LAT = -90.0d;
    public const double MAX_LAT = +90.0d;

    public const double MIN_LNG = -180.0d;
    public const double MAX_LNG = +180.0d;

    public const double DEG_TO_RAD =  Math.PI / 180d;

    public const double EARTH_RADIUS_KM = 6371d;

    public const double EARTH_DIAMETER_KM = 2 * EARTH_RADIUS_KM;

    public const double EARTH_CIRCUMFERENCE_KM = EARTH_DIAMETER_KM * Math.PI;


    private string m_Name;
    private double m_Lat;
    private double m_Lng;


    public LatLng(double lat, double lng, string name = null)
    {
      m_Name = name;
      m_Lat = 0d;
      m_Lng = 0d;
      Lat = lat;
      Lng = lng;
    }

    public LatLng(string val, string name = null)
    {
      if (val.IsNullOrWhiteSpace()) throw new NFXException(StringConsts.ARGUMENT_ERROR+"LatLng.ctor(val==null|empty)");
      var segs = val.Split(',');
      if (segs.Length<2) throw new NFXException(StringConsts.ARGUMENT_ERROR+"LatLng.ctor('lat,lng') expected");

      m_Name = name;
      m_Lat = 0d;
      m_Lng = 0d;

      try
      {
        Lat = parseDeg(segs[0]);
        Lng = parseDeg(segs[1]);
      }
      catch(Exception error)
      {
        throw new NFXException(StringConsts.ARGUMENT_ERROR+"LatLng.ctor('{0}'): {1}".Args(val, error.ToMessageWithType()));
      }
    }



    public string Name
    {
      get { return m_Name ?? this.ToString();}
    }


    public double Lat
    {
      get{ return m_Lat;}
      private set
      {
        if (value < MIN_LAT) value = MIN_LAT;
        else
        if (value > MAX_LAT) value = MAX_LAT;
        m_Lat = value;
      }
    }


    public double Lng
    {
      get{ return m_Lng;}
      private set
      {
        if (value < MIN_LNG) value = MIN_LNG;
        else
        if (value > MAX_LNG) value = MAX_LNG;
        m_Lng = value;
      }
    }


    public double RadLat
    {
      get{ return m_Lat * DEG_TO_RAD;}
    }

    public double RadLng
    {
      get{ return m_Lng * DEG_TO_RAD;}
    }


    public double HaversineEarthDistanceKm(LatLng other)
    {
      var dLat = this.RadLat - other.RadLat;
      var dLng = this.RadLng - other.RadLng;

		  var a  = Math.Pow(Math.Sin(dLat/2d), 2d) + Math.Cos(this.RadLat) * Math.Cos(other.RadLat) * Math.Pow(Math.Sin(dLng/2d), 2d);
		  var c  = 2d * Math.Atan2(Math.Sqrt(a), Math.Sqrt(1-a)); // great circle distance in radians

      return  EARTH_RADIUS_KM * c;
    }


    public string ComponentToString(double degVal)
    {
      var d = (int)degVal;
      degVal = Math.Abs(degVal-d) * 60d;
      var m = (int)degVal;
      degVal = Math.Abs(degVal-m) * 60d;
      var s = (int)degVal;
      return "{0}°{1}'{2}''".Args(d, m, s);
    }

    public override string ToString()
    {
      return "{0}, {1}".Args(ComponentToString(Lat), ComponentToString(Lng));
    }

    public override int GetHashCode()
    {
      return m_Lat.GetHashCode() + m_Lng.GetHashCode();
    }

    public override bool Equals(object obj)
    {
      if (!(obj is LatLng)) return false;
      var other = (LatLng)obj;
      return this.m_Lat == other.m_Lat && this.m_Lng == other.m_Lng;
    }


    private double parseDeg(string val)
    {
      if (val.Contains('°'))
      {
        var ideg = val.IndexOf('°');
        var deg = val.Substring(0, ideg);
        val = val.Substring(ideg+1);
        var imin = val.IndexOf("'");
        var min = "";
        if (imin>0)
        {
          min = val.Substring(0, imin);
          val = val.Substring(imin+1);
        }
        var isec = val.IndexOf("''");
        var sec = "";
        if (imin>0)
        {
          sec = val.Substring(0, isec);
        }

        var dd = deg.AsDouble(handling: ConvertErrorHandling.Throw);

        return dd < 0 ?
                 dd -
                 (min.AsDouble(handling: ConvertErrorHandling.Throw)/60d) -
                 (sec.AsDouble(handling: ConvertErrorHandling.Throw)/3600d)
                 :
                 dd +
                 (min.AsDouble(handling: ConvertErrorHandling.Throw)/60d) +
                 (sec.AsDouble(handling: ConvertErrorHandling.Throw)/3600d);
      }
      return double.Parse(val, System.Globalization.NumberStyles.Number);
    }


  }


}
