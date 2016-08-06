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
 * Author: Andrey Kolbasov <andrey@kolbasov.com>
 */
using System;
using System.Text;

namespace NFX.IO.FileSystem.GoogleDrive
{
  /// <summary>
  /// Methods for working with paths
  /// </summary>
  static class GoogleDrivePath
  {
    private static char PATH_SEPARATOR = '/';
    private static char[] PATH_SEPARATORS = new[] { PATH_SEPARATOR };
    private static char[] TRIMMERS = new[] { PATH_SEPARATOR, ' ', '\t', '\r', '\n' };

    public static string[] Split(string path)
    {
      return path.Split(PATH_SEPARATORS, StringSplitOptions.RemoveEmptyEntries);
    }

    public static string GetParentPath(string path)
    {
      if (path.IsNullOrWhiteSpace())
      {
        return null;
      }

      path = path.Trim(TRIMMERS);

      var segments = Split(path);

      if (segments.Length < 2)
      {
        return null;
      }

      var parentBuilder = new StringBuilder();

      for (int i = 0; i < segments.Length - 1; i++)
      {
        if (i > 0)
        {
          parentBuilder.Append(PATH_SEPARATOR);
        }

        parentBuilder.Append(segments[i]);
      }

      return parentBuilder.ToString();
    }
  }
}
