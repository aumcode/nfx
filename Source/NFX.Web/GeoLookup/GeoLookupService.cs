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

using NFX.Log;
using NFX.DataAccess.CRUD;
using NFX.Environment;
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

      private Dictionary<string, IPAddressBlock> m_Blocks;
      private Dictionary<string, Location> m_Locations;

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
      public GeoEntity Lookup(string address)
      {
        if (Status!=ControlStatus.Active || address.IsNullOrWhiteSpace()) return null;
        

        var ip = address;//todo add DNS lookup

        var key = ip;
        var pad = string.Empty;
        IPAddressBlock block = null;
        
        while(true)
        {
          if (m_Blocks.TryGetValue(key+pad, out block)) break;
          
          var id = key.LastIndexOf('.');
          if (id>0)
          {
            key = key.Substring(0, id);
            pad+=".0";
          }
          else
            return null;
        }

        return new GeoEntity(address, block, LookupLocation(block.LocationID));
      }

      /// <summary>
      /// Tries to lookup location by id. Returns null if no match could be made
      /// </summary>
      public Location LookupLocation(string locationID)
      {
        if (Status!=ControlStatus.Active || locationID.IsNullOrWhiteSpace()) return null;
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

        var fnBlocks    = Path.Combine(m_DataPath, "GeoLite2-{0}-Blocks.csv".Args(m_Resolution));
        var fnLocations = Path.Combine(m_DataPath, "GeoLite2-{0}-Locations.csv".Args(m_Resolution));

        if (!File.Exists(fnBlocks))
            throw new GeoException(StringConsts.GEO_LOOKUP_SVC_DATA_FILE_ERROR.Args(fnBlocks));

        if (!File.Exists(fnLocations))
            throw new GeoException(StringConsts.GEO_LOOKUP_SVC_DATA_FILE_ERROR.Args(fnLocations));

        m_CancelStart = false;
        m_Blocks    = new Dictionary<string, IPAddressBlock>();
        m_Locations = new Dictionary<string, Location>();

        try
        {
            const int MAX_PARSE_ERRORS = 8;

            using(var fr = new StreamReader(new FileStream(fnBlocks, FileMode.Open, FileAccess.Read, FileShare.Read, 1024*1024)))
            {
              fr.ReadLine();//skip header
              var line = 1;
              var errors = 0;
              while(!fr.EndOfStream && !m_CancelStart && App.Active)
              {
                IPAddressBlock row;
                try
                {
                  row = parseBlock( fr.ReadLine());
                  m_Blocks.Add( row.IPBlockStart, row );
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
        
            using(var fr = new StreamReader(fnLocations))
            {
              fr.ReadLine();//skip header
              var line = 1;
              var errors = 0;
              while(!fr.EndOfStream && !m_CancelStart && App.Active)
              {
                Location row;
                try
                {
                  row =  parseLocation( fr.ReadLine()); 
                  m_Locations.Add( row.ID, row );
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

      private IPAddressBlock parseBlock(string data)
      {
        var row = new IPAddressBlock();
        if (data.IsNullOrWhiteSpace()) return row;

        var i = 0;
        var fc = row.Schema.FieldCount;
        foreach(var seg in parseCSVLine(data))
        {
          if (i==fc) break;
          
          var v = seg;
          if (i==0)
          {
            if (v.StartsWith("::ffff:")) 
              v = v.Substring("::ffff:".Length);
          }  

          if (row.Schema[i].Type==typeof(Boolean)) v = v=="1"?"true":"false";
          try
          {
            row[i] = v;
          }
          catch(Exception error)
          {
            throw new GeoException("Block {0}.{1}[{2}] = '{3}' error: {4} ".Args(
                                   row.Schema.Name,
                                   row.Schema[i].Name,
                                   i,
                                   v,
                                   error.ToMessageWithType()), error); 
          }
          i++;
        }
        return row;
      }

      private Location parseLocation(string data)
      {
        var row = new Location();
        if (data.IsNullOrWhiteSpace()) return row;
        
        var i = 0;
        var fc = row.Schema.FieldCount; 
        foreach(var seg in parseCSVLine(data))
        {
          if (i==fc) break;
          var v = seg;

          if (row.Schema[i].Type==typeof(Boolean)) v = v=="1"?"true":"false";
          try
          {
            row[i] = v;
          }
          catch(Exception error)
          {
            throw new GeoException("Location {0}.{1}[{2}] = '{3}' error: {4} ".Args(
                                   row.Schema.Name,
                                   row.Schema[i].Name,
                                   i,
                                   v,
                                   error.ToMessageWithType()), error); 
          }
          i++;
        }
        return row;
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
   
  }

}
