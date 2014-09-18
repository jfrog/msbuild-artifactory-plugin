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
	$taskPath = join-path $solutionDirectory '\.artifactory\Deploy.targets'
	
	$taskDoc = New-Object xml
	$taskDoc.psbase.PreserveWhitespace = true
	$taskDoc.Load($taskPath)
	
	$taskDoc.Project.PropertyGroup[0].pluginVersion = $packageVersion
	$taskDoc.LoadXml($taskDoc.OuterXml)
	$taskDoc.Save($taskPath)
}

Function AddImportToNuget()
{
	$nugetPath = join-path $solutionDirectory '\.nuget\NuGet.targets'
	if((Test-Path $nugetPath ))
	{
		write-host "Updating NuGet.targets file... " 
		$nugetDoc = New-Object xml
		$nugetDoc.psbase.PreserveWhitespace = true
		$nugetDoc.Load($nugetPath)

		$resolvePath = '$(solutionDir)' + '\.artifactory\Resolve.targets'

		$child = $nugetDoc.Project.AppendChild($nugetDoc.CreateElement("Import"))
		$child.SetAttribute("Project",$resolvePath);
		$child.SetAttribute("Condition","Exists('$resolvePath')");

		$nugetDoc.LoadXml($nugetDoc.OuterXml.Replace(" xmlns=`"`"", ""))
		$nugetDoc.Save($nugetPath);
	}
}

Function AddImportToProj()
{
	write-host "Updating Project csproj file... " 

	# Grab the loaded MSBuild project for the project
	$msbuildProject = [Microsoft.Build.Evaluation.ProjectCollection]::GlobalProjectCollection.GetLoadedProjects([System.IO.Path]::GetFullPath($project.FullName)) | Select-Object -First 1
	$targetsPath = '$(solutionDir)' + '\.artifactory\Deploy.targets'
	
	# Add the import and save the project
    $msbuildProject.Xml.AddImport($targetsPath) | out-null
	$import = $msbuildProject.Xml.Imports | Where-Object { $_.Project.EndsWith('Deploy.targets') }
	$import.Condition = "Exists('$targetsPath')"

	$project.Save()
}

Function Install()
{
	write-host "Recognize first installation state"
	
	New-Item -ItemType directory -Path $artifactoryDir
	New-Item -ItemType directory -Path $artifactoryDir
	$fileArtifactoryFrom = join-path $rootDir '\artifactory\Artifactory.build'
	$fileArtifactoryTo = join-path $artifactoryDir '\Artifactory.build'
	Copy-Item $fileArtifactoryFrom $artifactoryDir
	
	$fileTaskFrom = join-path $rootDir '\artifactory\Deploy.targets'
	$fileTaskTo = join-path $artifactoryDir '\Deploy.targets'
	Copy-Item $fileTaskFrom $artifactoryDir

	# Updating the current version of the plugin
	dynamicVersioning

	$fileResolveFrom = join-path $rootDir '\artifactory\Resolve.targets'
	$fileResolveTo = join-path $artifactoryDir '\Resolve.targets'
	Copy-Item $fileResolveFrom $artifactoryDir


	$vsProject = $solution.AddSolutionFolder(".artifactory")
	
	$parentSolutionFolder = Get-Interface $vsProject.Object ([EnvDTE80.SolutionFolder])

	$parentProjectFolder = Get-Interface $vsProject.ProjectItems ([EnvDTE.ProjectItems])
	$projectFile = $parentProjectFolder.AddFromFile($fileArtifactoryTo)
	$projectFile = $parentProjectFolder.AddFromFile($fileTaskTo)
	$projectFile = $parentProjectFolder.AddFromFile($fileResolveTo)

	AddImportToNuget

	AddImportToProj
	
} 


Function Update()
{
	write-host "Recognize Update Mode"
	$fileTaskFrom = join-path $rootDir '\artifactory\Deploy.targets'
	$fileTaskTo = join-path $artifactoryDir '\Deploy.targets'
	Copy-Item $fileTaskFrom $artifactoryDir
	
	# Updating the current version of the plugin
	dynamicVersioning

	$fileResolveFrom = join-path $rootDir '\artifactory\Resolve.targets'
	$fileResolveTo = join-path $artifactoryDir '\Resolve.targets'
	Copy-Item $fileResolveFrom $artifactoryDir

	AddImportToNuget

	AddImportToProj
} 

#[System.Windows.Forms.MessageBox]::Show("We are proceeding with next step.") 


# Verifying that we are inside "Update" process.
if((Test-Path $artifactoryDir )){
	
	$taskPath = join-path $solutionDirectory '\.artifactory\Deploy.targets'
	
	$taskDoc = New-Object xml
	$taskDoc.psbase.PreserveWhitespace = true
	$taskDoc.Load($taskPath)
	$currentVersion = $taskDoc.Project.PropertyGroup[0].pluginVersion

	if($packageVersion -gt $currentVersion)
	{
		#[System.Windows.Forms.MessageBox]::Show("Install => Update") 
		$taskDoc.Save($taskPath)
		Update
	}
	else
	{
		#[System.Windows.Forms.MessageBox]::Show("Install => Install") 
		$taskDoc.Save($taskPath)
		Install
	}
}
else{
	#[System.Windows.Forms.MessageBox]::Show("Install => Install") 
	Install
}

write-host "Artifactory Package Install Script ended"