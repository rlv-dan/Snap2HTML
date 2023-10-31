using System.ComponentModel;
using System.Windows.Forms;
using Snap2HTMLNG.Shared.Builder;

namespace Snap2HTMLNG
{
    public partial class frmMain : Form
    {
        // This runs on a separate thread from the GUI
        private void backgroundWorker_DoWork(object sender, DoWorkEventArgs e)
        {
            DataBuilder.Build(Application.ProductName, Application.ProductVersion, backgroundWorker);
        }
        
    }
}

