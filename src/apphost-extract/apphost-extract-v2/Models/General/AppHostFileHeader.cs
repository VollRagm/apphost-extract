using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace apphost_extract_v2.General
{
    public class AppHostFileHeader
    {
        public byte[] Raw;

        public string Path { get; set; }
        
        public AppHostManifest Manifest { get; set; }

    }
}
