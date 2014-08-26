using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace JFrog.Artifactory.Utils
{
    /// <summary>
    /// find the nuget package directory from a given directory path
    /// </summary>
    public class DirectoryCrawler
    {
        /// <summary>
        /// find pacakges path
        /// </summary>
        /// <param name="path">the path to scan</param>
        /// <returns>packages path</returns>
        public DirectoryInfo FindPath(string path)
        {
            var fileInfo = new FileInfo(path);
            return findPackagesPath(fileInfo.Directory);

        }

        /// <summary>
        /// search ina given path where is the nuget library
        /// </summary>
        /// <param name="directory">the path to query on</param>
        /// <returns>the path where the packages diretory is located</returns>
        private static DirectoryInfo findPackagesPath(DirectoryInfo directory)
        {
            while (true)
            {
                if (directory.Parent == null) return null;
                if (directory.Parent.Name == "packages") return directory;
                directory = directory.Parent;
            }
        }
    }
}
