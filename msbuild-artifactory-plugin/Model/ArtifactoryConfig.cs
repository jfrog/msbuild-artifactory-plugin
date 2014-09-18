using System;
using System.Collections.Generic;
using System.ComponentModel;
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

        [XmlElement("EnvironmentVariables")]
        public EnvironmentVariables EnvironmentVariables { get; set; }

        [XmlElement("LicenseControl")]
        public LicenseControlCheck LicenseControlCheck { get; set; }

        [XmlElement(ElementName = "ConnectionTimeout")]
        //[DefaultValue(200)]
        public string ConnectionTimeout { get; set; }

        [XmlElement("ProxySettings")]
        public ProxySettings ProxySettings { get; set; } 
    }

    public class ArtifactoryDeploy 
    {
        [XmlElement("DeployAttribute")]
        public List<DeployAttribute> DeployAttribute { get; set; }   
    }

    public class DeployAttribute
    {
        //[XmlElement(ElementName = "Input", IsNullable=false)]
        public string Input { get; set; }
        //[XmlElement(ElementName = "Output", IsNullable = false)]
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

    public class EnvironmentVariables
    {
        [XmlElement("Enable")]
        public string EnableEnvVariable { get; set; }

        [XmlElement("IncludePatterns")]
        public IncludePatterns IncludePatterns { get; set; }

        [XmlElement("ExcludePatterns")]
        public ExcludePatterns ExcludePatterns { get; set; }   
    }

    public class IncludePatterns 
    {
        [XmlElement("Pattern")]
        public List<Pattern> Pattern { get; set; } 
    }

    public class ExcludePatterns
    {
        [XmlElement("Pattern")]
        public List<Pattern> Pattern { get; set; } 
    }

    public class Pattern
    {
        [XmlAttribute]
        public string key { get; set; }
    }

    public class LicenseControlCheck 
    {
        [XmlElement("Enable")]
        public string EnableLicenseControl { get; set; }

        [XmlElement("LicenseViolationNotification")]
        public LicenseViolationNotification LicenseViolationNotification { get; set; }

        [XmlElement("DisableAutomaticLicenseDiscovery")]
        public string DisableAutomaticLicenseDiscovery { get; set; }

        [XmlElement("LimitChecksToTheFollowingScopes")]
        public LimitChecksToTheFollowingScopes LimitChecksToTheFollowingScopes { get; set; }
    }

    public class LicenseViolationNotification 
    {
        [XmlElement("Recipient")]
        public List<Recipient> Recipient { get; set; } 
    }

    public class Recipient 
    {
        [XmlAttribute]
        public string email { get; set; }
    }

    public class LimitChecksToTheFollowingScopes 
    {
        [XmlElement("Scope")]
        public List<Scope> Scope { get; set; } 
    }

    public class Scope 
    {
        [XmlAttribute]
        public string value { get; set; }
    }

    public class ProxySettings 
    {
        [XmlElement("Bypass")]
        public string Bypass { get; set; }
        [XmlElement("Host")]
        public string Host { get; set; }
        [XmlElement("Port")]
        public int Port { get; set; }
        [XmlElement("UserName")]
        public string UserName { get; set; }
        [XmlElement("Password")]
        public string Password { get; set; } 
    }
}
