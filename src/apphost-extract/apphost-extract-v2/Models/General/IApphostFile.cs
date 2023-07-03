using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection.PortableExecutable;
using System.Text;
using System.Threading.Tasks;

namespace apphost_extract_v2.General
{
    public abstract class IApphostFile
    {
        internal FileStream FileStream;
        public PEHeaders PEHeader;
        public AppHostFileHeader Header;

        public IApphostFile(FileStream fs, PEHeaders peheader)
        {
            FileStream = fs;
            PEHeader = peheader;
        }

        public void ExtractAll(string outputDir)
        {
            Directory.CreateDirectory(outputDir);
          
            //foreach(var fileEntry in Header.Manifest.FileEntries)
            Parallel.ForEach(Header.Manifest.FileEntries, fileEntry =>
            {
                try
                {
                    var bytes = FileStream.ReadBuffer(fileEntry.Offset, fileEntry.Size);
                    var name = Path.GetFileName(fileEntry.Name);
                    var path = Path.GetDirectoryName(fileEntry.Name);
                    if (path.Length > 0)
                    {
                        Directory.CreateDirectory(Path.Combine(outputDir, path));
                    }

                    if (FileChecker.IsKnownFile(bytes))
                    {
                        Log.Info($"Extracting {name} --> Known file", ConsoleColor.Green);
                    }
                    else
                    {
                        Log.Info($"Extracting {name} --> Unknown file", ConsoleColor.Yellow);
                        name = name.Insert(0, "_");
                    }

                    var filePath = Path.Combine(outputDir, path, name);
                    File.WriteAllBytes(filePath, bytes);
                }
                catch (Exception ex)
                {
                    Log.Error($"Could not extract {fileEntry.Name}: {ex.Message}");
                }
            });
            Console.WriteLine();
        }

        public abstract void Close();
    }
}
