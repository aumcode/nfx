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

    public const string CONNECTION_TEST_FAILED_ERROR =
        "MongoDB connection test failed with message: {0}";

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


  }
}

