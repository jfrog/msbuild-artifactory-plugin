using JFrog.Artifactory.Model;
using JFrog.Artifactory.Utils;
using Microsoft.Build.Execution;
using Microsoft.Build.Framework;
using Microsoft.Build.Utilities;
using System;
using System.Collections.Generic;

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
                buildInfoLog = new BuildInfoLog(Log);

                //Incase the MSBuild process is up, and the global variable is still exist.
                deployableArtifactBuilderMap.Clear();

                
                //System.Diagnostics.Debugger.Launch();
                buildInfoLog.Info("Artifactory Post-Build task started");

                if (!string.IsNullOrWhiteSpace(TfsActive) && TfsActive.Equals("True"))
                {
                    buildInfoLog.Info("Running inside TFS...");
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
    }
}