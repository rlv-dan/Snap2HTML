using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Text;
using System.Windows.Forms;
using CommandLine.Utility;

namespace Snap2HTML
{
    public partial class frmMain : Form
    {
        private string outFile = "";	// set when automating via command line
		private bool initDone = false;

        public frmMain()
        {
            InitializeComponent();
        }

		private void frmMain_Load( object sender, EventArgs e )
		{
			labelVersion.Text = "version " + Application.ProductVersion.Split( '.' )[0] + "." + Application.ProductVersion.Split( '.' )[1];

			// initialize some settings
			int left = Snap2HTML.Properties.Settings.Default.WindowLeft;
			int top = Snap2HTML.Properties.Settings.Default.WindowTop;			
			if( left >= 0 ) this.Left = left;
			if( top >= 0 ) this.Top = top;

			if( System.IO.Directory.Exists( txtRoot.Text ) )
			{
				SetRootPath( txtRoot.Text , true);
			}
			else
			{
				SetRootPath( "" , false );
			}

			txtLinkRoot.Enabled = chkLinkFiles.Checked;

			// setup drag & drop handlers
			tabPage1.DragDrop += DragDropHandler;
			tabPage1.DragEnter += DragEnterHandler;
			tabPage1.AllowDrop = true;
			foreach( Control cnt in tabPage1.Controls )
			{
				cnt.DragDrop += DragDropHandler;
				cnt.DragEnter += DragEnterHandler;
				cnt.AllowDrop = true;
			}

			initDone = true;
		}

		private void frmMain_Shown( object sender, EventArgs e )
        {
            // parse command line
            var commandLine = Environment.CommandLine;
            var splitCommandLine = Arguments.SplitCommandLine(commandLine);
            var arguments = new Arguments(splitCommandLine);

            // first test for single argument (ie path only)
            if (splitCommandLine.Length == 2 && !arguments.Exists("path"))
            {
                if (System.IO.Directory.Exists(splitCommandLine[1]))
                {
					SetRootPath( splitCommandLine[1] );
                }
            }

            if (arguments.IsTrue("hidden")) chkHidden.Checked = true;
            if (arguments.IsTrue("system")) chkSystem.Checked = true;
			if( arguments.Exists( "path" ) )
            {
				// note: relative paths not handled
                string path = arguments.Single( "path" );
                if( !System.IO.Directory.Exists( path ) ) path = Environment.CurrentDirectory + System.IO.Path.DirectorySeparatorChar + path;

                if( System.IO.Directory.Exists( path ) )
                {
					SetRootPath( path );

                    // if outfile is also given, start generating snapshot
                    if (arguments.Exists("outfile"))
                    {
                        outFile = arguments.Single("outfile");
                        cmdCreate.PerformClick();
                    }
                }
            }

			// run link/title after path, since path automatically updates title
			if( arguments.Exists( "link" ) )
			{
				chkLinkFiles.Checked = true;
				txtLinkRoot.Text = arguments.Single( "link" );
				txtLinkRoot.Enabled = true;
			}
			if( arguments.Exists( "title" ) )
			{
				txtTitle.Text = arguments.Single( "title" );
			}           
        }

		private void frmMain_FormClosing( object sender, FormClosingEventArgs e )
		{
			if( backgroundWorker.IsBusy ) e.Cancel = true;

			if( outFile == "" ) // don't save settings when automated through command line
			{
				Snap2HTML.Properties.Settings.Default.WindowLeft = this.Left;
				Snap2HTML.Properties.Settings.Default.WindowTop = this.Top;
				Snap2HTML.Properties.Settings.Default.Save();
			}
		}

		private void cmdBrowse_Click(object sender, EventArgs e)
        {
			folderBrowserDialog1.RootFolder = Environment.SpecialFolder.Desktop;	// this makes it possible to select network paths too
			folderBrowserDialog1.SelectedPath = txtRoot.Text;
            if (folderBrowserDialog1.ShowDialog() == DialogResult.OK)
			{
				try
				{
					SetRootPath( folderBrowserDialog1.SelectedPath );
				}
				catch( System.Exception ex )
				{
					MessageBox.Show( "Could not select folder: " + ex.Message, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error );
					SetRootPath( "", false );
				}
            }
        }

        private void cmdCreate_Click(object sender, EventArgs e)
		{
			// ensure source path format
            txtRoot.Text = System.IO.Path.GetFullPath( txtRoot.Text );
            if (txtRoot.Text.EndsWith(@"\")) txtRoot.Text = txtRoot.Text.Substring(0, txtRoot.Text.Length - 1);
            if ( IsWildcardMatch( "?:" , txtRoot.Text , false ) ) txtRoot.Text += @"\";	// add backslash to path if only letter and colon eg "c:"

			// add slash or backslash to end of link (in cases where it is clearthat we we can)
			if( !txtLinkRoot.Text.EndsWith( @"/" ) && txtLinkRoot.Text.ToLower().StartsWith( @"http" ) )	// web site
			{
				txtLinkRoot.Text += @"/";
			}
			if( !txtLinkRoot.Text.EndsWith( @"\" ) && IsWildcardMatch( "?:*" , txtLinkRoot.Text , false ))	// local disk
			{
				txtLinkRoot.Text += @"\";
			}

			// get output file
			if( outFile == "" )
            {
				string fileName = new System.IO.DirectoryInfo( txtRoot.Text + @"\" ).Name;
				char[] invalid = System.IO.Path.GetInvalidFileNameChars();
				for (int i = 0; i < invalid.Length; i++)
				{
					fileName = fileName.Replace(invalid[i].ToString(), "");
				}

                saveFileDialog1.DefaultExt = "html";
				if( !fileName.ToLower().EndsWith( ".html" ) ) fileName += ".html";
				saveFileDialog1.FileName = fileName;
                saveFileDialog1.InitialDirectory = System.IO.Path.GetDirectoryName(txtRoot.Text);
                if (saveFileDialog1.ShowDialog() != DialogResult.OK) return;
            }
            else // command line
            {
                saveFileDialog1.FileName = outFile;
            }

			if( !saveFileDialog1.FileName.ToLower().EndsWith( ".html" ) ) saveFileDialog1.FileName += ".html";

			// make sure output path exists
			if( !System.IO.Directory.Exists( System.IO.Path.GetDirectoryName( saveFileDialog1.FileName ) ) )
			{
				MessageBox.Show( "The output folder does not exists...\n\n" + System.IO.Path.GetDirectoryName( saveFileDialog1.FileName ), "Error", MessageBoxButtons.OK, MessageBoxIcon.Error );
				return;
			}

			// begin generating html
			Cursor.Current = Cursors.WaitCursor;
			this.Text = "Snap2HTML (Working... Press Escape to Cancel)";
			tabControl1.Enabled = false;
			backgroundWorker.RunWorkerAsync();

		}

        private void chkLinkFiles_CheckedChanged(object sender, EventArgs e)
        {
            if (chkLinkFiles.Checked == true)
                txtLinkRoot.Enabled = true;
            else
                txtLinkRoot.Enabled = false;
        }

		// Link Label handlers
		private void linkLabel1_LinkClicked( object sender, LinkLabelLinkClickedEventArgs e )
		{
			System.Diagnostics.Process.Start( @"http://www.rlvision.com" );
		}
		private void linkLabel3_LinkClicked( object sender, LinkLabelLinkClickedEventArgs e )
		{
			System.Diagnostics.Process.Start( @"http://www.rlvision.com/snap2img/about.asp" );
		}
		private void linkLabel2_LinkClicked( object sender, LinkLabelLinkClickedEventArgs e )
		{
			System.Diagnostics.Process.Start( @"http://www.rlvision.com/flashren/about.asp" );
		}
		private void linkLabel4_LinkClicked( object sender, LinkLabelLinkClickedEventArgs e )
		{
			System.Diagnostics.Process.Start( "notepad.exe", System.IO.Path.GetDirectoryName( Application.ExecutablePath ) + "\\template.html" );
		}
		private void linkLabel5_LinkClicked( object sender, LinkLabelLinkClickedEventArgs e )
		{
			System.Diagnostics.Process.Start( @"http://www.rlvision.com/contact.asp" );
		}

		// Drag & Drop handlers
		private void DragEnterHandler( object sender, DragEventArgs e )
		{
			if( e.Data.GetDataPresent( DataFormats.FileDrop ) )
			{
				e.Effect = DragDropEffects.Copy;
			}
			else
			{
				e.Effect = DragDropEffects.None;
			}
		}
		private void DragDropHandler( object sender, DragEventArgs e )
		{
			if( e.Data.GetDataPresent( DataFormats.FileDrop ) )
			{
				string[] files = (string[])e.Data.GetData( DataFormats.FileDrop );
				if( files.Length == 1 && System.IO.Directory.Exists( files[0] ) )
				{
					SetRootPath( files[0] );
				}
			}
		}

		// Escape to cancel
		private void frmMain_KeyUp( object sender, KeyEventArgs e )
		{
			if( backgroundWorker.IsBusy )
			{
				if( e.KeyCode == Keys.Escape )
				{
					backgroundWorker.CancelAsync();
				}
			}
			else
			{
				if( e.KeyCode == Keys.F1 )
				{
					System.Diagnostics.Process.Start( System.IO.Path.GetDirectoryName( Application.ExecutablePath ) + "\\ReadMe.txt" );
				}
			}
		}

    }
}
