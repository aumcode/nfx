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
  /// Facilitates tasks of JSON generation for record models/rows as needed by WV.RecordModel client library
  /// </summary>
  public static class RecordModelGenerator
  {
      /// <summary>
      /// Generates JSON object suitable for passing into WV.RecordModel.Record(...) constructor on the client.
      /// Pass target to select attributes targeted to ANY target or to the specified one, for example
      ///  may get attributes for client data entry screen that sees field metadata differently, in which case target will reflect the name
      ///   of the screen
      /// </summary>
      public static JSONDataMap RowToRecordInitJSON(Row row, Exception validationError, string recID = null, string target = null, string isoLang = null)
      {
        var result = new JSONDataMap();
        if (row==null) return result;
        if (recID.IsNullOrWhiteSpace()) recID = Guid.NewGuid().ToString();

        result["OK"] = true;
        result["ID"] = recID;
        if (isoLang.IsNotNullOrWhiteSpace())
          result["ISOLang"] = isoLang;

        //20140914 DKh
        if (row is FormModel)
        {
          result[FormModel.JSON_MODE_PROPERTY] = ((FormModel)row).FormMode;
          result[FormModel.JSON_CSRF_PROPERTY] = ((FormModel)row).CSRFToken;
        }
        
        var fields = new JSONDataArray();
        result["fields"] = fields;

        var schemaName = row.Schema.Name;
        if (row.Schema.TypedRowType!=null) schemaName = row.Schema.TypedRowType.FullName;

        foreach(var fdef in row.Schema.FieldDefs.Where(fd=>!fd.NonUI))
        {
          var fld = new JSONDataMap();
          fields.Add(fld);
          fld["def"] = fieldDefToJSON(schemaName, fdef, target);
          fld["val"] = row.GetFieldValue(fdef);
          var ferr = validationError as CRUDFieldValidationException;
          //field level exception
          if (ferr!= null && ferr.FieldName==fdef.Name)
          {
            fld["error"] = ferr.ToMessageWithType();
            fld["errorText"] = localizeString(schemaName, "errorText", ferr.ClientMessage);
          }
        }

        //record level
        if (validationError!=null && !(validationError is CRUDFieldValidationException))
        {
          result["error"] = validationError.ToMessageWithType();
          result["errorText"] = localizeString(schemaName, "errorText", validationError.Message);
        }

        return result;
      }   

          private static readonly string[] METADATA_FIELDS = {
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
            "DeferValidation"};


          private static JSONDataMap fieldDefToJSON(string schema, Schema.FieldDef fdef, string target)
          {
            var result = new JSONDataMap();

            result["Name"] = fdef.Name;
            result["Type"] = mapType(fdef.NonNullableType);
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
               if (attr.Description!=null) result["Description"] = localizeString(schema, "Description", attr.Description);
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
                 var vl = localizeString(schema, "LookupDict", attr.ValueList);
                 result["LookupDict"] = FieldAttribute.ParseValueListString(vl);
               }
               if (attr.Kind!=DataKind.Text) result["Kind"] = mapKind(attr.Kind);
            }

            if (attr.Metadata!=null)
            {
               foreach(var fn in METADATA_FIELDS)
               { 
                var mv = attr.Metadata.AttrByName(fn).Value; 
                if (mv.IsNullOrWhiteSpace()) continue;
                
                if (fn=="Description"||fn=="Placeholder"||fn=="LookupDict"||fn=="Hint")
                 mv = localizeString(schema, fn, mv);

                result[fn] = mv;
               }
            }


            return result;
          }



          /// <summary>
          /// Invoked by generator to obtain localized versions of certain properties
          /// </summary>
          /// <param name="schema">Name of schema, i.e. Type of row</param>
          /// <param name="field">Name of property/field</param>
          /// <param name="value">English value</param>
          /// <returns>Localized value, i.e. depending on user culture looks-up culture-specific version of value</returns>
          public delegate string LocalizationFunc(string schema, string field, string value);


          /// <summary>
          /// Allows localization framework to install hook so that values can get translated per user-selected culture.
          /// The hook functor has the following signature: (string propertyName, string propValue): string. 
          /// Returns translated propValue.
          /// </summary>
          public static event LocalizationFunc Localization;


          private static string localizeString(string schema, string prop, string value)
          {
            if (Localization==null) return value;
            return Localization(schema, prop, value);
          }


          private static string mapType(Type tp)
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


          private static string mapKind(DataKind kind)
          {
            switch (kind)
            {
               case DataKind.DateTimeLocal: return "datetime-local";
               case DataKind.Telephone: return "tel";
               default: return kind.ToString().ToLowerInvariant();
            }
          }


  }
}
