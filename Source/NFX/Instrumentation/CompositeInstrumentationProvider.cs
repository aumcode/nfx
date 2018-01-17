/*<FILE_LICENSE>
* NFX (.NET Framework Extension) Unistack Library
* Copyright 2003-2018 Agnicore Inc. portions ITAdapter Corp. Inc.
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
using NFX.Log;

namespace NFX.Instrumentation
{
  /// <summary>
  /// Represents a provider that writes aggregated datums to multiple destination provider
  /// </summary>
  public class CompositeInstrumentationProvider : InstrumentationProvider
  {
    #region .ctor
    public CompositeInstrumentationProvider(InstrumentationService director) : base(director) { }
    #endregion

    #region Fields
    private List<InstrumentationProvider> m_Providers = new List<InstrumentationProvider>();
    #endregion

    #region Properties
    /// <summary>
    /// Returns destinations that this destination wraps. This call is thread safe
    /// </summary>
    public IEnumerable<InstrumentationProvider> Providers
    {
      get { lock (m_Providers) return m_Providers.ToList(); }
    }
    #endregion

    #region Public
    public void RegisterProvider(InstrumentationProvider provider)
    {
      lock (m_Providers)
      {
        if (!m_Providers.Contains(provider))
          m_Providers.Add(provider);
      }
    }

    public bool UnRegisterProvider(InstrumentationProvider provider)
    {
      lock (m_Providers)
      {
        return m_Providers.Remove(provider);
      }
    }
    #endregion

    #region Protected
    protected override void DoConfigure(IConfigSectionNode node)
    {
      base.DoConfigure(node);

      foreach (var dnode in node.Children.Where(n => n.Name.EqualsIgnoreCase(InstrumentationService.CONFIG_PROVIDER_SECTION)))
      {
        var dest = FactoryUtils.MakeAndConfigure(dnode, args: new[] { ComponentDirector }) as InstrumentationProvider;
        this.RegisterProvider(dest);
      }
    }

    protected internal override object BeforeBatch()
    {
      var dict = new Dictionary<string, object>(m_Providers.Count);
      lock (m_Providers)
        foreach (var provider in m_Providers)
        {
          var batchContext = provider.BeforeBatch();
          if (batchContext != null)
            dict.Add(provider.Name, batchContext);
        }
      return dict;
    }

    protected internal override void AfterBatch(object batchContext)
    {
      var dict = batchContext as Dictionary<string, object>;
      lock (m_Providers)
        foreach (var provider in m_Providers)
        {
          object providerBatchContext = null;
          if (dict != null)
            dict.TryGetValue(provider.Name, out providerBatchContext);
          provider.AfterBatch(providerBatchContext);
        }
    }

    protected internal override object BeforeType(Type type, object batchContext)
    {
      var batchDict = batchContext as Dictionary<string, object>;
      var dict = new Dictionary<string, object>(m_Providers.Count);
      lock (m_Providers)
        foreach (var provider in m_Providers)
        {
          object providerBatchContext = null;
          if (batchDict != null)
            batchDict.TryGetValue(provider.Name, out providerBatchContext);
          var typeContext = provider.BeforeType(type, providerBatchContext);
          if (batchContext != null)
            dict.Add(provider.Name, typeContext);
        }
      return dict;
    }

    protected internal override void AfterType(Type type, object batchContext, object typeContext)
    {
      var batchDict = batchContext as Dictionary<string, object>;
      var dict = typeContext as Dictionary<string, object>;
      lock (m_Providers)
        foreach (var provider in m_Providers)
        {
          object providerBatchContext = null;
          if (batchDict != null)
            batchDict.TryGetValue(provider.Name, out providerBatchContext);
          object providerTypeContext = null;
          if (dict != null)
            dict.TryGetValue(provider.Name, out providerTypeContext);
          provider.AfterType(type, providerBatchContext, providerTypeContext);
        }
    }

    protected internal override void Write(Datum aggregatedDatum, object batchContext, object typeContext)
    {
      var batchDict = batchContext as Dictionary<string, object>;
      var typeDict = typeContext as Dictionary<string, object>;
      lock (m_Providers)
        foreach (var provider in m_Providers)
          try
          {
            object providerBatchContext = null;
            if (batchDict != null)
              batchDict.TryGetValue(provider.Name, out providerBatchContext);
            object providerTypeContext = null;
            if (typeDict != null)
              typeDict.TryGetValue(provider.Name, out providerTypeContext);
            provider.Write(aggregatedDatum, providerBatchContext, providerTypeContext);
          }
          catch (Exception error)
          {
            ComponentDirector.Log(MessageType.Error, GetType().Name + ".Write", error.ToMessageWithType(), error);
          }
    }
    #endregion
  }
}
