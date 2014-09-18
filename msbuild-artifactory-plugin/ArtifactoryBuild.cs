using JFrog.Artifactory.Model;
using JFrog.Artifactory.Utils;
using JFrog.Artifactory.Utils.regexCapturing;
using Microsoft.Build.BuildEngine;
using Microsoft.Build.Framework;
using Microsoft.Build.Utilities;
using NuGet;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
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
        public string BuildInfoEnable { get; set; }
        public string DeployEnable { get; set;}
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
                return false;
            }
            finally 
            {
                deployableArtifactBuilderMap.Clear();
            }
        }    
    }
}