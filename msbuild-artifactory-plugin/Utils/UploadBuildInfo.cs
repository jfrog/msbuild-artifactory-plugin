using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Build.Framework;
using Microsoft.Build.Utilities;

namespace JFrog.Artifactory.Utils
{
    public static class UploadBuildInfo
    {
        public static void UploadBuildInfoJson(string jsonString, string url, string username, string password)
        {
            try
            {
                //var credentials = new NetworkCredential(username, password);
                var bytes = Encoding.Default.GetBytes(jsonString);
                using (var client = new WebClient())
                {
                    //client.Credentials = credentials;
                    var _auth = string.Format("{0}:{1}", username, password);
                    var _enc = Convert.ToBase64String(Encoding.UTF8.GetBytes(_auth));
                    var _cred = string.Format("{0} {1}", "Basic ", _enc);
                    //Custom headers
                    client.Headers.Add(HttpRequestHeader.ContentType, "application/vnd.org.jfrog.build.BuildInfo+json");
                    client.Headers.Add(HttpRequestHeader.Authorization, _cred);
                    var response = client.UploadData(url, "PUT", bytes);

                    var responsedata = Encoding.Default.GetString(response);
                }

            }
            catch (Exception ex)
            {

                throw new WebException("UploadBuildInfo.UploadBuildInfoJson | Artifactory build info upload failed", ex);
            }
        }
    }
}
