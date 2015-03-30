// --------------------------------------------------------------------------------------------------------------------
// <copyright file="CxxExternalSensor.cs" company="Copyright � 2014 jmecsoftware">
//   Copyright (C) 2014 [jmecsoftware, jmecsoftware2014@tekla.com]
// </copyright>
// <summary>
//   The vera sensor.
// </summary>
// --------------------------------------------------------------------------------------------------------------------
namespace CxxPlugin.LocalExtensions
{
    using System;
    using System.Collections.Generic;

    using Microsoft.FSharp.Collections;

    using SonarRestService;

    using VSSonarPlugins;
    using VSSonarPlugins.Helpers;
    using VSSonarPlugins.Types;

    /// <summary>
    ///     The vera sensor.
    /// </summary>
    public class CxxExternalSensor : ASensor
    {
        /// <summary>
        ///     The s key.
        /// </summary>
        public const string SKey = "other";

        /// <summary>
        ///     The other key.
        /// </summary>
        public readonly string OtherKey = string.Empty;

        /// <summary>Initializes a new instance of the <see cref="CxxExternalSensor"/> class.</summary>
        /// <param name="notificationManager">The notification Manager.</param>
        /// <param name="configurationHelper">The configuration Helper.</param>
        /// <param name="sonarRestService">The sonar Rest Service.</param>
        public CxxExternalSensor(
            INotificationManager notificationManager, 
            IConfigurationHelper configurationHelper, 
            ISonarRestService sonarRestService)
            : base(SKey, false, notificationManager, configurationHelper, sonarRestService)
        {
            this.WriteProperty("CustomEnvironment", string.Empty, true, true);
            this.WriteProperty("CustomExecutable", @"C:\Tekla\buildtools\Python\python.exe", true, true);
            this.WriteProperty(
                "CustomArguments", 
                @"C:\Tekla\buildtools\CppLint\cpplint_mod.py --output=vs7", 
                true, 
                true);
            this.WriteProperty("CustomKey", "cpplint", true, true);
        }

        /// <summary>The get violations.</summary>
        /// <param name="lines">The lines.</param>
        /// <returns>The VSSonarPlugin.SonarInterface.ResponseMappings.Violations.ViolationsResponse.</returns>
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
                    var start = 0;
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
        ///     The get environment.
        /// </summary>
        /// <returns>
        ///     The
        ///     <see>
        ///         <cref>Dictionary</cref>
        ///     </see>
        ///     .
        /// </returns>
        public override FSharpMap<string, string> GetEnvironment()
        {
            var data = VsSonarUtils.GetEnvironmentFromString(this.ReadGetProperty("CustomEnvironment"));
            return ConvertCsMapToFSharpMap(data);
        }

        /// <summary>
        ///     The get command.
        /// </summary>
        /// <returns>
        ///     The <see cref="string" />.
        /// </returns>
        public override string GetCommand()
        {
            return this.ReadGetProperty("CustomExecutable");
        }

        /// <summary>
        ///     The get arguments.
        /// </summary>
        /// <returns>
        ///     The <see cref="string" />.
        /// </returns>
        public override string GetArguments()
        {
            return this.ReadGetProperty("CustomArguments");
        }

        /// <summary>The get string until first char.</summary>
        /// <param name="start">The start.</param>
        /// <param name="line">The line.</param>
        /// <param name="charCheck">The char check.</param>
        /// <returns>The System.String.</returns>
        private static string GetStringUntilFirstChar(ref int start, string line, char charCheck)
        {
            var data = string.Empty;

            for (var i = start; i < line.Length; i++)
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