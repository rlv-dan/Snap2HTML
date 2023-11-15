using Snap2HTMLNG.Shared.Models;
using Snap2HTMLNG.Shared.Updater;
using System.Diagnostics;
using System.Windows.Forms;

namespace Snap2HTMLNG.Forms
{
    public partial class frmUpdateNotice : Form
    {
        private readonly Updater updater = new Updater();
        private string _releaseUrl = string.Empty;

        public frmUpdateNotice()
        {
            InitializeComponent();

            GetReleaseInformation();
        }

        private void GetReleaseInformation()
        {
            ReleasesModel data = updater.ReturnReleaseInformation();

            lblCurrentVersionNotice.Text = $"You are on version {Application.ProductVersion}, the latest version is {data.tag_name}";
            txtReleaseInformation.Text = $"{data.body}";
            lblReleaseDateValue.Text = $"{data.published_at:U}";

            _releaseUrl = $"{data.html_url}";
        }

        private void btnDownload_Click(object sender, System.EventArgs e)
        {
            Process.Start(_releaseUrl);
        }
    }
}
