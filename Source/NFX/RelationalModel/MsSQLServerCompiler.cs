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

namespace NFX.RelationalModel
{

    /// <summary>
    /// Compiles relation schema into Ms SQL Server scripts
    /// </summary>
    public class MsSQLServerCompiler : RDBMSCompiler
    {
        #region .ctor

            public MsSQLServerCompiler(Schema schema) : base(schema)
            {

            }


        #endregion

        #region Properties
            public override TargetType Target
            {
                get { return TargetType.MsSQLServer; }
            }

            public override string Name
            {
                get { return "MsSQL"; }
            }
        #endregion


        #region Protected

            public override string GetQuotedIdentifierName(RDBMSEntityType type, string name)
            {
                if (type!=RDBMSEntityType.Domain)
                    return "[{0}]".Args(name);
                else
                    return name;
            }

            public override string GetStatementDelimiterScript(RDBMSEntityType type, bool start)
            {
                return start ? string.Empty : "\nGO\n";
            }

        #endregion

    }
}
