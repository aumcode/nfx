/*<FILE_LICENSE>
* NFX (.NET Framework Extension) Unistack Library
* Copyright 2003-2017 ITAdapter Corp. Inc.
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

using NFX.Environment;
using NFX.Serialization.JSON;

namespace NFX.Web.Shipping
{
  public class ShippingCarrier : IConfigurable
  {
    #region Inner classes

    public class Method : IConfigurable, INamed
    {
      public Method(ShippingCarrier carrier)
      {
        if (carrier==null) throw new ShippingException("Method.ctor(carrier=null)");

        m_Carrier = carrier;
      }

      private readonly ShippingCarrier m_Carrier;

      /// <summary>
      /// inner (NFX) carrier ID
      /// </summary>
      [Config] public string InnerID { get; set; }
      /// <summary>
      /// outer system specific carrier ID
      /// </summary>
      [Config] public string OuterID { get; set; }
      [Config] public NLSMap NLSName { get; set; }
      [Config] public PriceCategory PriceCategory { get; set; }
      [Config] public bool IncludeBusinessDays { get; set; }
      [Config] public bool IncludeSaturdays { get; set; }
      [Config] public bool IncludeSundays { get; set; }
      [Config] public bool Trackable { get; set; }
      [Config] public bool Insurance { get; set; }

      public ShippingCarrier Carrier { get { return m_Carrier; } }
      string INamed.Name { get { return InnerID; } }

      public void Configure(IConfigSectionNode node)
      {
        ConfigAttribute.Apply(this, node);

        NLSName = new NLSMap(node[CONFIG_NLS_SECTION]);
      }
    }

    public class Template : IConfigurable, INamed
    {
      public Template(ShippingCarrier carrier)
      {
        if (carrier==null) throw new ShippingException("Template.ctor(carrier=null)");

        m_Carrier = carrier;
      }

      private readonly ShippingCarrier m_Carrier;

      /// <summary>
      /// inner (NFX) carrier ID
      /// </summary>
      [Config] public string InnerID { get; set; }
      /// <summary>
      /// outer system specific carrier ID
      /// </summary>
      [Config] public string OuterID { get; set; }
      [Config] public NLSMap NLSName { get; set; }

      public ShippingCarrier Carrier { get { return m_Carrier; } }
      string INamed.Name { get { return InnerID; } }


      public void Configure(IConfigSectionNode node)
      {
        ConfigAttribute.Apply(this, node);

        NLSName = new NLSMap(node[CONFIG_NLS_SECTION]);
      }
    }

    #endregion

    #region CONST

    public const string CONFIG_METHODS_SECTION = "methods";
    public const string CONFIG_METHOD_SECTION = "method";
    public const string CONFIG_TEMPLATES_SECTION = "templates";
    public const string CONFIG_TEMPLATE_SECTION = "template";
    public const string CONFIG_NLS_SECTION = "nls-name";

    #endregion

    public ShippingCarrier(ShippingSystem shippingSystem)
    {
      if (shippingSystem==null) throw new ShippingException("Carrier.ctor(shippingSystem=null)");

      m_ShippingSystem = shippingSystem;
      m_Methods = new Registry<Method>();
      m_Templates = new Registry<Template>();
    }

    private readonly ShippingSystem m_ShippingSystem;
    private readonly Registry<Method> m_Methods;
    private readonly Registry<Template> m_Templates;

    public ShippingSystem ShippingSystem { get { return m_ShippingSystem; } }

    [Config("$carrier-type")] public CarrierType Type { get; set; }
    /// <summary>
    /// inner (NFX) carrier ID
    /// </summary>
    [Config] public string InnerID { get; set; }
    /// <summary>
    /// outer system specific carrier ID
    /// </summary>
    [Config] public string OuterID { get; set; }
    [Config] public NLSMap NLSName { get; set; }

    public byte[] Logo { get; set; }

    public IRegistry<Method> Methods { get { return m_Methods; } }
    public IRegistry<Template> Templates { get { return m_Templates; } }


    public void Configure(IConfigSectionNode node)
    {
      ConfigAttribute.Apply(this, node);

      var mnodes = node[CONFIG_METHODS_SECTION].Children.Where(n=>n.IsSameName(CONFIG_METHOD_SECTION));
      foreach(var mnode in mnodes)
      {
          var method = FactoryUtils.MakeAndConfigure<Method>(mnode, typeof(Method), new object[] { this });
          m_Methods.Register(method);
      }

      var templates = new List<Template>();
      var tnodes = node[CONFIG_TEMPLATES_SECTION].Children.Where(n=>n.IsSameName(CONFIG_TEMPLATE_SECTION));
      foreach(var tnode in tnodes)
      {
          var template = FactoryUtils.MakeAndConfigure<Template>(tnode, typeof(Template), new object[] { this });
          m_Templates.Register(template);
      }

      NLSName = new NLSMap(node[CONFIG_NLS_SECTION]);
    }
  }

  public class Shipment
  {
    public decimal Length { get; set; }
    public decimal Width { get; set; }
    public decimal Height { get; set; }
    public DistanceUnit DistanceUnit { get; set; }

    public decimal Weight { get; set; }
    public MassUnit MassUnit { get; set; }

    public ShippingCarrier Carrier { get; set; }
    public ShippingCarrier.Method Method { get; set; }
    public ShippingCarrier.Template Template { get; set; }

    public LabelFormat LabelFormat { get; set; }
    public string LabelIDForReturn { get; set; }

    public Address FromAddress { get; set; }
    public Address ToAddress { get; set; }
    public Address ReturnAddress { get; set; }
  }
}
