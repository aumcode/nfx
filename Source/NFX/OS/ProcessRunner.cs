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
 * Revision: NFX 0.3  2009.10.12
 */

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using System.Diagnostics;

namespace NFX.OS
{
  /// <summary>
  /// Provides simple process invocation and output capture functionality
  /// </summary>
  public sealed class ProcessRunner
  {
    #region .ctor
        public ProcessRunner()
        {

        }
    #endregion

    #region Private Fields
        private string m_ProcessCmd;
        private string m_Arguments;
        private StringBuilder m_BufferedOutput;
        private int m_ExitCode;
        private bool m_TimedOutAndKilled;
        private int m_ExecutionTimeMs;
    #endregion

    #region Properties

        /// <summary>
        /// Gets/sets process to run
        /// </summary>
        public string ProcessCmd
        {
          get { return m_ProcessCmd ?? string.Empty; }
          set { m_ProcessCmd = value; }
        }

        /// <summary>
        /// Gets/sets process invocation arguments
        /// </summary>
        public string Arguments
        {
          get { return m_Arguments ?? string.Empty; }
          set { m_Arguments = value; }
        }

        /// <summary>
        /// Returns buffered process output
        /// </summary>
        public string BufferedOutput
        {
          get { return m_BufferedOutput != null ? m_BufferedOutput.ToString() : string.Empty; }
        }


        /// <summary>
        /// Returns process last exit code
        /// </summary>
        public int ExitCode
        {
          get { return m_ExitCode; }
        }

        /// <summary>
        /// Returns process last exit code
        /// </summary>
        public bool TimedOutAndKilled
        {
          get { return m_TimedOutAndKilled; }
        }

        /// <summary>
        /// Returns process execution time in milliseconds
        /// </summary>
        public int ExecutionTimeMs
        {
          get { return m_ExecutionTimeMs; }
        }

    #endregion


    #region Public

        /// <summary>
        /// Invokes a process specified by cmd parameters blocking until process finishes and returns stdout.
        /// Pass optional timeout parameter that will abort the process execution when exceeded, or zero for unlimited time.
        /// </summary>
        public static string Run(string cmd, string args, out int exitCode, out bool timedOut, int timeoutMs = 0)
        {
          ProcessRunner runner = new ProcessRunner();
          runner.ProcessCmd = cmd;
          runner.Arguments = args;
          exitCode = runner.Run(timeoutMs);
          timedOut = runner.TimedOutAndKilled;
          return runner.BufferedOutput;
        }
        /// <summary>
        /// Invokes a process specified by cmd parameters blocking until process finishes and returns stdout.
        /// Pass optional timeout parameter that will abort the process execution when exceeded, or zero for unlimited time.
        /// </summary>
        public static string Run(string cmd, string args,out bool timedOut, int timeoutMs = 0)
        {
          int code;
          return Run(cmd, args, out code, out timedOut, timeoutMs);
        }

        /// <summary>
        /// Invokes a process specified by cmd parameters blocking until process finishes and returns stdout
        /// Pass optional timeout parameter that will abort the process execution when exceeded, or zero for unlimited time.
        /// </summary>
        public static string Run(string cmd, int timeoutMs = 0)
        {
          int code;
          bool timedOut;
          return Run(cmd, null, out code, out timedOut, timeoutMs);
        }


        /// <summary>
        /// Runs process blocking until it finishes, or timeout is exceeded. Pass zero for time-unconstrained execution
        /// </summary>
        public int Run(int timeoutMs = 0)
        {
          m_BufferedOutput = new StringBuilder();
          m_ExitCode = 0;
          var watch = new Stopwatch();
          using (Process p = new Process())
          {
            p.StartInfo.FileName = ProcessCmd;
            p.StartInfo.Arguments = Arguments;
            p.StartInfo.UseShellExecute = false;//so we can redir IO gotta be false
            p.StartInfo.CreateNoWindow = true;
            p.StartInfo.RedirectStandardOutput = true;
            p.StartInfo.WindowStyle = ProcessWindowStyle.Hidden;

            p.OutputDataReceived += (sender, e) =>
            {
                if (e.Data != null)
                {
                    m_BufferedOutput.AppendLine(e.Data);
                }
            };


            p.Start();
            watch.Start();

            p.BeginOutputReadLine();

            if (timeoutMs>0)
                p.WaitForExit(timeoutMs);
            else
                p.WaitForExit();


            m_ExecutionTimeMs = (int)watch.ElapsedMilliseconds;

            if (!p.HasExited)
            {
                m_TimedOutAndKilled = true;
                p.Kill();
            }
            else
                m_ExitCode = p.ExitCode;


          }//using

          return m_ExitCode;
        }


    #endregion

  }
}
