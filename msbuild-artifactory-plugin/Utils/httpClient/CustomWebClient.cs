using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;

namespace JFrog.Artifactory.Utils.httpClient
{
    /// <summary>
    /// Extends the WebClient class, for manipulating the Web Request, and Web Response. 
    /// Creating the Preemptive client.
    /// </summary>
    class CustomWebClient : WebClient
    {
        private string _username;
        private string _password;
        private readonly string CLIENT_VERSION = "unknown";///!!!!!!!! for now

        public int _timeout { set; get; }

        public CustomWebClient(string username, string password)
        {
            this._username = username;
            this._password = password;
        }

        public CustomWebClient(string username, string password, int timeout) {
            this._username = username;
            this._password = password;
            this._timeout = timeout;
        }

        protected override WebRequest GetWebRequest(Uri uri)
        {
            WebRequest request = base.GetWebRequest(uri);
            request.Timeout = _timeout * 1000;

            //Add Basic Authentication 
            var _auth = string.Format("{0}:{1}", _username, _password);
            var _enc = Convert.ToBase64String(Encoding.UTF8.GetBytes(_auth));
            var _cred = string.Format("{0} {1}", "Basic ", _enc);
            request.Headers["Authorization"] = _cred;
            
            //Add Agent
            ((HttpWebRequest)request).UserAgent = "ArtifactoryBuildClient/.NET" + CLIENT_VERSION;

            return request;
        }

        protected override WebResponse GetWebResponse(WebRequest request)
        {
            HttpWebResponse response = (HttpWebResponse)base.GetWebResponse(request);

            //A way to pass the Web Response object.
            //System.Net.WebClient by default returns only status OK (200)
            if (response.StatusCode != HttpStatusCode.OK) {
                throw new WebException(response.StatusCode.ToString(), null, WebExceptionStatus.SendFailure, response);
            }

            return response;
        }
    }
}
