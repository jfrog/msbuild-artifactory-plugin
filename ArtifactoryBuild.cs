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
        [Required]
        public ITaskItem[] dllList { get; set; }

        [Required]
        public ITaskItem[] projRefList { get; set; }

        public string OutputPath { get; set; }
        public string BaseAddress { get; set; }
        
        public string AssemblyName { get; set; }
       // public string BaseOutputPath { get; set; }
        public string WebProjectOutputDir { get; set; }
        public string SolutionRoot { get; set; }
        public string ProjectPath { get; set; }
        
        public string StartTime { get; set; }       
        public string CurrentVersion { get; set; }
        
        public string ProjectName { get; set; }
        public string ServerName { get; set; }
        public string ToolVersion { get; set; }
        public string Configuration { get; set; }

        /* Artifactory parameters*/
        public string User { get; set; }
        public string Password { get; set; }
        public string Url { get; set; }
        public string ResolutionRepository { get; set; }
        public string DeploymentRepository { get; set; }

        /* TFS parameters*/
        public string TfsActive { get; set; }
        public string BuildNumber { get; set; }
        public string BuildUNCPath { get; set; }
        public string BuildURI { get; set; }
        public string BuildName { get; set; }
        public string BuildReason { get; set; }

        public Dictionary<string, DeployDetails> deployableArtifactBuilderMap = new Dictionary<string, DeployDetails>();

        private const string defaultBuildName = "Not specified";
        private const string defaultBuildNumber = "1.0";
        private const string defaultCurrentVersion = "1.0";
        private const string artifactoryDateFormat = "yyyy-MM-ddTHH:mm:ss";

        public ArtifactoryBuild(){}

        public override bool Execute()
        {
            try
            {
                //System.Diagnostics.Debugger.Launch();
                Log.LogMessageFromText("Artifactory Post-Build task started", MessageImportance.High);

                if (TfsActive != null && TfsActive.Equals("True"))
                {
                    Log.LogMessageFromText("TFS is Active: " + TfsActive + ";" + BuildNumber + ";" + BuildName, MessageImportance.High);
                }
                Build build = extractBuild();
                BuildDeploymentHelper buildDeploymentHelper = new BuildDeploymentHelper();
                buildDeploymentHelper.deploy(this, build, Log);
                       
                return true;
            }
            catch (Exception ex)
            {
                Log.LogErrorFromException(ex, true);
                return false;
            }
        }

        private Build extractBuild()
        {
            Build build = new Build
            {
                modules = new List<Module>(),
            };

            //generate json general meta-data
            Log.LogMessageFromText("Processong build info...", MessageImportance.High);

            /* yyyy-MM-ddTHH:mm:ss.906+0000 */
            build.started = string.Format("{0}.000+0000", StartTime); 
            build.artifactoryPrincipal = User;
            build.buildAgent = new BuildAgent { name = "msbuild", version = ToolVersion };

            build.number = string.IsNullOrWhiteSpace(BuildNumber) ? defaultBuildNumber : BuildNumber;
            build.name = BuildName ?? defaultBuildName;
            build.url = BuildURI;

            build.version = string.IsNullOrWhiteSpace(CurrentVersion) ? defaultCurrentVersion : CurrentVersion;

            //get the current use from the windows OS
            System.Security.Principal.WindowsIdentity user;
            user = System.Security.Principal.WindowsIdentity.GetCurrent();
            if (user != null) build.principal = string.Format(@"{0}", user.Name);

            //add system variables to the json file
            Log.LogMessageFromText("Collecting system variables...", MessageImportance.High);
            build.properties = AddSystemVariables();

            Log.LogMessageFromText("Processing build modules...", MessageImportance.High);

            ProjectRefModel p = new ProjectRefModel();
            //p.AssemblyName = ProjectName;
            //accumulate all refrenced dlls in the project 
            //ProccessModule(build, ProjectName);
            //accumulate all referenced projects
            ProccessModuleRef(build);
            //calculate how long it took to do the build
            DateTime start = DateTime.ParseExact(StartTime, artifactoryDateFormat, null);
            build.durationMillis = Convert.ToInt64((DateTime.Now - start).TotalMilliseconds);
            
            return build;
        }

        /// <summary>
        /// find all projects refefreneced by the current .csporj
        /// </summary>
        private void ProccessModuleRef(Build build)
        {
            /*Main project*/
            var mainProjectParser = new CSProjParser(AssemblyName, SolutionRoot + "\\", ProjectPath);
            var mainProject = mainProjectParser.Parse();
            if (mainProject != null)
            {
                ProccessModule(build, mainProject);
            }

            //Module module;
            foreach (var task in projRefList)
            {
                var projectParser = new CSProjParser(task.GetMetadata("Name"), task.GetMetadata("RelativeDir"), task.GetMetadata("RelativeDir"));
                var projectRef = projectParser.Parse();
                if (projectRef != null)
                {
                    ProccessModule(build, projectRef);
                }
            }
        }

        /// <summary>
        /// read all refrenced dll's in the .csproj calculate their md5, sha1 and id.
        /// </summary>
        private void ProccessModule(Build build, ProjectRefModel project)
        {
            var module = new Module(project.AssemblyName);

            string localSource = Path.Combine(SolutionRoot, "packages");
            string[] directoryPaths = Directory.GetDirectories(SolutionRoot, project.AssemblyName, SearchOption.AllDirectories);
            string[] packageConfigPath = Directory.GetFiles(directoryPaths[0], "packages.config", SearchOption.TopDirectoryOnly);

            if (project.artifactoryDeploy != null)
            {
                foreach (ProjectRefModel.DeployAttribute deployAttribute in project.artifactoryDeploy)
                {
                    List<DeployDetails> details = BuildArtifacts.resolve(deployAttribute, project.projectDirectory, DeploymentRepository);

                    foreach (DeployDetails artifactDetail in details)
                    {
                        string artifactName = artifactDetail.file.Name;
                        module.Artifacts.Add(new Artifact
                        {
                            type = artifactDetail.file.Extension.Replace(".", String.Empty),
                            md5 = artifactDetail.md5,
                            sha1 = artifactDetail.sha1,
                            name = artifactName
                        });

                        if (!deployableArtifactBuilderMap.ContainsKey(module.id + ":" + artifactName))
                            deployableArtifactBuilderMap.Add(module.id + ":" + artifactName, artifactDetail);
                    }
                }
            }
            addDependencies(project.AssemblyName, module, localSource, packageConfigPath);
            build.modules.Add(module);
        }

        private void addDependencies(string projectName, Module module, string localSource, string[] packageConfigPath)
        {
            if (packageConfigPath.Length != 0)
            {
                var sharedPackages = new LocalPackageRepository(localSource);
                var packageReferenceFile = new PackageReferenceFile(packageConfigPath[0]);
                IEnumerable<PackageReference> projectPackages = packageReferenceFile.GetPackageReferences();

                foreach (PackageReference package in projectPackages)
                {
                    var pack = sharedPackages.FindPackage(package.Id, package.Version);

                    using (Stream packageStream = ((NuGet.OptimizedZipPackage)(pack)).GetStream())
                    {
                        byte[] buf = new byte[packageStream.Length];
                        int byteread = packageStream.Read(buf, 0, buf.Length);

                        module.Dependencies.Add(new Dependency
                        {
                            type = "nupkg",
                            md5 = MD5CheckSum.GenerateMD5(buf),
                            sha1 = Sha1Reference.GenerateSHA1(buf),
                            scopes = new List<string> { Configuration },
                            id = pack.Id  + ":" + pack.Version
                        });
                    }
                }
            }
            else
            {
                Log.LogMessageFromText("Warning - packages.config dosn`t exists in project: " + projectName, MessageImportance.High);
            }
        }

        

        /// <summary>
        /// gather all windows system variables and their values
        /// </summary>
        /// <returns></returns>
        private Dictionary<string,string> AddSystemVariables()
        {
            var dicVariables = new Dictionary<string, string>();

            var arrEnv = new[]
            {
                "NUMBER_OF_PROCESSORS", "PATHEXT", "PROCESSOR_ARCHITECTURE", "PROCESSOR_IDENTIFIER", "PROCESSOR_LEVEL",
                "PROCESSOR_REVISION",
                "OS", "ALLUSERSPROFILE", "APPDATA", "CommonProgramFiles", "CommonProgramFiles(x86)",
                "CommonProgramW6432", "FP_NO_HOST_CHECK",
                "HOMEDRIVE", "HOMEPATH", "LOCALAPPDATA", "LOGONSERVER", "Path", "ProgramData", "ProgramFiles",
                "ProgramFiles(x86)", "ProgramW6432",
                "SystemDrive", "SESSIONNAME", "PUBLIC", "TEMP", "TMP", "USERDOMAIN", "USERNAME", "USERPROFILE",
                "VS110COMNTOOLS", "VS120COMNTOOLS",
                "windir", "windows_tracing_flags", "windows_tracing_logfile"
            };

            foreach (var s in arrEnv)
            {
                var variable = Environment.GetEnvironmentVariable(s);
                if(string.IsNullOrEmpty(variable))continue;
                dicVariables.Add(s,variable);
            }

            return dicVariables;
        }
    
    }
}