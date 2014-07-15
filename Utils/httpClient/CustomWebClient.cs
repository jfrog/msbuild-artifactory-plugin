using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;

namespace JFrog.Artifactory.Utils.httpClient
{
    class CustomWebClient : WebClient
    {
        private int _timeout;

        private string _username;

        private string _password;

        private readonly string CLIENT_VERSION = "unknown";///!!!!!!!! for now

        public CustomWebClient(string username, string password, int timeout) {
            _username = username;
            _password = password;
            _timeout = timeout;
        }

        protected override WebRequest GetWebRequest(Uri uri)
        {
            WebRequest request = base.GetWebRequest(uri);
            request.Timeout = _timeout * 1000;

            //Add preemtive 
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

            if (response.StatusCode != HttpStatusCode.OK) {
                throw new WebException(response.StatusCode.ToString(), null, WebExceptionStatus.SendFailure, response);
            }

            return response;
        }
    }
}
