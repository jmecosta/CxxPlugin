// --------------------------------------------------------------------------------------------------------------------
// <copyright file="LocalAnalyser.cs" company="Copyright © 2013 Tekla Corporation. Tekla is a Trimble Company">
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
    using System.Collections;
    using System.Collections.Generic;
    using System.Diagnostics;
    using System.IO;
    using System.Security.Permissions;
    using System.Threading;

    using global::CxxPlugin.Commands;

    using ExtensionHelpers;

    using ExtensionTypes;

    using SonarRestService;

    using VSSonarPlugins;

    /// <summary>
    ///     The local analyser.
    /// </summary>
    [HostProtection(SecurityAction.LinkDemand, Synchronization = true, ExternalThreading = true)]
    public class LocalAnalyser
    {
        #region Fields

        /// <summary>
        ///     The lock number of file analysers.
        /// </summary>
        private readonly object lockNumberOfFileAnalysers = new object();

        /// <summary>
        ///     The lock this.
        /// </summary>
        private readonly object lockThis = new object();

        /// <summary>
        ///     The plugin.
        /// </summary>
        private readonly IPlugin plugin;

        /// <summary>
        ///     The profile.
        /// </summary>
        private readonly Profile profile;

        /// <summary>
        ///     The rest service.
        /// </summary>
        private readonly ISonarRestService restService = new SonarRestService(new JsonSonarConnector());

        /// <summary>
        ///     The sensors.
        /// </summary>
        private readonly Dictionary<string, ASensor> sensors;

        /// <summary>
        ///     The configuration.
        /// </summary>
        private ConnectionConfiguration configuration;

        /// <summary>
        ///     The file being analysed.
        /// </summary>
        private string fileBeingAnalysed;

        /// <summary>
        ///     The issues in file.
        /// </summary>
        private List<Issue> issuesInFile;

        /// <summary>
        ///     The mode.
        /// </summary>
        private AnalysisMode mode;

        /// <summary>
        ///     The numberof files to analyse.
        /// </summary>
        private int numberofFilesToAnalyse;

        /// <summary>
        ///     The options.
        /// </summary>
        private Dictionary<string, string> options;

        /// <summary>
        ///     The project.
        /// </summary>
        private Resource project;

        /// <summary>
        ///     The solution path.
        /// </summary>
        private string solutionPath;

        #endregion

        #region Constructors and Destructors

        /// <summary>
        /// Initializes a new instance of the <see cref="LocalAnalyser"/> class.
        /// </summary>
        /// <param name="sensors">
        /// The sensors.
        /// </param>
        /// <param name="plugin">
        /// The plugin.
        /// </param>
        /// <param name="profile">
        /// The profile.
        /// </param>
        public LocalAnalyser(Dictionary<string, ASensor> sensors, IPlugin plugin, Profile profile)
        {
            this.profile = profile;
            this.plugin = plugin;
            this.sensors = sensors;
        }

        #endregion

        #region Public Properties

        /// <summary>
        ///     Gets or sets the issues.
        /// </summary>
        public List<Issue> Issues { get; set; }

        /// <summary>
        ///     Gets or sets the loghandler.
        /// </summary>
        public EventHandler Loghandler { get; set; }

        #endregion

        #region Public Methods and Operators

        /// <summary>
        /// The run maven runner.
        /// </summary>
        /// <param name="modeIn">
        /// The mode In.
        /// </param>
        /// <param name="executor">
        /// The executor.
        /// </param>
        /// <param name="pluginOptions">
        /// The plugin options.
        /// </param>
        /// <param name="loghandler">
        /// The loghandler.
        /// </param>
        /// <param name="rootPath">
        /// The root path.
        /// </param>
        /// <param name="projectIn">
        /// The project.
        /// </param>
        /// <param name="conf">
        /// The conf.
        /// </param>
        /// <param name="sonarVersion">
        /// The sonar Version.
        /// </param>
        public void RunMavenRunner(
            AnalysisMode modeIn, 
            ICommandExecution executor, 
            IPluginsOptions pluginOptions, 
            EventHandler loghandler, 
            string rootPath, 
            Resource projectIn, 
            ConnectionConfiguration conf, 
            double sonarVersion)
        {
            this.options = pluginOptions.GetOptions();
            this.Loghandler = loghandler;
            this.project = projectIn;
            this.mode = modeIn;
            this.solutionPath = rootPath;
            this.configuration = conf;

            this.Issues = new List<Issue>();
            this.numberofFilesToAnalyse = 0;
            var environment = this.SetupMavenEnvironment();
            var command = this.options[projectIn.Key + ".MavenPath"];

            var msg = "Maven Started: mvn.bat " + this.GetArguments(sonarVersion).Replace(conf.Password, "xxxx");
            CxxPlugin.WriteLogMessage(this, loghandler, msg);
            executor.ExecuteCommand(
                rootPath, 
                command, 
                this.GetArguments(sonarVersion), 
                environment, 
                this.ProcessOutputDataReceived, 
                this.ProcessOutputDataReceived, 
                null);

            this.ProcessRemainingIssuesFromSensors();
        }

        /// <summary>
        /// The run sonar runner.
        /// </summary>
        /// <param name="modeIn">
        /// The mode In.
        /// </param>
        /// <param name="executor">
        /// The executor.
        /// </param>
        /// <param name="pluginOptions">
        /// The plugin options.
        /// </param>
        /// <param name="loghandler">
        /// The loghandler.
        /// </param>
        /// <param name="rootPath">
        /// The root path.
        /// </param>
        /// <param name="projectIn">
        /// The project.
        /// </param>
        /// <param name="conf">
        /// The conf.
        /// </param>
        /// <param name="sonarVersion">
        /// The sonar Version.
        /// </param>
        public void RunSonarRunner(
            AnalysisMode modeIn, 
            ICommandExecution executor, 
            IPluginsOptions pluginOptions, 
            EventHandler loghandler, 
            string rootPath, 
            Resource projectIn, 
            ConnectionConfiguration conf, 
            double sonarVersion)
        {
            this.options = pluginOptions.GetOptions();
            this.Loghandler = loghandler;
            this.project = projectIn;
            this.mode = modeIn;
            this.solutionPath = rootPath;
            this.configuration = conf;

            this.numberofFilesToAnalyse = 0;
            this.Issues = new List<Issue>();

            var environment = this.SetupSonarRunnerEnvironment();
            var command = this.options[projectIn.Key + ".SonarRunnerPath"];

            var msg = "Sonar Runner Started: sonarrunner.bat " + this.GetArguments(sonarVersion).Replace(conf.Password, "xxxx");
            CxxPlugin.WriteLogMessage(this, loghandler, msg);
            executor.ExecuteCommand(
                rootPath, 
                command, 
                this.GetArguments(sonarVersion), 
                environment, 
                this.ProcessOutputDataReceived, 
                this.ProcessOutputDataReceived, 
                null);

            this.ProcessRemainingIssuesFromSensors();
        }

        #endregion

        #region Methods

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
            try
            {
                List<Issue> issuesInTool = this.sensors[key].GetViolations(sensorReportedLines);
                foreach (Issue issue in issuesInTool)
                {
                    Rule ruleInProfile = Profile.IsRuleEnabled(this.profile, issue.Rule);
                    if (ruleInProfile != null)
                    {
                        issue.Severity = ruleInProfile.Severity;
                        var projecItem = new VsProjectItem(
                            string.Empty, 
                            this.fileBeingAnalysed, 
                            string.Empty, 
                            string.Empty, 
                            string.Empty, 
                            this.solutionPath);
                        issue.Component = this.plugin.GetResourceKey(projecItem, this.project.Key);
                        this.issuesInFile.Add(issue);
                    }
                }
            }
            catch (Exception ex)
            {
                CxxPlugin.WriteLogMessage(this, this.Loghandler, "Exception: " + key + " " + ex.StackTrace);
            }
        }

        /// <summary>
        ///     The decrement file analyser.
        /// </summary>
        private void DecrementFileAnalyser()
        {
            lock (this.lockNumberOfFileAnalysers)
            {
                this.numberofFilesToAnalyse += 1;
            }
        }

        /// <summary>
        /// The execute analysis on file.
        /// </summary>
        /// <param name="fileName">
        /// The file name.
        /// </param>
        private void ExecuteAnalysisOnFile(string fileName)
        {
            lock (this.lockThis)
            {
                this.fileBeingAnalysed = fileName;
                this.issuesInFile = new List<Issue>();
                foreach (var sensor in this.sensors)
                {
                    try
                    {
                        CxxPlugin.WriteLogMessage(this, this.Loghandler, "Launching  Analysis on: " + sensor.Key + " " + fileName);
                        sensor.Value.LaunchSensor(this, this.Loghandler, fileName, this.CallbackDisplayLocalAnalysisMenuItem).WaitForExit();
                    }
                    catch (Exception ex)
                    {
                        CxxPlugin.WriteLogMessage(this, this.Loghandler, "Exception on Analysing: " + fileName + " " + ex.StackTrace);
                    }
                }

                if (this.issuesInFile.Count == 0)
                {
                    return;
                }

                if (this.mode.Equals(AnalysisMode.Incremental))
                {
                    try
                    {
                        var projecItem = new VsProjectItem(
                            string.Empty, 
                            this.fileBeingAnalysed, 
                            string.Empty, 
                            string.Empty, 
                            string.Empty, 
                            this.solutionPath);
                        string elemKey = this.plugin.GetResourceKey(projecItem, this.project.Key);

                        Source source = this.restService.GetSourceForFileResource(this.configuration, elemKey);
                        if (source != null)
                        {
                            string sourcestr = VsSonarUtils.GetLinesFromSource(source, "\r\n");
                            ArrayList diffReport = VsSonarUtils.GetDifferenceReport(this.fileBeingAnalysed, sourcestr, false);
                            this.Issues.AddRange(VsSonarUtils.GetIssuesInModifiedLinesOnly(this.issuesInFile, diffReport));
                        }
                        else
                        {
                            this.Issues.AddRange(this.issuesInFile);
                        }
                    }
                    catch (Exception)
                    {
                        this.Issues.AddRange(this.issuesInFile);
                    }
                }
                else
                {
                    this.Issues.AddRange(this.issuesInFile);
                }
            }
        }

        /// <summary>
        ///     The file analyser number.
        /// </summary>
        /// <returns>
        ///     The <see cref="int" />.
        /// </returns>
        private int FileAnalyserNumber()
        {
            lock (this.lockNumberOfFileAnalysers)
            {
                return this.numberofFilesToAnalyse;
            }
        }

        /// <summary>
        /// The get arguments.
        /// </summary>
        /// <param name="sonarVersion">
        /// The sonar version.
        /// </param>
        /// <returns>
        /// The <see cref="string"/>.
        /// </returns>
        /// <exception>
        /// <cref>Exception</cref>
        /// </exception>
        private string GetArguments(double sonarVersion)
        {
            var args = "sonar:sonar -X -Dsonar.login=" + this.configuration.Username +
                       " -Dsonar.password=" + this.configuration.Password + " -Dsonar.host.url=" + this.configuration.Hostname;

            if (sonarVersion >= 4.0)
            {
                switch (this.mode)
                {
                    case AnalysisMode.Incremental:
                        args += " -Dsonar.analysis.mode=incremental";
                        args += " -Dsonar.preview.excludePlugins=" + this.options[this.project.Key + ".ExcludedPlugins"];
                        break;
                    case AnalysisMode.Full:
                        break;
                    case AnalysisMode.Preview:
                        args += " -Dsonar.analysis.mode=preview";
                        args += " -Dsonar.preview.excludePlugins=" + this.options[this.project.Key + ".ExcludedPlugins"];
                        break;
                }
            }
            else
            {
                if (sonarVersion >= 3.4)
                {
                    switch (this.mode)
                    {
                        case AnalysisMode.Incremental:
                            throw new Exception("Analysis Method Not Available in this Version of SonarQube");
                        case AnalysisMode.Full:
                            break;
                        case AnalysisMode.Preview:
                            args += " -Dsonar.dryRun.excludePlugins=" + this.options[this.project.Key + ".ExcludedPlugins"];
                            args += " -Dsonar.dryRun=true";
                            break;
                    }
                }
                else
                {
                    switch (this.mode)
                    {
                        case AnalysisMode.Incremental:
                            throw new Exception("Analysis Method Not Available in this Version of SonarQube");
                        case AnalysisMode.Full:
                            break;
                        case AnalysisMode.Preview:
                            throw new Exception("Analysis Method Not Available in this Version of SonarQube");
                    }
                }
            }

            foreach (var option in this.options)
            {
                if (option.Key.StartsWith(this.project.Key + ".propertyToRunner."))
                {
                    args += " -D" + option.Key.Replace(this.project.Key + ".propertyToRunner.", string.Empty) + "=" + option.Value;
                }
            }

            return args;
        }

        /// <summary>
        /// The get double parent.
        /// </summary>
        /// <param name="path">
        /// The path.
        /// </param>
        /// <returns>
        /// The <see cref="string"/>.
        /// </returns>
        /// <exception>
        /// <cref>Exception</cref>
        /// </exception>
        private string GetDoubleParent(string path)
        {
            if (string.IsNullOrEmpty(path))
            {
                throw new Exception("Path Give in Options was not defined, be sure configuration is correctly set");
            }

            var data = Directory.GetParent(Directory.GetParent(path).ToString()).ToString();
            if (!Directory.Exists(data))
            {
                throw new Exception("Directory Not Found: " + path);
            }

            return data;
        }

        /// <summary>
        /// The get parent.
        /// </summary>
        /// <param name="path">
        /// The path.
        /// </param>
        /// <returns>
        /// The <see cref="string"/>.
        /// </returns>
        /// <exception>
        /// <cref>Exception</cref>
        /// </exception>
        private string GetParent(string path)
        {
            if (string.IsNullOrEmpty(path))
            {
                throw new Exception("Path Give in Options was not defined, be sure configuration is correctly set");
            }

            var data = Directory.GetParent(path).ToString();
            if (!Directory.Exists(data))
            {
                throw new Exception("Directory Not Found: " + path);
            }

            return data;
        }

        /// <summary>
        ///     The increment file analyser.
        /// </summary>
        private void IncrementFileAnalyser()
        {
            lock (this.lockNumberOfFileAnalysers)
            {
                this.numberofFilesToAnalyse -= 1;
            }
        }

        /// <summary>
        /// The launch file analyser.
        /// </summary>
        /// <param name="data">
        /// The data.
        /// </param>
        private void LaunchFileAnalyser(string data)
        {
            if (data != null && data.Contains("[DEBUG]") && data.Contains("Populating index from"))
            {
                string[] separator = { "Populating index from" };
                string fileName = data.Split(separator, StringSplitOptions.RemoveEmptyEntries)[1];
                if (File.Exists(fileName))
                {
                    this.IncrementFileAnalyser();
                    this.ExecuteAnalysisOnFile(fileName);
                    this.DecrementFileAnalyser();
                }
            }
        }

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
            if (e.Data == null)
            {
                return;
            }

            if (e.Data.StartsWith("[DEBUG]"))
            {
                if (this.options.ContainsKey(this.project.Key + ".IsDebugChecked")
                    && this.options[this.project.Key + ".IsDebugChecked"].Equals("true"))
                {
                    CxxPlugin.WriteLogMessage(this, this.Loghandler, e.Data);
                }
            }
            else
            {
                CxxPlugin.WriteLogMessage(this, this.Loghandler, e.Data);
            }

            if (!this.mode.Equals(AnalysisMode.Full))
            {
                this.LaunchFileAnalyser(e.Data);
            }
        }

        /// <summary>
        ///     The sync file analyser.
        /// </summary>
        private void ProcessRemainingIssuesFromSensors()
        {
            while (this.FileAnalyserNumber() > 0)
            {
                Thread.Sleep(1000);
            }
        }

        /// <summary>
        /// The setup maven environment.
        /// </summary>
        /// <returns>
        /// The
        ///     <see>
        ///         <cref>Dictionary</cref>
        ///     </see>
        ///     .
        /// </returns>
        private Dictionary<string, string> SetupMavenEnvironment()
        {
            var javahome = this.GetDoubleParent(this.options["JavaBinaryPath"]);
            var mavenhome = this.GetDoubleParent(this.options[this.project.Key + ".MavenPath"]);
            var path = this.GetParent(this.options["JavaBinaryPath"]) + ";" +
                       this.GetParent(this.options[this.project.Key + ".MavenPath"]) + ";" +
                       Environment.GetEnvironmentVariable("PATH");

            if (Environment.GetEnvironmentVariable("JAVA_HOME") != null)
            {
                Environment.SetEnvironmentVariable("JAVA_HOME", string.Empty);
            }

            if (Environment.GetEnvironmentVariable("MAVEN_HOME") != null)
            {
                Environment.SetEnvironmentVariable("MAVEN_HOME", string.Empty);
            }

            if (Environment.GetEnvironmentVariable("PATH") != null)
            {
                Environment.SetEnvironmentVariable("PATH", string.Empty);
            }

            return new Dictionary<string, string> { { "PATH", path }, { "JAVA_HOME", javahome }, { "MAVEN_HOME", mavenhome } };
        }

        /// <summary>
        /// The setup sonar runner environment.
        /// </summary>
        /// <returns>
        /// The
        ///     <see>
        ///         <cref>Dictionary</cref>
        ///     </see>
        ///     .
        /// </returns>
        private Dictionary<string, string> SetupSonarRunnerEnvironment()
        {
            var javahome = this.GetDoubleParent(this.options["JavaBinaryPath"]);
            var sonarrunnerhome = this.GetDoubleParent(this.options[this.project.Key + ".SonarRunnerPath"]);
            var path = this.GetParent(this.options["JavaBinaryPath"]) + ";" +
                       this.GetParent(this.options[this.project.Key + ".SonarRunnerPath"]) + ";" +
                       Environment.GetEnvironmentVariable("PATH");

            if (Environment.GetEnvironmentVariable("JAVA_HOME") != null)
            {
                Environment.SetEnvironmentVariable("JAVA_HOME", string.Empty);
            }

            if (Environment.GetEnvironmentVariable("SONAR_RUNNER_HOME") != null)
            {
                Environment.SetEnvironmentVariable("SONAR_RUNNER_HOME", string.Empty);
            }

            if (Environment.GetEnvironmentVariable("PATH") != null)
            {
                Environment.SetEnvironmentVariable("PATH", string.Empty);
            }

            var env = new Dictionary<string, string>
                        { 
                            { "PATH", path }, 
                            { "JAVA_HOME", javahome }, 
                            { "SONAR_RUNNER_HOME", sonarrunnerhome }
                        };

            return env;
        }

        #endregion
    }
}