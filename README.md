# MSBuild Artifactory Plugin

## Deprecation Notice
From version 2017 of Visual Studio, the MSBuild Artifactory Plugin is no longer supported.

To enable the continued use of MSBuild with Artifactory, JFrog has developed an alternative solution which can be found in [JFrog's GitHub repository](https://github.com/JFrogDev/project-examples/tree/master/msbuild-example) where it is, also, fully documented.

The current MSBuild Artifactory Plugin is now deprecated and we recommend using the new solution with all versions of Visual Studio.

## Overview
Artifactory brings Continuous Integration to MSBuild, TFS and Visual Studio through the MSBuild Artifactory Plugin. This allows you to capture information about deployed artifacts, resolve Nuget dependencies and environment data associated with MSBuild build runs, and deploy artifacts to Artifactory. In addition, the exhaustive build information captured by Artifactory enables fully traceable builds.

## Installation and usage instructions
The full MSBuild Artifactory Plugin documentation is available [here](https://www.jfrog.com/confluence/display/RTF/MSBuild+Artifactory+Plugin).

## Pull requests
We welcome community contribution through pull requests.

## Debugging the msbuild-artifactory-plugin code
### General
If you'd like to help us improve the plugin through code contributions, this section describes how to debug the code of the  project.

### Debug execution flow
To debug the msbuild-artifactory-plugin code, we need to use an additional dummy Visual Studio project.
Here is the debug execution flow:

* From Visual Studio, run "Debug" for the msbuild-artifactory-plugin project.
* The msbuild-artifactory-plugin project invokes the build of the dummy project.
* The dummy project uses the msbuild-artifactory-plugin dll, which we debug using the msbuild-artifactory-plugin sources.

### Setting up the debug environment:
Follow these steps to debug the plugin code.

* Clone this Github repository.
* Open the msbuild-artifactory-plugin solution in Visual Studio by opening the msbuild-artifactory-plugin.sln file.
* Build the solution.
* Locate the JFrog.Artifactory.dll file created by the build.
* Create a new dummy solution in Visual Studio.
* Create a dummy project in this solution.
* Add the Artifactory template to the dummy solution as described in the MSBuild Artifactory Plugin documentation.
* The next step is to have this project reference the JFrog.Artifactory.dll you located in step 4. To do this, you need to change the AssemblyFile path of the JFrog.Artifactory.ArtifactoryBuild task in the Deploy.targets file. For example:
```
<UsingTask TaskName="JFrog.Artifactory.ArtifactoryBuild" AssemblyFile="%MS_Build_Project_Path%\msbuild-artifactory-plugin\lib```\JFrog.Artifactory.dll" />
```
* Locate the path to the MSBuild.exe file. You can find it in the Visual Studio console printouts when building the solution.
* Go back the msbuild-artifactory-plugin solution.
* Right click on the msbuild-artifactory-plugin and select Properties.
* Go to the Debug section.
* Change the start action to: “Start external program” and set the full path to the MSBuild.exe.

