using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using JFrog.Artifactory.Utils.regexCapturing;
using System.Text.RegularExpressions;
using System.Collections.Generic;
using System.Collections;
using System.IO;
using System.Reflection;

namespace msbuild_tests
{
    [TestClass]
    public class RegexpCapturingGroupTest
    {
        //[TestMethod]
        //public void Test_Split_Multi_Patterns_In_One_Line()
        //{
        //    string patterns = "a/(.+)/(.+).dll => a/$1/$2.dll,(.+)/(.+)/(.+).dll => a-$1-$2-$3.dll";
        //    List<BuildArtifactsMapping> artifactsMapping = BuildArtifactsMappingResolver.splitPatterns(patterns);

        //    List<BuildArtifactsMapping> artifactsMappingExpected = new List<BuildArtifactsMapping>();
        //    artifactsMappingExpected.Add(new BuildArtifactsMapping("a/(.+)/(.+).dll", "a/$1/$2.dll"));
        //    artifactsMappingExpected.Add(new BuildArtifactsMapping("(.+)/(.+)/(.+).dll", "a-$1-$2-$3.dll"));           

        //    Assert.AreEqual(artifactsMapping, artifactsMappingExpected);
        //}

        //[TestMethod]
        //public void TestSplitTargetAndDes()
        //{
        //    string input = "a/(.+)/(.+).dll => a/$1/$2.dll";
        //    BuildArtifactsMapping mapping = BuildArtifactsMappingResolver.getArtifactsMapping(input);
                       
        //    BuildArtifactsMapping mappingExpect = new BuildArtifactsMapping();
        //    mappingExpect.input = "a/(.+)/(.+).dll";
        //    mappingExpect.output = "a/$1/$2.dll";

        //    Assert.AreEqual(mappingExpect, mapping);
        //}

        //[TestMethod]
        //[ExpectedException(typeof(ArgumentException))]
        //public void TestValidateCaptureGroups()
        //{
        //    string input = "a/(.+)/(.+).dll => a-$1-$2-$3.dll";
        //    BuildArtifactsMapping mapping = BuildArtifactsMappingResolver.getArtifactsMapping(input);
        //    // assert is handled by the ExcpectedException
        //}

        [TestMethod]
        public void TestExtractRegex()
        {
            BuildArtifactsMapping mapping = new BuildArtifactsMapping();

            mapping.input = "obj\\\\Debug\\\\(.+)\\\\(.+).dll";
            string regex = BuildArtifactsMapping.getRegexPattern(mapping);
            string regexExpect = "(.+)\\\\(.+).dll";
            Assert.AreEqual(regex, regexExpect);
        }

        [TestMethod]
        public void TestExtractRootDirectory()
        {
            BuildArtifactsMapping mapping = new BuildArtifactsMapping();
            
            mapping.input = "obj\\\\Debug\\\\(.+)\\\\(.+).dll";
            string path = BuildArtifactsMapping.getRootDirectory(mapping);
            string pathExpect = "obj\\\\Debug\\\\";
            Assert.AreEqual(path, pathExpect);
        }

        [TestMethod]
        public void TestPlaceHoldersList()
        {           
            BuildArtifactsMapping mapping = new BuildArtifactsMapping();
            List<string> groupList = null;

            mapping.output = "a-$1-$2-$3.dll";
            groupList = BuildArtifactsMapping.getPlaceHoldersList(mapping, 3);
            CollectionAssert.AreEqual(groupList, new List<string>() { "$1", "$2", "$3" });

            AssertExtension.Throws<ArgumentException>(() => BuildArtifactsMapping.getPlaceHoldersList(mapping, 2));

            mapping.output = "a-$1-$Z-$3.dll";
            AssertExtension.Throws<ArgumentException>(() => BuildArtifactsMapping.getPlaceHoldersList(mapping, 3));
        }

        [TestMethod]
        public void TestMatchMapping()
        {
            BuildArtifactsMapping mapping = new BuildArtifactsMapping();
            mapping.input = "regex_test\\(.+)\\(.+).dll";
            mapping.output = "msbuild-test\\$1\\$2.dll";
            
            Dictionary<string, string> resultMap = new Dictionary<string, string>();
            string projectPath = "C:\\Work\\nuget-project\\msbuild-artifactory-plugin\\msbuild_tests";

            BuildArtifactsMappingResolver.matchMappingArtifacts(mapping, projectPath, resultMap);

            Dictionary<string, string> resultMapExpected = new Dictionary<string, string>();
            resultMapExpected.Add("C:\\Work\\nuget-project\\msbuild-artifactory-plugin\\msbuild_tests\\regex_test\\lib\\JFrog.Artifactory.dll",
                                        "msbuild-test\\lib\\JFrog.Artifactory.dll");
            resultMapExpected.Add("C:\\Work\\nuget-project\\msbuild-artifactory-plugin\\msbuild_tests\\regex_test\\obj\\Debug\\JFrog.Artifactory.dll",
                                        "msbuild-test\\obj\\Debug\\JFrog.Artifactory.dll");
            resultMapExpected.Add("C:\\Work\\nuget-project\\msbuild-artifactory-plugin\\msbuild_tests\\regex_test\\obj\\Release\\JFrog.Artifactory.dll",
                                        "msbuild-test\\obj\\Release\\JFrog.Artifactory.dll");

            CollectionAssert.AreEqual(resultMap, resultMapExpected);


            mapping.input = "regex_test\\(.+)\\(.+).dll";
            mapping.output = "msbuild-test\\$2\\$1.dll";

            resultMap.Clear();
            resultMapExpected.Clear();
            BuildArtifactsMappingResolver.matchMappingArtifacts(mapping, projectPath, resultMap);        
            resultMapExpected.Add("C:\\Work\\nuget-project\\msbuild-artifactory-plugin\\msbuild_tests\\regex_test\\lib\\JFrog.Artifactory.dll",
                                        "msbuild-test\\JFrog.Artifactory\\lib.dll");
            resultMapExpected.Add("C:\\Work\\nuget-project\\msbuild-artifactory-plugin\\msbuild_tests\\regex_test\\obj\\Debug\\JFrog.Artifactory.dll",
                                        "msbuild-test\\JFrog.Artifactory\\obj\\Debug.dll");
            resultMapExpected.Add("C:\\Work\\nuget-project\\msbuild-artifactory-plugin\\msbuild_tests\\regex_test\\obj\\Release\\JFrog.Artifactory.dll",
                                        "msbuild-test\\JFrog.Artifactory\\obj\\Release.dll");

            
            //Relative directory
            //resultMapExpected.Add("regex_test\\obj\\Release\\JFrog.Artifactory.dll",
            //                            "msbuild-test\\regex_test\\obj\\Release\\JFrog.Artifactory.dll");

            //none exists folder need to check
            CollectionAssert.AreEqual(resultMap, resultMapExpected);
        }

        private static string getTestFilesFolder()
        {
            string currentPath = System.Environment.CurrentDirectory;
            return currentPath.Substring(0, currentPath.LastIndexOf("bin")) + "regex_test";
        }
    }
}
