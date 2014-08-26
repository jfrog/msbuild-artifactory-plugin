using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO.Compression;
using System.Xml;
using JFrog.Artifactory.Model;

namespace JFrog.Artifactory.Utils
{
    /// <summary>
    /// parse for reading .nuspec file into a  c# class
    /// although it can be done using nuget apis we prefer not
    /// to use them becuase we want thin client with small footprint
    /// </summary>
    public class NuspecParser
    {
        private readonly string _path;

        //default cotur.
        public NuspecParser(string path)
        {
            _path = path;
        }

        /// <summary>
        /// parse nuget file.
        /// </summary>
        /// <returns>nuget spec object</returns>
    
        public NuSpec Parse()
        {
            try
            {
                //we are using xmldoc without namesapce becuase the nugetname space is not the same
                //in all .nuspec due to nuget versioning.

                //read .nuspec xml file
                var xmldoc = new XmlDocument();
                xmldoc.Load(_path);
                //TODO linq to xml queireis on xmldoc - more safe
                    var spec = new NuSpec()
                    {
                        Id = innerText(xmldoc.SelectNodes("//*[local-name()='package']/*[local-name()='metadata']//*[local-name()='id']"/*//x:metadata/x:id", mgr*/)),
                        Version = innerText(xmldoc.SelectNodes("//*[local-name()='package']/*[local-name()='metadata']//*[local-name()='version']")),
                        Title = innerText(xmldoc.SelectNodes("//*[local-name()='package']/*[local-name()='metadata']//*[local-name()='title']")),
                        Authors = innerText(xmldoc.SelectNodes("//*[local-name()='package']/*[local-name()='metadata']//*[local-name()='authors']")),
                        Owners = innerText(xmldoc.SelectNodes("//*[local-name()='package']/*[local-name()='metadata']//*[local-name()='owners']")),
                        LicenseUrl = innerText(xmldoc.SelectNodes("//*[local-name()='package']/*[local-name()='metadata']//*[local-name()='licenseUrl']")),
                        ProjectUrl = innerText(xmldoc.SelectNodes("//*[local-name()='package']/*[local-name()='metadata']//*[local-name()='projectUrl']")),
                        Description = innerText(xmldoc.SelectNodes("//*[local-name()='package']/*[local-name()='metadata']//*[local-name()='description']")),
                        Language = innerText(xmldoc.SelectNodes("//*[local-name()='package']/*[local-name()='metadata']//*[local-name()='language']")),
                        Tags = innerText(xmldoc.SelectNodes("//*[local-name()='package']/*[local-name()='metadata']//*[local-name()='tags']")),
                        RequireLicenseAcceptance = innerText(xmldoc.SelectNodes("//*[local-name()='package']/*[local-name()='metadata']//*[local-name()='requireLicenseAcceptance']")),
                        IconUrl = innerText(xmldoc.SelectNodes("//*[local-name()='package']/*[local-name()='metadata']//*[local-name()='iconUrl']"))
                    };
                    return spec;

            }
            catch (Exception ex)
            {
                throw new Exception("error parsing nuget file", ex);
            }
        }

        /// <summary>
        /// extract the text of an xml node
        /// </summary>
        /// <param name="node">then xml node to extract its inner text </param>
        /// <returns>inner text</returns>
        private static string innerText(XmlNodeList node)
        {
            if (node == null) return string.Empty;
            return node.Count > 0 ? node.Item(0).InnerText : string.Empty;
        }
    }
}
