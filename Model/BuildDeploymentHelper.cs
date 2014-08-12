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
            //client.sendBuildInfo(build);


            DeployDetails dd = new DeployDetails();
            dd.file = new FileInfo("C:\\Work\\nuget-project\\multi-project\\packages\\AWSSDK.2.0.15.0\\AWSSDK.2.0.15.0.nupkg");
            dd.targetRepository="nuget-local";
            dd.artifactPath = "AWSSDK.2.0.15.0.nupkg";

            MD5CheckSum md = new MD5CheckSum();
            dd.md5 = md.GenerateMD5(dd.file.FullName);

            Sha1Reference sha = new Sha1Reference();
            dd.sha1 = sha.GenerateSHA1(dd.file.FullName);

            client.deployArtifact(dd);
            client.Dispose();
        }
    }
}
