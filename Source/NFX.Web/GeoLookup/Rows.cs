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

using NFX.DataAccess.CRUD;

namespace NFX.Web.GeoLookup
{
  /// <summary>
  /// Provides address segment block information
  /// </summary>
  [Serializable]
  public class IPAddressBlock : TypedRow
  {
    [Field(key:true)] public string IPBlockStart       {get; set;} 
    [Field] public string           LocationID         {get; set;}  
    [Field] public string           RegisteredLocationID  {get; set;}
    [Field] public string           RepresentedLocationID {get; set;}
    [Field] public bool             AnonymousProxy     {get; set;}
    [Field] public bool             SatelliteProvider  {get; set;}
    [Field] public string           PostalCode         {get; set;}
    [Field] public string           Lat                {get; set;}
    [Field] public string           Lng                {get; set;}
    public override string ToString()
    {
      return @" 
IPBlockStart       {0} 
LocationID         {1}  
RegisteredLocationID  {2}
RepresentedLocationID {3}
PostalCode         {4}
Lat                {5}
Lng                {6}
AnonymousProxy     {7}
SatelliteProvider  {8}".Args(
IPBlockStart       , 
LocationID         ,  
RegisteredLocationID,  
RepresentedLocationID, 
PostalCode         ,
Lat                ,
Lng                ,
AnonymousProxy     ,
SatelliteProvider);
    }
  }

  /// <summary>
  /// Provides location information
  /// </summary>
  [Serializable]
  public class Location : TypedRow
  {
    [Field(key:true)] public string ID                 {get; set;}
    [Field] public string           LocaleCode        {get; set;}
    [Field] public string           ContinentID        {get; set;}
    [Field] public string           ContinentName      {get; set;} 
    [Field] public string           CountryISOName     {get; set;}  
    [Field] public string           CountryName        {get; set;}  
    [Field] public string           SubdivisionISOCode {get; set;}
    [Field] public string           SubdivisionName    {get; set;}  
    [Field] public string           Subdivision2ISOCode {get; set;}
    [Field] public string           Subdivision2Name    {get; set;}
    [Field] public string           CityName           {get; set;}
    [Field] public string           MetroCode          {get; set;}
    [Field] public string           TimeZone           {get; set;}
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
