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
using NFX.Serialization.JSON;

namespace NFX.DataAccess.CRUD
{
    /// <summary>
    /// Provides a base for attributes which ar targeted for particular techlology (i.e. "ORACLE")
    /// </summary>
    [Serializable]
    public abstract class TargetedAttribute : Attribute
    {
        public const string ANY_TARGET = "*";

        public TargetedAttribute(string targetName, string metadata)
        {
            TargetName = targetName ?? ANY_TARGET;
            m_MetadataContent = metadata;
        }

        /// <summary>
        /// Returns the name of target, i.e. the name of database engine i.e. "ORACLE11g" or "MySQL"
        /// </summary>
        public readonly string TargetName;

        /// <summary>
        /// Returns metadata content string in Laconic format or null. Root not is not specified. I.e.: 'a=1 b=true c{...}'
        /// </summary>
        public string MetadataContent {get{ return m_MetadataContent;}}

        protected string m_MetadataContent;
        
        [NonSerialized]
        private IConfigSectionNode m_Metadata;

        /// <summary>
        /// Returns structured metadata or null if there is no metadata defined
        /// </summary>
        public IConfigSectionNode Metadata
        {
          get 
          {
            if (MetadataContent.IsNullOrWhiteSpace()) return null;
            if (m_Metadata==null)//not thread safe but its ok, in the worst case 2nd copy will be made
             m_Metadata = ParseMetadataContent(m_MetadataContent);

            return m_Metadata;
          }
        }

        /// <summary>
        /// Parses content with or without root node
        /// </summary>
        public static ConfigSectionNode ParseMetadataContent(string content)
        {
          try
          {
              content = content ?? string.Empty;
              var root = ("metadata{"+content+"}").AsLaconicConfig(handling: ConvertErrorHandling.Throw);

              //Unwrap extra "metadata" root node
              if (!root.HasAttributes && root.Children.Count()==1)
              {
                var subroot = root["metadata"];
                if (subroot.Exists) return subroot;
              }

              return root;
          }
          catch(Exception error)
          {
              throw new DataAccessException(StringConsts.CRUD_FIELD_ATTR_METADATA_PARSE_ERROR.Args(error.ToMessageWithType(), content), error); 
          }
        }

    }
    
    
    /// <summary>
    /// Provides information about table schema that this row is a part of
    /// </summary>
    [Serializable]
    [AttributeUsage(AttributeTargets.Class, AllowMultiple=true, Inherited=false)]
    public sealed class TableAttribute : TargetedAttribute
    {
        public TableAttribute(string targetName = null, string name = null, bool immutable = false, string metadata = null) : base(targetName, metadata)
        {
            Name = name ?? string.Empty;
            Immutable = immutable;
        }

        /// <summary>
        /// Returns the name of schema that decorated class represents, i.e. the name of database table i.e. "TBL_PERSON".
        /// This value is set so datastore implementation can use it instead of inferring table name from declaring class name
        /// </summary>
        public readonly string Name;

        /// <summary>
        /// Indicates whether the data represented by the decorated instance can only be created (and possibly deleted) but can not change(no update).
        /// This attribute allows some backends to perform some optimizations (such as better failover data handling and caching) as any version of the data
        /// that could be found is valid and the latest
        /// </summary>
        public readonly bool Immutable;


        public override int GetHashCode()
        {
            return TargetName.GetHashCodeSenseCase() + Name.GetHashCodeSenseCase();
        }

        public override bool Equals(object obj)
        {
            var other = obj as TableAttribute;
            if (other==null) return false;
            return this.TargetName.EqualsSenseCase(other.TargetName) && this.Name.EqualsSenseCase(other.Name) && this.MetadataContent.EqualsSenseCase(other.MetadataContent);
        }
    }


    /// <summary>
    /// Provides hint/classification for field textual data
    /// </summary>
    public enum DataKind
    {
        Text,
        ScreenName,
        Color,
        Date,
        DateTime,
        DateTimeLocal,
        EMail,
        Month,
        Number,
        Range,
        Search,
        Telephone,
        Time,
        Url,
        Week
    }


    /// <summary>
    /// Provides information about table schema that this typed row is a part of
    /// </summary>
    [Serializable]
    [AttributeUsage(AttributeTargets.Property, AllowMultiple=true, Inherited=false)]
    public sealed class FieldAttribute : TargetedAttribute
    {
        public FieldAttribute(
                        string targetName   = ANY_TARGET,
                        StoreFlag storeFlag = StoreFlag.LoadAndStore,
                        bool key            = false,
                        DataKind kind       = DataKind.Text,
                        bool required       = false,
                        bool visible        = true,
                        string valueList    = null,
                        object dflt         = null,
                        object min          = null,
                        object max          = null,
                        int minLength       = 0,
                        int maxLength       = 0,
                        CharCase charCase   = CharCase.AsIs,
                        string backendName  = null,
                        string description  = null,
                        string metadata = null,
                        bool nonUI = false
                    ) : base(targetName, metadata)
        {
              StoreFlag = storeFlag;
              BackendName = backendName;
              Key = key;
              Kind = kind;
              Required = required;
              Visible = visible;
              Min = min;
              Max = max;
              Default = dflt;
              MinLength = minLength;
              MaxLength = maxLength;
              CharCase = charCase;
              ValueList = valueList;
              Description = description;
              NonUI = nonUI;
        }

        public FieldAttribute(
                        Type protoType, 
                        string protoFieldName, //Schema:Field
                        string targetName   = ANY_TARGET,
                        object storeFlag    = null,
                        object key          = null,
                        object kind         = null,
                        object required     = null,
                        object visible      = null,
                        string valueList    = null,
                        object dflt         = null,
                        object min          = null,
                        object max          = null,
                        object minLength    = null,
                        object maxLength    = null,
                        object charCase     = null,
                        string backendName  = null,
                        string description  = null,
                        string metadata     = null,
                        object nonUI        = null
                    ) : base(targetName, null)
        {
            if (protoType==null || protoFieldName.IsNullOrWhiteSpace()) throw new CRUDException(StringConsts.ARGUMENT_ERROR+"FieldAttr.ctor(protoType|protoFieldName=null|empty)");
            try
            {
              var schema = Schema.GetForTypedRow(protoType);
              var protoTargetName = targetName;
              var segs = protoFieldName.Split(':');
              if (segs.Length>1)
              {
                protoTargetName = segs[0].Trim();
                protoFieldName = segs[1].Trim();
              }
              if (protoTargetName.IsNullOrWhiteSpace()) throw new Exception("Wrong target syntax");
              if (protoFieldName.IsNullOrWhiteSpace()) throw new Exception("Wrong field syntax");

              var protoFieldDef = schema[protoFieldName];
              if (protoFieldDef==null) throw new Exception("Prototype '{0}' field '{1}' not found".Args(protoType.FullName, protoFieldName));
              var protoAttr = protoFieldDef[protoTargetName];
            
              try
              {
                StoreFlag        = storeFlag    == null? protoAttr.StoreFlag   : (StoreFlag)storeFlag     ;
                BackendName      = backendName  == null? protoAttr.BackendName : backendName   ;
                Key              = key          == null? protoAttr.Key         : (bool)key           ;
                Kind             = kind         == null? protoAttr.Kind        : (DataKind)kind          ;
                Required         = required     == null? protoAttr.Required    : (bool)required      ;
                Visible          = visible      == null? protoAttr.Visible     : (bool)visible       ;
                Min              = min          == null? protoAttr.Min         : min           ;
                Max              = max          == null? protoAttr.Max         : max           ;
                Default          = dflt         == null? protoAttr.Default     : dflt          ;
                MinLength        = minLength    == null? protoAttr.MinLength   : (int)minLength     ;
                MaxLength        = maxLength    == null? protoAttr.MaxLength   : (int)maxLength     ;
                CharCase         = charCase     == null? protoAttr.CharCase    : (CharCase)charCase      ;
                ValueList        = valueList    == null? protoAttr.ValueList   : valueList     ;
                Description      = description  == null? protoAttr.Description : description   ;
                NonUI            = nonUI        == null? protoAttr.NonUI       : (bool)nonUI         ;
                
                if (metadata.IsNullOrWhiteSpace())
                 m_MetadataContent = protoAttr.m_MetadataContent;
                else
                if (protoAttr.m_MetadataContent.IsNullOrWhiteSpace()) m_MetadataContent = metadata;
                else
                {
                  var conf1 = ParseMetadataContent(protoAttr.m_MetadataContent);
                  var conf2 = ParseMetadataContent(metadata);
                  
                  var merged = new LaconicConfiguration();
                  merged.CreateFromMerge(conf1, conf2);
                  m_MetadataContent = merged.SaveToString();
                }
              }
              catch(Exception err)
              {
                throw new Exception("Invalid assignment of prototype override value: " + err.ToMessageWithType());
              }
            }
            catch(Exception error)
            {
              throw new CRUDException(StringConsts.CRUD_FIELD_ATTR_PROTOTYPE_CTOR_ERROR.Args(error.Message));
            }

        }
           
         /// <summary>
        /// Determines whether field should be loaded/stored from/to storage
        /// </summary>
        public readonly StoreFlag StoreFlag;

        /// <summary>
        /// Provides an overriden name for this field
        /// </summary>
        public readonly string BackendName;

        /// <summary>
        /// Determines whether this field is a part of the primary key
        /// </summary>
        public readonly bool Key; 
        
        /// <summary>
        /// Provides hint/classification for textual field data
        /// </summary>
        public readonly DataKind Kind;   
           
                                
        /// <summary>
        /// Determines whether the field must have data
        /// </summary>
        public readonly bool Required; 


        /// <summary>
        /// Determines whether the field is shown to user (i.e. as a grid column)
        /// </summary>
        public readonly bool Visible; 
        
        /// <summary>
        /// Returns a ";/,/|"-delimited list of permitted field values - used for lookup validation
        /// </summary>
        public readonly string ValueList;  

        /// <summary>
        /// Returns a ValueList parsed into key values as:  val1: descr1,val2: desc2...
        /// </summary>
        public JSONDataMap ParseValueList(bool caseSensitiveKeys = false)
        {
            return ParseValueListString(ValueList, caseSensitiveKeys);
        }

        /// <summary>
        /// Returns a string parsed into key values as:  val1: descr1,val2: desc2...
        /// </summary>
        public static JSONDataMap ParseValueListString(string valueList, bool caseSensitiveKeys = false)
        {
            var result = new JSONDataMap(caseSensitiveKeys);
            if (valueList.IsNullOrWhiteSpace()) return result;

            var segs = valueList.Split(',','|',';');
            foreach(var seg in segs)
            {
              var i = seg.LastIndexOf(':');
              if (i>0&&i<seg.Length-1) result[seg.Substring(0,i).Trim()] = seg.Substring(i+1).Trim();
              else
                result[seg] = seg;
            }

            return result;
        }

        /// <summary>
        /// Provides low-bound validation check
        /// </summary>
        public readonly object Min;

        /// <summary>
        /// Provides high-bound validation check
        /// </summary>
        public readonly object Max;

        /// <summary>
        /// Provides default value
        /// </summary>
        public readonly object Default;


        /// <summary>
        /// Imposes a limit on minimum amount of characters in a textual field
        /// </summary>
        public readonly int MinLength;

        /// <summary>
        /// Imposes a limit on maximum amount of characters in a textual field
        /// </summary>
        public readonly int MaxLength;

        /// <summary>
        /// Controls character casing of textual fields
        /// </summary>
        public readonly CharCase CharCase;

        /// <summary>
        /// Provides description
        /// </summary>
        public readonly string Description;


        /// <summary>
        /// If true indicates that this field is ignored when generating UI and ignored when UI supplies the value to the server.
        /// Pass true to protect server-only structures from being modified by client
        /// </summary>
        public readonly bool NonUI;


        public override int GetHashCode()
        {
            return TargetName.GetHashCodeSenseCase();
        }

        public override bool Equals(object obj)
        {
            var other = obj as FieldAttribute;
            if (other==null) return false;
            var equ = 
                this.TargetName.EqualsSenseCase(other.TargetName) &&
                this.StoreFlag   == other.StoreFlag &&
                this.BackendName.EqualsSenseCase(other.BackendName) &&
                this.Key         == other.Key &&
                this.Kind        == other.Kind &&
                this.Required    == other.Required &&
                this.Visible     == other.Visible &&
                
                (
                  (this.Min==null && other.Min==null) ||
                  (this.Min!=null && other.Min!=null && this.Min.Equals(other.Min))
                ) &&

                (
                  (this.Max==null && other.Max==null) ||
                  (this.Max!=null && other.Max!=null && this.Max.Equals(other.Max))
                ) &&
                
                (
                  (this.Default==null && other.Default==null) ||
                  (this.Default!=null && other.Default!=null && this.Default.Equals(other.Default))
                ) &&

                this.MinLength   == other.MinLength &&
                this.MaxLength   == other.MaxLength &&
                this.CharCase    == other.CharCase &&
                this.ValueList   == other.ValueList &&
                this.Description == other.Description &&
                this.MetadataContent==other.MetadataContent &&
                this.NonUI == other.NonUI;

            return equ;
        }
    }
}
