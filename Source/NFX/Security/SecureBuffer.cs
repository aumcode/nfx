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

namespace NFX.Security
{
  [NFX.Serialization.Slim.SlimSerializationProhibited]
  public sealed class SecureBuffer : DisposableObject
  {
    public SecureBuffer(int capacity = 32)
    {
      if (capacity < 0)
        throw new SecurityException(StringConsts.ARGUMENT_ERROR + "SecureBuffer.ctor(capacity < 0)");

      m_IsSealed = false;
      m_Content = new byte[capacity];
    }

    protected override void Destructor() { Forget(); }



    private bool m_IsSealed;

    [NonSerialized]
    private byte[] m_Content;
    private int m_Length;



    public byte[] Content
    {
      get
      {
        if (!m_IsSealed)
        {
          Forget();
          throw new SecurityException(GetType().Name + ".Content.!IsSealed");
        }
        return m_Content;
      }
    }
    public bool IsSealed { get { return m_IsSealed; } }

    public void Forget()
    {
      if (Content == null) return;
      Array.Clear(Content, 0, Content.Length);
    }

    public void Push(byte b)
    {
      if (m_IsSealed)
      {
        Forget();
        throw new SecurityException(GetType().Name + ".Push(IsSealed)");
      }
      if (m_Length == m_Content.Length)
      {
        var content = new byte[m_Length * 2];
        Array.Copy(m_Content, content, m_Length);
        Array.Clear(m_Content, 0, m_Length);
        m_Content = content;
      }
      m_Content[m_Length] = b;
      m_Length++;
    }

    public void Seal()
    {
      if (m_IsSealed)
      {
        Forget();
        throw new SecurityException(GetType().Name + ".Seal(IsSealed)");
      }
      m_IsSealed = true;
      if (m_Length != m_Content.Length)
      {
        var content = new byte[m_Length];
        Array.Copy(m_Content, content, m_Length);
        Array.Clear(m_Content, 0, m_Content.Length);
        m_Content = content;
      }
    }
  }
}
