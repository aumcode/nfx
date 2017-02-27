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


// Author: Serge Aleynikov <saleyn@gmail.com>
// Date:   2013-07-23
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using NFX.Environment;
using System.Text.RegularExpressions;
using System.IO;

namespace NFX.Log.Destinations
{
  /// <summary>
  /// Provides a file storage destination implementation
  /// </summary>
  public abstract class TextFileDestination : FileDestination
  {

    protected TextFileDestination(string name) : base(name) { }


    private StreamWriter m_Writer;

    protected override void DoOpenStream()
    {
      m_Writer = new StreamWriter(m_Stream);
    }

    protected override void DoCloseStream()
    {
      DisposableObject.DisposeAndNull(ref m_Writer);
    }

    /// <summary>
    /// Warning: don't override this method in derived destinations, use
    /// DoFormatMessage() instead!
    /// </summary>
    protected override void DoWriteMessage(Message msg)
    {
      var txtMsg = DoFormatMessage(msg);
      m_Writer.WriteLine( txtMsg );
    }

    protected internal override void DoPulse()
    {
      base.DoPulse();
      m_Writer.Flush();
    }

    /// <summary>
    /// Called when message is to be written to stream
    /// </summary>
    protected abstract string DoFormatMessage(Message msg);
  }
}
