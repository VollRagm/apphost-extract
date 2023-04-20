using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace apphost_extract
{
    class Program
    {
        static void Main(string[] args)
        {
            Log.Info("apphost-extract by VollRagm\n", ConsoleColor.Yellow);
            var path = GetPath(args);

            var path2 = "test.exe";
            //var path2 = "AmongUsUnlocker.exe";

            var file = AppHostFile.Open(path2);//path.FullName);
            Log.Info($"{file.Header.Manifest.FileEntries.Count} embedded file(s) found.");

            var directory = Path.Combine(path.DirectoryName, path.Name.Remove(path.Name.Length - path.Extension.Length) +"_extracted");
            Console.WriteLine();
            Log.Info("Extracting...");

            file.ExtractAll(directory);

            Console.WriteLine();
            Log.Info("Done.");
            file.Close();
            Console.ReadLine();
        }

        static FileInfo GetPath(string[] args)
        {
            try
            {
                var fileName = new FileInfo(Assembly.GetExecutingAssembly().Location).Name;

                if (args.Length > 0)
                {
                    if (File.Exists(args[0]))
                    {
                        return new FileInfo(args[0]);
                    }
                    else
                    {
                        Log.Fatal($"{args[0]} could not be found. Usage: {fileName} <path>");
                    }
                }
                else
                {
                    Log.Fatal($"No File provided. Usage: {fileName} <path>");
                }
            }catch(Exception ex)
            {
                Log.Fatal($"Could not get file: {ex.Message}");
            }
            return null;
        }
    }
}
