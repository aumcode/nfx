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
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;

using NFX.Log;
using NFX.Environment;
using NFX.Parsing;
using NFX.ServiceModel;

namespace NFX.Web.GeoLookup
{
  /// <summary>
  /// Represents a service that can lookup country/city names by  domainnames/ip addresses.
  /// Thehis implementatuion uses free data from: http://dev.maxmind.com/geoip/geoip2/geolite2/.
  /// Must include MaxMind attribution on the public site that uses this data (see License section on maxmind.com)
  /// </summary>
  public class GeoLookupService : Service, IGeoLookup
  {
    #region CONSTS

      public const string CONFIG_GEO_LOOKUP_SECTION = "geo-lookup";

    #endregion

    #region Static

      private static object s_InstanceLock = new object();
      private static GeoLookupService s_Instance;


      /// <summary>
      /// Lazily creates the default service instance
      /// </summary>
      public static IGeoLookup Instance
      {
        get
        {
          if (s_Instance!=null) return s_Instance;
          lock(s_InstanceLock)
          {
            if (s_Instance==null)
            {
              s_Instance = new GeoLookupService();
              try
              {
               s_Instance.Configure(null);
              }
              catch
              {
                s_Instance = null;
                throw;
              }
              Task.Factory.StartNew(()=>start(), TaskCreationOptions.LongRunning);
            }
            return s_Instance;
          }
        }
      }

      private static void start()
      {
        try
        {
          s_Instance.Start();
        }
        catch(Exception error)
        {
          s_Instance = null;

          log(MessageType.CatastrophicError,
              "Instance.get(){svc.start()}",
              error.ToMessageWithType(), error);
        }
      }

    #endregion

    #region .ctor

      public GeoLookupService() : base() { }
      public GeoLookupService(object director) : base(director) { }

    #endregion

    #region Fields

      private bool m_CancelStart;
      private string m_DataPath;
      private LookupResolution m_Resolution = LookupResolution.Country;

      private SubnetTree<IPSubnetBlock> m_SubnetBST;
      private Dictionary<SealedString, Location> m_Locations;

    #endregion

    #region Properties


      /// <summary>
      /// Returns true to indoicate that service has loaded and ready to serve data
      /// </summary>
      public bool Available { get{ return Status== ControlStatus.Active;} }


      /// <summary>
      /// Specifies where the data is
      /// </summary>
      [Config]
      public string DataPath
      {
        get { return m_DataPath ?? string.Empty; }
        set
        {
          CheckServiceInactive();
          m_DataPath = value;
        }
      }

      /// <summary>
      /// Specifies what resolution service provides
      /// </summary>
      [Config]
      public LookupResolution Resolution
      {
        get { return m_Resolution; }
        set
        {
          CheckServiceInactive();
          m_Resolution = value;
        }
      }

      /// <summary>
      /// Returns true to indicate that previous attempt to start service - load data, was canceled
      /// </summary>
      public bool StartCanceled { get{ return m_CancelStart; } }

    #endregion

    #region Public

      /// <summary>
      /// Tries to lookup the location by ip/dns name. Returns null if no match could be made
      /// </summary>
      public GeoEntity Lookup(IPAddress address)
      {
        if (Status!=ControlStatus.Active || address==null) return null;

        var block = m_SubnetBST[new Subnet(address)];
        Location location;
        m_Locations.TryGetValue(block.LocationID, out location);
        return new GeoEntity(address, block, location);
      }

      /// <summary>
      /// Cancels service start. This method may be needed when Start() blocks for a long time due to large volumes of data
      /// </summary>
      public void CancelStart()
      {
        m_CancelStart = true;
      }

    #endregion

    #region Protected


      protected override void DoConfigure(IConfigSectionNode node)
      {
        if (node==null)
        {
          node = App.ConfigRoot[CONFIG_GEO_LOOKUP_SECTION];
          if (node.Exists)
            ConfigAttribute.Apply(this, node);
        }
      }


      protected override void DoStart()
      {
        if (m_Resolution!= LookupResolution.Country && m_Resolution!= LookupResolution.City)
          throw new GeoException(StringConsts.GEO_LOOKUP_SVC_RESOLUTION_ERROR.Args(m_Resolution));

        if (!Directory.Exists(m_DataPath))
          throw new GeoException(StringConsts.GEO_LOOKUP_SVC_PATH_ERROR.Args(m_DataPath ?? StringConsts.UNKNOWN));

        var fnBlocks    = Path.Combine(m_DataPath, "GeoLite2-{0}-Blocks-IPv6.csv".Args(m_Resolution));
        var fnBlocksV4 = Path.Combine(m_DataPath, "GeoLite2-{0}-Blocks-IPv4.csv".Args(m_Resolution));
        var fnLocations = Path.Combine(m_DataPath, "GeoLite2-{0}-Locations-en.csv".Args(m_Resolution));

        if (!File.Exists(fnBlocks))
            throw new GeoException(StringConsts.GEO_LOOKUP_SVC_DATA_FILE_ERROR.Args(fnBlocks));
        if (!File.Exists(fnBlocksV4))
            throw new GeoException(StringConsts.GEO_LOOKUP_SVC_DATA_FILE_ERROR.Args(fnBlocksV4));
        if (!File.Exists(fnLocations))
            throw new GeoException(StringConsts.GEO_LOOKUP_SVC_DATA_FILE_ERROR.Args(fnLocations));

        m_CancelStart = false;
        m_Locations = new Dictionary<SealedString, Location>();

        try
        {
            const int MAX_PARSE_ERRORS = 8;

            var tree = new BinaryTree<Subnet, IPSubnetBlock>();
            var scope = new SealedString.Scope();
            foreach (var blocksFn in new[] { fnBlocks, fnBlocksV4 })
            {
              using (var stream = new FileStream(blocksFn, FileMode.Open, FileAccess.Read, FileShare.Read, 4*1024*1024))
              {
                int errors = 0;
                try
                {
                  foreach (var row in stream.AsCharEnumerable().ParseCSV(skipHeader: true, columns: 10))
                  {
                    if (m_CancelStart || !App.Active) break;
                    var arr = row.ToArray();
                    var block = new IPSubnetBlock(
                      scope.Seal(arr[0]),
                      scope.Seal(arr[1]),
                      scope.Seal(arr[2]),
                      scope.Seal(arr[3]),
                      arr[4].AsBool(),
                      arr[5].AsBool(),
                      scope.Seal(arr[6]),
                      arr[7].AsFloat(),
                      arr[8].AsFloat());

                    tree[new Subnet(block.Subnet.Value, true)] = block;
                  }
                }
                catch (Exception error)
                {
                  log(MessageType.Error, "DoStart('{0}')".Args(blocksFn), "Line: {0} {1}".Args(0/*line*/, error.ToMessageWithType()), error);
                  errors++;
                  if (errors > MAX_PARSE_ERRORS)
                  {
                    log(MessageType.CatastrophicError, "DoStart('{0}')".Args(blocksFn), "Errors > {0}. Aborting file '{1}' import".Args(MAX_PARSE_ERRORS, blocksFn));
                    break;
                  }
                }
              }
            }
            m_SubnetBST = new SubnetTree<IPSubnetBlock>(tree.BuildIndex());

            using (var stream = new FileStream(fnLocations, FileMode.Open, FileAccess.Read, FileShare.Read, 4 * 1024 * 1024))
            {
              try
              {
                foreach (var row in stream.AsCharEnumerable().ParseCSV(skipHeader: true, columns: 13))
                {
                  if (m_CancelStart || !App.Active) break;
                  var arr = row.ToArray();
                  var location = new Location(
                    scope.Seal(arr[0]),
                    scope.Seal(arr[1]),
                    scope.Seal(arr[2]),
                    scope.Seal(arr[3]),
                    scope.Seal(arr[4]),
                    scope.Seal(arr[5]),
                    scope.Seal(arr[6]),
                    scope.Seal(arr[7]),
                    scope.Seal(arr[8]),
                    scope.Seal(arr[9]),
                    scope.Seal(arr[10]),
                    scope.Seal(arr[11]),
                    scope.Seal(arr[12]));
                  m_Locations.Add(location.ID, location);
                }
              }
              catch (CSVParserException error)
              {
                log(MessageType.Error, "DoStart('{0}')".Args(fnLocations), "Line: {0} Column: {1} {2}".Args(error.Line, error.Column, error.ToMessageWithType()), error);
              }
              catch (Exception error)
              {
                log(MessageType.Error, "DoStart('{0}')".Args(fnLocations), "{1}".Args(error.ToMessageWithType()), error);
              }
            }
        }
        catch
        {
          m_SubnetBST = null;
          m_Locations = null;
          m_SubnetBST = null;
          throw;
        }

        if (m_CancelStart) throw new GeoException(StringConsts.GEO_LOOKUP_SVC_CANCELED_ERROR);
      }

      protected override void DoWaitForCompleteStop()
      {
        m_SubnetBST = null;
        m_Locations = null;
      }

    #endregion

    #region .pvt
      private static void log(MessageType type, string from, string text, Exception error = null)
      {
         var msg = new Message
          {
            Type = MessageType.CatastrophicError,
            Topic = StringConsts.WEB_LOG_TOPIC,
            From = "{0}.{1}".Args(typeof(GeoLookupService).FullName, from),
            Text = text,
            Exception = error
          };
          App.Log.Write( msg );
      }

    #endregion
  }
}
