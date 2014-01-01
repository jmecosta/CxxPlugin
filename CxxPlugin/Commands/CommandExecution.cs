// --------------------------------------------------------------------------------------------------------------------
// <copyright file="CommandExecution.cs" company="Copyright © 2013 Tekla Corporation. Tekla is a Trimble Company">
//     Copyright (C) 2013 [Jorge Costa, Jorge.Costa@tekla.com]
// </copyright>
// --------------------------------------------------------------------------------------------------------------------
// This program is free software; you can redistribute it and/or modify it under the terms of the GNU Lesser General Public License
// as published by the Free Software Foundation; either version 3 of the License, or (at your option) any later version.
//
// This program is distributed in the hope that it will be useful, but WITHOUT ANY WARRANTY; without even the implied warranty
// of MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the GNU Lesser General Public License for more details. 
// You should have received a copy of the GNU Lesser General Public License along with this program; if not, write to the Free
// Software Foundation, Inc., 59 Temple Place, Suite 330, Boston, MA 02111-1307 USA
// --------------------------------------------------------------------------------------------------------------------
namespace CxxPlugin.Commands
{
    using System;
    using System.Collections.Generic;
    using System.Diagnostics;

    /// <summary>
    /// Implementation of Command Execution
    /// </summary>
    public class CommandExecution : ICommandExecution
    {
        /// <summary>
        /// The CommandLineOuput.
        /// </summary>
        public readonly List<string> CommandLineOuput = new List<string>();

        /// <summary>
        /// The command line error.
        /// </summary>
        public readonly List<string> CommandLineError = new List<string>();

        /// <summary>
        /// The execute command.
        /// </summary>
        /// <param name="rootPath">
        /// The root path.
        /// </param>
        /// <param name="cmd">
        /// The cmd.
        /// </param>
        /// <param name="args">
        /// The args.
        /// </param>
        /// <param name="environment">
        /// The environment.
        /// </param>
        /// <param name="stderrOutputReceived">
        /// The stderr output received.
        /// </param>
        /// <param name="stdoutOutputReceived">
        /// The stdout output received.
        /// </param>
        /// <param name="processEnded">
        /// The process ended.
        /// </param>
        /// <returns>
        /// The <see cref="Process"/>.
        /// </returns>
        public Process ExecuteCommand(
            string rootPath,
            string cmd,
            string args,
            Dictionary<string, string> environment,
            DataReceivedEventHandler stderrOutputReceived,
            DataReceivedEventHandler stdoutOutputReceived,
            EventHandler processEnded)
        {
            var processStartInfo = new ProcessStartInfo
            {
                FileName = cmd,
                Arguments = args,
                WindowStyle = ProcessWindowStyle.Normal,
                UseShellExecute = false,
                RedirectStandardOutput = true,
                RedirectStandardError = true,
                RedirectStandardInput = true,
                CreateNoWindow = true
            };

            if (!string.IsNullOrEmpty(rootPath))
            {
                processStartInfo.WorkingDirectory = rootPath;
            }

            if (environment != null)
            {
                foreach (var entry in environment)
                {
                    processStartInfo.EnvironmentVariables.Add(entry.Key, entry.Value);
                }
            }

            if (Environment.OSVersion.Version.Major >= 6)
            {
                processStartInfo.Verb = "runas";
            }

            var processexec = new Process { StartInfo = processStartInfo };
            if (stdoutOutputReceived != null)
            {
                processexec.OutputDataReceived += stdoutOutputReceived;
            }

            if (stderrOutputReceived != null)
            {
                processexec.ErrorDataReceived += stderrOutputReceived;
            }

            if (processEnded != null)
            {
                processexec.Exited += processEnded;
            }

            processexec.EnableRaisingEvents = true;
            processexec.Start();
            processexec.BeginOutputReadLine();
            processexec.BeginErrorReadLine();

            if (processEnded == null)
            {
                processexec.WaitForExit();
            }

            return processexec;
        }
    }
}
