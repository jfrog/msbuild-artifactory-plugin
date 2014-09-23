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
        [XmlElement("Deployments")]
        public Deployments Deployments { get; set; }

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

    public class Deployments 
    {
        [XmlElement("Deploy")]
        public List<Deploy> Deploy { get; set; }   
    }

    public class Deploy
    {
        //[XmlElement(ElementName = "Input", IsNullable=false)]
        public string InputPattern { get; set; }
        //[XmlElement(ElementName = "Output", IsNullable = false)]
        public string OutputPattern { get; set; }

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
        [XmlElement("Enabled")]
        public string EnabledEnvVariable { get; set; }

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
        [XmlElement("Enabled")]
        public string EnabledLicenseControl { get; set; }

        [XmlElement("IncludePublishedArtifacts")]
        public string IncludePublishedArtifacts { get; set; }

        [XmlElement("LicenseViolationRecipients")]
        public LicenseViolationRecipients LicenseViolationRecipients { get; set; }

        [XmlElement("EnableAutomaticLicenseDiscovery")]
        public string EnableAutomaticLicenseDiscovery { get; set; }

        [XmlElement("ScopesForLicenseAnalysis")]
        public ScopesForLicenseAnalysis ScopesForLicenseAnalysis { get; set; }
    }

    public class LicenseViolationRecipients 
    {
        [XmlElement("Recipient")]
        public List<Recipient> Recipient { get; set; } 
    }

    public class Recipient 
    {
        [XmlAttribute]
        public string email { get; set; }
    }

    public class ScopesForLicenseAnalysis 
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
