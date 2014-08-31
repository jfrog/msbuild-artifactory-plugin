using JFrog.Artifactory.Model;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace JFrog.Artifactory.Utils.regexCapturing
{
    public class BuildArtifacts
    {

        public static List<DeployDetails> resolve(ProjectModel.DeployAttribute deployAttribute, string projectDirectory, string repository)
        {          
            Dictionary<string, string> resultMap = new Dictionary<string, string>();
            BuildArtifactsMapping mapping = new BuildArtifactsMapping(deployAttribute.InputPattern, deployAttribute.OutputPattern);
            BuildArtifactsMappingResolver.matchMappingArtifacts(mapping, projectDirectory, resultMap);
          
            return resultMap.Select(a => new DeployDetails
            {
                artifactPath = a.Value,
                file = new FileInfo(a.Key),
                md5 = MD5CheckSum.GenerateMD5(a.Key),
                sha1 = Sha1Reference.GenerateSHA1(a.Key),
                targetRepository = repository,
                properties = deployAttribute.properties
            }).ToList();
        }
    }
}
