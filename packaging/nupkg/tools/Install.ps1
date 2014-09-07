param($installPath, $toolsPath, $package, $project)

write-host "Artifactory Package Install Script start"

write-host "Copying Artifactory configuration files to Solution"

$rootDir = (Get-Item $installPath)
$solution = Get-Interface $dte.Solution ([EnvDTE80.Solution2])
$solutionDirectory = Split-Path -parent $solution.FileName
$artifactoryDir = join-path $solutionDirectory '\.artifactory'
$artifactoryTargetsDir = join-path $artifactoryDir '\targets'

#In case the plugin is already installed, for know, we are not overriding the user files. 
if((Test-Path $artifactoryDir )){
    #[System.Windows.Forms.MessageBox]::Show("We are proceeding with next step.") 
	return
}

New-Item -ItemType directory -Path $artifactoryDir
New-Item -ItemType directory -Path $artifactoryTargetsDir

$fileArtifactoryFrom = join-path $rootDir '\artifactory\artifactory.build'
$fileArtifactoryTo = join-path $artifactoryDir '\artifactory.build'
Copy-Item $fileArtifactoryFrom $artifactoryDir

$fileTaskFrom = join-path $rootDir '\artifactory\artifactory.targets'
$fileTaskTo = join-path $artifactoryTargetsDir '\artifactory.targets'
Copy-Item $fileTaskFrom $artifactoryTargetsDir

$fileResolveFrom = join-path $rootDir '\artifactory\resolve.targets'
$fileResolveTo = join-path $artifactoryTargetsDir '\resolve.targets'
Copy-Item $fileResolveFrom $artifactoryTargetsDir


$vsProject = $solution.AddSolutionFolder(".artifactory")
$parentSolutionFolder = Get-Interface $vsProject.Object ([EnvDTE80.SolutionFolder])

$parentProjectFolder = Get-Interface $vsProject.ProjectItems ([EnvDTE.ProjectItems])
$projectFile = $parentProjectFolder.AddFromFile($fileArtifactoryTo)

$childSolution = $parentSolutionFolder.AddSolutionFolder("targets")
$childSolutionFolder = Get-Interface $childSolution.Object ([EnvDTE80.SolutionFolder])
$childProjectFolder = Get-Interface $childSolution.ProjectItems ([EnvDTE.ProjectItems])

$projectFile = $childProjectFolder.AddFromFile($fileTaskTo)
$projectFile = $childProjectFolder.AddFromFile($fileResolveTo)
write-host "Updating NuGet.targets file... " 

$nugetPath = join-path $solutionDirectory '\.nuget\NuGet.targets'

$nugetDoc = New-Object xml
$nugetDoc.psbase.PreserveWhitespace = true
$nugetDoc.Load($nugetPath)

$resolvePath = '$(solutionDir)' + '\.artifactory\targets\resolve.targets'

$child = $nugetDoc.Project.AppendChild($nugetDoc.CreateElement("Import"))
$child.SetAttribute("Project",$resolvePath);
$child.SetAttribute("Condition","Exists('$resolvePath')");

$nugetDoc.LoadXml($nugetDoc.OuterXml.Replace(" xmlns=`"`"", ""))
$nugetDoc.Save($nugetPath);

write-host "Updating Project csproj file... " 
$project.Save()
$projectDoc = New-Object xml
$projectDoc.psbase.PreserveWhitespace = true
$projectDoc.Load($project.FileName)

$resolvePath = '$(solutionDir)' + '\.artifactory\targets\Artifactory.targets'

$child = $projectDoc.Project.AppendChild($projectDoc.CreateElement("Import"))
$child.SetAttribute("Project",$resolvePath);
$child.SetAttribute("Condition","Exists('$resolvePath')");

$projectDoc.LoadXml($projectDoc.OuterXml.Replace(" xmlns=`"`"", ""))
$projectDoc.Save($project.FileName);
$project.Save()
write-host "Artifactory Package Install Script end"



