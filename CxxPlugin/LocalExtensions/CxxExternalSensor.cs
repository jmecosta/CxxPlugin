// --------------------------------------------------------------------------------------------------------------------
// <copyright file="CxxExternalSensor.cs" company="Copyright © 2013 Tekla Corporation. Tekla is a Trimble Company">
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

    using VSSonarPlugins;

    /// <summary>
    /// The vera sensor.
    /// </summary>
    public class CxxExternalSensor : ASensor
    {
        /// <summary>
        /// The s key.
        /// </summary>
        public const string SKey = "other";

        /// <summary>
        /// The other key.
        /// </summary>
        public readonly string OtherKey = string.Empty;

        /// <summary>
        /// The plugin options.
        /// </summary>
        private readonly IPluginsOptions pluginOptions;

        /// <summary>
        /// Initializes a new instance of the <see cref="CxxExternalSensor"/> class.
        /// </summary>
        /// <param name="pluginOptions">
        /// The plugin Options.
        /// </param>
        public CxxExternalSensor(IPluginsOptions pluginOptions)
            : base(SKey, false)
        {
            this.pluginOptions = pluginOptions;
            this.OtherKey = this.pluginOptions.GetOptions()["CustomKey"];
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

            foreach (var line in lines)
            {
                try
                {
                    int start = 0;
                    var file = GetStringUntilFirstChar(ref start, line, '(');

                    start++;
                    var linenumber = Convert.ToInt32(GetStringUntilFirstChar(ref start, line, ')'));

                    start += 2;
                    var msg = GetStringUntilFirstChar(ref start, line, '[').Trim();

                    start++;
                    var id = GetStringUntilFirstChar(ref start, line, ']');

                    if (!string.IsNullOrEmpty(this.OtherKey))
                    {
                        id = this.OtherKey + "." + id;
                    }

                    var entry = new Issue
                                    {
                                        Line = linenumber,
                                        Message = msg,
                                        Rule = this.RepositoryKey + "." + id,
                                        Component = file
                                    };

                    violations.Add(entry);
                }
                catch (Exception ex)
                {
                    Console.WriteLine("error: " + ex.Message);
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
            var data = VsSonarUtils.GetEnvironmentFromString(this.pluginOptions.GetOptions()["CustomEnvironment"]);
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
            return this.pluginOptions.GetOptions()["CustomExecutable"];
        }

        /// <summary>
        /// The get arguments.
        /// </summary>
        /// <returns>
        /// The <see cref="string"/>.
        /// </returns>
        public override string GetArguments()
        {
            return this.pluginOptions.GetOptions()["CustomArguments"];
        }

        /// <summary>
        /// The get string until first char.
        /// </summary>
        /// <param name="start">
        /// The start.
        /// </param>
        /// <param name="line">
        /// The line.
        /// </param>
        /// <param name="charCheck">
        /// The char check.
        /// </param>
        /// <returns>
        /// The System.String.
        /// </returns>
        private static string GetStringUntilFirstChar(ref int start, string line, char charCheck)
        {
            var data = string.Empty;

            for (int i = start; i < line.Length; i++)
            {
                start = i;
                if (line[i].Equals(charCheck))
                {
                    break;
                }

                data += line[i];
            }

            return data;
        }
    }
}
