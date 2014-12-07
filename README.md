CxxPlugin
=========

CxxPlugin - Plugin for VSSonarQubeExtension - Supports Cxx community plugin

### License
This program is free software; you can redistribute it and/or modify it under the terms of the GNU Lesser General Public License as published by the Free Software Foundation; either version 3 of the License, or (at your option) any later version.

This program is distributed in the hope that it will be useful, but WITHOUT ANY WARRANTY; without even the implied warranty of MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the GNU Lesser General Public License for more details. You should have received a copy of the GNU Lesser General Public License along with this program; if not, write to the Free Software Foundation, Inc., 59 Temple Place, Suite 330, Boston, MA 02111-1307 USA


## How to compile
Use VS2012 and just build, it will produce CxxPlugin.dll.

## Installation

Copy into Extensions Folder of Visual Studio. For Example:
$(ProFile)\AppData\Local\Microsoft\Visual Studio\11\Extensions

## Usage
The plugin supports:
* Vera, Rats, CppCheck, PcLint, a external configured Tool and Sonar Analysis

![Image](../master/wiki/VeraConfig.png?raw=true)

To configure the plugin, locate the binaries on disk and configure any arguments and environments that the tools will use while they execute. 

**Please ensure that Java, sonar runner or maven are correctly located.**

## Local Analysis
To run local analysis, a valid sonar project needs to be available in the working directory. Additional properties can be set in the options, these will be passed to as command arguments so they will overwrite the ones defined in the project file



