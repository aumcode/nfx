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

using NFX;
using NFX.Instrumentation;
using NFX.Instrumentation.Telemetry;

namespace TelemetryViewer
{
    /// <summary>
    /// Server class that implements ITelemetryReceiver interface
    /// </summary>
    public class Receiver : ITelemetryReceiver
    {
        private static Registry<SiteData> s_Sites = new Registry<SiteData>();


        public static IEnumerable<SiteData> Sites
        {
            get {  return s_Sites; }
        }



        public Receiver() {}

        public void Send(string siteName, Datum data)
        {
     //   System.Windows.Forms.MessageBox.Show("Arrived from: " + siteName + "  " + data.ToString());

           if (siteName.IsNullOrWhiteSpace()) siteName = "<unnamed>";
           var site = s_Sites.GetOrRegister(siteName, (name) => new SiteData(name), siteName);

           site.Put( data );
        }
    }


    /// <summary>
    /// Holds data per reporting site
    /// </summary>
    public class SiteData : INamed
    {
       /// <summary>
       /// How many samples to cache in ram
       /// </summary>
       public const int BUFFER_SIZE = 1024;


                class bucket
                {
                    public Datum[] m_Data;
                    public int m_Index;
                }

                class sources : Dictionary<string, bucket>
                {

                }


        public SiteData(string name)
        {
            m_Name = name;
            m_LastTraffic = App.TimeSource.Now;
        }

        private string m_Name;
        private Dictionary<Type, sources> m_Data = new Dictionary<Type, sources>();
        private DateTime m_LastTraffic;

        public string Name
        {
            get { return m_Name; }
        }


        public DateTime LastTraffic
        {
            get { return m_LastTraffic; }
        }

        /// <summary>
        /// Returns a thread-safe enumerable of Datum types
        /// </summary>
        public List<Type> DataTypes
        {
            get
            {
                lock(m_Data)
                 return m_Data.Keys.ToList();
            }
        }

        /// <summary>
        /// Returns thread-safe list of string source names per Datum type
        /// </summary>
        public List<string> this[Type tp]
        {
            get
            {
                lock(m_Data)
                {
                    sources s;
                    if (!m_Data.TryGetValue(tp, out s)) return new List<string>();
                    return s.Keys.OrderBy(str=>str).ToList();
                }
            }
        }


        /// <summary>
        /// Returns thread-safe list of Datums of special type and source name, sorted by their timestamps ascending
        /// </summary>
        public List<Datum> this[Type tp, string src]
        {
            get
            {
                lock(m_Data)
                {
                    sources s;
                    if (!m_Data.TryGetValue(tp, out s)) return new List<Datum>();

                    bucket b;
                    if (!s.TryGetValue(src, out b)) return new List<Datum>();
                    return b.m_Data.Where(d => d!=null).OrderBy(d => d.UTCTime).ToList();
                }
            }
        }


        public void Put(Datum datum)
        {
            var tp = datum.GetType();
            m_LastTraffic = App.TimeSource.Now;
            lock(m_Data)
            {

                sources src;
                if (!m_Data.TryGetValue( tp, out src))
                {
                    src = new sources();
                    m_Data[tp] = src;
                }

                bucket b;
                if (!src.TryGetValue( datum.Source, out b))
                {
                    b = new bucket();
                    b.m_Data = new Datum[BUFFER_SIZE];
                    src[datum.Source] = b;
                }

                b.m_Data[b.m_Index] = datum;
                b.m_Index++;
                if (b.m_Index>=b.m_Data.Length) b.m_Index = 0;
            }

        }
    }


}
