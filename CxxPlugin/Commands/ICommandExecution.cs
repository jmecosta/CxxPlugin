// --------------------------------------------------------------------------------------------------------------------
// <copyright file="ICommandExecution.cs" company="Trimble Navigation Limited">
//     Copyright (C) 2013 [Jorge Costa, Jorge.Costa@trimble.com]
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

namespace CxxPlugin
{
    using System;
    using System.Collections.Generic;
    using System.Diagnostics;

    /// <summary>
    /// Interface for command execution 
    /// </summary>
    public interface ICommandExecution
    {
        /// <summary>
        /// The execute command.
        /// </summary>
        /// <param name="rootPath">
        /// The root Path.
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
        Process ExecuteCommand(
            string rootPath,
            string cmd,
            string args,
            Dictionary<string, string> environment,
            DataReceivedEventHandler stderrOutputReceived,
            DataReceivedEventHandler stdoutOutputReceived,
            EventHandler processEnded);
    }
}
