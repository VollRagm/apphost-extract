using apphost_extract_v2.General;
using System;
using System.Diagnostics;
using System.IO;
using System.Reflection.PortableExecutable;
using System.Text;
using static apphost_extract_v2.Util;

namespace apphost_extract_v2.Models
{
    public class ApphostFile80 : IApphostFile
    {
        private readonly byte[] HEADER_OFFSET_SIG = { 0x4C, 0x8B, 0x3D, 0x00, 0x00, 0x00, 0x00, 0x48, 0x8D, 0x55 };
        private const string HEADER_OFFSET_MASK = "xxx????xxx";

        private const int HEADER_SIZE = 0xD;
        private const int FILE_ENTRY_SIZE = 0x1A;

        public ApphostFile80(FileStream fs, PEHeaders peheader) : base(fs, peheader)
        {
            Header = new AppHostFileHeader();
            var headerAddress = FindHeaderOffset();

            if(headerAddress == 0)
                Log.Fatal("Unable to located bundle header :/");

            var headerBuffer = fs.ReadBuffer(headerAddress, HEADER_SIZE);

            Header.Raw = headerBuffer;
            Header.Path = Encoding.UTF8.GetString(
                                               fs.ReadBuffer(headerAddress + HEADER_SIZE, 0x20));

            Header.Manifest = ParseManifest();

        }

        public ApphostFile80(FileStream fs, PEHeaders peheader, uint headerOffset) : base(fs, peheader)
        {
            Header = new AppHostFileHeader();
            var headerAddress = headerOffset;

            if (headerAddress == 0)
                Log.Fatal("Unable to located bundle header :/");

            var headerBuffer = fs.ReadBuffer(headerAddress, HEADER_SIZE);

            Header.Raw = headerBuffer;
            Header.Path = Encoding.UTF8.GetString(
                                               fs.ReadBuffer(headerAddress + HEADER_SIZE, 0x20));

            Header.Manifest = ParseManifest();

        }

        private AppHostManifest ParseManifest()
        {
            //Seek over random bullshit that got added in .NET 5.0
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

            byte[] stringBuffer = new byte[entry.Raw[0x19]];
            FileStream.Read(stringBuffer, 0, stringBuffer.Length);
            entry.Name = Encoding.UTF8.GetString(stringBuffer);

            return entry;
        }

        private uint FindHeaderOffset()
        {
            var textSegment = PEHeader.GetSegment(".text");
            Log.Info("Scanning for the .NET 8.0 Bundle header...");
            var sw = Stopwatch.StartNew();

            var sigscanResults = PatternScan(FileStream,
                                    textSegment.PointerToRawData, textSegment.SizeOfRawData,
                                        HEADER_OFFSET_SIG, HEADER_OFFSET_MASK);

            sw.Stop();

            if (sigscanResults.Length == 0) return 0;

            var headerOffset = (int)BitConverter.ToUInt32(
                        FileStream.ReadBuffer(sigscanResults[0] + 3, 4));

            var headerPtr = PEHeader.AddVirtualOffset(sigscanResults[0] + 7, headerOffset);
            var headerAddress = BitConverter.ToUInt32(FileStream.ReadBuffer(headerPtr, 4));

            Log.Info($"Found bundle header offset at 0x{headerPtr:X8} in {sw.ElapsedMilliseconds}ms -> {headerAddress:X8}");
            return headerAddress;
        }
        public override void Close()
        {
            FileStream.Close();
        }
    }
}
