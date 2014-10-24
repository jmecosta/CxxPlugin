// --------------------------------------------------------------------------------------------------------------------
// <copyright file="CxxLocalExtension.cs" company="">
//   
// </copyright>
// <summary>
//   The cxx server extension.
// </summary>
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

    using ExtensionHelpers;

    using ExtensionTypes;

    using Microsoft.FSharp.Collections;

    using MSBuild.Tekla.Tasks.Executor;

    using VSSonarPlugins;

    /// <summary>
    ///     The cxx server extension.
    /// </summary>
    [ComVisible(false)]
    [HostProtection(SecurityAction.LinkDemand, Synchronization = true, ExternalThreading = true)]
    public class CxxLocalExtension : WaitHandle, ILocalAnalyserExtension
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
        ///     The plugin options.
        /// </summary>
        private readonly IPluginsOptions pluginOptions;

        /// <summary>
        ///     The sensors.
        /// </summary>
        private readonly Dictionary<string, ASensor> sensors;

        /// <summary>
        ///     The file path to analyse.
        /// </summary>
        private string filePathToAnalyse;

        /// <summary>
        ///     The modified lines only.
        /// </summary>
        private bool modifiedLinesOnly;

        /// <summary>
        ///     The options.
        /// </summary>
        private Dictionary<string, string> options;

        /// <summary>
        ///     The profileIn.
        /// </summary>
        private Profile profile;

        /// <summary>
        ///     The project item.
        /// </summary>
        private VsProjectItem projectItem;

        /// <summary>
        ///     The source in server.
        /// </summary>
        private string sourceInServer;

        /// <summary>
        /// The project.
        /// </summary>
        private Resource project;

        #endregion

        #region Constructors and Destructors

        /// <summary>
        /// Initializes a new instance of the <see cref="CxxLocalExtension"/> class.
        /// </summary>
        /// <param name="commandPlugin">
        /// The command plugin.
        /// </param>
        /// <param name="connectionConfiguration">
        /// The connection Configuration.
        /// </param>
        public CxxLocalExtension(IAnalysisPlugin commandPlugin, ISonarConfiguration connectionConfiguration)
        {
            this.commandPlugin = commandPlugin;
            this.pluginOptions = commandPlugin.GetPluginControlOptions(connectionConfiguration);
            this.options = this.pluginOptions.GetOptions();

            this.issues = new List<Issue>();
            this.sensors = new Dictionary<string, ASensor>
                               {
                                   {
                                       CppCheckSensor.SKey, 
                                       new CppCheckSensor(this.pluginOptions)
                                   }, 
                                   { RatsSensor.SKey, new RatsSensor(this.pluginOptions) }, 
                                   { VeraSensor.SKey, new VeraSensor(this.pluginOptions) }, 
                                   {
                                       PcLintSensor.SKey, new PcLintSensor(this.pluginOptions)
                                   }, 
                                   {
                                       CxxExternalSensor.SKey, 
                                       new CxxExternalSensor(this.pluginOptions)
                                   }
                               };
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="CxxLocalExtension"/> class.
        /// </summary>
        /// <param name="cxxPlugin">
        /// The cxx plugin.
        /// </param>
        public CxxLocalExtension(IAnalysisPlugin cxxPlugin)
        {
            this.commandPlugin = cxxPlugin;
            this.pluginOptions = cxxPlugin.GetPluginControlOptions(new ConnectionConfiguration());
            this.options = this.pluginOptions.GetOptions();

            this.issues = new List<Issue>();
            this.sensors = new Dictionary<string, ASensor>
                               {
                                   {
                                       CppCheckSensor.SKey, 
                                       new CppCheckSensor(this.pluginOptions)
                                   }, 
                                   { RatsSensor.SKey, new RatsSensor(this.pluginOptions) }, 
                                   { VeraSensor.SKey, new VeraSensor(this.pluginOptions) }, 
                                   {
                                       PcLintSensor.SKey, new PcLintSensor(this.pluginOptions)
                                   }, 
                                   {
                                       CxxExternalSensor.SKey, 
                                       new CxxExternalSensor(this.pluginOptions)
                                   }
                               };
        }

        #endregion

        #region Public Events

        /// <summary>
        ///     The local analysis completed.
        /// </summary>
        public event EventHandler LocalAnalysisCompleted;

        /// <summary>
        ///     The std err event.
        /// </summary>
        public event EventHandler StdErrEvent;

        /// <summary>
        ///     The std out event.
        /// </summary>
        public event EventHandler StdOutEvent;

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
        public List<Issue> ExecuteAnalysisOnFile(VsProjectItem itemInView, Profile externlProfile, Resource projectIn)
        {
            var threads = new List<Thread>();
            var allIssues = new List<Issue>();
            this.options = this.pluginOptions.GetOptions();

            foreach (var sensor in this.sensors)
            {
                CxxPlugin.WriteLogMessage(
                    this, 
                    this.StdOutEvent, 
                    "Launching  Analysis on: " + sensor.Key + " " + itemInView.FilePath);
                threads.Add(
                    this.RunSensorThread(
                        this.StdOutEvent, 
                        itemInView, 
                        sensor,
                        projectIn, 
                        externlProfile, 
                        false, 
                        string.Empty, 
                        allIssues));
            }

            foreach (Thread thread in threads)
            {
                thread.Join();
            }

            return allIssues;
        }

        /// <summary>
        /// The launch local analysis.
        /// </summary>
        /// <param name="file">
        /// The file.
        /// </param>
        /// <param name="projectIn">
        /// The project In.
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
        public Thread GetFileAnalyserThread(
            VsProjectItem file, 
            Resource projectIn, 
            Profile profileIn, 
            string fileSourceInServer, 
            bool onModifiedLinesOnly)
        {
            this.sourceInServer = fileSourceInServer;
            this.modifiedLinesOnly = onModifiedLinesOnly;
            this.profile = profileIn;
            this.filePathToAnalyse = file.FilePath;
            this.projectItem = file;
            this.project = projectIn;
            return new Thread(this.LocalFileAnalyserExternalThread);
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
        /// The get supported issues.
        /// </summary>
        /// <param name="inputIssues">
        /// The issues.
        /// </param>
        /// <returns>
        /// The <see>
        ///         <cref>List</cref>
        ///     </see>
        ///     .
        /// </returns>
        public List<Issue> GetSupportedIssues(List<Issue> inputIssues)
        {
            return this.GetIssuesFromJsonReports(inputIssues);
        }

        /// <summary>
        /// The get local analysis paramenters.
        /// </summary>
        /// <param name="projectIn">
        /// The project.
        /// </param>
        /// <returns>
        /// The <see cref="ICollection"/>.
        /// </returns>
        public List<SonarQubeProperties> GetLocalAnalysisParamenters(Resource projectIn)
        {
            var collection = new List<SonarQubeProperties>();
            foreach (var option in this.options)
            {
                if (option.Key.StartsWith(projectIn.Key + ".propertyToRunner."))
                {
                    collection.Add(
                        new SonarQubeProperties
                            {
                                Key =
                                    option.Key.Replace(
                                        projectIn.Key + ".propertyToRunner.", 
                                        string.Empty), 
                                Value = option.Value, 
                                ValueInServer = option.Value
                            });
                }
            }

            return collection;
        }

        /// <summary>
        ///     The local File analyser.
        /// </summary>
        public void LocalFileAnalyserExternalThread()
        {
            var threads = new List<Thread>();
            this.issues.Clear();
            this.options = this.pluginOptions.GetOptions();

            foreach (var sensor in this.sensors)
            {
                CxxPlugin.WriteLogMessage(
                    this, 
                    this.StdOutEvent, 
                    "Launching  Analysis on: " + sensor.Key + " " + this.filePathToAnalyse);
                threads.Add(
                    this.RunSensorThread(
                        this.StdOutEvent, 
                        this.projectItem, 
                        sensor, 
                        this.project, 
                        this.profile, 
                        this.modifiedLinesOnly, 
                        this.sourceInServer,
                        this.issues));
            }

            foreach (Thread thread in threads)
            {
                thread.Join();
            }

            EventHandler tempEvent = this.LocalAnalysisCompleted;
            if (tempEvent != null)
            {
                tempEvent(this, new LocalAnalysisEventArgs(CxxPlugin.Key, string.Empty, null));
            }
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
            VsProjectItem file, 
            KeyValuePair<string, ASensor> sensor, 
            Resource projectIn, 
            Profile profileIn, 
            bool changedlines, 
            string sourceRef, 
            List<Issue> issuesToReturn)
        {
            var t =
                new Thread(
                    () =>
                    this.RunSensor(output, file, sensor, projectIn, profileIn, changedlines, sourceRef, issuesToReturn));
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
        /// The get issues from json reports.
        /// </summary>
        /// <param name="issuestoParse">
        /// The issuesto parse.
        /// </param>
        /// <returns>
        /// The <see>
        ///         <cref>List</cref>
        ///     </see>
        ///     .
        /// </returns>
        private List<Issue> GetIssuesFromJsonReports(IEnumerable<Issue> issuestoParse)
        {
            return issuestoParse.Where(this.IsIssueSupported).ToList();
        }

        /// <summary>
        /// The is issue supported.
        /// </summary>
        /// <param name="issue">
        /// The issue.
        /// </param>
        /// <returns>
        /// The <see cref="bool"/>.
        /// </returns>
        private bool IsIssueSupported(Issue issue)
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
            FSharpList<string> sensorReportedLines, 
            VsProjectItem itemInView, 
            Resource projectIn, 
            Profile profileIn, 
            bool modfiedLines, 
            string fileSourceRef, 
            List<Issue> issuesToReturn)
        {
            var issuesPerTool = new List<Issue>();

            if (this.options.ContainsKey(projectIn.Key + ".IsDebugChecked")
                && this.options[projectIn.Key + ".IsDebugChecked"].Equals("true"))
            {
                foreach (string line in sensorReportedLines)
                {
                    CxxPlugin.WriteLogMessage(this, this.StdOutEvent, "[" + key + "] : " + line);
                }
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
                        issue.Component = this.commandPlugin.GetResourceKey(itemInView, projectIn.Key, false);
                        issue.ComponentSafe = this.commandPlugin.GetResourceKey(itemInView, projectIn.Key, true);
                        Rule ruleInProfile = Profile.IsRuleEnabled(profileIn, issue.Rule);
                        if (ruleInProfile != null)
                        {
                            issue.Severity = ruleInProfile.Severity;
                            issuesPerTool.Add(issue);
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                CxxPlugin.WriteLogMessage(this, this.StdOutEvent, "Exception: " + key + " " + ex.StackTrace);
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
            VsProjectItem file, 
            KeyValuePair<string, ASensor> sensor, 
            Resource projectIn, 
            Profile profileIn, 
            bool changedlines, 
            string sourceRef, 
            List<Issue> issuesToReturn)
        {
            ICommandExecutor exec = new CommandExecutor(null, 10);
            try
            {
                FSharpList<string> lines = sensor.Value.LaunchSensor(this, output, file.FilePath, exec);
                this.ProcessSensorsIssues(
                    sensor.Key, 
                    lines, 
                    file, 
                    projectIn, 
                    profileIn, 
                    changedlines, 
                    sourceRef, 
                    issuesToReturn);
            }
            catch (Exception ex)
            {
                CxxPlugin.WriteLogMessage(
                    this, 
                    this.StdOutEvent, 
                    sensor.Key + " : Exception on Analysis Plugin : " + this.filePathToAnalyse + " " + ex.StackTrace);
                CxxPlugin.WriteLogMessage(this, this.StdOutEvent, sensor.Key + " : StdError: " + exec.GetStdError);
                CxxPlugin.WriteLogMessage(this, this.StdOutEvent, sensor.Key + " : StdOut: " + exec.GetStdError);
            }
        }

        #endregion
    }
}