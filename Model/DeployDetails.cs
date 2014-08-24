using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace JFrog.Artifactory.Model
{
    public class DeployDetails
    {
        public string targetRepository { set; get; }

        public string artifactPath { set; get; }

        public FileInfo file { set; get; }

        public string sha1 { set; get; }

        public string md5 { set; get; }

        public Dictionary<string, string> properties;
    }

    //public static class Builder
    //{
    //    private DeployDetails deployDetails;

    //    public Builder()
    //    {
    //        deployDetails = new DeployDetails();
    //    }

    //    public DeployDetails build()
    //    {
    //        if (deployDetails.file == null || !deployDetails.file.Exists)
    //        {
    //            throw new ArgumentException("File not found: " + deployDetails.file);
    //        }
    //        if (String.IsNullOrWhiteSpace(deployDetails.targetRepository))
    //        {
    //            throw new ArgumentException("Target repository cannot be empty");
    //        }
    //        if (String.IsNullOrWhiteSpace(deployDetails.artifactPath))
    //        {
    //            throw new ArgumentException("Artifact path cannot be empty");
    //        }

    //        return deployDetails;
    //    }

    //    public Builder file(FileInfo file)
    //    {
    //        deployDetails.file = file;
    //        return this;
    //    }

    //    public Builder targetRepository(String targetRepository)
    //    {
    //        deployDetails.targetRepository = targetRepository;
    //        return this;
    //    }

    //    public Builder artifactPath(String artifactPath)
    //    {
    //        deployDetails.artifactPath = artifactPath;
    //        return this;
    //    }

    //    public Builder sha1(String sha1)
    //    {
    //        deployDetails.sha1 = sha1;
    //        return this;
    //    }

    //    public Builder md5(String md5)
    //    {
    //        deployDetails.md5 = md5;
    //        return this;
    //    }

    //    public Builder addProperty(String key, String value)
    //    {
    //        if (deployDetails.properties == null)
    //        {
    //            deployDetails.properties = new Dictionary<string, string>();
    //        }
    //        deployDetails.properties.Add(key, value);
    //        return this;
    //    }

    //    public Builder addProperties(Dictionary<string, string> propertiesToAdd)
    //    {
    //        if (deployDetails.properties == null)
    //        {
    //            deployDetails.properties = new Dictionary<string, string>();
    //        }


    //        foreach (var prop in propertiesToAdd)
    //            deployDetails.properties.Add(prop.Key, prop.Value);

    //        return this;
    //    }
    //}   
}
