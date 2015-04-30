using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.VisualStudio.TemplateWizard;
using EnvDTE80;
using System.Windows.Forms;
using System.IO;
using EnvDTE;
using System.Xml;
using VSIXProjectArtifactory;
using Microsoft.Build.Construction;

namespace ArtifactoryWizard
{   

    class InitScriptWizard : IWizard 
    {
        private EnvDTE80.DTE2 _dte;
        EnvDTE.Project _project;
        WizardRunKind _runKind;

        //private UserInputForm inputForm;
        //private string customMessage;
        string _destinationDirectory;
        string _solutionDir;

        public void RunStarted(object automationObject,
          Dictionary<string, string> replacementsDictionary,
          WizardRunKind runKind, object[] customParams)
        {
            _dte = automationObject as EnvDTE80.DTE2;
            _runKind = runKind;

            if(runKind.HasFlag(WizardRunKind.AsNewProject))
            {
                _solutionDir = replacementsDictionary["$solutiondirectory$"];
                _destinationDirectory = replacementsDictionary["$destinationdirectory$"];           
            }         

            try
            {
                // Display a form to the user. The form collects 
                // input for the custom message.
                //inputForm = new UserInputForm();
                //inputForm.ShowDialog();

                //customMessage = inputForm.get_CustomMessage();

                //// Add custom parameters.
                //replacementsDictionary.Add("$custommessage$",
                //    customMessage);
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.ToString());
            }
            //Console.Write("Wizard!!!");
        }


        public void BeforeOpeningFile(EnvDTE.ProjectItem projectItem)
        {
            //throw new NotImplementedException();
        }

        public void ProjectFinishedGenerating(EnvDTE.Project project)
        {
            _project = project;
            var p = Microsoft.Build.Evaluation.ProjectCollection.GlobalProjectCollection.GetLoadedProjects(_project.FullName).ElementAt(0);
            String artifactoryConfLocation = Path.Combine(project.FullName, "..\\.artifactory");
            System.IO.Directory.CreateDirectory(Path.Combine(_solutionDir, ".artifactory"));
            Project folder = ((Solution2)_dte.Solution).AddSolutionFolder(".artifactory");

            File.Copy(Path.Combine(artifactoryConfLocation, @"Artifactory.build"), Path.Combine(_solutionDir, @".artifactory\Artifactory.build"));
            folder.ProjectItems.AddFromFile(Path.Combine(_solutionDir, @".artifactory\Artifactory.build"));

            File.Copy(Path.Combine(artifactoryConfLocation, "Deploy.targets"), Path.Combine(_solutionDir, @".artifactory\Deploy.targets"));
            folder.ProjectItems.AddFromFile(Path.Combine(_solutionDir, @".artifactory\Deploy.targets"));

            File.Copy(Path.Combine(artifactoryConfLocation, "Resolve.targets"), Path.Combine(_solutionDir, @".artifactory\Resolve.targets"));
            folder.ProjectItems.AddFromFile(Path.Combine(_solutionDir, @".artifactory\Resolve.targets"));

            Directory.Delete(artifactoryConfLocation, true);

            foreach(ProjectItem pi in project.ProjectItems)
            {
                if(pi.Name.Equals(".artifactory"))
                {
                    pi.Delete();
                }
            }
        }

        public void ProjectItemFinishedGenerating(EnvDTE.ProjectItem projectItem)
        {
            //throw new NotImplementedException();
        }

        public void RunFinished()
        {
            UpdatePackagesfolderLocation();    

            AddImportToNuget();

            //AddImportToProj();
           
            //throw new NotImplementedException();
        }

        public bool ShouldAddProjectItem(string filePath)
        {
            return true;
        }

        public void projectRun() 
        {
        
        
        }

        //Override the packages.config file with limitation on the Artifactory nuget version "allowedVersions"
        private void AddNugetVersionRange() 
        {
            String packagesConfig = _project.FullName + "\\..\\packages.config";
            XmlDocument xmlDoc = new XmlDocument();
            xmlDoc.Load(packagesConfig);
            XmlNodeList matches = xmlDoc.SelectNodes("packages/package[@id='Artifactory']");
            XmlAttribute attr = xmlDoc.CreateAttribute("allowedVersions");
            attr.Value = "[1.1.0,)";
            matches.Item(0).Attributes.Append(attr);
            xmlDoc.Save(packagesConfig);      
        }

        private void UpdatePackagesfolderLocation() 
        {
            var p = Microsoft.Build.Evaluation.ProjectCollection.GlobalProjectCollection.GetLoadedProjects(_project.FullName).ElementAt(0);
            string packagesFolderLocation = "$(solutionDir)";
            foreach (var item in p.GetItems("Reference"))
            {
                if (item.EvaluatedInclude.Equals("JFrog.Artifactory"))
                {
                    if (item.Metadata.Count > 0)
                    {
                        var m = item.Metadata.FirstOrDefault(x => x.Name.Contains("HintPath"));
                        if (m != null)
                        {
                            packagesFolderLocation = m.EvaluatedValue.Substring(0, m.EvaluatedValue.IndexOf("packages"));
                        }
                    }
                }

                string deployTargetsFile = Path.Combine(_solutionDir, @".artifactory\Deploy.targets");
                XmlDocument xmlDoc = new XmlDocument();
                xmlDoc.Load(deployTargetsFile);

                for (int i = 0; xmlDoc.FirstChild.ChildNodes.Count > i; i++)
                {
                    var child = xmlDoc.FirstChild.ChildNodes.Item(i);
                    if (child.Name.Equals("PropertyGroup") && child.FirstChild.Name.Equals("NugetPackagesLocation"))
                    {
                        child.FirstChild.InnerText = packagesFolderLocation;
                        break;
                    }
                }
                xmlDoc.Save(deployTargetsFile);
            }   
        }

        private void AddImportToNuget() 
        {
            string nugetPath = _solutionDir + @".nuget\NuGet.targets";
            if (File.Exists(nugetPath)) 
            {
                string resolvePath = @"$(solutionDir)\.artifactory\Resolve.targets";
                XmlDocument xmlDoc = new XmlDocument();
                xmlDoc.Load(nugetPath);
                XmlElement element = xmlDoc.CreateElement("Import");

                XmlAttribute projectAttr = xmlDoc.CreateAttribute("Project");
                projectAttr.Value = resolvePath;

                XmlAttribute conditionAttr = xmlDoc.CreateAttribute("Condition");
                conditionAttr.Value = "Exists('" + resolvePath + "')";

                element.Attributes.Append(projectAttr);
                element.Attributes.Append(conditionAttr);

                xmlDoc.LastChild.AppendChild(element); 

                xmlDoc.LoadXml(xmlDoc.OuterXml.Replace(" xmlns=\"\"", ""));
                xmlDoc.Save(nugetPath);
            }
        }

        /// <summary>
        /// Add MSBuild Import to the Artifactory project
        /// </summary>
        private void AddImportToProj() 
        {
            var p = Microsoft.Build.Evaluation.ProjectCollection.GlobalProjectCollection.GetLoadedProjects(_project.FullName).ElementAt(0);

            string targetsPath = @"$(solutionDir)\.artifactory\Deploy.targets";
            ProjectImportElement import = p.Xml.AddImport(targetsPath);
            import.Condition = "Exists('" + targetsPath + "')";
            _project.Save();
        }
    }
}
