using Microsoft.Build.Utilities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;

namespace JFrog.Artifactory.Utils
{
    class ArtifactoryHttpClient
    {
        private static readonly int DEFAULT_CONNECTION_TIMEOUT_SECS = 300;

        private string _artifactoryUrl;
        private string _username;
        private string _password;
        private int connectionTimeout = DEFAULT_CONNECTION_TIMEOUT_SECS;
        private WebHeaderCollection _header;

        private WebClient deployClient;


        public ArtifactoryHttpClient(string artifactoryUrl, string username, string password) 
        {
            _artifactoryUrl = artifactoryUrl;
            _username = username;
            _password = password;
            _header = setHeader();
        }

        public void setTimeout() { }

        public void setProxy() { }

        public WebClient getHttpClient() 
        {
            if (deployClient == null)
            {
                WebClient httpClient = new WebClient();                          
                deployClient = httpClient;
                deployClient.Credentials = new NetworkCredential(_username, _password);
            }

            return deployClient;
        }

        public void Dispose(){
            if (deployClient != null)
            {
                deployClient.Dispose();
            }
        }

        private WebHeaderCollection setHeader()
        {
            var _auth = string.Format("{0}:{1}", _username, _password);
            var _enc = Convert.ToBase64String(Encoding.UTF8.GetBytes(_auth));
            var _cred = string.Format("{0} {1}", "Basic ", _enc);
            WebHeaderCollection header = new WebHeaderCollection();
            header.Add(HttpRequestHeader.Authorization, _cred);
            
            return header;
        }
    }
}
