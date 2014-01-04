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
    using System.IO;
    using System.Runtime.InteropServices;
    using System.Security.Permissions;
    using System.Threading;

    using global::CxxPlugin.Commands;
    using global::CxxPlugin.Options;

    using ExtensionHelpers;

    using ExtensionTypes;

    using SonarRestService;

    using VSSonarPlugins;

    /// <summary>
    /// The analysis mode.
    /// </summary>
    public enum AnalysisMode
    {
        /// <summary>
        /// The incremental.
        /// </summary>
        Incremental,

        /// <summary>
        /// The preview.
        /// </summary>
        Preview,

        /// <summary>
        /// The full.
        /// </summary>
        Full,
    }

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
        /// The executor.
        /// </summary>
        private readonly ICommandExecution executor;

        /// <summary>
        /// The project.
        /// </summary>
        private readonly Resource project;

        /// <summary>
        /// The configuration.
        /// </summary>
        private readonly ConnectionConfiguration configuration;

        /// <summary>
        /// The plugin options.
        /// </summary>
        private readonly IPluginsOptions pluginOptions;

        /// <summary>
        /// The rest service.
        /// </summary>
        private readonly ISonarRestService restService = new SonarRestService(new JsonSonarConnector());

        /// <summary>
        /// The issues.
        /// </summary>
        private readonly List<Issue> issues;

        /// <summary>
        /// The issues in file.
        /// </summary>
        private readonly List<Issue> issuesInFile;

        /// <summary>
        /// Gets or sets the sonar version.
        /// </summary>
        private readonly double sonarVersion;

        /// <summary>
        /// The executors lauched.
        /// </summary>
        private int executorsLauched;

        /// <summary>
        /// The file path to analyse.
        /// </summary>
        private string filePathToAnalyse;

        /// <summary>
        /// The command plugin.
        /// </summary>
        private IPlugin commandPlugin;

        /// <summary>
        /// The project key.
        /// </summary>
        private string projectKey;

        /// <summary>
        /// The project item.
        /// </summary>
        private VsProjectItem projectItem;

        /// <summary>
        /// The solution path.
        /// </summary>
        private string solutionPath;

        /// <summary>
        /// The mode.
        /// </summary>
        private AnalysisMode mode;

        /// <summary>
        /// The profileIn.
        /// </summary>
        private Profile profile;

        /// <summary>
        /// The source in server.
        /// </summary>
        private string sourceInServer;

        /// <summary>
        /// The modified lines only.
        /// </summary>
        private bool modifiedLinesOnly;

        /// <summary>
        /// Initializes a new instance of the <see cref="CxxLocalExtension"/> class.
        /// </summary>
        /// <param name="commandPlugin">
        /// The command plugin.
        /// </param>
        /// <param name="executor">
        /// The executor.
        /// </param>
        /// <param name="connectionConfiguration">
        /// The connection Configuration.
        /// </param>
        /// <param name="resource">
        /// The resource.
        /// </param>
        /// <param name="sonarVersion">
        /// The sonar Version.
        /// </param>
        public CxxLocalExtension(IPlugin commandPlugin, ICommandExecution executor, ConnectionConfiguration connectionConfiguration, Resource resource, double sonarVersion)
        {
            this.configuration = connectionConfiguration;
            this.project = resource;
            this.commandPlugin = commandPlugin;
            this.executor = executor;
            this.pluginOptions = commandPlugin.GetPluginControlOptions(connectionConfiguration, resource);
            this.sonarVersion = sonarVersion;

            this.issuesInFile = new List<Issue>();
            this.issues = new List<Issue>();
            this.sensors = new Dictionary<string, ASensor>
                               {
                                   { CppCheckSensor.SKey, new CppCheckSensor(executor, this.pluginOptions) },
                                   { RatsSensor.SKey, new RatsSensor(executor, this.pluginOptions) },
                                   { VeraSensor.SKey, new VeraSensor(executor, this.pluginOptions) },
                                   { CxxExternalSensor.SKey, new CxxExternalSensor(executor, this.pluginOptions) }
                               };
        }

        /// <summary>
        /// The local analysis completed.
        /// </summary>
        public event EventHandler LocalAnalysisCompleted;

        /// <summary>
        /// The std out event.
        /// </summary>
        public event EventHandler StdOutEvent;

        /// <summary>
        /// The std err event.
        /// </summary>
        public event EventHandler StdErrEvent;

        /// <summary>
        /// The launch local analysis.
        /// </summary>
        /// <param name="file">
        /// The file.
        /// </param>
        /// <param name="projectKeyIn">
        /// The project Key.
        /// </param>
        /// <param name="profileIn">
        /// The profileIn.
        /// </param>
        /// <param name="fileSourceInServer">
        /// The file Source In Server.
        /// </param>
        /// <param name="onModifiedLinesOnly">
        /// The on Modified Lines Only.
        /// </param>
        /// <returns>
        /// The <see cref="Thread"/>.
        /// </returns>
        public Thread GetFileAnalyserThread(VsProjectItem file, string projectKeyIn, Profile profileIn, string fileSourceInServer, bool onModifiedLinesOnly)
        {
            this.sourceInServer = fileSourceInServer;
            this.modifiedLinesOnly = onModifiedLinesOnly;
            this.profile = profileIn;
            this.filePathToAnalyse = file.FilePath;
            this.projectItem = file;
            this.projectKey = projectKeyIn;
            return new Thread(this.LocalFileAnalyser);       
        }

        /// <summary>
        /// The local File analyser.
        /// </summary>
        public void LocalFileAnalyser()
        {
            if (this.executorsLauched > 0)
            {
                return;
            }

            this.issuesInFile.Clear();
            this.issues.Clear();
            this.executorsLauched = this.sensors.Count;
            foreach (var sensor in this.sensors)
            {
                try
                {
                    CxxPlugin.WriteLogMessage(this, this.StdOutEvent, "Launching  Analysis on: " + sensor.Key + " " + this.filePathToAnalyse);
                    var process = sensor.Value.LaunchSensor(this, this.StdOutEvent, this.filePathToAnalyse, this.ProcessSensorsIssues);
                    if (process.HasExited && process.ExitCode != 0)
                    {
                        CxxPlugin.WriteLogMessage(this, this.StdOutEvent, "Analysis Failed: " + sensor.Key + " " + this.filePathToAnalyse + " Error Code: " + process.ExitCode);
                    }
                }
                catch (Exception ex)
                {
                    CxxPlugin.WriteLogMessage(this, this.StdOutEvent, "Exception on Analysing: " + this.filePathToAnalyse + " " + ex.StackTrace);
                    var tempEvent = this.LocalAnalysisCompleted;
                    if (tempEvent != null)
                    {
                        tempEvent(this, new LocalAnalysisCompletedEventArgs(CxxPlugin.Key, sensor.Key + " Failed To Execute", ex));
                    }
                }
            }
        }

        /// <summary>
        /// The local analyser.
        /// </summary>
        public void LocalAnalyser()
        {
            this.issues.Clear();
            var localAnalyser = new LocalAnalyser(this.sensors, this.commandPlugin, this.profile);
            var options = (CxxOptionsController)this.pluginOptions;

            if (options.MavenIsChecked)
            {
                localAnalyser.RunMavenRunner(this.mode, this.executor, this.pluginOptions, this.StdOutEvent, this.solutionPath, this.project, this.configuration, this.sonarVersion);
            }
            else
            {
                localAnalyser.RunSonarRunner(this.mode, this.executor, this.pluginOptions, this.StdOutEvent, this.solutionPath, this.project, this.configuration, this.sonarVersion);
            }

            // process issues
            try
            {
                this.issues.Clear();

                if (this.sonarVersion >= 4.0)
                {
                    var reportOfIssues = this.restService.ParseReportOfIssues(Path.Combine(this.solutionPath, "target\\sonar\\sonar-report.json"));
                    this.issues.AddRange(reportOfIssues);
                    this.issues.AddRange(localAnalyser.Issues);
                }
                else
                {
                    if (this.sonarVersion >= 3.7)
                    {
                        this.issues.AddRange(this.restService.ParseDryRunReportOfIssues(Path.Combine(this.solutionPath, @".sonar\dryRun.json")));
                        this.issues.AddRange(this.restService.ParseReportOfIssues(Path.Combine(this.solutionPath, @".sonar\sonar-report.json")));
                    }
                    else
                    {
                        if (this.sonarVersion >= 3.4)
                        {
                            var reportOfIssues = this.restService.ParseReportOfIssuesOld(Path.Combine(this.solutionPath, ".sonar\\dryRun.json"));
                            this.issues.AddRange(reportOfIssues);
                            this.issues.AddRange(localAnalyser.Issues);
                        }
                    }
                }

                var tempEvent = this.LocalAnalysisCompleted;
                if (tempEvent != null)
                {
                    tempEvent(this, new LocalAnalysisCompletedEventArgs(CxxPlugin.Key, string.Empty, null));
                }
            }
            catch (Exception ex)
            {
                CxxPlugin.WriteLogMessage(this, this.StdOutEvent, "Exception on Analysis: " + ex.Message + " " + ex.StackTrace);
                var tempEvent = this.LocalAnalysisCompleted;
                if (tempEvent != null)
                {
                    tempEvent(this, new LocalAnalysisCompletedEventArgs(CxxPlugin.Key, string.Empty + " Failed To Execute", ex));
                }
            }
        }

        /// <summary>
        /// The get incremental analyser thread.
        /// </summary>
        /// <param name="solutionpath">
        /// The solutionpath.
        /// </param>
        /// <param name="profileIn">
        /// The profile In.
        /// </param>
        /// <returns>
        /// The <see cref="Thread"/>.
        /// </returns>
        public Thread GetIncrementalAnalyserThread(string solutionpath, Profile profileIn)
        {
            this.profile = profileIn;
            this.mode = AnalysisMode.Incremental;
            this.solutionPath = solutionpath;
            return new Thread(this.LocalAnalyser);
        }

        /// <summary>
        /// The get preview analyser thread.
        /// </summary>
        /// <param name="solutionpath">
        /// The solutionpath.
        /// </param>
        /// <param name="profileIn">
        /// The profile In.
        /// </param>
        /// <returns>
        /// The <see cref="Thread"/>.
        /// </returns>
        public Thread GetPreviewAnalyserThread(string solutionpath, Profile profileIn)
        {
            this.profile = profileIn;
            this.mode = AnalysisMode.Preview;
            this.solutionPath = solutionpath;
            return new Thread(this.LocalAnalyser);
        }

        /// <summary>
        /// The get analyser thread.
        /// </summary>
        /// <param name="solutionpath">
        /// The solutionpath.
        /// </param>
        /// <returns>
        /// The <see cref="Thread"/>.
        /// </returns>
        public Thread GetAnalyserThread(string solutionpath)
        {
            this.mode = AnalysisMode.Full;
            this.solutionPath = solutionpath;
            return new Thread(this.LocalAnalyser);
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
        /// The process sensors issues.
        /// </summary>
        /// <param name="key">
        /// The key.
        /// </param>
        /// <param name="sensorReportedLines">
        /// The sensor reported lines.
        /// </param>
        private void ProcessSensorsIssues(string key, List<string> sensorReportedLines)
        {
            lock (this.lockThis)
            {
                try
                {
                    var issuesInTool = this.sensors[key].GetViolations(sensorReportedLines);
                    foreach (var issue in issuesInTool)
                    {
                        var path1 = Path.GetFullPath(this.projectItem.FilePath); 
                        var path2 = issue.Component;
                        if (path1.Equals(path2))
                        {
                            issue.Component = this.commandPlugin.GetResourceKey(this.projectItem, this.projectKey);
                            var ruleInProfile = Profile.IsRuleEnabled(this.profile, issue.Rule);
                            if (ruleInProfile != null)
                            {
                                issue.Severity = ruleInProfile.Severity;
                                this.issuesInFile.Add(issue);
                            }
                        }
                    }
                }
                catch (Exception ex)
                {
                    CxxPlugin.WriteLogMessage(this, this.StdOutEvent, "Exception: " + key + " " + ex.StackTrace);
                }
                
                this.executorsLauched--;
                if (this.executorsLauched == 0)
                {
                    try
                    {
                        if (this.modifiedLinesOnly)
                        {
                            if (this.sourceInServer != null)
                            {
                                var diffReport = VsSonarUtils.GetDifferenceReport(this.filePathToAnalyse, this.sourceInServer, false);
                                this.issues.AddRange(VsSonarUtils.GetIssuesInModifiedLinesOnly(this.issuesInFile, diffReport));
                            }
                            else
                            {
                                this.issues.AddRange(this.issuesInFile);
                            }
                        }
                        else
                        {
                            this.issues.AddRange(this.issuesInFile);
                        }
                    }
                    catch (Exception)
                    {
                        this.issues.AddRange(this.issuesInFile);
                    }

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
