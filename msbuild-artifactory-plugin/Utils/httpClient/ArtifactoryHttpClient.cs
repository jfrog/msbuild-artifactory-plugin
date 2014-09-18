using Microsoft.Build.Utilities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;

namespace JFrog.Artifactory.Utils
{
    /// <summary>
    /// Wrapper of Preemptive that creates Singleton of it.
    /// Handle Client properties like Proxy, timeout, credentials
    /// </summary>
    class ArtifactoryHttpClient
    {      
        private string _artifactoryUrl;
        private string _username;
        private string _password;

        private PreemptiveHttpClient deployClient;

        public ArtifactoryHttpClient(string artifactoryUrl, string username, string password)
        {
            _artifactoryUrl = artifactoryUrl;
            _username = username;
            _password = password;
        }

        public PreemptiveHttpClient getHttpClient()
        {
            if (deployClient == null)
            {              
                PreemptiveHttpClient client = new PreemptiveHttpClient(_username, _password);
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
    }
}
