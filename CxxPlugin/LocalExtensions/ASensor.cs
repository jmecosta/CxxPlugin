// --------------------------------------------------------------------------------------------------------------------
// <copyright file="ASensor.cs" company="Copyright © 2014 jmecsoftware">
//     Copyright (C) 2014 [jmecsoftware, jmecsoftware2014@tekla.com]
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

namespace CxxPlugin.LocalExtensions
{
    using System;
    using System.Collections.Generic;
    using System.Diagnostics;
    using System.IO;

    using global::CxxPlugin.Commands;

    using ExtensionTypes;

    using Microsoft.FSharp.Collections;

    using MSBuild.Tekla.Tasks.Executor;

    /// <summary>
    /// The Sensor interface.
    /// </summary>
    public abstract class ASensor
    {
        /// <summary>
        /// The repository key.
        /// </summary>
        protected readonly string RepositoryKey;

        /// <summary>
        /// The use stdout.
        /// </summary>
        protected readonly bool UseStdout;

        /// <summary>
        /// The CommandLineOuput.
        /// </summary>
        private List<string> commandLineOuput = new List<string>();

        /// <summary>
        /// The command line error.
        /// </summary>
        private List<string> commandLineError = new List<string>();

        /// <summary>
        /// The call back handler.
        /// </summary>
        private Action<string, List<string>> callBackHandler;

        /// <summary>
        /// Initializes a new instance of the <see cref="ASensor"/> class.
        /// </summary>
        /// <param name="repositoryKey">
        /// The repository key.
        /// </param>
        /// <param name="ctrl">
        /// The ctrl.
        /// </param>
        /// <param name="useStdout">
        /// The use Stdout.
        /// </param>
        protected ASensor(string repositoryKey, bool useStdout)
        {
            this.RepositoryKey = repositoryKey;
            this.UseStdout = useStdout;
        }

        /// <summary>
        /// The launch sensor.
        /// </summary>
        /// <param name="caller">
        ///     The caller.
        /// </param>
        /// <param name="logger">
        ///     The logger.
        /// </param>
        /// <param name="filePath">
        ///     The file Path.
        /// </param>
        /// <param name="executor"></param>
        /// <param name="callBackHandlerIn">
        ///     The call back handler in.
        /// </param>
        /// <returns>
        /// The <see cref="Process"/>.
        /// </returns>
        public virtual FSharpList<string> LaunchSensor(
            object caller,
            EventHandler logger,
            string filePath,
            ICommandExecutor executor)
        {
            var commandline = "[" + Directory.GetParent(filePath) + "] : " + this.GetCommand() + " "
                              + this.GetArguments() + " " + filePath;
            CxxPlugin.WriteLogMessage(caller, logger, commandline);
            executor.ExecuteCommand(
                this.GetCommand(),
                this.GetArguments() + " " + filePath,
                this.GetEnvironment(),
                string.Empty);

            return this.UseStdout ? executor.GetStdOut : executor.GetStdError;
        }

        /// <summary>
        /// The get key.
        /// </summary>
        /// <returns>
        /// The <see cref="string"/>.
        /// </returns>
        public string GetKey()
        {
            return this.RepositoryKey;
        }

        /// <summary>
        /// The get violations.
        /// </summary>
        /// <param name="lines">
        ///     The lines.
        /// </param>
        /// <returns>
        /// The VSSonarPlugin.SonarInterface.ResponseMappings.Violations.ViolationsResponse.
        /// </returns>
        public abstract List<Issue> GetViolations(FSharpList<string> lines);

        /// <summary>
        /// The get environment.
        /// </summary>
        /// <returns>
        /// The <see>
        ///         <cref>Dictionary</cref>
        ///     </see>
        ///     .
        /// </returns>
        public abstract FSharpMap<string, string> GetEnvironment();

        /// <summary>
        /// The get command.
        /// </summary>
        /// <returns>
        /// The <see cref="string"/>.
        /// </returns>
        public abstract string GetCommand();

        /// <summary>
        /// The get arguments.
        /// </summary>
        /// <returns>
        /// The <see cref="string"/>.
        /// </returns>
        public abstract string GetArguments();

        /// <summary>
        /// The process output data received.
        /// </summary>
        /// <param name="sender">
        /// The sender.
        /// </param>
        /// <param name="e">
        /// The e.
        /// </param>
        private void ProcessOutputDataReceived(object sender, DataReceivedEventArgs e)
        {
            if (!string.IsNullOrEmpty(e.Data))
            {
                this.commandLineOuput.Add(e.Data);
            }
        }

        /// <summary>
        /// The process error data received.
        /// </summary>
        /// <param name="sender">
        /// The sender.
        /// </param>
        /// <param name="e">
        /// The e.
        /// </param>
        private void ProcessErrorDataReceived(object sender, DataReceivedEventArgs e)
        {
            if (!string.IsNullOrEmpty(e.Data))
            {
                this.commandLineError.Add(e.Data);
            }
        }

        /// <summary>
        /// The event handler function.
        /// </summary>
        /// <param name="sender">
        /// The sender.
        /// </param>
        /// <param name="e">
        /// The e.
        /// </param>
        private void EventHandlerFunction(object sender, EventArgs e)
        {
            this.callBackHandler(this.RepositoryKey, this.UseStdout ? this.commandLineOuput : this.commandLineError);
        }

        public static FSharpMap<string, string> ConvertCsMapToFSharpMap(Dictionary<string, string> data)
        {
            var map = new FSharpMap<string, string>(new List<Tuple<string, string>>());
            foreach (var elem in data)
            {
               map = map.Add(elem.Key, elem.Value);
            }

            return map;
        }
    }
}
