using Microsoft.Build.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace JFrog.Artifactory.Model
{
    public abstract class Agent
    {
        public string name { get; set; }
        public string version { get; set; }
    
        internal const string PRE_FIX_ENV = "buildInfo.env.";

        public static Agent BuildAgentFactory(ArtifactoryBuild task) 
        {
            if (task.TfsActive != null && task.TfsActive.Equals("True"))
            {
                IEnumerable<String> tfsCollectionURI = BuildEngineExtensions.GetEnvironmentVariable(task.BuildEngine, "TF_BUILD_COLLECTIONURI", false);
                if (tfsCollectionURI != null)
                {
                    return new AgentTFS(task);
                }
                else
                {
                    return new AgentTFS2015(task);
                }
            }

            return new AgentMSBuild(task);
        }

        /// <summary>
        /// Specific environment variables to a Build server/agent
        /// </summary>
        /// <returns></returns>
        public abstract IDictionary<string, string> BuildAgentEnvironment();

        public abstract string BuildAgentUrl();
    }
}
