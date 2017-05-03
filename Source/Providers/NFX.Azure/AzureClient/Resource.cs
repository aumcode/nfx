using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using NFX.Collections;
using NFX.Serialization.JSON;
using NFX.Web;
using NFX.DataAccess.CRUD;

namespace NFX.AzureClient
{
  public class Resource : AmorphousTypedRow
  {
    [Field(backendName: "id")]
    public string Id { get; set; }

    public override Exception Validate(string targetName)
    {
      if (AmorphousDataEnabled && AmorphousData.Count > 0)
        return base.Validate(targetName);
      return base.Validate(targetName);
    }
  }

  public class NamedResource : Resource
  {
    [Field(backendName: "name")]
    public string Name { get; private set; }
    [Field(backendName: "type")]
    public string Type { get; private set; }
    [Field(backendName: "location")]
    public string Location { get; set; }
    [Field(backendName: "tags")]
    public JSONDataMap Tags { get; set; }
  }

  public class NamedResourceWithETag : NamedResource
  {
    [Field(backendName: "etag")]
    public string ETag { get; set; }
  }

  public class SubResource : Resource {}

  public class NamedSubResourceWithETag : SubResource
  {
    [Field(backendName: "name")]
    public string Name { get; private set; }
    [Field(backendName: "etag")]
    public string ETag { get; set; }
  }

  public class PropertiesFormat : AmorphousTypedRow
  {
    public override Exception Validate(string targetName)
    {
      if (AmorphousDataEnabled && AmorphousData.Count > 0)
        return base.Validate(targetName);
      return base.Validate(targetName);
    }
  }

  public class NamedResourceWithProperties<TPropertiesFormat> : NamedResource
    where TPropertiesFormat : PropertiesFormat
  {
    [Field(backendName: "properties")]
    public TPropertiesFormat Properties { get; set; }
  }

  public class NamedResourceWithETagAndProperties<TPropertiesFormat> : NamedResourceWithETag
    where TPropertiesFormat : PropertiesFormat
  {
    [Field(backendName: "properties")]
    public TPropertiesFormat Properties { get; set; }
  }

  public class SubResourceWithProperties<TPropertiesFormat> : SubResource
    where TPropertiesFormat : PropertiesFormat
  {
    [Field(backendName: "properties")]
    public TPropertiesFormat Properties { get; set; }
  }

  public class NamedSubResourceWithETagAndProperties<TPropertiesFormat> : NamedSubResourceWithETag
    where TPropertiesFormat : PropertiesFormat
  {
    [Field(backendName: "properties")]
    public TPropertiesFormat Properties { get; set; }
  }
}
