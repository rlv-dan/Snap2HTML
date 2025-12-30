using System.Collections.Generic;
using System.IO;

namespace Snap2HTML
{
    public class SnapSettings
    {
        public string rootFolder { get; set; }
        public string title { get; set; }
        public string outputFile { get; set; }
        public bool skipHiddenItems { get; set; }
        public bool skipSystemItems { get; set; }
        public bool openInBrowser { get; set; }
        public bool linkFiles { get; set; }
        public string linkRoot { get; set; }

        public SnapSettings()
        {
            this.skipHiddenItems = true;
            this.skipSystemItems = true;
            this.openInBrowser = false;
            this.linkFiles = false;
            this.linkRoot = "";
        }
    }

    public class SnapError
    {
        public string ErrorMessage { get; set; } = "";
        public bool UnauthorizedAccessException { get; set; } = false;
        public bool ReparsePoint { get; set; } = false;
    }

    public class SnappedFile
    {
        public string Name { get; }
        public long Size { get; }
        public int Modified { get; }

        public SnappedFile(FileInfo file)
        {
            this.Name = file.Name;
            this.Size = file.Length;
            this.Modified = Utils.ToUnixTimestamp(file.LastWriteTime.ToLocalTime());
        }
    }

    public class SnappedFolder
    {
        public string Name { get; }
        public string Path { get; }
        public string FullPath { get; }
        public int Modified { get; }
        public long Size { get; set; } = 0; // Only this folder
        public long DeepSize { get; set; } = 0; // Including all subfolders
        public List<SnappedFile> Files { get; }
        public SnapError Error { get; set; } = null;

        public SnappedFolder(DirectoryInfo dir)
        {
            var cleanFullPath = dir.FullName.Replace(@"\\?\", "");
            var path = System.IO.Path.GetDirectoryName(cleanFullPath);
            if(path == null)
            {
                var root = System.IO.Path.GetPathRoot(cleanFullPath);   // for UNC paths this (correctly) includes the server name
                this.Name = root;
                this.Path = null;
                this.FullPath = root;
            }
            else
            {
                this.Name = System.IO.Path.GetFileName(cleanFullPath);
                this.Path = path;
                this.FullPath = cleanFullPath;
            }

            this.Modified = Utils.ToUnixTimestamp(dir.LastWriteTime.ToLocalTime());

            this.Files = new List<SnappedFile>();
        }
    }

    public class JsMetadata
    {
        public string title { get; set; }
        public int timestamp { get; set; }
        public string sourceDir { get; set; }
        public string linkRoot { get; set;  }
        public long numFiles { get; set;  }
        public long numDirs { get; set;  }
        public long totBytes { get; set;  }

    }
}
