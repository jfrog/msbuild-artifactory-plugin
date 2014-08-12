param($installPath, $toolsPath, $package)

write-host "Artifactory Package Install Script start"

write-host "Copying Artifactory.build to Solution"

$rootDir = (Get-Item $installPath)

$solution = Get-Interface $dte.Solution ([EnvDTE80.Solution2])

$fileFrom = join-path $rootDir '\artifactory\artifactory.build'

Copy-Item $fileFrom $solutionPath

write-host "Updating NuGet.targets file..."

$nugetPath = join-path $(SolutionDir) '\.nuget\NuGet.targets'

$doc = [System.Xml.XmlDocument](Get-Content $nugetPath); 

$child = $doc.Project.AppendChild($doc.CreateElement("Import"))
$child.SetAttribute("Project","$installPath\artifactory\resolve.targets");
$child.SetAttribute("Condition","Exists('$installPath\artifactory\resolve.targets')");

$doc = [xml] $doc.OuterXml.Replace(" xmlns=`"`"", "")
$doc.Save($nugetPath);

write-host "Artifactory Package Install Script end"

