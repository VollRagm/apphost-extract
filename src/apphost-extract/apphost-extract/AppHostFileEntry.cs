using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace apphost_extract
{
    public class AppHostFileEntry
    {
        public long Offset { get; set; }
        public int Size { get; set; }
        public string Name { get; set; }

        private byte[] Raw;
        private FileStream FileStream;

        private const int FILE_ENTRY_SIZE = 0x12;

        public AppHostFileEntry(FileStream File)
        {
            FileStream = File;
            byte[] entryBuffer = new byte[FILE_ENTRY_SIZE];
            File.Read(entryBuffer, 0, entryBuffer.Length);
            Raw = entryBuffer;

            Offset = BitConverter.ToInt64(Raw, 0);

            //hopefully nobody embeds a file larger than 2GB :D
            Size = (int)BitConverter.ToInt64(Raw, 0x8); 

            byte[] stringBuffer = new byte[Raw[0x11]];
            File.Read(stringBuffer, 0, stringBuffer.Length);
            Name = Encoding.UTF8.GetString(stringBuffer);
        }

        public byte[] Read()
        {
            //jumps to the offsets and reads the bytes
            byte[] buffer = new byte[Size];
            FileStream.Seek(Offset, SeekOrigin.Begin);
            FileStream.Read(buffer, 0, Size);
            return buffer;
        }
    }
}
