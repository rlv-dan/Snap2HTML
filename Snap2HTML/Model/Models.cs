using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

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
        public String searchPattern { get; set; }


        public SnapSettings()
        {
            this.skipHiddenItems = true;
            this.skipSystemItems = true;
            this.openInBrowser = false;
            this.linkFiles = false;
            this.linkRoot = "";
            this.searchPattern = "*.*";
        }
    }


    public class SnappedFile
    {
        public SnappedFile(string name)
        {
            this.Name = name;
            this.Properties = new Dictionary<string, string>();
        }

        public string Name { get; set; }
        public Dictionary<string, string> Properties { get; set; }

        public string GetProp(string key)
        {
            if (this.Properties.ContainsKey(key))
                return this.Properties[key];
            else
                return "";
        }

    }

    public class SnappedFolder
    {
        public SnappedFolder(string name, string path)
        {
            this.Name = name;
            this.Path = path;
            this.Properties = new Dictionary<string, string>();
            this.Files = new List<SnappedFile>();
        }

        public string Name { get; set; }
        public string Path { get; set; }
        public Dictionary<string, string> Properties { get; set; }
        public List<SnappedFile> Files { get; set; }

        public string GetFullPath()
        {
            string path;

            if (this.Path.EndsWith(@"\"))
                path = this.Path + this.Name;
            else
                path = this.Path + @"\" + this.Name;

            if (path.EndsWith(@"\")) // remove trailing backslash
            {
                if (!Utils.IsWildcardMatch(@"?:\", path, false)) // except for drive letters
                {
                    path = path.Remove(path.Length - 1);
                }

            }

            return path;
        }

        public string GetProp(string key)
        {
            if (this.Properties.ContainsKey(key))
                return this.Properties[key];
            else
                return "";
        }
    }
}
