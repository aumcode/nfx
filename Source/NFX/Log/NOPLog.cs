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
 * Originated: 2011.01.31
 * Revision: NFX 1.0  2011.02.06
 */
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using NFX.ApplicationModel;

namespace NFX.Log
{

  /// <summary>
  /// Represents log that does not do anything
  /// </summary>
  public sealed class NOPLog : ApplicationComponent, ILog
  {
      private static NOPLog s_Instance = new NOPLog();


      private NOPLog(): base()
      {

      }

      /// <summary>
      /// Returns a singlelton instance of the log that does not do anything
      /// </summary>
      public static NOPLog Instance
      {
        get { return s_Instance; }
      }

    #region ILog Members


            public Message LastWarning     { get {return null;}}

            public Message LastError       { get {return null;}}

            public Message LastCatastrophy { get {return null;}}


        public void Write(Message msg)
        {

        }

        public void Write(Message msg, bool urgent)
        {

        }


        public void Write(MessageType type, string text, string topic = null, string from = null)
        {

        }

        public void Write(MessageType type, string text, bool urgent, string topic = null, string from = null)
        {

        }

        public Time.TimeLocation TimeLocation
        {
            get { return App.TimeLocation; }
        }

        public DateTime LocalizedTime
        {
            get { return App.LocalizedTime; }
        }

        public DateTime UniversalTimeToLocalizedTime(DateTime utc)
        {
            return App.UniversalTimeToLocalizedTime(utc);
        }

        public DateTime LocalizedTimeToUniversalTime(DateTime local)
        {
            return App.LocalizedTimeToUniversalTime(local);
        }


    #endregion


  }
}
