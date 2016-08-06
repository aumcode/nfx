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
using System.Text;
using System.IO;
using System.Threading;
using System.Reflection;

namespace NFX
{

  /// <summary>
  ///  Fetches resources such as script statement text by scriptName from assembly resource stream.
  ///  Mostly used for SQL and JavaScript but maybe used for any text retrieval.
  ///  This class is 100% safe for multithreading operations.
  ///  Script texts are cached in ram for faster subsequent access.
  /// </summary>
  public static class EmbeddedResource
  {
    private static object s_CacheLock = new object();
    private static Dictionary<string, string> s_Cache = new Dictionary<string, string>();

    /// <summary>
    /// Pass a type and resource path rooted at type's namespace, for example
    ///  given <code> string sql = typeof(SomeType).GetText("SQL.User.Insert.sql");</code>
    ///  If "SomeType" is declared in "TestApp.Types", then statement's resource will have to be embedded under resource named:
    ///   "TestApp.Types.SQL.User.Insert.sql"
    /// </summary>
    public static string GetText(this Type scopingType, string scriptName)
    {
      string result = null;

      var entryName = "text://"+scopingType.Namespace + "::" + scriptName;


      if (s_Cache.TryGetValue(entryName, out result)) return result;

        lock(s_CacheLock)
        {
          var dict = new Dictionary<string,string>(s_Cache);
          if (dict.TryGetValue(entryName, out result)) return result;

          try
          {
            using (Stream stream = scopingType.Assembly.GetManifestResourceStream(scopingType, scriptName))
             using (TextReader reader = new StreamReader(stream))
               result = reader.ReadToEnd();
          }
          catch
          {
             //this will throw when resource is not found - this is VERY slow
          }

          dict[entryName] = result;
          s_Cache = dict;
        }

      return result;
    }


    /// <summary>
    /// Pass a type and resource path rooted at type's namespace, for example
    ///  given <code> using (var stream = typeof(SomeType).GetBinary("My.Picture.gif")){...}</code>
    ///  If "SomeType" is declared in "TestApp.Types", then statement's resource will have to be embedded under resource named:
    ///   "TestApp.Types.My.Picture.gif"
    /// </summary>
    public static Stream GetBinaryStream(this Type scopingType, string resourceName)
    {
       return scopingType.Assembly.GetManifestResourceStream(scopingType, resourceName);
    }

    /// <summary>
    /// Pass a type and resource path rooted at type's namespace, for example
    ///  given <code> using (var stream = typeof(SomeType).GetBinary("My.Picture.gif")){...}</code>
    ///  If "SomeType" is declared in "TestApp.Types", then statement's resource will have to be embedded under resource named:
    ///   "TestApp.Types.My.Picture.gif"
    /// </summary>
    public static byte[] GetBinaryContent(this Type scopingType, string resourceName)
    {
      using(var ms = new MemoryStream())
        using(var str = scopingType.GetBinaryStream(resourceName))
        {
          str.CopyTo(ms);
          return ms.ToArray();
        }
    }


  }
}
