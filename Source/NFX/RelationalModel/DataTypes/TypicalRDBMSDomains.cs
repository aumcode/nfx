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

namespace NFX.RelationalModel.DataTypes
{
    public abstract class UncategorizedValue : RDBMSDomain
    {
    }

    public abstract class UncategorizedNumericValue : UncategorizedValue
    {
    }

    public abstract class UncategorizedTextualValue : UncategorizedValue
    {
    }

    public sealed class TVarchar : UncategorizedTextualValue
    {
        public readonly int Size;

        public TVarchar()
        {
            Size = 25;
        }
        public TVarchar(int size)
        {
            Size = size;
        }

        public override string GetTypeName(RDBMSCompiler compiler)
        {
            return "VARCHAR({0})".Args(Size);
        }
    }

    public sealed class TChar : UncategorizedTextualValue
    {
        public readonly int Size;

        public TChar()
        {
            Size = 5;
        }
        public TChar(int size)
        {
            Size = size;
        }

        public override string GetTypeName(RDBMSCompiler compiler)
        {
            return "CHAR({0})".Args(Size);
        }
    }

    //todo More research needed about int sizes in different targets
    public sealed class TInt : UncategorizedNumericValue
    {
        public readonly int Size;
        public readonly bool Unsigned;

        public TInt()
        {
            Size = 8;
        }
        public TInt(int size)
        {
            Size = size;
        }

        public TInt(int size, bool unsigned)
        {
            Size = size;
            Unsigned = unsigned;
        }

        public override string GetTypeName(RDBMSCompiler compiler)
        {
            string t;
            if (Size<2) t = "TINYINT";
            else
            if (Size<4) t = "SMALLINT";
            else
            if (Size<8) t = "INT";
            else
             t = "BIGINT";

            var u = "";
            if (Unsigned) u += " UNSIGNED";

            return "{0}({1}){2}".Args(t, Size, u);
        }
    }

    //todo More research needed about int sizes in different targets
    public sealed class TPercent : UncategorizedNumericValue
    {
        public readonly int Scale;

        public TPercent()
        {
            Scale = 4;
        }
        public TPercent(int scale)
        {
            Scale = scale;
        }

        public override string GetTypeName(RDBMSCompiler compiler)
        {
            return "NUMBER({0},{1})".Args(Scale+4, Scale);
        }
    }

    public sealed class TDouble : UncategorizedNumericValue
    {
        public TDouble()
        {
        }

        public override string GetTypeName(RDBMSCompiler compiler)
        {
            switch (compiler.Target)
            {
                case TargetType.PostgreSQL:     return "DOUBLE";
                case TargetType.Oracle:         return "BINARY_DOUBLE";
                case TargetType.MsSQLServer:    return "DOUBLE";
                default:                        return "NUMERIC(18,6)";
            }
        }
    }


    public enum DBCharType {Char, Varchar}

    public sealed class TEnum : UncategorizedTextualValue
    {
        [Config]
        public  DBCharType Type;

        [Config]
        public  int Size;

        public  string[] Values;

        public TEnum() {}

        public TEnum(string values) : this(DBCharType.Char, values)
        {
        }
        public TEnum(DBCharType type, string values)
        {
            Type = type;
            var vlist = values.Split('|');
            Size = vlist.Max(v=>v.Trim().Length);
            if (Size<1) Size = 1;
            Values = vlist;
        }

        public override string GetTypeName(RDBMSCompiler compiler)
        {
            return  Type==DBCharType.Varchar ? "VARCHAR({0})".Args(Size) : "CHAR({0})".Args(Size);
        }

        public override string GetColumnCheckScript(RDBMSCompiler compiler, RDBMSEntity column, Compiler.Outputs outputs)
        {
            var enumLine = string.Join(", ", Values.Select(v => compiler.EscapeString( v.Trim() )) );
            return compiler.TransformKeywordCase("check ({0} in ({1}))")
                           .Args(
                                  compiler.GetQuotedIdentifierName(RDBMSEntityType.Column, column.TransformedName),
                                  enumLine
                                );
        }

        public override void Configure(IConfigSectionNode node)
        {
            base.Configure(node);
            var nv = node["values"];
            if (nv.Exists)
                Values = nv.Children.Select(c=> c.AttrByName("key").Value).ToArray();
        }
    }





    public abstract class NonInteligentKey : RDBMSDomain
    {
        public override string GetTypeName(RDBMSCompiler compiler)
        {
            return "BIGINT UNSIGNED";
        }

        public override bool? GetColumnRequirement(RDBMSCompiler compiler)
        {
            return null;
        }
    }


    public class TCounter : NonInteligentKey
    {
        public override bool? GetColumnRequirement(RDBMSCompiler compiler)
        {
            return true;
        }


        public override string GetColumnAutoGeneratedScript(RDBMSCompiler compiler, RDBMSEntity column, Compiler.Outputs outputs)
        {
            switch(compiler.Target)
            {
                case TargetType.MySQL: return "AUTO_INCREMENT";
                case TargetType.PostgreSQL: {
                                              var seq = "SEQ_{0}_{1}".Args(column.ParentEntity.TransformedShortName, column.TransformedShortName);
                                              outputs[RDBMSCompiler.SEQUENCES_OUTPUT].AppendLine("CREATE SEQUENCE {0} START 0;".Args(seq));
                                              return "default nextval('{0}')".Args(seq);
                                            }
                case TargetType.MsSQLServer: return "IDENTITY(1,1)";
                default: return string.Empty;
            }
        }
    }

    public class TCounterRef : NonInteligentKey
    {
        public override void TransformColumnName(RDBMSCompiler compiler, RDBMSEntity column)
        {
            column.TransformedName = "C_{0}".Args(column.TransformedName);
        }
    }



    public class NonInteligentGDIDKey : NonInteligentKey
    {
        [Config]
        public  bool Required;


        public NonInteligentGDIDKey() {}

        public NonInteligentGDIDKey(bool required)
        {
          Required = required;
        }


        public override bool? GetColumnRequirement(RDBMSCompiler compiler)
        {
            return Required;
        }

        public override string GetTypeName(RDBMSCompiler compiler)
        {
            return Required ? "BINARY(12)" : "VARBINARY(12)";
        }
    }




    public class TGDID : NonInteligentGDIDKey
    {
        public TGDID():base(true){}
        public TGDID(bool required):base(required){}
    }

    public class TGDIDRef : NonInteligentGDIDKey
    {
        public TGDIDRef():base(true){}
        public TGDIDRef(bool required):base(required){}

        public override void TransformColumnName(RDBMSCompiler compiler, RDBMSEntity column)
        {
            column.TransformedName = "G_{0}".Args(column.TransformedName);
        }
    }

    public abstract class HumanAttribute : RDBMSDomain
    {

    }

    public class THumanAge : HumanAttribute
    {
        public override string GetTypeName(RDBMSCompiler compiler)
        {
            return "INT";
        }

        public override string GetColumnCheckScript(RDBMSCompiler compiler, RDBMSEntity column, Compiler.Outputs outputs)
        {
            return compiler.TransformKeywordCase("check ({0} > 0 and {0} < 200)").Args( compiler.GetQuotedIdentifierName(RDBMSEntityType.Column, column.TransformedName) );
        }
    }

    public class THumanName : HumanAttribute
    {
        public override string GetTypeName(RDBMSCompiler compiler)
        {
            return "VARCHAR(25)";
        }

        public override bool? GetColumnRequirement(RDBMSCompiler compiler)
        {
            return null;
        }
    }



    public class TDateTime : RDBMSDomain
    {
        public override string GetTypeName(RDBMSCompiler compiler)
        {
            return "DATETIME";
        }
    }

    public class TMonetaryAmount : RDBMSDomain
    {

        public override string GetTypeName(RDBMSCompiler compiler)
        {
            switch(compiler.Target)
            {
                case TargetType.PostgreSQL: return "money";
                case TargetType.Oracle:     return "NUMBER(12,4)";
                case TargetType.MsSQLServer: return "MONEY";
                default: return "DECIMAL(12,4)";
            }
        }
    }


    public class TScript : RDBMSDomain
    {
        public override string GetTypeName(RDBMSCompiler compiler)
        {
            return "TEXT";
        }
    }

    public class TNote : RDBMSDomain
    {
        public override string GetTypeName(RDBMSCompiler compiler)
        {
            return "TEXT";
        }
    }


    public class TBool : RDBMSDomain
    {
        public override string GetTypeName(RDBMSCompiler compiler)
        {
            switch(compiler.Target)
            {
                case TargetType.MySQL: return "tinyint(1)";
                case TargetType.PostgreSQL: return "boolean";
                case TargetType.Oracle:     return "char(1)";
                case TargetType.MsSQLServer: return "bit";
                default: return "char(1)";
            }
        }
    }


    public class TSex : RDBMSDomain
    {
        public override string GetTypeName(RDBMSCompiler compiler)
        {
           return "char(1)";
        }
    }

    public class TPhone : RDBMSDomain
    {

        public override string GetTypeName(RDBMSCompiler compiler)
        {
           return "char(25)";
        }
    }

    public class TEMail : RDBMSDomain
    {

        public override string GetTypeName(RDBMSCompiler compiler)
        {
           return "varchar(64)";
        }
    }

    public class TScreenName : RDBMSDomain
    {

        public override string GetTypeName(RDBMSCompiler compiler)
        {
           return "varchar(32)";
        }
    }


    public abstract class PostalAttribute : RDBMSDomain
    {

    }

    public class TAddrLine : PostalAttribute
    {
        public override string GetTypeName(RDBMSCompiler compiler)
        {
            return "varchar(32)";
        }
    }

    public class TAddrCity : PostalAttribute
    {
        public override string GetTypeName(RDBMSCompiler compiler)
        {
            return "varchar(32)";
        }
    }


    public class TAddrInternationalStateTerritory : PostalAttribute
    {
        public override string GetTypeName(RDBMSCompiler compiler)
        {
            return "varchar(32)";
        }
    }

    public class TAddrInternationalZipPostal : PostalAttribute
    {
        public override string GetTypeName(RDBMSCompiler compiler)
        {
            return "varchar(25)";
        }
    }


    public class TAddrUSState : PostalAttribute
    {
        public override string GetTypeName(RDBMSCompiler compiler)
        {
            return "char(2)";
        }
    }

    public class TAddrUSZip : PostalAttribute
    {
        public override string GetTypeName(RDBMSCompiler compiler)
        {
            return "char(10)";
        }
    }

}
