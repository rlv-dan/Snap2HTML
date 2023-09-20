using System.Collections.Generic;

namespace Snap2HTML.Model
{
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
