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
        
        private WebHeaderCollection _header;
        private PreemptiveHttpClient deployClient;


        public ArtifactoryHttpClient(string artifactoryUrl, string username, string password)
        {
            _artifactoryUrl = artifactoryUrl;
            _username = username;
            _password = password;
        }

        public void setTimeout() { }

        public void setProxy() { }

        public PreemptiveHttpClient getHttpClient()
        {
            if (deployClient == null)
            {
                if (connectionTimeout == 0)
                    connectionTimeout = DEFAULT_CONNECTION_TIMEOUT_SECS;

                PreemptiveHttpClient client = new PreemptiveHttpClient(_username, _password, connectionTimeout);
                deployClient = client;
            }

            return deployClient;
        }

        public void Dispose()
        {
            if (deployClient != null)
            {
                deployClient.Dispose();
            }
        }

        public int connectionTimeout { set; get; } 

    }
}
