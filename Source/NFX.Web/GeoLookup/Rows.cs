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

using System.Net;
using System.Collections;

namespace NFX.Web.GeoLookup
{
  public struct Subnet: IEnumerable<bool>
  {
    public readonly IPAddress address;
    public readonly uint CIDR;
    public Subnet(IPAddress ipaddress, uint cidr)
    {
      address = ipaddress;
      CIDR = cidr;
    }
    public Subnet(IPAddress ipaddress)
    {
      address = ipaddress;
      CIDR = 128;
    }
    public Subnet(string subnet)
    {
      var parts = subnet.Split('/');
      address = IPAddress.Parse(parts[0]);
      CIDR = uint.Parse(parts[1]);
    }

    public IEnumerator<bool> GetEnumerator()
    {
      var bytes = address.GetAddressBytes();
      for (var i = 0u; i < CIDR; i++)
        yield return (bytes[i / 8] & (1u << ((byte)(7 - i) % 8))) != 0;
    }

    IEnumerator IEnumerable.GetEnumerator()
    {
      return GetEnumerator();
    }

    public override string ToString()
    {
      return "{0}/{1}".Args(address, CIDR);
    }      
  }

  /// <summary>
  /// Provides address segment block information
  /// </summary>
  [Serializable]
  public struct IPSubnetBlock
  {
    public readonly Subnet Subnet;
    public readonly SealedString LocationID;
    public readonly SealedString RegisteredLocationID;
    public readonly SealedString RepresentedLocationID;
    public readonly bool AnonymousProxy;
    public readonly bool SatelliteProvider;
    public readonly SealedString PostalCode;
    public readonly float Lat;
    public readonly float Lng;
    public IPSubnetBlock(
      Subnet subnet,
      string locationID,
      string registeredLocationID,
      string representedLocationID,
      bool anonymousProxy,
      bool satelliteProvider,
      string postalCode,
      float lat,
      float lng)
    {
      Subnet = subnet;
      LocationID = new SealedString(locationID);
      RegisteredLocationID = new SealedString(registeredLocationID);
      RepresentedLocationID = new SealedString(representedLocationID);
      AnonymousProxy = anonymousProxy;
      SatelliteProvider = satelliteProvider;
      PostalCode = new SealedString(postalCode);
      Lat = lat;
      Lng = lng;
    }
    public override string ToString()
    {
      return @" 
Subnet                {0} 
LocationID            {1}  
RegisteredLocationID  {2}
RepresentedLocationID {3}
PostalCode            {4}
Lat                   {5}
Lng                   {6}
AnonymousProxy        {7}
SatelliteProvider     {8}".Args(
Subnet               , 
LocationID           ,  
RegisteredLocationID ,  
RepresentedLocationID, 
PostalCode           ,
Lat                  ,
Lng                  ,
AnonymousProxy       ,
SatelliteProvider    );
    }
  }

  /// <summary>
  /// Provides location information
  /// </summary>
  [Serializable]
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
      string id,
      string localeCode,
      string continentID,
      string continentName,
      string countryISOName,
      string countryName,
      string subdivisionISOCode,
      string subdivisionName,
      string subdivision2ISOCode,
      string subdivision2Name,
      string cityName,
      string metroCode,
      string timeZone)
    {
      ID = new SealedString(id);
      LocaleCode = new SealedString(localeCode);
      ContinentID = new SealedString(continentID);
      ContinentName = new SealedString(continentName);
      CountryISOName = new SealedString(countryISOName);
      CountryName = new SealedString(countryName);
      SubdivisionISOCode = new SealedString(subdivisionISOCode);
      SubdivisionName = new SealedString(subdivisionName);
      Subdivision2ISOCode = new SealedString(subdivision2ISOCode);
      Subdivision2Name = new SealedString(subdivision2Name);
      CityName = new SealedString(cityName);
      MetroCode = new SealedString(metroCode);
      TimeZone = new SealedString(timeZone);
    }
    public override string ToString()
    {
      return @" 
ID                  {0}
LocaleCode          {1} 
ContinentID         {2} 
ContinentName       {3}  
CountryISOName      {4}
CountryName         {5}
SubdivisionISOCode  {6}
SubdivisionName     {7}
Subdivision2ISOCode {8}
Subdivision2Name    {9}
CityName            {10}
MetroCode           {11}
TimeZone            {12}".Args(
ID                  , 
LocaleCode          ,
ContinentID         , 
ContinentName       ,  
CountryISOName      ,  
CountryName         , 
SubdivisionISOCode  ,
SubdivisionName     , 
Subdivision2ISOCode ,
Subdivision2Name    ,
CityName            ,
MetroCode           ,
TimeZone            );
    }
  }

}
