using Microsoft.Build.Framework;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace JFrog.Artifactory.Model
{
    /// <summary>
    /// Represent TFS Agent
    /// </summary>
    class AgentTFS : Agent
    {
        protected IBuildEngine buildEngine { get; set; }
        private const string PRE_FIX_TFS = "TF_";
        public AgentTFS(ArtifactoryBuild task)
        {
            name = "TFS";
            version = "";
            buildEngine = task.BuildEngine;
        }

        public override IDictionary<string, string> BuildAgentEnvironment() 
        {
            IDictionary<string, string> tfsProperties = BuildEngineExtensions.ContainsEnvironmentVariables(buildEngine, PRE_FIX_TFS, false);

            IDictionary<string, string> results = new Dictionary<string, string>();
            foreach (string key in tfsProperties.Keys)
            {
                results.Add(PRE_FIX_ENV + key, tfsProperties[key]);
            }

            return results;
        }

        public override string BuildAgentUrl() 
        {
            StringBuilder tfsUrl = new StringBuilder("$(TF_BUILD_COLLECTIONURI)/$(TEAM_PROJECT)/_build#buildUri=$(TF_BUILD_BUILDURI)");
            IEnumerable<String> tfsCollectionURI = BuildEngineExtensions.GetEnvironmentVariable(buildEngine, "TF_BUILD_COLLECTIONURI", false);
            IEnumerable<String> tfsTeamProject = BuildEngineExtensions.GetEnvironmentVariable(buildEngine, "TEAM_PROJECT", false);
            IEnumerable<String> tfsBuildURI = BuildEngineExtensions.GetEnvironmentVariable(buildEngine, "TF_BUILD_BUILDURI", false);

            if (tfsCollectionURI != null)
                tfsUrl = tfsUrl.Replace("$(TF_BUILD_COLLECTIONURI)", tfsCollectionURI.First());

            if (tfsTeamProject != null)
                tfsUrl = tfsUrl.Replace("$(TEAM_PROJECT)", tfsTeamProject.First());

            if (tfsBuildURI != null)
                tfsUrl = tfsUrl.Replace("$(TF_BUILD_BUILDURI)", tfsBuildURI.First());

            return tfsUrl.ToString();
        }
    }
}
