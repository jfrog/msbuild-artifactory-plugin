using System;
using System.Collections.Generic;
using Microsoft.Build.Evaluation;
using System.Xml.Linq;
using System.Text;
using System.Net;

namespace JFrog.Artifactory.Model
{
    public class ProjectModel
    {
        public string AssemblyName { get; set; }
        public string projectDirectory { get; set; }
        public List<ProjectMetadata> LstProjectMetadata { get; set; }
        public List<DeployAttribute> artifactoryDeploy { get; set; }

        public class DeployAttribute
        {
            public string InputPattern { get; set; }
            public string OutputPattern { get; set; }
            public List<KeyValuePair<string, string>> properties { get; set; }
        }
    }
}
