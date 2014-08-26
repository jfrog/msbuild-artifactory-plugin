using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace JFrog.Artifactory.Utils.regexCapturing
{
    public class BuildArtifactsMapping
    {
        private const string PLACE_HOLDER_PATTERN = "\\$([1-9]*)";

        public string input { get; set; }

        public string output { get; set; }

        public BuildArtifactsMapping() { }

        public BuildArtifactsMapping(string left, string right)
        {
            input = left;
            output = right;
        }
        public static List<string> getPlaceHoldersList(BuildArtifactsMapping mapping, int inputGroupsNum)
        {
            List<string> placeHoldersList = new List<string>();

            if (String.IsNullOrWhiteSpace(mapping.output) || inputGroupsNum < 1) 
            {
                // No output mapping or input has no capturing groups
                return placeHoldersList;
            }

            Match placeHoldersMatcher = Regex.Match(mapping.output, PLACE_HOLDER_PATTERN);
            int capturingNumber = 1;
            while (placeHoldersMatcher.Success)
            {
                int placeHolderGroupNum;
                string group = placeHoldersMatcher.Groups[1].Value;
                try 
                {
                    placeHolderGroupNum = Int32.Parse(group);

                    if (placeHolderGroupNum > inputGroupsNum) 
                    {
                        throw new ArgumentException(String.Format("Place holder number '{0}' exceeds the number of capture groups ('{1}') in the matching mapping input '{2}'",
                            placeHolderGroupNum, inputGroupsNum, mapping.input));
                    }
                }
                catch (FormatException e)
                {                 
                    throw new ArgumentException(String.Format("Place holder '{0}' from mapping output '{1}' is not a valid number", group, mapping.output));
                }

                placeHoldersList.Add("$" + capturingNumber);
                capturingNumber++;
                placeHoldersMatcher = placeHoldersMatcher.NextMatch();
            }

            return placeHoldersList;
        }

        public static string getRegexPattern(BuildArtifactsMapping mapping)
        {
            string rootDirectory = getRootDirectory(mapping);

            if (String.IsNullOrWhiteSpace(rootDirectory))
                return mapping.input;

            return mapping.input.Replace(rootDirectory, String.Empty);
        }

        public static string getRootDirectory(BuildArtifactsMapping mapping)
        {
            string input = mapping.input;
            int firstParenthesesIndex = input.IndexOf("(");

            return input.Substring(0, firstParenthesesIndex);
        }

        // override object.Equals
        public override bool Equals(Object obj)
        {
            if (obj == null)
            {
                return false;
            }

            BuildArtifactsMapping p = obj as BuildArtifactsMapping;
            if ((System.Object)p == null)
            {
                return false;
            }

            if (p.input.Equals(this.input) && p.output.Equals(this.output))
            {
                return true;
            }

            return false;
        }

        // override object.GetHashCode
        public override int GetHashCode()
        {
            int hash = 17;
            hash = hash * 23 + this.input.GetHashCode();
            hash = hash * 23 + this.output.GetHashCode();

            return hash;
        }
    }
}
