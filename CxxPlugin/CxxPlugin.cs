// --------------------------------------------------------------------------------------------------------------------
// <copyright file="CxxPlugin.cs" company="Copyright © 2014 jmecsoftware">
//   Copyright (C) 2014 [jmecsoftware, jmecsoftware2014@tekla.com]
// </copyright>
// <summary>
//   The cpp plugin.
// </summary>
// --------------------------------------------------------------------------------------------------------------------
namespace CxxPlugin
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel.Composition;
    using System.Globalization;
    using System.IO;
    using System.Reflection;
    using System.Windows.Threading;

    using global::CxxPlugin.LocalExtensions;
    using global::CxxPlugin.Options;

    using SonarRestService;

    using VSSonarPlugins;
    using VSSonarPlugins.Types;

    /// <summary>
    ///     The cpp plugin.
    /// </summary>
    [Export(typeof(IPlugin))]
    public class CxxPlugin : IAnalysisPlugin
    {
        #region Static Fields

        /// <summary>
        ///     The key.
        /// </summary>
        public static readonly string Key = "CxxPlugin";

        /// <summary>
        ///     The lock that log.
        /// </summary>
        private static readonly object LockThatLog = new object();

        /// <summary>
        ///     The path.
        /// </summary>
        private static readonly string LogPath = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData)
                                                 + "\\VSSonarExtension\\CxxPlugin.log";

        #endregion

        #region Fields

        /// <summary>
        ///     The plugin options.
        /// </summary>
        private readonly IPluginControlOption pluginOptions;

        /// <summary>The notification manager.</summary>
        private readonly INotificationManager notificationManager;

        /// <summary>The configuration helper.</summary>
        private readonly IConfigurationHelper configurationHelper;

        /// <summary>The file analysis extension.</summary>
        private readonly IFileAnalyser fileAnalysisExtension;

        /// <summary>The rest service.</summary>
        private readonly ISonarRestService restService;

        /// <summary>The desc.</summary>
        private readonly PluginDescription desc;

        #endregion

        #region Constructors and Destructors

        /// <summary>
        ///     Initializes a new instance of the <see cref="CxxPlugin" /> class.
        /// </summary>
        public CxxPlugin()
        {
            if (File.Exists(LogPath))
            {
                File.Delete(LogPath);
            }

            this.desc = new PluginDescription
                            {
                                Description = "Cxx OpenSource Plugin", 
                                Name = "CxxPlugin", 
                                SupportedExtensions = "cpp,cc,hpp,h,h,c", 
                                Version = this.GetVersion(), 
                                AssemblyPath = this.GetAssemblyPath()
                            };
        }

        /// <summary>Initializes a new instance of the <see cref="CxxPlugin"/> class.</summary>
        /// <param name="notificationManager">The notification manager.</param>
        /// <param name="configurationHelper">The configuration helper.</param>
        /// <param name="service">The service.</param>
        public CxxPlugin(
            INotificationManager notificationManager, 
            IConfigurationHelper configurationHelper, 
            ISonarRestService service)
        {
            this.pluginOptions = new CxxOptionsController(configurationHelper);

            if (File.Exists(LogPath))
            {
                File.Delete(LogPath);
            }

            this.desc = new PluginDescription
                            {
                                Description = "Cxx OpenSource Plugin", 
                                Name = "CxxPlugin", 
                                SupportedExtensions = "cpp,cc,hpp,h,h,c", 
                                Version = this.GetVersion(), 
                                AssemblyPath = this.GetAssemblyPath()
                            };

            this.notificationManager = notificationManager;
            this.configurationHelper = configurationHelper;
            this.restService = service;

            this.fileAnalysisExtension = new CxxLocalExtension(
                this, 
                this.notificationManager, 
                this.configurationHelper, 
                this.restService);
        }

        #endregion

        #region Public Methods and Operators

        /// <summary>The is supported.</summary>
        /// <param name="resource">The resource.</param>
        /// <returns>The <see cref="bool"/>.</returns>
        public static bool IsSupported(string resource)
        {
            if (resource.EndsWith(".cpp", true, CultureInfo.CurrentCulture)
                || resource.EndsWith(".cc", true, CultureInfo.CurrentCulture)
                || resource.EndsWith(".c", true, CultureInfo.CurrentCulture)
                || resource.EndsWith(".h", true, CultureInfo.CurrentCulture)
                || resource.EndsWith(".hpp", true, CultureInfo.CurrentCulture))
            {
                return true;
            }

            return false;
        }

        /// <summary>The is supported.</summary>
        /// <param name="resource">The resource.</param>
        /// <returns>The <see cref="bool"/>.</returns>
        public static bool IsSupported(Resource resource)
        {
            return resource != null && resource.Lang.Equals("c++");
        }

        /// <summary>The write log message.</summary>
        /// <param name="e">The e.</param>
        /// <param name="handler">The handler.</param>
        /// <param name="message">The message.</param>
        public static void WriteLogMessage(object e, EventHandler handler, string message)
        {
            var dispatcher = Dispatcher.CurrentDispatcher;

            dispatcher.Invoke(
                () =>
                    {
                        var tempEvent = handler;
                        if (tempEvent != null)
                        {
                            tempEvent(e, new LocalAnalysisEventArgs(Key, message, null));
                        }
                        else
                        {
                            lock (LockThatLog)
                            {
                                using (var w = File.AppendText(LogPath))
                                {
                                    var op = w as TextWriter;
                                    op.WriteLine(DateTime.Now.ToString(CultureInfo.InvariantCulture) + " : " + message);
                                }
                            }
                        }
                    });
        }

        /// <summary>The generate token id.</summary>
        /// <param name="configuration">The configuration.</param>
        /// <returns>The <see cref="string"/>.</returns>
        public string GenerateTokenId(ISonarConfiguration configuration)
        {
            return string.Empty;
        }

        /// <summary>
        ///     The get assembly path.
        /// </summary>
        /// <returns>
        ///     The <see cref="string" />.
        /// </returns>
        public string GetAssemblyPath()
        {
            return Assembly.GetExecutingAssembly().Location;
        }

        /// <summary>The get key.</summary>
        /// <param name="configuration">The configuration.</param>
        /// <returns>The <see cref="string"/>.</returns>
        public string GetKey(ISonarConfiguration configuration)
        {
            return Key;
        }

        /// <summary>The get language key.</summary>
        /// <param name="projectItem">The project Item.</param>
        /// <returns>The <see cref="string"/>.</returns>
        public string GetLanguageKey(VsProjectItem projectItem)
        {
            return "c++";
        }

        /// <summary>The get licenses.</summary>
        /// <param name="configuration">The configuration.</param>
        /// <returns>The<see>
        ///         <cref>Dictionary</cref>
        ///     </see>
        ///     .</returns>
        public Dictionary<string, VsLicense> GetLicenses(ISonarConfiguration configuration)
        {
            return new Dictionary<string, VsLicense>();
        }

        /// <summary>The get local analysis extension.</summary>
        /// <param name="configuration">The configuration.</param>
        /// <returns>The <see cref="ILocalAnalyserExtension"/>.</returns>
        public IFileAnalyser GetLocalAnalysisExtension(ISonarConfiguration configuration)
        {
            return this.fileAnalysisExtension;
        }

        /// <summary>The get plugin control options.</summary>
        /// <param name="project">The project.</param>
        /// <param name="configuration">The configuration.</param>
        /// <returns>The <see cref="IPluginsOptions"/>.</returns>
        public IPluginControlOption GetPluginControlOptions(Resource project, ISonarConfiguration configuration)
        {
            return this.pluginOptions;
        }

        /// <summary>
        ///     The get plugin description.
        /// </summary>
        /// <returns>
        ///     The <see cref="PluginDescription" />.
        /// </returns>
        public PluginDescription GetPluginDescription()
        {
            return this.desc;
        }

        /// <summary>The get resource key.</summary>
        /// <param name="projectItem">The project item.</param>
        /// <param name="projectKey">The project key.</param>
        /// <param name="safeGeneration">The safe generation.</param>
        /// <returns>The <see cref="string"/>.</returns>
        public string GetResourceKey(VsProjectItem projectItem, string projectKey, bool safeGeneration)
        {
            if (safeGeneration && projectItem.ProjectName != null)
            {
                var filePath = projectItem.FilePath.Replace("\\", "/");
                var path = Directory.GetParent(projectItem.ProjectFilePath).ToString().Replace("\\", "/");
                var file = filePath.Replace(path + "/", string.Empty);
                return projectKey + ":" + projectItem.ProjectName + ":" + file;
            }

            var filerelativePath =
                projectItem.FilePath.Replace(projectItem.SolutionPath + "\\", string.Empty).Replace("\\", "/");
            var options = (CxxOptionsController)this.pluginOptions;
            if (string.IsNullOrEmpty(options.ProjectWorkingDir))
            {
                return projectKey + ":" + filerelativePath.Trim();
            }

            var toReplace = options.ProjectWorkingDir.Replace("\\", "/") + "/";
            return projectKey + ":" + filerelativePath.Replace(toReplace, string.Empty).Trim();
        }

        /// <summary>
        ///     The get version.
        /// </summary>
        /// <returns>
        ///     The <see cref="string" />.
        /// </returns>
        public string GetVersion()
        {
            return Assembly.GetExecutingAssembly().GetName().Version.ToString();
        }

        /// <summary>The is supported.</summary>
        /// <param name="configuration">The configuration.</param>
        /// <param name="resource">The resource.</param>
        /// <returns>The <see cref="bool"/>.</returns>
        public bool IsSupported(ISonarConfiguration configuration, Resource resource)
        {
            return resource != null && resource.Lang.Equals("c++");
        }

        /// <summary>The is supported.</summary>
        /// <param name="fileToAnalyse">The file to analyse.</param>
        /// <returns>The <see cref="bool"/>.</returns>
        public bool IsSupported(VsProjectItem fileToAnalyse)
        {
            return IsSupported(fileToAnalyse.FileName);
        }

        /// <summary>The associate project.</summary>
        /// <param name="project">The project.</param>
        /// <param name="configuration">The configuration.</param>
        public void AssociateProject(Resource project, ISonarConfiguration configuration)
        {
        }

        /// <summary>The reset defaults.</summary>
        public void ResetDefaults()
        {
        }

        #endregion
    }
}