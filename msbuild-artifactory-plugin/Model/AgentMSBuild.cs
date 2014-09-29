
using System.Collections.Generic;
namespace JFrog.Artifactory.Model
{
    /// <summary>
    /// Represent MSBuild Agent
    /// </summary>
    public class AgentMSBuild : Agent
    {
        public AgentMSBuild(ArtifactoryBuild task) 
        {
            name = "MSBuild";
            version = task.ToolVersion; 
        }

        public override IDictionary<string, string> BuildAgentEnvironment()
        {          
            return new Dictionary<string, string>();
        }

        public override string BuildAgentUrl()
        {
            return string.Empty;
        }
    }
}
