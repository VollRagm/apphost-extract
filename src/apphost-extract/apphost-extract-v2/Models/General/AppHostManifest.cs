using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace apphost_extract_v2.General
{
    public class AppHostManifest
    {
        public List<AppHostFileEntry> FileEntries { get; set; }

        public AppHostManifest()
        {
            FileEntries = new List<AppHostFileEntry>();
        }
    }
}
