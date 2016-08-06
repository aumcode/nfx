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

using NFX.Serialization.JSON;
using NFX.Environment;
using NFX.Wave.Templatization;
using ErrorPage=NFX.Wave.Templatization.StockContent.Error;

namespace NFX.Wave.Filters
{
  /// <summary>
  /// Provides base for filters that have before/after semantics
  /// </summary>
  public abstract class BeforeAfterFilterBase : WorkFilter
  {
    #region CONSTS
      public const string CONFIG_BEFORE_SECTION = "before";
      public const string CONFIG_AFTER_SECTION = "after";

    #endregion

    #region .ctor
      public BeforeAfterFilterBase(WorkDispatcher dispatcher, string name, int order) : base(dispatcher, name, order)
      {
      }

      public BeforeAfterFilterBase(WorkDispatcher dispatcher, IConfigSectionNode confNode): base(dispatcher, confNode)
      {
        configureMatches(confNode);
      }

      public BeforeAfterFilterBase(WorkHandler handler, string name, int order) : base(handler, name, order)
      {
      }

      public BeforeAfterFilterBase(WorkHandler handler, IConfigSectionNode confNode): base(handler, confNode)
      {
        configureMatches(confNode);
      }

    #endregion

    #region Fields

      private OrderedRegistry<WorkMatch> m_BeforeMatches = new OrderedRegistry<WorkMatch>();
      private OrderedRegistry<WorkMatch> m_AfterMatches = new OrderedRegistry<WorkMatch>();

    #endregion

    #region Properties

      /// <summary>
      /// Returns matches used by the handler to determine whether match should be made before the work processing
      /// </summary>
      public OrderedRegistry<WorkMatch> BeforeMatches { get{ return m_BeforeMatches;}}

      /// <summary>
      /// Returns matches used by the handler to determine whether match should be made before the work processing
      /// </summary>
      public OrderedRegistry<WorkMatch> AfterMatches { get{ return m_AfterMatches;}}

    #endregion

    #region Protected
      protected sealed override void DoFilterWork(WorkContext work, IList<WorkFilter> filters, int thisFilterIndex)
      {
         if (m_BeforeMatches.Count>0)
          foreach(var match in m_BeforeMatches.OrderedValues)
          {
            var matched = match.Make(work);
            if (matched!=null)
            {
              DoBeforeWork(work, matched);
              break;
            }
          }

         this.InvokeNextWorker(work, filters, thisFilterIndex);

         if (m_AfterMatches.Count>0)
          foreach(var match in m_AfterMatches.OrderedValues)
          {
            var matched = match.Make(work);
            if (matched!=null)
            {
              DoAfterWork(work, matched);
              break;
            }
          }
      }

      /// <summary>
      /// Override to do the work when one of the BeforeMatches was matched
      /// </summary>
      protected abstract void DoBeforeWork(WorkContext work, JSONDataMap matched);

      /// <summary>
      /// Override to do the work when one of the AfterMatches was matched
      /// </summary>
      protected abstract void DoAfterWork(WorkContext work, JSONDataMap matched);

    #endregion

    #region .pvt

      private void configureMatches(IConfigSectionNode confNode)
      {
        foreach(var cn in confNode[CONFIG_BEFORE_SECTION].Children.Where(cn=>cn.IsSameName(WorkMatch.CONFIG_MATCH_SECTION)))
          if(!m_BeforeMatches.Register( FactoryUtils.Make<WorkMatch>(cn, typeof(WorkMatch), args: new object[]{ cn })) )
            throw new WaveException(StringConsts.CONFIG_OTHER_DUPLICATE_MATCH_NAME_ERROR.Args(cn.AttrByName(Configuration.CONFIG_NAME_ATTR).Value, "{0}.BeforeMatches".Args(GetType().FullName)));

        foreach(var cn in confNode[CONFIG_AFTER_SECTION].Children.Where(cn=>cn.IsSameName(WorkMatch.CONFIG_MATCH_SECTION)))
          if(!m_AfterMatches.Register( FactoryUtils.Make<WorkMatch>(cn, typeof(WorkMatch), args: new object[]{ cn })) )
            throw new WaveException(StringConsts.CONFIG_OTHER_DUPLICATE_MATCH_NAME_ERROR.Args(cn.AttrByName(Configuration.CONFIG_NAME_ATTR).Value, "{0}.AfterMatches".Args(GetType().FullName)));
      }

    #endregion
  }

}
