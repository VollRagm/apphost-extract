using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Security.Cryptography;
using System.Security.Cryptography.X509Certificates;
using System.Text;

namespace apphost_extract_v2
{
    public static class FileChecker
    {
        private const string HASHFILE = "apphost-hashes.txt"; 
        private static string[] Hashes;
        private static SHA256Managed sha = new SHA256Managed();

        public static void Load()
        {
            var path = Path.Combine(Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location), HASHFILE);
            if (File.Exists(path))
                Hashes = File.ReadAllLines(path);
            else
            {
                Log.Error("apphost-hashes.txt wasn't found, only running cert check.");
                Console.WriteLine();
                Hashes = new string[0];
            }
        }

        public static bool IsKnownFile(byte[] buffer)
        {
            string hash = "";
            lock (sha)
            {
                hash = BitConverter.ToString(sha.ComputeHash(buffer)).Replace("-", "");
            }
            return Hashes.Contains(hash) || SignedByMS(buffer);
        }

        public static bool SignedByMS(byte[] buffer)
        {
            try
            {
                X509Certificate cert = new X509Certificate(buffer);
                return cert.GetCertHashString() == "2485A7AFA98E178CB8F30C9838346B514AEA4769";
            }catch { return false; }
        }











    }
}
