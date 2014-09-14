using Microsoft.Build.Framework;
using Microsoft.Build.Utilities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace JFrog.Artifactory.Utils
{
    public class BuildInfoLog
    {
        private const string PRE_FIX = "[ARTIFACTORY] ";
        private TaskLoggingHelper _log { set; get; }

        public BuildInfoLog(TaskLoggingHelper taskLogger)
        {
            _log = taskLogger;
        }

        /*
         *  Verbosity: All 
         */
        public void Error(string message)
        {
            _log.LogError(PRE_FIX + message);
        }

        /*
         *  Verbosity: All
         */
        public void Error(string message, Exception ex)
        {
            Error(message);
            _log.LogErrorFromException(ex, true);
        }

        /*
         *  Verbosity: Detailed=>Diagnostic 
         */
        public void Debug(string message) 
        {
            _log.LogMessage(MessageImportance.Low, PRE_FIX + message);
        }

        
        /*
         *  Verbosity: Minimal=>Normal=>Detailed=>Diagnostic 
         */
        public void Info(string message)
        {
            _log.LogMessage(MessageImportance.High, PRE_FIX + message);
        }
    }
}
