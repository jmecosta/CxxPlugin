// --------------------------------------------------------------------------------------------------------------------
// <copyright file="ASensor.cs" company="Copyright © 2014 jmecsoftware">
//   Copyright (C) 2014 [jmecsoftware, jmecsoftware2014@tekla.com]
// </copyright>
// <summary>
//   The Sensor interface.
// </summary>
// --------------------------------------------------------------------------------------------------------------------
namespace CxxPlugin.LocalExtensions
{
    using System;
    using System.Collections.Generic;
    using System.Diagnostics;
    using System.IO;

    using Microsoft.FSharp.Collections;

    using SonarRestService;

    using VSSonarPlugins;
    using VSSonarPlugins.Types;

    using VSSonarQubeCmdExecutor;

    /// <summary>
    ///     The Sensor interface.
    /// </summary>
    public abstract class ASensor
    {
        /// <summary>
        ///     The command line error.
        /// </summary>
        private readonly List<string> commandLineError = new List<string>();

        /// <summary>
        ///     The CommandLineOuput.
        /// </summary>
        private readonly List<string> commandLineOuput = new List<string>();

        /// <summary>
        ///     The repository key.
        /// </summary>
        protected readonly string RepositoryKey;

        /// <summary>
        ///     The use stdout.
        /// </summary>
        protected readonly bool UseStdout;

        /// <summary>
        /// The configuration helper.
        /// </summary>
        protected IConfigurationHelper configurationHelper;

        /// <summary>
        /// The notification manager.
        /// </summary>
        protected INotificationManager notificationManager;

        /// <summary>
        /// The sonar rest service.
        /// </summary>
        protected ISonarRestService sonarRestService;

        /// <summary>
        /// Initializes a new instance of the <see cref="ASensor"/> class.
        /// </summary>
        /// <param name="repositoryKey">
        /// The repository key.
        /// </param>
        /// <param name="useStdout">
        /// The use stdout.
        /// </param>
        /// <param name="notificationManager">
        /// The notification manager.
        /// </param>
        /// <param name="configurationHelper">
        /// The configuration helper.
        /// </param>
        /// <param name="sonarRestService">
        /// The sonar rest service.
        /// </param>
        protected ASensor(
            string repositoryKey, 
            bool useStdout, 
            INotificationManager notificationManager, 
            IConfigurationHelper configurationHelper, 
            ISonarRestService sonarRestService)
        {
            this.notificationManager = notificationManager;
            this.configurationHelper = configurationHelper;
            this.sonarRestService = sonarRestService;
            this.RepositoryKey = repositoryKey;
            this.UseStdout = useStdout;
        }

        /// <summary>
        /// The launch sensor.
        /// </summary>
        /// <param name="caller">
        /// The caller.
        /// </param>
        /// <param name="logger">
        /// The logger.
        /// </param>
        /// <param name="filePath">
        /// The file Path.
        /// </param>
        /// <param name="executor">
        /// </param>
        /// <returns>
        /// The <see cref="Process"/>.
        /// </returns>
        public virtual FSharpList<string> LaunchSensor(
            object caller, 
            EventHandler logger, 
            string filePath, 
            IVSSonarQubeCmdExecutor executor)
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
        ///     The get key.
        /// </summary>
        /// <returns>
        ///     The <see cref="string" />.
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
        public abstract List<Issue> GetViolations(FSharpList<string> lines);

        /// <summary>
        ///     The get environment.
        /// </summary>
        /// <returns>
        ///     The
        ///     <see>
        ///         <cref>Dictionary</cref>
        ///     </see>
        ///     .
        /// </returns>
        public abstract FSharpMap<string, string> GetEnvironment();

        /// <summary>
        ///     The get command.
        /// </summary>
        /// <returns>
        ///     The <see cref="string" />.
        /// </returns>
        public abstract string GetCommand();

        /// <summary>
        ///     The get arguments.
        /// </summary>
        /// <returns>
        ///     The <see cref="string" />.
        /// </returns>
        public abstract string GetArguments();

        /// <summary>
        /// The read get property.
        /// </summary>
        /// <param name="key">
        /// The key.
        /// </param>
        /// <returns>
        /// The <see cref="string"/>.
        /// </returns>
        protected string ReadGetProperty(string key)
        {
            try
            {
                return
                    this.configurationHelper.ReadSetting(
                        Context.FileAnalysisProperties, 
                        OwnersId.PluginGeneralOwnerId, 
                        key).Value;
            }
            catch (Exception)
            {
                return string.Empty;
            }
        }

        /// <summary>
        /// The write property.
        /// </summary>
        /// <param name="key">
        /// The key.
        /// </param>
        /// <param name="value">
        /// The value.
        /// </param>
        /// <param name="sync">
        /// The sync.
        /// </param>
        /// <param name="skipIfFound">
        /// The skip if found.
        /// </param>
        protected void WriteProperty(string key, string value, bool sync = false, bool skipIfFound = false)
        {
            this.configurationHelper.WriteOptionInApplicationData(
                Context.FileAnalysisProperties, 
                OwnersId.PluginGeneralOwnerId, 
                key, 
                value, 
                sync, 
                skipIfFound);
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
        /// The convert cs map to f sharp map.
        /// </summary>
        /// <param name="data">
        /// The data.
        /// </param>
        /// <returns>
        /// The <see cref="FSharpMap"/>.
        /// </returns>
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