using JFrog.Artifactory.Model;
using JFrog.Artifactory.Utils;
using Microsoft.Build.Framework;
using Microsoft.Build.Utilities;
using System;
using System.Collections.Generic;
using System.Linq;

namespace JFrog.Artifactory
{
    public class ArtifactoryBuild : Task
    {
        [Required]
        public ITaskItem[] dllList { get; set; }

        [Required]
        public ITaskItem[] projRefList { get; set; }

        public string BuildUNCPath { get; set; }
        public string BuildURI { get; set; }
        public string BuildName { get; set; }
        public string BuildReason { get; set; }
        public string StartTime { get; set; }
        public string BuildNumber { get; set; }
        public string User { get; set; }
        public string Password { get; set; }
        public string Url { get; set; }
        public string ProjectName { get; set; }
        public string ServerName { get; set; }
        public string ToolVersion { get; set; }
        public string Configuration { get; set; }
        private readonly BuildInfoModel _buildInfoModel;
        private string jsonString;
        private readonly Sha1Reference sha1;
        private readonly MD5CheckSum md5;

        private const string defaultBuildName = "Not specified***********";
        private const string defaultbuildNumber = "1.0";
        /// <summary>
        /// initiate sha1,md5, _buildInfoModel
        /// </summary>
        public ArtifactoryBuild()
        {
            sha1 = new Sha1Reference();
            md5 = new MD5CheckSum();
            _buildInfoModel = new BuildInfoModel
            {
                modules = new List<Module>(),
            };
        }

        /// <summary>
        /// this function is called by the msbuild post event.
        /// it will iterate over the dllList and projRefList defined in the csporj file.
        /// </summary>
        /// <returns></returns>
        public override bool Execute()
        {
            try
            {
                Log.LogMessageFromText("Artifactory Post-Build task started", MessageImportance.High);

                //generate json general meta-data
                Log.LogMessageFromText("Processong build info...", MessageImportance.High);
                _buildInfoModel.started = string.Format("{0}.000+0000", StartTime); //2014-04-29T00:47:41.906+0000
                _buildInfoModel.artifactoryPrincipal = User;
                _buildInfoModel.buildAgent = new BuildAgent {name = "msbuild", version = ToolVersion};

                _buildInfoModel.number = string.IsNullOrWhiteSpace(BuildNumber) ? defaultbuildNumber : BuildNumber;
                _buildInfoModel.name = BuildName ?? defaultBuildName;
                _buildInfoModel.url = BuildURI;
                //TODO where are we getting version number?
                _buildInfoModel.version = "1.0";
                
                //get the current use from the windows OS
                System.Security.Principal.WindowsIdentity user;
                user = System.Security.Principal.WindowsIdentity.GetCurrent();
                if (user != null) _buildInfoModel.principal = string.Format(@"{0}", user.Name);

                //add system variables to the json file
                Log.LogMessageFromText("Collecting system variables...", MessageImportance.High);
                _buildInfoModel.properties = AddSystemVariables();

                Log.LogMessageFromText("Processing build modules...", MessageImportance.High);
                //accumulate all refrenced dlls in the project 
                ProccessMainModule();
                //accumulate all referenced projects
                ProccessModuleRef();
                //calculate how long it took to do the build
                _buildInfoModel.durationMillis =
                    Convert.ToInt64((DateTime.Now - DateTime.Parse(StartTime)).TotalMilliseconds);
                jsonString = _buildInfoModel.ToJsonString();
                Log.LogMessageFromText("JSON output: \n"  + jsonString, MessageImportance.High);

                //upload json file to artifactory
                Log.LogMessageFromText("Uploading build info to Artifactory...", MessageImportance.High);
                UploadBuildInfo.UploadBuildInfoJson(jsonString, string.Format(Url + "/api/build", ServerName), User, Password);
                Log.LogMessageFromText("Artifactory Post-Build task succeeded", MessageImportance.High);
                return true;
            }
            catch (Exception ex)
            {
                Log.LogErrorFromException(ex, true);
                return false;
            }
        }

        /// <summary>
        /// read all refrenced dll's in the .csproj calculate their md5, sha1 and id.
        /// </summary>
        private void ProccessMainModule()
        {
            var module = new Module(ProjectName);
            foreach (var task in dllList)
            {
                var hint = task.GetMetadata("HintPath");
                if (string.IsNullOrEmpty(hint)) continue;
                var spec = GetRefrenceDetails(hint);
                if (spec == null) continue;
                module.Dependencies.Add(new Dependency
                {
                    type = "dll",
                    md5 = md5.GenerateMD5(FindNupkg(hint)),
                    sha1 = sha1.GenerateSHA1(FindNupkg(hint)),
                    name = spec.Id,
                    scopes = new List<string> {Configuration}
                });
            }
            _buildInfoModel.modules.Add(module);
        }

        /// <summary>
        /// find all projects refefreneced by the current .csporj
        /// </summary>
        private void ProccessModuleRef()
        {
            Module module;
            foreach (var task in projRefList)
            {

                //get dll nuget information
                var projectParser = new CSProjParser(Environment.CurrentDirectory + "..\\" + task.ItemSpec);
                var projectRef = projectParser.Parse();
                if (projectRef == null) continue;
                module = new Module(projectRef.AssemblyName)
                {
                    Dependencies = projectRef.LstProjectMetadata
                        .Select(x => new {reference = GetRefrenceDetails(x.EvaluatedValue), hint = x.EvaluatedValue})
                        .Where(x => x != null && x.reference != null && x.hint != string.Empty)
                        //extract only nuget dlls - only if hintpath is not empty
                        .Select(x => new Dependency
                        {
                            type = "dll",
                            name = x.reference.Id,
                            md5 = md5.GenerateMD5(FindNupkg(x.hint)),
                            sha1 = sha1.GenerateSHA1(x.hint),
                            scopes = new List<string> {Configuration}
                        }).ToList()
                };

                _buildInfoModel.modules.Add(module);
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
                //TODO handle this exption - do not hide
            }

            return null;
        }

        /// <summary>
        /// get the nupkg path
        /// </summary>
        /// <param name="path"></param>
        /// <returns></returns>
        private static string FindNupkg(string path)
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
                //TODO handle this exption - do not hide
                return null;
            }

            
        }

    }
}