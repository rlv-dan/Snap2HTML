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
		private bool initDone = false;
		private bool runningAutomated = false;

        public frmMain()
        {
            InitializeComponent();
        }

		private void frmMain_Load( object sender, EventArgs e )
		{
			this.Text = Application.ProductName + " (Press F1 for Help)";
			labelAboutVersion.Text = "version " + Application.ProductVersion.Split( '.' )[0] + "." + Application.ProductVersion.Split( '.' )[1];

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

			Opacity = 0;	// for silent mode

			initDone = true;
		}

		private void frmMain_Shown( object sender, EventArgs e )
        {
            // parse command line
            var commandLine = Environment.CommandLine;
			commandLine = commandLine.Replace( "-output:", "-outfile:" );	// correct wrong parameter to avoid confusion
            var splitCommandLine = Arguments.SplitCommandLine(commandLine);
            var arguments = new Arguments(splitCommandLine);

            // first test for single argument (ie path only)
			if( splitCommandLine.Length == 2 && !arguments.Exists( "path" ) )
			{
				if( System.IO.Directory.Exists( splitCommandLine[1] ) )
				{
					SetRootPath( splitCommandLine[1] );
				}
			}

			var settings = new SnapSettings();
			if( arguments.Exists( "path" ) && arguments.Exists( "outfile" ) )
            {
				this.runningAutomated = true;

				settings.rootFolder = arguments.Single( "path" );
				settings.outputFile = arguments.Single( "outfile" );

				// First validate paths
				if( !System.IO.Directory.Exists( settings.rootFolder ) )
				{
					if( !arguments.Exists( "silent" ) )
					{
						MessageBox.Show( "Input path does not exist: " + settings.rootFolder, "Automation Error", MessageBoxButtons.OK, MessageBoxIcon.Error );
					}
					Application.Exit();
				}
				if( !System.IO.Directory.Exists( System.IO.Path.GetDirectoryName(settings.outputFile) ) )
				{
					if( !arguments.Exists( "silent" ) )
					{
						MessageBox.Show( "Output path does not exist: " + System.IO.Path.GetDirectoryName( settings.outputFile ), "Automation Error", MessageBoxButtons.OK, MessageBoxIcon.Error );
					}
					Application.Exit();
				}

				// Rest of settings

				settings.skipHiddenItems = !arguments.Exists( "hidden" );
				settings.skipSystemItems = !arguments.Exists( "system" );
				settings.openInBrowser = false;
				
				settings.linkFiles = false;
				if( arguments.Exists( "link" ) )
				{
					settings.linkFiles = true;
					settings.linkRoot = arguments.Single( "link" );
				}

				settings.title = "Snapshot of " + settings.rootFolder;
				if( arguments.Exists( "title" ) )
				{
					settings.title = arguments.Single( "title" );
				}

            }

			// keep window hidden in silent mode
			if( arguments.IsTrue( "silent" ) && this.runningAutomated )
			{
				Visible = false;
			}
			else
			{
				Opacity = 100;
			}

			if( this.runningAutomated )
			{
				StartProcessing( settings );
			}
        }

		private void frmMain_FormClosing( object sender, FormClosingEventArgs e )
		{
			if( backgroundWorker.IsBusy ) e.Cancel = true;

			if( !this.runningAutomated ) // don't save settings when automated through command line
			{
				Snap2HTML.Properties.Settings.Default.WindowLeft = this.Left;
				Snap2HTML.Properties.Settings.Default.WindowTop = this.Top;
				Snap2HTML.Properties.Settings.Default.Save();
			}
		}

		private void cmdBrowse_Click(object sender, EventArgs e)
        {
			folderBrowserDialog1.RootFolder = Environment.SpecialFolder.Desktop;    // this makes it possible to select network paths too

			// Use the following registry key when some network shares do not show up in the dialog
			// and reboot the system to apply the change.
            //Windows Registry Editor Version 5.00

            //[HKEY_LOCAL_MACHINE\SOFTWARE\Microsoft\Windows\CurrentVersion\Policies\System]
            //         "EnableLinkedConnections" = dword:00000001

			folderBrowserDialog1.SelectedPath = txtRoot.Text;
            if (folderBrowserDialog1.ShowDialog() == DialogResult.OK)
			{
				try
				{
					SetRootPath( folderBrowserDialog1.SelectedPath );
				}
				catch( System.Exception ex )
				{
					MessageBox.Show( "Could not select folder:\n\n" + ex.Message, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error );
					SetRootPath( "", false );
				}
            }
        }

        private void cmdCreate_Click(object sender, EventArgs e)
		{
			// ask for output file
			string fileName = new System.IO.DirectoryInfo( txtRoot.Text + @"\" ).Name;
			char[] invalid = System.IO.Path.GetInvalidFileNameChars();
			for (int i = 0; i < invalid.Length; i++)
			{
				fileName = fileName.Replace(invalid[i].ToString(), "");
			}

            saveFileDialog1.DefaultExt = "html";
			if( !fileName.ToLower().EndsWith( ".html" ) ) fileName += ".html";
			saveFileDialog1.FileName = fileName;
			saveFileDialog1.Filter = "HTML files (*.html)|*.html|All files (*.*)|*.*";
            saveFileDialog1.InitialDirectory = System.IO.Path.GetDirectoryName(txtRoot.Text);
			saveFileDialog1.CheckPathExists = true;
            if (saveFileDialog1.ShowDialog() != DialogResult.OK) return;

			if( !saveFileDialog1.FileName.ToLower().EndsWith( ".html" ) ) saveFileDialog1.FileName += ".html";

			// begin generating html
			var settings = new SnapSettings()
			{
				rootFolder = txtRoot.Text,
				title = txtTitle.Text,
				outputFile = saveFileDialog1.FileName,
				skipHiddenItems = !chkHidden.Checked,
				skipSystemItems = !chkSystem.Checked,
				openInBrowser = chkOpenOutput.Checked,
				linkFiles = chkLinkFiles.Checked,
				linkRoot = txtLinkRoot.Text,
			};
			StartProcessing(settings);
		}

		private void StartProcessing(SnapSettings settings)
		{
			// ensure source path format
			settings.rootFolder = System.IO.Path.GetFullPath( settings.rootFolder );
			if( settings.rootFolder.EndsWith( @"\" ) ) settings.rootFolder = settings.rootFolder.Substring( 0, settings.rootFolder.Length - 1 );
			if( Utils.IsWildcardMatch( "?:", settings.rootFolder, false ) ) settings.rootFolder += @"\";	// add backslash to path if only letter and colon eg "c:"

			// add slash or backslash to end of link (in cases where it is clear that we we can)
			if( settings.linkFiles )
			{
				if( !settings.linkRoot.EndsWith( @"/" ) )
				{
					if( settings.linkRoot.ToLower().StartsWith( @"http" ) )	// web site
					{
						settings.linkRoot += @"/";
					}
					if( Utils.IsWildcardMatch( "?:*", settings.linkRoot, false ) )	// local disk
					{
						settings.linkRoot += @"\";
					}
					if( settings.linkRoot.StartsWith( @"\\" ) )    // unc path
					{
						settings.linkRoot += @"\";
					}
				}
			}

			Cursor.Current = Cursors.WaitCursor;
			this.Text = "Snap2HTML (Working... Press Escape to Cancel)";
			tabControl1.Enabled = false;
			backgroundWorker.RunWorkerAsync( argument: settings );
		}

		private void backgroundWorker_ProgressChanged( object sender, ProgressChangedEventArgs e )
		{
			toolStripStatusLabel1.Text = e.UserState.ToString();
		}

		private void backgroundWorker_RunWorkerCompleted( object sender, RunWorkerCompletedEventArgs e )
		{
			GC.Collect();
			GC.WaitForPendingFinalizers();

			Cursor.Current = Cursors.Default;
			tabControl1.Enabled = true;
			this.Text = "Snap2HTML";

			// Quit when finished if automated via command line
			if( this.runningAutomated )
			{
				Application.Exit();
			}
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
			System.Diagnostics.Process.Start( @"https://rlvision.com/exif/about.php" );
		}
		private void linkLabel2_LinkClicked( object sender, LinkLabelLinkClickedEventArgs e )
		{
			System.Diagnostics.Process.Start( @"http://www.rlvision.com/flashren/about.php" );
		}
		private void linkLabel4_LinkClicked( object sender, LinkLabelLinkClickedEventArgs e )
		{
			System.Diagnostics.Process.Start( "notepad.exe", System.IO.Path.GetDirectoryName( Application.ExecutablePath ) + "\\template.html" );
		}
		private void linkLabel5_LinkClicked( object sender, LinkLabelLinkClickedEventArgs e )
		{
			System.Diagnostics.Process.Start( @"http://www.rlvision.com/contact.php" );
		}
		private void pictureBoxDonate_Click( object sender, EventArgs e )
		{
			System.Diagnostics.Process.Start( @"https://www.paypal.com/cgi-bin/webscr?cmd=_donations&business=U3E4HE8HMY9Q4&item_name=Snap2HTML&currency_code=USD&source=url" );
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

		// Sets the root path input box and makes related gui parts ready to use
		private void SetRootPath( string path, bool pathIsValid = true )
		{
			if( pathIsValid )
			{
				txtRoot.Text = path;
				cmdCreate.Enabled = true;
				toolStripStatusLabel1.Text = "";
				if( initDone )
				{
					txtLinkRoot.Text = txtRoot.Text;
					txtTitle.Text = "Snapshot of " + txtRoot.Text;
				}
			}
			else
			{
				txtRoot.Text = "";
				cmdCreate.Enabled = false;
				toolStripStatusLabel1.Text = "";
				if( initDone )
				{
					txtLinkRoot.Text = txtRoot.Text;
					txtTitle.Text = "";
				}
			}
		}

    }
}
