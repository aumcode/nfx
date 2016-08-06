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

namespace NFX.ApplicationModel
{
    /// <summary>
    /// Represents a session that does nothing and returns fake user
    /// </summary>
    public sealed class NOPSession : ISession
    {
        private static NOPSession s_Instance = new NOPSession();

        private static Guid s_ID = new Guid("DA132A02-0D36-47D3-A9D7-10BC64741A6E");

        private NOPSession()
        {

        }

        /// <summary>
        /// Returns a singlelton instance of the NOPSession
        /// </summary>
        public static NOPSession Instance
        {
           get { return s_Instance; }
        }


        public Guid ID
        {
            get { return s_ID; }
        }

        public Guid? OldID
        {
           get { return null;}
        }

        public bool IsNew
        {
           get { return false; }
        }

        public bool IsJustLoggedIn
        {
           get { return false; }
        }

        public DateTime? LastLoginUTC
        {
           get { return null; }
        }

        public SessionLoginType LastLoginType
        {
           get { return SessionLoginType.Unspecified; }
        }

        public bool IsEnded
        {
           get { return false; }
        }

        public Security.User User
        {
            get { return Security.User.Fake; }
            set {}
        }

        public string LanguageISOCode
        {
            get { return CoreConsts.ISO_LANG_ENGLISH;}
        }


        public IDictionary<object, object> Items
        {
            get { return new Dictionary<object, object>(); } //new instance is needed for thread safety
        }

        public object this[object key]
        {
           get { return null;}
           set {}
        }

        public void End()
        {

        }

        public void Acquire()
        {

        }

        public void Release()
        {

        }

        public void HasJustLoggedIn(SessionLoginType loginType)
        {

        }

        public void RegenerateID()
        {

        }

    }
}
