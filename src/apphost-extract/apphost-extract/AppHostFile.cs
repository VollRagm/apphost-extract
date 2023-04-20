using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection.PortableExecutable;
using System.Text;
using System.Threading.Tasks;

namespace apphost_extract
{
    public class AppHostFile
    {
        private FileStream FileStream;

        public AppHostFileHeader Header { get; set; }
        private ApphostVersion Version { get; set; }

        private const int VERSION_OFFSET_NET3 = 0x1A3E8;

        private const int HEADER_OFFSET_PTR_NET5 = 0x8E508;
        private const int HEADER_OFFSET_PTR_NET3 = 0x27600;

       

        public AppHostFile(FileStream fileStream) 
        {
            FileStream = fileStream;

            //RDATA = GetRDATASection(fileStream);
            var ver = GetVersion(VERSION_OFFSET_NET3);
            var headerVA = GetHeaderAddress(HEADER_OFFSET_PTR_NET3);
            
            Header = new AppHostFileHeader(FileStream, headerVA);
        }

        private int GetRDATASection(FileStream fileStream)
        {
            var pefile = new PEReader(fileStream);
            var sectionHeaders = pefile.PEHeaders.SectionHeaders;
            return sectionHeaders.Where(header => header.Name == ".rdata").FirstOrDefault().VirtualAddress + 0x668;
        }

        public int GetHeaderAddress(int offset)
        {
            var buffer = new byte[16];
            FileStream.Seek(offset, SeekOrigin.Begin);
            FileStream.Read(buffer, 0, buffer.Length);
            return BitConverter.ToInt32(buffer, 0);
        }

        public ApphostVersion GetVersion(int offset)
        {
            FileStream.Seek(offset, SeekOrigin.Begin);
            var buffer = new byte[10];
            FileStream.Read(buffer, 0, buffer.Length);
            var versionStr = Encoding.Unicode.GetString(buffer);

            if (versionStr.StartsWith("3."))
            {
                Log.Info("Detected .NET Core 3.");
                return ApphostVersion.NET3;
            }
            else
            {
                Log.Info("Could not detect .NET Core version, assumming .NET Core 5.");
                return ApphostVersion.NET5;
            }
        }


        public static AppHostFile Open(string path)
        {
            try
            {
                FileStream fs = new FileStream(path, FileMode.Open, FileAccess.ReadWrite);
                AppHostFile file = new AppHostFile(fs);
                Log.Info("File opened successfully!");
                return file;
            }
            catch(Exception ex)
            {
                Log.Fatal($"Exception when trying to open file: {ex.Message}");
                return null;
            }
        }

        public void ExtractAll(string outputDir)
        {
            Directory.CreateDirectory(outputDir);
            foreach (var fileEntry in Header.Manifest.FileEntries)
            {
                try
                {
                    var bytes = fileEntry.Read();
                    var name = fileEntry.Name;
                    var filePath = Path.Combine(outputDir, name);
                    File.WriteAllBytes(filePath, bytes);

                    Log.Critical($"Extracted {name}");

                }
                catch (Exception ex)
                {
                    Log.Error($"Could not extract {fileEntry.Name}: {ex.Message}");
                }

            }
        }

        public void Close()
        {
            FileStream.Close();
        }
    }

    public enum ApphostVersion
    {
        NET5,
        NET3
    }
}
