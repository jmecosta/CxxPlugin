// --------------------------------------------------------------------------------------------------------------------
// <copyright file="RatsSensor.cs" company="Copyright © 2013 Tekla Corporation. Tekla is a Trimble Company">
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

    using global::CxxPlugin.Commands;

    using ExtensionHelpers;

    using ExtensionTypes;

    using Microsoft.FSharp.Collections;

    using RestSharp;
    using RestSharp.Deserializers;

    using VSSonarPlugins;

    /// <summary>
    /// The rats sensor.
    /// </summary>
    public class RatsSensor : ASensor
    {
        /// <summary>
        /// The s key.
        /// </summary>
        public const string SKey = "rats";

        /// <summary>
        /// The plugin options.
        /// </summary>
        private readonly IPluginsOptions pluginOptions;

        /// <summary>
        /// Initializes a new instance of the <see cref="RatsSensor"/> class.
        /// </summary>
        /// <param name="processCtrlIn">
        /// The process ctrl in.
        /// </param>
        /// <param name="pluginsOptions">
        /// The plugins options.
        /// </param>
        public RatsSensor(IPluginsOptions pluginsOptions)
            : base(SKey, true)
        {
            this.pluginOptions = pluginsOptions;
        }

        /// <summary>
        /// The get violations.
        /// </summary>
        /// <param name="lines">
        ///     The lines.
        /// </param>
        /// <returns>
        /// The VSSonarPlugin.SonarInterface.ResponseMappings.Violations.ViolationsResponse.
        /// </returns>
        public override List<Issue> GetViolations(FSharpList<string> lines)
        {
            var violations = new List<Issue>();

            if (lines == null || lines.Length == 0)
            {
                return violations;
            }

            var xml = new XmlDeserializer();
            var output = xml.Deserialize<List<Vulnerability>>(new RestResponse { Content = string.Join("\r\n", lines) });

            foreach (var result in output)
            {
                if (string.IsNullOrEmpty(result.Type))
                {
                    foreach (var file in result.Files)
                    {
                        foreach (var line in file.LineId)
                        {
                            var entry = new Issue
                                            {
                                                Rule = this.RepositoryKey + "." + result.Type,
                                                Line = line.Value,
                                                Message = result.Message,
                                                Severity = result.Severity,
                                                Component = file.Name
                                            };

                            violations.Add(entry);
                        }
                    }
                }
            }

            return violations;
        }

        /// <summary>
        /// The get environment.
        /// </summary>
        /// <returns>
        /// The <see>
        ///         <cref>Dictionary</cref>
        ///     </see>
        ///     .
        /// </returns>
        public override FSharpMap<string, string> GetEnvironment()
        {
            var data = VsSonarUtils.GetEnvironmentFromString(this.pluginOptions.GetOptions()["RatsEnvironment"]);
            return ConvertCsMapToFSharpMap(data);
        }

        /// <summary>
        /// The get command.
        /// </summary>
        /// <returns>
        /// The <see cref="string"/>.
        /// </returns>
        public override string GetCommand()
        {
            return this.pluginOptions.GetOptions()["RatsExecutable"];
        }

        /// <summary>
        /// The get arguments.
        /// </summary>
        /// <returns>
        /// The <see cref="string"/>.
        /// </returns>
        public override string GetArguments()
        {
            return this.pluginOptions.GetOptions()["RatsArguments"];
        }

        /// <summary>
        /// The vulnerability.
        /// </summary>
        internal class Vulnerability
        {
            /// <summary>
            /// Gets or sets the severity.
            /// </summary>
            public string Severity { get; set; }

            /// <summary>
            /// Gets or sets the type.
            /// </summary>
            public string Type { get; set; }

            /// <summary>
            /// Gets or sets the message.
            /// </summary>
            public string Message { get; set; }

            /// <summary>
            /// Gets or sets the files.
            /// </summary>
            public List<File> Files { get; set; }
        }

        /// <summary>
        /// The file.
        /// </summary>
        internal class File 
        {
            /// <summary>
            /// Gets or sets the name.
            /// </summary>
            public string Name { get; set; }

            /// <summary>
            /// Gets or sets the line.
            /// </summary>
            public List<Line> LineId { get; set; }
        }

        /// <summary>
        /// The line.
        /// </summary>
        internal class Line
        {
            /// <summary>
            /// Gets or sets the value.
            /// </summary>
            public int Value { get; set; }
        }
    }
}
