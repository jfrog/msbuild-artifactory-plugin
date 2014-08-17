using JFrog.Artifactory.Utils;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Build.Utilities;
using Microsoft.Build.Framework;
using Microsoft.Build.Evaluation;
using System.IO;
using NuGet;

namespace JFrog.Artifactory
{
    public class ArtifactoryResolve : Task
    {
        public string Url { get; set; }

        public ArtifactoryResolve() { }

        public override bool Execute()
        {
            try
            {
                //Project projectParser = new Project("C:\\Work\\nuget-project\\multi-project\\.nuget\\NuGet.targets");
                //projectParser.SetProperty("PackageSources", "http://192.168.56.1:8080/artifactory/api/nuget/nuget-virtual");
                //projectParser.Save();
                //projectParser.ReevaluateIfNecessary();

                var repository = PackageRepositoryFactory.Default.CreateRepository(Environment.CurrentDirectory + "..\\packages");

                Log.LogMessageFromText("Resoling variables...", MessageImportance.High);
                return true;
            }
            catch (Exception ex)
            {
                Log.LogErrorFromException(ex, true);
                return false;
            }
        }
    }
}
