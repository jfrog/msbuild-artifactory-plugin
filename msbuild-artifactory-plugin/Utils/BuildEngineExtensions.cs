using Microsoft.Build.Execution;
using Microsoft.Build.Framework;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace JFrog.Artifactory
{
    /// <summary>
    /// Extension of the MSBuild BuildEngine.
    /// </summary>
    public static class BuildEngineExtensions
    {
        const BindingFlags bindingFlags = BindingFlags.NonPublic | BindingFlags.FlattenHierarchy | BindingFlags.Instance | BindingFlags.Public;

        /// <summary>
        /// A way to collect all the properties in the MSBuild build context
        /// </summary>
        /// <param name="buildEngine">MSBuild implemented Task</param>
        /// <param name="key">Property key</param>
        /// <param name="throwIfNotFound"></param>
        /// <returns>Property value</returns>
        public static IEnumerable<String> GetEnvironmentVariable(this IBuildEngine buildEngine, string key, bool throwIfNotFound)
        {
            var projectInstance = GetProjectInstance(buildEngine);

            var items = projectInstance.Items
                .Where(x => string.Equals(x.ItemType, key, StringComparison.InvariantCultureIgnoreCase)).ToList();
            if (items.Count > 0)
            {
                return items.Select(x => x.EvaluatedInclude);
            }


            var properties = projectInstance.Properties
                .Where(x => string.Equals(x.Name, key, StringComparison.InvariantCultureIgnoreCase)).ToList();
            if (properties.Count > 0)
            {
                return properties.Select(x => x.EvaluatedValue);
            }

            if (throwIfNotFound)
            {
                throw new Exception(string.Format("Could not extract from '{0}' environmental variables.", key));
            }

            return null;
        }

        private static ProjectInstance GetProjectInstance(IBuildEngine buildEngine)
        {
            var buildEngineType = buildEngine.GetType();
            var targetBuilderCallbackField = buildEngineType.GetField("targetBuilderCallback", bindingFlags);
            if (targetBuilderCallbackField == null)
            {
                throw new Exception("Could not extract targetBuilderCallback from " + buildEngineType.FullName);
            }
            var targetBuilderCallback = targetBuilderCallbackField.GetValue(buildEngine);
            var targetCallbackType = targetBuilderCallback.GetType();
            var projectInstanceField = targetCallbackType.GetField("projectInstance", bindingFlags);
            if (projectInstanceField == null)
            {
                throw new Exception("Could not extract projectInstance from " + targetCallbackType.FullName);
            }
            return (ProjectInstance)projectInstanceField.GetValue(targetBuilderCallback);
        }
    }
}
