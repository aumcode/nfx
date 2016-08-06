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


/* NFX by ITAdapter
 * Originated: 2006.01
 * Revision: NFX 0.8  2010.09.20
 */
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using System.Runtime.Serialization;
using System.Runtime.Serialization.Formatters.Binary;
using System.Diagnostics;
using System.Threading.Tasks;
using System.Threading;


using NFX.Log;
using NFX.Environment;
using NFX.ServiceModel;
using NFX.Serialization;
using NFX.Serialization.Slim;

namespace NFX.ApplicationModel.Volatile
{
  /// <summary>
  /// Format of files on disk
  /// </summary>
  public enum FileObjectFormat {Slim = 0, MSBinary }


  /// <summary>
  /// Defines a file-based provider that stores objects for ObjectStoreService class
  /// </summary>
  public class FileObjectStoreProvider : ObjectStoreProvider
  {
    #region CONSTS

        public const string FROM = "FileObjectStoreProvider";

        public const string CONFIG_LOAD_LIMIT_ATTR = "load-limit";
        public const string CONFIG_ROOT_PATH_ATTR = "root-path";
        public const string CONFIG_FORMAT_ATTR = "format";

        public const string CONFIG_KNOWN_TYPES_SECTION = "known-types";
        public const string CONFIG_KNOWN_SECTION = "known";
        public const string CONFIG_TYPE_ATTR = "type";

        public const long MAX_LOAD_LIMIT = 8L/*gb*/ * 1024/*mb*/ * 1024/*k bytes */ * 1024/*bytes*/;
        public const long DEFAULT_LOAD_LIMIT = 512/*mb*/ * 1024/*k bytes */ * 1024/*bytes*/;


    #endregion


    #region .ctor
        public FileObjectStoreProvider() : base(null)
        {

        }

        public FileObjectStoreProvider(ObjectStoreService director) : base(director)
        {

        }
    #endregion

    #region Private Fields

        private FileObjectFormat m_Format;
        private ISerializer m_Serializer;

        private List<string> m_KnownTypes = new List<string>();


        private long m_LoadLimit;
        private string m_RootPath;
        private long m_LoadSize;


    #endregion


    #region Properties

        /// <summary>
        /// Returns file format used for serialization/deserialization into/from files
        /// </summary>
        public FileObjectFormat Format
        {
          get { return m_Format; }
        }



        /// <summary>
        /// Imposes the limit on number of bytes that can be read from disk on load all.
        /// Once limit is exceeded the rest of objects will not load.
        /// Provider loads most recent objects first
        /// </summary>
        public long LoadLimit
        {
          get { return m_LoadLimit; }
          set
          {
            if (value > MAX_LOAD_LIMIT)
              value = MAX_LOAD_LIMIT;
            m_LoadLimit = value;
          }
        }

        /// <summary>
        /// Returns how many bytes have been loaded from disk
        /// </summary>
        public long LoadSize
        {
          get { return m_LoadSize; }
        }



        /// <summary>
        /// Gets/sets the root path where objects are stored
        /// </summary>
        public string RootPath
        {
          get { return m_RootPath ?? string.Empty; }
          set
          {
            if (Status != ControlStatus.Inactive)
              throw new NFXException(StringConsts.SERVICE_INVALID_STATE + "FileObjectStoreProvider.Path.set()");

            checkPath(value);

            m_RootPath = value;
          }
        }

    #endregion




    #region Public

        public override IEnumerable<ObjectStoreEntry> LoadAll()
        {
          var now = App.LocalizedTime;
          var clock = Stopwatch.StartNew();
          long priorLoadSize = 0;

          var tasks = new Task<ObjectStoreEntry>[System.Environment.ProcessorCount];

          var serializer = getSerializer();
          foreach (var fname in getStorePath().AllFileNames(true))
          {
            var fi = new FileInfo(fname);
            if ( (now - fi.LastWriteTime).TotalMilliseconds > ComponentDirector.ObjectLifeSpanMS) continue;//dont need to read

            if (m_LoadLimit > 0 && m_LoadSize > m_LoadLimit)
            {
              WriteLog(MessageType.Info, "Load limit imposed", m_LoadLimit.ToString() + " bytes", FROM);
              break;
            }

            var assigned = false;
            while(!assigned)
                for(int i=0; i<tasks.Length; i++)
                {
                  var task = tasks[i];
                  if (task==null)
                  {
                    var file = fname;//C# lambda closure
                    tasks[i] = Task.Factory.StartNew( () => readFile(file, serializer, now, ref priorLoadSize, clock) );
                    assigned = true;
                    break;
                  }

                  if (task.IsCompleted)
                  {
                    tasks[i] = null;
                    if (task.Result!=null)
                     yield return task.Result;
                  }
                }
          }

          foreach(var task in tasks)
           if (task!=null)
            if (task.Result!=null)//20130715 DKh
              yield return task.Result;

          clock.Stop();
        }

        public override void Write(ObjectStoreEntry entry)
        {
          using (var fs = new FileStream(getFileName(entry), FileMode.Create))
          {
             m_Serializer.Serialize(fs, entry.Value);
          }
        }

        public override void Delete(ObjectStoreEntry entry)
        {
          File.Delete(getFileName(entry));
        }


    #endregion


    #region Protected

        protected override void DoConfigure(IConfigSectionNode node)
        {
          base.DoConfigure(node);
          LoadLimit = node.AttrByName(CONFIG_LOAD_LIMIT_ATTR).ValueAsLong(DEFAULT_LOAD_LIMIT);
          RootPath = node.AttrByName(CONFIG_ROOT_PATH_ATTR).ValueAsString();
          m_Format = node.AttrByName(CONFIG_FORMAT_ATTR).ValueAsEnum<FileObjectFormat>(FileObjectFormat.Slim);

          foreach(var cn in  node[CONFIG_KNOWN_TYPES_SECTION].Children.Where(cn => cn.IsSameName(CONFIG_KNOWN_SECTION)))
          {
            var tn = cn.AttrByName(CONFIG_TYPE_ATTR).ValueAsString(CoreConsts.UNKNOWN);
            m_KnownTypes.Add( tn );
          }
        }


        protected override void DoStart()
        {
          checkPath(m_RootPath);

          base.DoStart();

          m_LoadSize = 0;

          m_Serializer = getSerializer();
        }



    #endregion


    #region .pvt .utils

        private ISerializer getSerializer()
        {
          if (m_Format!=FileObjectFormat.Slim)
          {
            return new MSBinaryFormatter();
          }


          var treg = new TypeRegistry(TypeRegistry.CommonCollectionTypes,
                                                TypeRegistry.BoxedCommonTypes,
                                                TypeRegistry.BoxedCommonNullableTypes);

          foreach(var tn in m_KnownTypes)
          {
            var t = Type.GetType(tn, false);
            if (t!=null)
             treg.Add(t);
            else
             WriteLog(MessageType.Warning, "Specified known type could not be found: " + tn, tn, "getSerializer(slim)");
          }
          return  new SlimSerializer(treg);
        }


        private ObjectStoreEntry readFile(string fname, ISerializer serializer, DateTime now, ref long priorLoadSize, Stopwatch clock)
        {
            using (var fs = new FileStream(fname, FileMode.Open))//, FileAccess.Read, FileShare.Read, 64*1024, FileOptions.SequentialScan))
            {

              var entry = new ObjectStoreEntry();
              try
              {
                entry.Value = serializer.Deserialize(fs);
              }
              catch (Exception error)
              {
                WriteLog(MessageType.Error, "Deserialization error: " + error.Message, fname, FROM);
                return null;
              }

              Interlocked.Add( ref m_LoadSize, fs.Length);

              if (m_LoadSize-priorLoadSize > 32*1024*1024)
              {
               WriteLog(MessageType.Info, string.Format("Loaded disk bytes {0} in {1}", LoadSize, clock.Elapsed), null, FROM);
               priorLoadSize = m_LoadSize;
              }

              entry.Key = getGUIDFromFileName(fname);
              entry.LastTime = now;
              entry.Status = ObjectStoreEntryStatus.Normal;

              return entry;
            }
        }


        private void checkPath(string path)
        {
          path = path ?? CoreConsts.UNKNOWN;

          if (!Directory.Exists(path))
            throw new NFXException(StringConsts.OBJSTORESVC_PROVIDER_CONFIG_ERROR + "Bad path: " + path);
        }


        private string getStorePath()
        {
          var path = Path.Combine(m_RootPath, ComponentDirector.StoreGUID.ToString());
          if (!Directory.Exists(path))
            Directory.CreateDirectory(path);

          return path;
        }


        // storePath: c:\root
        // Guid:      facaca23423434dada
        // ->
        //   c:\root\fa\ca\ca23423434dada.faca
        private string getFileName(ObjectStoreEntry entry)
        {
          var key = entry.Key.ToString();
          var path = Path.Combine(getStorePath(), key.Substring(0,2), key.Substring(2,2));

          if (!Directory.Exists(path))
            Directory.CreateDirectory(path);

          return Path.Combine(path, key.Substring(4)+'.'+key.Substring(0,4));//for better file system tree balancing, first 4 chars moved as extension to the back
        }


        private Guid getGUIDFromFileName(string fname)
        {
          var str = Path.GetExtension(fname).Substring(1) + Path.GetFileNameWithoutExtension(fname); //reconstruct GUID from file name ext+name

          return new Guid(str);
        }



    #endregion


  }


}
