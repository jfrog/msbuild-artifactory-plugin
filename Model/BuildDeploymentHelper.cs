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

            //DeployDetails dd = new DeployDetails();
            //dd.file = new FileInfo("C:\\Builds\\1\\msbuild-plugin\\DemoSolution\\src\\Work\\nuget-project\\multi-project\\packages\\Antlr.3.4.1.9004\\Antlr.3.4.1.9004.nupkg");
            //dd.targetRepository = "nuget-local";
            //dd.artifactPath = "Antlr.3.4.1.9004.nupkg";

            //MD5CheckSum md = new MD5CheckSum();
            //dd.md5 = md.GenerateMD5(dd.file.FullName);

            //Sha1Reference sha = new Sha1Reference();
            //dd.sha1 = sha.GenerateSHA1(dd.file.FullName);

            try
            {
                client.sendBuildInfo(build);
                //client.deployArtifact(dd);
            }
            catch (Exception e) 
            {
                log.LogMessageFromText("Exception has append from ArtifactoryBuildInfoClient: " + e.Message, MessageImportance.High);          
            }

            client.Dispose();
        }
    }
}
