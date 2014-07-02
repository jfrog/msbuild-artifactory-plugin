using System.Text.RegularExpressions;
using JFrog.Artifactory.Model;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace JFrog.Artifactory.Utils
{
    public static class Helpers
    {
        public static string ToJsonString(this BuildInfoModel model)
        {
            // sb.AppendFormat("\"\":\"{0}\",", model);
            var sb = new StringBuilder();
            //open json root
            sb.Append("{");
            sb.AppendFormat("\"version\":\"{0}\",",model.version);
            sb.AppendFormat("\"name\":\"{0}\",", model.name);
            sb.AppendFormat("\"number\":\"{0}\",", model.number);
            sb.AppendFormat("\"buildAgent\":{{\"name\":\"{0}\",\"version\":\"{1}\"}},", model.buildAgent.name, model.buildAgent.version);
            sb.AppendFormat("\"started\":\"{0}\",", model.started);
            sb.AppendFormat("\"durationMillis\":{0},", model.durationMillis);
            var arrString = model.principal.Split('\\');
            sb.AppendFormat("\"principal\":\"{0}\",", arrString.LastOrDefault());
            sb.AppendFormat("\"artifactoryPrincipal\":\"{0}\",", model.artifactoryPrincipal);
            sb.AppendFormat("\"url\":\"{0}\",", model.url);
            sb.AppendFormat("\"vcsRevision\":\"{0}\",", model.vcsRevision);
            //TODO license control
            sb.AppendFormat("\"licenseControl\":null,", model.licenseControl);
            sb.Append("\"buildRetention\":null,");

            //system variables start
            sb.Append("\"properties\":{");
            var lastKey = model.properties.LastOrDefault();
            foreach (var kvp in model.properties)
            {
                sb.AppendFormat("\"{0}\":\"{1}\"", kvp.Key, Regex.Replace(kvp.Value,@"\\",@"\\"));
                if (kvp.Key != lastKey.Key)
                {
                    sb.Append(",");
                }
            }
            sb.Append("},");

            sb.Append("\"modules\":[");
            var modulesCount = model.modules.Count();
            for (var i=0; i < modulesCount; i++)
            {
                //module start
                sb.Append("{");

                sb.AppendFormat("\"id\":\"{0}\",", model.modules[i].id);
                sb.Append("\"dependencies\": [");
                for (var ii = 0; ii < model.modules[i].Dependencies.Count();ii++)
                {
                    sb.Append("{");
                    sb.AppendFormat("\"type\":\"{0}\",", model.modules[i].Dependencies[ii].type);
                    sb.AppendFormat("\"sha1\":\"{0}\",", model.modules[i].Dependencies[ii].sha1);
                    sb.AppendFormat("\"md5\":\"{0}\",", model.modules[i].Dependencies[ii].md5);
                    sb.AppendFormat("\"id\":\"{0}\",", model.modules[i].Dependencies[ii].name);
                    sb.Append("\"scopes\":[");
                    for (var n = 0; n < model.modules[i].Dependencies[ii].scopes.Count; n++)
                    {
                        sb.AppendFormat(
                            n + 1 < model.modules[i].Dependencies[ii].scopes.Count ? "\"{0}\"," : "\"{0}\"",
                            model.modules[i].Dependencies[ii].scopes[n]);
                    }
                    sb.Append("]}");
                    if ((ii + 1) < model.modules[i].Dependencies.Count())
                    {
                        sb.Append(",");
                    }
                }
                sb.Append("],");

                sb.Append("\"artifacts\": [");
                sb.Append("]");

                //module end
                sb.Append("}");
                if ((i + 1) < modulesCount)
                {
                    sb.Append(",");
                }
            }
            sb.Append("]"); 

            //close json root
            sb.Append("}");

            return sb.ToString();
        }
    }
}
