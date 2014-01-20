// --------------------------------------------------------------------------------------------------------------------
// <copyright file="CxxPlugin.cs" company="Copyright © 2013 Tekla Corporation. Tekla is a Trimble Company">
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
namespace CxxPlugin
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel.Composition;
    using System.Globalization;
    using System.IO;
    using System.Windows.Threading;
    using ExtensionTypes;

    using global::CxxPlugin.Commands;

    using LocalExtensions;
    using Options;
    using ServerExtensions;
    using VSSonarPlugins;

    /// <summary>
    /// The cpp plugin.
    /// </summary>
    [Export(typeof(IPlugin))]
    public class CxxPlugin : IPlugin
    {
        /// <summary>
        /// The key.
        /// </summary>
        public static readonly string Key = "CxxPlugin";

        /// <summary>
        /// The lock that log.
        /// </summary>
        private static readonly object LockThatLog = new object();

        /// <summary>
        /// The path.
        /// </summary>
        private static readonly string LogPath = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData) + "\\VSSonarExtension\\CxxPlugin.log";

        /// <summary>
        /// The plugin options.
        /// </summary>
        private readonly IPluginsOptions pluginOptions = new CxxOptionsController();

        /// <summary>
        /// Initializes a new instance of the <see cref="CxxPlugin"/> class.
        /// </summary>
        public CxxPlugin()
        {
            if (File.Exists(LogPath))
            {
                File.Delete(LogPath);
            }
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
            var dispatcher = Dispatcher.CurrentDispatcher;

            dispatcher.Invoke(() =>
            {
                var tempEvent = handler;
                if (tempEvent != null)
                {
                    tempEvent(e, new LocalAnalysisCompletedEventArgs(Key, message, null));
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

        /// <summary>
        /// The get server analyser extensions.
        /// </summary>
        /// <returns>
        /// The <see>
        ///     <cref>List</cref>
        /// </see>
        ///     .
        /// </returns>
        public IServerAnalyserExtension GetServerAnalyserExtension()
        {
            return new CxxServerExtension();
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
        public string GetKey(ConnectionConfiguration configuration)
        {
            return Key;
        }

        /// <summary>
        /// The get plugin control options.
        /// </summary>
        /// <param name="configuration">
        /// The configuration.
        /// </param>
        /// <param name="project">
        /// The project.
        /// </param>
        /// <returns>
        /// The <see cref="IPluginsOptions"/>.
        /// </returns>
        public IPluginsOptions GetPluginControlOptions(ConnectionConfiguration configuration, Resource project)
        {
            ((CxxOptionsController)this.pluginOptions).Project = this.IsSupported(configuration, project) ? project : null;

            return this.pluginOptions;
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
        public bool IsSupported(ConnectionConfiguration configuration, string resource)
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
        public bool IsSupported(ConnectionConfiguration configuration, Resource resource)
        {
            return resource != null && resource.Lang.Equals("c++");
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
        /// <returns>
        /// The <see cref="string"/>.
        /// </returns>
        public string GetResourceKey(VsProjectItem projectItem, string projectKey)
        {
            var filerelativePath = projectItem.FilePath.Replace(projectItem.SolutionPath + "\\", string.Empty).Replace("\\", "/");
            var options = (CxxOptionsController)this.pluginOptions;
            if (string.IsNullOrEmpty(options.ProjectWorkingDir))
            {
                return projectKey + ":" + filerelativePath.Trim();
            }

            var toReplace = options.ProjectWorkingDir.Replace("\\", "/") + "/";
            return projectKey + ":" + filerelativePath.Replace(toReplace, string.Empty).Trim();
        }

        /// <summary>
        /// The get local analysis extension.
        /// </summary>
        /// <param name="configuration">
        /// The configuration.
        /// </param>
        /// <param name="project">
        /// The project.
        /// </param>
        /// <param name="sonarVersion">
        /// The sonar Version.
        /// </param>
        /// <returns>
        /// The <see cref="ILocalAnalyserExtension"/>.
        /// </returns>
        public ILocalAnalyserExtension GetLocalAnalysisExtension(ConnectionConfiguration configuration, Resource project, double sonarVersion)
        {
            return this.IsSupported(configuration, project) ? new CxxLocalExtension(this, new CommandExecution(), configuration, project, sonarVersion) : null;
        }

        /// <summary>
        /// The get server analyser extension.
        /// </summary>
        /// <param name="configuration">
        /// The configuration.
        /// </param>
        /// <param name="project">
        /// The project.
        /// </param>
        /// <returns>
        /// The <see cref="IServerAnalyserExtension"/>.
        /// </returns>
        public IServerAnalyserExtension GetServerAnalyserExtension(ConnectionConfiguration configuration, Resource project)
        {
            return new CxxServerExtension();
        }

        /// <summary>
        /// The get licenses.
        /// </summary>
        /// <param name="configuration">
        /// The configuration.
        /// </param>
        /// <returns>
        /// The <see>
        ///         <cref>Dictionary</cref>
        ///     </see>
        ///     .
        /// </returns>
        public Dictionary<string, VsLicense> GetLicenses(ConnectionConfiguration configuration)
        {
            return new Dictionary<string, VsLicense>();
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
        public string GenerateTokenId(ConnectionConfiguration configuration)
        {
            return string.Empty;
        }
    }
}
