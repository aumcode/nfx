using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using NFX;
using NFX.DataAccess.CRUD;

namespace NFX.DataAccess.MongoDB
{
  public sealed class MongoDBCursor : Cursor
  {
    internal MongoDBCursor(Connector.Cursor cursor, IEnumerable<Row> source) : base(source)
    {
      m_Cursor = cursor;
    }

    protected override void Destructor()
    {
      base.Destructor();
      DisposableObject.DisposeAndNull(ref m_Cursor);
    }

    private Connector.Cursor m_Cursor;
  }
}
