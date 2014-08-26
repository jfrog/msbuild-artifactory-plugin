using JFrog.Artifactory.Utils.httpClient;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;

namespace JFrog.Artifactory.Utils
{
    class PreemptiveHttpClient
    {
        private WebClient _httpClient;

        public PreemptiveHttpClient(string username, string password, int timeout) 
        {
            _httpClient = createHttpClient(username, password, timeout);  
        }

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

        public HttpResponse execute(String url, string method, byte[] data)
        {
            try
            {
                _httpClient.UploadData(url, method, data);
            }
            catch (WebException ex) {
                if (ex.Response != null)
                {
                    var statusCode = ((HttpWebResponse)ex.Response).StatusCode;
                    return new HttpResponse(statusCode, ex.Message);
                }

                throw new WebException(ex.Message, ex);
            }

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
