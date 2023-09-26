using System.Collections.Generic;

namespace Snap2HTMLNG.Shared.Models
{
    public class SnappedFile
    {
        public SnappedFile(string name)
        {
            Name = name;
            Properties = new Dictionary<string, string>();
        }

        public string Name { get; set; }
        public Dictionary<string, string> Properties { get; set; }

        public string GetProp(string key)
        {
            return Properties.ContainsKey(key) ? Properties[key] : "";
        }

    }
}
