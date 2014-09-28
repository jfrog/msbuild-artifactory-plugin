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
    }
}
