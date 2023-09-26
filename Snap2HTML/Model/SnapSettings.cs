using System;

namespace Snap2HTMLNG.Model
{
    /// <summary>
    /// Legacy Settings System
    /// </summary>
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
            skipHiddenItems = true;
            skipSystemItems = true;
            openInBrowser = false;
            linkFiles = false;
            linkRoot = "";
            searchPattern = "*.*";
        }
    }
}
