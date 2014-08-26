using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Serialization;

namespace JFrog.Artifactory.Model
{
    [XmlRootAttribute("Project", Namespace = "http://schemas.microsoft.com/developer/msbuild/2003", IsNullable = false)]
    public class ArtifactoryConfig
    {
        [XmlElement("PropertyGroup")]
        public PropertyGroup PropertyGroup { get; set; }       
    }

    public class PropertyGroup 
    {
        [XmlElement("ArtifactoryDeploy")]
        public ArtifactoryDeploy ArtifactoryDeploy { get; set; }     
    }

    public class ArtifactoryDeploy 
    {
        [XmlElement("DeployAttribute")]
        public List<DeployAttribute> DeployAttribute { get; set; }   
    }

    public class DeployAttribute
    {
        //[XmlElement("input")]
        public string Input { get; set; }
        //[XmlElement("output")]
        public string Output { get; set; }

        [XmlElement("Properties")]
        public Properties Properties { get; set; }

    }

    public class Properties 
    {
        [XmlElement("Property")]
        public List<Property> Property { get; set; }
    }

    public class Property
    {
        [XmlAttribute]
        public string key { get; set; }
        [XmlAttribute]
        public string val { get; set; }
    }
}
