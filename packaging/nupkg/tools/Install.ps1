param($installPath, $toolsPath, $package, $project)

write-host "Artifactory Package Install Script starting"

$rootDir = (Get-Item $installPath)
$solution = Get-Interface $dte.Solution ([EnvDTE80.Solution2])
$solutionDirectory = Split-Path -parent $solution.FileName
$artifactoryDir = join-path $solutionDirectory '\.artifactory'
$artifactoryTargetsDir = join-path $artifactoryDir '\targets'
$packageVersion = $package.version.toString()

Function dynamicVersioning(){
	# Updating the current version of the plugin
	$taskPath = join-path $solutionDirectory '\.artifactory\targets\artifactory.targets'
	
	$taskDoc = New-Object xml
	$taskDoc.psbase.PreserveWhitespace = true
	$taskDoc.Load($taskPath)
	
	$taskDoc.Project.PropertyGroup[0].pluginVersion = $packageVersion
	$taskDoc.LoadXml($taskDoc.OuterXml)
	$taskDoc.Save($taskPath)
}

Function Install()
{
	write-host "Recognize first installation state"
	
	New-Item -ItemType directory -Path $artifactoryDir
	New-Item -ItemType directory -Path $artifactoryTargetsDir
	$fileArtifactoryFrom = join-path $rootDir '\artifactory\artifactory.build'
	$fileArtifactoryTo = join-path $artifactoryDir '\artifactory.build'
	Copy-Item $fileArtifactoryFrom $artifactoryDir
	
	$fileTaskFrom = join-path $rootDir '\artifactory\artifactory.targets'
	$fileTaskTo = join-path $artifactoryTargetsDir '\artifactory.targets'
	Copy-Item $fileTaskFrom $artifactoryTargetsDir

	# Updating the current version of the plugin
	dynamicVersioning

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

	# Grab the loaded MSBuild project for the project
	$msbuildProject = [Microsoft.Build.Evaluation.ProjectCollection]::GlobalProjectCollection.GetLoadedProjects([System.IO.Path]::GetFullPath($project.FullName)) | Select-Object -First 1
	$targetsPath = '$(solutionDir)' + '\.artifactory\targets\artifactory.targets'
	
	# Add the import and save the project
    $msbuildProject.Xml.AddImport($targetsPath) | out-null

	$project.Save()
} 


Function Update()
{
	write-host "Recognize Update Mode"
	$fileTaskFrom = join-path $rootDir '\artifactory\artifactory.targets'
	$fileTaskTo = join-path $artifactoryTargetsDir '\artifactory.targets'
	Copy-Item $fileTaskFrom $artifactoryTargetsDir
	
	# Updating the current version of the plugin
	dynamicVersioning

	$fileResolveFrom = join-path $rootDir '\artifactory\resolve.targets'
	$fileResolveTo = join-path $artifactoryTargetsDir '\resolve.targets'
	Copy-Item $fileResolveFrom $artifactoryTargetsDir
} 

#[System.Windows.Forms.MessageBox]::Show("We are proceeding with next step.") 

if((Test-Path $artifactoryDir )){
	Update
}
else{
	Install
}

write-host "Artifactory Package Install Script ended"



