using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;

namespace JFrog.Artifactory.Model
{
    /// <summary>
    /// Artifactory MsBuild info model 
    /// </summary>
    public class Build
    {
        public readonly static string STARTED_FORMAT = "{0}";//.000+0000";

        /// <summary>
        /// build/assembly  version
        /// </summary>
        public string version { get { return "1.0.1"; } }
        /// <summary>
        /// project name
        /// </summary>
        public string name { get; set; } 
        
        /// <summary>
        /// build number
        /// </summary>
        public string number { get; set; }

        public string type { get; set; }
        public BuildAgent buildAgent { get; set; }
        public Agent agent { get; set; } 

        /// <summary>
        /// Build start time
        /// </summary>
        public string started { get; set; }
        /// <summary>
        /// Build start time
        /// </summary>
        public string startedDateMillis { get; set; } 
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

        public DeployClient deployClient { set; get; }
    
        public Dictionary<string, string> getDefaultProperties()
        {
            Dictionary<string, string> result = new Dictionary<string, string>();
            result.Add("build.name", name);
            result.Add("build.number", number);
            result.Add("build.timestamp", startedDateMillis);
            result.Add("vcs.revision", vcsRevision);

            return result;
        }

        /// <summary>
        /// Preparing the properties (Matrix params) to suitable Url Query 
        /// </summary>
        /// <param name="matrixParam"></param>
        /// <returns></returns>
        public static string buildMatrixParamsString(List<KeyValuePair<string, string>> matrixParam)
        {
            StringBuilder matrix = new StringBuilder();

            if (matrixParam != null)
            {
                matrixParam.ForEach(
                            pair => matrix.Append(";").Append(WebUtility.UrlEncode(pair.Key)).Append("=").
                                    Append(WebUtility.UrlEncode(pair.Value))
                );
            }

            return matrix.ToString();
        }
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
        public string runChecks { get; set; }
        public string includePublishedArtifacts { get; set; }
        public string autoDiscover { get; set; }
        public List<string> licenseViolationsRecipients { get; set; }
        public List<string> scopes { get; set; }

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
            Artifacts = new HashSet<Artifact>(new Artifact());
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
        public HashSet<Artifact> Artifacts { get; set; }

        /// <summary>
        /// A list of dependencies used when building this module
        /// </summary>
        public List<Dependency> Dependencies { get; set; }
    }

    public class Artifact : IEqualityComparer<Artifact>
    {
        public string type { get; set; }
        public string sha1 { get; set; }
        public string md5 { get; set; }
        public string name { get; set; }

        public bool Equals(Artifact a, Artifact b)
        {
            if (!a.type.Equals(b.type))
                return false;
            if (!a.sha1.Equals(b.sha1))
                return false;
            if (!a.md5.Equals(b.md5))
                return false;
            if (!a.name.Equals(b.name))
                return false;
          
            return true;   
        }

        public int GetHashCode(Artifact obj) 
        {
            int hash = 17;
            hash = hash * 31 + obj.type.GetHashCode();
            hash = hash * 31 + obj.sha1.GetHashCode();
            hash = hash * 31 + obj.md5.GetHashCode();
            hash = hash * 31 + obj.name.GetHashCode();

            return hash;
        }
    }

    public class Dependency
    {
        public string type { get; set; }
        public string sha1 { get; set; }
        public string md5 { get; set; }
        public string id { get; set; }
        public List<string> scopes { get; set; }
    }

    public class DeployClient
    {
        public int timeout { get; set; }
        public Proxy proxy { set; get; }
    }

    public class Proxy 
    {
        private string host;
        private int port;
        private string username;
        private string password;
        private bool isCredentialsExists;

        public Proxy() { }

        public Proxy(string host, int port)
        {
            this.host = host;
            this.port = port;
            this.isCredentialsExists = false;
        }

        public Proxy(string host, int port, string username, string password) 
        {
            this.host = host;
            this.port = port;
            this.username = username;
            this.password = password;
            this.isCredentialsExists = true;
        }

        public string Host { get { return this.host; } }
        public int Port { get { return this.port; } }
        public string Username { get { return this.username; } }
        public string Password { get { return this.password; } }
        public bool IsCredentialsExists { get { return this.isCredentialsExists; } }
        public bool IsBypass { get; set; }
    }
    //public static class Json
    //{
    //    public static const string version = "version";
    //    public static const string name = "name";
    //    public static const string number = "number";
    //    public static const string buildAgent = "buildAgent";
    //    public static const string agent = "agent";
    //    public static const string started = "started";
    //    public static const string durationMillis = "durationMillis";
    //    public static const string principal = "principal";
    //    public static const string artifactoryPrincipal = "artifactoryPrincipal";
    //    public static const string url = "url";
    //    public static const string vcsRevision = "vcsRevision";
    //    public static const string licenseControl = "licenseControl";
    //    public static const string buildRetention = "buildRetention";
    //    public static const string properties = "properties";
    //    public static const string modules = "modules";
    //    public static const string dependencies = "dependencies";
    //    public static const string artifacts = "artifacts";
    //    public static const string scopes = "scopes";
    //}
}
