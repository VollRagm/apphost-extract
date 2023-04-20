using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace apphost_extract_v2.General
{
    public class AppHostFileEntry
    {
        public long Offset { get; set; }
        public int Size { get; set; }
        public string Name { get; set; }

        public byte[] Raw;
    }
}
