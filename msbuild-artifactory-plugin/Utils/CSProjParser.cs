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
    /// </summary>
    public class CSProjParser
    {
        private string ProjectName { get; set; }
        private string ProjectDirectory { get; set; }
        private ArtifactoryConfig ArtifactoryConfiguration { get; set; }

        //default cotur.
        public CSProjParser(string projectName, string projectDirectory)
        {
            ProjectName = projectName;
            ProjectDirectory = projectDirectory;
        }

        /// <summary>
        /// parse a csproj file and return its refrences for uget, local and other projects
        /// </summary>
        /// <returns>list of MetaData items</returns>
        public ProjectRefModel Parse()
        {
                ProjectRefModel projectRefModel = new ProjectRefModel();

                if (ArtifactoryConfiguration != null) 
                {
                    projectRefModel.artifactoryDeploy = new List<ProjectRefModel.DeployAttribute>();

                    var result = ArtifactoryConfiguration.PropertyGroup.ArtifactoryDeploy.DeployAttribute.Select(attr => new ProjectRefModel.DeployAttribute()
                    {
                        InputPattern = (attr.Input != null ? attr.Input : string.Empty),
                        OutputPattern = (attr.Output != null ? attr.Output : string.Empty),
                        properties = ProjectRefModel.buildMatrixParamsString(attr.Properties.Property)
                    });

                    projectRefModel.artifactoryDeploy.AddRange(result);
                }

                projectRefModel.AssemblyName = ProjectName;
                projectRefModel.projectDirectory = ProjectDirectory;

                return projectRefModel;           
        }

        /*
         * Trying to read the "artifactory.build" file for deployment configuration.
         */
        public bool parseArtifactoryConfigFile(string projectArtifactoryConfigPath)
        {
            FileInfo artifactoryConfigurationFile = new FileInfo(projectArtifactoryConfigPath + "artifactory.build");
            if (artifactoryConfigurationFile.Exists)
            {
                System.Xml.Serialization.XmlSerializer reader = new System.Xml.Serialization.XmlSerializer(typeof(ArtifactoryConfig));
                System.IO.StreamReader file = new System.IO.StreamReader(artifactoryConfigurationFile.FullName);
                ArtifactoryConfiguration = (ArtifactoryConfig)reader.Deserialize(file);

                return true;
            }

            return false;
        }       
    }
}
