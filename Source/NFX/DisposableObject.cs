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


/* NFX by ITAdapter
 * Originated: 2006.01
 * Revision: NFX 1.0  2011.01.31
 */
using System;
using System.Runtime.Serialization;
using System.Collections.Generic;
using System.Text;

namespace NFX
{

  /// <summary>
  /// General-purpose base class for objects that need to be disposed
  /// </summary>
  [Serializable]
  public abstract class DisposableObject : IDisposable
  {

    #region .ctor / .dctor
     
      ~DisposableObject()
      {
        if (!m_Disposed)
          try
          {
            Destructor();
          }
          finally
          {
            m_Disposed = true;
          }
      }

    #endregion

    #region Private Fields
      private volatile bool m_Disposed;
    #endregion

    #region Properties
    
      /// <summary>
      /// Indicates whether this object was already disposed
      /// </summary>
      public bool Disposed
      {
        get { return m_Disposed; }
      }
    #endregion


    #region Public/Protected

    /// <summary>
    /// Override this method to do actual destructor work
    /// </summary>
    protected virtual void Destructor()
    {
    }

    /// <summary>
    /// Checks to see whether object has been disposed and throws an exception if it has
    /// </summary>
    public void EnsureObjectNotDisposed()
    {
      if (m_Disposed)
        throw new DisposedObjectException(StringConsts.OBJECT_DISPOSED_ERROR+" {0}".Args(this.GetType().FullName));
    }

    #endregion

    #region IDisposable Members

        /// <summary>
        /// Deterministically disposes object. DO NOT OVERRIDE this method, override Destructor() instead UNLESS
        ///  you need to achive special thread-safe dispose behavior
        /// </summary>
        public virtual void Dispose()
        {
            if (!m_Disposed)
            {
                try
                {
                    Destructor();
                }
                finally
                {
                    m_Disposed = true;
                    GC.SuppressFinalize(this);
                }
            }
        }

    #endregion

  }
                                  


  /// <summary>
  /// This exception is thrown from DisposableObject.EnsureObjectNotDisposed() method
  /// </summary>
  [Serializable]
  public class DisposedObjectException : NFXException
  {
    public DisposedObjectException()
    {
    }

    public DisposedObjectException(string message)
      : base(message)
    {
    }

    public DisposedObjectException(string message, Exception inner)
      : base(message, inner)
    {
    }

    protected DisposedObjectException(SerializationInfo info, StreamingContext context)
      : base(info, context)
    {
    }
  }








}
