using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;

namespace JFrog.Artifactory.Utils
{
    /// <summary>
    /// generate a SHA1 to a given file
    /// </summary>
    public class Sha1Reference
    {

        /// <summary>
        /// generate the SHA1 for a file 
        /// </summary>
        /// <param name="path">the path for the file and its name</param>
        /// <returns>SHA1 as string</returns>
        public static string GenerateSHA1(string path)
        {
            if (path == null) return string.Empty;
                using (var fs = new FileStream(path, FileMode.Open, FileAccess.Read, FileShare.Read))
                //using (FileStream fs = File.OpenRead(path))
                {
                    //byte[] buf = new byte[fs.Length];
                    // int byteread = fs.Read(buf, 0, buf.Length);SHA256CryptoServiceProvider
                    fs.Position = 0;
                    using (var sha1 = new SHA1Managed())
                    {
                        return BitConverter.ToString(sha1.ComputeHash(fs)).Replace("-", String.Empty).ToLower();
                    }
                }
        }

        /// <summary>
        /// generate the SHA1 for a file 
        /// </summary>
        /// <param name="bytes">the bytes for the file</param>
        /// <returns>SHA1 as string</returns>
        public static string GenerateSHA1(byte[] bytes)
        {
            if (bytes == null) return string.Empty;
                using (var sha1 = new SHA1Managed())
                {
                    return BitConverter.ToString(sha1.ComputeHash(bytes)).Replace("-", "").ToLower();
                }
        }
    }
}
