﻿// --------------------------------------------------------------------------------------------------------------------
// <copyright file="TestParsingOfSensors.cs" company="Copyright © 2014 jmecsoftware">
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

using VSSonarPlugins;

namespace CxxPlugin.Test
{
    using System;
    using System.Collections.Generic;
    using System.IO;

    using global::CxxPlugin.Commands;
    using global::CxxPlugin.LocalExtensions;

    using Microsoft.FSharp.Collections;

    using Moq;

    using NUnit.Framework;

    public static class Interop
    {
        public static FSharpList<T> ToFSharpList<T>(this IList<T> input)
        {
            return CreateFSharpList(input, 0);
        }

        private static FSharpList<T> CreateFSharpList<T>(IList<T> input, int index)
        {
            if (index >= input.Count)
            {
                return FSharpList<T>.Empty;
            }
            else
            {
                return FSharpList<T>.Cons(input[index], CreateFSharpList(input, index + 1));
            }
        }
    }

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
    }
}
