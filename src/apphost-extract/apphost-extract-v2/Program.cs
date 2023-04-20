using System;
using System.Diagnostics;
using System.IO;
using System.IO.MemoryMappedFiles;
using System.Reflection;

namespace apphost_extract_v2
{
    class Program
    {
        static void Main(string[] args)
        {
            Log.Info("apphost-extract-v2 by VollRagm\n", ConsoleColor.Yellow);

            FileChecker.Load();

            var fileInfo = GetFileInfo(args);

            var apphostAnalyzer = new Analyzer(fileInfo);
            var apphost = apphostAnalyzer.Open();

            if (apphost == null)
            {
                Log.Error("Unable to determine apphost version automatically.");
                var version = Log.QueryString("Please enter the apphost version, you can find it in the entry point of the app (3.0, 3.1, 5, 6.0): ");
                var headerOffset = uint.Parse(Log.QueryString("Please enter the Header offset \n(parse pdb, search for header_offset in names or\n use string reference 'Bundle Header Offset: [%lx]', find the .data ptr and enter its value): ").Replace("0x", ""), System.Globalization.NumberStyles.HexNumber);
                
                apphostAnalyzer = new Analyzer(fileInfo);
                apphost = apphostAnalyzer.Open(version, headerOffset);
            }
                

            Log.Info("File parsed successfully, extracting contents...");
            Console.WriteLine();

            var directory = Path.Combine(fileInfo.DirectoryName, fileInfo.Name.Remove(fileInfo.Name.Length - fileInfo.Extension.Length) + "_extracted");
            apphost.ExtractAll(directory);

            Log.Info("Extraction completed successfully and unknown files have been prefixed with _ .");
            Console.ReadLine();
        }

        static FileInfo GetFileInfo(string[] args)
        {
            try
            {
                var fileName = new FileInfo(Assembly.GetExecutingAssembly().Location).Name;

                if (args.Length > 0)
                {
                    if (File.Exists(args[0]))
                    {
                        var fullPath = Path.GetFullPath(args[0]);
                        return new FileInfo(fullPath);
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
            }
            catch (Exception ex)
            {
                Log.Fatal($"Could not get file: {ex.Message}");
            }
            return null;
        }
    }
}
