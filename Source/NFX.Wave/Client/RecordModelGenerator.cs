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

using NFX.DataAccess;
using NFX.DataAccess.CRUD;
using NFX.DataAccess.Distributed;
using NFX.Environment;
using NFX.Serialization.JSON;

namespace NFX.Wave.Client
{
  /// <summary>
  /// Invoked by generator to obtain localized versions of certain properties
  /// </summary>
  /// <param name="sender">Generator</param>
  /// <param name="schema">Name of schema, i.e. Type of row</param>
  /// <param name="field">Name of property/field</param>
  /// <param name="value">English value</param>
  /// <param name="isoLang">Requested language or null</param>
  /// <returns>Localized value, i.e. depending on user culture looks-up culture-specific version of value</returns>
  public delegate string ModelLocalizationEventHandler(RecordModelGenerator sender, string schema, string field, string value, string isoLang);


  /// <summary>
  /// Invoked by generator to obtain a list of dynamic lookup values for a field.
  /// This event is invoked ONLY for fields that DO NOT have valueList specified
  /// </summary>
  /// <param name="sender">Generator</param>
  /// <param name="row">Row that data model is generated for</param>
  /// <param name="fdef">Field definition</param>
  /// <param name="target">Target name</param>
  /// <param name="isoLang">Desired isoLang for localization</param>
  /// <returns>JSONDataMap populated by business logic or null to indicate that no lookup values are available</returns>
  public delegate JSONDataMap ModelFieldValueListLookupFunc(RecordModelGenerator sender,
                                                            Row  row,
                                                            Schema.FieldDef fdef,
                                                            string target,
                                                            string isoLang);

  /// <summary>
  /// Facilitates tasks of JSON generation for record models/rows as needed by WV.RecordModel client library.
  /// This class does not generate nested models, only flat models for particular row
  /// (i.e. id row has a complex type field, it will be serialized as "object")
  /// </summary>
  public class RecordModelGenerator
  {

      private volatile static RecordModelGenerator s_DefaultInstance;

      /// <summary>
      /// Returns Default instance for simple use-cases
      /// </summary>
      public static RecordModelGenerator DefaultInstance
      {
        get
        {
          if (s_DefaultInstance==null) //not thread safe its ok as instance is very lite
            s_DefaultInstance = new RecordModelGenerator();

          return s_DefaultInstance;
        }
      }

      public RecordModelGenerator(){ }
      public RecordModelGenerator(IConfigSectionNode conf){ }


      public static readonly string[] METADATA_FIELDS = {
            "Description",
            "Placeholder",
            "Type",
            "Kind",
            "Case",
            "Stored",
            "Required",
            "Applicable",
            "Enabled",
            "ReadOnly",
            "Visible",
            "Password",
            "MinValue",
            "MaxValue",
            "MinSize",
            "Size",
            "ControlType",
            "DefaultValue",
            "Hint",
            "Marked",
            "LookupDict",
            "Lookup",
            "DeferValidation",
            "ScriptType"};


      /// <summary>
      /// Allows localization framework to install hook so that values can get translated per user-selected culture.
      /// The hook functor has the following signature: (string propertyName, string propValue): string.
      /// Returns translated propValue.
      /// </summary>
      public event ModelLocalizationEventHandler ModelLocalization;


      /// <summary>
      /// Generates JSON object suitable for passing into WV.RecordModel.Record(...) constructor on the client.
      /// Pass target to select attributes targeted to ANY target or to the specified one, for example
      ///  may get attributes for client data entry screen that sees field metadata differently, in which case target will reflect the name
      ///   of the screen
      /// </summary>
      public virtual JSONDataMap RowToRecordInitJSON(Row row,
                                                     Exception validationError,
                                                     string recID = null,
                                                     string target = null,
                                                     string isoLang = null,
                                                     ModelFieldValueListLookupFunc valueListLookup = null)
      {
        var result = new JSONDataMap();
        if (row==null) return result;
        if (recID.IsNullOrWhiteSpace()) recID = Guid.NewGuid().ToString();

        result["OK"] = true;
        result["ID"] = recID;
        if (isoLang.IsNotNullOrWhiteSpace())
          result["ISOLang"] = isoLang;

        //20140914 DKh
        var form = row as FormModel;
        if (form != null)
        {
          result[FormModel.JSON_MODE_PROPERTY] = form.FormMode;
          result[FormModel.JSON_CSRF_PROPERTY] = form.CSRFToken;

          //20160123 DKh
          if (form.HasRoundtripBag)
            result[FormModel.JSON_ROUNDTRIP_PROPERTY] = form.RoundtripBag.ToJSON(JSONWritingOptions.CompactASCII);
        }

        var fields = new JSONDataArray();
        result["fields"] = fields;

        var schemaName = row.Schema.Name;
        if (row.Schema.TypedRowType!=null) schemaName = row.Schema.TypedRowType.FullName;

        foreach(var sfdef in row.Schema.FieldDefs.Where(fd=>!fd.NonUI))
        {
          var fdef = row.GetClientFieldDef(this, sfdef, target, isoLang);
          if (fdef==null || fdef.NonUI) continue;

          var fld = new JSONDataMap();
          fields.Add(fld);
          fld["def"] = FieldDefToJSON(row, schemaName, fdef, target, isoLang, valueListLookup);
          var val = row.GetClientFieldValue(this, sfdef, target, isoLang);
          if (val is GDID && ((GDID)val).IsZero) val = null;
          fld["val"] = val;
          var ferr = validationError as CRUDFieldValidationException;
          //field level exception
          if (ferr!= null && ferr.FieldName==fdef.Name)
          {
            fld["error"] = ferr.ToMessageWithType();
            fld["errorText"] = OnLocalizeString(schemaName, "errorText", ferr.ClientMessage, isoLang);
          }
        }

        //record level
        if (validationError!=null && !(validationError is CRUDFieldValidationException))
        {
          result["error"] = validationError.ToMessageWithType();
          result["errorText"] = OnLocalizeString(schemaName, "errorText", validationError.Message, isoLang);
        }

        return result;
      }


      protected virtual JSONDataMap FieldDefToJSON(Row row,
                                                   string schema,
                                                   Schema.FieldDef fdef,
                                                   string target,
                                                   string isoLang,
                                                   ModelFieldValueListLookupFunc valueListLookup)
      {
        var result = new JSONDataMap();

        result["Name"] = fdef.Name;
        result["Type"] = MapCLRTypeToJS(fdef.NonNullableType);
        var key = fdef.AnyTargetKey;
        if (key) result["Key"] = key;


        if (fdef.NonNullableType.IsEnum)
        { //Generate default lookupdict for enum
          var names = Enum.GetNames(fdef.NonNullableType);
          var values = new JSONDataMap(true);
          foreach(var name in names)
            values[name] = name;

          result["LookupDict"] = values;
        }

        var attr = fdef[target];
        if (attr!=null)
        {
            if (attr.Description!=null) result["Description"] = OnLocalizeString(schema, "Description", attr.Description, isoLang);
            var str =  attr.StoreFlag==StoreFlag.OnlyStore || attr.StoreFlag==StoreFlag.LoadAndStore;
            if (!str) result["Stored"] = str;
            if (attr.Required) result["Required"] = attr.Required;
            if (!attr.Visible) result["Visible"] = attr.Visible;
            if (attr.Min!=null) result["MinValue"] = attr.Min;
            if (attr.Max!=null) result["MaxValue"] = attr.Max;
            if (attr.MinLength>0) result["MinSize"] = attr.MinLength;
            if (attr.MaxLength>0) result["Size"]    = attr.MaxLength;
            if (attr.Default!=null) result["DefaultValue"] = attr.Default;
            if (attr.ValueList.IsNotNullOrWhiteSpace())
            {
              var vl = OnLocalizeString(schema, "LookupDict", attr.ValueList, isoLang);
              result["LookupDict"] = FieldAttribute.ParseValueListString(vl);
            }
            else
            {
              var valueList = valueListLookup!=null ? valueListLookup(this, row, fdef, target, isoLang)
                                                    : row.GetClientFieldValueList(this, fdef, target, isoLang);

              if (valueList==null && attr.HasValueList)
                valueList = attr.ParseValueList();

              if (valueList!=null)
                result["LookupDict"] = valueList;
            }

            if (attr.Kind!=DataKind.Text) result["Kind"] = MapCLRKindToJS(attr.Kind);

            if (attr.CharCase!=CharCase.AsIs) result["Case"] = MapCLRCharCaseToJS(attr.CharCase);
        }

        if (attr.Metadata!=null)
        {
            foreach(var fn in METADATA_FIELDS)
            {
              var mv = attr.Metadata.AttrByName(fn).Value;
              if (mv.IsNullOrWhiteSpace()) continue;

              if (fn=="Description"||fn=="Placeholder"||fn=="LookupDict"||fn=="Hint")
                mv = OnLocalizeString(schema, fn, mv, isoLang);

              result[fn] = mv;
            }
        }


        return result;
      }



      protected virtual string OnLocalizeString(string schema, string prop, string value, string isoLang)
      {
        if (ModelLocalization==null) return value;
        return ModelLocalization(this, schema, prop, value, isoLang);
      }


      protected virtual string MapCLRTypeToJS(Type tp)
      {
        if (tp==typeof(string) || tp.IsEnum) return "string";

        if (tp==typeof(sbyte)||
            tp==typeof(Int16)||
            tp==typeof(Int32)||
            tp==typeof(Int64)||
            tp==typeof(byte)||
            tp==typeof(UInt16)||
            tp==typeof(UInt32)||
            tp==typeof(UInt64)) return "int";

        if (tp==typeof(float)||
            tp==typeof(double)||
            tp==typeof(decimal)) return "real";

        if (tp==typeof(bool)) return "bool";

        if (tp==typeof(DateTime)) return "datetime";

        if (tp==typeof(GDID)) return "string";

        return "object";
      }


      protected virtual string MapCLRKindToJS(DataKind kind)
      {
        switch (kind)
        {
            case DataKind.DateTimeLocal: return "datetime-local";
            case DataKind.Telephone: return "tel";
            default: return kind.ToString().ToLowerInvariant();
        }
      }

      protected virtual string MapCLRCharCaseToJS(CharCase kind)
      {
        return kind.ToString().ToLowerInvariant();
      }


  }
}
