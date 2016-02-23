// --------------------------------------------------------------------------------------------------------------------
// <copyright file="CxxOptionsController.cs" company="Copyright © 2014 jmecsoftware">
//   Copyright (C) 2014 [jmecsoftware, jmecsoftware2014@tekla.com]
// </copyright>
// <summary>
//   The dummy options controller.
// </summary>
// --------------------------------------------------------------------------------------------------------------------
namespace CxxPlugin.Options
{
    using System;
    using System.Windows.Controls;
    using System.Windows.Media;

    using global::CxxPlugin.Commands;

    using PropertyChanged;

    using VSSonarPlugins;
    using VSSonarPlugins.Types;

    /// <summary>
    ///     The dummy options controller.
    /// </summary>
    [ImplementPropertyChanged]
    public class CxxOptionsController : IPluginControlOption
    {
        #region Public Events

        /// <summary>
        ///     The configuration.
        /// </summary>
        private readonly IConfigurationHelper configuration;

        #endregion

        #region Fields

        /// <summary>
        ///     The dummy control.
        /// </summary>
        private UserControl cxxControl;

        /// <summary>
        ///     Initializes a new instance of the <see cref="CxxOptionsController" /> class.
        /// </summary>
        public CxxOptionsController()
        {
            this.cxxControl = null;
            this.OpenCommand = new CxxOpenFileCommand(this, new CxxService());
            this.ForeGroundColor = Colors.Black;
            this.BackGroundColor = Colors.White;
        }

        /// <summary>Initializes a new instance of the <see cref="CxxOptionsController"/> class.</summary>
        /// <param name="configurationHelper">The configuration helper.</param>
        public CxxOptionsController(IConfigurationHelper configurationHelper)
        {
            this.configuration = configurationHelper;
            this.cxxControl = null;
            this.OpenCommand = new CxxOpenFileCommand(this, new CxxService());
            this.ForeGroundColor = Colors.Black;
            this.BackGroundColor = Colors.White;
        }

        /// <summary>Initializes a new instance of the <see cref="CxxOptionsController"/> class.</summary>
        /// <param name="service">The service.</param>
        public CxxOptionsController(ICxxIoService service)
        {
            this.cxxControl = null;
            this.OpenCommand = service != null
                                   ? new CxxOpenFileCommand(this, service)
                                   : new CxxOpenFileCommand(this, new CxxService());

            this.ForeGroundColor = Colors.Black;
            this.BackGroundColor = Colors.White;
        }

        #endregion

        #region Public Properties

        /// <summary>
        ///     Gets or sets the fore ground color.
        /// </summary>
        public Color ForeGroundColor { get; set; }

        /// <summary>
        ///     Gets or sets the back ground color.
        /// </summary>
        public Color BackGroundColor { get; set; }

        /// <summary>Gets or sets the cpp check arguments.</summary>
        public string CppCheckArguments { get; set; }

        /// <summary>Gets or sets the cpp check environment.</summary>
        public string CppCheckEnvironment { get; set; }

        /// <summary>Gets or sets the cpp check executable.</summary>
        public string CppCheckExecutable { get; set; }

        /// <summary>Gets or sets the custom arguments.</summary>
        public string CustomArguments { get; set; }

        /// <summary>Gets or sets the custom environment.</summary>
        public string CustomEnvironment { get; set; }

        /// <summary>Gets or sets the custom executable.</summary>
        public string CustomExecutable { get; set; }

        /// <summary>Gets or sets the custom key.</summary>
        public string CustomKey { get; set; }

        /// <summary>Gets or sets the open command.</summary>
        public CxxOpenFileCommand OpenCommand { get; set; }

        /// <summary>Gets or sets the project.</summary>
        public Resource Project { get; set; }

        /// <summary>Gets or sets the properties to runner.</summary>
        public string PropertiesToRunner { get; set; }

        /// <summary>Gets or sets the rats environment.</summary>
        public string RatsEnvironment { get; set; }

        /// <summary>Gets or sets the rats executable.</summary>
        public string RatsExecutable { get; set; }

        /// <summary>Gets or sets the rats arguments.</summary>
        public string RatsArguments { get; set; }

        /// <summary>Gets or sets the vera arguments.</summary>
        public string VeraArguments { get; set; }

        /// <summary>Gets or sets the vera environment.</summary>
        public string VeraEnvironment { get; set; }

        /// <summary>Gets or sets the vera executable.</summary>
        public string VeraExecutable { get; set; }

        /// <summary>Gets or sets the pc lint executable.</summary>
        public string PcLintExecutable { get; set; }

        /// <summary>Gets or sets the pc lint arguments.</summary>
        public string PcLintArguments { get; set; }

        /// <summary>Gets or sets the pc lint environment.</summary>
        public string PcLintEnvironment { get; set; }

        /// <summary>Gets or sets the project working dir.</summary>
        public string ProjectWorkingDir { get; set; }

        /// <summary>Gets or sets a value indicating whether project is associated.</summary>
        public bool ProjectIsAssociated { get; set; }

        #endregion

        #region Public Methods and Operators

        /// <summary>The get user control options.</summary>
        /// <param name="projectIn">The project.</param>
        /// <returns>The <see cref="UserControl"/>.</returns>
        public UserControl GetUserControlOptions(Resource projectIn)
        {
            this.Project = projectIn;
            return this.cxxControl ?? (this.cxxControl = new CxxUserControl(this));
        }

        /// <summary>
        ///     The set options.
        /// </summary>
        public void SetOptions()
        {
            this.VeraExecutable = this.GetOptionIfExists("VeraExecutable");
            this.VeraArguments = this.GetOptionIfExists("VeraArguments");
            this.VeraEnvironment = this.GetOptionIfExists("VeraEnvironment");

            this.PcLintExecutable = this.GetOptionIfExists("PcLintExecutable");
            this.PcLintArguments = this.GetOptionIfExists("PcLintArguments");
            this.PcLintEnvironment = this.GetOptionIfExists("PcLintEnvironment");

            this.RatsExecutable = this.GetOptionIfExists("RatsExecutable");
            this.RatsArguments = this.GetOptionIfExists("RatsArguments");
            this.RatsEnvironment = this.GetOptionIfExists("RatsEnvironment");

            this.CppCheckExecutable = this.GetOptionIfExists("CppCheckExecutable");
            this.CppCheckArguments = this.GetOptionIfExists("CppCheckArguments");
            this.CppCheckEnvironment = this.GetOptionIfExists("CppCheckEnvironment");

            this.CustomExecutable = this.GetOptionIfExists("CustomExecutable");
            this.CustomArguments = this.GetOptionIfExists("CustomArguments");
            this.CustomKey = this.GetOptionIfExists("CustomKey");
            this.CustomEnvironment = this.GetOptionIfExists("CustomEnvironment");
        }

        /// <summary>The get option control user interface.</summary>
        /// <returns>The <see cref="UserControl" />.</returns>
        public UserControl GetOptionControlUserInterface()
        {
            return this.cxxControl ?? (this.cxxControl = new CxxUserControl(this));
        }

        /// <summary>The refresh data in ui.</summary>
        /// <param name="project">The project.</param>
        /// <param name="helper">The helper.</param>
        public void RefreshDataInUi(Resource project, IConfigurationHelper helper)
        {
            // read properties
            this.SetOptions();

            this.Project = project;

            if (this.Project == null)
            {
                this.ProjectIsAssociated = false;
                return;
            }

            if (string.IsNullOrEmpty(this.Project.Lang))
            {
                this.ProjectIsAssociated = true;
                return;
            }

            this.ProjectIsAssociated = CxxPlugin.IsSupported(this.Project);
        }

        /// <summary>The save data in ui.</summary>
        /// <param name="project">The project.</param>
        /// <param name="helper">The helper.</param>
        public void SaveDataInUi(Resource project, IConfigurationHelper helper)
        {
            this.SaveOption("VeraExecutable", this.VeraExecutable);
            this.SaveOption("VeraArguments", this.VeraArguments);
            this.SaveOption("VeraEnvironment", this.VeraEnvironment);

            this.SaveOption("PcLintExecutable", this.PcLintExecutable);
            this.SaveOption("PcLintArguments", this.PcLintArguments);
            this.SaveOption("PcLintEnvironment", this.PcLintEnvironment);

            this.SaveOption("RatsExecutable", this.RatsExecutable);
            this.SaveOption("RatsArguments", this.RatsArguments);
            this.SaveOption("RatsEnvironment", this.RatsEnvironment);

            this.SaveOption("CppCheckExecutable", this.CppCheckExecutable);
            this.SaveOption("CppCheckArguments", this.CppCheckArguments);
            this.SaveOption("CppCheckEnvironment", this.CppCheckEnvironment);

            this.SaveOption("CustomExecutable", this.CustomExecutable);
            this.SaveOption("CustomArguments", this.CustomArguments);
            this.SaveOption("CustomKey", this.CustomKey);
            this.SaveOption("CustomEnvironment", this.CustomEnvironment);
        }

        /// <summary>The refresh colours.</summary>
        /// <param name="foreground">The foreground.</param>
        /// <param name="background">The background.</param>
        public void RefreshColours(Color foreground, Color background)
        {
            this.BackGroundColor = background;
            this.ForeGroundColor = foreground;
        }

        /// <summary>The get option if exists.</summary>
        /// <param name="key">The key.</param>
        /// <returns>The <see cref="string"/>.</returns>
        private string GetOptionIfExists(string key)
        {
            try
            {
                return
                    this.configuration.ReadSetting(Context.FileAnalysisProperties, "CxxPlugin", key)
                        .Value;
            }
            catch (Exception)
            {
                return string.Empty;
            }
        }

        /// <summary>The save option.</summary>
        /// <param name="key">The key.</param>
        /// <param name="value">The value.</param>
        private void SaveOption(string key, string value)
        {
            this.configuration.WriteOptionInApplicationData(
                Context.FileAnalysisProperties, 
                "CxxPlugin", 
                key, 
                value);
        }

        #endregion
    }
}