using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace apphost_extract
{
    public class AppHostFileHeader
    {
        private const int HEADER_SIZE = 0xD;

        private byte[] Raw;

        public string Path { get; set; }
        
        public AppHostManifest Manifest { get; set; }

        public AppHostFileHeader(FileStream File, long HeaderOffset)
        {
            File.Seek(HeaderOffset, SeekOrigin.Begin);
            byte[] headerBuffer = new byte[HEADER_SIZE];
            File.Read(headerBuffer, 0, HEADER_SIZE);
            Raw = headerBuffer;

            byte[] stringBuffer = new byte[Raw[0xC]];
            File.Read(stringBuffer, 0, stringBuffer.Length);
            Path = Encoding.UTF8.GetString(stringBuffer);
            Log.Info("Header parsed successfully!");

            Manifest = new AppHostManifest(File, BitConverter.ToInt32(Raw, 0x8));

           
        }

        
    }
}
