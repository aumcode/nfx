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
using System.IO;
using System.Threading.Tasks;
using System.Net;


using NFX.Log;
using NFX.DataAccess.CRUD;
using NFX.Environment;
using NFX.ServiceModel;
using System.Net.Sockets;
using System.Diagnostics;

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

      private IPSubnetBlock[] m_Blocks;
      private Dictionary<string, Location> m_Locations;
      private BinaryRecord[] m_BST;
      private uint m_OffsetV4;

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

        var subnet = new Subnet("5.11.16.0/32");
        var record = subnet.address.AddressFamily == AddressFamily.InterNetwork ? m_OffsetV4 : 0;
        var index = -1;
        foreach (var bit in subnet)
        {          
          int i = m_BST[record].Index;
          if (i >= 0)
            index = i;
          record = bit ? m_BST[record].Right : m_BST[record].Left;
          if (record == 0)
            break;
        }
        if (index < 0)
          return null;
        var block = m_Blocks[index]; 
        return new GeoEntity(address, block, LookupLocation(block.LocationID.Value));
      }
  
      /// <summary>
      /// Tries to lookup location by id. Returns null if no match could be made
      /// </summary>
      public Location? LookupLocation(string locationID)
      {
        if (Status!=ControlStatus.Active || string.IsNullOrEmpty(locationID)) return null;
        Location result;
        if (m_Locations.TryGetValue(locationID, out result)) return result;
        return null;
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
        m_Locations = new Dictionary<string, Location>();

        try
        {
            const int MAX_PARSE_ERRORS = 8;

            var blocks = new List<IPSubnetBlock>();
            var tree = new Tree();
            using (var fr = new StreamReader(new FileStream(fnBlocks, FileMode.Open, FileAccess.Read, FileShare.Read, 1024*1024)))
            {
              fr.ReadLine();//skip header
              var line = 1;
              var errors = 0;
              while(!fr.EndOfStream && !m_CancelStart && App.Active)
              {
                try
                {
                  var rowLine = fr.ReadLine();
                  var row = parseBlock(rowLine);
                  if (row.HasValue)
                  {
                    tree.Insert(row.Value.Subnet, blocks.Count);
                    blocks.Add(row.Value);
                  }
                }
                catch(Exception error)
                {
                  log(MessageType.Error, "DoStart('{0}')".Args(fnBlocks), "Line: {0} {1}".Args(line, error.ToMessageWithType()), error);
                  errors++;
                  if (errors>MAX_PARSE_ERRORS)
                  {
                    log(MessageType.CatastrophicError, "DoStart('{0}')".Args(fnBlocks), "Errors > {0}. Aborting file '{1}' import".Args(MAX_PARSE_ERRORS, fnBlocks));
                    break;
                  }
                }
                line++;
              }
            }
            using (var fr = new StreamReader(new FileStream(fnBlocksV4, FileMode.Open, FileAccess.Read, FileShare.Read, 1024*1024)))
            {
              fr.ReadLine();//skip header
              var line = 1;
              var errors = 0;
              while(!fr.EndOfStream && !m_CancelStart && App.Active)
              {
                try
                {
                  if (line == 48265)
                    ((Action)(() => { }))();
                  var rowLine = fr.ReadLine();
                  var row = parseBlock(rowLine);
                  if (row.HasValue)
                  {
                    tree.Insert(row.Value.Subnet, blocks.Count);
                    blocks.Add(row.Value);
                  }
                }
                catch(Exception error)
                {
                  log(MessageType.Error, "DoStart('{0}')".Args(fnBlocksV4), "Line: {0} {1}".Args(line, error.ToMessageWithType()), error);
                  errors++;
                  if (errors>MAX_PARSE_ERRORS)
                  {
                    log(MessageType.CatastrophicError, "DoStart('{0}')".Args(fnBlocksV4), "Errors > {0}. Aborting file '{1}' import".Args(MAX_PARSE_ERRORS, fnBlocksV4));
                    break;
                  }
                }
                line++;
              }
            }
            m_BST = tree.Build(out m_OffsetV4);
            m_Blocks = blocks.ToArray();

            using(var fr = new StreamReader(fnLocations))
            {
              fr.ReadLine();//skip header
              var line = 1;
              var errors = 0;
              while(!fr.EndOfStream && !m_CancelStart && App.Active)
              {
                try
                {
                  var row =  parseLocation( fr.ReadLine()); 
                  if (row.HasValue)
                    m_Locations.Add(row.Value.ID.Value, row.Value);
                }
                catch(Exception error)
                {
                  log(MessageType.Error, "DoStart('{0}')".Args(fnLocations), "Line: {0} {1}".Args(line, error.ToMessageWithType()), error);
                  errors++;
                  if (errors>MAX_PARSE_ERRORS)
                  {
                    log(MessageType.CatastrophicError, "DoStart('{0}')".Args(fnLocations), "Errors > {0}. Aborting file '{1}' import".Args(MAX_PARSE_ERRORS, fnLocations));
                    break;
                  }
                }
                line++;
              }
            }  
        }
        catch
        {
          m_Blocks = null;
          m_Locations = null;
          m_BST = null;
          throw;
        }

        if (m_CancelStart) throw new GeoException(StringConsts.GEO_LOOKUP_SVC_CANCELED_ERROR);
      }

      protected override void DoWaitForCompleteStop()
      {
        m_Blocks = null;
        m_Locations = null;
      }

    #endregion

    #region .pvt

      private IPSubnetBlock? parseBlock(string data)
      {
        if (data.IsNullOrWhiteSpace()) return null;

        var arr = parseCSVLine(data).ToArray();
        return new IPSubnetBlock(new Subnet(arr[0]),
          arr[1],
          arr[2],
          arr[3],
          arr[4] == "1",
          arr[5] == "1",
          arr[6],
          string.IsNullOrEmpty(arr[7]) ? 0f : float.Parse(arr[7]),
          string.IsNullOrEmpty(arr[8]) ? 0f : float.Parse(arr[8]));
      }

      private Location? parseLocation(string data)
      {
        if (data.IsNullOrWhiteSpace()) return null;
        
        var arr = parseCSVLine(data).ToArray();
        return new Location(
          arr[0],
          arr[1],
          arr[2],
          arr[3],
          arr[4],
          arr[5],
          arr[6],
          arr[7],
          arr[8],
          arr[9],
          arr[10],
          arr[11],
          arr[12]);
      }


        /// <summary>
        /// Lazily parses CSV data respecting the ""
        /// </summary>
        public static IEnumerable<string> parseCSVLine(string line)
        {
          if (line.IsNullOrWhiteSpace()) yield break;
         
          var sb = new StringBuilder();
          var quote = false;
          for(var i=0;i<line.Length;i++)
          {
            var c = line[i];
            if (c==',' && !quote)
            {
              yield return sb.ToString();
              sb.Clear();
              continue;
            }
            if (c=='"' && !quote)
            {
              quote = true;
              continue;
            }
            if (c=='"' && quote)
            {
              quote = false;
              continue;
            }

            sb.Append(line[i]);
          }

          yield return sb.ToString();
        }

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

    #region BST
      public struct BinaryRecord
      {
        public int Index;
        public uint Left;
        public uint Right;
      }

      public class Node
      {
        public Node Left { get; set; }
        public Node Right { get; set; }
        public int Index { get; set; }
        public uint Number { get; set; }
        public uint Level { get; set; }
        public Node()
        {
          Index = -1;
        }
      }

      public class Tree
      {
        public Node Root { get; private set; } = new Node();
        public Node RootV4 { get; private set; }
        private uint m_Count = 0;

        public Tree()
        {
          RootV4 = findNode(Root, ref m_Count, new Subnet(IPAddress.Any.MapToIPv6(), 96));
        }

        public void Insert(Subnet network, int index)
        {
          Node node;
          switch (network.address.AddressFamily)
          {
            case AddressFamily.InterNetwork:
              node = findNode(RootV4, ref m_Count, network);
              break;
            case AddressFamily.InterNetworkV6:
              node = findNode(Root, ref m_Count, network);
              break;
            default:
              throw new NotImplementedException();
          }
          if (node.Index != -1)
            ((Action)(() => { }))();
          node.Index = index;
        }

        private static Node findNode(Node root, ref uint count, Subnet network)
        {
          var node = root;

          uint i = 0;
          foreach (var bit in network)
          {
            i++;
            if (node.Index >= 0)
              ((Action)(() => { }))();
            var next = bit ? node.Right : node.Left;

            if (next == null)
            {
              next = new Node { Number = count++, Level = i };
              if (bit) node.Right = next;
              else node.Left = next;
            }
            node = next;
          }

          return node;
        }

        public BinaryRecord[] Build(out uint offsetV4)
        {
          offsetV4 = RootV4.Number;
          var binaryTree = new BinaryRecord[m_Count];
          iterate(Root, (node, index) =>
          {
            binaryTree[node.Number].Index = index;
            binaryTree[node.Number].Left = node.Left != null ? node.Left.Number : 0;
            binaryTree[node.Number].Right = node.Right != null ? node.Right.Number : 0;
          });
          return binaryTree;
        }

        private void iterate(Node node, Action<Node, int> callback)
        {
          if (node != null)
          {
            callback(node, node.Index);
            iterate(node.Left, callback);
            iterate(node.Right, callback);
          }
        }
      }

    #endregion

  }

}
