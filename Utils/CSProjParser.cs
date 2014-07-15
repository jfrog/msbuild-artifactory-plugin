using JFrog.Artifactory.Model;
using Microsoft.Build.Evaluation;
using System;
using System.Linq;

namespace JFrog.Artifactory.Utils
{
    /// <summary>
    /// extract project metadata from a csporj file.
    /// the metadata extracted is only for "refrence"
    /// </summary>
    public class CSProjParser
    {
        private string ProjectPath {get;set;}

        //default cotur.
        public CSProjParser(string projectPath)
        {
            ProjectPath = projectPath;
        }

        /// <summary>
        /// parse a csproj file and return its refrences for uget, local and other projects
        /// </summary>
        /// <returns>list of MetaData items</returns>
        public ProjectRefModel Parse()
        {
            try
            {
                var project = new Project(ProjectPath);
                
                return new ProjectRefModel
                {
                   AssemblyName = project.GetProperty("AssemblyName").EvaluatedValue,
                   LstProjectMetadata = project.GetItems("Reference")
                        .Where(x => x.Metadata.Count > 0)
                        .Select(x => x.Metadata.FirstOrDefault( m=> m.Name.Contains("HintPath")))
                        .ToList()

                };

            }
            catch (Exception ex)
            {
                //TODO handle this exception - do not hide...
                return null;
            }
           

        }
        
    }
}
