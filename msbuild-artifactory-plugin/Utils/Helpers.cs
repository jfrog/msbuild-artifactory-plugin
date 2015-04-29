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
        public static string ToJsonString(this Build model)
        {
            // sb.AppendFormat("\"\":\"{0}\",", model);
            var json = new StringBuilder();
            //open json root
            json.Append("{");
            json.AppendFormat("\"version\":\"{0}\",", model.version);
            json.AppendFormat("\"name\":\"{0}\",", model.name);
            json.AppendFormat("\"number\":\"{0}\",", model.number);
            // json.AppendFormat("\"type\":\"{0}\",", model.type);
            json.AppendFormat("\"buildAgent\":{{\"name\":\"{0}\",\"version\":\"{1}\"}},", model.buildAgent.name, model.buildAgent.version);
            json.AppendFormat("\"agent\":{{\"name\":\"{0}\",\"version\":\"{1}\"}},", model.agent.name, model.agent.version);
            json.AppendFormat("\"started\":\"{0}\",", model.started);
            json.AppendFormat("\"durationMillis\":{0},", model.durationMillis);
            var arrString = model.principal.Split('\\');
            json.AppendFormat("\"principal\":\"{0}\",", arrString.LastOrDefault());
            json.AppendFormat("\"artifactoryPrincipal\":\"{0}\",", model.artifactoryPrincipal);
            json.AppendFormat("\"url\":\"{0}\",", model.url);
            json.AppendFormat("\"vcsRevision\":\"{0}\",", model.vcsRevision);

            createLicenseControl(json, model);
            createBlackDuck(json, model);

            json.Append("\"buildRetention\":null,");

            createEnvVar(json, model);           

            json.Append("\"modules\":[");
            var modulesCount = model.modules.Count();
            for (var i = 0; i < modulesCount; i++)
            {
                createModule(model, json, i);
                if ((i + 1) < modulesCount)
                {
                    json.Append(",");
                }
            }
            json.Append("]");

            //close json root
            json.Append("}");

            return json.ToString();
        }

        private static void createModule(Build model, StringBuilder sb, int i)
        {
            //module start
            sb.Append("{");

            sb.AppendFormat("\"id\":\"{0}\",", model.modules[i].id);
            sb.Append("\"dependencies\": [");
            for (var ii = 0; ii < model.modules[i].Dependencies.Count(); ii++)
            {
                sb.Append("{");
                sb.AppendFormat("\"type\":\"{0}\",", model.modules[i].Dependencies[ii].type);
                sb.AppendFormat("\"sha1\":\"{0}\",", model.modules[i].Dependencies[ii].sha1);
                sb.AppendFormat("\"md5\":\"{0}\",", model.modules[i].Dependencies[ii].md5);
                sb.AppendFormat("\"id\":\"{0}\",", model.modules[i].Dependencies[ii].id);
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

            foreach (var artifact in model.modules[i].Artifacts)
            {
                sb.Append("{");
                sb.AppendFormat("\"type\":\"{0}\",", artifact.type);
                sb.AppendFormat("\"sha1\":\"{0}\",", artifact.sha1);
                sb.AppendFormat("\"md5\":\"{0}\",", artifact.md5);
                sb.AppendFormat("\"name\":\"{0}\"", artifact.name);
                sb.Append("},");
            }

            if (model.modules[i].Artifacts.Count != 0)
                sb = (sb.Remove(sb.Length - 1, 1));

            sb.Append("]");

            //module end
            sb.Append("}");
        }

        //Artifactory License Control
        private static void createLicenseControl(StringBuilder json, Build model) 
        {
            json.Append("\"licenseControl\":{");
            json.AppendFormat("\"runChecks\":\"{0}\",", model.licenseControl.runChecks);
            json.AppendFormat("\"includePublishedArtifacts\":\"{0}\",", model.licenseControl.includePublishedArtifacts);
            json.AppendFormat("\"autoDiscover\":\"{0}\",", model.licenseControl.autoDiscover);
            json.Append("\"licenseViolationRecipients\":[");

            var lastRecip = model.licenseControl.licenseViolationsRecipients.LastOrDefault();
            foreach (string recip in model.licenseControl.licenseViolationsRecipients)
            {
                json.AppendFormat("\"{0}\"", recip);
                if (!lastRecip.Equals(recip))
                    json.Append(",");
            }
            json.Append("],");
            json.Append("\"scopes\":[");
            var lastScope = model.licenseControl.scopes.LastOrDefault();
            foreach (string scope in model.licenseControl.scopes)
            {
                json.AppendFormat("\"{0}\"", scope);
                if (!lastScope.Equals(scope))
                    json.Append(",");
            }
            json.Append("]");
            json.Append("},");
        
        }

        //BlackDuck
        private static void createBlackDuck(StringBuilder json, Build model) 
        {             
            if (model.blackDuckGovernance == null)
                return;

            json.Append("\"governance\":{");
            json.Append("\"blackDuckProperties\":{");
            json.AppendFormat("\"runChecks\":\"{0}\",", model.blackDuckGovernance.runChecks);
            json.AppendFormat("\"appName\":\"{0}\",", model.blackDuckGovernance.appName);
            json.AppendFormat("\"appVersion\":\"{0}\",", model.blackDuckGovernance.appVersion);

            if (model.blackDuckGovernance.reportRecipients.Count > 0)
            {
                json.Append("\"reportRecipients\":");
                var lastRecip = model.blackDuckGovernance.reportRecipients.LastOrDefault();
                string recipes = string.Empty;
                foreach (string recip in model.blackDuckGovernance.reportRecipients)
                {
                    recipes += recip;
                    if (!lastRecip.Equals(recip))
                        recipes += " ";
                }
                json.AppendFormat("\"{0}\"", recipes);
                json.Append(",");
            }

            if (model.blackDuckGovernance.scopes.Count > 0)
            {
                json.Append("\"scopes\":");
                var lastScope = model.blackDuckGovernance.scopes.LastOrDefault();
                string scopes = string.Empty;
                foreach (string scope in model.blackDuckGovernance.scopes)
                {
                    scopes += scope;
                    if (!lastScope.Equals(scope))
                        scopes += " ";
                }
                json.AppendFormat("\"{0}\"", scopes);
                json.Append(",");
            }

            json.AppendFormat("\"includePublishedArtifacts\":\"{0}\",", model.blackDuckGovernance.includePublishedArtifacts);
            json.AppendFormat("\"autoCreateMissingComponentRequests\":\"{0}\",", model.blackDuckGovernance.autoCreateMissingComponentRequests);
            json.AppendFormat("\"autoDiscardStaleComponentRequests\":\"{0}\"", model.blackDuckGovernance.autoDiscardStaleComponentRequests);
            json.Append("}");
            json.Append("},");       
        }

        //system variables start
        private static void createEnvVar(StringBuilder json, Build model)
        {        
            if (model.properties != null && model.properties.Count > 0)
            {
                json.Append("\"properties\":{");
                var lastKey = model.properties.LastOrDefault();

                String quoteMatch = @"""";
                String doubleBackSlashMatch = @"\\";

                foreach (var kvp in model.properties)
                {
                    String cleanValue = Regex.Replace(kvp.Value, doubleBackSlashMatch, doubleBackSlashMatch).Replace(quoteMatch, @"\""");
                    json.AppendFormat("\"{0}\":\"{1}\"", kvp.Key, cleanValue);
                    if (kvp.Key != lastKey.Key)
                    {
                        json.Append(",");
                    }
                }
                json.Append("},");
            }
        }
    }

    
}
