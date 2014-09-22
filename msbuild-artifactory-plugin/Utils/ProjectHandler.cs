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
    /// Represent Project in the Solution.
    /// </summary>
    public class ProjectHandler
    {
        private string ProjectName { get; set; }
        private string ProjectDirectory { get; set; }
        public ArtifactoryConfig ArtifactoryConfiguration { get; set; }
        private List<Property> DefaultProperties { get; set; }

        public ProjectHandler(string projectName, string projectDirectory)
        {
            ProjectName = projectName;
            ProjectDirectory = projectDirectory;
        }

        /// <summary>
        /// create project model and return its references for nuget, local and other projects
        /// </summary>
       
        public ProjectModel generate()
        {
                ProjectModel projectRefModel = new ProjectModel();

                if (ArtifactoryConfiguration != null) 
                {
                    projectRefModel.artifactoryDeploy = new List<ProjectModel.DeployAttribute>();

                    var result = ArtifactoryConfiguration.PropertyGroup.Deployments.Deploy.Select(attr => new ProjectModel.DeployAttribute()
                    {
                        InputPattern = (attr.InputPattern != null ? attr.InputPattern : string.Empty),
                        OutputPattern = (attr.OutputPattern != null ? attr.OutputPattern : string.Empty),
                        properties = convertProperties(attr.Properties)
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
            FileInfo artifactoryConfigurationFile = new FileInfo(projectArtifactoryConfigPath + "Artifactory.build");
            if (artifactoryConfigurationFile.Exists)
            {
                System.Xml.Serialization.XmlSerializer reader = new System.Xml.Serialization.XmlSerializer(typeof(ArtifactoryConfig));
                System.IO.StreamReader file = new System.IO.StreamReader(artifactoryConfigurationFile.FullName);
                ArtifactoryConfiguration = (ArtifactoryConfig)reader.Deserialize(file);

                //Validate the xml file
                if (ArtifactoryConfiguration.PropertyGroup == null || ArtifactoryConfiguration.PropertyGroup.Deployments == null ||
                    ArtifactoryConfiguration.PropertyGroup.Deployments.Deploy == null)
                {
                    return false;
                }

                return true;
            }

            return false;
        }

        private List<KeyValuePair<string, string>> convertProperties(Properties customProperties) 
        {
            List<KeyValuePair<string, string>> result = new List<KeyValuePair<string, string>>();
            if (customProperties != null)
            {               
                customProperties.Property.ForEach(prop => result.Add(new KeyValuePair<string, string>(prop.key, prop.val)));
            }
            
            return result;
        }
    }
}
