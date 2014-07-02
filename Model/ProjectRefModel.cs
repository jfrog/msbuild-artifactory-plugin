using System;
using System.Collections.Generic;
using Microsoft.Build.Evaluation;

namespace JFrog.Artifactory.Model
{
    public class ProjectRefModel
    {
        public string AssemblyName { get; set; }
        public List<ProjectMetadata> LstProjectMetadata { get; set; }

    }
}
