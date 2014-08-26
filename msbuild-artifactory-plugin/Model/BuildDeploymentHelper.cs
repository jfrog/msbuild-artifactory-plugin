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

            //Upload Build Info json file to artifactory
            log.LogMessageFromText("Uploading build info to Artifactory...", MessageImportance.High);
            ArtifactoryBuildInfoClient client = new ArtifactoryBuildInfoClient(task.Url, task.User, task.Password, log);
            
            try
            {
                /* Send Build Info  */
                client.sendBuildInfo(build);

                /* Deploy every artifacts from the Map< module.name : artifact.name > => List<DeployDetails> */
                task.deployableArtifactBuilderMap.ToList().ForEach(entry => entry.Value.ForEach(artifact => client.deployArtifact(artifact)));
            }
            catch (Exception e) 
            {
                log.LogMessageFromText("Exception has append from ArtifactoryBuildInfoClient: " + e.Message, MessageImportance.High);          
            }

            client.Dispose();
        }
    }
}
