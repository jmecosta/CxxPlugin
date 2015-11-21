CxxPlugin [![Build status](https://ci.appveyor.com/api/projects/status/137dth8cg4jupjpm?svg=true)](https://ci.appveyor.com/project/jorgecosta/cxxplugin-421)
=========

CxxPlugin - Plugin for VSSonarQubeExtension - Supports Cxx community plugin

### License
This program is free software; you can redistribute it and/or modify it under the terms of the GNU Lesser General Public License as published by the Free Software Foundation; either version 3 of the License, or (at your option) any later version.

This program is distributed in the hope that it will be useful, but WITHOUT ANY WARRANTY; without even the implied warranty of MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the GNU Lesser General Public License for more details. You should have received a copy of the GNU Lesser General Public License along with this program; if not, write to the Free Software Foundation, Inc., 59 Temple Place, Suite 330, Boston, MA 02111-1307 USA


## How to compile
Use any visual studio version higher than 2010 and just build, it will produce CxxPlugin.VSQ in the BuildDrop.

## Installation
Use the vssonarextension plugin manager to install the .VSQ

## Configuration
The plugin supports:
* Vera, Rats, CppCheck, PcLint, a external configured Tool for local file analysis

![Image](../master/wiki/VeraConfig.png?raw=true)

To configure the plugin, locate the binaries on disk and configure any arguments and environments that the tools will use while they execute.

The environment variable is with format:
KEY1=VALUE1;KEY2=VALUE2 

### Custom external tool
In the figure above, cpplint is configured in order to generate issues during local file analysis. Here you can do whatever you want, you can create your own script to run analysis. the only requirement is that the script or tool will  report issues to standard output in visual studio compatible format. 

The key value in the external tool is crucial to map the ids of the tool with rules in sonar. In cpplint above the fianal key will be other.cpplint.RuleKey. So cpplint is added to the Key property

## Analysis modes provided
The tools configured in this plugin are used during File analysis and in the local analyses mode: Incremental and preview. The VSSonarExtension will call each of this tools with the file and report issues against it.


