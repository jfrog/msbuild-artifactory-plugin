using System;
using System.Collections.Generic;
using Microsoft.Build.Evaluation;
using System.Xml.Linq;

namespace JFrog.Artifactory.Model
{
    public class ProjectRefModel
    {
        public string AssemblyName { get; set; }
        public string projectDirectory { get; set; }
        //public XDocument ArtifactoryConfigurationXml { get; set; }
        public List<ProjectMetadata> LstProjectMetadata { get; set; }
        public List<DeployAttribute> artifactoryDeploy { get; set; }
       // public List<string> Pattern { get; set; }
        //public List<string> properties { get; set; } 

        public class DeployAttribute
        {
            public string Pattern { get; set; }
            public string properties { get; set; }
        }
    }
}
