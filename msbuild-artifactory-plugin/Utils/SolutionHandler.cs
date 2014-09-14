using JFrog.Artifactory.Model;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

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
            FileInfo artifactoryConfigurationFile = new FileInfo(_task.SolutionRoot + "\\.artifactory\\artifactory.build");

            if (!artifactoryConfigurationFile.Exists)
                throw new Exception("The main configuration file are missing! (Location: solutionDir\\.artifactory)");

            System.Xml.Serialization.XmlSerializer reader = new System.Xml.Serialization.XmlSerializer(typeof(ArtifactoryConfig));
            System.IO.StreamReader file = new System.IO.StreamReader(artifactoryConfigurationFile.FullName);
            MainArtifactoryConfiguration = (ArtifactoryConfig)reader.Deserialize(file);
        }

        public void Execute() 
        {
            ExtractBuildProperties();
            _log.Info("Processing build modules...");
            ProcessMainProject();
            ProccessModuleRef();
        }

        private void ExtractBuildProperties() 
        {
            _log.Info("Processing build info...");
            _buildInfo = BuildInfoExtractor.extractBuild(_task, MainArtifactoryConfiguration);
        }

        private void ProcessMainProject()
        {
         /*Main project*/
            var mainProjectParser = new ProjectHandler(_task.ProjectName, _task.ProjectPath);
            /*
                 * Trying to check if Artifactory configuration file exists (overrides) in the sub module level.
                 *  If not we will use the configurations from the solution level
                 */
            if (!mainProjectParser.parseArtifactoryConfigFile(_task.ProjectPath))
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
                if (!projectParser.parseArtifactoryConfigFile(task.GetMetadata("RelativeDir")))
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

        //add system variables to the json file
            //log.Info("Collecting system variables...");
            //build.properties = AddSystemVariables();


    }
}
