using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection.PortableExecutable;
using System.Text;

namespace apphost_extract_v2
{
    public static class Util
    {
        public static int[] PatternScan(FileStream fs, int start, int length, byte[] pattern, string mask)
        {
            byte[] scanBuffer = fs.ReadBuffer(start, length);

            List<int> scanResults = new List<int>();

            for(int i = 0; i < scanBuffer.Length - pattern.Length; i++)
            {
                if (!IsMatch(scanBuffer, i, pattern, mask))
                    continue;

                scanResults.Add(start + i);
            }

            return scanResults.ToArray();
        }


        //https://stackoverflow.com/a/283648/10724593
        private static bool IsMatch(byte[] array, int position, byte[] candidate, string mask)
        {
            if (candidate.Length > (array.Length - position))
                return false;

            for (int i = 0; i < candidate.Length; i++)
                if (mask[i] == 'x' && array[position + i] != candidate[i])
                    return false;

            return true;
        }

        public static byte[] ReadBuffer(this FileStream fs, long start, int length)
        {
            byte[] buff = new byte[length];
            lock (fs)
            {
                fs.Seek(start, SeekOrigin.Begin);
                fs.Read(buff, 0, length);
            }
            return buff;
        }

        public static int AddVirtualOffset(this PEHeaders header, int fileAddress, int offset)
        {
            return header.VirtualAddressToFileOffset(header.FileOffsetToVirtualAddress(fileAddress) + offset);
        }

        public static int FileOffsetToVirtualAddress(this PEHeaders header, int offset)
        {
            var section = header.FindSection(offset, true);
            return offset + (section.VirtualAddress - section.PointerToRawData);
        }

        public static int VirtualAddressToFileOffset(this PEHeaders header, int address)
        {
            var section = header.FindSection(address, false);
            return address - (section.VirtualAddress - section.PointerToRawData);
        }

        public static SectionHeader GetSegment(this PEHeaders header, string name)
        {
            var section = header.SectionHeaders.Where(x => x.Name == name).FirstOrDefault();
            return section;
        }

        public static SectionHeader FindSection(this PEHeaders header, int address, bool fileOffset)
        {
            foreach (var section in header.SectionHeaders)
            {
                if (fileOffset)
                {
                    if (section.PointerToRawData < address && section.PointerToRawData + section.SizeOfRawData > address) return section;
                }
                else
                    if (section.VirtualAddress < address && section.VirtualAddress + section.VirtualSize > address) return section;
            }

            return new SectionHeader();
        }
    }
}
