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
using System.Reflection;
using System.Runtime.Serialization;

using NFX.DataAccess;

namespace NFX.RecordModel
{
  /// <summary>
  /// Decorates model methods that can be called by external code, such as NFX.WebForms
  /// </summary>
  [AttributeUsage(AttributeTargets.Method, Inherited = false, AllowMultiple = false)]
  public class CallableMethodAttribute : Attribute
  {
     public CallableMethodAttribute()
     {
     
     }
  }

  

  /// <summary>
  /// A dictionary of [script-type, text] pairs which is returned to client.
  /// Use ClientScripts.*_TYPE family of constants to pass standard values for script-type keys
  /// </summary>
  [Serializable]
  public sealed class ClientScripts : Dictionary<string, string> 
  {
    /// <summary>
    /// Constant for client-side validation scripts
    /// </summary>
    public const string VALIDATION_TYPE = "validation";

    /// <summary>
    /// Constant for client-side presentation scripts
    /// </summary>
    public const string PRESENTATION_TYPE = "presentation";
  
  }


  /// <summary>
  /// Declaratively specifies what resource/text should be used to serve client script for requested technology. Decorates record models and record fields.
  /// Script text is taken from ClientScript.Text field, or from resource pointed-to by ClientScript.ResourceName.
  /// ResourceName may include @tech@ tag that is replaced with technology name.
  /// If neither Text nor ResourceName attribute fields specified, then script resource name is constructed according to this pattern:
  ///  record-type.@tech@.js for records or record-type.codefieldname.@tech@.js for fields.
  /// If Type is not set than it defaults to ClientScripts.VALIDATION_TYPE value 
  /// </summary>
  [AttributeUsage(AttributeTargets.Class | AttributeTargets.Field, Inherited = false, AllowMultiple = true)]
  public class ClientScriptAttribute : Attribute
  {
     public const string TECHNOLOGY_TAG = "@tech@";
     public const string DEFAULT_SCRIPT_EXT = ".js";
     
     
     /// <summary>
     /// Creates an attribute
     /// </summary>
     public ClientScriptAttribute()
     {
       this.Type = ClientScripts.VALIDATION_TYPE;
     }

     /// <summary>
     /// Creates an attribute of particular type and text
     /// </summary>
     public ClientScriptAttribute(string type, string text)
     {
       this.Type = type; 
       this.Text = text;     
     }

     /// <summary>
     /// Gets/sets script text. This property takes precedence over ResourceName when specified
     /// </summary>
     public string Text { get; set;}

     /// <summary>
     /// Name of script resource which must be co-located with the record class that declares an entity decorated with this attribute.
     /// The property is used if Text is null or empty.
     /// The @tech@ tag may be placed within this string - it will be replaced with requested lower-case technology name, i.e. 
     ///   "A.@tech@.B"  will be evaluate as "A.nfx.B" if "nfx" technology was requested
     /// </summary>
     public string ResourceName { get; set;}

     /// <summary>
     /// A name of technology that this script supports
     /// </summary>
     public string Technology { get; set;}

     /// <summary>
     /// Script type - such as validation, presentation etc. which is served to client. Use ClientScripts.*_TYPE family of constants to pass standard values
     /// </summary>
     public string Type { get; set;}



     /// <summary>
     /// Harvests client scripts from ClientScript attributes per specified technology.
     /// Script text is taken from ClientScript.Text field, or from resource pointed-to by ClientScript.ResourceName.
     /// ResourceName may include @tech@ tag that is replaced with technology name.
     /// If neither Text nor ResourceName attribute fields specified, then script resource name is constructed according to this pattern:
     ///  record-type.@tech@.js for records or record-type.codefieldname.@tech@.js for fields
     /// </summary>
     /// <param name="scopeType">Scoping type relative to which resource are located</param>
     /// <param name="extraResourceScope">When not null, added to record type name if attribute does not specify either Text or ResourceName</param>
     /// <param name="attributes">Attributes decorating the declaration of record or field</param>
     /// <param name="technology">Technology of interest or null</param>
     public static ClientScripts GetScripts(Type scopeType, string extraResourceScope, IEnumerable<ClientScriptAttribute> attributes, string technology)
     {
       var result = new ClientScripts();

       var attrs = attributes.Where(a=> (
                                          (string.IsNullOrWhiteSpace(a.Technology) || string.IsNullOrWhiteSpace(technology)) && 
                                           !attributes.Any( at=>  //named technology takes precedence, need to make sure there are no attributes that match
                                                            string.Equals(at.Technology, technology, StringComparison.InvariantCultureIgnoreCase)
                                                          )
                                         )|| 
                                        string.Equals(a.Technology, technology, StringComparison.InvariantCultureIgnoreCase));
       foreach(var atr in attrs)
       {
         var text = atr.Text;

         if (string.IsNullOrWhiteSpace(text))
         {
            var rname = atr.ResourceName;
            if (string.IsNullOrWhiteSpace(rname))
            {
              if (string.IsNullOrWhiteSpace(extraResourceScope))
               rname = scopeType.Name + "." +TECHNOLOGY_TAG + DEFAULT_SCRIPT_EXT;
              else
               rname = scopeType.Name + "." +extraResourceScope + "." + TECHNOLOGY_TAG + DEFAULT_SCRIPT_EXT;
            }
            text = scopeType.GetText(rname.Replace(TECHNOLOGY_TAG, (technology ?? string.Empty).ToLowerInvariant()));
         }

         result[atr.Type ?? string.Empty] = text;
       }

       return result;
     }
  }

     


  /// <summary>
  /// A convenience for defining record properties declaratively
  /// </summary>
  [AttributeUsage(AttributeTargets.Class, Inherited = true, AllowMultiple = false)]
  public class RecordDefAttribute : Attribute
  {
      public RecordDefAttribute()  { SupportsNotificationBinding = true;}

      public string Name { get; set; }
      public string TableName{ get; set;}
      public StoreFlag StoreFlag { get; set; }

      /// <summary>
      /// Determines whether field validation should be deferred until Record.Post() 
      /// </summary>
      public bool FieldValidationSuspended { get; set; }
      
      /// <summary>
      /// Determines whether record instance building should bypass duplicate names checks. Setting this flag to true significantly improves performance
      /// when many record instances need to be allocated
      /// </summary>
      public bool NoBuildCrosscheks { get; set;}


      /// <summary>
      /// Determines whether record can notify bindings. Disabling improves speed
      /// </summary>
      public bool SupportsNotificationBinding { get; set;}
  }

  /// <summary>
  /// A convenience for defining field properties declaratively
  /// </summary>
  [AttributeUsage(AttributeTargets.Field, Inherited = false, AllowMultiple = false)]
  public class FieldDefAttribute : Attribute
  {
      public FieldDefAttribute() { }

      public string Name { get; set; }
      public string Description { get; set; }
      public StoreFlag StoreFlag { get; set; }
      public bool Required { get; set; }
      public int Size { get; set; }
      public DataEntryType DataEntryType { get; set; }
      public LookupType LookupType { get; set; }
      public string[] LookupDictionary { get; set; }
      public bool Marked { get; set; }

      public bool MinMaxChecking { get; set; }
      public object MinValue { get; set; }
      public object MaxValue { get; set; }

      public object DefaultValue { get; set;}

      public bool Calculated { get; set; }
      public string Formula { get; set; }


      /// <summary>
      /// Constructs fields for particular record instance using attributes
      /// </summary>
      public static void BuildAndDefineFields(Record rec)
      {
         try
         {
         
          var nfxField = typeof(Field);
          var clrFields = rec.GetType().GetFields(BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance);
          
          var fields = clrFields.Where(fi => nfxField.IsAssignableFrom(fi.FieldType));

          foreach (var fi in fields)
          {
              //Create an instance of the particular field
              var fld = (Field)Activator.CreateInstance(fi.FieldType);
              fi.SetValue(rec, fld);

              fld.Owner = rec;

              fld.ApplyDefinitionAttributes(fi.GetCustomAttributes(false));
          }
        }
        catch(Exception error)
        {
            throw new RecordModelException(string.Format(StringConsts.FIELD_ATTRIBUTES_DEFS_ERROR, 
                                                         rec!=null ? rec.GetType().FullName : CoreConsts.UNKNOWN),
                                           error);
        }
      }


  }


  
}
