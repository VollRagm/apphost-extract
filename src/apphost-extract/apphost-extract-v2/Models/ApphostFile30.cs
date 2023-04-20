using apphost_extract_v2.General;
using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection.PortableExecutable;
using System.Text;

namespace apphost_extract_v2.Models
{
    public class ApphostFile30 : IApphostFile
    {
        private const int HEADER_OFFSET_PTR = 0x23E00;
        private const int HEADER_SIZE = 0xD;
        private const int FILE_ENTRY_SIZE = 0x12;

        public ApphostFile30(FileStream fs, PEHeaders peheader) : base(fs, peheader)
        {
            Header = new AppHostFileHeader();
            Log.Info($"Reading header at 0x{HEADER_OFFSET_PTR:X8}...");
            var headerAddress = BitConverter.ToInt32(fs.ReadBuffer(HEADER_OFFSET_PTR, 4));
            
            if(headerAddress == 0)
                Log.Fatal("The address of the Bundle header is 0 :/");

            var headerBuffer = fs.ReadBuffer(headerAddress, HEADER_SIZE);

            Header.Raw = headerBuffer;
            Header.Path = Encoding.UTF8.GetString(fs.ReadBuffer(headerAddress + HEADER_SIZE, 0xC));

            Header.Manifest = ParseManifest();
        }

        public ApphostFile30(FileStream fs, PEHeaders peheader, uint headerOffset) : base(fs, peheader)
        {
            Header = new AppHostFileHeader();
            Log.Info($"Reading header at 0x{HEADER_OFFSET_PTR:X8}...");
            var headerAddress = headerOffset;

            if (headerAddress == 0)
                Log.Fatal("The address of the Bundle header is 0 :/");

            var headerBuffer = fs.ReadBuffer(headerAddress, HEADER_SIZE);

            Header.Raw = headerBuffer;
            Header.Path = Encoding.UTF8.GetString(fs.ReadBuffer(headerAddress + HEADER_SIZE, 0xC));

            Header.Manifest = ParseManifest();
        }

        private AppHostManifest ParseManifest()
        {
            AppHostManifest manifest = new AppHostManifest();
            var embeddedFileCount = BitConverter.ToInt32(Header.Raw, 0x8);
            Log.Info($"Found {embeddedFileCount} embedded files.");

            for (int i = 0; i < embeddedFileCount; i++)
            {
                manifest.FileEntries.Add(GetNextEntry());
            }

            return manifest;
        }

        private AppHostFileEntry GetNextEntry()
        {
            AppHostFileEntry entry = new AppHostFileEntry();
            byte[] entryBuffer = new byte[FILE_ENTRY_SIZE];
            FileStream.Read(entryBuffer, 0, entryBuffer.Length);
            entry.Raw = entryBuffer;

            entry.Offset = BitConverter.ToInt64(entry.Raw, 0);

            //hopefully nobody embeds a file larger than 2GB :D
            entry.Size = (int)BitConverter.ToInt64(entry.Raw, 0x8);

            byte[] stringBuffer = new byte[entry.Raw[0x11]];
            FileStream.Read(stringBuffer, 0, stringBuffer.Length);
            entry.Name = Encoding.UTF8.GetString(stringBuffer);

            return entry;
        }



        public override void Close()
        {
            FileStream.Close();
        }


    }
}
