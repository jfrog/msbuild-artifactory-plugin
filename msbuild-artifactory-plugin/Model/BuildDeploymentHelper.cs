using JFrog.Artifactory.Utils;
using Microsoft.Build.Framework;
using Microsoft.Build.Utilities;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;

namespace JFrog.Artifactory.Model
{
    class BuildDeploymentHelper
    {
        public void deploy(ArtifactoryBuild task, Build build, TaskLoggingHelper log) 
        {
            ArtifactoryBuildInfoClient client = new ArtifactoryBuildInfoClient(task.Url, task.User, task.Password, log);

            try
            {
                if (task.DeployEnable != null && task.DeployEnable.Equals("true"))
                {
                    /* Deploy every artifacts from the Map< module.name : artifact.name > => List<DeployDetails> */
                    task.deployableArtifactBuilderMap.ToList().ForEach(entry => entry.Value.ForEach(artifact => client.deployArtifact(artifact)));
                }

                if (task.BuildInfoEnable != null && task.BuildInfoEnable.Equals("true"))
                {
                    //Upload Build Info json file to Artifactory
                    log.LogMessageFromText("Uploading build info to Artifactory...", MessageImportance.High);
                    /* Send Build Info  */
                    client.sendBuildInfo(build);
                }
            }
            catch (Exception e)
            {
                log.LogError("Exception has append from ArtifactoryBuildInfoClient: " + e.Message);
            }
            finally 
            {
                client.Dispose();
            }         
        }
    }
}
