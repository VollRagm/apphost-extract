using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace apphost_extract
{
    public class AppHostManifest
    {
        public IReadOnlyList<AppHostFileEntry> FileEntries { get; set; }

        public AppHostManifest(FileStream File, int embeddedFileCount)
        {
            List<AppHostFileEntry> entries = new List<AppHostFileEntry>();
            
            for(int i = 0; i < embeddedFileCount; i++)
            {
                AppHostFileEntry entry = new AppHostFileEntry(File);
                entries.Add(entry);
            }
            FileEntries = entries.AsReadOnly();

            Log.Info("Manifest parsed successfully!");
        }
    }
}
