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
    
        protected const string PRE_FIX_ENV = "buildInfo.env.";

        public static Agent BuildAgentFactory(ArtifactoryBuild task) 
        {
            if (task.TfsActive != null && task.TfsActive.Equals("True"))
            {
                return new AgentTFS(task);
            }

            return new AgentMSBuild(task);
        }

        /// <summary>
        /// Specific environment variables to a Build server/agent
        /// </summary>
        /// <returns></returns>
        public abstract IDictionary<string, string> BuildAgentEnvironment();
    }
}
