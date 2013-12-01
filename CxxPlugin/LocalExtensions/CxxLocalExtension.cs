// --------------------------------------------------------------------------------------------------------------------
// <copyright file="CxxLocalExtension.cs" company="Copyright © 2013 Tekla Corporation. Tekla is a Trimble Company">
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
    using System.Globalization;
    using System.IO;
    using System.Runtime.InteropServices;
    using System.Security.Permissions;
    using System.Threading;

    using ExtensionHelpers;

    using ExtensionTypes;

    using VSSonarPlugins;

    /// <summary>
    /// The cxx server extension.
    /// </summary>
    [ComVisible(false)]
    [HostProtection(SecurityAction.LinkDemand, Synchronization = true,
        ExternalThreading = true)]
    public class CxxLocalExtension : WaitHandle, ILocalAnalyserExtension
    {
        /// <summary>
        /// The sensors.
        /// </summary>
        private readonly Dictionary<string, ASensor> sensors;

        /// <summary>
        /// The lock this.
        /// </summary>
        private readonly object lockThis = new object();

        /// <summary>
        /// The executors lauched.
        /// </summary>
        private int executorsLauched;

        /// <summary>
        /// The file path to analyse.
        /// </summary>
        private string filePathToAnalyse;

        /// <summary>
        /// The issues.
        /// </summary>
        private List<Issue> issues;

        /// <summary>
        /// Initializes a new instance of the <see cref="CxxLocalExtension"/> class.
        /// </summary>
        /// <param name="commandPlugin">
        /// The command plugin.
        /// </param>
        /// <param name="executor">
        /// The executor.
        /// </param>
        public CxxLocalExtension(IPlugin commandPlugin, ICommandExecution executor)
        {
            this.issues = new List<Issue>();
            var options = commandPlugin.GetUsePluginControlOptions().GetOptions();
            this.sensors = new Dictionary<string, ASensor>
                               {
                                   { CppCheckSensor.SKey, new CppCheckSensor(executor, options) },
                                   { RatsSensor.SKey, new RatsSensor(executor, options) },
                                   { VeraSensor.SKey, new VeraSensor(executor, options) },
                                   { CxxExternalSensor.SKey, new CxxExternalSensor(executor, options) }
                               };
        }

        /// <summary>
        /// The local analysis completed.
        /// </summary>
        public event EventHandler LocalAnalysisCompleted;

        /// <summary>
        /// The launch local analysis.
        /// </summary>
        /// <param name="filePath">
        /// The file Path.
        /// </param>
        /// <returns>
        /// The <see cref="Thread"/>.
        /// </returns>
        public Thread GetFileAnalyserThread(string filePath)
        {
            this.filePathToAnalyse = filePath;
            return new Thread(this.LocalAnalyser);            
        }

        /// <summary>
        /// The local analyser.
        /// </summary>
        public void LocalAnalyser()
        {
            if (this.executorsLauched > 0)
            {
                return;
            }

            this.issues = new List<Issue>();
            this.executorsLauched = this.sensors.Count;
            foreach (var sensor in this.sensors)
            {
                try
                {
                    CxxPlugin.WriteLogMessage("Launching  Analysis on: " + sensor.Key + " " + this.filePathToAnalyse);
                    sensor.Value.LaunchSensor(this.filePathToAnalyse, this.CallbackDisplayLocalAnalysisMenuItem);                   
                }
                catch (Exception ex)
                {
                    CxxPlugin.WriteLogMessage("Exception on Analysing: " + this.filePathToAnalyse + " " + ex.StackTrace);
                    var tempEvent = this.LocalAnalysisCompleted;
                    if (tempEvent != null)
                    {
                        tempEvent(this, new LocalAnalysisCompletedEventArgs(CxxPlugin.Key, sensor.Key + " Failed To Execute", ex));
                    }
                }
            }
        }

        public Thread GetIncrementalAnalyserThread(string solutionpath)
        {
            throw new NotImplementedException();
        }

        public Thread GetPreviewAnalyserThread(string solutionpath)
        {
            throw new NotImplementedException();
        }

        public Thread GetAnalyserThread(string solutionpath)
        {
            throw new NotImplementedException();
        }

        /// <summary>
        /// The get issues.
        /// </summary>
        /// <returns>
        /// The <see>
        ///     <cref>List</cref>
        /// </see>
        ///     .
        /// </returns>
        public List<Issue> GetIssues()
        {
            return this.issues;
        }

        /// <summary>
        /// The callback display local analysis menu item.
        /// </summary>
        /// <param name="key">
        /// The key.
        /// </param>
        /// <param name="sensorReportedLines">
        /// The sensor reported lines.
        /// </param>
        private void CallbackDisplayLocalAnalysisMenuItem(string key, List<string> sensorReportedLines)
        {
            lock (this.lockThis)
            {
                try
                {
                    var issuesInTool = this.sensors[key].GetViolations(sensorReportedLines);
                    foreach (var issue in issuesInTool)
                    {
                        this.issues.Add(issue);
                    }
                }
                catch (Exception ex)
                {
                    CxxPlugin.WriteLogMessage("Exception: " + key + " " + ex.StackTrace);
                }
                
                this.executorsLauched--;
                if (this.executorsLauched == 0)
                {
                    var tempEvent = this.LocalAnalysisCompleted;
                    if (tempEvent != null)
                    {
                        tempEvent(this, new LocalAnalysisCompletedEventArgs(CxxPlugin.Key, string.Empty, null));
                    }
                }
            }
        }
    }
}
