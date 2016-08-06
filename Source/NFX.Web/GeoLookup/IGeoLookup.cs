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
using System.Net;

using NFX.ApplicationModel;


namespace NFX.Web.GeoLookup
{

  public enum LookupResolution{Planet, Continent, Country, City, Street }

  /// <summary>
  /// Looks-up entities by their ip addr
  /// </summary>
  public interface IGeoLookup : IApplicationComponent
  {
    /// <summary>
    /// Returns true when the underlying provider is initialized and ready to serve queries
    /// </summary>
    bool Available{get;}

    /// <summary>
    /// Specifies how accurate the lookup is
    /// </summary>
    LookupResolution Resolution{get;}

    /// <summary>
    /// Tries to lookup the geo entitiy by domain IPAddress.
    /// Returns null when no match could be made
    /// </summary>
    GeoEntity Lookup(IPAddress address);
  }


  /// <summary>
  /// Represents lookup result
  /// </summary>
  [NFX.Serialization.Slim.SlimSerializationProhibited]
  public class GeoEntity
  {
    protected internal GeoEntity(IPAddress query, IPSubnetBlock block, Location? location)
    {
      Query = query;
      Block = block;
      Location = location;
    }
    /// <summary>
    /// Returns the address originally requested
    /// </summary>
    public readonly IPAddress Query;

    /// <summary>
    /// Information about IP address block
    /// </summary>
    public readonly IPSubnetBlock Block;

    /// <summary>
    /// Information about geo location
    /// </summary>
    public readonly Location? Location;

    /// <summary>
    /// Returns handy name of locality, i.e. "City, Country" if city is available
    /// </summary>
    public string LocalityName
    {
      get
      {
        if (!Location.HasValue) return string.Empty;

        return Location.Value.CityName.IsAssigned
          ? "{0}, {1} {2}".Args(Location.Value.CityName.Value, Location.Value.SubdivisionName.Value, Location.Value.CountryName.Value)
          : "{0} {1}".Args(Location.Value.SubdivisionName.Value, Location.Value.CountryName.Value);
      }
    }

    public string CountryISOName
    {
      get
      {
        return Location.HasValue ? Location.Value.CountryISOName.Value : null;
      }
    }

    public string CountryName
    {
      get
      {
        return Location.HasValue ? Location.Value.CountryName.Value : null;
      }
    }

    public override string ToString()
    {
      return "{0} -> {1}{2}".Args(Query, Block, Location);
    }

  }




}
