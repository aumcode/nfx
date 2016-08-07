/*<FILE_LICENSE>
* NFX (.NET Framework Extension) Unistack Library
* Copyright 2003-2014 Dmitriy Khmaladze, IT Adapter Inc / 2015-2016 Aum Code LLC
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

namespace NFX.Health
{
    /// <summary>
    /// Property bag to report health check results
    /// </summary>
    public class CheckResult : Dictionary<string, object>
    {
       private Exception m_Exception;
       private bool m_Skipped;


       /// <summary>
       /// Indicates whether health check succeeded
       /// </summary>
       public bool Successful
       {
         get { return m_Exception == null && !m_Skipped;}
       }


       /// <summary>
       /// Exception that surfaced during check, consequently failing the check
       /// </summary>
       public Exception Exception
       {
         get { return m_Exception; }
         internal set { m_Exception = value; }
       }

       /// <summary>
       /// Indicates whether check was not run
       /// </summary>
       public bool Skipped
       {
         get { return m_Skipped; }
         internal set { m_Skipped = value; }
       }



    }
}
