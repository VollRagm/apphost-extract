using apphost_extract_v2.General;
using System;
using System.Diagnostics;
using System.IO;
using System.Reflection.PortableExecutable;
using System.Text;
using static apphost_extract_v2.Util;

namespace apphost_extract_v2.Models
{
    public class ApphostFile6 : IApphostFile
    {
        private readonly byte[] HEADER_OFFSET_SIG = { 0xE8, 0x0, 0x0, 0x0, 0x0, 0x48, 0x8B, 0x05, 0x0, 0x0, 0x0, 0x0, 0x48, 0x85, 0xC0 };
        private const string HEADER_OFFSET_MASK = "x????xxx????xxx";

        private const int HEADER_SIZE = 0xD;
        private const int FILE_ENTRY_SIZE = 0x12;

        public ApphostFile6(FileStream fs, PEHeaders peheader) : base(fs, peheader)
        {
            Header = new AppHostFileHeader();
            var headerAddress = FindHeaderOffset();

            if(headerAddress == 0)
                Log.Fatal("Unable to located bundle header :/");

            var headerBuffer = fs.ReadBuffer(headerAddress, HEADER_SIZE);

            Header.Raw = headerBuffer;
            Header.Path = Encoding.UTF8.GetString(
                                fs.ReadBuffer(headerAddress + HEADER_SIZE, 0xC));

            Header.Manifest = ParseManifest();

        }

        public ApphostFile6(FileStream fs, PEHeaders peheader, uint headerOffset) : base(fs, peheader)
        {
            Header = new AppHostFileHeader();
            var headerAddress = headerOffset;

            if (headerAddress == 0)
                Log.Fatal("Unable to located bundle header :/");

            var headerBuffer = fs.ReadBuffer(headerAddress, HEADER_SIZE);

            Header.Raw = headerBuffer;
            Header.Path = Encoding.UTF8.GetString(
                                fs.ReadBuffer(headerAddress + HEADER_SIZE, 0xC));

            Header.Manifest = ParseManifest();

        }

        private uint FindHeaderOffset()
        {
            var textSegment = PEHeader.GetSegment(".text");
            Log.Info("Scanning for the .NET 5 Bundle header...");
            var sw = Stopwatch.StartNew();

            var sigscanResults = PatternScan(FileStream,
                                    textSegment.PointerToRawData, textSegment.SizeOfRawData,
                                        HEADER_OFFSET_SIG, HEADER_OFFSET_MASK);

            sw.Stop();

            if (sigscanResults.Length == 0) return 0;

            var headerOffset = (int)BitConverter.ToUInt32(
                                    FileStream.ReadBuffer(sigscanResults[0] + 8, 4));

            var headerPtr = PEHeader.AddVirtualOffset(sigscanResults[0] + 12, headerOffset);
            var headerAddress = BitConverter.ToUInt32(FileStream.ReadBuffer(headerPtr, 4));

            Log.Info($"Found bundle header offset at 0x{headerPtr:X8} in {sw.ElapsedMilliseconds}ms -> {headerAddress:X8}");
            return headerAddress;
        }

        private AppHostManifest ParseManifest()
        {
            //Seek over random bullshit that got added in .NET 5
            FileStream.Seek(0x28, SeekOrigin.Current);

            AppHostManifest manifest = new AppHostManifest();
            var embeddedFileCount = BitConverter.ToInt32(Header.Raw, 0x8);
            Log.Info($"Found {embeddedFileCount} embedded files.");
            for (int i = 0; i < embeddedFileCount; i++)
                manifest.FileEntries.Add(GetNextEntry());

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
