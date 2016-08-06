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

namespace NFX.Log.Syslog
{

    /// <summary>
    /// Standard SYSLOG severity levels http://en.wikipedia.org/wiki/Syslog
    /// </summary>
    public enum FacilityLevel
    {
        Kernel =0,
        User   =1,
        Mail   =2,
        Daemon =3,
        Auth   =4,
        Syslog =5,
        Lpr    =6,
        News   =7,
        UUCP   =8,
        Cron   =9,
        AuthPriv    =10,
        Ftp         =11,
        Ntp         =12,
        LogAudit    =13,
        LogAlert    =14,
        ClockDaemon =15,
        Local0      =16,
        Local1,
        Local2,
        Local3,
        Local4,
        Local5,
        Local6,
        Local7
    }



    /// <summary>
    /// Standard SYSLOG severity levels http://en.wikipedia.org/wiki/Syslog
    /// </summary>
    public enum SeverityLevel
    {
        Emergency= 0,
        Alert=1,
        Critical=2,
        Error=3,
        Warning=4,
        Notice=5,
        Information=6,
        Debug=7,
    }


}
