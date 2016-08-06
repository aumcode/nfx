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

using NFX.CodeAnalysis.Source;
using NFX.CodeAnalysis.Laconfig;

namespace NFX.Environment
{
    /// <summary>
    /// Provides implementation of configuration based on Laconic content format
    /// </summary>
    /// <example>
    /// Example Laconic Configuration Content:
    ///
    /// nfx //comments are allowed
    /// {
    ///   log-root=$"c:\nfx\"
    ///   log-csv="NFX.Log.Destinations.CSVFileDestination, NFX"
    ///   debug-default-action="Log,Throw"
    ///
    ///   log
    ///   {
    ///     name="Logger"
    ///
    ///     destination
    ///     {
    ///       type=$(/$log-csv)
	  ///       name="WinFormsTest Log"//strings in dblquotes
	  ///       path=$(/$log-root)
    ///       name-time-format='yyyyMMdd'//strings in snglquotes
    ///       generate-failover-msg=false
    ///     }
    ///   }
    ///   /* multiline comments
    ///    data-store {type="NFX.RecordModel.DataAccess.MongoDB.MongoDBModelDataStore, NFX.MongoDB"
    ///                connect-string="mongodb://localhost"
    ///                db-name="test"}
    ///   */
    /// }
    /// </example>
    [Serializable]
    public class LaconicConfiguration : FileConfiguration
    {
       #region .ctor / static

            /// <summary>
            /// Creates an instance of a new configuration not bound to any laconfig file
            /// </summary>
            public LaconicConfiguration() : base()
            {

            }

            /// <summary>
            /// Creates an isntance of the new configuration and reads contents from a laconfig file
            /// </summary>
            public LaconicConfiguration(string filename) : base(filename)
            {
              readFromFile();
            }

            /// <summary>
            /// Creates an instance of configuration initialized from laconfig passed as string
            /// </summary>
            public static LaconicConfiguration CreateFromString(string content)
            {
              var result = new LaconicConfiguration();
              result.readFromString(content);

              return result;
            }


       #endregion

       #region Public

              /// <summary>
              /// Saves configuration into a file
              /// </summary>
              public override void SaveAs(string filename)
              {
                this.SaveAs(filename, null);
              }


              /// <summary>
              /// Saves configuration into a file
              /// </summary>
              public void SaveAs(string filename, LaconfigWritingOptions options = null)
              {
                using(var fs = new FileStream(filename, FileMode.Create))
                   LaconfigWriter.Write(this, fs, options);
                base.SaveAs(filename);
              }


              /// <summary>
              /// Saves laconic configuration into string in Laconfig format and returns it
              /// </summary>
              public string SaveToString(LaconfigWritingOptions options = null)
              {
                return LaconfigWriter.Write(this, options);
              }


              public override void Refresh()
              {
                readFromFile();
              }


              public override void Save()
              {
                SaveAs(m_FileName);
              }

              public override string ToString()
              {
                return SaveToString();
              }



       #endregion
       #region Protected

          //for Laconfig we dont need to adjust names as the are properly escaped
          //protected override string AdjustNodeName(string name)
          //{

          //}

       #endregion

       #region .pvt
            private void readFromFile()
            {
              using(var fsrc = new FileSource(m_FileName))
                  read(fsrc);
            }

            private void readFromString(string content)
            {
              read(new StringSource(content));
            }

            private void read(ISourceText source)
            {
              var context = new LaconfigData(this);

              var lexer = new LaconfigLexer(source, throwErrors: true);
              var parser = new LaconfigParser(context, lexer, throwErrors: true);

              parser.Parse();
            }



       #endregion

    }
}
