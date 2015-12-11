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
using System.Collections.Concurrent;
using System.Linq;
using System.Threading;

using NFX.Environment;
using NFX.Financial;
using NFX.ServiceModel;
using NFX.ApplicationModel;

namespace NFX.Web.Pay.Tax
{
  /// <summary>
  /// Base class for ITaxCalcImplementation implementation.
  /// Instances of descendants of this class is typically created and configured in WebSettings class.
  /// Then particular tax calculator can be obtained from TaxCalculator.Instances[TAX_CALCULATOR_NAME] indexer
  /// </summary>
  public abstract class TaxCalculator : ITaxCalculatorImplementation, INamed, IWebClientCaller
  {
    #region Static

      private static Registry<TaxCalculator> s_Instances = new Registry<TaxCalculator>();

      /// <summary>
      /// Returns the read-only registry view of tax calculator currently present
      /// </summary>
      public static IRegistry<TaxCalculator> Instances { get { return s_Instances; } }

    #endregion

    public TaxCalculator(string name = null, IConfigSectionNode cfg = null)
    {
      var calculatorName = name;

      if (cfg != null)
      {
        Configure(cfg);
        calculatorName = cfg.AttrByName(Configuration.CONFIG_NAME_ATTR).Value;
      }

      if (name.IsNotNullOrWhiteSpace()) calculatorName = name;

      if (calculatorName.IsNullOrWhiteSpace()) calculatorName = GetType().Name;

      Name = calculatorName;

      if (!s_Instances.Register(this))
        throw new TaxException(StringConsts.TAX_CALCULATOR_DUPLICATE_NAME.Args(GetType().FullName, Name));
    }

    public static TTaxCalculator Make<TTaxCalculator>(string name, IConfigSectionNode node) where TTaxCalculator: TaxCalculator
    {
      WebSettings.RequireInitializedSettings();

      return FactoryUtils.MakeAndConfigure<TTaxCalculator>(node, typeof(TTaxCalculator), new object[] {name, node});
    }

    public static TTaxCalculator Make<TTaxCalculator>(string name, string cfgStr, string format = Configuration.CONFIG_LACONIC_FORMAT) where TTaxCalculator: TaxCalculator
    {
      WebSettings.RequireInitializedSettings();

      var cfg = Configuration.ProviderLoadFromString(cfgStr, format);
      return Make<TTaxCalculator>(name, cfg.Root);
    }

    #region Properties

      /// <summary>
      /// Provides textual name for the tax calculator
      /// </summary>
      public string Name
      {
          get { return m_Name ?? this.GetType().Name; }
          protected set { m_Name = value; }
      }



    #endregion

    public abstract ITaxCalculation Calc(IAddress wholesellerAddress, IAddress retailerAddress, IAddress shippingAddress);

    public void Configure(IConfigSectionNode node)
    {
    }

    #region IWebClientCaller

      public int WebServiceCallTimeoutMs
      {
        get { throw new NotImplementedException(); }
      }

      public bool KeepAlive
      {
        get { throw new NotImplementedException(); }
      }

      public bool Pipelined
      {
        get { throw new NotImplementedException(); }
      } 

    #endregion

    #region IInstrumentable

      public bool InstrumentationEnabled
      {
        get
        {
          throw new NotImplementedException();
        }
        set
        {
          throw new NotImplementedException();
        }
      }

      public IEnumerable<KeyValuePair<string, Type>> ExternalParameters
      {
        get { throw new NotImplementedException(); }
      }

      public IEnumerable<KeyValuePair<string, Type>> ExternalParametersForGroups(params string[] groups)
      {
        throw new NotImplementedException();
      }

      public bool ExternalGetParameter(string name, out object value, params string[] groups)
      {
        throw new NotImplementedException();
      }

      public bool ExternalSetParameter(string name, object value, params string[] groups)
      {
        throw new NotImplementedException();
      } 

    #endregion

    #region Pvt/Prot/Int Fields

      private string m_Name;
      
      private bool m_InstrumentationEnabled;
      private Time.Event m_InstrumentationEvent;

      private IConfigSectionNode m_DefaultSesssionConnParamsCfg;
      private PayConnectionParameters m_DefaultSessionConnectParams;

      protected internal readonly List<TaxSession> m_Sessions;

      private int m_WebServiceCallTimeoutMs;

    #endregion


    
  }
}
