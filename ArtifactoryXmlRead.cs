using System.Linq;
using System.Xml.Linq;
using Microsoft.Build.Utilities;
using Microsoft.Build.Framework;
using System;
namespace JFrog.Artifactory
{
    /// <summary>
    /// read artifactory build xml file, and extract the text
    /// of a single xml element.
    /// </summary>
    public class ArtifactoryXmlRead : Task
    {
        
        [Required]
        public string Property {get;set;}

        [Output]
        public string Value { get; private set; }

        #region static properties
        private static XDocument _xmldoc;
        private static DateTime? _dtStartBuild;
        #endregion

        public ArtifactoryXmlRead()
        {
            if (!_dtStartBuild.HasValue)
            {
                _dtStartBuild = DateTime.Now;
            }
            if (_xmldoc == null)
            {
                _xmldoc = XDocument.Load("artifactory.build");
            }
        }

        /// <summary>
        /// executes the task.
        /// </summary>
        /// <returns>
        /// true if the task successfully executed; otherwise, false.
        /// </returns>
        public override bool Execute()
        {
            try
            {
                if (Property == "End")
                {
                    _xmldoc = null;
                    Log.LogMessageFromText("End Task", MessageImportance.High);
                    return true;
                }

                if (Property == "StartTime")
                {
                    Log.LogMessageFromText("Artifactory Pre-Build task started", MessageImportance.High);
                    if (_dtStartBuild != null) Value = ((DateTime)_dtStartBuild).ToString("s");
                    return true;
                }

                if (_xmldoc.Root != null)
                {
                    var element = _xmldoc.Root.Descendants().FirstOrDefault(x => x.Name == Property);

                    Value = element != null ? element.Value : string.Empty;
                }

                //log success to build output window if last property task fron .csproj file
                if (Property == "BuildServer")
                    Log.LogMessageFromText("Artifactory Pre-Build task succeeded", MessageImportance.High);

                return true;
            }
            catch (Exception ex)
            {
                Log.LogErrorFromException(ex, true);
                return false;
            }
        }
    }
}
