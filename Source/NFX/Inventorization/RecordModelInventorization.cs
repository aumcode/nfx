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

using NFX.Environment;
using NFX.RecordModel;

namespace NFX.Inventorization
{
   
    /// <summary>
    /// Performs inventorization of records , fields and their relevant properties 
    /// </summary>
    public class RecordModelInventorization : IInventorization
    {
        public void Inventorize(Type t, Environment.ConfigSectionNode root)
        {
            if (!typeof(Record).IsAssignableFrom(t)) return;
           

            if (t.IsAbstract) return;
            if (t.ContainsGenericParameters) return;

            var rec = Record.Make(t);

            var rnode = root.AddChildNode("record-model");

            rnode.AddAttributeNode("clr-type-ns",  t.Namespace);
            rnode.AddAttributeNode("clr-type",  t.Name);
            rnode.AddAttributeNode("clr-type-full",  t.FullName);
            rnode.AddAttributeNode("table",  rec.TableName);
            rnode.AddAttributeNode("store-flag",  rec.StoreFlag.ToString());
            rnode.AddAttributeNode("case-sensitive",  rec.CaseSensitiveFieldBinding);
            rnode.AddAttributeNode("permission",  rec.PermissionName);
            rnode.AddAttributeNode("permission-namespace",  rec.PermissionNamespace);
            rnode.AddAttributeNode("throw-init-validation",  rec.ThrowOnInitValidation);
            rnode.AddAttributeNode("visible",  rec.Visible);

            foreach(var f in rec.Fields)
            {
              var fnode = rnode.AddChildNode("field");
              
              fnode.AddAttributeNode("name",  f.FieldName);
              fnode.AddAttributeNode("type",  f.GetFieldDataType().FullName);
              fnode.AddAttributeNode("calc-override",  f.AllowCalculationOverride);
              fnode.AddAttributeNode("calculated",  f.Calculated);
              fnode.AddAttributeNode("entry-type",  f.DataEntryType.ToString());
              fnode.AddAttributeNode("description",  f.Description);
              fnode.AddAttributeNode("display-format",  f.DisplayFormat);
              fnode.AddAttributeNode("display-h-align",  f.DisplayTextHAlignment.ToString());
              fnode.AddAttributeNode("display-width",  f.DisplayWidth);
              fnode.AddAttributeNode("has-default-value",  f.HasDefaultValue);
              fnode.AddAttributeNode("hint",  f.Hint);
              fnode.AddAttributeNode("keep-in-errored-field",  f.KeepInErroredField);
              fnode.AddAttributeNode("key",  f.KeyField);
              fnode.AddAttributeNode("order",  f.LogicalOrder);
              fnode.AddAttributeNode("lookup-type",  f.LookupType);

              InventorizationManager.WriteType(f.GetFieldDataType(), fnode.AddChildNode("data-type"));

              if (f.LookupDictionary.Count>0)
              {
                var dnode = fnode.AddChildNode("lookup-dictionary");
                foreach(var key in f.LookupDictionary.Keys)
                {
                  var inode = dnode.AddChildNode("item");
                  inode.AddAttributeNode("key", key);
                  inode.AddAttributeNode("value", f.LookupDictionary[key]);
                }
              }


              fnode.AddAttributeNode("permission",  f.PermissionName);
              fnode.AddAttributeNode("permission-namespace",  f.PermissionNamespace);
              fnode.AddAttributeNode("readonly",  f.Readonly);
              fnode.AddAttributeNode("required",  f.Required);
              fnode.AddAttributeNode("store-flag",  f.StoreFlag.ToString());
              fnode.AddAttributeNode("throw-init-validation",  f.ThrowOnInitValidation);
              fnode.AddAttributeNode("visible",  f.Visible);

              
              if (f is StringField)
              {
                var sf = (StringField)f;
                fnode.AddAttributeNode("size",  sf.Size);
                fnode.AddAttributeNode("char-case",  sf.CharCase.ToString());
                fnode.AddAttributeNode("password",  sf.Password);
                fnode.AddAttributeNode("regexp",  sf.FormatRegExp);
                fnode.AddAttributeNode("regexp-description",  sf.FormatRegExpDescription);
              }
              else
              if (f is IntField)
              {
                var castf = (IntField)f;
                fnode.AddAttributeNode("min-max-check",  castf.MinMaxChecking);
                fnode.AddAttributeNode("min-value",  castf.MinValue);
                fnode.AddAttributeNode("max-value",  castf.MaxValue);
                fnode.AddAttributeNode("default-value",  castf.DefaultValue);
              }else
              if (f is LongField)
              {
                var castf = (LongField)f;
                fnode.AddAttributeNode("min-max-check",  castf.MinMaxChecking);
                fnode.AddAttributeNode("min-value",  castf.MinValue);
                fnode.AddAttributeNode("max-value",  castf.MaxValue);
                fnode.AddAttributeNode("default-value",  castf.DefaultValue);
              }else
              if (f is ShortField)
              {
                var castf = (ShortField)f;
                fnode.AddAttributeNode("min-max-check",  castf.MinMaxChecking);
                fnode.AddAttributeNode("min-value",  castf.MinValue);
                fnode.AddAttributeNode("max-value",  castf.MaxValue);
                fnode.AddAttributeNode("default-value",  castf.DefaultValue);
              }else
              if (f is FloatField)
              {
                var castf = (FloatField)f;
                fnode.AddAttributeNode("min-max-check",  castf.MinMaxChecking);
                fnode.AddAttributeNode("min-value",  castf.MinValue);
                fnode.AddAttributeNode("max-value",  castf.MaxValue);
                fnode.AddAttributeNode("default-value",  castf.DefaultValue);
              } else
              if (f is DoubleField)
              {
                var castf = (DoubleField)f;
                fnode.AddAttributeNode("min-max-check",  castf.MinMaxChecking);
                fnode.AddAttributeNode("min-value",  castf.MinValue);
                fnode.AddAttributeNode("max-value",  castf.MaxValue);
                fnode.AddAttributeNode("default-value",  castf.DefaultValue);
              }else
              if (f is DecimalField)
              {
                var castf = (DecimalField)f;
                fnode.AddAttributeNode("min-max-check",  castf.MinMaxChecking);
                fnode.AddAttributeNode("min-value",  castf.MinValue);
                fnode.AddAttributeNode("max-value",  castf.MaxValue);
                fnode.AddAttributeNode("default-value",  castf.DefaultValue);
              }

            }

        }
    }
}
