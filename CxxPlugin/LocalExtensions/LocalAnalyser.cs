// --------------------------------------------------------------------------------------------------------------------
// <copyright file="LocalAnalyser.cs" company="">
//   
// </copyright>
// <summary>
//   Defines the LocalAnalyser type.
// </summary>
// --------------------------------------------------------------------------------------------------------------------

using System.Security.Permissions;
using System.Threading;
using ExtensionHelpers;
using Microsoft.TeamFoundation.Controls.WinForms;


namespace CxxPlugin.LocalExtensions
{
    using System;
    using System.Collections.Generic;
    using System.Diagnostics;
    using System.IO;
    using ExtensionTypes;
    using VSSonarPlugins;
    using SonarRestService;

    /// <summary>
    /// The local analyser.
    /// </summary>
    [HostProtection(SecurityAction.LinkDemand, Synchronization = true,
        ExternalThreading = true)]
    public class LocalAnalyser
    {
        /// <summary>
        /// The sensors.
        /// </summary>
        private readonly Dictionary<string, ASensor> sensors;

        /// <summary>
        /// The lock this.
        /// </summary>
        private readonly object lockThis = new object();

        private readonly IPlugin plugin;
        private Resource project;
        private string fileBeingAnalysed;
        private AnalysisMode mode;
        private string solutionPath;
        private readonly Profile profile;
        private readonly object lockNumberOfFileAnalysers = new object();
        private readonly ISonarRestService restService = new SonarRestService(new JsonSonarConnector());
        private int numberofFilesToAnalyse;
        private List<Issue> issuesInFile;
        private ConnectionConfiguration configuration;
        private Dictionary<string, string> options;

        /// <summary>
        /// Initializes a new instance of the <see cref="LocalAnalyser"/> class.
        /// </summary>
        /// <param name="sensors">
        ///     The sensors.
        /// </param>
        /// <param name="plugin">
        ///     The plugin.
        /// </param>
        /// <param name="profile"></param>
        public LocalAnalyser(Dictionary<string, ASensor> sensors, IPlugin plugin, Profile profile)
        {
            this.profile = profile;
            this.plugin = plugin;
            this.sensors = sensors;
        }

        /// <summary>
        /// Gets or sets the loghandler.
        /// </summary>
        public EventHandler Loghandler { get; set; }

        /// <summary>
        /// Gets or sets the issues.
        /// </summary>
        public List<Issue> Issues { get; set; }

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
        /// <param name="project">
        /// The project.
        /// </param>
        /// <param name="conf">
        /// The conf.
        /// </param>
        /// <param name="sonarVersion">
        /// The sonar Version.
        /// </param>
        public void RunSonarRunner(AnalysisMode modeIn, ICommandExecution executor, IPluginsOptions pluginOptions, EventHandler loghandler, string rootPath, Resource project, ConnectionConfiguration conf, double sonarVersion)
        {
            this.options = pluginOptions.GetOptions();
            var environment = this.SetupSonarRunnerEnvironment(this.options, project);
            var command = this.options[project.Key + ".SonarRunnerPath"];
            this.Issues = new List<Issue>();
            this.Loghandler = loghandler;
            this.project = project;
            this.mode = modeIn;
            this.solutionPath = rootPath;
            this.numberofFilesToAnalyse = 0;
            this.configuration = conf;
            CxxPlugin.WriteLogMessage(this, loghandler, "Sonar Runner Started: sonarrunner.bat " + this.GetArguments(this.options, project, mode, conf, sonarVersion).Replace(conf.Password, "xxxx"));
            executor.ExecuteCommand(rootPath, command, this.GetArguments(this.options, project, this.mode, conf, sonarVersion), environment, this.ProcessOutputDataReceived, this.ProcessOutputDataReceived, null);

            this.ProcessRemainingIssuesFromSensors();
        }

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
        /// <param name="project">
        /// The project.
        /// </param>
        /// <param name="conf">
        /// The conf.
        /// </param>
        /// <param name="sonarVersion">
        /// The sonar Version.
        /// </param>
        public void RunMavenRunner(AnalysisMode modeIn, ICommandExecution executor, IPluginsOptions pluginOptions, EventHandler loghandler, string rootPath, Resource project, ConnectionConfiguration conf, double sonarVersion)
        {
            this.Issues = new List<Issue>();
            this.options = pluginOptions.GetOptions();
            var environment = this.SetupMavenEnvironment(this.options, project);
            var command = this.options[project.Key + ".MavenPath"];
            this.Loghandler = loghandler;
            this.project = project;
            this.mode = modeIn;
            this.solutionPath = rootPath;
            this.numberofFilesToAnalyse = 0;
            this.configuration = conf;
            CxxPlugin.WriteLogMessage(this, loghandler, "Sonar Runner Started: sonarrunner.bat ");
            executor.ExecuteCommand(rootPath, command, this.GetArguments(this.options, project, this.mode, conf, sonarVersion), environment, this.ProcessOutputDataReceived, this.ProcessOutputDataReceived, null);

            this.ProcessRemainingIssuesFromSensors();
        }

        /// <summary>
        /// The sync file analyser.
        /// </summary>
        private void ProcessRemainingIssuesFromSensors()
        {
            while (this.FileAnalyserNumber() > 0)
            {
                Thread.Sleep(1000);
            }
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
        ///     <cref>Exception</cref>
        /// </exception>
        private static string GetDoubleParent(string path)
        {
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
        ///     <cref>Exception</cref>
        /// </exception>
        private string GetParent(string path)
        {
            var data = Directory.GetParent(path).ToString();
            if (!Directory.Exists(data))
            {
                throw new Exception("Directory Not Found: " + path);
            }

            return data;
        }

        /// <summary>
        /// The setup sonar runner environment.
        /// </summary>
        /// <param name="options">
        /// The options.
        /// </param>
        /// <param name="project">
        /// The project.
        /// </param>
        /// <returns>
        /// The <see>
        ///         <cref>Dictionary</cref>
        ///     </see>
        ///     .
        /// </returns>
        private Dictionary<string, string> SetupSonarRunnerEnvironment(Dictionary<string, string> options, Resource project)
        {
            var javahome = GetDoubleParent(options["JavaBinaryPath"]);
            var sonarrunnerhome = GetDoubleParent(options[project.Key + ".SonarRunnerPath"]);
            var path = this.GetParent(options["JavaBinaryPath"]) +
                       ";" + this.GetParent(options[project.Key + ".SonarRunnerPath"]) +
                       ";" + Environment.GetEnvironmentVariable("PATH");

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

            var env = new Dictionary<string, string> { { "PATH", path }, { "JAVA_HOME", javahome }, { "SONAR_RUNNER_HOME", sonarrunnerhome } };

            return env;
        }

        /// <summary>
        /// The setup maven environment.
        /// </summary>
        /// <param name="options">
        /// The options.
        /// </param>
        /// <param name="project">
        /// The project.
        /// </param>
        /// <returns>
        /// The <see>
        ///         <cref>Dictionary</cref>
        ///     </see>
        ///     .
        /// </returns>
        private Dictionary<string, string> SetupMavenEnvironment(Dictionary<string, string> options, Resource project)
        {
            var javahome = GetDoubleParent(options["JavaBinaryPath"]);
            var mavenhome = GetDoubleParent(options[project.Key + ".MavenPath"]);
            var path = this.GetParent(options["JavaBinaryPath"]) +
                       ";" + this.GetParent(options[project.Key + ".MavenPath"]) +
                       ";" + Environment.GetEnvironmentVariable("PATH");

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
        /// The get arguments.
        /// </summary>
        /// <param name="options">
        /// The options.
        /// </param>
        /// <param name="project">
        /// The project.
        /// </param>
        /// <param name="mode">
        /// The mode.
        /// </param>
        /// <param name="conf">
        /// The conf.
        /// </param>
        /// <param name="sonarVersion">
        /// The sonar version.
        /// </param>
        /// <returns>
        /// The <see cref="string"/>.
        /// </returns>
        /// <exception>
        ///     <cref>Exception</cref>
        /// </exception>
        private string GetArguments(Dictionary<string, string> options, Resource project, AnalysisMode mode, ConnectionConfiguration conf, double sonarVersion)
        {
            var args = "sonar:sonar -X -Dsonar.login=" + conf.Username + " -Dsonar.password=" + conf.Password;

            if (sonarVersion >= 4.0)
            {
                switch (mode)
                {
                    case AnalysisMode.Incremental:
                        args += " -Dsonar.analysis.mode=incremental";
                        args += " -Dsonar.preview.excludePlugins=" + options[project.Key + ".ExcludedPlugins"];
                    break;
                    case AnalysisMode.Full:
                    break;
                    case AnalysisMode.Preview:
                        args += " -Dsonar.analysis.mode=preview";
                        args += " -Dsonar.preview.excludePlugins=" + options[project.Key + ".ExcludedPlugins"];
                    break;
                }
            }
            else
            {
                if (sonarVersion >= 3.4)
                {
                    switch (mode)
                    {
                        case AnalysisMode.Incremental:
                            throw new Exception("Analysis Method Not Available in this Version of SonarQube");
                        case AnalysisMode.Full:
                            break;
                        case AnalysisMode.Preview:
                            args += " -Dsonar.dryRun.excludePlugins=" + options[project.Key + ".ExcludedPlugins"];
                            args += " -Dsonar.dryRun=true";
                            break;
                    }
                }
                else
                {
                    switch (mode)
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
            
            foreach (var option in options)
            {
                if (option.Key.StartsWith(project.Key + ".propertyToRunner."))
                {
                    args += " -D" + option.Key.Replace(project.Key + ".propertyToRunner.", string.Empty) + "=" + option.Value;
                }
            }

            return args;
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
                var fileName = data.Split(separator, StringSplitOptions.RemoveEmptyEntries)[1];
                if (File.Exists(fileName))
                {
                    this.IncrementFileAnalyser();
                    this.ExecuteAnalysisOnFile(fileName);
                    this.DecrementFileAnalyser();
                }
            }
        }

        private void DecrementFileAnalyser()
        {
            lock (this.lockNumberOfFileAnalysers)
            {
                this.numberofFilesToAnalyse += 1;
            }
        }

        private int FileAnalyserNumber()
        {
            lock (this.lockNumberOfFileAnalysers)
            {
                return numberofFilesToAnalyse;
            }
        }

        private void IncrementFileAnalyser()
        {
            lock (this.lockNumberOfFileAnalysers)
            {
                this.numberofFilesToAnalyse -= 1;
            }
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
            try
            {
                var issuesInTool = this.sensors[key].GetViolations(sensorReportedLines);
                foreach (var issue in issuesInTool)
                {
                    var ruleInProfile = Profile.IsRuleEnabled(this.profile, issue.Rule);
                    if (ruleInProfile != null)
                    {
                        issue.Severity = ruleInProfile.Severity;
                        var projecItem = new VsProjectItem(string.Empty, this.fileBeingAnalysed, string.Empty, string.Empty, string.Empty, this.solutionPath);
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
                        var projecItem = new VsProjectItem(string.Empty, this.fileBeingAnalysed, string.Empty, string.Empty, string.Empty, this.solutionPath);
                        var elemKey = this.plugin.GetResourceKey(projecItem, this.project.Key);

                        var source = this.restService.GetSourceForFileResource(this.configuration, elemKey);
                        if (source != null)
                        {
                            var sourcestr = VsSonarUtils.GetLinesFromSource(source, "\r\n");
                            var diffReport = VsSonarUtils.GetDifferenceReport(this.fileBeingAnalysed, sourcestr, false);
                            this.Issues.AddRange(VsSonarUtils.GetIssuesInModifiedLinesOnly(this.issuesInFile, diffReport));
                        }
                        else
                        {
                            this.Issues.AddRange(this.issuesInFile);
                        }
                    }
                    catch (Exception ex)
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
                if (this.options.ContainsKey(this.project.Key + ".IsDebugChecked") &&
                    this.options[this.project.Key + ".IsDebugChecked"].Equals("true"))
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
    }
}
