using JFrog.Artifactory.Model;
using JFrog.Artifactory.Utils;
using JFrog.Artifactory.Utils.regexCapturing;
using Microsoft.Build.BuildEngine;
using Microsoft.Build.Execution;
using Microsoft.Build.Framework;
using Microsoft.Build.Utilities;
using NuGet;
using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Security.Cryptography;

namespace JFrog.Artifactory
{
    public class ArtifactoryBuild : Task
    {

        /* MSBuild parameters */
        [Required]
        public ITaskItem[] projRefList { get; set; }     
        public string ProjectName { get; set; }
        public string SolutionRoot { get; set; }
        public string ProjectPath { get; set; }       
        public string StartTime { get; set; }       
        public string ServerName { get; set; }
        public string ToolVersion { get; set; }
        public string Configuration { get; set; }

        /* Artifactory parameters */
        public string BuildInfoEnabled { get; set; }
        public string DeployEnabled { get; set;}
        public string User { get; set; }
        public string Password { get; set; }
        public string Url { get; set; }        
        public string DeploymentRepository { get; set; }

        /* TFS parameters */
        public string TfsActive { get; set; }
        public string BuildNumber { get; set; }
        public string BuildUNCPath { get; set; }
        public string BuildURI { get; set; }
        public string BuildName { get; set; }
        public string BuildReason { get; set; }
        public string VcsRevision { get; set; }
        public string VcsUrl { get; set; }
        

        public Dictionary<string, List<DeployDetails>> deployableArtifactBuilderMap = new Dictionary<string, List<DeployDetails>>();
        public BuildInfoLog buildInfoLog;

        public ArtifactoryBuild(){}

        public override bool Execute()
        {
            try
            {
                //GetEnvironmentVariable(this.BuildEngine, "soultionDir", false);

                buildInfoLog = new BuildInfoLog(Log);
                //System.Diagnostics.Debugger.Launch();
                buildInfoLog.Info("Artifactory Post-Build task started");

                if (TfsActive != null && TfsActive.Equals("True"))
                {
                    buildInfoLog.Info("Running inside TFS " +
                                            "\n TFS_Build_Number: " + BuildNumber +
                                            "\n TFS_Build_Name: " + BuildName +
                                            "\n TFS_Vcs_Revision: " + VcsRevision);

                }
                SolutionHandler solution = new SolutionHandler(this, buildInfoLog);
                solution.Execute();

                BuildDeploymentHelper buildDeploymentHelper = new BuildDeploymentHelper();
                buildDeploymentHelper.deploy(this, solution._buildInfo, buildInfoLog);

                return true;
            }
            catch (Exception ex)
            {
                buildInfoLog.Error("Exception from Artifactory Task: " + ex.Message, ex);
                /*By returning false in exception, the task will not fail the all build.*/
                throw new Exception("Exception from Artifactory Task: " + ex.Message);
                //return false;
            }
            finally 
            {
                deployableArtifactBuilderMap.Clear();
            } 
        }

        //const BindingFlags bindingFlags = BindingFlags.NonPublic | BindingFlags.FlattenHierarchy | BindingFlags.Instance | BindingFlags.Public;
        //public IEnumerable GetEnvironmentVariable(this IBuildEngine buildEngine, string key, bool throwIfNotFound)
        //{
        //    var projectInstance = GetProjectInstance(buildEngine);

        //    var items = projectInstance.Items
        //        .Where(x => string.Equals(x.ItemType, key, StringComparison.InvariantCultureIgnoreCase)).ToList();
        //    if (items.Count > 0)
        //    {
        //        return items.Select(x => x.EvaluatedInclude);
        //    }


        //    var properties = projectInstance.Properties
        //        .Where(x => string.Equals(x.Name, key, StringComparison.InvariantCultureIgnoreCase)).ToList();
        //    if (properties.Count > 0)
        //    {
        //        return properties.Select(x => x.EvaluatedValue);
        //    }

        //    if (throwIfNotFound)
        //    {
        //        throw new Exception(string.Format("Could not extract from '{0}' environmental variables.", key));
        //    }

        //    return null;
        //}

        // ProjectInstance GetProjectInstance(IBuildEngine buildEngine)
        //{
        //    var buildEngineType = buildEngine.GetType();
        //    var targetBuilderCallbackField = buildEngineType.GetField("targetBuilderCallback", bindingFlags);
        //    if (targetBuilderCallbackField == null)
        //    {
        //        throw new Exception("Could not extract targetBuilderCallback from " + buildEngineType.FullName);
        //    }
        //    var targetBuilderCallback = targetBuilderCallbackField.GetValue(buildEngine);
        //    var targetCallbackType = targetBuilderCallback.GetType();
        //    var projectInstanceField = targetCallbackType.GetField("projectInstance", bindingFlags);
        //    if (projectInstanceField == null)
        //    {
        //        throw new Exception("Could not extract projectInstance from " + targetCallbackType.FullName);
        //    }
        //    return (ProjectInstance)projectInstanceField.GetValue(targetBuilderCallback);
        //}
    }
}