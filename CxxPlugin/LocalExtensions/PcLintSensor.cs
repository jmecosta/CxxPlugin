// --------------------------------------------------------------------------------------------------------------------
// <copyright file="PcLintSensor.cs" company="Copyright � 2014 jmecsoftware">
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

namespace CxxPlugin.LocalExtensions
{
    using System;
    using System.Collections.Generic;
    using System.IO;

    using global::CxxPlugin.Commands;

    

    

    using Microsoft.FSharp.Collections;

    using VSSonarPlugins;
    using VSSonarPlugins.Types;
    using SonarRestService;
    using VSSonarPlugins.Helpers;

    /// <summary>
    /// The vera sensor.
    /// </summary>
    public class PcLintSensor : ASensor
    {
        /// <summary>
        /// The s key.
        /// </summary>
        public const string SKey = "pclint";

        /// <summary>
        /// Initializes a new instance of the <see cref="PcLintSensor"/> class. 
        /// </summary>
        /// <param name="ctrl">
        /// The ctrl.
        /// </param>
        /// <param name="pluginOptions">
        /// The plugin Options.
        /// </param>
        public PcLintSensor(INotificationManager notificationManager, IConfigurationHelper configurationHelper, ISonarRestService sonarRestService)
            : base(SKey, false, notificationManager, configurationHelper, sonarRestService)
        {
            WriteProperty("PcLintEnvironment", "", true, true);
            WriteProperty("PcLintExecutable", "", true, true);
            WriteProperty("PcLintArguments", "", true, true);
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

                    var entry = new Issue
                                    {
                                        Line = linenumber,
                                        Message = msg,
                                        Rule = this.RepositoryKey + ":" + id,
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
            var data = VsSonarUtils.GetEnvironmentFromString(ReadGetProperty("PcLintEnvironment"));
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
            return ReadGetProperty("PcLintExecutable");
        }

        /// <summary>
        /// The get arguments.
        /// </summary>
        /// <returns>
        /// The <see cref="string"/>.
        /// </returns>
        public override string GetArguments()
        {
            var executable = ReadGetProperty("PcLintExecutable");
            var parent = Directory.GetParent(executable);
            return "-\"format=%(%F(%l):%) error : (%t -- %m) : [%n]\"" + "-i\"" + parent + "\" +ffn std.lnt env-vc10.lnt " + ReadGetProperty("PcLintArguments");
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
