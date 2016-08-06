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
using System.Runtime.InteropServices;
using System.Linq;
using System.Text;
using System.Net;
using System.Reflection;


namespace NFX.Wave
{
  public static class PlatformUtils
  {


        /// <summary>
        /// Must be called after Listener.Start();
        /// </summary>
        public unsafe static void SetRequestQueueLimit(HttpListener listener, long len)
        {
           #if LINUX
               return;
           #else

            var prop_RequestQueueHandle = typeof(HttpListener).GetProperties(BindingFlags.NonPublic | BindingFlags.Instance)
                                                              .First(p => p.Name.Equals("RequestQueueHandle"));

            var requestQueueHandle = (CriticalHandle)prop_RequestQueueHandle.GetValue(listener, null);
            var result = HttpSetRequestQueueProperty(requestQueueHandle,
                                                     HTTP_SERVER_PROPERTY.HttpServerQueueLengthProperty,
                                                     new IntPtr((void*)&len),
                                                     (uint)Marshal.SizeOf(len),
                                                     0,
                                                     IntPtr.Zero);

            if (result != 0)
                throw new HttpListenerException((int) result);

           #endif
        }

        #if LINUX

        #else

                [DllImport("httpapi.dll", CallingConvention = CallingConvention.StdCall, SetLastError = true)]
                internal static extern uint HttpSetRequestQueueProperty(
                    CriticalHandle requestQueueHandle,
                    HTTP_SERVER_PROPERTY serverProperty,
                    IntPtr pPropertyInfo,
                    uint propertyInfoLength,
                    uint reserved,
                    IntPtr pReserved);

                internal enum HTTP_SERVER_PROPERTY
                {
                    HttpServerAuthenticationProperty,
                    HttpServerLoggingProperty,
                    HttpServerQosProperty,
                    HttpServerTimeoutsProperty,
                    HttpServerQueueLengthProperty,
                    HttpServerStateProperty,
                    HttpServer503VerbosityProperty,
                    HttpServerBindingProperty,
                    HttpServerExtendedAuthenticationProperty,
                    HttpServerListenEndpointProperty,
                    HttpServerChannelBindProperty,
                    HttpServerProtectionLevelProperty,
                }
        #endif


  }
}
