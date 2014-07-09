using JFrog.Artifactory.Model;
using Microsoft.Build.Framework;
using Microsoft.Build.Utilities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;

namespace JFrog.Artifactory.Utils
{
    class ArtifactoryBuildInfoClient
    {
        private static String BUILD_REST_URL = "/api/build";
        private static String BUILD_BROWSE_URL = "/webapp/builds";
        private WebClient _httpClient;
        private string _artifactoryUrl;
        private TaskLoggingHelper _log;

        public ArtifactoryBuildInfoClient(string artifactoryUrl) {
            //(artifactoryUrl, null, null);
        }

        public ArtifactoryBuildInfoClient(string artifactoryUrl, string username, string password, TaskLoggingHelper log)
        {
            _httpClient = new WebClient();
            //client.Credentials = credentials;
            _artifactoryUrl = artifactoryUrl; 
            var _auth = string.Format("{0}:{1}", username, password);
            var _enc = Convert.ToBase64String(Encoding.UTF8.GetBytes(_auth));
            var _cred = string.Format("{0} {1}", "Basic ", _enc);

            _httpClient.Headers.Add(HttpRequestHeader.Authorization, _cred);
            _log = log;
        
        }

        public void setTimeout() { }

        public void setProxy() { }

        public void sendBuildInfo(Build buildInfo) {

            try
            {
                sendBuildInfo(buildInfo.ToJsonString());
                _log.LogMessageFromText("Build successfully deployed. Browse it in Artifactory under " + string.Format(_artifactoryUrl + BUILD_BROWSE_URL) +
                    "/" + buildInfo.name + "/" + buildInfo.version + "/" + buildInfo.started + "/", MessageImportance.High);
            }
            catch (Exception ex)
            {
                _log.LogMessageFromText("Could not publish the build-info object: " + ex.InnerException, MessageImportance.High);
                throw new Exception("Could not publish build-info", ex);
            }            
        }

        public void sendBuildInfo(String buildInfoJson)
        {
            string url = _artifactoryUrl + BUILD_REST_URL;
            //upload json file to artifactory
            _log.LogMessageFromText("Uploading build info to Artifactory...", MessageImportance.High);

            try
            {
                var bytes = Encoding.Default.GetBytes(buildInfoJson);
                {
                    //Custom headers
                    _httpClient.Headers.Add(HttpRequestHeader.ContentType, "application/vnd.org.jfrog.build.BuildInfo+json");
                    var response = _httpClient.UploadData(url, "PUT", bytes);

                    var responsedata = Encoding.Default.GetString(response);
                }
            }
            catch (WebException we) {
                throw new WebException("UploadBuildInfo.UploadBuildInfoJson | Artifactory build info upload failed", we);
            }
        }

        public void deployArtifact() { 
        
        
        }
    }
}
