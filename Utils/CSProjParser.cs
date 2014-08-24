using JFrog.Artifactory.Model;
using Microsoft.Build.Evaluation;
using Microsoft.Build.Framework;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Xml.Linq;

namespace JFrog.Artifactory.Utils
{
    /// <summary>
    /// extract project metadata from a csporj file.
    /// the metadata extracted is only for "refrence"
    /// </summary>
    public class CSProjParser
    {
        private string ProjectName { get; set; }
        private string ProjectArtifactoryConfigPath { get; set; }
        private string ProjectDirectory { get; set; }
        private XDocument ArtifactoryConfiguration { get; set; }

        //default cotur.
        public CSProjParser(string projectName, string projectArtifactoryConfigPath, string projectDirectory)
        {
            ProjectName = projectName;
            ProjectArtifactoryConfigPath = projectArtifactoryConfigPath;
            ProjectDirectory = projectDirectory;
        }

        /// <summary>
        /// parse a csproj file and return its refrences for uget, local and other projects
        /// </summary>
        /// <returns>list of MetaData items</returns>
        public ProjectRefModel Parse()
        {
            try
            {
                ProjectRefModel projectRefModel = new ProjectRefModel();
                FileInfo artifactoryConfigurationFile = new FileInfo(ProjectArtifactoryConfigPath + "artifactory.build");
                if (artifactoryConfigurationFile.Exists) 
                {
                    ArtifactoryConfiguration = XDocument.Load(artifactoryConfigurationFile.FullName, LoadOptions.None);
                 
                    var patterns = ArtifactoryConfiguration.Root.Descendants().
                        Where(tag => tag.Name.LocalName == "DeployAttribute").Select(pattern => pattern.Descendants());
                           
                    projectRefModel.artifactoryDeploy = new List<ProjectRefModel.DeployAttribute>();
                    foreach (var p in patterns)
                    {
                        ProjectRefModel.DeployAttribute deployAttribute = new ProjectRefModel.DeployAttribute();
                        var pattern = p.FirstOrDefault(a => a.Name.LocalName == "pattern");
                        deployAttribute.Pattern = (pattern != null ? pattern.Value : string.Empty);

                        var properties = p.FirstOrDefault(a => a.Name.LocalName == "properties");
                        deployAttribute.properties = (properties != null ? properties.Value : string.Empty);

                        projectRefModel.artifactoryDeploy.Add(deployAttribute);
                    }                  
                }

                //var project = new Project(ProjectCsprojPath);

                projectRefModel.AssemblyName = ProjectName;
                projectRefModel.projectDirectory = ProjectDirectory;

                return projectRefModel;

            }
            catch (Exception ex)
            {
                //TODO handle this exception - do not hide...
                return null;
            }
           

        }
        
    }
}
