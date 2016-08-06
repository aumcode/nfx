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
