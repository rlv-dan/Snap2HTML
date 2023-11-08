using System.ComponentModel;
using System.Windows.Forms;
using Snap2HTMLNG.Shared.Builder;
using Snap2HTMLNG.Shared.Models;
using Snap2HTMLNG.Shared.Settings;

namespace Snap2HTMLNG
{
    public partial class frmMain : Form
    {
        // This runs on a separate thread from the GUI
        private void backgroundWorker_DoWork(object sender, DoWorkEventArgs e)
        {

            // Load the user settings from the configuration file as we're using the GUI here
            UserSettingsModel usm = new UserSettingsModel
            {
                RootDirectory = XmlConfigurator.Read("RootFolder"),
                Title = XmlConfigurator.Read("Title"),
                OutputFile = XmlConfigurator.Read("OutputFile"),
                SkipHiddenItems = bool.Parse(XmlConfigurator.Read("SkipHiddenItems")),
                SkipSystemItems = bool.Parse(XmlConfigurator.Read("SkipSystemItems")),
                OpenInBrowserAfterCapture = bool.Parse(XmlConfigurator.Read("OpenInBrowserAfterCapture")),
                LinkFiles = bool.Parse(XmlConfigurator.Read("LinkFiles")),
                LinkRoot = XmlConfigurator.Read("LinkRoot"),
                SearchPattern = XmlConfigurator.Read("SearchPattern")
            };

            DataBuilder.Build(usm, Application.ProductName, Application.ProductVersion, backgroundWorker);
        }
        
    }
}

