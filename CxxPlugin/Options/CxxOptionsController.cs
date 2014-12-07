// --------------------------------------------------------------------------------------------------------------------
// <copyright file="CxxOptionsController.cs" company="Copyright © 2014 jmecsoftware">
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

    using VSSonarPlugins;

    using UserControl = System.Windows.Controls.UserControl;

    /// <summary>
    ///     The dummy options controller.
    /// </summary>
    public class CxxOptionsController : INotifyPropertyChanged, IPluginsOptions
    {
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

        /// <summary>
        /// The pc lint executable.
        /// </summary>
        private string pclintExecutable;

        /// <summary>
        /// The pclint arguments.
        /// </summary>
        private string pclintArguments;

        /// <summary>
        /// The pclint environment.
        /// </summary>
        private string pclintEnvironment;

        /// <summary>
        /// The project working dir.
        /// </summary>
        private string projectWorkingDir;

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
        public bool ProjectIsAssociated
        {
            get
            {
                if (this.Project == null)
                {
                    return false;
                }

                if (string.IsNullOrEmpty(this.Project.Lang))
                {
                    return true;
                }

                return CxxPlugin.IsSupported(this.Project);
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

        /// <summary>
        /// Gets or sets the pc lint executable.
        /// </summary>
        public string PcLintExecutable
        {
            get
            {
                return this.pclintExecutable;
            }

            set
            {
                this.pclintExecutable = value;
                this.OnPropertyChanged("pclintExecutable");
            }
        }

        /// <summary>
        /// Gets or sets the pc lint arguments.
        /// </summary>
        public string PcLintArguments
        {
            get
            {
                return this.pclintArguments;
            }

            set
            {
                this.pclintArguments = value;
                this.OnPropertyChanged("PcLintArguments");
            }
        }

        /// <summary>
        /// Gets or sets the pc lint environment.
        /// </summary>
        public string PcLintEnvironment
        {
            get
            {
                return this.pclintEnvironment;
            }

            set
            {
                this.pclintEnvironment = value;
                this.OnPropertyChanged("PcLintEnvironment");
            }
        }

        /// <summary>
        /// Gets or sets the pc lint environment.
        /// </summary>
        public string ProjectWorkingDir
        {
            get
            {
                return this.projectWorkingDir;
            }

            set
            {
                this.projectWorkingDir = value;
                this.OnPropertyChanged("ProjectWorkingDir");
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
                                  { "PcLintExecutable", this.PcLintExecutable }, 
                                  { "PcLintArguments", this.PcLintArguments }, 
                                  { "PcLintEnvironment", this.PcLintEnvironment }, 
                                  { "RatsExecutable", this.RatsExecutable }, 
                                  { "RatsArguments", this.RatsArguments }, 
                                  { "RatsEnvironment", this.RatsEnvironment }, 
                                  { "CppCheckExecutable", this.CppCheckExecutable }, 
                                  { "CppCheckArguments", this.CppCheckArguments }, 
                                  { "CppCheckEnvironment", this.CppCheckEnvironment }, 
                                  { "CustomExecutable", this.CustomExecutable }, 
                                  { "CustomArguments", this.CustomArguments }, 
                                  { "CustomKey", this.CustomKey }, 
                                  { "CustomEnvironment", this.CustomEnvironment }
                              };

            // add specific solution properties
            if (this.Project == null)
            {
                return options;
            }

            if (!string.IsNullOrEmpty(this.ProjectWorkingDir))
            {
                options.Add(this.Project.Key + ".ProjectWorkingDir", this.ProjectWorkingDir);
            }

            if (this.propertiesToRunner != null)
            {
                string[] separator = { "\r\n", ";" };

                string[] properties = this.propertiesToRunner.Split(separator, StringSplitOptions.RemoveEmptyEntries);

                foreach (var elements in properties.Select(property => property.Split('=')))
                {
                    options.Add(this.Project.Key + ".propertyToRunner." + elements[0].Trim(), elements[1].Trim());
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
            this.Project = projectIn;
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
            const string PathDef = "C:\\Tekla\\BuildTools";

            if (optionsTab.Equals("Vera++"))
            {
                this.VeraExecutable = string.Empty;
                this.VeraArguments = string.Empty;

                if (!string.IsNullOrEmpty(PathDef))
                {
                    this.VeraExecutable = PathDef + "\\vera++\\bin\\vera++.exe";
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

                if (!string.IsNullOrEmpty(PathDef))
                {
                    this.CppCheckExecutable = PathDef + "\\cppcheck\\cppcheck.exe";
                    this.CppCheckArguments = "--inline-suppr --enable=all --xml -D__cplusplus -DNT";
                }
            }

            if (optionsTab.Equals("Rats"))
            {
                this.RatsExecutable = string.Empty;
                this.RatsArguments = string.Empty;

                if (!string.IsNullOrEmpty(PathDef))
                {
                    this.RatsExecutable = PathDef + "\\rats-2.3\\rats.exe";
                    this.RatsArguments = "--xml";
                }
            }

            if (optionsTab.Equals("RunnerOptions"))
            {
                if (File.Exists(PathDef + "\\Cpplint\\settingsForSonar.cfg"))
                {
                    this.PropertiesToRunner = File.ReadAllText(PathDef + "\\Cpplint\\settingsForSonar.cfg");
                }
            }

            if (optionsTab.Equals("ExternalSensor"))
            {
                this.CustomExecutable = string.Empty;
                this.CustomArguments = string.Empty;
                this.CustomKey = string.Empty;
                this.CustomEnvironment = string.Empty;

                if (!string.IsNullOrEmpty(PathDef))
                {
                    this.CustomExecutable = PathDef + "\\Python\\python.exe";
                    this.CustomArguments = PathDef + "\\CppLint\\cpplint_mod.py --output=vs7";
                    this.CustomKey = "cpplint";
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

            this.PcLintExecutable = this.GetOptionIfExists(options, "PcLintExecutable");
            this.PcLintArguments = this.GetOptionIfExists(options, "PcLintArguments");
            this.PcLintEnvironment = this.GetOptionIfExists(options, "PcLintEnvironment");

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
          
            if (this.Project == null)
            {
                return;
            }

            this.ProjectWorkingDir = this.GetOptionIfExists(options, this.Project.Key + ".ProjectWorkingDir");

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
        /// The <see>
        ///         <cref>Dictionary</cref>
        ///     </see>
        ///     .
        /// </returns>
        private Dictionary<string, string> GetOptionsStartingWith(Dictionary<string, string> options, string key)
        {
            return options.Where(option => option.Key.StartsWith(key)).ToDictionary(option => option.Key, option => option.Value);
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
                var openFileDialog = new OpenFileDialog { Filter = filter };
                return openFileDialog.ShowDialog() == DialogResult.OK ? openFileDialog.FileName : string.Empty;
            }

            #endregion
        }
    }
}