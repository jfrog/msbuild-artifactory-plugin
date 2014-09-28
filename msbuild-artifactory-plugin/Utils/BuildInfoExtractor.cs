using JFrog.Artifactory.Model;
using JFrog.Artifactory.Utils.regexCapturing;
using NuGet;
using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Text.RegularExpressions;

namespace JFrog.Artifactory.Utils
{
    public class BuildInfoExtractor
    {
        private const string defaultBuildName = "Not_specified";
        private const string defaultBuildNumber = "1.0";
        private const string artifactoryDateFormat = "yyyy-MM-dd'T'HH:mm:ss.ssszzzz";
        private const string validEmailPattern = "^[_A-Za-z0-9-]+(\\.[_A-Za-z0-9-]+)*@[A-Za-z0-9-]+(\\.[A-Za-z0-9-]+)*((\\.[A-Za-z]{2,}){1}$)";

        public static Build extractBuild(ArtifactoryBuild task, ArtifactoryConfig artifactoryConfig, BuildInfoLog log)
        {
            Build build = new Build
            {
                modules = new List<Module>(),
            };

            build.started = string.Format(Build.STARTED_FORMAT, task.StartTime);
            build.artifactoryPrincipal = task.User;
            build.buildAgent = new BuildAgent { name = "MSBuild", version = task.ToolVersion };
            build.type = "MSBuild";

            build.agent = Agent.BuildAgentFactory(task);

            //get the current use from the windows OS
            System.Security.Principal.WindowsIdentity user;
            user = System.Security.Principal.WindowsIdentity.GetCurrent();
            if (user != null) build.principal = string.Format(@"{0}", user.Name);

            //Calculate how long it took to do the build
            DateTime start = DateTime.ParseExact(task.StartTime, artifactoryDateFormat, null);
            build.startedDateMillis = GetTimeStamp(start);
            build.durationMillis = Convert.ToInt64((DateTime.Now - start).TotalMilliseconds);

            build.number = string.IsNullOrWhiteSpace(task.BuildNumber) ? build.startedDateMillis : task.BuildNumber;
            build.name = task.BuildName ?? task.ProjectName;
            build.url = task.BuildURI;
            build.vcsRevision = task.VcsRevision;

            IDictionary<string, string> buildProperties = AddSystemVariables(artifactoryConfig);
            buildProperties.AddRange(build.agent.BuildAgentEnvironment());

            //Add build server properties, if exists.
            build.properties = buildProperties;
            build.licenseControl = AddLicenseControl(artifactoryConfig, log);

            ConfigHttpClient(artifactoryConfig, build);

            return build;
        }

        /// <summary>
        /// Read all referenced nuget`s in the .csproj calculate their md5, sha1 and id.
        /// </summary>
        public static void ProccessModule(Build build, ProjectModel project, ArtifactoryBuild _task)
        {
            var module = new Module(project.AssemblyName);

            string localSource = Path.Combine(_task.SolutionRoot, "packages");
            //string[] directoryPaths = Directory.GetDirectories(_task.SolutionRoot, project.AssemblyName, SearchOption.AllDirectories);
            string[] packageConfigPath = Directory.GetFiles(project.projectDirectory, "packages.config", SearchOption.AllDirectories);

            if (project.artifactoryDeploy != null)
            {
                foreach (ProjectModel.DeployAttribute deployAttribute in project.artifactoryDeploy)
                {
                    List<DeployDetails> details = BuildArtifacts.resolve(deployAttribute, project.projectDirectory, _task.DeploymentRepository);
                    deployAttribute.properties.AddRange(build.getDefaultProperties());
                    foreach (DeployDetails artifactDetail in details)
                    {
                        //Add default artifact properties                    
                        artifactDetail.properties = Build.buildMatrixParamsString(deployAttribute.properties);

                        string artifactName = artifactDetail.file.Name;
                        module.Artifacts.Add(new Artifact
                        {
                            type = artifactDetail.file.Extension.Replace(".", String.Empty),
                            md5 = artifactDetail.md5,
                            sha1 = artifactDetail.sha1,
                            name = artifactName
                        });

                        string artifactId = module.id + ":" + artifactName;
                        if (_task.deployableArtifactBuilderMap.ContainsKey(artifactId))
                        {
                            _task.deployableArtifactBuilderMap[artifactId].Add(artifactDetail);
                        }
                        else
                        {
                            _task.deployableArtifactBuilderMap.Add(artifactId, new List<DeployDetails> { artifactDetail });
                        }
                    }
                }
            }
            addDependencies(project.AssemblyName, module, localSource, packageConfigPath, _task.Configuration);
            build.modules.Add(module);
        }

        /*<summary>
        *   Using Nuget.Core API, we gather all nuget`s packages that specific project depend on them.
        * </summary>
        * <returns></returns>
        */
        private static void addDependencies(string projectName, Module module, string localSource, string[] packageConfigPath, string Configuration)
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
                            id = pack.Id + ":" + pack.Version
                        });
                    }
                }
            }
        }

        /// <summary>
        /// Gather all windows system variables and their values
        /// </summary>
        /// <returns></returns>
        private static Dictionary<string, string> AddSystemVariables(ArtifactoryConfig artifactoryConfig)
        {
            string enable = artifactoryConfig.PropertyGroup.EnvironmentVariables.EnabledEnvVariable;
            if (string.IsNullOrWhiteSpace(enable) || !enable.ToLower().Equals("true"))
                return new Dictionary<string, string>();

            // includePatterns = new List<Pattern>();
            //List<Pattern> excludePatterns = new List<Pattern>();
            List<Pattern> includePatterns = artifactoryConfig.PropertyGroup.EnvironmentVariables.IncludePatterns.Pattern;
            List<Pattern> excludePatterns = artifactoryConfig.PropertyGroup.EnvironmentVariables.ExcludePatterns.Pattern;

            StringBuilder includeRegexUnion = new StringBuilder();
            StringBuilder excludeRegexUnion = new StringBuilder();

            if (includePatterns != null && includePatterns.Count > 0) 
            {
                includePatterns.ForEach(pattern => includeRegexUnion.Append(WildcardToRegex(pattern.key)).Append("|"));
                includeRegexUnion.Remove(includeRegexUnion.Length - 1, 1);
            }

            if (excludePatterns != null && excludePatterns.Count > 0) 
            {
                excludePatterns.ForEach(pattern => excludeRegexUnion.Append(WildcardToRegex(pattern.key)).Append("|"));
                excludeRegexUnion.Remove(excludeRegexUnion.Length - 1, 1);
            }

            Regex includeRegex = new Regex(includeRegexUnion.ToString(), RegexOptions.IgnoreCase);
            Regex excludeRegex = new Regex(excludeRegexUnion.ToString(), RegexOptions.IgnoreCase);

            //System.Environment.GetEnvironmentVariables()
            //EnvironmentVariableTarget
            IDictionary sysVariables = Environment.GetEnvironmentVariables(EnvironmentVariableTarget.Process);
            var dicVariables = new Dictionary<string, string>();

            foreach(string key in sysVariables.Keys)
            {
                if (!PathConflicts(includePatterns, excludePatterns, includeRegex, excludeRegex, key)) 
                {
                    dicVariables.Add(key, (string)sysVariables[key]);
                }
            }


            return dicVariables;
        }

        private static LicenseControl AddLicenseControl(ArtifactoryConfig artifactoryConfig, BuildInfoLog log)
        {
            LicenseControl licenseControl = new LicenseControl();

            licenseControl.runChecks = artifactoryConfig.PropertyGroup.LicenseControlCheck.EnabledLicenseControl;
            licenseControl.autoDiscover = artifactoryConfig.PropertyGroup.LicenseControlCheck.AutomaticLicenseDiscovery;
            licenseControl.includePublishedArtifacts = artifactoryConfig.PropertyGroup.LicenseControlCheck.IncludePublishedArtifacts;
            licenseControl.licenseViolationsRecipients = new List<string>();
            licenseControl.scopes = new List<string>();
           
            foreach (Recipient recip in artifactoryConfig.PropertyGroup.LicenseControlCheck.LicenseViolationRecipients.Recipient)
            {
                if (validateEmail(recip))
                {
                    licenseControl.licenseViolationsRecipients.Add(recip.email);
                }
                else 
                {
                    log.Warning("Invalid email address, under License Control violation recipients.");
                }                            
            }

            foreach (Scope scope in artifactoryConfig.PropertyGroup.LicenseControlCheck.ScopesForLicenseAnalysis.Scope)
            {
                licenseControl.scopes.Add(scope.value);
            }

            return licenseControl;
        }

        /// <summary>
        /// Get Timestamp sine 1/1/1970
        /// </summary>
        /// <param name="dateTime"></param>
        /// <returns>string double value</returns>
        private static string GetTimeStamp(DateTime dateTime)
        {
            DateTime baseDate = new DateTime(1970, 1, 1);
            TimeSpan diff = dateTime - baseDate;

            return diff.TotalMilliseconds.ToString();
        }

        private static string WildcardToRegex(string pattern)
        {
            if (string.IsNullOrWhiteSpace(pattern))
                return pattern;

            return "^" + Regex.Escape(pattern).Replace("\\*", ".*").Replace("\\?", ".") + "$";
        }

        private static bool PathConflicts(List<Pattern> includePatterns, List<Pattern> excludePatterns, Regex includeRegex, Regex excludeRegex, string key)
        {
            if ((includePatterns.Count > 0) && !includeRegex.Match(key).Success)
            {
                return true;
            }

            if ((excludePatterns.Count > 0) && excludeRegex.Match(key).Success)
            {
                return true;
            }

            return false;
        }

        private static bool validateEmail(Recipient recipient) 
        {
            if (recipient == null && string.IsNullOrWhiteSpace(recipient.email))
                return false;

            Regex emailRegex = new Regex(validEmailPattern, RegexOptions.IgnoreCase);

            return emailRegex.Match(recipient.email).Success;
        }

        private static void ConfigHttpClient(ArtifactoryConfig artifactoryConfig, Build build)
        {
            build.deployClient = new DeployClient();
            if (!string.IsNullOrWhiteSpace(artifactoryConfig.PropertyGroup.ConnectionTimeout))
                build.deployClient.timeout = int.Parse(artifactoryConfig.PropertyGroup.ConnectionTimeout);

            ProxySettings proxySettings = artifactoryConfig.PropertyGroup.ProxySettings;

            //Check if the user Bypass proxy settings
            if (!string.IsNullOrWhiteSpace(proxySettings.Bypass) && proxySettings.Bypass.ToLower().Equals("true")) 
            {             
                    build.deployClient.proxy = new Proxy();
                    build.deployClient.proxy.IsBypass = true;
                    return;             
            }

            /*
            * Incase that the proxy settings, is not set in the plugin level, we need
            * to check for proxy settings in the environment variables.
            */
            if (string.IsNullOrWhiteSpace(proxySettings.Host)) 
            {
                ProxySettings envVariableProxy = proxyEnvVariables();
                if (envVariableProxy != null)
                {
                    proxySettings = envVariableProxy;
                }
                else 
                {
                    build.deployClient.proxy = new Proxy();
                    build.deployClient.proxy.IsBypass = true;
                    return;  
                }
            }

            if (!string.IsNullOrWhiteSpace(proxySettings.UserName) && !string.IsNullOrWhiteSpace(proxySettings.Password))
                build.deployClient.proxy = new Proxy(proxySettings.Host, proxySettings.Port, proxySettings.UserName, proxySettings.Password);
            else
                build.deployClient.proxy = new Proxy(proxySettings.Host, proxySettings.Port);

            build.deployClient.proxy.IsBypass = false;
        }

        /// <summary>
        /// Trying to find proxy configuration on the environment variables.
        /// </summary>
        /// <returns>Proxy instance</returns>
        private static ProxySettings proxyEnvVariables()
        {
            Uri uri;
            string httpHost = Environment.GetEnvironmentVariable("http_proxy");
            if (!string.IsNullOrWhiteSpace(httpHost) && Uri.TryCreate(httpHost, UriKind.Absolute, out uri))
            {
                ProxySettings proxy = new ProxySettings();

                if (!String.IsNullOrEmpty(uri.UserInfo))
                {
                    var credentials = uri.UserInfo.Split(':');
                    if (credentials.Length > 1)
                    {
                        proxy.UserName = credentials[0];
                        proxy.Password = credentials[1];
                    }
                }

                //Regex for capturing the host and the port (if exists).
                Regex addressPattern = new Regex(@"^\w+://(?<host>[^/]+):(?<port>\d+)/?");
                Match match = addressPattern.Match(uri.GetComponents(UriComponents.HttpRequestUrl, UriFormat.SafeUnescaped));
                if (match.Success)
                {
                    if (!string.IsNullOrWhiteSpace(match.Groups["port"].Value))
                        proxy.Port = int.Parse(match.Groups["port"].Value);

                    proxy.Host = match.Groups["host"].Value;
                }

                return proxy;
            }

            return null;
        }
    }
}
