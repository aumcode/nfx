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
 * Originated: 2011.01.31
 * Revision: NFX 1.0  2011.02.06
 */
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using NFX.Environment;
using NFX.ApplicationModel;

namespace NFX.DataAccess
{
  /// <summary>
  /// Represents data store that does not do anything
  /// </summary>
  public sealed class NOPDataStore : ApplicationComponent, IDataStoreImplementation
  {
    private static NOPDataStore s_Instance = new NOPDataStore();


    private NOPDataStore(): base(){}

    /// <summary>
    /// Returns a singlelton instance of the data store that does not do anything
    /// </summary>
    public static NOPDataStore Instance
    {
      get { return s_Instance; }
    }

    public string TargetName{ get{ return NFX.DataAccess.CRUD.TargetedAttribute.ANY_TARGET;}}


         public bool InstrumentationEnabled{ get{return false;} set{}}
         public IEnumerable<KeyValuePair<string, Type>> ExternalParameters{ get{ return null;}}
         public IEnumerable<KeyValuePair<string, Type>> ExternalParametersForGroups(params string[] groups){ return null;}
         public bool ExternalGetParameter(string name, out object value, params string[] groups)
         {
           value = null;
           return false;
         }
         public bool ExternalSetParameter(string name, object value, params string[] groups)
         {
           return false;
         }

    public StoreLogLevel LogLevel { get; set;}

    public void TestConnection()
    {

    }

    public void Configure(IConfigSectionNode node)
    {
    }
  }
}
