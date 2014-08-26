using JFrog.Artifactory.Model;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace JFrog.Artifactory.Utils.regexCapturing
{
    public class BuildArtifactsMappingResolver
    {
        public static void matchMappingArtifacts(BuildArtifactsMapping mapping, string projectDirectory, Dictionary<string, string> resultMap)
        {           
            string rootDirectory = BuildArtifactsMapping.getRootDirectory(mapping);

            /*Incase we have none relative pattern, we need to add the project path for the 
             * regular expression not to recognize it as part of the capturing groups. 
             */
            if (String.IsNullOrWhiteSpace(rootDirectory) || !System.IO.Path.IsPathRooted(rootDirectory))
            {
                mapping.input = projectDirectory + "\\" + mapping.input;
                rootDirectory = projectDirectory + "\\" + rootDirectory; 
            }

            if (Directory.Exists(rootDirectory))
            {
                IEnumerable<string> fileList = Directory.GetFiles(rootDirectory, "*.*", SearchOption.AllDirectories);

                string regexNormalize = mapping.input.Replace("\\", "\\\\");
                Regex regex = new Regex(regexNormalize);
                int inputGroupsNum = regex.GetGroupNumbers().Length - 1;

                List<string> placeHoldersList = BuildArtifactsMapping.getPlaceHoldersList(mapping, inputGroupsNum);

                //Console.WriteLine("regexNormalize: " + regexNormalize);

                foreach (string file in fileList)
                {
                    Match fileMatcher = regex.Match(file);
                    if (fileMatcher.Success)
                    {
                        /* In case we didn't receive an output pattern, 
                        *   the target will be the root of the repository
                        */
                        if (String.IsNullOrWhiteSpace(mapping.output))
                        {
                            FileInfo fileInfo = new FileInfo(file);
                            resultMap.Add(file, fileInfo.Name);
                        }
                        else
                        {
                            List<string> replacementList = new List<string>();
                            string repositoryPath = mapping.output;
                            for (int i = 1; i <= placeHoldersList.Count; i++)
                                repositoryPath = repositoryPath.Replace(placeHoldersList[i - 1], fileMatcher.Groups[i].Value);

                            if (!resultMap.ContainsKey(file))
                                resultMap.Add(file, repositoryPath);
                        }

                        //Console.WriteLine("file: " + file);
                        //Console.WriteLine("output: " + resultMap[file].ToString());
                    }
                }
            }
        }
    }
}
