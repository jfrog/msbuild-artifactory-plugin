using System;
using JFrog.Artifactory.Utils.regexCapturing;
using System.Text.RegularExpressions;
using System.Collections.Generic;
using System.Collections;
using System.IO;
using System.Reflection;
using NUnit.Framework;

namespace msbuild_tests
{
    [TestFixture]
    public class RegexpCapturingGroupTest
    {
        [Test]
        public void TestExtractRegex()
        {
            BuildArtifactsMapping mapping = new BuildArtifactsMapping();

            mapping.input = @"obj\Debug\(.+)\(.+).dll";
            string regex = BuildArtifactsMapping.getRegexPattern(mapping);
            string regexExpect = @"(.+)\(.+).dll";
            Assert.AreEqual(regex, regexExpect);
        }

        [Test]
        public void TestExtractRootDirectory()
        {
            BuildArtifactsMapping mapping = new BuildArtifactsMapping();
            
            mapping.input = @"obj\Debug\(.+)\(.+).dll";
            string path = BuildArtifactsMapping.getRootDirectory(mapping);
            string pathExpect = @"obj\Debug\";
            Assert.AreEqual(path, pathExpect);
        }

        [Test]
        public void TestPlaceHoldersList()
        {
            BuildArtifactsMapping mapping = new BuildArtifactsMapping();
            ISet<Int32> groupList = null;

            mapping.output = "a-$1-$2-$3.dll";
            groupList = BuildArtifactsMapping.getPlaceHoldersList(mapping, 3);
            CollectionAssert.AreEqual(groupList, new HashSet<Int32>() { 1, 2, 3 });

            AssertExtension.Throws<ArgumentException>(() => BuildArtifactsMapping.getPlaceHoldersList(mapping, 2));

            mapping.output = "a-$1-$Z-$3.dll";
            AssertExtension.Throws<ArgumentException>(() => BuildArtifactsMapping.getPlaceHoldersList(mapping, 3));
        }

        [Test]
        public void TestLessPlaceholdersInOutput()
        {
            string projectPath = Path.GetDirectoryName(Path.GetDirectoryName(System.IO.Directory.GetCurrentDirectory()));
            Dictionary<string, string> resultMap = new Dictionary<string, string>();
            BuildArtifactsMapping mapping = new BuildArtifactsMapping();

            mapping.input = @"regex_test\(.+)\(.+).dll";
            mapping.output = @"a/$2.dll";

            BuildArtifactsMappingResolver.matchMappingArtifacts(mapping, projectPath, resultMap);
            Dictionary<string, string> resultMapExpected = new Dictionary<string, string>();

            CollectionAssert.Contains(resultMap, new KeyValuePair<string, string>(projectPath + @"\regex_test\obj\Debug\JFrog.Artifactory.dll",
                                                                                                @"a/JFrog.Artifactory.dll"));
        }

        [Test]
        public void TestComplexRegex()
        {
            string projectPath = Path.GetDirectoryName(Path.GetDirectoryName(System.IO.Directory.GetCurrentDirectory()));
            Dictionary<string, string> resultMap = new Dictionary<string, string>();
            BuildArtifactsMapping mapping = new BuildArtifactsMapping();

            //Enforce only file with digits
            mapping.input = @"regex_test\(.+)\(.+[\d].+)";
            mapping.output = @"a/$2.dll";

            BuildArtifactsMappingResolver.matchMappingArtifacts(mapping, projectPath, resultMap);
            Dictionary<string, string> resultMapExpected = new Dictionary<string, string>();

            //CollectionAssert.Contains(resultMap, new KeyValuePair<string, string>(
            //                                projectPath + @"\regex_test\lib\TemporaryGeneratedFile_036C0B5B-1481-4323-8D20-8F5ADCB23D92.cs",
            //                                              @"a/TemporaryGeneratedFile_036C0B5B-1481-4323-8D20-8F5ADCB23D92.cs"
            //                         ));
        }

        [Test]
        public void TestMatchMapping()
        {
            BuildArtifactsMapping mapping = new BuildArtifactsMapping();
            mapping.input = @"regex_test\(.+)\(.+).dll";
            mapping.output = @"msbuild-test\$1\$2.dll";
            
            Dictionary<string, string> resultMap = new Dictionary<string, string>();
            string projectPath = Path.GetDirectoryName(Path.GetDirectoryName(System.IO.Directory.GetCurrentDirectory()));

            BuildArtifactsMappingResolver.matchMappingArtifacts(mapping, projectPath, resultMap);

            Dictionary<string, string> resultMapExpected = new Dictionary<string, string>();
            resultMapExpected.Add(projectPath + @"\regex_test\lib\JFrog.Artifactory.dll",
                                        @"msbuild-test\lib\JFrog.Artifactory.dll");
            resultMapExpected.Add(projectPath + @"\regex_test\obj\Debug\JFrog.Artifactory.dll",
                                        @"msbuild-test\obj\Debug\JFrog.Artifactory.dll");
            resultMapExpected.Add(projectPath + @"\regex_test\obj\Release\JFrog.Artifactory.dll",
                                        @"msbuild-test\obj\Release\JFrog.Artifactory.dll");

            CollectionAssert.AreEqual(resultMap, resultMapExpected);


            mapping.input = @"regex_test\(.+)\(.+).dll";
            mapping.output = @"msbuild-test\$2\$1.dll";

            resultMap.Clear();
            resultMapExpected.Clear();
            BuildArtifactsMappingResolver.matchMappingArtifacts(mapping, projectPath, resultMap);        
            resultMapExpected.Add(projectPath + @"\regex_test\lib\JFrog.Artifactory.dll",
                                        @"msbuild-test\JFrog.Artifactory\lib.dll");
            resultMapExpected.Add(projectPath + @"\regex_test\obj\Debug\JFrog.Artifactory.dll",
                                        @"msbuild-test\JFrog.Artifactory\obj\Debug.dll");
            resultMapExpected.Add(projectPath + @"\regex_test\obj\Release\JFrog.Artifactory.dll",
                                        @"msbuild-test\JFrog.Artifactory\obj\Release.dll");

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
