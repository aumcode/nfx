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
using System.Text;

namespace NFX
{
  /// <summary>
  /// A dictionary of framework text messages.
  /// Localization may be done in this class in future
  /// </summary>
  internal static class StringConsts
  {
    public const string SECURITY_NON_AUTHENTICATED = "<non-authenticated>";

        public const string OBJECT_DISPOSED_ERROR =
            "Object was already disposed";

         public const string OPERATION_NOT_SUPPORTED_ERROR =
            "Operation not supported: ";

        public const string LOGSVC_NODESTINATIONS_ERROR =
            "No log destinations registered. Log service could not start";

        public const string SERVICE_INVALID_STATE =
            "Service is in inappropriate state for requested operation: ";

        public const string SERVICE_COMPOSITE_CHILD_START_ABORT_ERROR =
            "Composite service host start aborted due to exception from child service '{0}' start: {1} ";

        public const string ARGUMENT_ERROR =
            "Argument error: ";

        public const string CANNOT_RETURN_NULL_ERROR = "'next' function cannot return null. ";

        public const string HTTP_OPERATION_ERROR = "HTTP[S] error: ";

        public const string FILE_NOT_FOUND_ERROR = "File not found: ";

        public const string LOGSVC_DESTINATION_OPEN_ERROR =
            "Log service '{0}' could not open destination '{1}'. Destination.TestOnStart = {2}. Destination exception:\n   {3}";

        public const string LOGSVC_DESTINATION_EXCEEDS_MAX_PROCESSING_TIME_ERROR =
            "Destination '{0}' exceeded allowed max processing time. Allowed {1} ms, took {2} ms";

        public const string LOGSVC_DESTINATION_IS_OFFLINE_ERROR =
            "Destination '{0}' is offline due to prior failure and RestartProcessingAfterMs timespan has not expired yet";

        public const string LOGSVC_FAILOVER_MSG_TEXT =
            "Message {0} delivery failed over from destination '{1}' to '{2}'. Average processing latency of failed destination '{3}'";


        public const string CONFIGURATION_FILE_UNKNOWN_ERROR = "Configuration file is unknown/not set ";

        public const string CONFIGURATION_EMPTY_NODE_MODIFY_ERROR = "Empty configuration node may not be modified";

        public const string CONFIGURATION_READONLY_ERROR = "Readonly configuration can not be modified";

        public const string CONFIGURATION_TYPE_CREATION_ERROR = "Type could not be created from node path '{0}'. Error: {1}";

        public const string CONFIGURATION_TYPE_ASSIGNABILITY_ERROR = "Instance of type '{0}' is not assignable to '{1}'";

        public const string CONFIGURATION_MAKE_USING_CTOR_ERROR = "MakeUsingCtor for type '{0}' failed with error: {1}";

        public const string CONFIGURATION_TYPE_NOT_SUPPLIED_ERROR = "Type not supplied either as 'type' attribute or default";

        public const string CONFIGURATION_TYPE_RESOLVE_ERROR = "Type name '{0}' could not be resolved into type object";

        public const string CONFIGURATION_CLONE_EMPTY_NODE_ERROR = "Empty sentinel nodes can not be cloned";

        public const string CONFIGURATION_ENTITY_NAME_ERROR = "The name is invalid for this configuration type. Name: ";

        public const string CONFIGURATION_NODE_NAME_ERROR = "The supplied node name '{0}' is not supported by this configuration when StrictNames is set to true";

        public const string CONFIGURATION_ATTRIBUTE_MEMBER_READONLY_ERROR = "Instance of '{0}' could not be configured using ConfigAttribute because its member '{1}' is readonly ";

        public const string CONFIGURATION_NAVIGATION_REQUIRED_ERROR = "Bad navigation: path '{0}' requires node but did not land at an existing node";

        public const string CONFIGURATION_PATH_SEGMENT_NOT_SECTION_ERROR = "Bad navigation: path segment '{0}' in path '{1}' can not be navigated to because its parent is not a section node ";

        public const string CONFIGURATION_PATH_INDEXER_ERROR = "Bad navigation: path '{0}' contains bad indexer specification '{1}'";

        public const string CONFIGURATION_PATH_INDEXER_SYNTAX_ERROR = "Bad navigation: path '{0}' contains bad indexer syntax";

        public const string CONFIGURATION_PATH_VALUE_INDEXER_SYNTAX_ERROR = "Bad navigation: path '{0}' contains bad value indexer syntax";

        public const string CONFIGURATION_PATH_VALUE_INDEXER_CAN_NOT_USE_WITH_ATTRS_ERROR = "Bad navigation: cannot use value indexer with attributes, path '{0}'";

        public const string CONFIGURATION_NODE_DOES_NOT_BELONG_TO_THIS_CONFIGURATION_ERROR = "Node '{0}' does not belong to configuration";

        public const string CONFIGURATION_NODE_MUST_NOT_BELONG_TO_THIS_CONFIGURATION_ERROR = "Node '{0}' must not belong to configuration";

        public const string CONFIGURATION_CAN_NOT_INCLUDE_INSTEAD_OF_ROOT_ERROR = "Can not include '{0}' instead of this root node";

        public const string CONFIGURATION_NAVIGATION_SECTION_REQUIRED_ERROR = "Path '{0}' requires section node but did not land at an existing section";

        public const string CONFIGURATION_NAVIGATION_BAD_PATH_ERROR = "Bad navigation path: ";

        public const string CONFIGURATION_SECTION_INDEXER_EMPTY_ERROR = "Section indexer is an empty array";

        public const string CONFIGURATION_VALUE_COULD_NOT_BE_GOTTEN_AS_TYPE_ERROR = "Value from '{0}' could not be gotten as type '{1}' ";

        public const string CONFIGURATION_OVERRIDE_PROHOBITED_ERROR = "Section override failed because it is prohibited by base node: ";


        public const string CONFIGURATION_PATH_ICONFIGURABLE_SECTION_ERROR = "Instance of '{0}' could not be configured using ConfigAttribute because its memeber '{1}' is IConfigurable, however config path did not yield a IConfigSectionNode instance";

        public const string CONFIGURATION_PATH_ICONFIGSECTION_SECTION_ERROR = "Instance of '{0}' could not be configured using ConfigAttribute because its memeber '{1}' is IConfigSection, however config path did not yield a IConfigSectionNode instance";


        public const string CONFIGURATION_ATTR_APPLY_VALUE_ERROR = "Error applying config attribute to property/field '{0}' for the instance of '{1}'. Exception: {2}";


        public const string CONFIGURATION_OVERRIDE_SPEC_ERROR = "Configuration node override specification was not understood per supplied NodeOverrideRules: ";

        public const string CONFIGURATION_SCRIPT_EXECUTION_ERROR = "Configuration script execution error: ";

        public const string CONFIGURATION_SCRIPT_TARGET_CONFIGURATION_MUST_BE_EMPTY_ERROR = "Target configuration must be empty";

        public const string CONFIGURATION_SCRIPT_EXPRESSION_EVAL_ERROR = "Configuration script expression '{0}' evaluation at node '{1}'. Error: {2}";

        public const string CONFIGURATION_SCRIPT_SYNTAX_ERROR = "Script sysntax error: ";

        public const string CONFIGURATION_SCRIPT_ELSE_NOT_AFTER_IF_ERROR = CONFIGURATION_SCRIPT_SYNTAX_ERROR + "ELSE clause '{0}' is not after IF clause";

        public const string CONFIGURATION_SCRIPT_SET_PATH_ERROR = "Configuration script SET instruction '{0}' references path '{1}' which does not exist";

        public const string CONFIGURATION_SCRIPT_CALL_TARGET_PATH_ERROR = "Configuration script CALL instruction '{0}' references invocation target path '{1}' which does not exist";


        public const string CONFIGURATION_SCRIPT_TIMEOUT_ERROR = "Configuration script exceeded allowed timeout of {0}ms while executing '{1}' statement";


        public const string CONFIGURATION_INCLUDE_PRAGMA_ERROR = "Config error processing include pragma '{0}': {1}";

        public const string BUILD_INFO_READ_ERROR =
            "Error reading BUILD_INFO resource: ";

        public const string INVALID_IPSTRING_ERROR =
            "Invalid IP:PORT string: ";

         public const string INVALID_EPOINT_ERROR =
            "Invalid endpoint IP|HOST:PORT string '{0}'. Error: {1}";


        public const string INVALID_TIMESPEC_ERROR =
            "Invalid time specification";

        public const string STRING_VALUE_COULD_NOT_BE_GOTTEN_AS_TYPE_ERROR = "String value '{0}' could not be gotten as type '{1}' ";


        public const string INVENTORIZATION_NEED_STRATEGY_ERROR =
            "Inventorization requires at least one strategy to be added to Strategies collection of InventorizationManager class";

        public const string INVENTORIZATION_ASSEMBLY_LOAD_ERROR =
            "Inventorization assembly load failed: ";


    public const string SLIM_STREAM_CORRUPTED_ERROR = "Slim data stream is corrupted: ";

    public const string SECDB_STREAM_CORRUPTED_ERROR = "SecDB data stream is corrupted: ";


    public const string BINLOG_STREAM_NULL_ERROR = "BinLog stream is null: ";
    public const string BINLOG_STREAM_CANT_SEEK_WRITE_ERROR = "BinLog stream can not seek or can not write: ";
    public const string BINLOG_STREAM_CORRUPTED_ERROR = "BinLog data stream is corrupted: ";
    public const string BINLOG_READER_FACTORY_ERROR = "BinLog reader factory error: ";
    public const string BINLOG_READER_TYPE_MISMATCH_ERROR = "BinLog reader type mismatched. Class: '{0}' Stream: '{1}'";
    public const string BINLOG_BAD_READER_TYPE_ERROR = "BinLog header contains reader type which could not be loaded or is not a valid LogReader derivative: ";


    public const string SECURITY_AUTHROIZATION_ERROR =
        "Authorization to '{0}' failed from '{1}'";

    public const string SECURITY_REPRESENT_CREDENTIALS_FORGOTTEN =
        "Credentials can not be represented as they are forgotten";

    public const string NULL_STRING = "<null>";
    public const string UNKNOWN_STRING = "unknown";


    public const string INVALID_RECORD_TYPE_ERROR =
        "Invalid record type: ";


    public const string LOOKUP_COMMAND_ERROR =
        "Lookup command error: ";

    public const string FORM_LOOKUP_COMMAND_ERROR =
        "Form lookup command error: ";

    public const string MODEL_OPERATION_NOT_ALLOWED_ERROR =
        "Operation not allowed in this state. Current state: \"{0}\", Operation: \"{1}\"";

    public const string FIELD_OPERATION_NOT_ALLOWED_ERROR =
        "Operation not allowed when field is hosted by a record.\nField: \"{0}\", Operation: \"{1}\"";

    public const string ENABLE_BINDINGS_ERROR =
         "Call to EnableBindings() was mismatched for: \"{0}\"";


    public const string FIELD_LOOKUP_NOT_ALLOWED_ERROR =
        "Can not call Lookup() on a field when prior lookup call is pending. Field: ";


    public const string RECURSIVE_FORMULA_FIELD_ERROR =
        "Recursive formula field: ";

    public const string FORMULA_CALCULATED_ANOTHER_FIELD_ERROR =
        "Formula can not reference another calculated field: ";


    public const string FORM_CREATION_ERROR =
        "Form could not be created: ";

    public const string FORM_FACTORY_NULL_ERROR =
        "Form factory was not initialized";


    public const string SURROGATE_LOAD_ERROR =
        "Surrogate record type load error";

    public const string INVALID_FIELD_ACCESSOR_ERROR =
        "Invalid field accessor. Field: <{0}>.{1}";


    public const string READONLY_FIELD_ERROR =
        "Can not modify readonly field value: <{0}>";


    public const string APP_IS_NOT_EXPECTED_TYPE_ERROR =
        "Application is not of expected type";


    public const string APPLY_RECORD_CHANGES_ERROR =
        "Can not apply record changes:";

    public const string METHOD_INVOCATION_CHANGE_ERROR =
        "Method invocation change error:";

    public const string UNINITIALIZED_STATE_ERROR =
        "Model was not initialized and is in unknown state. No data manipulations possible";

    public const string VIEW_MODIFY_ERROR =
        "Can not modify field value while in view state. Field <{0}>";

    public const string REFERENCE_TYPE_VALUE_SET_ERROR =
        "Can not set reference type value for field. Field <{0}>";

    public const string SAVE_MODIFY_ERROR =
        "Can not modify field value while in saving operation. Field <{0}>";

    public const string DELETE_MODIFY_ERROR =
        "Can not modify field value while in deleting operation. Field <{0}>";

    public const string CALC_FIELD_CHANGE_ERROR =
        "Can not modify calculated field when AllowCalculationOverride is not true. Field: <{0}>";


    public const string REQUIRED_FIELD_ERROR =
        "Field '{0}' value is required";

    public const string SIZE_FIELD_ERROR =
        "Field '{0}' size is exceeded {1} chars";

    public const string REGEXP_FIELD_ERROR =
        "Field '{0}' does not conform to set format: {1}";

    public const string MINMAX_FIELD_ERROR =
        "Field min/max check failed. Field: '{0}' value must be between {1} and {2}";

    public const string LOOKUP_DICTIONARY_FIELD_ERROR =
        "Field '{0}' value is not permitted by lookup dictionary";

    public const string OWNER_BINDING_ERROR =
        "ReleaseOwner() has to be called before re-binding with different owner object";

    public const string ITEM_ALREADY_EXISTS_ERROR =
        "An item is already owned by this context";

    public const string VALUE_ASSIGNMENT_ERROR =
        "Invalid value assignment for this field type";

    public const string INVALID_ARGUMENT_ERROR =
        "Invalid argument supplied";

    public const string INVALID_OPERATION_ERROR =
        "Invalid operation: ";

    public const string READONLY_COLLECTION_MUTATION_ERROR =
        "Can not mutate read-only collection: ";

    public const string DUPLICATE_FIELD_NAME_ERROR =
        "Field with such a name already exists in a containing record. Field name: <{0}>";

    public const string FIELD_NAME_NOT_FOUND_ERROR =
        "Field with such a name was not found. Field name: <{0}>";

    public const string FAILED_TO_CREATE_FILE_ERROR =
        "{0} failed to create file {1}: ";

    public const string FAILED_TO_ALLOCATE_RESOURCE_ERROR =
        "{0} failed to allocate resource: ";

    public const string NOT_EDIT_STATE_ERROR =
        "Model is not in Editing state";

    public const string NOT_CREATE_EDIT_STATE_ERROR =
        "Model is not in Creating or Editing state";

    public const string NOT_UNINIT_VIEW_STATE_ERROR =
        "Model is not in Uninitialized or View state";

    public const string NOT_LOADING_STATE_ERROR =
        "Model is not in Loading/Initializing state";

    public const string NOT_VALID_ON_LOAD_ERROR =
        "Model is not in valid state after it was loaded/initialized";

    public const string NOT_VALID_ON_POST_ERROR =
        "Model is not in valid state and can not be posted";

    public const string NOT_APPROPRIATE_STATE_FOR_SAVE_ERROR =
        "Model is not in appropriate state to be saved";

    public const string NOT_APPROPRIATE_STATE_FOR_DELETE_ERROR =
        "Model is not in appropriate state to be deleted";

    public const string STORAGE_OPERATION_IN_PROGRESS_ERROR =
        "Model storage operation is already in progress";


    public const string CHECK_LIST_ALREADY_RUN_ERROR =
        "Checklist has already been run";

    public const string OBJSTORESVC_PROVIDER_CONFIG_ERROR =
            "ObjectStoreService could not configure provider: ";

    public const string INSTRUMENTATIONSVC_PROVIDER_CONFIG_ERROR =
            "InstrumentationService could not configure provider: ";

    public const string CAN_NOT_CREATE_MORE_SCOPE_EXPRESSIONS_ERROR =
        "Can not create more expression instances in this scope as expression evaluation assembly is already built: ";

    public const string CAN_NOT_ADD_FAILED_SCOPE_COMPILE_ERROR =
        "Can not create more expression instances in this scope as expression evaluation assembly compilation failed: ";

    public const string EXPRESSION_SCOPE_COMPILE_ERROR =
        "Expression scope compilation error: ";



    public const string MODEL_METHOD_NOT_FOUND_ERROR =
     "Model could not find a public callable method to invoke. Method Name: '{0}'";

    public const string MODEL_METHOD_ERROR =
     "Model callable method error: ";

    public const string FIELD_COMPARISON_NOT_IMPLEMENTED_ERROR =
     "Comparison is not implemented at Field abstract level";






    public const string FIELD_ATTRIBUTES_DEFS_ERROR =
       "Error while applying field definitions BuildAndDefineFields('{0}'). Check attributes";

    public const string FIELD_TYPE_MAP_ERROR =
       "Error mapping type '{0}' to record model field. No mapping exists";


    public const string APP_CONFIG_SETTING_NOT_FOUND_ERROR =
        "Application configuration setting \"{0}\" not found.";

    public const string CONFIG_RECURSIVE_VARS_ERROR =
        "Configuration line contains recursive vars that can not be resolved: \"{0}\"";

    public const string CONFIG_NO_PROVIDER_LOAD_FILE_ERROR =
        "No configuration provider can open file name: ";

    public const string CONFIG_NO_PROVIDER_LOAD_FORMAT_ERROR =
        "No configuration provider can open content supplied in this format: ";

    public const string CONFIG_VARS_EVAL_ERROR =
        "Configuration variable '{0}' evaluation error: {1}";


    public const string CONFIG_BEHAVIOR_APPLY_ERROR =
        "Error while applying behavior to {0}. Error: {1}";

    public const string CONFIG_JSON_MAP_ERROR =
        "JSONConfig must be represented by a valid JSON map(hash) with a single root key, not array or multikey map";

    public const string CONFIG_JSON_STRUCTURE_ERROR =
        "JSONConfig wa supplied content with invalid logical structure, all members of an array must be non-null maps that represent config sub-sections";

    public const string WORK_ITEM_NOT_AGGREGATABLE_ERROR =
        "Work item must implement IAggregatableWorkItem interface to be posted in this queue";



        public const string SVCAPP_INSTANCE_NULL_ERROR =
             "ServiceApp instance has not been created";

        public const string SVCAPP_INSTANCE_ALREADY_CREATED_ERROR =
             "ServiceApp instance has already been created";


        public const string APP_INJECTION_TYPE_MISMATCH_ERROR =
              "The injected type is mismatched for entity being configured: ";


        public const string APP_LOG_INIT_ERROR =
              "App log initApplication error: ";

        public const string APP_TIMESOURCE_INIT_ERROR =
              "App time source initApplication error: ";

        public const string APP_EVENT_TIMER_INIT_ERROR =
              "App event timer initApplication error: ";

        public const string APP_INSTRUMENTATION_INIT_ERROR =
              "App instrumentation initApplication error: ";

        public const string APP_THROTTLING_INIT_ERROR =
              "App throttling initApplication error: ";

        public const string APP_DATA_STORE_INIT_ERROR =
              "App data store initApplication error: ";

        public const string APP_ASSEMBLY_PRELOAD_ERROR =
              "App assembly preload from '{0}' error: {1}";

        public const string APP_OBJECT_STORE_INIT_ERROR =
              "App object store initApplication error: ";

        public const string APP_GLUE_INIT_ERROR =
              "App glue initApplication error: ";

        public const string APP_SECURITY_MANAGER_INIT_ERROR =
              "App security manager initApplication error: ";

        public const string APP_APPLY_BEHAVIORS_ERROR =
              "App apply behaviors initApplication error: ";

        public const string APP_FORMS_FACTORY_INIT_ERROR =
              "App forms factory initApplication error: ";

        public const string SVCAPPLICATION_TOPIC =
              "SvcApp";

        public const string APP_FINISH_NOTIFIABLES_ERROR =
              "Application finish notifiables threw exceptions: ";


        public const string APP_STARTER_BEFORE_ERROR =
              "Error calling Starter.ApplicationStartBeforeInit() '{0}'. Exception: {1}";

        public const string APP_STARTER_AFTER_ERROR =
              "Error calling Starter.ApplicationStartAfterInit() '{0}'. Exception: {1}";


        public const string APP_FINISH_NOTIFIABLE_BEFORE_ERROR =
              "Error calling notifiable.ApplicationFinishBeforeCleanup() '{0}'. Exception: {1}";

        public const string APP_FINISH_NOTIFIABLE_AFTER_ERROR =
             "Error calling notifiable.ApplicationFinishAfterCleanup() '{0}'. Exception: {1}";



   public const string TEMPLATE_COMPILER_ALREADY_COMPILED_ERROR =
        "Operation not applicable as target TemplateCompiler instance has already been compiled. Compiler class: ";


    public const string TEMPLATE_CS_COMPILER_UNMATCHED_SPAN_ERROR =
        "Span started on line {0} and is unmatched up until line {1}";

    public const string TEMPLATE_CS_COMPILER_EMPTY_EXPRESSION_ERROR =
        "Empty expression on line {0}";

    public const string TEMPLATE_CS_COMPILER_CONFIG_CLOSE_TAG_ERROR =
        "Missing config close tag";

    public const string TEMPLATE_CS_COMPILER_CONFIG_ERROR =
        "Configuration segment error: ";


    public const string PARAMETER_MAY_NOT_BE_NULL_ERROR =
        "Parameter '{0}' may not be null";


    public const string STREAM_READ_EOF_ERROR =
        "Stream EOF before operation could complete: ";


    public const string SLIM_READ_X_ARRAY_MAX_SIZE_ERROR =
        "Slim reader could not read requested array of {0} {1} as it exceeds the maximum limit of {2} bytes'";

    public const string SLIM_WRITE_X_ARRAY_MAX_SIZE_ERROR =
        "Slim writer could not write requested array of {0} {1} as it exceeds the maximum limit of {2} bytes'";



    public const string SLIM_SERIALIZATION_EXCEPTION_ERROR =
        "Exception in SlimSerializer.Serialize():  ";

    public const string SLIM_DESERIALIZATION_EXCEPTION_ERROR =
        "Exception in SlimSerializer.Deserialize():  ";


    public const string SLIM_DESERIALIZE_CALLBACK_ERROR =
        "Exception leaked from OnDeserializationCallback() invoked by SlimSerializer. Error:  ";

    public const string SLIM_ISERIALIZABLE_MISSING_CTOR_ERROR =
        "ISerializable object does not implement .ctor(SerializationInfo, StreamingContext): ";

    public const string SLIM_BAD_HEADER_ERROR =
        "Bad SLIM format header";

    public const string SLIM_TREG_COUNT_ERROR =
        "Slim type registry count mismatch";

    public const string SLIM_TREG_CSUM_ERROR =
        "Slim type registry CSUM mismatch";


    public const string SLIM_HNDLTOREF_MISSING_TYPE_NAME_ERROR =
        "HandleToReference(). Missing type name: ";


    public const string SLIM_ARRAYS_TYPE_NOT_ARRAY_ERROR =
        "DescriptorToArray(). Type is not array : ";

    public const string SLIM_ARRAYS_MISSING_ARRAY_DIMS_ERROR =
        "DescriptorToArray(). Missing array dimensions: ";

    public const string SLIM_ARRAYS_OVER_MAX_DIMS_ERROR =
        "Slim does not support an array with {0} dimensions. Only up to {1} maximum array dimensions supported";

     public const string SLIM_ARRAYS_OVER_MAX_ELM_ERROR =
        "Slim does not support an array with {0} elements. Only up to {1} maximum array elements supported";

    public const string SLIM_ARRAYS_WRONG_ARRAY_DIMS_ERROR =
        "DescriptorToArray(). Wrong array dimensions: ";

    public const string SLIM_ARRAYS_ARRAY_INSTANCE_ERROR =
        "DescriptorToArray(). Error instantiating array '";

    public const string SLIM_SER_PROHIBIT_ERROR =
        "Slim can not process type '{0}' as it is marked with [{1}] attribute";


    public const string POD_ISERIALIZABLE_MISSING_CTOR_ERROR =
        "ISerializable object does not implement .ctor(SerializationInfo, StreamingContext): ";

    public const string POD_DONT_KNOW_HOWTO_DESERIALIZE_FROM_CUSTOM_DATA =
        "PortableObjectDocument can not deserialize an instance of '{0}' as the type foes not provide neither PortableObjectDocumentDeserializationTransform nor ISerializable impmenetation";

    public const string CODE_LOGIC_ERROR =
        "Logic error in code: ";


    public const string IO_FS_ITEM_IS_READONLY_ERROR =
        "FileSystem '{0}' item '{1}' is read only";


    public const string GLUE_SHUTTING_DOWN_REPORT =
        "Component {0} is shutting down";

    public const string GLUE_CLIENT_CONNECTION_REPORT =
        "Transport {0} {1} client connection {2}{3}";

    public const string GLUE_TRANSPORT_CONNECTED_REPORT =
        "Transport {0} connected to address {1}";

    public const string GLUE_TRANSPORT_DISCONNECT_REPORT =
        "Transport {0} disconnected from address {1}: {2}";

    public const string GLUE_NAMED_BINDING_NOT_FOUND_ERROR =
        "The binding could not be located by name: ";

    public const string GLUE_CLIENT_CALL_ERROR =
        "Client call failed. Status: ";

    public const string GLUE_SYSTEM_NOT_RUNNING_ERROR =
        "Application, Glue or its components is not running/maybe shutting down";

    public const string GLUE_CLIENT_CALL_TRANSPORT_ACQUISITION_TIMEOUT_ERROR =
        "Binding '{0}' could not acquire client transport for making a call after waiting {1} ms. Revise limits in client transport binding config";

     public const string GLUE_CLIENT_CALL_NO_BINDING_ERROR =
        "'{0}' client call failed because there is no binding available";

    public const string GLUE_CALL_SERVICED_BY_DIFFERENT_REACTOR_ERROR =
        "The call is already serviced by different reactor instance";

    public const string GLUE_NO_INPROC_MATCHING_SERVER_ENDPOINT_ERROR =
        "No matching inrpoc server endpoint found for: ";

    public const string GLUE_MSG_DUMP_PATH_INVALID_ERROR =
        "Msg dump path is invalid: ";

    public const string GLUE_MSG_DUMP_FAILURE_ERROR =
        "Msg dump failure: ";

    public const string GLUE_DUPLICATE_NAMED_INSTANCE_ERROR =
        "Duplicate named instance: ";

    public const string GLUE_ONE_WAY_RESPONSE_ERROR =
        "Attempting to obtain response for a [OneWay] call";

    public const string GLUE_POOL_ITEM_ALLOCATION_ERROR =
        "Pool item allocation error: ";

    public const string GLUE_SERVER_ENDPOINT_CONTRACT_SERVER_TYPE_ERROR =
        "Server endpoint contract server name could not be resolved to type: ";

    public const string GLUE_TYPE_SPEC_ERROR =
        "The type specification could not be resolved to actual type. Make sure that contract assembly is present on both client and server: ";

    public const string GLUE_METHOD_SPEC_UNSUPPORTED_ERROR =
        "Method '{0}'.'{1}' is unsupported by Glue as it contains '{2}' parameter which is either generic, OUT or REF";

    public const string GLUE_METHOD_ARGS_MARSHAL_LAMBDA_ERROR =
        "Could not compile dynamic lamda for args marshalling for method '{0}'.'{1}'. Exception: {2}";


    public const string GLUE_NO_SERVER_INSTANCE_ERROR =
        "Attempt to invoke instance method but no server instance identified/allocated yet. Could not invoke server contract method: {0}.{1}";


    public const string GLUE_NO_ARGS_MARSHAL_LAMBDA_ERROR =
        "The server received dynamic RequestMsg-derivative for '{0}.{1}' however no dynamic lambda was compiled. This may be due to the mismatch between [ArgsMarshalling] on server contract method and the client";


    public const string GLUE_SERVER_CONTRACT_METHOD_INVOCATION_ERROR =
        "Contract method '{0}.{1}' threw exception: {2}";

    public const string GLUE_AMBIGUOUS_CTOR_DCTOR_DEFINITION_ERROR =
        "Ambiguous constructor/destructor definition. Could not invoke server contract method: {0}.{1}";

    public const string GLUE_ARGS_MARSHALLING_INVALID_REQUEST_MSG_TYPE_ERROR =
        "Invalid type supplied into ArgsMarshalling attribute. The request message type must be RequestMsg-derived and not be RequestAnyMsg";

    public const string GLUE_NATIVE_SERIALIZATION_WRONG_ROOT_ERROR =
        "Wrong root type passed to native MsgSerializer";

    public const string GLUE_NATIVE_SERIALIZATION_WRONG_ROOT_TOKEN_IN_STREAM_ERROR =
        "Wrong root token in stream read by MsgSerializer";

    public const string GLUE_ENDPOINT_CONTRACT_NOT_IMPLEMENTED_ERROR =
        "Server endpoint '{0}' does not implement contract: {1}";

    public const string GLUE_ENDPOINT_CONTRACT_INTF_MAPPING_ERROR =
        "Server endpoint '{0}' contract '{1}' method '{2}' could not be mapped to method in implementor '{3}'";

     public const string GLUE_ENDPOINT_MSPEC_NOT_FOUND_ERROR =
        "Server endpoint '{0}' contract '{1}' method spec '{2}' not found";


    public const string GLUE_ENDPOINT_CONTRACT_MANY_SERVERS_WARNING =
        "More than one server classes implement requested contract {0} at server endpoint '{1}'";

    public const string GLUE_SERVER_ONE_WAY_CALL_ERROR =
        "Server-side error while processing OneWay call: ";

    public const string GLUE_SERVER_HANDLER_ERROR =
        "Server-side error in handler: ";

    public const string GLUE_SERVER_INSTANCE_ACTIVATION_ERROR =
        "Server instance activation error for: ";

    public const string GLUE_STATEFUL_SERVER_INSTANCE_DOES_NOT_EXIST_ERROR =
        "Stateful server instance does not exist/may have expired or object store is not configured: ";

    public const string GLUE_STATEFUL_SERVER_INSTANCE_LOCK_TIMEOUT_ERROR =
        "Stateful server instance is not marked as [ThreadSafe] and could not be locked before timeout has passed: ";

    public const string GLUE_CLIENT_INSPECTORS_THREW_ERROR =
        "Client-side inspectors threw error upon arrived response message processing: ";

    public const string GLUE_CLIENT_CONNECT_ERROR =
        "Error connecting client {0} to address {1}: ";

    public const string GLUE_LISTENER_EXCEPTION_ERROR =
        "Listener threw exception: ";

    public const string GLUE_CLIENT_THREAD_ERROR =
        "Exception in client processing thread: ";

    public const string GLUE_CLIENT_THREAD_COMMUNICATION_ERROR =
        "Communication exception in client processing thread. Channel will be closed: ";

    public const string GLUE_UNEXPECTED_MSG_ERROR =
        "Received unexpected message. Expected: ";

    public const string GLUE_BAD_PROTOCOL_FRAME_ERROR =
        "Bad protocol transport frame: ";

     public const string GLUE_BAD_PROTOCOL_CLIENT_SITE_ERROR =
        "Bad protocol transport client site frame: ";

    public const string GLUE_MAX_MSG_SIZE_EXCEEDED_ERROR =
        "Message size of {0} bytes exceeds limit of {1} bytes. Operation: '{2}'";

    public const string GLUE_MPX_SOCKET_SEND_CLOSED_ERROR =
        "Glue mpx socket is closed and can not send data";

    public const string GLUE_MPX_SOCKET_SEND_CHUNK_ALREADY_GOTTEN_ERROR =
        "Glue mpx socket send chunk is already reserved/gotten and not released yet. Keep in mind that MpxSocket is NOT THREAD SAFE for sends";

    public const string GLUE_MPX_SOCKET_RECEIVE_ACTION_ERROR =
        "Glue mpx socket receive action leaked: ";


    public const string ASSERTION_ERROR =
        "Assertion failure";


    public const string CA_PROCESSOR_EXCEPTION_ERROR =
        "{0} processor {1} error: {2}";


    public const string SCHEMA_INCLUDE_FILE_DOSNT_EXIST_ERROR =
        "Relational schema include file does not exist: ";

    public const string SCHEMA_INCLUDE_FILE_REFERENCED_MORE_THAN_ONCE_ERROR =
        "Relational schema include file is referenced more than once. Cycle possible. File: ";

    public const string SCHEMA_HAS_ALREADY_COMPILED_OPTION_CHANGE_ERROR =
        "Can not change options on '{0}' after compilation happened";

    public const string RELATIONAL_COMPILER_OUTPUT_PATH_DOSNT_EXIST_ERROR =
        "Relational schema compiler output path does not exist: ";


    public const string RELATIONAL_COMPILER_INCLUDE_SCRIPT_NOT_FOUND_ERROR =
        "Relational schema compiler include script '{0}' at node '{1}' not found";


    public const string JSON_SERIALIZATION_MAX_NESTING_EXCEEDED_ERROR =
              "JSONWriter can not serialize object graph as it exceeds max nesting level of {0}. Graph may contain reference cycles";


    public const string CRUD_CURSOR_ALREADY_ENUMERATED_ERROR = "CRUD Cursor was already enumerated";

    public const string CRUD_FIELDDEF_ATTR_MISSING_ERROR = "CRUD FieldDef must be constructed using at least one [Field] attribute. Name: '{0}'";

    public const string CRUD_FIELD_VALUE_REQUIRED_ERROR = "Field value is required";

    public const string CRUD_FIELD_VALUE_IS_NOT_IN_LIST_ERROR = "Field value is not in list of permitted values";

    public const string CRUD_FIELD_VALUE_MIN_LENGTH_ERROR = "Field value is shorter than min length";

    public const string CRUD_FIELD_VALUE_MAX_LENGTH_ERROR = "Field value exceeds max length";

    public const string CRUD_FIELD_VALUE_SCREEN_NAME_ERROR = "Field value is not a valid screen name ID";

    public const string CRUD_FIELD_VALUE_EMAIL_ERROR = "Field value is not a valid EMail";

    public const string CRUD_FIELD_VALUE_PHONE_ERROR = "Field value is not a valid Telephone";

    public const string CRUD_FIELD_VALUE_REGEXP_ERROR = "Field value is not valid per defined format: {0}";

    public const string CRUD_FIELD_VALUE_MIN_BOUND_ERROR = "Field value is below the permitted min bound";

    public const string CRUD_FIELD_VALUE_MAX_BOUND_ERROR = "Field value is above the permitted max bound";

    public const string CRUD_FIELD_VALUE_GET_ERROR = "CRUD field '{0}' value get error: {1}";

    public const string CRUD_FIELD_VALUE_SET_ERROR = "CRUD field '{0}' value set error: {1}";

    public const string CRUD_GDID_ERA_CONVERSION_ERROR = "CRUD field '{0}' value conversion: GDID value with Era!=0 can not be converted to type '{1}'";

    public const string CRUD_FIELD_NOT_FOUND_ERROR = "CRUD field '{0}' not found in schema '{1}'";

    public const string CRUD_QUERY_RESOLVER_ALREADY_STARTED_ERROR = "CRUD QueryResolver already started to be used and can not be configured";

    public const string CRUD_FIELD_ATTR_METADATA_PARSE_ERROR = "Field attribute metadata parse exception: '{0}'. Metadata: {1}";

    public const string CRUD_FIELD_ATTR_PROTOTYPE_CTOR_ERROR = "Field attribute construction from prototype error: {0}";

    public const string CRUD_TYPED_ROW_RECURSIVE_FIELD_DEFINITION_ERROR = "Typedrow '{0}' recursive field definition. Check for [Field(prototype..)] cycle";

    public const string CRUD_TYPED_ROW_SINGLE_CLONED_FIELD_ERROR = "Typedrow '{0}' defines field clone via [Field(....)]]'{1}' in which case only a single [Field(....)] decoration is allowed";

    public const string CRUD_TYPED_ROW_CLONED_FIELD_NOTEXISTS_ERROR = "Typedrow '{0}' defines field clone via [Field(....)]]'{1}' but there is no field with such name in the cloned-from type";

    public const string CRUD_TYPE_IS_NOT_DERIVED_FROM_ROW_ERROR = "CRUD supplied type of '{0}' is not a Row-derivative";

    public const string CRUD_TYPE_IS_NOT_DERIVED_FROM_TYPED_ROW_ERROR = "CRUD supplied type of '{0}' is not a TypedRow-derivative";

    public const string CRUD_FIND_BY_KEY_LENGTH_ERROR = "CRUD table FindByKey/KeyRowFromValues was supplied wrong number of key field values";

    public const string CRUD_ROWSET_OPERATION_ROW_IS_NULL_OR_SCHEMA_MISMATCH_ERROR = "CRUD rowset was supplied either a null row or a row with a different schema";

    public const string CRUD_TRANSACTION_IS_NOT_OPEN_ERROR = "CRUD transaction is not open for requested operation '{0}'. Current transaction status: '{0}'";

    public const string CRUD_OPERATION_NOT_SUPPORTED_ERROR = "CRUD operation not supported: ";

    public const string CRUD_QUERY_SOURCE_PRAGMA_ERROR = "CRUD query source pragma error in line number {0}: '{1}'. Error: {2}";

    public const string CRUD_QUERY_SOURCE_PRAGMA_LINE_ERROR = "PRAGMA line error Name: '{0}' Value: '{1}'";

    public const string CRUD_QUERY_RESOLUTION_ERROR = "CRUD query '{0}' could not be resolved: {1}";

    public const string CRUD_QUERY_RESOLUTION_NO_HANDLER_ERROR = "no handler matches query name";

    public const string CRUD_CONFIG_EMPTY_LOCATIONS_WARNING = "CRUD configuration contains empty location entries which are ignored";

    public const string CRUD_OPERATION_CALL_CONTEXT_SCOPE_MISMATCH_ERROR = "CRUDOperationCallContext scope mismatch error";

    public const string DISTRIBUTED_DATA_GDID_CTOR_ERROR = "GDID can not be created from the supplied: 'authority={0}>{1},counter={2}>{3}'";

    public const string DISTRIBUTED_DATA_GDID_PARSE_ERROR = "String value '{0}' can not be parsed as GDID";

    public const string DISTRIBUTED_DATA_PARCEL_INVALID_OPERATION_ERROR = "Error performing '{0}' operation on parcel '{1}' because parcel is in '{2}' state";

    public const string DISTRIBUTED_DATA_PARCEL_MERGE_NOT_IMPLEMENTED_ERROR = "Error performing Merge() operation on parcel '{0}' because DoMerge() is not implemented. Either 'parcel.MetadataAttribute.SupportsMerge' was not checked before making this call, or forgot to implement DoMerge()";

    public const string DISTRIBUTED_DATA_PARCEL_SEAL_VALIDATION_ERROR = "Error sealing parcel '{0}' due to validation errors: ";

    public const string DISTRIBUTED_DATA_PARCEL_UNWRAP_FORMAT_ERROR = "Parcel '{0}' can not unwrap the payload as its format '{1}' is not handled";
    public const string DISTRIBUTED_DATA_PARCEL_UNWRAP_DESER_ERROR = "Parcel '{0}' could not unwrap the payload due to deserialization exception: {1}";


    public const string DISTRIBUTED_DATA_PARCEL_MISSING_ATTRIBUTE_ERROR = "Parcel '{0}' does not specify the required [DataParcel(...)] attribute in its declaration";

    public const string ELINK_CHAR_COMBINATION_ERROR = "ELink '{0}' could not be read as it contains an invalid combination '{1}'";
    public const string ELINK_CHAR_LENGTH_LIMIT_ERROR = "ELink '{0}...' could not be encoded/decoded as it exceeds maximum permissible length";
    public const string ELINK_SEGMENT_LENGTH_ERROR = "ELink '{0}' could not be read as it contains an invalid segment data length";
    public const string ELINK_CHAR_LENGTH_ERROR = "ELink '{0}' could not be read as it contains an invalid character data length";
    public const string ELINK_CSUM_MISMATCH_ERROR = "ELink '{0}' could not be read as its checksum does not match";


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

    public const string CACHE_VALUE_FACTORY_ERROR = "Cache value factory func threw error from {0}. Exception: {1}";
    public const string CACHE_RECORD_ITEM_DISPOSE_ERROR = "Cache value threw error while trying to be disposed from {0}. Exception: {1}";

    public const string CACHE_TABLE_CTOR_SIZES_WARNING =
                      "Cache.Table.ctor(bucketCount==recPerPage), two parameters may not be equal because they will cause hash clustering. The 'recPerPage' has been increased by the system";


    public const string LOCAL_INSTALL_ROOT_PATH_NOT_FOUND_ERROR = "Local installation root path '{0}' does not exist";

    public const string LOCAL_INSTALL_NOT_STARTED_INSTALL_PACKAGE_ERROR = "Local installation not started but InstallPackage() was called";

    public const string LOCAL_INSTALL_PACKAGES_MANIFEST_FILE_NAME_COLLISION_ERROR =
      "Packages manifest file '{0}' could not be saved locally as some packages contain files that collide with the name";

    public const string LOCAL_INSTALL_LOCAL_MANIFEST_READ_ERROR =
      "Local installation can not open local manifest file '{0}'. Exception: {1}";

    public const string LOCAL_INSTALL_INSTALL_SET_PACKAGE_MANIFEST_READ_ERROR =
      "Local installation can not install a package '{0}' from the install set. The package manifest could not be read with exception: {1}";

    public const string LOCAL_INSTALL_INSTALL_SET_PACKAGE_WITHOUT_MANIFEST_ERROR =
      "Local installation can not install a package '{0}' from the install set. The package does not contain a manifest file '{1}' in its root";


    public const string FS_DUPLICATE_NAME_ERROR = "Can not have file system instance of type '{0}' with the name '{1}' as this name is already registered. ";

    public const string FS_SESSION_BAD_PARAMS_ERROR =
      "Can not create an instance of file system session '{0}'. Make sure that suitable derivative of FileSystemSessionConnectParams is passed for the particular file system";

    public const string NETGATE_CONFIG_DUPLICATE_ENTITY_ERROR =
              "NetGate configuration specifies duplicate name '{0}' for '{1}'";

    public const string NETGATE_VARDEF_NAME_EMPTY_CTOR_ERROR =
              "NetGate.VarDef must have a valid name in config or passed to .ctor";

    public const string TEXT_PATTERN_MULTI_WC_ERROR =
              "Error in text match pattern '{0}' as it contains more than one multi-char wildcard '{1}'";



    public const string FINANCIAL_AMOUNT_DIFFERENT_CURRENCIES_ERROR =
            "Financial operation '{0}' could not proceed. Amounts '{1}' and '{2}' are in different currencies";

    public const string FINANCIAL_AMOUNT_PARSE_ERROR =
            "Could not parse amount '{0}'";

    public const string PILE_AV_BAD_SEGMENT_ERROR =  "Pile access violation. Bad segment: ";

    public const string PILE_AV_BAD_ADDR_EOF_ERROR =  "Pile access violation. Bad address points beyond buf length: ";

    public const string PILE_AV_BAD_ADDR_CHUNK_FLAG_ERROR =  "Pile access violation. Does not point to a valid used object chunk: ";

    public const string PILE_AV_BAD_ADDR_PAYLOAD_SIZE_ERROR =  "Pile access violation. Pointer '{0}' point to wrong chunk payload size '{1}";

    public const string PILE_CHUNK_SZ_ERROR =  "Pile free chunk sizes are not properly configured. Must be consequently increasing {0} sizes starting at {1} bytes at minimum";

    public const string PILE_CONFIG_PROPERTY_ERROR =  "Pile configuration error in property '{0}'. Error: {1}";

    public const string PILE_CRAWL_INTERNAL_SEGMENT_CORRUPTION_ERROR =  "Pile segment crawl internal error: chunk flag corrupted at address {0}";

    public const string PILE_OUT_OF_SPACE_ERROR =  "Pile is out of allowed space of {0:n} max bytes, {1} max segments @ {2:n} bytes/segment";

    public const string PILE_OBJECT_LARGER_SEGMENT_ERROR =  "Pile could not put object of {0} bytes as this size exceeds the size of a segment";


    public const string IO_STREAM_POSITION_ERROR =  "Stream position of {0} is beyond the length of stream {1}";

    public const string IO_STREAM_NOT_SUPPORTED_ERROR =  "Stream {0} does not support '{1}'";



    public const string PILE_CACHE_SCV_START_PILE_NULL_ERROR =
      "Pile cache service can not start because pile is null";


    public const string PILE_CACHE_SCV_START_PILE_NOT_STARTED_ERROR =
      "Pile cache service can not start as it is injected a pile instance which is not managed by cache but has not been started externally";


    public const string PILE_CACHE_TBL_KEYTYPE_MISMATCH_ERROR =
      "Key type mismatch for pile cache table '{0}'. Existing: '{1}' Requested: '{2}'";

    public const string PILE_CACHE_TBL_DOES_NOT_EXIST_ERROR =
      "Pile cache table '{0}' does not exist in the cache";

    public const string PILE_CACHE_TBL_KEYCOMPARER_MISMATCH_ERROR =
      "Key comparer mismatch for pile cache table '{0}'. Existing: '{1}' Requested: '{2}'";


    public const string PDF_UNIT_INCONSISTENCY_ERROR =
        "PDF Unit '{0}' is inconsistently defined";

    public const string PDF_UNIT_DOESNOTEXIST_ERROR =
        "PDF Unit '{0}' does not exist";

    public const string PDF_COLOR_DOESNOTEXIST_ERROR =
        "PDF Color '{0}' does not exist";

    public const string PDF_COLOR_INCONSISTENCY_ERROR =
        "PDF Color '{0}' is inconsistently defined";



    public const string BSON_DOCUMENT_SIZE_EXCEEDED_ERROR =
        "Size '{0}' exceeds BSON default document size {1}";

    public const string BSON_ELEMENT_OBJECT_VALUE_SET_ERROR =
        "Can not set the '{0}' value of BSON element '{1}'. Error: {2}";

    public const string BSON_READ_PREMATURE_EOF_ERROR =
        "Premature EOF while doing '{0}' over BSON stream";

    public const string BSON_NAMED_ELEMENT_ADDED_ERROR =
        "The element with name '{0}' has already been added";

    public const string BSON_TYPE_NOT_SUPORTED_ERROR =
        "BSON type '{0}' is not supported. BSON source is likely corrupted";

    public const string BSON_DOCUMENT_RECURSION_ERROR =
        "BSONDocument recursion detected";

    public const string BSON_ARRAY_ELM_NAME_ERROR =
        "BSONElement.Name can not be accessed because it is an array element. Check IsArrayElement";

    public const string BSON_TEMPLATE_COMPILE_ERROR =
        "BSONDocument template compilation error: {0}. Template source: ' {1} '";

    public const string BSON_ARRAY_ELM_DOC_ERROR =
        "BSONElement '{0}' is an array element and can not be used in document directly";

    public const string BSON_OBJECTID_LENGTH_ERROR =
        "Byte array length must be equal to 12";

    public const string BSON_THREE_BYTES_UINT_ERROR =
        "The value of {0} should be less than 2^24 to be correctly encoded as 3-bytes";

    public const string BSON_UNEXPECTED_END_OF_STRING_ERROR =
        "Unexpected end of string. Expected: 0x00";

    public const string BSON_INCORRECT_STRING_LENGTH_ERROR =
        "BSON source is corrupted. String length '{0}' should be positive integer";

    public const string BSON_READ_BOOLEAN_ERROR =
        "BSON source is corrupted. Unexpected boolean value '{0}'";

    public const string BSON_EOD_ERROR =
        "BSON source is corrupted. Incorrect end of document/array";

    public const string CLR_BSON_CONVERSION_TYPE_NOT_SUPPORTED_ERROR =
        "CLR type '{0}' conversion into BSON is not supported";

    public const string CLR_BSON_CONVERSION_REFERENCE_CYCLE_ERROR =
        "CLR value of type '{0}' could not be converted into BSON as there is a reference cycle";

    public const string DECIMAL_OUT_OF_RANGE_ERROR =
        "Decimal value of {0} is outside of to-int64 convertable range of {1}..{2}";

    public const string BUFFER_LONGER_THAN_ALLOWED_ERROR =
        "Byte[] buffer has a length of {0} bytes which is over the allowed maximum of {1} bytes";

    public const string GDID_BUFFER_ERROR =
        "Error converting GDID data buffer: {0}";


    public const string SECDB_FILE_HEADER_ERROR = "Eror while parsing the SecDB file header: ";

    public const string SECDB_FS_SEEK_STREAM_ERROR =
       "SecDB requires a file system that supports random file access with content stream seek. Passed '{0}' does not";

    public const string SECDB_FILE_NOT_FOUND_ERROR =
       "SecDB file '{0}' could not be read as it was not found by the file system '{1}'";

    public const string SEALED_STRING_OUT_OF_SPACE_ERROR =  "SealedString has allocated a maximum of {0} segments";
    public const string SEALED_STRING_TOO_BIG_ERROR =  "SealedString value of {0} bytes is too big";
  }
}
