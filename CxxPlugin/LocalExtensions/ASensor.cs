// --------------------------------------------------------------------------------------------------------------------
// <copyright file="ASensor.cs" company="Copyright © 2013 Tekla Corporation. Tekla is a Trimble Company">
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

namespace CxxPlugin.LocalExtensions
{
    using System;
    using System.Collections.Generic;
    using System.Diagnostics;

    using ExtensionHelpers;

    using ExtensionTypes;

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
        /// The executor.
        /// </summary>
        protected readonly ICommandExecution ProcessCtrl;

        /// <summary>
        /// The CommandLineOuput.
        /// </summary>
        private readonly List<string> commandLineOuput = new List<string>();

        /// <summary>
        /// The command line error.
        /// </summary>
        private readonly List<string> commandLineError = new List<string>();

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
        protected ASensor(string repositoryKey, ICommandExecution ctrl, bool useStdout)
        {
            this.RepositoryKey = repositoryKey;
            this.ProcessCtrl = ctrl;
            this.UseStdout = useStdout;
        }
   
        /// <summary>
        /// Gets or sets the environment.
        /// </summary>
        protected Dictionary<string, string> Environment { get; set; }

        /// <summary>
        /// Gets or sets the args.
        /// </summary>
        protected string Args { get; set; }

        /// <summary>
        /// Gets or sets the command.
        /// </summary>
        protected string Command { get; set; }

        /// <summary>
        /// The launch sensor.
        /// </summary>
        /// <param name="executable">
        /// The executable.
        /// </param>
        /// <param name="args">
        /// The args.
        /// </param>
        /// <param name="environment">
        /// The environment.
        /// </param>
        /// <param name="callBackHandlerIn">
        /// The call back handler.
        /// </param>
        public virtual void LaunchSensor(string executable, string args, Dictionary<string, string> environment, Action<string, List<string>> callBackHandlerIn)
        {
            this.callBackHandler = callBackHandlerIn;
            this.ProcessCtrl.ExecuteCommand(executable, args, environment, this.ProcessOutputDataReceived, this.ProcessErrorDataReceived, this.EventHandlerFunction);
        }

        /// <summary>
        /// The launch sensor.
        /// </summary>
        /// <param name="filePath">
        /// The file Path.
        /// </param>
        /// <param name="callBackHandlerIn">
        /// The call back handler in.
        /// </param>
        public virtual void LaunchSensor(string filePath, Action<string, List<string>> callBackHandlerIn)
        {
            CxxPlugin.WriteLogMessage("LaunchSensor: " + this.Command + this.Args + " " + filePath);
            this.callBackHandler = callBackHandlerIn;
            this.ProcessCtrl.ExecuteCommand(this.Command, this.Args + " " + filePath, this.Environment, this.ProcessOutputDataReceived, this.ProcessErrorDataReceived, this.EventHandlerFunction);
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
        /// The lines.
        /// </param>
        /// <returns>
        /// The VSSonarPlugin.SonarInterface.ResponseMappings.Violations.ViolationsResponse.
        /// </returns>
        public abstract List<Issue> GetViolations(List<string> lines);

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
            CxxPlugin.WriteLogMessage("Finish Executing : " + this.RepositoryKey);
            this.callBackHandler(this.RepositoryKey, this.UseStdout ? this.commandLineOuput : this.commandLineError);
        }
    }
}
