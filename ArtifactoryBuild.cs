using JFrog.Artifactory.Model;
using JFrog.Artifactory.Utils;
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
        public string TfsActive { get; set; }
        public string AssemblyName { get; set; }
        public string BaseOutputPath { get; set; }
        public string WebProjectOutputDir { get; set; }
        public string SolutionRoot { get; set; }
        public string BuildUNCPath { get; set; }
        public string BuildURI { get; set; }
        public string BuildName { get; set; }
        public string BuildReason { get; set; }
        public string StartTime { get; set; }
        public string BuildNumber { get; set; }
        public string CurrentVersion { get; set; }
        public string User { get; set; }
        public string Password { get; set; }
        public string Url { get; set; }
        public string ResolutionRepository { get; set; }
        public string ProjectName { get; set; }
        public string ServerName { get; set; }
        public string ToolVersion { get; set; }
        public string Configuration { get; set; }
        //private readonly 
        //private readonly Sha1Reference sha1;
        //private readonly MD5CheckSum md5;

        private const string defaultBuildName = "Not specified";
        private const string defaultBuildNumber = "1.0";
        private const string defaultCurrentVersion = "1.0";
        private const string artifactoryDateFormat = "yyyy-MM-ddTHH:mm:ss";
        /// <summary>
        /// initiate sha1,md5, _buildInfoModel
        /// </summary>
        public ArtifactoryBuild()
        {
            //sha1 = new Sha1Reference();
            //md5 = new MD5CheckSum();
            
        }

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

            /*yyyy-MM-ddTHH:mm:ss.906+0000*/
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
            //accumulate all refrenced dlls in the project 
            ProccessMainModule(build, ProjectName);
            //accumulate all referenced projects
            ProccessModuleRef(build);
            //calculate how long it took to do the build
            DateTime start = DateTime.ParseExact(StartTime, artifactoryDateFormat, null);
            build.durationMillis = Convert.ToInt64((DateTime.Now - start).TotalMilliseconds);
            
            return build;
        }

        /// <summary>
        /// read all refrenced dll's in the .csproj calculate their md5, sha1 and id.
        /// </summary>
        private void ProccessMainModule(Build build, string projectName)
        {
            var module = new Module(projectName);

            string localSource = Path.Combine(SolutionRoot, "packages");          
            string[] directoryPaths = Directory.GetDirectories(SolutionRoot, projectName, SearchOption.AllDirectories);
            string[] packageConfigPath = Directory.GetFiles(directoryPaths[0], "packages.config", SearchOption.TopDirectoryOnly);


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
                                name = package.Id,
                                scopes = new List<string> { Configuration }
                            });
                    }
                }
            }
            else 
            {
                Log.LogMessageFromText("Warning - packages.config dosn`t exists in project: " + projectName, MessageImportance.High);
            }
            
            //IQueryable<IPackage> packages = repo.GetPackages();

            //foreach (IPackage package in packages)
            //{              
            //    //foreach (var dependency in package.DependencySets){

            //    //}
            //    Stream packageStream = ((NuGet.OptimizedZipPackage)(package)).GetStream();
            //    string id = package.Id;
            //    module.Dependencies.Add(new Dependency
            //    {
            //        type = "nupkg",
            //        md5 = md5.GenerateMD5(packageStream),
            //        sha1 = sha1.GenerateSHA1(packageStream),
            //        name = package.Id,
            //        scopes = new List<string> { Configuration }
            //    });
            //}
            
            
            
            
            //foreach (var task in dllList)
            //{
            //    var hint = task.GetMetadata("HintPath");
            //    if (string.IsNullOrEmpty(hint)) continue;
            //    var spec = GetRefrenceDetails(hint);
            //    if (spec == null) continue;
            //    module.Dependencies.Add(new Dependency
            //    {
            //        type = "nupkg", //?????? dll OR nupkg
            //        md5 = md5.GenerateMD5(FindNupkg(hint)),
            //        sha1 = sha1.GenerateSHA1(FindNupkg(hint)),
            //        name = spec.Id,
            //        scopes = new List<string> {Configuration}
            //    });
            //}
            build.modules.Add(module);
        }

        /// <summary>
        /// find all projects refefreneced by the current .csporj
        /// </summary>
        private void ProccessModuleRef(Build build)
        {
            //Module module;
            foreach (var task in projRefList)
            {
                
                //get dll nuget information
                var projectParser = new CSProjParser(Environment.CurrentDirectory + "..\\" +  task.ItemSpec);
                var projectRef = projectParser.Parse();
                if (projectRef != null)
                {

                    ProccessMainModule(build, projectRef.AssemblyName);

                    //module = new Module(projectRef.AssemblyName)
                    //{
                    //    Dependencies = projectRef.LstProjectMetadata
                    //        .Select(x => new { reference = GetRefrenceDetails(x.EvaluatedValue), hint = x.EvaluatedValue })
                    //        .Where(x => x != null && x.reference != null && x.hint != string.Empty)
                    //        //extract only nuget dlls - only if hintpath is not empty
                    //        .Select(x => new Dependency
                    //        {
                    //            type = "dll",
                    //            name = x.reference.Id,
                    //            md5 = md5.GenerateMD5(FindNupkg(x.hint)),
                    //            sha1 = sha1.GenerateSHA1(x.hint),
                    //            scopes = new List<string> { Configuration }
                    //        }).ToList()
                    //};

                    //build.modules.Add(module);
                }
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
    

        /// <summary>
        /// once a referance is found we will try crawiling up the directory
        /// to find the nuget packages directory and then extract the information from .nuspec file
        /// </summary>
        /// <param name="path">dll directory as specifide in the csproj file</param>
        /// <returns>the .nuspec meta data information</returns>
        private NuSpec GetRefrenceDetails(string path)
        {
            try
            {

                //var repository = PackageRepositoryFactory.Default.CreateRepository(Environment.CurrentDirectory + "..\\packages");
                var repo = new LocalPackageRepository("C:\\Work\\nuget-project\\multi-project\\packages");
                IQueryable<IPackage> packages = repo.GetPackages();

                foreach (IPackage package in packages)
                {
                    //((NuGet.OptimizedZipPackage)(package)).FindDependency();
                    //foreach (var dependency in package.DependencySets){
                    
                    //}
                    
                    string id = package.Id;

                }

                //Debug.WriteLine(String.Format("Reading Nuget Reference for : {0} {1}", Environment.CurrentDirectory + "..\\", path));
                var cr = new DirectoryCrawler();
                var info = cr.FindPath(Environment.CurrentDirectory + "..\\" + path);
                var fileInfo = info.GetFiles("*.nuspec");
                if (fileInfo.Length > 0)
                {
                   // Debug.WriteLine(String.Format("Found nuget specification file at : {0}", fileInfo[0].FullName));
                    var n = new NuspecParser(fileInfo[0].FullName);
                    var  nuspec = n.Parse();
                    return nuspec;
                }
            }
            catch(Exception ex)
            {
                Log.LogMessageFromText("Exception: " + ex.Message, MessageImportance.High);
            }

            return null;
        }

        /// <summary>
        /// get the nupkg path
        /// </summary>
        /// <param name="path"></param>
        /// <returns></returns>
        private string FindNupkg(string path)
        {
            try
            {
                var cr = new DirectoryCrawler();
                var info = cr.FindPath(Environment.CurrentDirectory + "..\\" + path);
                var fileInfo = info.GetFiles("*.nupkg");
                return fileInfo.Length > 0 ? fileInfo[0].FullName : null;
            }
            catch (Exception ex)
            {
                Log.LogMessageFromText("Exception: " + ex.Message, MessageImportance.High);
                return null;
            }          
        }

    }
}