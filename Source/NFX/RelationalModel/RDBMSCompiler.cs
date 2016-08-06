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
using System.IO;
using System.Linq;
using System.Text;
using NFX.Environment;

using NFX.RelationalModel.DataTypes;

namespace NFX.RelationalModel
{

    /// <summary>
    /// Denotes a type of RDBMS entity
    /// </summary>
    public enum RDBMSEntityType
    {
        Unknown = 0,
        Schema,
        Table,
        Column,
        Domain,
        PrimaryKey,
        Index,
        Trigger,
        View,
        StoreProc,
        CheckConstraint,
        Reference
    }

    public enum RDBMSSortOrder
    {
        Asc=0,
        Ascending=Asc,
        Desc,
        Descending=Desc
    }

    public class RDBMSEntity
    {
        public readonly RDBMSEntity ParentEntity;
        public readonly IConfigNode SourceNode;
        public readonly RDBMSEntityType EntityType;
        public readonly string OriginalName;
        public readonly string OriginalShortName;
        public string TransformedName;
        public string TransformedShortName;
        public RDBMSDomain Domain;//used for columns
        public readonly List<RDBMSEntity> Children = new List<RDBMSEntity>();

        public RDBMSEntity(RDBMSEntity parentEntity, IConfigNode sourceNode, RDBMSEntityType entityType, string originalName, string originalShortName = null, string transformedName = null, string transformedShortName = null)
        {
            if (parentEntity!=null)
                parentEntity.Children.Add(this);

            ParentEntity = parentEntity;
            SourceNode = sourceNode;
            EntityType = entityType;
            OriginalName = originalName;
            OriginalShortName = originalShortName;
            TransformedName = transformedName;
            TransformedShortName = transformedShortName;
        }
    }


    /// <summary>
    /// Compiles schema scripts into RDBMS-family of outputs - the ones that have tables, keys, indexes, constraints etc...
    /// </summary>
    public abstract class RDBMSCompiler : Compiler
    {
        #region CONSTS
            public const string TABLE_SECTION = "table";
            public const string COLUMN_SECTION = "column";
            public const string REFERENCE_SECTION = "reference";

            public const string PRIMARY_KEY_SECTION = "primary-key";
            public const string INDEX_SECTION = "index";

            public const string NAME_ATTR = "name";
            public const string TYPE_ATTR = "type";
            public const string REQUIRED_ATTR = "required";
            public const string DEFAULT_ATTR = "default";
            public const string COMMENT_ATTR = "comment";
            public const string UNIQUE_ATTR = "unique";
            public const string ORDER_ATTR = "order";
            public const string CLUSTERED_ATTR = "clustered";
            public const string SHORT_NAME_ATTR = "short-name";
            public const string LENGTH_ATTR = "length";

            public const string NOW_FUNC = "now()";

            public const string DEFAULT_DOMAIN_SEARCH_PATHS = "NFX.RelationalModel.DataTypes";


            public const string TABLES_OUTPUT = "tables";
            public const string INDEXES_OUTPUT = "indexes";
            public const string SEQUENCES_OUTPUT = "sequences";
            public const string FOREIGN_KEYS_OUTPUT = "foreignkeys";

        #endregion

        #region .ctor

            public RDBMSCompiler(Schema schema) : base(schema)
            {

            }


        #endregion

        #region Fields

            private string m_DomainSearchPaths = DEFAULT_DOMAIN_SEARCH_PATHS;

            private bool m_SeparateIndexes;
            private bool m_SeparateForeignKeys;

            private RDBMSEntity m_All;

        #endregion

        #region Properties

            public override TargetType Target
            {
                get { return TargetType.GenericSQL; }
            }

            /// <summary>
            /// Gets/sets ';' separated list of domain search namespaces paths
            /// </summary>
            [Config("$domain-search-paths", DEFAULT_DOMAIN_SEARCH_PATHS)]
            public virtual string DomainSearchPaths
            {
                get { return m_DomainSearchPaths ?? string.Empty;}
                set
                {
                    EnsureNotCompiled();
                    m_DomainSearchPaths = value;
                }
            }

            /// <summary>
            /// Gets/sets the flag that indicates whether indexes should be written in the separate output from tables
            /// </summary>
            [Config("$separate-indexes")]
            public virtual bool SeparateIndexes
            {
                get { return m_SeparateIndexes;}
                set
                {
                    EnsureNotCompiled();
                    m_SeparateIndexes = value;
                }
            }

            /// <summary>
            /// Gets/sets the flag that indicates whether foreign keys should be written in the separate output from tables.
            /// Foreign keys get added as constraints under the table when referenced table is already present and this flag is false
            /// </summary>
            [Config("$separate-fkeys")]
            public virtual bool SeparateForeignKeys
            {
                get { return m_SeparateForeignKeys;}
                set
                {
                    EnsureNotCompiled();
                    m_SeparateForeignKeys = value;
                }
            }



        #endregion


        #region Public/Protected


            protected override void DoCompile()
            {
                var node = Schema.Source;
                m_All = new RDBMSEntity(null, node, RDBMSEntityType.Schema, node.AttrByName(NAME_ATTR).ValueAsString("schema"));
                TransformEntityName(m_All);
                base.DoCompile();
            }

            public override string GetOutputFileSuffix(string outputName)
            {
                return ".{0}.sql".Args(Name);
            }

            protected override void BuildNodeOutput(IConfigSectionNode node, Outputs outputs)
            {
                if (node.IsSameName(TABLE_SECTION)) DoTable(node, outputs);
                else
                     m_CompileErrors.Add(new SchemaCompilationException(node.RootPath, "Unrecognized item: " + node.Name));

            }

            /// <summary>
            /// Turns domain name into domain instance
            /// </summary>
            protected virtual RDBMSDomain CreateDomain(string sourcePath, string name, IConfigNode node)
            {
               try
               {
                string argsLine = null;
                var iop = name.LastIndexOf('(');
                var icp = name.LastIndexOf(')');
                if (iop>0 && icp>iop)
                {
                  argsLine = name.Substring(iop+1, icp-iop-1);
                  name = name.Substring(0, iop);
                }

                Type dtype = Type.GetType(name, false, true);
                if (dtype==null)
                {
                    var paths = DomainSearchPaths.Split('|', ';');
                    foreach(var path in paths)
                    {
                        var fullName = path.Replace(".*", "." + name);
                        dtype = Type.GetType(fullName, false, true);
                        if (dtype!=null) break;
                    }
                }

                if (dtype==null)
                {
                      m_CompileErrors.Add(new SchemaCompilationException(sourcePath, "Domain type not found in any paths: " + name));
                      return null;
                }

                object[] args = null;
                var ctor = dtype.GetConstructor(Type.EmptyTypes);

                if (argsLine!=null)
                {
                    var argsStrings = argsLine.Split(',');
                    ctor = dtype.GetConstructors().Where(ci => ci.GetParameters().Length==argsStrings.Length).FirstOrDefault();
                    if (ctor==null)
                    {
                      m_CompileErrors.Add(new SchemaCompilationException(sourcePath, "Domain .ctor '{0}' argument mismatch '{1}'".Args(dtype.FullName, argsLine)));
                      return null;
                    }

                    args = new object[ctor.GetParameters().Length];
                    var i = 0;
                    foreach(var pi in ctor.GetParameters())
                    {
                        args[i] = argsStrings[i].Trim().AsType(pi.ParameterType);
                        i++;
                    }
                }

                var result = Activator.CreateInstance(dtype, args) as RDBMSDomain;
                if (node is IConfigSectionNode)
                    result.Configure((IConfigSectionNode)node);
                return result;
               }
               catch(Exception error)
               {
                 m_CompileErrors.Add(new SchemaCompilationException(sourcePath, "Domain '{0}' creation error: {1} ".Args(name, error.ToMessageWithType())));
                 return null;
               }
            }


            /// <summary>
            /// Override to map a name from schema into the name that should be used in the output (i.e. real table name)
            /// </summary>
            public virtual void TransformEntityName(RDBMSEntity entity)
            {
                var name = entity.OriginalName;
                var parentName = entity.ParentEntity!=null ? "{0}_".Args(entity.ParentEntity.TransformedShortName) : string.Empty;

                if (entity.EntityType==RDBMSEntityType.PrimaryKey || entity.EntityType==RDBMSEntityType.Reference)
                {
                    var parentParentName = entity.ParentEntity!=null && entity.ParentEntity.ParentEntity!=null ? "{0}_".Args(entity.ParentEntity.ParentEntity.TransformedShortName) : string.Empty;
                    parentName = entity.ParentEntity.EntityType==RDBMSEntityType.Column ? parentParentName : parentName;
                }


                string result = entity.OriginalName;
                switch(entity.EntityType)
                {
                    case RDBMSEntityType.Table:       { result =  "tbl_" + name;    break; }
                    case RDBMSEntityType.Column:      { result =  name;             break; }
                    case RDBMSEntityType.Domain:      { result =  name;             break; }
                    case RDBMSEntityType.PrimaryKey:  { result =  "pk_"+parentName+name;  break; }
                    case RDBMSEntityType.Index:       { result =  "idx_"+parentName+name; break; }
                    case RDBMSEntityType.Trigger:     { result =  "trg_"+parentName+name; break; }
                    case RDBMSEntityType.View:        { result =  "viw_"+name;            break; }
                    case RDBMSEntityType.StoreProc:   { result =  "sp_"+name;             break; }
                    case RDBMSEntityType.CheckConstraint:  { result =  "cc_"+name;             break; }
                    case RDBMSEntityType.Reference:   { result =  "fk_"+parentName+name;  break; }
                }

                entity.TransformedName = result;
                entity.TransformedShortName = entity.OriginalShortName;

                if (entity.TransformedShortName.IsNullOrWhiteSpace())
                {
                    entity.TransformedShortName = entity.TransformedName;
                }

                if (!CaseSensitiveNames)
                {
                    entity.TransformedName = entity.TransformedName.ToUpperInvariant();
                    entity.TransformedShortName = entity.TransformedShortName.ToUpperInvariant();
                }

            }


            /// <summary>
            /// Gets quoted name per particular technology
            /// </summary>
            public virtual string GetQuotedIdentifierName(RDBMSEntityType type, string name)
            {
                return "\"{0}\"".Args(name);
            }

            /// <summary>
            /// Override to return statement delimiter script for particular target , i.e. "Go" at the statementend for MsSQL Server T-SQL
            /// </summary>
            public virtual string GetStatementDelimiterScript(RDBMSEntityType type, bool start)
            {
                return start ? string.Empty : ";";
            }


            protected virtual string FormatColumnStatement(string column, string type, string nnull, string auto, string dflt, string chk, string comment)
            {
                const int TAB = 4;
                var cw = 16;
                while(column.Length>cw) cw+=TAB;

                var result = " {{0,-{0}}} {{1}}".Args(cw).Args(column, type).TrimEnd();

                var rw = 32;
                while(result.Length>rw) rw+=TAB;
                result = "{{0,-{0}}} {{1}}".Args(rw).Args(result, nnull);

                if (auto.IsNotNullOrWhiteSpace())
                    result = "{0}{1}{2}".Args(result, " ".PadLeft(result.Length%TAB), auto);

                if (dflt.IsNotNullOrWhiteSpace())
                    result = "{0}{1}{2}".Args(result, " ".PadLeft(result.Length%TAB), dflt);

                if (chk.IsNotNullOrWhiteSpace())
                    result = "{0}{1}{2}".Args(result, " ".PadLeft(result.Length%TAB), chk);

                if (comment.IsNotNullOrWhiteSpace())
                    result = "{0}{1}{2}".Args(result, " ".PadLeft(result.Length%TAB), comment);

                return result.TrimEnd();
            }


            public virtual string TransformKeywordCase(string keywords)
            {
                return keywords.ToLowerInvariant();
            }

            public virtual string GetColumnNullNotNullClause(RDBMSEntity column, bool required)
            {
                return required ? "not null" : string.Empty;
            }


            public virtual string TransformSortOrder(RDBMSSortOrder order)
            {
                return order==RDBMSSortOrder.Asc ? string.Empty : "DESC";
            }


            /// <summary>
            /// Override to compile a RDBMS Table
            /// </summary>
            protected virtual void DoTable(IConfigSectionNode tableNode, Outputs outputs)
            {
                var tname = tableNode.Value;
                if (tname.IsNullOrWhiteSpace())
                {
                     m_CompileErrors.Add(new SchemaCompilationException(tableNode.RootPath, "Table name missing"));
                     return;
                }

                var table = new RDBMSEntity(m_All, tableNode, RDBMSEntityType.Table, tname, tableNode.AttrByName(SHORT_NAME_ATTR).Value);

                TransformEntityName(table);


                var sb = outputs[RDBMSCompiler.TABLES_OUTPUT];
                sb.AppendLine(GetStatementDelimiterScript(RDBMSEntityType.Table, true));


                sb.AppendLine("-- {0}".Args( tableNode.AttrByName(SCRIPT_COMMENT_ATTR).ValueAsString("Table " + table.TransformedName)));

                sb.AppendLine("{0} {1}".Args( TransformKeywordCase("create table"),
                                              GetQuotedIdentifierName(RDBMSEntityType.Table, table.TransformedName) ));
                sb.AppendLine("(");

                var firstItem = true;
                foreach(var node in tableNode.Children)
                {
                    if      (node.IsSameName(COLUMN_SECTION))         { DoColumn(node, table, sb, ref firstItem, outputs); }
                    else if (node.IsSameName(PRIMARY_KEY_SECTION))    { DoReadPrimaryKeySection(node, table); }
                    else if (node.IsSameName(INDEX_SECTION))          { DoReadIndexSection(node, table); }
                    else if (node.IsSameName(SCRIPT_INCLUDE_SECTION)) { IncludeScriptFile(node, outputs); sb.AppendLine(); }
                    else if (node.IsSameName(SCRIPT_TEXT_SECTION))    { IncludeScriptText(node, outputs); sb.AppendLine(); }
                    else
                       m_CompileErrors.Add(new SchemaCompilationException(node.RootPath, "Unrecognized item inside '{0}' table section '{1}'".Args(tname, node.Name)));

                }

                DoPrimaryKeys(table, sb, ref firstItem);
                DoForeignKeys(table, sb, ref firstItem, outputs);

                sb.AppendLine();
                sb.AppendLine(")");

                var comment = tableNode.AttrByName(COMMENT_ATTR).Value;
                if (comment.IsNotNullOrWhiteSpace())
                {
                    sb.AppendLine("    {0} = {1}".Args(TransformKeywordCase("comment"),
                                                       EscapeString(comment) ));
                }


                sb.AppendLine(GetStatementDelimiterScript(RDBMSEntityType.Table, false));
                sb.AppendLine();

                DoTableIndexes(table, outputs);
            }

            /// <summary>
            /// Override to compile a indexes per table
            /// </summary>
            protected virtual void DoTableIndexes(RDBMSEntity table, Outputs outputs)
            {
                var sb = m_SeparateIndexes ? outputs[RDBMSCompiler.INDEXES_OUTPUT] : outputs[RDBMSCompiler.TABLES_OUTPUT];

                var indexes = table.Children.Where(c=>c.EntityType==RDBMSEntityType.Index);
                foreach(var idx in indexes)
                {
                   var node = (IConfigSectionNode)idx.SourceNode;
                   var unique = node.AttrByName(UNIQUE_ATTR).ValueAsBool();

                   var colNames = new List<string>();
                   foreach(var col in node.Children.Where(c=>c.IsSameName(COLUMN_SECTION)))
                   {
                       var cn = col.Value;
                       var column = table.Children.FirstOrDefault(c=>c.EntityType==RDBMSEntityType.Column && c.OriginalName.Equals(cn, NameComparison));
                       if (column==null)
                       {
                            m_CompileErrors.Add(new SchemaCompilationException(table.SourceNode.RootPath,
                                                                     "Table '{0}' defines index '{1}' which references column by name '{2}' which does not exist"
                                                                     .Args(table.OriginalName, idx.OriginalName, cn)));
                            return;
                       }

                       var colName =  GetQuotedIdentifierName(RDBMSEntityType.Column, column.TransformedName);

                       var colLen = col.AttrByName(LENGTH_ATTR).ValueAsInt();
                       if (colLen>0) colName += "({0})".Args(colLen);

                       var colOrder = col.AttrByName(ORDER_ATTR).ValueAsEnum<RDBMSSortOrder>();
                       var ord = TransformSortOrder( colOrder );
                       if (ord.IsNotNullOrWhiteSpace())
                         colName = "{0} {1}".Args(colName, ord);

                       colNames.Add( colName );
                   }

                   var colNamesLine = string.Join(", ",colNames);

                   var comment = node.AttrByName(COMMENT_ATTR).Value;
                   var commentClause = comment.IsNullOrWhiteSpace()? string.Empty :
                                       TransformKeywordCase(" comment {0}").Args( EscapeString(comment) );

                   sb.AppendLine(GetStatementDelimiterScript(RDBMSEntityType.Index, true));

                   sb.Append(TransformKeywordCase("  create {0} index {1} on {2}({3}){4}")
                                                        .Args(
                                                          TransformKeywordCase( unique?"unique":string.Empty ),
                                                          GetQuotedIdentifierName( RDBMSEntityType.Index, idx.TransformedName ),
                                                          GetQuotedIdentifierName( RDBMSEntityType.Table, table.TransformedName ),
                                                          colNamesLine,
                                                          commentClause
                                                        )
                               );

                   sb.AppendLine(GetStatementDelimiterScript(RDBMSEntityType.Index, false));
                }
            }

            /// <summary>
            /// Override to compile a RDBMS Table
            /// </summary>
            protected virtual void DoColumn(IConfigSectionNode columnNode, RDBMSEntity table, StringBuilder sb, ref bool firstColumn, Outputs outputs)
            {
                var colComment = columnNode.AttrByName(SCRIPT_COMMENT_ATTR).Value;
                if (colComment.IsNotNullOrWhiteSpace())
                    sb.AppendLine("  -- {0}".Args( colComment ) );

                var columnName = columnNode.Value;
                if (columnName.IsNullOrWhiteSpace())
                {
                  m_CompileErrors.Add(new SchemaCompilationException(columnNode.RootPath, "Table '{0}' missing column name.".Args(table.OriginalName)));
                  return;
                }

                var column = new RDBMSEntity(table, columnNode, RDBMSEntityType.Column, columnName, columnNode.AttrByName(SHORT_NAME_ATTR).Value ?? columnName);
                TransformEntityName(column);

                var typeNode = columnNode.Navigate(TYPE_ATTR + "|$"+TYPE_ATTR);
                if (typeNode==null || typeNode.VerbatimValue.IsNullOrWhiteSpace())
                {
                  m_CompileErrors.Add(new SchemaCompilationException(columnNode.RootPath, "Column '{0}' missing {1} attribute.".Args(columnName, TYPE_ATTR)));
                  return;
                }

                var columnType = typeNode.Value;
                var type = new RDBMSEntity(column, typeNode, RDBMSEntityType.Domain, columnType);
                TransformEntityName(type);

                var domain = CreateDomain("{0}.{1}::{2}".Args(table.OriginalName, column.OriginalName, type.OriginalName), type.OriginalName, typeNode);
                if (domain==null)
                {
                    m_CompileErrors.Add(new SchemaCompilationException(columnNode.RootPath, "Domain could not be created: " +type.TransformedName ));
                    return;
                }

                domain.TransformColumnName(this, column);

                if (!firstColumn) sb.AppendLine(",");

                #region Column Line
                {
                    var cn = GetQuotedIdentifierName(RDBMSEntityType.Column, column.TransformedName);
                    var tn = GetQuotedIdentifierName(RDBMSEntityType.Domain, domain.GetTypeName(this));
                    var required = (domain.GetColumnRequirement(this) ?? false) || columnNode.AttrByName(REQUIRED_ATTR).ValueAsBool();
                    var nn = TransformKeywordCase( GetColumnNullNotNullClause(column, required) );
                    var auto = domain.GetColumnAutoGeneratedScript(this, column, outputs);
                    var dfltValue = columnNode.AttrByName(DEFAULT_ATTR).Value ?? domain.GetColumnDefaultScript(this, column, outputs);
                    var dflt = dfltValue.IsNotNullOrWhiteSpace()? "{0} {1}".Args(TransformKeywordCase("default"), EscapeString(dfltValue)) : string.Empty;
                    var chk = domain.GetColumnCheckScript(this, column, outputs);
                    var cmntValue = columnNode.AttrByName(COMMENT_ATTR).Value;
                    var cmnt = cmntValue.IsNotNullOrWhiteSpace()? "{0} {1}".Args(TransformKeywordCase("comment"), EscapeString(cmntValue)) : string.Empty;
                    sb.Append( FormatColumnStatement(cn, tn, nn, auto, dflt, chk, cmnt) );
                }
                #endregion

                firstColumn = false;

                foreach(var colSubNode in columnNode.Children)
                {
                    if (colSubNode.IsSameName(PRIMARY_KEY_SECTION) || colSubNode.IsSameName(REFERENCE_SECTION))
                    {
                        var keyType = colSubNode.IsSameName(PRIMARY_KEY_SECTION) ? RDBMSEntityType.PrimaryKey : RDBMSEntityType.Reference;
                        var keyName = colSubNode.Value;
                        if (keyName.IsNullOrWhiteSpace())
                            keyName = column.OriginalShortName;
                        var key = new RDBMSEntity(column, colSubNode, keyType, keyName, colSubNode.AttrByName(SHORT_NAME_ATTR).Value);
                        TransformEntityName(key);
                    }
                    else  if (colSubNode.IsSameName(TYPE_ATTR)) { } //type may be used as section as well
                    else
                        m_CompileErrors.Add(new SchemaCompilationException(colSubNode.RootPath,
                                                                           "Unrecognized item inside '{0}.{1}' column, section '{2}'"
                                                                           .Args(table.OriginalName, columnName, colSubNode.Name)));
                }

            }


            /// <summary>
            /// Override to read primary key definition form sub-section of table (not column level)
            /// </summary>
            protected virtual void DoReadPrimaryKeySection(IConfigSectionNode pkNode, RDBMSEntity table)
            {
                var keyName = pkNode.Value;
                if (keyName.IsNullOrWhiteSpace())
                    keyName = "PRIMARY";
                var pKey = new RDBMSEntity(table, pkNode, RDBMSEntityType.PrimaryKey, keyName);
                TransformEntityName(pKey);
            }


            /// <summary>
            /// Override to output primary keys
            /// </summary>
            protected virtual void DoPrimaryKeys(RDBMSEntity table, StringBuilder sb, ref bool firstItem)
            {
                var tableLevelKeys =  table.Children.Where(c=>c.EntityType==RDBMSEntityType.PrimaryKey);


                var columnLevelKeys = table.Children.Where(c=>c.EntityType==RDBMSEntityType.Column)
                                                .SelectMany(col=>col.Children)
                                                .Where(c=>c.EntityType==RDBMSEntityType.PrimaryKey);


                var allKeys = tableLevelKeys.Concat(columnLevelKeys);

                var pk = allKeys.FirstOrDefault();
                if (pk==null) return;
                if (allKeys.Count()>1)
                  m_CompileErrors.Add(new SchemaCompilationException(table.SourceNode.RootPath,
                                                                     "Table '{0}' defines more than one primary key. Only first on '{1}' is used"
                                                                     .Args(table.OriginalName, pk.ParentEntity.OriginalName)));



                if (pk.ParentEntity.EntityType==RDBMSEntityType.Column)
                {
                    if (!firstItem) sb.AppendLine(",");
                    sb.Append(TransformKeywordCase("  constraint {0} primary key ({1})")
                                                        .Args(
                                                          GetQuotedIdentifierName( RDBMSEntityType.PrimaryKey, pk.TransformedName ),
                                                          GetQuotedIdentifierName( RDBMSEntityType.Column, pk.ParentEntity.TransformedName )
                                                        )
                               );
                }
                else//table
                {
                   var colNames = new List<string>();
                   foreach(var col in ((ConfigSectionNode)pk.SourceNode).Children.Where(c=>c.IsSameName(COLUMN_SECTION)))
                   {
                       var cn = col.Value;
                       var column = table.Children.FirstOrDefault(c=>c.EntityType==RDBMSEntityType.Column && c.OriginalName.Equals(cn, NameComparison));
                       if (column==null)
                       {
                            m_CompileErrors.Add(new SchemaCompilationException(table.SourceNode.RootPath,
                                                                     "Table '{0}' defines primary key which references column by name '{1}' which does not exist"
                                                                     .Args(table.OriginalName, cn)));
                            return;
                       }

                       colNames.Add(GetQuotedIdentifierName(RDBMSEntityType.Column, column.TransformedName));
                   }

                   var colNamesLine = string.Join(", ",colNames);

                   if (!firstItem) sb.AppendLine(",");
                   sb.Append(TransformKeywordCase("  constraint {0} primary key ({1})")
                                                        .Args(
                                                          GetQuotedIdentifierName( RDBMSEntityType.PrimaryKey, pk.TransformedName ),
                                                          colNamesLine
                                                        )
                               );

                }

                firstItem = false;
            }


            /// <summary>
            /// Override to outpur foreign keys
            /// </summary>
            protected virtual void DoForeignKeys(RDBMSEntity table, StringBuilder sb, ref bool firstItem, Outputs outputs)
            {
                var tableLevelKeys =  table.Children.Where(c=>c.EntityType==RDBMSEntityType.Reference);


                var columnLevelKeys = table.Children.Where(c=>c.EntityType==RDBMSEntityType.Column)
                                                .SelectMany(col=>col.Children)
                                                .Where(c=>c.EntityType==RDBMSEntityType.Reference);


                var allKeys = tableLevelKeys.Concat(columnLevelKeys);


                foreach(var fke in allKeys)
                {
                    var rt = ((IConfigSectionNode)fke.SourceNode).AttrByName(TABLE_SECTION).Value;
                    var rc = ((IConfigSectionNode)fke.SourceNode).AttrByName(COLUMN_SECTION).Value;

                    if (rt.IsNullOrWhiteSpace() || rc.IsNullOrWhiteSpace())
                    {
                      m_CompileErrors.Add(new SchemaCompilationException(fke.SourceNode.RootPath,
                                               "Both table and column names are required for reference '{0}' table section '{1}'".Args(table.OriginalName, fke.SourceNode.Name)));
                      continue;
                    }

                    var refTable = new RDBMSEntity(null, null, RDBMSEntityType.Table, rt);
                    TransformEntityName(refTable);

                    var refColumn = new RDBMSEntity(refTable, null, RDBMSEntityType.Column, rc);
                    TransformEntityName(refColumn);


                    var refTableWasAltreadyDeclared = m_All.Children.Any(t=>t.EntityType==RDBMSEntityType.Table && refTable.OriginalName.Equals(t.OriginalName, NameComparison));

                    var useAlterStatement = !refTableWasAltreadyDeclared || m_SeparateForeignKeys;

                    var constraint =  TransformKeywordCase("  constraint {0} foreign key ({1}) references {2}({3})")
                                                        .Args(
                                                          GetQuotedIdentifierName( RDBMSEntityType.Reference, fke.TransformedName ),
                                                          GetQuotedIdentifierName( RDBMSEntityType.Column, fke.ParentEntity.TransformedName ),
                                                          GetQuotedIdentifierName( RDBMSEntityType.Table, refTable.TransformedName ),
                                                          GetQuotedIdentifierName( RDBMSEntityType.Column, refColumn.TransformedName )
                                                        );


                    if (useAlterStatement)
                    {
                        var ksb = outputs[RDBMSCompiler.FOREIGN_KEYS_OUTPUT];
                        ksb.AppendLine(GetStatementDelimiterScript(RDBMSEntityType.Reference, true));

                        ksb.AppendLine(TransformKeywordCase("  alter table {0} add {1}")
                                                        .Args(
                                                          GetQuotedIdentifierName( RDBMSEntityType.Table, table.TransformedName ),
                                                          constraint
                                                        )
                               );

                        ksb.AppendLine(GetStatementDelimiterScript(RDBMSEntityType.Reference, false));
                    }
                    else
                    {
                        if (!firstItem) sb.AppendLine(",");
                        sb.Append( constraint );
                        firstItem = false;
                    }
                }

            }


            /// <summary>
            /// Override to read primary key definition form sub-section of table (not column level)
            /// </summary>
            protected virtual void DoReadIndexSection(IConfigSectionNode idxNode, RDBMSEntity table)
            {
                var idxName = idxNode.Value;
                if (idxName.IsNullOrWhiteSpace())
                {
                    m_CompileErrors.Add(new SchemaCompilationException(idxNode.RootPath,
                                               "Table '{0}' declares an index without a name '{1}'".Args(table.OriginalName, idxNode.RootPath)));
                    return;
                }
                var idx = new RDBMSEntity(table, idxNode, RDBMSEntityType.Index, idxName);
                TransformEntityName(idx);
            }


        #endregion

        #region .pvt


        #endregion



    }





}
