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
 * Revision: NFX 1.0  2011.01.31
 */
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

using NFX.Environment;
using NFX.DataAccess.CRUD;

namespace NFX
{
  public static class DataUtils
  {
    private static readonly char[] FIELD_DELIMITER = {',',';','|'};


    /// <summary>
    /// Converts field names separated by ',' or ';' into a FieldFilterFunction
    /// </summary>
    public static FieldFilterFunc OnlyTheseFields(this string fields, bool caseSensitive = false)
    {
      if (fields.IsNullOrWhiteSpace()) return (row, key, fdef) => false;
      var names = fields.Split(FIELD_DELIMITER).Where(n => n.IsNotNullOrWhiteSpace()).Select( n => n.Trim());
      return (row, key, fdef) => names.Contains(fdef.Name, caseSensitive ? StringComparer.InvariantCulture : StringComparer.InvariantCultureIgnoreCase);
    }

    /// <summary>
    /// Converts field names separated by ',' or ';' into a FieldFilterFunction
    /// </summary>
    public static FieldFilterFunc OnlyTheseFields(this IEnumerable<string> fields, bool caseSensitive = false)
    {
      if (fields==null) return (row, key, fdef) => false;
      var names = fields.Where(n => n.IsNotNullOrWhiteSpace()).Select( n => n.Trim());
      return (row, key, fdef) => fields.Contains(fdef.Name, caseSensitive ? StringComparer.InvariantCulture : StringComparer.InvariantCultureIgnoreCase);
    }

    /// <summary>
    /// Converts field names separated by ',' or ';' into a FieldFilterFunction
    /// </summary>
    public static FieldFilterFunc AllButTheseFields(this string fields, bool caseSensitive = false)
    {
      if (fields.IsNullOrWhiteSpace()) return (row, key, fdef) => false;
      var names = fields.Split(FIELD_DELIMITER).Select( n => n.Trim());
      return (row, key, fdef) => !names.Contains(fdef.Name, caseSensitive ? StringComparer.InvariantCulture : StringComparer.InvariantCultureIgnoreCase);
    }

    /// <summary>
    /// Converts field names separated by ',' or ';' into a FieldFilterFunction
    /// </summary>
    public static FieldFilterFunc AllButTheseFields(this IEnumerable<string> fields, bool caseSensitive = false)
    {
      if (fields==null) return (row, key, fdef) => false;
      var names = fields.Where(n => n.IsNotNullOrWhiteSpace()).Select( n => n.Trim());
      return (row, key, fdef) => !fields.Contains(fdef.Name, caseSensitive ? StringComparer.InvariantCulture : StringComparer.InvariantCultureIgnoreCase);
    }


    /// <summary>
    /// If source is not null, creates a shallow clone using 'source.CopyFields(copy)'
    /// </summary>
    public static TRow Clone<TRow>(this TRow source,
                                   bool includeAmorphousData = true,
                                   bool invokeAmorphousAfterLoad = true,
                                   Func<string, Schema.FieldDef, bool> fieldFilter = null,
                                   Func<string, string, bool> amorphousFieldFilter = null) where TRow : Row
    {
      if (source==null) return null;
      var copy = Row.MakeRow(source.Schema, source.GetType());//must be GetType() not typeof() as we want to clone possibly more derived row as specified by the instance
      source.CopyFields(copy, includeAmorphousData, invokeAmorphousAfterLoad, fieldFilter, amorphousFieldFilter);
      return (TRow)copy;
    }


    /// <summary>
    /// Casts enumerable of rows (such as rowset) to the specified row type, returning empty enumerable if the source is null
    /// </summary>
    public static IEnumerable<TRow> AsEnumerableOf<TRow>(this IEnumerable<Row> source) where TRow : Row
    {
      if (source==null) return Enumerable.Empty<TRow>();

      return source.Cast<TRow>();
    }

    /// <summary>
    /// Loads one row cast per Query(T) or null
    /// </summary>
    public static TRow LoadRow<TRow>(this ICRUDOperations operations, Query<TRow> query) where TRow : Row
    {
      if (operations==null || query==null)
        throw new NFXException(StringConsts.ARGUMENT_ERROR+"LoadRow(ICRUDOperations==null | query==null)");

      return operations.LoadOneRow(query) as TRow;
    }

    /// <summary>
    /// Async version - loads one row cast per Query(T) or null
    /// </summary>
    public static Task<TRow> LoadRowAsync<TRow>(this ICRUDOperations operations, Query<TRow> query) where TRow : Row
    {
      if (operations==null || query==null)
        throw new NFXException(StringConsts.ARGUMENT_ERROR+"LoadRowAsync(ICRUDOperations==null | query==null)");

      return operations.LoadOneRowAsync(query)
                       .ContinueWith<TRow>( (antecedent) => antecedent.Result as TRow);
    }

    /// <summary>
    /// Loads rowset with rows cast per Query(T) or empty enum
    /// </summary>
    public static IEnumerable<TRow> LoadEnumerable<TRow>(this ICRUDOperations operations, Query<TRow> query) where TRow : Row
    {
      if (operations==null || query==null)
        throw new NFXException(StringConsts.ARGUMENT_ERROR+"LoadEnumerable(ICRUDOperations==null | query==null)");

      return operations.LoadOneRowset(query).AsEnumerableOf<TRow>();
    }

    /// <summary>
    /// Async version - loads rowset with rows cast per Query(T) or empty enum
    /// </summary>
    public static Task<IEnumerable<TRow>> LoadEnumerableAsync<TRow>(this ICRUDOperations operations, Query<TRow> query) where TRow : Row
    {
      if (operations==null || query==null)
        throw new NFXException(StringConsts.ARGUMENT_ERROR+"LoadEnumerableAsync(ICRUDOperations==null | query==null)");

      return operations.LoadOneRowsetAsync(query)
                       .ContinueWith<IEnumerable<TRow>>( (antecedent) => antecedent.Result.AsEnumerableOf<TRow>());
    }

  }
}
