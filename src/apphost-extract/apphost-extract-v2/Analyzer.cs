using apphost_extract_v2.General;
using apphost_extract_v2.Models;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Reflection.PortableExecutable;
using System.Text;
using static apphost_extract_v2.Util;

namespace apphost_extract_v2
{
    public class Analyzer
    {
        private FileStream File;
        public PEHeaders PEHeader;

        private readonly byte[] VERSION_SIGNATURE = new byte[] { 0x4C, 0x8D, 0x05, 0x0, 0x0, 0x0, 0x0, 0x48, 0x8D, 0x15, 0x0, 0x0, 0x0, 0x0, 0x48, 0x8D, 0x0D, 0x0, 0x0, 0x0, 0x0 };
        private const string VERSION_SIGNATURE_MASK = "xxx???xxxx???xxxx???x";

        public Analyzer(FileInfo fi)
        {
            File = new FileStream(fi.FullName, FileMode.Open, FileAccess.Read);
            PEHeader = new PEHeaders(File);
        }

        public IApphostFile Open()
        {
              var textSegment = PEHeader.GetSegment(".text");

              var sw = Stopwatch.StartNew();
              Log.Info("Scanning for version string pointer...");

              var sigscanResults = PatternScan(File, 
                                  textSegment.PointerToRawData, textSegment.SizeOfRawData,
                                      VERSION_SIGNATURE, VERSION_SIGNATURE_MASK);
              sw.Stop();

              if (sigscanResults.Length == 0)
                  return null;

              var versionOffset = (int)BitConverter.ToUInt32(File.ReadBuffer(sigscanResults[0] + 3, 4));
              var versionStringPtr = PEHeader.AddVirtualOffset(sigscanResults[0] + 7, versionOffset);
              var versionString = Encoding.Unicode.GetString(
                                                  File.ReadBuffer(
                                                      versionStringPtr, 6));

              Log.Info($"Found version string at 0x{versionStringPtr:X8} in {sw.ElapsedMilliseconds}ms -> {versionString}");
              Console.WriteLine();


            switch (versionString)
            {
                case "3.0":
                    return new ApphostFile30(File, PEHeader);
                case "3.1":
                    return new ApphostFile31(File, PEHeader);
                case "5.0":
                    return new ApphostFile5(File, PEHeader);
                case "6.0":
                    return new ApphostFile6(File, PEHeader);
                case "7.0":
                    return new ApphostFile7(File, PEHeader);
                case "8.0":
                    return new ApphostFile80(File, PEHeader);
                default:
                    return null;
            }
        }

        public IApphostFile Open(string versionString, uint headerOffset)
        {
            switch (versionString)
            {
                case "3.0":
                    return new ApphostFile30(File, PEHeader, headerOffset);
                case "3.1":
                    return new ApphostFile31(File, PEHeader, headerOffset);
                case "5.0":
                    return new ApphostFile5(File, PEHeader, headerOffset);
                case "6.0":
                    return new ApphostFile6(File, PEHeader, headerOffset);
                case "7.0":
                    return new ApphostFile7(File, PEHeader, headerOffset);
                case "8.0":
                    return new ApphostFile80(File, PEHeader, headerOffset);
                default:
                    return null;
            }
        }
    }
}
