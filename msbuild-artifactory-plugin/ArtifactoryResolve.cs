using JFrog.Artifactory.Model;
using JFrog.Artifactory.Utils;
using Microsoft.Build.Framework;
using Microsoft.Build.Utilities;
using System;
using System.Collections.Generic;
using System.Diagnostics;

namespace JFrog.Artifactory
{
    public class ArtifactoryResolve : Task
    {

        /* MSBuild parameters */
        [Required]
        public ITaskItem[] projRefList { get; set; }     
        public string ProjectName { get; set; }
        public string SolutionRoot { get; set; }
        public string ProjectPath { get; set; }       
        public string ToolVersion { get; set; }
        public string Configuration { get; set; }

        /* Artifactory parameters */
        public string User { get; set; }
        public string Password { get; set; }
        public string Url { get; set; }        

        /* TFS parameters */
        public string TfsActive { get; set; }
        public string BuildUNCPath { get; set; }
        //public string BuildURI { get; set; }
        public string BuildReason { get; set; }
        public string VcsRevision { get; set; }
        public string VcsUrl { get; set; }
        

        public Dictionary<string, List<DeployDetails>> deployableArtifactBuilderMap = new Dictionary<string, List<DeployDetails>>();
        public BuildInfoLog buildInfoLog;

        public ArtifactoryResolve(){}

        public override bool Execute()
        {
            try
            {
                buildInfoLog = new BuildInfoLog(Log);
                buildInfoLog.Info("Artifactory Pre-Build task started");
                LaunchCommandLineApp();

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

        static void LaunchCommandLineApp()
        {
            // Use ProcessStartInfo class
            ProcessStartInfo startInfo = new ProcessStartInfo();
            startInfo.CreateNoWindow = true;
            startInfo.UseShellExecute = false;
            startInfo.FileName = "C:\\Users\\tamirh\\Downloads\\jfrog.exe";
            startInfo.WindowStyle = ProcessWindowStyle.Hidden;
            startInfo.Arguments = "rt dl ext-release-local/*" + ;

            try
            {
                // Start the process with the info we specified.
                // Call WaitForExit and then the using statement will close.
                using (Process exeProcess = Process.Start(startInfo))
                {
                    exeProcess.WaitForExit();
                    int r = exeProcess.ExitCode;
                    Console.WriteLine(r);
                }
            }
            catch
            {
                // Log error.
            }
        }
    }
}