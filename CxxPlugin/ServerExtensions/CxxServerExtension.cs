// --------------------------------------------------------------------------------------------------------------------
// <copyright file="CxxServerExtension.cs" company="Copyright © 2013 Tekla Corporation. Tekla is a Trimble Company">
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
namespace CxxPlugin.ServerExtensions
{
    using VSSonarPlugins;

    /// <summary>
    /// The cxx server extension.
    /// </summary>
    public class CxxServerExtension : IServerAnalyserExtension
    {
        /// <summary>
        /// The get resource key.
        /// </summary>
        /// <param name="filePath">
        /// The file path.
        /// </param>
        /// <param name="projectItem">
        /// The project Item.
        /// </param>
        /// <param name="solutionPath">
        /// The solution Path.
        /// </param>
        /// <param name="repoKey">
        /// The repo Key.
        /// </param>
        /// <returns>
        /// The <see cref="string"/>.
        /// </returns>
        public string GetResourceKey(string filePath, VsProjectItem projectItem, string solutionPath, string repoKey)
        {
            var filerelativePath = filePath.Replace(solutionPath + "\\", string.Empty).Replace("\\", "/");
            return repoKey + ":" + filerelativePath;
        }        
    }
}
