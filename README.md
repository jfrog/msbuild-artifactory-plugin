# MSBuild Artifactory Plugin

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

1. From Visual Studio, run "Debug" for the msbuild-artifactory-plugin project.
2. The msbuild-artifactory-plugin project invokes the build of the dummy project.
3. The dummy project uses the msbuild-artifactory-plugin dll, which we debug using the msbuild-artifactory-plugin sources.

### Setting up the debug environment:
Follow these steps to debug the plugin code.

1. Clone this Github repository.
2. Open the msbuild-artifactory-plugin solution in Visual Studio by opening the msbuild-artifactory-plugin.sln file.
3  Build the solution.
4. Locate the JFrog.Artifactory.dll file created by the build.
5. Create a new dummy solution in Visual Studio.
6. Create a dummy project in this solution.
7. Add the Artifactory template to the dummy solution as described in the MSBuild Artifactory Plugin documentation.
8. The next step is to have this project reference the JFrog.Artifactory.dll you located in step 4. To do this, you need to change the AssemblyFile path of the JFrog.Artifactory.ArtifactoryBuild task in the Deploy.targets file. 
For example:
<UsingTask TaskName="JFrog.Artifactory.ArtifactoryBuild" AssemblyFile="%MS_Build_Project_Path%\msbuild-artifactory-plugin\lib\JFrog.Artifactory.dll" />
9. Locate the path to the MSBuild.exe file. You can find it in the Visual Studio console printouts when building the solution.
10. Go back the msbuild-artifactory-plugin solution.
11. Right click on the msbuild-artifactory-plugin and select Properties.
12. Go to the Debug section.
13. Change the start action to: “Start external program” and set the full path to the MSBuild.exe.

