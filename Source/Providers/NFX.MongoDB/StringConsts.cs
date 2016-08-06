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
namespace NFX.DataAccess.MongoDB
{
  internal static class StringConsts
  {

    public const string ARGUMENT_ERROR =
            "Argument error: ";

    public const string OP_NOT_SUPPORTED_ERROR =
        "Operation '{0}' is not supported by the {1}";

    public const string OP_ROW_NO_PK_ID_ERROR =
        "MongoDBDataStore requires the row '{0}' to have a primary key '{1}' field for operation '{2}'";

    public const string OP_CRUD_ERROR =
        "MongoDBDataStore CRUD operation '{0}' error on row '{1}': {2}";

    public const string QUERY_MODIFY_TARGET_MISSING_ERROR =
        "MongoDBDataStore query is missing modify target collection name: ";

    public const string CONNECTION_TEST_FAILED_ERROR =
        "MongoDB connection test failed with message: {0}";

    public const string CONNECTION_EXISTING_ACQUISITION_ERROR =
        "Could not obtain a connection to '{0}' as the connection limit of {1} is set and {2} ms existing acquisition timeout has passed";

    public const string MODEL_TYPE_NOT_RECORD_ERROR =
        "Model is not of a 'Record' type. Passed type: {0}. Check provider implementation";

    public const string RECORD_TABLE_NAME_ERROR =
        "Record's 'TableName' property is blank. Passed type: '{0}'. Check record implementation";

    public const string COLLECTION_DOES_NOT_EXIST_ERROR =
        "Record's 'TableName' property specifies a collection that does not exist: '{0}'";

    public const string KEY_SUPPORT_ERROR =
        "MongoDB driver supports only the following IDataStoreKey implementations:  NameValueDataStoreKey";

    public const string LOADING_ENTITY_NOT_FOUND_ERROR =
        "Could not load entity as store returned no data for supplied key:\n{0}";

    public const string UNSUPPORTED_BSON_TYPE_ERROR =
        "BSON type is not supported: '{0}'";

    public const string DECIMAL_OUT_OF_RANGE_ERROR =
        "Decimal value of {0} is outside of to-int64 convertable range of {1}..{2}";

    public const string CLR_BSON_CONVERSION_TYPE_NOT_SUPPORTED_ERROR =
        "CLR type '{0}' conversion into BSON is not supported";

    public const string CLR_BSON_CONVERSION_REFERENCE_CYCLE_ERROR =
        "CLR value of type '{0}' could not be converted into BSON as there is a reference cycle";

    public const string BUFFER_LONGER_THAN_ALLOWED_ERROR =
        "Byte[] buffer has a length of {0} bytes which is over the allowed maximum of {1} bytes";

    public const string GDID_BUFFER_ERROR =
        "Error converting GDID data buffer: {0}";

    public const string PROTO_SOCKET_READ_EXCEED_LIMIT_ERROR =
        "MongoDB connector can not read payload as its size of {0} bytes exceeds the limit of {1} bytes. Socket connection may be corrupted";

    public const string PROTO_SOCKET_WRITE_EXCEED_LIMIT_ERROR =
        "MongoDB connector can not write payload as its size of {0} bytes exceeds the limit of {1} bytes. Reduce the doc size or reduce the number of docs in the batch operation";

    public const string PROTO_READ_CRUD_RESPONSE_ERROR =
        "Error reading CRUD operation response: ";

    public const string PROTO_READ_OP_REPLY_ERROR =
        "Error reading OP_REPLY: ";

    public const string PROTO_PING_REPLY_ERROR =
        "Server PING command did not return expected OK: 1";

    public const string PROTO_LIST_COLLECTIONS_REPLY_ERROR =
        "Server ListCollections command did not return expected structure";

    public const string PROTO_COUNT_REPLY_ERROR =
        "Server Count command did not return OK: 1";

    public const string PROTO_RUN_COMMAND_REPLY_ERROR =
        "Server RunCommand did not return OK: 1. Command document: ";

    public const string SERVER_CURSOR_NOT_FOUND_ERROR =
        "Cursor not found by the server";

    public const string SERVER_QUERY_FAILURE_ERROR =
        "Server query failure: ";

    public const string PROTO_SERVER_REPLIED_WITH_NO_DOCUMENTS_ERROR =
        "Server replied with no documents";

    public const string PROTO_REPLY_DOC_COUNT_EXCEED_LIMIT_ERROR =
        "Can not process OP_REPLY as it contains {0} documents which exceeds the limit of {1} documents. Socket connection is most likely corrupted";


    public const string SERVER_OPERATION_RETURNED_ERROR =
        "Server returned failure executing '{0}'. Code: '{1}' Message: '{2}'";

    public const string CURSOR_ENUM_ALREADY_STARTED_ERROR =
        "Cursor can be enumerated only once and can not be Reset() again";


    public const string QUERY_FINISH_STATEMENT_COMPILE_ERROR =
        "Current query statement should be finished with one of the following statements: Equals(), NotEquals(), GreaterThan(), NotGreaterThan(), LessThan(), NotLessThan(), Exists()";

    public const string QUERY_START_STATEMENT_COMPILE_ERROR =
        "Current query statement should start with element name. Use Add(string) method";
  }
}

