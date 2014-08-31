using JFrog.Artifactory.Utils.httpClient;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;

namespace JFrog.Artifactory.Utils
{

    /// <summary>
    /// Wrapper of HttpClient that forces preemptive BASIC authentication if user credentials exist.
    /// </summary>
    class PreemptiveHttpClient
    {
        private WebClient _httpClient;

        public PreemptiveHttpClient(string username, string password, int timeout) 
        {
            _httpClient = createHttpClient(username, password, timeout);  
        }

        /// <summary>
        /// Execute HTTP request
        /// </summary>
        /// <param name="url">address</param>
        /// <param name="method">HTTP Method Definitions</param>
        /// <returns>Response object</returns>
        public HttpResponse execute(String url, string method)
        {
            try
            {
                _httpClient.UploadString(url, method, String.Empty);
            }
            catch (WebException ex)
            {
                if (ex.Response != null)
                {
                    var statusCode = ((HttpWebResponse)ex.Response).StatusCode;
                    return new HttpResponse(statusCode, ex.Message);
                }

                throw new WebException(ex.Message, ex);
            }

            return new HttpResponse(HttpStatusCode.OK, "Request Succeeded");
        }

        /// <summary>
        /// Execute HTTP request
        /// </summary>
        /// <param name="url">address</param>
        /// <param name="method">HTTP Method Definitions</param>
        /// <param name="data">data buffer to send</param>
        /// <returns>Response object</returns>
        public HttpResponse execute(String url, string method, byte[] data)
        {
            try
            {
                _httpClient.UploadData(url, method, data);
            }
            catch (WebException ex) {

                //Creating custom response for upper use.
                if (ex.Response != null)
                {
                    var statusCode = ((HttpWebResponse)ex.Response).StatusCode;
                    return new HttpResponse(statusCode, ex.Message);
                }

                throw new WebException(ex.Message, ex);
            }

            //If no exception, the response is in OK status (200)
            return new HttpResponse(HttpStatusCode.OK, "Request Succeeded");
        }

        public void setHeader(WebHeaderCollection newHeader)
        {
            _httpClient.Headers = newHeader;
        }

        public void Dispose()
        {
            if (_httpClient != null)
            {
                _httpClient.Dispose();
            }
        }

        private WebClient createHttpClient(string username, string password, int timeout) 
        {
            CustomWebClient client = new CustomWebClient(username, password, timeout);
            client.Credentials = new NetworkCredential(username, password);
          
            return client;
        }

        
    }
}
