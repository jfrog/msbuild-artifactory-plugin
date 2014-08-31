using System;
using System.Collections.Generic;
using Microsoft.Build.Evaluation;
using System.Xml.Linq;
using System.Text;
using System.Net;

namespace JFrog.Artifactory.Model
{
    public class ProjectModel
    {
        public string AssemblyName { get; set; }
        public string projectDirectory { get; set; }
        public List<ProjectMetadata> LstProjectMetadata { get; set; }
        public List<DeployAttribute> artifactoryDeploy { get; set; }

        public class DeployAttribute
        {
            public string InputPattern { get; set; }
            public string OutputPattern { get; set; }
            public string properties { get; set; }
        }

        /// <summary>
        /// Preparing the properties (Matrix params) to suitable Url Query 
        /// </summary>
        /// <param name="matrixParam"></param>
        /// <returns></returns>
        public static string buildMatrixParamsString(List<Property> matrixParam) 
        {
            StringBuilder matrix = new StringBuilder();

            if (matrixParam != null)
            {
                matrixParam.ForEach(prop => matrix.Append(";").Append(WebUtility.UrlEncode(prop.key)).
                    Append("=").Append(WebUtility.UrlEncode(prop.val)));

            }
            return matrix.ToString();
        
        }

    }
}
