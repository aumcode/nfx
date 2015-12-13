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
using System.Threading;
using System.Diagnostics;

using NFX;
using NFX.Environment;
using NFX.Security;

namespace NFX.Web.Pay.Tax
{
  public class TaxConnectionParameters: INamed, IConfigurable
  {
    #region Static

      public static TParams Make<TParams>(IConfigSectionNode node) where TParams: TaxConnectionParameters
      {
        return FactoryUtils.MakeAndConfigure<TParams>(node, typeof(TParams), args: new object[] {node});
      }

      public static TParams Make<TParams>(string connStr, string format = Configuration.CONFIG_LACONIC_FORMAT)
        where TParams: TaxConnectionParameters
      {
        var cfg = Configuration.ProviderLoadFromString(connStr, format).Root;
        return Make<TParams>(cfg);
      }

    #endregion

    #region ctor

		  public TaxConnectionParameters() {}

      public TaxConnectionParameters(IConfigSectionNode node) { Configure(node); }

      public TaxConnectionParameters(string connStr, string format = Configuration.CONFIG_LACONIC_FORMAT)
      {
        var conf = Configuration.ProviderLoadFromString(connStr, format).Root;
      } 

	  #endregion

    #region Properties

		  [Config] public string Name { get; set; }

	  #endregion

    #region Protected

		  public virtual void Configure(IConfigSectionNode node)
      {
        ConfigAttribute.Apply(this, node);
      }

	  #endregion
  } //TaxConnectionParameters

}
