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
using System.Diagnostics;
using System.Runtime.InteropServices;

namespace NFX.OS
{
    /// <summary>
    /// Denots primary OS Families: Win/Mac/Lin*nix
    /// </summary>
    public enum OSFamily
    {
        Undetermined = 0,
        Windows = 1,
        Linux = 100,
        Mac = 200
    }

    /// <summary>
    /// Provides current memory status snapshot
    /// </summary>
    [Serializable]
    public struct MemoryStatus
    {
      public uint LoadPct { get; internal set; }

      public ulong TotalPhysicalBytes { get; internal set; }
      public ulong AvailablePhysicalBytes { get; internal set; }

      public ulong TotalPageFileBytes { get; internal set; }
      public ulong AvailablePageFileBytes { get; internal set; }

      public ulong TotalVirtBytes { get; internal set; }
      public ulong AvailableVirtBytes { get; internal set; }
    }


    /// <summary>
    /// Facilitates various computer-related tasks such as CPU usage, memory utilization etc.
    /// </summary>
    public static class Computer
    {
        #if LINUX

        #else
            //Note this is Windows-only implementation
            private static PerformanceCounter s_CPUCounter;
            private static PerformanceCounter s_RAMAvailableCounter;

            static Computer()
            {
                s_CPUCounter = new PerformanceCounter("Processor", "% Processor Time", "_Total", true);
                s_RAMAvailableCounter = new PerformanceCounter("Memory", "Available MBytes", true);
            }

        #endif


        /// <summary>
        /// Returns current computer-wide CPU utilization percentage
        /// </summary>
        public static int CurrentProcessorUsagePct
        {
            get
            {
                #if LINUX
                    return 0;
                #else
                    //Note this is Windows-only implementation
                    return (int)s_CPUCounter.NextValue();
                #endif
            }
        }


        /// <summary>
        /// Returns current computer-wide RAM availability in mbytes
        /// </summary>
        public static int CurrentAvailableMemoryMb
        {
            get
            {
                #if LINUX
                    return 0;
                #else
                    //Note this is Windows-only implementation
                    return (int)s_RAMAvailableCounter.NextValue();
                #endif
            }
        }


        private static OSFamily s_OSFamily;

        /// <summary>
        /// Rsturns OS family for this computer: Linux vs Win vs Mac
        /// </summary>
        public static OSFamily OSFamily
        {
          get
          {
            if (s_OSFamily!=OSFamily.Undetermined) return s_OSFamily;

            switch (System.Environment.OSVersion.Platform)
            {
                case PlatformID.Unix:
                {
                    // Need to check for Mac-specific root folders, because Mac may get reported as UNIX
                    if (System.IO.Directory.Exists("/Applications")
                        & System.IO.Directory.Exists("/System")
                        & System.IO.Directory.Exists("/Users")
                        & System.IO.Directory.Exists("/Volumes"))
                    {
                        s_OSFamily = OSFamily.Mac;
                        break;
                    }
                    else
                    {
                        s_OSFamily = OSFamily.Linux;
                        break;
                    }
                }

                case PlatformID.MacOSX: { s_OSFamily = OSFamily.Mac; break;}

                default: {  s_OSFamily = OSFamily.Windows;  break; };
            }

            return s_OSFamily;
          }

        }


        private static string s_UniqueNetworkSignature;

        /// <summary>
        /// Returns network signature for this machine which is unique in the eclosing network segment (MAC-based)
        /// </summary>
        public static string UniqueNetworkSignature
        {
          get
          {
             if (s_UniqueNetworkSignature==null)
               s_UniqueNetworkSignature = NetworkUtils.GetMachineUniqueMACSignature();

             return s_UniqueNetworkSignature;
          }
        }



        #if LINUX
          public static MemoryStatus GetMemoryStatus() { return new MemoryStatus(); }
        #else
          [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Auto)]
          private class MEMORYSTATUSEX
          {
            public uint dwLength;
            public uint dwMemoryLoad;
            public ulong ullTotalPhys;
            public ulong ullAvailPhys;
            public ulong ullTotalPageFile;
            public ulong ullAvailPageFile;
            public ulong ullTotalVirtual;
            public ulong ullAvailVirtual;
            public ulong ullAvailExtendedVirtual;
            public MEMORYSTATUSEX()
            {
              this.dwLength = (uint)Marshal.SizeOf(typeof(MEMORYSTATUSEX));
            }
          }

          [return: MarshalAs(UnmanagedType.Bool)]
          [DllImport("kernel32.dll", CharSet = CharSet.Auto, SetLastError = true)]
          private static extern bool GlobalMemoryStatusEx([In, Out] MEMORYSTATUSEX lpBuffer);

          public static MemoryStatus GetMemoryStatus()
          {
            var stat = new MEMORYSTATUSEX();

            if (OSFamily==OS.OSFamily.Windows)
             GlobalMemoryStatusEx( stat );

            return new MemoryStatus()
            {
              LoadPct = stat.dwMemoryLoad,

              TotalPhysicalBytes = stat.ullTotalPhys,
              AvailablePhysicalBytes = stat.ullAvailPhys,

              TotalPageFileBytes = stat.ullTotalPageFile,
              AvailablePageFileBytes = stat.ullAvailPageFile,

              TotalVirtBytes = stat.ullTotalVirtual,
              AvailableVirtBytes = stat.ullAvailVirtual
            };
          }
        #endif

    }
}
