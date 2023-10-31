using Snap2HTMLNG.Shared.Utils;
using System.Collections.Generic;

namespace Snap2HTMLNG.Shared.Models
{
    public class SnappedFolder
    {
        public SnappedFolder(string name, string path)
        {
            Name = name;
            Path = path;
            Properties = new Dictionary<string, string>();
            Files = new List<SnappedFile>();
        }

        public string Name { get; set; }
        public string Path { get; set; }
        public Dictionary<string, string> Properties { get; set; }
        public List<SnappedFile> Files { get; set; }

        public string GetFullPath()
        {
            string path = Path.EndsWith(@"\") ? Path + Name : Path + @"\" + Name;

            if (path.EndsWith(@"\")) // remove trailing backslash
            {
                if (!Utils.Legacy.Helpers.IsWildcardMatch(@"?:\", path, false)) // except for drive letters
                {
                    path = path.Remove(path.Length - 1);
                }

            }

            return path;
        }

        public string GetProp(string key)
        {
            return Properties.ContainsKey(key) ? Properties[key] : "";
        }
    }
}
