// --------------------------------------------------------------------------------------------------------------------
// <copyright file="TestParsingOfSensors.cs" company="Copyright © 2013 Tekla Corporation. Tekla is a Trimble Company">
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

using VSSonarPlugins;

namespace CxxPlugin.Test
{
    using System;
    using System.Collections.Generic;
    using System.IO;

    using global::CxxPlugin.LocalExtensions;

    using Moq;

    using NUnit.Framework;

    /// <summary>
    /// The test server extension.
    /// </summary>
    [TestFixture]
    public class TestParsingOfSensors
    {
        /// <summary>
        /// The sample data path.
        /// </summary>
        private readonly string sampleDataPath = Path.Combine(Environment.CurrentDirectory, "TestData\\Sensors");

        /// <summary>
        /// The options.
        /// </summary>
        private Dictionary<string, string> options = new Dictionary<string, string>();

        /// <summary>
        /// The set up.
        /// </summary>
        [SetUp]
        public void SetUp()
        {
            this.options.Add("CustomKey", "cpplint");
            this.options.Add("CustomArguments", "cpplint_mod.py --output=vs7");
            this.options.Add("CustomExecutable", "python.exe");
            this.options.Add("CustomEnvironment", string.Empty);
            this.options.Add("CppCheckEnvironment", string.Empty);
            this.options.Add("CppCheckArguments", "--inline-suppr --enable=all --xml -D__cplusplus -DNT");
            this.options.Add("CppCheckExecutable", "cppcheck.exe");
            this.options.Add("RatsEnvironment", string.Empty);
            this.options.Add("RatsArguments", "-nodup -showrules");
            this.options.Add("RatsExecutable", "rats.exe");
            this.options.Add("VeraEnvironment", "VERA_ROOT=lib\\\vera++");
            this.options.Add("VeraArguments", "-nodup -showrules");
            this.options.Add("VeraExecutable", "vera++.exe");
        }

        [TearDown]
        public void TearDown()
        {
            this.options.Clear();
        }

        /// <summary>
        /// The test cpp check execution.
        /// </summary>
        [Test]
        public void TestCppCheckGetViolations()
        {
            var lines = new List<string>(File.ReadAllLines(Path.Combine(this.sampleDataPath, "cppcheck-result-0.xml")));
            var serviceStub = new Mock<ICommandExecution>();
            var pluginOptions = new Mock<IPluginsOptions>();
            pluginOptions.Setup(control => control.GetOptions()).Returns(this.options);
            var cppcheckSensor = new CppCheckSensor(serviceStub.Object, pluginOptions.Object);
            var violations = cppcheckSensor.GetViolations(lines);
            Assert.IsNotNull(violations);
            Assert.AreEqual(9, violations.Count);
        }

        /// <summary>
        /// The test cpp check execution.
        /// </summary>
        [Test]
        public void TestCpplintGetViolations()
        {
            var lines = new List<string>(File.ReadAllLines(Path.Combine(this.sampleDataPath, "cpplint-result-0.txt")));
            var serviceStub = new Mock<ICommandExecution>();
            var pluginOptions = new Mock<IPluginsOptions>();
            pluginOptions.Setup(control => control.GetOptions()).Returns(this.options);
            var sensor = new CxxExternalSensor(serviceStub.Object, pluginOptions.Object);
            var violations = sensor.GetViolations(lines);
            Assert.IsNotNull(violations);
            Assert.AreEqual(11, violations.Count);
            Assert.AreEqual("cxxexternal.cpplint.whitespace/comments-0", violations[0].Rule);
            Assert.AreEqual("Warning : At least two spaces is best between code and comments", violations[0].Message);
        }

        /// <summary>
        /// The test cpp check execution.
        /// </summary>
        [Test]
        public void TestRatsGetViolations()
        {
            var lines = new List<string>(File.ReadAllLines(Path.Combine(this.sampleDataPath, "rats-result-0.xml")));
            var serviceStub = new Mock<ICommandExecution>();
            var pluginOptions = new Mock<IPluginsOptions>();
            pluginOptions.Setup(control => control.GetOptions()).Returns(this.options);
            var ratsSensor = new RatsSensor(serviceStub.Object, pluginOptions.Object);
            var violations = ratsSensor.GetViolations(new List<string>(lines));
            Assert.IsNotNull(violations);
            Assert.AreEqual(35, violations.Count);
        }

        /// <summary>
        /// The test cpp check execution.
        /// </summary>
        [Test]
        public void TestVeraGetViolations()
        {
            var lines = new List<string>(File.ReadAllLines(Path.Combine(this.sampleDataPath, "vera++-result-0.txt")));
            var serviceStub = new Mock<ICommandExecution>();
            var pluginOptions = new Mock<IPluginsOptions>();
            pluginOptions.Setup(control => control.GetOptions()).Returns(this.options);
            var sensor = new VeraSensor(serviceStub.Object, pluginOptions.Object);
            var violations = sensor.GetViolations(lines);
            Assert.IsNotNull(violations);
            Assert.AreEqual(39, violations.Count);
        }
    }
}
