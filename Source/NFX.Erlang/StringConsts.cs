/*<FILE_LICENSE>
* NFX (.NET Framework Extension) Unistack Library
* Copyright 2003-2017 ITAdapter Corp. Inc.
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
using System.Text;

namespace NFX.Erlang
{
  /// <summary>
  /// A dictionary of framework text messages.
  /// Localization may be done in this class in future
  /// </summary>
  internal static class StringConsts
  {

    public const string ARGUMENT_ERROR = "Argument error: ";

    public const string CONFIGURATION_NAVIGATION_SECTION_REQUIRED_ERROR = "Path '{0}' requires section node but did not land at an existing section";

    public const string ERL_DS_RPC_EXEC_ERROR                 = "ErlDataStore could not execute RPC call '{0}'. Error: {1}";

    public const string ERL_DS_START_REMOTE_ABSENT_ERROR      = "ErlDataStore could not start as required remote name is absent";
    public const string ERL_DS_START_REMOTE_DUPLICATE_ERROR   = "ErlDataStore could not start as remote name '{0}' is already used by another instance. An app may only have one ErlDataStore with the same remote node name";

    public const string ERL_DS_QUERY_SCRIPT_PARSE_ERROR       = "ErlDataStore could not parse script query source '{0}'. Error: {1}";
    public const string ERL_DS_QUERY_PARAM_NOT_FOUND_ERROR    = "ErlDataStore could not bind script query source '{0}' as param '{1}' was not found";

    public const string ERL_DS_QUERY_SUBSCR_NOT_FOUND_ERROR   = "ErlDataStore subscription query must include Subscriber::pid() parameter";
    public const string ERL_DS_QUERY_TMSTAMP_NOT_FOUND_ERROR  = "ErlDataStore subscription query must include Timestamp::long() parameter";
    public const string ERL_DS_QUERY_TMSTAMP_CTX_ABSENT_ERROR = "ErlDataStore subscription Timestamp::long() is absent in context";

    public const string ERL_DS_SCHEMA_NOT_KNOWN_ERROR         = "Schema '{0}' is not known in: {1}";
    public const string ERL_DS_SCHEMA_INVALID_VALUE_ERROR     = "Schema '{0}' has invalid value in term {1}";
    public const string ERL_DS_SCHEMA_MAP_NOT_KNOWN_ERROR     = "ErlSchema name '{0}' is not known in the map set";
    public const string ERL_DS_SCHEMA_MAP_ERL_TYPE_ERROR      = "ErlSchema mapping does not handle '{0}' erl type";
    public const string ERL_DS_INVALID_RESP_PROTOCOL_ERROR    = "ErlDataStore received an invalid protocol response: ";
    public const string ERL_DS_CRUD_WRITE_FAILED_ERROR        = "ErlDataStore CRUD write failed: ";
    public const string ERL_DS_CRUD_RESP_SCH_MISMATCH_ERROR   = "ErlDataStore map can not convert to row erlang tuple named '{0}' per supplied schema name '{1}'";
    public const string ERL_DS_CRUD_RESP_SCH_FLD_COUNT_ERROR  =
           "ErlDataStore map can not convert to row erlang tuple named '{0}' per supplied CRUD schema name '{1}' as field count differ";
    public const string ERL_DS_INTERNAL_MAPPING_ERROR         = "ErlDataStore internal mapping error: ";

    public const string ERL_ATOM_SIZE_TOO_LONG_ERROR          = "Atom size is too long!";
    public const string ERL_ATOM_TABLE_IS_FULL_ERROR          = "Atom table is full!";
    public const string ERL_BIG_INTEGER_OVERFLOW_ERROR        = "Big integer overflow";
    public const string ERL_CANNOT_CONVERT_TYPES_ERROR        = "Cannot convert type {0} to {1}";
    public const string ERL_CANNOT_CONVERT_TYPES_CYCLE_ERROR  = "Cannot convert type {0} to {1} as there is a reference cycle";
    public const string ERL_CANNOT_CLONE_INSTANCE_ERROR       = "Cannot clone instance of type {0}";
    public const string ERL_CANNOT_READ_FROM_STREAM_ERROR     = "Cannot read from input stream";
    public const string ERL_CONFIG_SINGLE_NODE_ERROR          = "Configuration must contain a single local node (found: {0} local nodes)";
    public const string ERL_CONNECTION                        = "connecton {0} {1} {2}";
    public const string ERL_CONN_ACCEPT_FROM                  = "Accept from {0}:{1}";
    public const string ERL_CONN_ACCEPT_ERROR                 = "Error accepting connection from {0}";
    public const string ERL_CONN_ALREADY_CONNECTED_ERROR      = "Already connected";
    public const string ERL_CONN_CANT_CONNECT_TO_NODE_ERROR   = "Cannot establish connection to node {0}";
    public const string ERL_CONN_CANT_CONNECT_TO_HOST_ERROR   = "Cannot establish {0} connection to host {1}:{2}";
    public const string ERL_CONN_CANT_RPC_TO_LOCAL_NODE_ERROR = "Cannot make rpc calls on local node!";
    public const string ERL_CONN_HANDSHAKE_FAILED_ERROR       = "Handshake failed - {0}";
    public const string ERL_CONN_HANDSHAKE_DATA_ERROR         = "Handshake failed - data/protocol error";
    public const string ERL_CONN_HANDSHAKE_EXT_PIDS_ERROR     = "Handshake failed - peer cannot handle extended pids and ports";
    public const string ERL_CONN_HANDSHAKE_REF_ERROR          = "Handshake failed - peer cannot handle extended references";
    public const string ERL_CONN_INVALID_DATA_FROM_PEER_ERROR = "Invalid data from remote node";
    public const string ERL_CONN_EOF_AFTER_N_BYTES_ERROR      = "EOF after {0} of {1} bytes";
    public const string ERL_CONN_LOCAL_RPC_ERROR              = "Cannot make rpc calls on local node!";
    public const string ERL_CONN_MSG_SIZE_TOO_LONG_ERROR      = "Message size too long (max={0}, got={1})";
    public const string ERL_CONN_NO_COMMON_PROTO_ERROR        = "No common protocol found - cannot connect";
    public const string ERL_CONN_NOT_CONNECTED_ERROR          = "Not connected";
    public const string ERL_CONN_PEER_AUTH_ERROR              = "Peer authentication error";
    public const string ERL_CONN_READ_TOO_SHORT_ERROR         = "Read {0} out of {1} bytes!";
    public const string ERL_CONN_REMOTE_CLOSED_ERROR          = "Remote connection closed";
    public const string ERL_CONN_TIMEOUT_ERROR                = "Timeout waiting for connect to {0}:{1}";
    public const string ERL_CONN_UNAUTH_COOKIE_ERROR          = "Remote cookie not authorized: {0}";
    public const string ERL_CONN_UNKNOWN_REMOTE_NODE_ERROR    = "Unknown remote node type";
    public const string ERL_CONN_UNKNOWN_TAG_ERROR            = "Unknown tag {0}: {1}";
    public const string ERL_CONN_WRONG_STATUS_ERROR           = "Peer replied with status '{0}' instead of 'ok'";
    public const string ERL_EPMD_INVALID_RESPONSE_ERROR       = "No valid EPMD response from host {0} for node {1}: {2}";
    public const string ERL_EPMD_INVALID_PORT_ERROR           = "EPMD couldn't resolve port number for node '{0}'";
    public const string ERL_EPMD_FAILED_TO_CONNECT_ERROR      = "Failed to connect to EPMD server";
    public const string ERL_EPMD_LOOKUP_R4                    = "LOOKUP {0} (ver=r4)";
    public const string ERL_EPMD_NOT_FOUND                    = "NOT FOUND";
    public const string ERL_EPMD_NO_RESPONSE                  = "No EPMD response";
    public const string ERL_EPMD_NOT_RESPONDING               = "Epmd not responding on host '{0}' when looking up node '{1}'";
    public const string ERL_EPMD_OK                           = "OK";
    public const string ERL_EPMD_PORT                         = "PORT {0}";
    public const string ERL_EPMD_PUBLISH                      = "PUBLISH {0} (port={1}, ver={2})";
    public const string ERL_EPMD_UNPUBLISH                    = "UNPUBLISH {0} (port={1}, res=OK[assumed])";
    public const string ERL_INVALID_IO_REQUEST                = "Invalid Erlang I/O request: ";
    public const string ERL_INVALID_MBOX_NAME_ERROR           = "Invalid mailbox name";
    public const string ERL_INVALID_MFA_FORMAT_ERROR          = "Invalid MFA format (expected \"Module:Function(Arg1, Arg2, ..., ArgN)\")";
    public const string ERL_INVALID_RPC_REQUEST_ERROR         = "Invalid rpc request {0}";
    public const string ERL_INVALID_TERM_TYPE_ERROR           = "Invalid term type (expected: {0}, got tag: {1})";
    public const string ERL_INVALID_DATA_FORMAT_ERROR         = "Invalid data format (version tag: {0})";
    public const string ERL_INVALID_FORMATTING_CHAR_ERROR     = "Invalid formatting character '{0}' in {1}";
    public const string ERL_INVALID_NUMBER_OF_ARGS_ERROR      = "Invalid number of arguments";
    public const string ERL_INVALID_VALUE_CAST_ERROR          = "Cannot cast type {0} to {1} (value={2})";
    public const string ERL_INVALID_VALUE_ERROR               = "Invalid value ({0})";
    public const string ERL_INVALID_VALUE_LENGTH_ERROR        = "Invalid value length ({0})";
    public const string ERL_INVALID_VALUE_TYPE_ERROR          = "Invalid {0} value type: {1}";
    public const string ERL_INVALID_VARIABLE_TYPE_ERROR       = "Invalid variable type: {0}";
    public const string ERL_MISSING_VALUE_FOR_ARGUMENT_ERROR  = "Missing value for argument #{0}: ~{1}";
    public const string ERL_PARSING_AT_ERROR                  = "Error parsing {0} at pos {1}";
    public const string ERL_REF_ID_ARGUMENT_ERROR             = "Expected ID array of 3 integers!";
    public const string ERL_STOPPING_SERVER                   = "Node {0} stopping {1} server";
    public const string ERL_UNBOUND_VARIABLE_ERROR            = "Unbound variable!";
    public const string ERL_UNSUPPORTED_ELEMENT_TYPE_ERROR    = "Unsupported type of element {0}: {1}";
    public const string ERL_UNSUPPORTED_TERM_TYPE_ERROR       = "Unsupported term type (tag: {0})";
    public const string ERL_VALUE_MUST_NOT_BE_NULL_ERROR      = "Value must not be null!";
    public const string ERL_VALUE_TOO_LARGE_FOR_TYPE_ERROR    = "Value too large for {0} type (arity={1})";
    public const string ERL_VARIABLE_NOT_FOUND_ERROR          = "Variable {0} not found!";
    public const string ERL_VARIABLE_INVALID_VALUE_TYPE_ERROR = "Invalid variable {0} value type (got={1}, expected={2})";
    public const string ERL_WRONG_VALUE_FOR_ARGUMENT_ERROR    = "Wrong value type for argument #{0}: {1}";


  }
}
