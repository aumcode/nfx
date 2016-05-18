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

namespace NFX.Web.GeoLookup
{
  /// <summary>
  /// Provides address segment block information
  /// </summary>
  public struct IPSubnetBlock
  {
    public readonly SealedString Subnet;
    public readonly SealedString LocationID;
    public readonly SealedString RegisteredLocationID;
    public readonly SealedString RepresentedLocationID;
    public readonly bool AnonymousProxy;
    public readonly bool SatelliteProvider;
    public readonly SealedString PostalCode;
    public readonly float Lat;
    public readonly float Lng;
    public IPSubnetBlock(
      SealedString subnet,
      SealedString locationID,
      SealedString registeredLocationID,
      SealedString representedLocationID,
      bool anonymousProxy,
      bool satelliteProvider,
      SealedString postalCode,
      float lat,
      float lng)
    {
      Subnet = subnet;
      LocationID = locationID;
      RegisteredLocationID = registeredLocationID;
      RepresentedLocationID = representedLocationID;
      AnonymousProxy = anonymousProxy;
      SatelliteProvider = satelliteProvider;
      PostalCode = postalCode;
      Lat = lat;
      Lng = lng;
    }
  }

  /// <summary>
  /// Provides location information
  /// </summary>
  public struct Location
  {
    public readonly SealedString ID;
    public readonly SealedString LocaleCode;
    public readonly SealedString ContinentID;
    public readonly SealedString ContinentName;
    public readonly SealedString CountryISOName;
    public readonly SealedString CountryName;
    public readonly SealedString SubdivisionISOCode;
    public readonly SealedString SubdivisionName;
    public readonly SealedString Subdivision2ISOCode;
    public readonly SealedString Subdivision2Name;
    public readonly SealedString CityName;
    public readonly SealedString MetroCode;
    public readonly SealedString TimeZone;
    public Location(
      SealedString id,
      SealedString localeCode,
      SealedString continentID,
      SealedString continentName,
      SealedString countryISOName,
      SealedString countryName,
      SealedString subdivisionISOCode,
      SealedString subdivisionName,
      SealedString subdivision2ISOCode,
      SealedString subdivision2Name,
      SealedString cityName,
      SealedString metroCode,
      SealedString timeZone)
    {
      ID = id;
      LocaleCode = localeCode;
      ContinentID = continentID;
      ContinentName = continentName;
      CountryISOName = countryISOName;
      CountryName = countryName;
      SubdivisionISOCode = subdivisionISOCode;
      SubdivisionName = subdivisionName;
      Subdivision2ISOCode = subdivision2ISOCode;
      Subdivision2Name = subdivision2Name;
      CityName = cityName;
      MetroCode = metroCode;
      TimeZone = timeZone;
    }
  }
}
