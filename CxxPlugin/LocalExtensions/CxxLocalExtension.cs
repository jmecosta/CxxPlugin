// --------------------------------------------------------------------------------------------------------------------
// <copyright file="CxxLocalExtension.cs" company="Copyright © 2014 jmecsoftware">
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
    using System.Collections;
    using System.Collections.Generic;
    using System.IO;
    using System.Linq;
    using System.Runtime.InteropServices;
    using System.Security.Permissions;
    using System.Threading;

    using VSSonarPlugins;
    using VSSonarPlugins.Types;
    using VSSonarPlugins.Helpers;

    /// <summary>
    ///     The cxx server extension.
    /// </summary>
    [ComVisible(false)]
    [HostProtection(SecurityAction.LinkDemand, Synchronization = true, ExternalThreading = true)]
    public class CxxLocalExtension : WaitHandle, IFileAnalyser
    {
        #region Fields

        /// <summary>
        ///     The command plugin.
        /// </summary>
        private readonly IAnalysisPlugin commandPlugin;

        /// <summary>
        ///     The issues.
        /// </summary>
        private readonly List<Issue> issues;

        /// <summary>
        ///     The lock this.
        /// </summary>
        private readonly object lockThis = new object();

        /// <summary>
        ///     The sensors.
        /// </summary>
        private readonly Dictionary<string, ASensor> sensors;

        #endregion

        #region Constructors and Destructors

        public CxxLocalExtension(IAnalysisPlugin commandPlugin,
            INotificationManager notificationManager,
            IConfigurationHelper configurationHelper,
            ISonarRestService sonarRestService,
            IVSSonarQubeCmdExecutor exec)
        {
            this.commandPlugin = commandPlugin;
            this.notificationManager = notificationManager;
            this.configurationHelper = configurationHelper;
            this.sonarRestService = sonarRestService;
            this.executor = exec;

            this.issues = new List<Issue>();
            this.sensors = new Dictionary<string, ASensor>
                               {
                                   {
                                       CppCheckSensor.SKey, 
                                       new CppCheckSensor(notificationManager, configurationHelper, sonarRestService)
                                   }, 
                                   { RatsSensor.SKey, new RatsSensor(notificationManager, configurationHelper, sonarRestService) }, 
                                   { VeraSensor.SKey, new VeraSensor(notificationManager, configurationHelper, sonarRestService) }, 
                                   { PcLintSensor.SKey, new PcLintSensor(notificationManager, configurationHelper, sonarRestService) }, 
                                   {
                                       CxxExternalSensor.SKey, 
                                       new CxxExternalSensor(notificationManager, configurationHelper, sonarRestService)
                                   }
                               };
        }

        #endregion

        #region Public Events

        /// <summary>
        ///     The std out event.
        /// </summary>
        public event EventHandler StdOutEvent;
        private readonly INotificationManager notificationManager;
        private readonly IConfigurationHelper configurationHelper;
        private readonly ISonarRestService sonarRestService;
        private readonly IVSSonarQubeCmdExecutor executor;

        #endregion

        #region Public Methods and Operators

        /// <summary>
        /// The execute analysis on file.
        /// </summary>
        /// <param name="itemInView">
        /// The item In View.
        /// </param>
        /// <param name="externlProfile">
        /// The externl profile.
        /// </param>
        /// <param name="projectIn">
        /// The project.
        /// </param>
        /// <returns>
        /// The <see>
        ///         <cref>List</cref>
        ///     </see>
        ///     .
        /// </returns>
        public List<Issue> ExecuteAnalysisOnFile(VsFileItem itemInView, Profile externlProfile, Resource project, ISonarConfiguration conf)
        {
            var threads = new List<Thread>();
            var allIssues = new List<Issue>();

            foreach (var sensor in this.sensors)
            {
                CxxPlugin.WriteLogMessage(this.notificationManager, this.GetType().ToString(), 
                    "Launching  Analysis on: " + sensor.Key + " " + itemInView.FilePath);
                threads.Add(
                    this.RunSensorThread(
                        this.StdOutEvent, 
                        itemInView, 
                        sensor,
                        externlProfile, 
                        false, 
                        string.Empty, 
                        allIssues,
                        project,
                        this.executor));
            }

            foreach (Thread thread in threads)
            {
                thread.Join();
            }

            return allIssues;
        }

        /// <summary>
        ///     The get issues.
        /// </summary>
        /// <returns>
        ///     The
        ///     <see>
        ///         <cref>List</cref>
        ///     </see>
        ///     .
        /// </returns>
        public List<Issue> GetIssues()
        {
            return this.issues;
        }
      
        /// <summary>
        /// The run sensor thread.
        /// </summary>
        /// <param name="output">
        /// The output.
        /// </param>
        /// <param name="file">
        /// The file.
        /// </param>
        /// <param name="sensor">
        /// The sensor.
        /// </param>
        /// <param name="projectIn">
        /// The project in.
        /// </param>
        /// <param name="profileIn">
        /// The profile in.
        /// </param>
        /// <param name="changedlines">
        /// The changedlines.
        /// </param>
        /// <param name="sourceRef">
        /// The source ref.
        /// </param>
        /// <param name="issuesToReturn">
        /// The issues to return.
        /// </param>
        /// <returns>
        /// The <see cref="Thread"/>.
        /// </returns>
        public Thread RunSensorThread(
            EventHandler output, 
            VsFileItem file, 
            KeyValuePair<string, ASensor> sensor, 
            Profile profileIn, 
            bool changedlines, 
            string sourceRef, 
            List<Issue> issuesToReturn,
            Resource project,
            IVSSonarQubeCmdExecutor exec)
        {
            var t =
                new Thread(
                    () =>
                    this.RunSensor(output, file, sensor, profileIn, changedlines, sourceRef, issuesToReturn,project, exec));
            t.Start();
            return t;
        }

        /// <summary>
        /// The stop all execution.
        /// </summary>
        /// <param name="runningThread">
        /// The running thread.
        /// </param>
        public void StopAllExecution(Thread runningThread)
        {
        }

        #endregion

        #region Methods

        /// <summary>
        /// The is issue supported.
        /// </summary>
        /// <param name="issue">
        /// The issue.
        /// </param>
        /// <returns>
        /// The <see cref="bool"/>.
        /// </returns>
        public bool IsIssueSupported(Issue issue, ISonarConfiguration conf)
        {
            return CxxPlugin.IsSupported(issue.Component);
        }

        /// <summary>
        /// The process sensors issues.
        /// </summary>
        /// <param name="key">
        /// The key.
        /// </param>
        /// <param name="sensorReportedLines">
        /// The sensor reported lines.
        /// </param>
        /// <param name="itemInView">
        /// The item in view.
        /// </param>
        /// <param name="projectIn">
        /// The project in.
        /// </param>
        /// <param name="profileIn">
        /// The profile in.
        /// </param>
        /// <param name="modfiedLines">
        /// The modfied lines.
        /// </param>
        /// <param name="fileSourceRef">
        /// The file source ref.
        /// </param>
        /// <param name="issuesToReturn">
        /// The issues to return.
        /// </param>
        private void ProcessSensorsIssues(
            string key, 
            List<string> sensorReportedLines, 
            VsFileItem itemInView, 
            Profile profileIn, 
            bool modfiedLines, 
            string fileSourceRef, 
            List<Issue> issuesToReturn,
            Resource project)
        {
            var issuesPerTool = new List<Issue>();
            try {
                var data = configurationHelper.ReadSetting(Context.GlobalPropsId, OwnersId.ApplicationOwnerId, GlobalIds.ExtensionDebugModeEnabled).Value;
                if (data.ToLower().Equals("true"))
                {
                    foreach (string line in sensorReportedLines)
                    {
                        CxxPlugin.WriteLogMessage(this.notificationManager, this.GetType().ToString(), "[" + key + "] : " + line);
                    }
                }
            } catch(Exception)
            {
            }
            

            try
            {
                List<Issue> issuesInTool = this.sensors[key].GetViolations(sensorReportedLines);
                foreach (Issue issue in issuesInTool)
                {
                    string path1 = Path.GetFullPath(itemInView.FilePath);
                    string path2 = issue.Component;
                    if (path1.Equals(path2))
                    {
                        issue.Component = this.commandPlugin.GetResourceKey(itemInView, false);
                        Rule ruleInProfile = profileIn.GetRule(issue.Rule);
                        if (ruleInProfile != null)
                        {
                            issue.Debt = ruleInProfile.DebtRemFnCoeff;
                            issue.Severity = ruleInProfile.Severity;
                            issuesPerTool.Add(issue);
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                CxxPlugin.WriteLogMessage(this.notificationManager, this.GetType().ToString(), "Exception: " + key + " " + ex.StackTrace);
            }

            lock (this.lockThis)
            {
                try
                {
                    if (modfiedLines)
                    {
                        if (fileSourceRef != null)
                        {
                            ArrayList diffReport = VsSonarUtils.GetSourceDiffFromStrings(
                                itemInView.FilePath, 
                                fileSourceRef);
                            issuesToReturn.AddRange(
                                VsSonarUtils.GetIssuesInModifiedLinesOnly(issuesPerTool, diffReport));
                        }
                        else
                        {
                            issuesToReturn.AddRange(issuesPerTool);
                        }
                    }
                    else
                    {
                        issuesToReturn.AddRange(issuesPerTool);
                    }
                }
                catch (Exception)
                {
                    issuesToReturn.AddRange(issuesPerTool);
                }
            }
        }

        /// <summary>
        /// The run sensor.
        /// </summary>
        /// <param name="output">
        /// The output.
        /// </param>
        /// <param name="file">
        /// The file.
        /// </param>
        /// <param name="sensor">
        /// The sensor.
        /// </param>
        /// <param name="projectIn">
        /// The project in.
        /// </param>
        /// <param name="profileIn">
        /// The profile in.
        /// </param>
        /// <param name="changedlines">
        /// The changedlines.
        /// </param>
        /// <param name="sourceRef">
        /// The source ref.
        /// </param>
        /// <param name="issuesToReturn">
        /// The issues to return.
        /// </param>
        private void RunSensor(
            EventHandler output, 
            VsFileItem file, 
            KeyValuePair<string, ASensor> sensor,  
            Profile profileIn, 
            bool changedlines, 
            string sourceRef, 
            List<Issue> issuesToReturn,
            Resource project,
            IVSSonarQubeCmdExecutor exec)
        {
            try
            {
                List<string> lines = sensor.Value.LaunchSensor(this, output, file.FilePath, exec);
                this.ProcessSensorsIssues(
                    sensor.Key, 
                    lines, 
                    file,  
                    profileIn, 
                    changedlines, 
                    sourceRef, 
                    issuesToReturn,
                    project);
            }
            catch (Exception ex)
            {
                CxxPlugin.WriteLogMessage(this.notificationManager, this.GetType().ToString(),
                    sensor.Key + " : Exception on Analysis Plugin : " + file.FilePath + " " + ex.StackTrace);
                CxxPlugin.WriteLogMessage(this.notificationManager, this.GetType().ToString(), sensor.Key + " : StdError: " + exec.GetStdError());
                CxxPlugin.WriteLogMessage(this.notificationManager, this.GetType().ToString(), sensor.Key + " : StdOut: " + exec.GetStdError());
            }
        }

        #endregion
    }
}