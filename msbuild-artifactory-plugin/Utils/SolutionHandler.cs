using JFrog.Artifactory.Model;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;

namespace JFrog.Artifactory.Utils
{
    class SolutionHandler
    {
        public Build _buildInfo { set; get; }
        private ArtifactoryBuild _task;
        private BuildInfoLog _log;
        private ArtifactoryConfig MainArtifactoryConfiguration { get; set; }

        public SolutionHandler(ArtifactoryBuild task, BuildInfoLog log) 
        {
            _task = task;
            _log = log;
            GetMainConfiguration();
        }

        private void GetMainConfiguration()
        {
            FileInfo artifactoryConfigurationFile = new FileInfo(_task.SolutionRoot + "\\.artifactory\\Artifactory.build");

            if (!artifactoryConfigurationFile.Exists)
                throw new Exception("The main configuration file are missing! (Location: solutionDir\\.artifactory)");

            StringBuilder xmlContent = MsbuildInterpreter(artifactoryConfigurationFile, _task);
            System.Xml.Serialization.XmlSerializer reader = new System.Xml.Serialization.XmlSerializer(typeof(ArtifactoryConfig));
            MainArtifactoryConfiguration = (ArtifactoryConfig)reader.Deserialize(new System.IO.StringReader(xmlContent.ToString()));
        }

        public void Execute() 
        {
            ExtractBuildProperties();
            _log.Info("Processing build modules...");
            
            ProcessMainProject(_task);
            ProccessModuleRef();
        }

        private void ExtractBuildProperties() 
        {
            _log.Info("Processing build info...");
            _buildInfo = BuildInfoExtractor.extractBuild(_task, MainArtifactoryConfiguration, _log);
        }

        private void ProcessMainProject(ArtifactoryBuild _task)
        {
            if (_task.SkipParent != null && _task.SkipParent.Equals("true"))
                return;

         /*Main project*/
            var mainProjectParser = new ProjectHandler(_task.ProjectName, _task.ProjectPath);
            /*
                 * Trying to check if Artifactory configuration file exists (overrides) in the sub module level.
                 *  If not we will use the configurations from the solution level
                 */
            if (!mainProjectParser.parseArtifactoryConfigFile(_task.ProjectPath, _task))
            {
                mainProjectParser.ArtifactoryConfiguration = MainArtifactoryConfiguration;
            }

            var mainProject = mainProjectParser.generate();
            if (mainProject != null)
            {
                BuildInfoExtractor.ProccessModule(_buildInfo, mainProject, _task);
            }
        }

        /// <summary>
        /// Find all projects referenced by the current .csporj
        /// </summary>
        private void ProccessModuleRef()
        {
            foreach (var task in _task.projRefList)
            {
                var projectParser = new ProjectHandler(task.GetMetadata("Name"), task.GetMetadata("RelativeDir"));             
                /*
                 * Trying to check if Artifactory configuration file exists (overrides) in the sub module level.
                 *  If not we will use the configurations from the solution level
                 */
                if (!projectParser.parseArtifactoryConfigFile(task.GetMetadata("RelativeDir"), _task))
                {
                    projectParser.ArtifactoryConfiguration = MainArtifactoryConfiguration;
                }
                var projectRef = projectParser.generate();
                if (projectRef != null)
                {
                    BuildInfoExtractor.ProccessModule(_buildInfo, projectRef, _task);
                }
            }
        }

        /// <summary>
        /// Replacing all the MSBuild properties placeholder $()
        /// </summary>
        /// <param name="artifactoryConfigurationFile">Full file path</param>
        /// <param name="task">MSBuild implemented Task</param>
        /// <returns>New file content after replacement.</returns>
        public static StringBuilder MsbuildInterpreter(FileInfo artifactoryConfigurationFile, ArtifactoryBuild task)
        {
            StringBuilder xmlContent = new StringBuilder(artifactoryConfigurationFile.OpenText().ReadToEnd());

            Regex propertiesPattern = new Regex(@"(\$\([\w]+\))");
            MatchCollection matchCollection = propertiesPattern.Matches(xmlContent.ToString());
            foreach (var match in matchCollection)
            {
                string propertyKey = match.ToString().Replace("$(", string.Empty).Replace(")", string.Empty);
                IEnumerable<String> msbuildProperty = BuildEngineExtensions.GetEnvironmentVariable(task.BuildEngine, propertyKey, false);
                if (msbuildProperty != null)
                {
                    xmlContent = xmlContent.Replace(match.ToString(), msbuildProperty.First());
                }
            }
            return xmlContent;
        }
    }
}
