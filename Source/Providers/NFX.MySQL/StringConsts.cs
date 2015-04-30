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
 * Originated: 2008.03
 * Revision: NFX 1.0  2011.02.06
 */
namespace NFX.DataAccess.MySQL
{
  internal static class StringConsts
  {
     public const string MYSQL_CRUD_TARGET = "MySQL";


    public const string MODEL_INVALID_STATE_ERROR =
        "Model is in invalid state. Expected: {0} Actual: {1}";

    public const string SQL_STATEMENT_FAILED_ERROR =
        "SQL statement failed";

    public const string CONNECTION_TEST_FAILED_ERROR =
        "MySql connection test failed with message: {0}";

    public const string MODEL_TYPE_UNKNOWN_ERROR =
        "MySql provider could not handle '{0}' operation for model of type '{1}'";

    public const string MODEL_TYPE_NOT_RECORD_ERROR =
        "Model is not of a 'Record' type. Passed type: {0}. Check provider implementation";

    public const string KEY_UNAVAILABLE_ERROR =
        "Key was not supplied and could not be obtained from controller";

    public const string BROAD_UPDATE_ERROR =
        "Broad update without WHERE is not allowed";

    public const string LOADING_ENTITY_NOT_FOUND_ERROR =
        "Could not load entity as store returned no data for supplied key:\n{0}";

    public const string NO_ROWS_AFFECTED_ERROR =
        "No rows affected: {0}";

    public const string INVALID_KEY_TYPE_ERROR =
        "Supplied key is of invalid type";

    public const string LOAD_NO_SELECT_COLUMNS_ERROR =
        "MySQL Load failed as no columns for selection could be generated";

    public const string RECORD_TABLE_NAME_ERROR =
        "Record's 'TableName' property is blank. Passed type: '{0}'. Check record implementation";

    public const string CRUD_STATEMENT_EXECUTION_ERROR =
        "Error executing {0} CRUD statement: {1}";

    public const string CRUD_READONLY_SCHEMA_ERROR =
        "CRUD schema '{0}' is readonly and can not perform operation: {1}";

  }
}

