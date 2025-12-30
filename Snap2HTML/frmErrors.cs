using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace Snap2HTML
{
    public partial class frmErrors : Form
    {
        public frmErrors(List<SnappedFolder> errorFolders)
        {
            InitializeComponent();

            var errorText = new StringBuilder();
            errorText.AppendLine($"{errorFolders.Count} error(s) reported");
            if(Utils.IsAdministrator() == false)
            {
                errorText.AppendLine("Run Snap2HTML as admin to reduce risk of errors!");
            }
            errorText.AppendLine();

            foreach (var e in errorFolders)
            {
                errorText.AppendLine(e.Error.ErrorMessage.Replace(@"\\?\", ""));
            }

            textBox1.Text = errorText.ToString();
            textBox1.Select(0, 0);
        }

        private void frmErrors_KeyUp(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Escape)
            {
                this.Close();
            }
            if (e.Control)
            {
                if (e.KeyCode == Keys.Add)
                {
                    var oldSize = textBox1.Font.Size;
                    textBox1.Font = new Font("Consolas", oldSize + 1, FontStyle.Regular);
                }
                if (e.KeyCode == Keys.Subtract)
                {
                    var oldSize = textBox1.Font.Size;
                    if (oldSize > 8)
                    {
                        textBox1.Font = new Font("Consolas", oldSize - 1, FontStyle.Regular);
                    }
                }
            }
        }
    }
}
