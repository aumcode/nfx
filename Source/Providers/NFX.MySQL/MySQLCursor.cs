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
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using MySql.Data.MySqlClient;

using NFX;
using NFX.DataAccess.CRUD;

namespace NFX.DataAccess.MySQL
{
  public sealed class MySQLCursor : Cursor
  {
    internal MySQLCursor(MySQLCRUDQueryExecutionContext context, 
                         MySqlCommand command,
                         MySqlDataReader reader, 
                         IEnumerable<Row> source) : base(source)
    {
      m_Context = context;
      m_Command = command;
      m_Reader = reader;
    }

    protected override void Destructor()
    {
      base.Destructor();

      DisposableObject.DisposeAndNull(ref m_Reader);
      DisposableObject.DisposeAndNull(ref m_Command);

      if (m_Context.Transaction==null)
        m_Context.Connection.Dispose();
    }

    private MySQLCRUDQueryExecutionContext m_Context;
    private MySqlCommand m_Command;
    private MySqlDataReader m_Reader;
  }
}
