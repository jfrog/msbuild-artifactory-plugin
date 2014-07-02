using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace JFrog.Artifactory.Model
{
    /// <summary>
    /// Artifactory MsBuild info model 
    /// </summary>
    public class BuildInfoModel
    {
        /// <summary>
        /// build/assembly  version
        /// </summary>
        public string version { get; set; } 
        /// <summary>
        /// project name
        /// </summary>
        public string name { get; set; } 
        
        /// <summary>
        /// build number
        /// </summary>
        public string number { get; set; } 
       
        public BuildAgent buildAgent { get; set; }
        public Agent agent { get; set; } 

        /// <summary>
        /// Build start time
        /// </summary>
        public string started { get; set; } 
        /// <summary>
        /// Build duration in millis
        /// </summary>
        public long durationMillis { get; set; } 
        /// <summary>
        /// The user who executed the build (TFS user or system user)
        /// </summary>
        public string principal { get; set; } 
        /// <summary>
        /// The artifactory user used for deploythe build’s artifacts
        /// </summary>
        public string artifactoryPrincipal { get; set; }

        /// <summary>
        /// build url in the build server
        /// </summary>
        public string url { get; set; }

        /// <summary>
        /// system variables
        /// </summary>
        public Dictionary<string,string> properties { get; set; }

        /// <summary>
        /// Version control revision (Changeset number in TFS)
        /// </summary>
        public string vcsRevision { get; set; }

        public LicenseControl licenseControl { get; set; }

        public BuildRetention buildRetention { get; set; }

        /// <summary>
        /// A list of one or more modules produced by this build
        /// </summary>
        public List<Module> modules { get; set; }
    }

    /// <summary>
    /// Build agent name and version, for example MSBuild 12.0
    /// </summary>
    public class BuildAgent
    {
        public string name { get; set; }
        public string version { get; set; }
    }

    /// <summary>
    /// CI server name and version, for example TFS 2013
    /// </summary>
    public class Agent
    {
        public string name { get; set; }
        public string version { get; set; }
    }

    public class LicenseControl
    {
        public bool runChecks { get; set; }
        public bool includePublishedArtifacts { get; set; }
        public bool autoDiscover { get; set; }
        public string licenseViolationsRecipientsList { get; set; }
        public string scopeList { get; set; }

    }

    public class BuildRetention
    {
        public int count { get; set; }
        public bool deleteBuildArtifacts { get; set; }
        public List<string> buildNumbersNotToBeDiscarded { get; set; }
    }

    /// <summary>
    /// build module data
    /// </summary>
    public class Module
    {
        public Module(string projectName)
        {
            Artifacts = new List<Artifact>();
            Dependencies = new List<Dependency>();
            id = projectName;
        }

        /// <summary>
        /// module identifier
        /// </summary>
        public string id { get; set; }
        /// <summary>
        /// A list of artifacts deployed for this module
        /// </summary>
        public List<Artifact> Artifacts { get; set; }

        /// <summary>
        /// A list of dependencies used when building this module
        /// </summary>
        public List<Dependency> Dependencies { get; set; }
    }

    public class Artifact
    {
        public string type { get; set; }
        public string sha1 { get; set; }
        public string md5 { get; set; }
        public string name { get; set; }
    }

    public class Dependency
    {
        public string type { get; set; }
        public string sha1 { get; set; }
        public string md5 { get; set; }
        public string name { get; set; }
        public List<string> scopes { get; set; }
    }

}
