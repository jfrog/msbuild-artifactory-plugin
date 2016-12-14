using JFrog.Artifactory.Model;
using JFrog.Artifactory.Utils.httpClient;
using System;
using System.IO;
using System.Net;
using System.Text;

namespace JFrog.Artifactory.Utils
{
    /// <summary>
    /// Artifactory client to perform build info related tasks.
    /// <summary>
    class ArtifactoryBuildInfoClient
    {
        private static String BUILD_REST_URL = "/api/build";
        private static String BUILD_BROWSE_URL = "/webapp/builds";
        private static readonly int DEFAULT_CONNECTION_TIMEOUT_SECS = 300;

        /* Try checksum deploy of files greater than 10KB */
        private static readonly int CHECKSUM_DEPLOY_MIN_FILE_SIZE = 10240; 
        private ArtifactoryHttpClient _httpClient;
        private string _artifactoryUrl;
        private BuildInfoLog _log;

        //public ArtifactoryBuildInfoClient(string artifactoryUrl) {
        //    (artifactoryUrl, null, null);
        //}

        public ArtifactoryBuildInfoClient(string artifactoryUrl, string username, string password, BuildInfoLog log)
        {
            //Removing ending slash
            if ((!string.IsNullOrEmpty(artifactoryUrl)) && artifactoryUrl.EndsWith("/")) 
            {
                artifactoryUrl = artifactoryUrl.Remove(artifactoryUrl.LastIndexOf('/'));
            }

            _httpClient = new ArtifactoryHttpClient(artifactoryUrl, username, password);
            _artifactoryUrl = artifactoryUrl; 
            _log = log;
        }

        public void setProxy(DeployClient deployClient)
        {
            if (deployClient.proxy.IsBypass)
                return;

            WebProxy proxy = new WebProxy(deployClient.proxy.Host, deployClient.proxy.Port);
            proxy.UseDefaultCredentials = false;
            if (deployClient.proxy.IsCredentialsExists) 
            {
                proxy.Credentials = new NetworkCredential(deployClient.proxy.Username, deployClient.proxy.Password);
            }
            
            _httpClient.getHttpClient().setProxy(proxy);
        }

        public void setConnectionTimeout(DeployClient deployClient) 
        {
            if (deployClient.timeout == 0)
                _httpClient.getHttpClient().setConnectionTimeout(DEFAULT_CONNECTION_TIMEOUT_SECS);
            else
                _httpClient.getHttpClient().setConnectionTimeout(deployClient.timeout);
        }

        public void sendBuildInfo(Build buildInfo) {
            try
            {               
                sendBuildInfo(buildInfo.ToJsonString());
                _log.Info("Build successfully deployed. Browse it in Artifactory under " + string.Format(_artifactoryUrl + BUILD_BROWSE_URL) +
                    "/" + buildInfo.name + "/" + buildInfo.number + "/" + buildInfo.started + "/");
            }
            catch (Exception ex)
            {
                _log.Error("Could not publish the build-info object: " + ex.InnerException);
                throw new Exception("Could not publish build-info", ex);
            }            
        }

        public void sendBuildInfo(String buildInfoJson)
        {
            string url = _artifactoryUrl + BUILD_REST_URL;

            try
            {
                var bytes = Encoding.Default.GetBytes(buildInfoJson);
                {
                    //Custom headers
                    WebHeaderCollection headers = new WebHeaderCollection();
                    headers.Add(HttpRequestHeader.ContentType, "application/vnd.org.jfrog.build.BuildInfo+json");
                    _httpClient.getHttpClient().setHeader(headers);
                        
                    HttpResponse response = _httpClient.getHttpClient().execute(url, "PUT", bytes);
                    
                    ///When sending build info, Expecting for NoContent (204) response from Artifactory 
                    if (response._statusCode != HttpStatusCode.NoContent) 
                    {
                        throw new WebException("Failed to send build info:" + response._message);  
                    }                   
                }
            }
            catch (Exception we) {
                _log.Error(we.Message, we);
                throw new WebException("Exception in Uploading BuildInfo: " + we.Message, we);
            }
        }

        public void deployArtifact(DeployDetails details) 
        {
            string deploymentPath = String.IsNullOrEmpty(details.targetRepository) ? _artifactoryUrl + "/" + details.artifactPath : _artifactoryUrl + "/" + details.targetRepository + "/" + details.artifactPath;
            _log.Info("Deploying artifact: " + deploymentPath);

            if (tryChecksumDeploy(details, _artifactoryUrl))
            {
                return;
            }

            //Custom headers
            WebHeaderCollection headers = new WebHeaderCollection();
            headers = createHttpPutMethod(details);
            headers.Add(HttpRequestHeader.ContentType, "binary/octet-stream");

            /*
             * "100 (Continue)" status is to allow a client that is sending a request message with a request body to determine if the origin server is
             *  willing to accept the request (based on the request headers) before the client sends the request body.
             */
            //headers.Add("Expect", "100-continue");

            _httpClient.getHttpClient().setHeader(headers);

            byte[] data = File.ReadAllBytes(details.file.FullName);

            

            /* Add properties to the artifact, if any */
            deploymentPath = deploymentPath + details.properties;
          
            HttpResponse response = _httpClient.getHttpClient().execute(deploymentPath, "PUT", data);

            ///When deploying artifact, Expecting for Created (201) response from Artifactory 
            if ((response._statusCode != HttpStatusCode.OK) && (response._statusCode != HttpStatusCode.Created))
            {
                _log.Error("Error occurred while publishing artifact to Artifactory: " + details.file);
                throw new WebException("Failed to deploy file:" + response._message);
            }    
        }

        /// <summary>
        ///  Deploy an artifact to the specified destination by checking if the artifact content already exists in Artifactory
        /// </summary>
        private Boolean tryChecksumDeploy(DeployDetails details, String uploadUrl) 
        {
            // Try checksum deploy only on file size greater than CHECKSUM_DEPLOY_MIN_FILE_SIZE
            if (details.file.Length < CHECKSUM_DEPLOY_MIN_FILE_SIZE) {
                _log.Debug("Skipping checksum deploy of file size " + details.file.Length + " , falling back to regular deployment.");
                return false;
            }

            string checksumUrlPath = uploadUrl + "/" + details.targetRepository + "/" + details.artifactPath;

            /* Add properties to the artifact, if any */
            checksumUrlPath = checksumUrlPath + details.properties;

            WebHeaderCollection headers = createHttpPutMethod(details);
            headers.Add("X-Checksum-Deploy", "true");
            headers.Add(HttpRequestHeader.ContentType, "application/vnd.org.jfrog.artifactory.storage.ItemCreated+json");

            _httpClient.getHttpClient().setHeader(headers);
            HttpResponse response = _httpClient.getHttpClient().execute(checksumUrlPath, "PUT");

            ///When sending Checksum deploy, Expecting for Created (201) or Success (200) responses from Artifactory 
            if (response._statusCode == HttpStatusCode.Created || response._statusCode == HttpStatusCode.OK)
            {

                _log.Debug(string.Format("Successfully performed checksum deploy of file {0} : {1}", details.file.FullName, details.sha1));
                return true;
            }
            else 
            {
                _log.Debug(string.Format("Failed checksum deploy of checksum '{0}' with statusCode: {1}", details.sha1, response._statusCode));
            }

            return false;
        }

        /// <summary>
        /// Typical PUT header with Checksums, for deploying files to Artifactory 
        /// </summary>
        private WebHeaderCollection createHttpPutMethod(DeployDetails details)
        {
            WebHeaderCollection putHeaders = new WebHeaderCollection();
            putHeaders.Add("X-Checksum-Sha1", details.sha1);
            putHeaders.Add("X-Checksum-Md5", details.md5);

            return putHeaders;
        }

        public void Dispose()
        {
            if (_httpClient != null)
            {
                _httpClient.Dispose();
            }
        }
    }
}
