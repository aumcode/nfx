/*<FILE_LICENSE>
* NFX (.NET Framework Extension) Unistack Library
* Copyright 2003-2018 Agnicore Inc. portions ITAdapter Corp. Inc.
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


namespace NFX.DataAccess.CRUD
{
  /// <summary>
  /// Represents a buffer-less unidicrectional reader that binds IEnumerable(Row) and the backend resource
  /// (such as SQLReader or other object which is internal to the backend).
  /// The cursor is NOT thread-safe and must be disposed properly by closing all resources associated with it.
  /// Only one iteration (one call to GetEnumerator) is possible
  /// </summary>
  public abstract class Cursor : DisposableObject, IEnumerable<Row>
  {
              protected class enumerator : IEnumerator<Row>
              {
                internal enumerator(Cursor cursor,IEnumerator<Row> original)
                {
                   Original = original;
                   Cursor = cursor;
                }

                private readonly IEnumerator<Row> Original;
                private readonly Cursor Cursor;

                public Row Current
                {
                  get { return Original.Current; }
                }

                public void Dispose()
                {
                  Original.Dispose();
                  if (!Cursor.DisposeStarted) Cursor.Dispose();
                }

                object System.Collections.IEnumerator.Current
                {
                  get { return Original.Current; }
                }

                public bool MoveNext()
                {
                  return Original.MoveNext();
                }

                public void Reset()
                {
                  Original.Reset();
                }
              }

    /// <summary>
    /// This method is not inteded to be called by application developers
    /// </summary>
    protected Cursor(IEnumerable<Row> source)
    {
      m_Source = source;
    }

    protected override void Destructor()
    {
      DisposableObject.DisposeAndNull(ref m_Enumerator);
    }

    protected IEnumerable<Row>  m_Source;
    protected enumerator  m_Enumerator;


    public virtual IEnumerator<Row> GetEnumerator()
    {
      EnsureObjectNotDisposed();

      if (m_Enumerator!=null) throw new DataAccessException(StringConsts.CRUD_CURSOR_ALREADY_ENUMERATED_ERROR);

      m_Enumerator = new enumerator(this, m_Source.GetEnumerator());
      return m_Enumerator;
    }

    System.Collections.IEnumerator System.Collections.IEnumerable.GetEnumerator()
    {
      return this.GetEnumerator();
    }
  }

  /// <summary>
  /// Represents a cursor that basically does nothing else but
  /// passes through control to source IEnumerable(Row)
  /// </summary>
  public sealed class PassthroughCursor : Cursor
  {
    public PassthroughCursor(IEnumerable<Row> source) : base(source)
    {
    }
  }



}
