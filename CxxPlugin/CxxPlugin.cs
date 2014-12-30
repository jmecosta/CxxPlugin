// --------------------------------------------------------------------------------------------------------------------
// <copyright file="CxxPlugin.cs" company="Copyright © 2014 jmecsoftware">
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

    using ExtensionTypes;

    using VSSonarPlugins;

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
        private readonly IPluginsOptions pluginOptions = new CxxOptionsController();

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

            this.Desc = new PluginDescription
            {
                Description = "Cxx OpenSource Plugin",
                Name = "CxxPlugin",
                SupportedExtensions = "cpp,cc,hpp,h,h,c",
                Version = this.GetVersion(),
                AssemblyPath = this.GetAssemblyPath()
            };
        }

        #endregion

        #region Public Methods and Operators

        /// <summary>
        /// The is supported.
        /// </summary>
        /// <param name="resource">
        /// The resource.
        /// </param>
        /// <returns>
        /// The <see cref="bool"/>.
        /// </returns>
        public static bool IsSupported(string resource)
        {
            if (resource.EndsWith(".cpp", true, CultureInfo.CurrentCulture) || resource.EndsWith(".cc", true, CultureInfo.CurrentCulture)
                || resource.EndsWith(".c", true, CultureInfo.CurrentCulture) || resource.EndsWith(".h", true, CultureInfo.CurrentCulture)
                || resource.EndsWith(".hpp", true, CultureInfo.CurrentCulture))
            {
                return true;
            }

            return false;
        }

        /// <summary>
        /// The is supported.
        /// </summary>
        /// <param name="resource">
        /// The resource.
        /// </param>
        /// <returns>
        /// The <see cref="bool"/>.
        /// </returns>
        public static bool IsSupported(Resource resource)
        {
            return resource != null && resource.Lang.Equals("c++");
        }

        /// <summary>
        /// The write log message.
        /// </summary>
        /// <param name="e">
        /// The e.
        /// </param>
        /// <param name="handler">
        /// The handler.
        /// </param>
        /// <param name="message">
        /// The message.
        /// </param>
        public static void WriteLogMessage(object e, EventHandler handler, string message)
        {
            Dispatcher dispatcher = Dispatcher.CurrentDispatcher;

            dispatcher.Invoke(
                () =>
                    {
                        EventHandler tempEvent = handler;
                        if (tempEvent != null)
                        {
                            tempEvent(e, new LocalAnalysisEventArgs(Key, message, null));
                        }
                        else
                        {
                            lock (LockThatLog)
                            {
                                using (StreamWriter w = File.AppendText(LogPath))
                                {
                                    var op = w as TextWriter;
                                    op.WriteLine(DateTime.Now.ToString(CultureInfo.InvariantCulture) + " : " + message);
                                }
                            }
                        }
                    });
        }

        /// <summary>
        /// The generate token id.
        /// </summary>
        /// <param name="configuration">
        /// The configuration.
        /// </param>
        /// <returns>
        /// The <see cref="string"/>.
        /// </returns>
        public string GenerateTokenId(ISonarConfiguration configuration)
        {
            return string.Empty;
        }

        /// <summary>
        /// The get assembly path.
        /// </summary>
        /// <returns>
        /// The <see cref="string"/>.
        /// </returns>
        public string GetAssemblyPath()
        {
            return Assembly.GetExecutingAssembly().Location;
        }

        /// <summary>
        /// The get key.
        /// </summary>
        /// <param name="configuration">
        /// The configuration.
        /// </param>
        /// <returns>
        /// The <see cref="string"/>.
        /// </returns>
        public string GetKey(ISonarConfiguration configuration)
        {
            return Key;
        }

        /// <summary>
        /// The get language key.
        /// </summary>
        /// <returns>
        /// The <see cref="string"/>.
        /// </returns>
        public string GetLanguageKey()
        {
            return "c++";
        }

        /// <summary>
        /// The get licenses.
        /// </summary>
        /// <param name="configuration">
        /// The configuration.
        /// </param>
        /// <returns>
        /// The
        ///     <see>
        ///         <cref>Dictionary</cref>
        ///     </see>
        ///     .
        /// </returns>
        public Dictionary<string, VsLicense> GetLicenses(ISonarConfiguration configuration)
        {
            return new Dictionary<string, VsLicense>();
        }

        /// <summary>
        /// The get local analysis extension.
        /// </summary>
        /// <param name="configuration">
        /// The configuration.
        /// </param>
        /// <returns>
        /// The <see cref="ILocalAnalyserExtension"/>.
        /// </returns>
        public ILocalAnalyserExtension GetLocalAnalysisExtension(ISonarConfiguration configuration)
        {
            return new CxxLocalExtension(this as IAnalysisPlugin, configuration);
        }

        /// <summary>
        /// The get plugin control options.
        /// </summary>
        /// <param name="configuration">
        /// The configuration.
        /// </param>
        /// <returns>
        /// The <see cref="IPluginsOptions"/>.
        /// </returns>
        public IPluginsOptions GetPluginControlOptions(ISonarConfiguration configuration)
        {
            return this.pluginOptions;
        }

        /// <summary>
        /// The get plugin description.
        /// </summary>
        /// <param name="vsinter">
        /// The vsinter.
        /// </param>
        /// <returns>
        /// The <see cref="PluginDescription"/>.
        /// </returns>
        public PluginDescription GetPluginDescription()
        {
            return this.Desc;
        }

        /// <summary>
        /// The get resource key.
        /// </summary>
        /// <param name="projectItem">
        /// The project item.
        /// </param>
        /// <param name="projectKey">
        /// The project key.
        /// </param>
        /// <param name="safeGeneration">
        /// The safe generation.
        /// </param>
        /// <returns>
        /// The <see cref="string"/>.
        /// </returns>
        public string GetResourceKey(VsProjectItem projectItem, string projectKey, bool safeGeneration)
        {
            if (safeGeneration && projectItem.ProjectName != null)
            {
                string filePath = projectItem.FilePath.Replace("\\", "/");
                string path = Directory.GetParent(projectItem.ProjectFilePath).ToString().Replace("\\", "/");
                string file = filePath.Replace(path + "/", string.Empty);
                return projectKey + ":" + projectItem.ProjectName + ":" + file;
            }

            string filerelativePath = projectItem.FilePath.Replace(projectItem.SolutionPath + "\\", string.Empty).Replace("\\", "/");
            var options = (CxxOptionsController)this.pluginOptions;
            if (string.IsNullOrEmpty(options.ProjectWorkingDir))
            {
                return projectKey + ":" + filerelativePath.Trim();
            }

            string toReplace = options.ProjectWorkingDir.Replace("\\", "/") + "/";
            return projectKey + ":" + filerelativePath.Replace(toReplace, string.Empty).Trim();
        }

        /// <summary>
        /// The get version.
        /// </summary>
        /// <returns>
        /// The <see cref="string"/>.
        /// </returns>
        public string GetVersion()
        {
            return Assembly.GetExecutingAssembly().GetName().Version.ToString();
        }

        /// <summary>
        /// The is supported.
        /// </summary>
        /// <param name="configuration">
        /// The configuration.
        /// </param>
        /// <param name="resource">
        /// The resource.
        /// </param>
        /// <returns>
        /// The <see cref="bool"/>.
        /// </returns>
        public bool IsSupported(ISonarConfiguration configuration, Resource resource)
        {
            return resource != null && resource.Lang.Equals("c++");
        }

        /// <summary>
        /// The is supported.
        /// </summary>
        /// <param name="fileToAnalyse">
        /// The file to analyse.
        /// </param>
        /// <returns>
        /// The <see cref="bool"/>.
        /// </returns>
        public bool IsSupported(VsProjectItem fileToAnalyse)
        {
            return IsSupported(fileToAnalyse.FileName);
        }

        #endregion

        public PluginDescription Desc { get; set; }
    }
}