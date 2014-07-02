using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace JFrog.Artifactory.Model
{
    /// <summary>
    /// simple POCO representing the .nuspec file
    /// </summary>
    public class NuSpec
    {
        public string Id { get; set; }
        public string Version{ get; set; }
        public string Title{ get; set; }
        public string Authors{ get; set; }
        public string Owners{ get; set; }
        public string LicenseUrl{ get; set; }
        public string ProjectUrl{ get; set; }
        public string IconUrl{ get; set; }
        public string RequireLicenseAcceptance{ get; set; }
        public string Description{ get; set; }
        public string Language{ get; set; }
        public string Tags { get; set; }
        public string SHA1 { get; set; }

    }
}
