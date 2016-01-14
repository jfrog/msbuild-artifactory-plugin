using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Build.Framework;
using NuGet;

namespace JFrog.Artifactory.Model
{
    class AgentTFS2015 : Agent
    {
        protected IBuildEngine buildEngine { get; set; }
        private const string PRE_FIX_AGENT = "AGENT_";
        private const string PRE_FIX_BUILD = "BUILD_";
        private const string PRE_FIX_SYSTEM= "SYSTEM_";
        public AgentTFS2015(ArtifactoryBuild task)
        {
            name = "TFS2015";
            version = "";
            buildEngine = task.BuildEngine;
        }

        public override IDictionary<string, string> BuildAgentEnvironment()
        {
            IDictionary<string, string> tfsProperties = BuildEngineExtensions.ContainsEnvironmentVariablesStartingWith(buildEngine, PRE_FIX_AGENT, false);

            tfsProperties.AddRange(BuildEngineExtensions.ContainsEnvironmentVariablesStartingWith(buildEngine, PRE_FIX_BUILD, false));
            tfsProperties.AddRange(BuildEngineExtensions.ContainsEnvironmentVariablesStartingWith(buildEngine, PRE_FIX_SYSTEM, false));

            IDictionary<string, string> results = new Dictionary<string, string>();
            foreach (string key in tfsProperties.Keys)
            {
                results.Add(PRE_FIX_ENV + key, tfsProperties[key]);
            }

            return results;
        }

        public override string BuildAgentUrl()
        {
            StringBuilder tfsUrl = new StringBuilder("$(SYSTEM_TEAMFOUNDATIONCOLLECTIONURI)/$(SYSTEM_TEAMPROJECT)/_build#buildId=$(BUILD_BUILDID)&_a=summary");
            IEnumerable<String> tfsCollectionURI = BuildEngineExtensions.GetEnvironmentVariable(buildEngine, "SYSTEM_TEAMFOUNDATIONCOLLECTIONURI", false);
            IEnumerable<String> tfsTeamProject = BuildEngineExtensions.GetEnvironmentVariable(buildEngine, "SYSTEM_TEAMPROJECT", false);
            IEnumerable<String> tfsBuildURI = BuildEngineExtensions.GetEnvironmentVariable(buildEngine, "BUILD_BUILDID", false);

            if (tfsCollectionURI != null)
                tfsUrl = tfsUrl.Replace("$(SYSTEM_TEAMFOUNDATIONCOLLECTIONURI)", tfsCollectionURI.First());

            if (tfsTeamProject != null)
                tfsUrl = tfsUrl.Replace("$(SYSTEM_TEAMPROJECT)", tfsTeamProject.First());

            if (tfsBuildURI != null)
                tfsUrl = tfsUrl.Replace("$(BUILD_BUILDID)", tfsBuildURI.First());

            return tfsUrl.ToString();
        }
    }
}
