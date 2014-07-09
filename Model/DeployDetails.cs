using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace JFrog.Artifactory.Model
{
    class DeployDetails
    {
        public string targetRepository { set; get; }

        public string artifactPath { set; get; }

        public FileInfo file { set; get; }

        public string sha1 { set; get; }

        public string md5 { set; get; }
    }
}
