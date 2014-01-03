﻿// --------------------------------------------------------------------------------------------------------------------
// <copyright file="CxxOptionsController.cs" company="Copyright © 2013 Tekla Corporation. Tekla is a Trimble Company">
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
namespace CxxPlugin.Options
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel;
    using System.IO;
    using System.Linq;
    using System.Windows.Forms;

    using global::CxxPlugin.Commands;

    using ExtensionTypes;

    using SonarRestService;

    using VSSonarPlugins;

    using UserControl = System.Windows.Controls.UserControl;

    /// <summary>
    ///     The dummy options controller.
    /// </summary>
    public class CxxOptionsController : INotifyPropertyChanged, IPluginsOptions
    {
        #region Constants

        /// <summary>
        /// The excluded plugins default value.
        /// </summary>
        private const string ExcludedPluginsDefaultValue = "devcockpit,pdfreport,report,scmactivity,views,jira,scmstats";

        #endregion

        #region Fields

        /// <summary>
        ///     The cpp check arguments.
        /// </summary>
        private string cppCheckArguments;

        /// <summary>
        ///     The cpp check environments.
        /// </summary>
        private string cppCheckEnvironment;

        /// <summary>
        ///     The cpp check executable.
        /// </summary>
        private string cppCheckExecutable;

        /// <summary>
        ///     The custom arguments.
        /// </summary>
        private string customArguments;

        /// <summary>
        ///     The custom environment.
        /// </summary>
        private string customEnvironment;

        /// <summary>
        ///     The custom executable.
        /// </summary>
        private string customExecutable;

        /// <summary>
        ///     The custom key.
        /// </summary>
        private string customKey;

        /// <summary>
        ///     The dummy control.
        /// </summary>
        private UserControl cxxControl;

        /// <summary>
        ///     The excluded plugins.
        /// </summary>
        private string excludedPlugins;

        /// <summary>
        ///     The is debug checked.
        /// </summary>
        private bool isDebugChecked;

        /// <summary>
        ///     The java binary path.
        /// </summary>
        private string javaBinaryPath;

        /// <summary>
        ///     The maven is checked.
        /// </summary>
        private bool mavenIsChecked;

        /// <summary>
        ///     The maven path.
        /// </summary>
        private string mavenPath;

        /// <summary>
        ///     The project.
        /// </summary>
        private Resource project;

        /// <summary>
        ///     The properties to runner.
        /// </summary>
        private string propertiesToRunner;

        /// <summary>
        ///     The rats arguments.
        /// </summary>
        private string ratsArguments;

        /// <summary>
        ///     The rats environment.
        /// </summary>
        private string ratsEnvironment;

        /// <summary>
        ///     The rats executable.
        /// </summary>
        private string ratsExecutable;

        /// <summary>
        ///     The sonar runner is checked.
        /// </summary>
        private bool sonarRunnerIsChecked;

        /// <summary>
        ///     The sonar runner path.
        /// </summary>
        private string sonarRunnerPath;

        /// <summary>
        ///     The vera arguments.
        /// </summary>
        private string veraArguments;

        /// <summary>
        ///     The vera environment.
        /// </summary>
        private string veraEnvironment;

        /// <summary>
        ///     The vera executable.
        /// </summary>
        private string veraExecutable;

        #endregion

        #region Constructors and Destructors

        /// <summary>
        ///     Initializes a new instance of the <see cref="CxxOptionsController" /> class.
        /// </summary>
        public CxxOptionsController()
        {
            this.cxxControl = null;
            this.OpenCommand = new CxxOpenFileCommand(this, new CxxService());
            this.ResetDefaultCommand = new CxxResetDefaultsCommand(this);
            this.ExcludedPlugins = ExcludedPluginsDefaultValue;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="CxxOptionsController"/> class.
        /// </summary>
        /// <param name="service">
        /// The service.
        /// </param>
        public CxxOptionsController(ICxxIoService service)
        {
            this.cxxControl = null;
            this.OpenCommand = service != null ? new CxxOpenFileCommand(this, service) : new CxxOpenFileCommand(this, new CxxService());
            this.ResetDefaultCommand = new CxxResetDefaultsCommand(this);
        }

        #endregion

        #region Public Events

        /// <summary>
        ///     The property changed.
        /// </summary>
        public event PropertyChangedEventHandler PropertyChanged;

        #endregion

        #region Public Properties

        /// <summary>
        ///     Gets or sets the vera arguments.
        /// </summary>
        public string CppCheckArguments
        {
            get
            {
                return this.cppCheckArguments;
            }

            set
            {
                this.cppCheckArguments = value;
                this.OnPropertyChanged("CppCheckArguments");
            }
        }

        /// <summary>
        ///     Gets or sets the vera arguments.
        /// </summary>
        public string CppCheckEnvironment
        {
            get
            {
                return this.cppCheckEnvironment;
            }

            set
            {
                this.cppCheckEnvironment = value;
                this.OnPropertyChanged("CppCheckEnvironment");
            }
        }

        /// <summary>
        ///     Gets or sets the text box.
        /// </summary>
        public string CppCheckExecutable
        {
            get
            {
                return this.cppCheckExecutable;
            }

            set
            {
                this.cppCheckExecutable = value;
                this.OnPropertyChanged("CppCheckExecutable");
            }
        }

        /// <summary>
        ///     Gets or sets the vera arguments.
        /// </summary>
        public string CustomArguments
        {
            get
            {
                return this.customArguments;
            }

            set
            {
                this.customArguments = value;
                this.OnPropertyChanged("CustomArguments");
            }
        }

        /// <summary>
        ///     Gets or sets the vera arguments.
        /// </summary>
        public string CustomEnvironment
        {
            get
            {
                return this.customEnvironment;
            }

            set
            {
                this.customEnvironment = value;
                this.OnPropertyChanged("CustomEnvironment");
            }
        }

        /// <summary>
        ///     Gets or sets the text box.
        /// </summary>
        public string CustomExecutable
        {
            get
            {
                return this.customExecutable;
            }

            set
            {
                this.customExecutable = value;
                this.OnPropertyChanged("CustomExecutable");
            }
        }

        /// <summary>
        ///     Gets or sets the vera arguments.
        /// </summary>
        public string CustomKey
        {
            get
            {
                return this.customKey;
            }

            set
            {
                this.customKey = value;
                this.OnPropertyChanged("CustomKey");
            }
        }

        /// <summary>
        ///     Gets or sets the excluded plugins.
        /// </summary>
        public string ExcludedPlugins
        {
            get
            {
                return this.excludedPlugins;
            }

            set
            {
                this.excludedPlugins = value;
                this.OnPropertyChanged("ExcludedPlugins");
            }
        }

        /// <summary>
        ///     Gets or sets a value indicating whether is debug checked.
        /// </summary>
        public bool IsDebugChecked
        {
            get
            {
                return this.isDebugChecked;
            }

            set
            {
                this.isDebugChecked = value;
                this.OnPropertyChanged("IsDebugChecked");
            }
        }

        /// <summary>
        ///     Gets or sets the java binary path.
        /// </summary>
        public string JavaBinaryPath
        {
            get
            {
                return this.javaBinaryPath;
            }

            set
            {
                this.javaBinaryPath = value;
                this.OnPropertyChanged("JavaBinaryPath");
            }
        }

        /// <summary>
        ///     Gets or sets a value indicating whether maven is checked.
        /// </summary>
        public bool MavenIsChecked
        {
            get
            {
                return this.mavenIsChecked;
            }

            set
            {
                this.mavenIsChecked = value;
                if (value)
                {
                    this.SonarRunnerIsChecked = false;
                }

                this.OnPropertyChanged("MavenIsChecked");
            }
        }

        /// <summary>
        ///     Gets or sets the maven path.
        /// </summary>
        public string MavenPath
        {
            get
            {
                return this.mavenPath;
            }

            set
            {
                this.mavenPath = value;
                this.OnPropertyChanged("MavenPath");
            }
        }

        /// <summary>
        ///     Gets or sets the open command.
        /// </summary>
        public CxxOpenFileCommand OpenCommand { get; set; }

        /// <summary>
        ///     Gets or sets the project.
        /// </summary>
        public Resource Project
        {
            get
            {
                return this.project;
            }

            set
            {
                this.project = value;
                this.OnPropertyChanged("ProjectIsAssociated");
            }
        }

        /// <summary>
        ///     Gets the project is associated.
        /// </summary>
        public object ProjectIsAssociated
        {
            get
            {
                return this.Project != null;
            }
        }

        /// <summary>
        ///     Gets or sets the properties to runner.
        /// </summary>
        public string PropertiesToRunner
        {
            get
            {
                return this.propertiesToRunner;
            }

            set
            {
                this.propertiesToRunner = value;
                this.OnPropertyChanged("PropertiesToRunner");
            }
        }

        /// <summary>
        ///     Gets or sets the vera arguments.
        /// </summary>
        public string RatsArguments
        {
            get
            {
                return this.ratsArguments;
            }

            set
            {
                this.ratsArguments = value;
                this.OnPropertyChanged("RatsArguments");
            }
        }

        /// <summary>
        ///     Gets or sets the vera arguments.
        /// </summary>
        public string RatsEnvironment
        {
            get
            {
                return this.ratsEnvironment;
            }

            set
            {
                this.ratsEnvironment = value;
                this.OnPropertyChanged("RatsEnvironment");
            }
        }

        /// <summary>
        ///     Gets or sets the text box.
        /// </summary>
        public string RatsExecutable
        {
            get
            {
                return this.ratsExecutable;
            }

            set
            {
                this.ratsExecutable = value;
                this.OnPropertyChanged("RatsExecutable");
            }
        }

        /// <summary>
        ///     Gets or sets the reset default command.
        /// </summary>
        public CxxResetDefaultsCommand ResetDefaultCommand { get; set; }

        /// <summary>
        ///     Gets or sets a value indicating whether sonar runner is checked.
        /// </summary>
        public bool SonarRunnerIsChecked
        {
            get
            {
                return this.sonarRunnerIsChecked;
            }

            set
            {
                this.sonarRunnerIsChecked = value;
                if (value)
                {
                    this.MavenIsChecked = false;
                }

                this.OnPropertyChanged("SonarRunnerIsChecked");
            }
        }

        /// <summary>
        ///     Gets or sets the sonar runner path.
        /// </summary>
        public string SonarRunnerPath
        {
            get
            {
                return this.sonarRunnerPath;
            }

            set
            {
                this.sonarRunnerPath = value;
                this.OnPropertyChanged("SonarRunnerPath");
            }
        }

        /// <summary>
        ///     Gets or sets the vera arguments.
        /// </summary>
        public string VeraArguments
        {
            get
            {
                return this.veraArguments;
            }

            set
            {
                this.veraArguments = value;
                this.OnPropertyChanged("VeraArguments");
            }
        }

        /// <summary>
        ///     Gets or sets the vera arguments.
        /// </summary>
        public string VeraEnvironment
        {
            get
            {
                return this.veraEnvironment;
            }

            set
            {
                this.veraEnvironment = value;
                this.OnPropertyChanged("VeraEnvironment");
            }
        }

        /// <summary>
        ///     Gets or sets the text box.
        /// </summary>
        public string VeraExecutable
        {
            get
            {
                return this.veraExecutable;
            }

            set
            {
                this.veraExecutable = value;
                this.OnPropertyChanged("VeraExecutable");
            }
        }

        #endregion

        #region Public Methods and Operators

        /// <summary>
        ///     The get options.
        /// </summary>
        /// <returns>
        ///     The
        ///     <see>
        ///         <cref>Dictionary</cref>
        ///     </see>
        /// </returns>
        public Dictionary<string, string> GetOptions()
        {
            var options = new Dictionary<string, string>
                              {
                                  { "VeraExecutable", this.VeraExecutable }, 
                                  { "VeraArguments", this.VeraArguments }, 
                                  { "VeraEnvironment", this.VeraEnvironment }, 
                                  { "RatsExecutable", this.RatsExecutable }, 
                                  { "RatsArguments", this.RatsArguments }, 
                                  { "RatsEnvironment", this.RatsEnvironment }, 
                                  { "CppCheckExecutable", this.CppCheckExecutable }, 
                                  { "CppCheckArguments", this.CppCheckArguments }, 
                                  { "CppCheckEnvironment", this.CppCheckEnvironment }, 
                                  { "CustomExecutable", this.CustomExecutable }, 
                                  { "CustomArguments", this.CustomArguments }, 
                                  { "CustomKey", this.CustomKey }, 
                                  { "CustomEnvironment", this.CustomEnvironment }, 
                                  { "JavaBinaryPath", this.JavaBinaryPath }
                              };

            // add specific solution properties
            if (this.Project == null)
            {
                return options;
            }

            options.Add(this.Project.Key + ".IsDebugChecked", this.IsDebugChecked ? "true" : "false");

            if (this.SonarRunnerIsChecked)
            {
                options.Add(this.Project.Key + ".MavenIsChecked", "false");
                options.Add(this.Project.Key + ".SonarRunnerIsChecked", "true");
            }
            else
            {
                options.Add(this.Project.Key + ".MavenIsChecked", "true");
                options.Add(this.Project.Key + ".SonarRunnerIsChecked", "false");
            }

            options.Add(this.Project.Key + ".ExcludedPlugins", this.ExcludedPlugins);
            options.Add(this.Project.Key + ".MavenPath", this.MavenPath);
            options.Add(this.Project.Key + ".SonarRunnerPath", this.SonarRunnerPath);

            if (this.propertiesToRunner != null)
            {
                string[] separator = { "\r\n", ";" };

                string[] properties = this.propertiesToRunner.Split(separator, StringSplitOptions.RemoveEmptyEntries);

                foreach (var elements in properties.Select(property => property.Split('=')))
                {
                    options.Add(this.Project.Key + ".propertyToRunner." + elements[0], elements[1]);
                }
            }

            return options;
        }

        /// <summary>
        /// The get user control options.
        /// </summary>
        /// <param name="projectIn">
        /// The project.
        /// </param>
        /// <returns>
        /// The <see cref="UserControl"/>.
        /// </returns>
        public UserControl GetUserControlOptions(Resource projectIn)
        {
            return this.cxxControl ?? (this.cxxControl = new CxxUserControl(this));
        }

        /// <summary>
        ///     The is enabled.
        /// </summary>
        /// <returns>
        ///     The <see cref="bool" />.
        /// </returns>
        public bool IsEnabled()
        {
            return true;
        }

        /// <summary>
        ///     The reset defaults.
        /// </summary>
        public void ResetDefaults()
        {
            this.ResetOptions("Vera++");
            this.ResetOptions("CppCheck");
            this.ResetOptions("Rats");
            this.ResetOptions("ExternalSensor");
            this.ResetOptions("RunnerOptions");
        }

        /// <summary>
        /// The reset options.
        /// </summary>
        /// <param name="optionsTab">
        /// The options tab.
        /// </param>
        public void ResetOptions(string optionsTab)
        {
            string home = Environment.GetEnvironmentVariable("HOME");
            string xmlpath = home + "\\ts_user.settings";
            string pathDef = string.Empty;
            if (File.Exists(xmlpath))
            {
                IXmlHelpersService xmlparser = new XmlHelpersService();
                try
                {
                    pathDef = xmlparser.GetUserSRCDir(xmlpath).Replace("$(TSVersion)", "work");
                }
                catch (Exception)
                {
                    pathDef = string.Empty;
                }
            }

            if (optionsTab.Equals("Vera++"))
            {
                this.VeraExecutable = string.Empty;
                this.VeraArguments = string.Empty;

                if (!string.IsNullOrEmpty(pathDef))
                {
                    this.VeraExecutable = pathDef + "\\MSBuild\\Sonar\\apps\\vera++\\bin\\vera++.exe";
                    this.VeraArguments = "-nodup -showrules";

                    string activeDir = Path.GetDirectoryName(this.VeraExecutable);
                    if (!string.IsNullOrEmpty(activeDir))
                    {
                        string rootp = Directory.GetParent(activeDir).ToString();
                        this.VeraEnvironment = "VERA_ROOT=" + Path.Combine(rootp, "lib", "vera++");
                    }
                }
            }

            if (optionsTab.Equals("CppCheck"))
            {
                this.CppCheckExecutable = string.Empty;
                this.CppCheckArguments = string.Empty;

                if (!string.IsNullOrEmpty(pathDef))
                {
                    this.CppCheckExecutable = pathDef + "\\MSBuild\\Sonar\\apps\\cppcheck\\cppcheck.exe";
                    this.CppCheckArguments = "--inline-suppr --enable=all --xml -D__cplusplus -DNT";
                }
            }

            if (optionsTab.Equals("Rats"))
            {
                this.RatsExecutable = string.Empty;
                this.RatsArguments = string.Empty;

                if (!string.IsNullOrEmpty(pathDef))
                {
                    this.RatsExecutable = pathDef + "\\MSBuild\\Sonar\\apps\\rats-2.3\\rats.exe";
                    this.RatsArguments = "--xml";
                }
            }

            if (optionsTab.Equals("RunnerOptions"))
            {
                this.MavenIsChecked = true;
                this.MavenPath = pathDef + "\\MSBuild\\BuildTools\\apache-maven-2.2.1\\bin\\mvn.bat";
                this.ExcludedPlugins = ExcludedPluginsDefaultValue;
                this.PropertiesToRunner = "SRCDir=" + pathDef
                                          + ";COMMON=Core\\Common;MODEL=Core\\Model;DRAWINGS=Core\\Drawings;TS=Core\\TeklaStructures;ANALYSIS=Core\\Analysis";
            }

            if (optionsTab.Equals("ExternalSensor"))
            {
                this.CustomExecutable = string.Empty;
                this.CustomArguments = string.Empty;
                this.CustomKey = string.Empty;
                this.CustomEnvironment = string.Empty;

                if (!string.IsNullOrEmpty(pathDef))
                {
                    this.CustomExecutable = pathDef + "\\MSBuild\\Sonar\\apps\\Python27\\python.exe";
                    this.CustomArguments = pathDef + "\\MSBuild\\Sonar\\scripts\\cpplint_mod.py --output=vs7";
                    this.CustomKey = "cpplint";
                    this.CustomEnvironment = "UserSRCDir=" + pathDef;
                }
            }
        }

        /// <summary>
        /// The set options.
        /// </summary>
        /// <param name="options">
        /// The options.
        /// </param>
        public void SetOptions(Dictionary<string, string> options)
        {
            this.VeraExecutable = this.GetOptionIfExists(options, "VeraExecutable");
            this.VeraArguments = this.GetOptionIfExists(options, "VeraArguments");
            this.VeraEnvironment = this.GetOptionIfExists(options, "VeraEnvironment");

            this.RatsExecutable = this.GetOptionIfExists(options, "RatsExecutable");
            this.RatsArguments = this.GetOptionIfExists(options, "RatsArguments");
            this.RatsEnvironment = this.GetOptionIfExists(options, "RatsEnvironment");

            this.CppCheckExecutable = this.GetOptionIfExists(options, "CppCheckExecutable");
            this.CppCheckArguments = this.GetOptionIfExists(options, "CppCheckArguments");
            this.CppCheckEnvironment = this.GetOptionIfExists(options, "CppCheckEnvironment");

            this.CustomExecutable = this.GetOptionIfExists(options, "CustomExecutable");
            this.CustomArguments = this.GetOptionIfExists(options, "CustomArguments");
            this.CustomKey = this.GetOptionIfExists(options, "CustomKey");
            this.CustomEnvironment = this.GetOptionIfExists(options, "CustomEnvironment");

            this.JavaBinaryPath = this.GetOptionIfExists(options, "JavaBinaryPath");

            if (this.Project == null)
            {
                return;
            }

            if (this.GetOptionIfExists(options, this.Project.Key + ".MavenIsChecked").Equals("true"))
            {
                this.MavenIsChecked = true;
            }
            else
            {
                this.SonarRunnerIsChecked = true;
            }

            if (this.GetOptionIfExists(options, this.Project.Key + ".IsDebugChecked").Equals("true"))
            {
                this.IsDebugChecked = true;
            }
            else
            {
                this.IsDebugChecked = false;
            }

            this.ExcludedPlugins = this.GetOptionIfExists(options, this.Project.Key + ".ExcludedPlugins");
            if (string.IsNullOrEmpty(this.ExcludedPlugins))
            {
                this.ExcludedPlugins = ExcludedPluginsDefaultValue;
            }

            this.MavenPath = this.GetOptionIfExists(options, this.Project.Key + ".MavenPath");
            this.SonarRunnerPath = this.GetOptionIfExists(options, this.Project.Key + ".SonarRunnerPath");

            Dictionary<string, string> optionsForProject = this.GetOptionsStartingWith(options, this.Project.Key + ".propertyToRunner.");

            string props = optionsForProject.Aggregate(
                string.Empty, 
                (current, option) =>
                current + (option.Key.Replace(this.Project.Key + ".propertyToRunner.", string.Empty) + "=" + option.Value + "\r\n"));
            this.PropertiesToRunner = props;
        }

        #endregion

        #region Methods

        /// <summary>
        /// The on property changed.
        /// </summary>
        /// <param name="propertyName">
        /// The property name.
        /// </param>
        protected virtual void OnPropertyChanged(string propertyName)
        {
            PropertyChangedEventHandler handler = this.PropertyChanged;
            if (handler != null)
            {
                handler(this, new PropertyChangedEventArgs(propertyName));
            }
        }

        /// <summary>
        /// The get option if exists.
        /// </summary>
        /// <param name="options">
        /// The options.
        /// </param>
        /// <param name="key">
        /// The key.
        /// </param>
        /// <returns>
        /// The <see cref="string"/>.
        /// </returns>
        private string GetOptionIfExists(Dictionary<string, string> options, string key)
        {
            if (options.ContainsKey(key))
            {
                return options[key];
            }

            return string.Empty;
        }

        /// <summary>
        /// The get options starting with.
        /// </summary>
        /// <param name="options">
        /// The options.
        /// </param>
        /// <param name="key">
        /// The key.
        /// </param>
        /// <returns>
        /// The <see cref="Dictionary"/>.
        /// </returns>
        private Dictionary<string, string> GetOptionsStartingWith(Dictionary<string, string> options, string key)
        {
            var optionsToReturn = new Dictionary<string, string>();

            foreach (var option in options)
            {
                if (option.Key.StartsWith(key))
                {
                    optionsToReturn.Add(option.Key, option.Value);
                }
            }

            return optionsToReturn;
        }

        #endregion

        /// <summary>
        ///     The io service.
        /// </summary>
        internal class CxxService : ICxxIoService
        {
            #region Public Methods and Operators

            /// <summary>
            /// The open file dialog.
            /// </summary>
            /// <param name="filter">
            /// The filter.
            /// </param>
            /// <returns>
            /// The <see cref="string"/>.
            /// </returns>
            public string OpenFileDialog(string filter)
            {
                var openFileDialog = new OpenFileDialog();
                openFileDialog.Filter = filter;
                return openFileDialog.ShowDialog() == DialogResult.OK ? openFileDialog.FileName : string.Empty;
            }

            #endregion
        }
    }
}