using System;
using System.ComponentModel;
using System.IO;
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
			int left = Properties.Settings.Default.WindowLeft;
			int top = Properties.Settings.Default.WindowTop;			
			if( left >= 0 ) this.Left = left;
			if( top >= 0 ) this.Top = top;

			if(Directory.Exists(txtRoot.Text))
			{
				SetRootPath(txtRoot.Text);
			}
			else
			{
				SetRootPath("");
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
				if( Directory.Exists( splitCommandLine[1] ) )
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
				if(!Directory.Exists( settings.rootFolder ) )
				{
					if( !arguments.Exists( "silent" ) )
					{
						MessageBox.Show( "Input path does not exist: " + settings.rootFolder, "Automation Error", MessageBoxButtons.OK, MessageBoxIcon.Error );
					}
					Application.Exit();
				}
				if(!Directory.Exists(Path.GetDirectoryName(settings.outputFile)))
				{
					if( !arguments.Exists( "silent" ) )
					{
						MessageBox.Show( "Output path does not exist: " + Path.GetDirectoryName( settings.outputFile ), "Automation Error", MessageBoxButtons.OK, MessageBoxIcon.Error );
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
				Properties.Settings.Default.WindowLeft = this.Left;
				Properties.Settings.Default.WindowTop = this.Top;
				Properties.Settings.Default.Save();
			}
		}

		private void cmdBrowse_Click(object sender, EventArgs e)
        {
			folderBrowserDialog1.RootFolder = Environment.SpecialFolder.Desktop;	// this makes it possible to select network paths too
			folderBrowserDialog1.SelectedPath = txtRoot.Text;

            if (folderBrowserDialog1.ShowDialog() == DialogResult.OK)
			{
				txtRoot.Text = folderBrowserDialog1.SelectedPath;
            }
        }

        private void cmdCreate_Click(object sender, EventArgs e)
		{
			if (SetRootPath(txtRoot.Text))
			{

				// Check if the search pattern starts with an astrix or not
				if(!txtSearchPattern.Text.StartsWith("*"))
                {
					string tmp = txtSearchPattern.Text;
					txtSearchPattern.Text = $"*{txtSearchPattern.Text}";
                }

				// ask for output file
				string fileName = new DirectoryInfo(txtRoot.Text + @"\").Name;
				char[] invalid = Path.GetInvalidFileNameChars();
				for (int i = 0; i < invalid.Length; i++)
				{
					fileName = fileName.Replace(invalid[i].ToString(), "");
				}

				saveFileDialog1.DefaultExt = "html";
				if (!fileName.ToLower().EndsWith(".html")) fileName += ".html";
				saveFileDialog1.FileName = fileName;
				saveFileDialog1.Filter = "HTML files (*.html)|*.html|All files (*.*)|*.*";
				saveFileDialog1.InitialDirectory = Path.GetDirectoryName(txtRoot.Text);
				saveFileDialog1.CheckPathExists = true;
				if (saveFileDialog1.ShowDialog() != DialogResult.OK) return;

				if (!saveFileDialog1.FileName.ToLower().EndsWith(".html")) saveFileDialog1.FileName += ".html";

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
					searchPattern = txtSearchPattern.Text,
				};

				StartProcessing(settings);
			}
		}

		private void StartProcessing(SnapSettings settings)
		{
			// ensure source path format
			settings.rootFolder = Path.GetFullPath( settings.rootFolder );
			if( settings.rootFolder.EndsWith( @"\" ) ) settings.rootFolder = settings.rootFolder.Substring( 0, settings.rootFolder.Length - 1 );
			if( Utils.IsWildcardMatch( "?:", settings.rootFolder, false ) ) settings.rootFolder += @"\";	// add backslash to path if only letter and colon eg "c:"

			// add slash or backslash to end of link (in cases where it is clear that we we can)
			if( settings.linkFiles )
			{
				if( !settings.linkRoot.EndsWith( @"/" ) )
				{
					if( settings.linkRoot.ToLower().StartsWith(@"http") || settings.linkRoot.ToLower().StartsWith(@"https"))	// web site
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
		#region Link Label Handlers
		private void linkLabel1_LinkClicked(object sender, LinkLabelLinkClickedEventArgs e)
		{
			System.Diagnostics.Process.Start(@"http://www.rlvision.com");
		}
		private void linkLabel3_LinkClicked(object sender, LinkLabelLinkClickedEventArgs e)
		{
			System.Diagnostics.Process.Start(@"https://rlvision.com/exif/about.php");
		}
		private void linkLabel2_LinkClicked(object sender, LinkLabelLinkClickedEventArgs e)
		{
			System.Diagnostics.Process.Start(@"http://www.rlvision.com/flashren/about.php");
		}
		private void linkLabel4_LinkClicked(object sender, LinkLabelLinkClickedEventArgs e)
		{
			System.Diagnostics.Process.Start("notepad.exe", Path.GetDirectoryName(Application.ExecutablePath) + "\\template.html");
		}
		private void linkLabel5_LinkClicked(object sender, LinkLabelLinkClickedEventArgs e)
		{
			System.Diagnostics.Process.Start(@"http://www.rlvision.com/contact.php");
		}
		private void pictureBoxDonate_Click(object sender, EventArgs e)
		{
			System.Diagnostics.Process.Start(@"https://www.paypal.com/cgi-bin/webscr?cmd=_donations&business=U3E4HE8HMY9Q4&item_name=Snap2HTML&currency_code=USD&source=url");
		}

		private void linkLabelLaim_LinkClicked(object sender, LinkLabelLinkClickedEventArgs e)
		{
			System.Diagnostics.Process.Start("https://github.com/laim");
		}
		#endregion

		// Drag & Drop handlers
		#region Drag & Drop
		private void DragEnterHandler(object sender, DragEventArgs e)
		{
            if (e.Data.GetDataPresent(DataFormats.FileDrop))
            {
                e.Effect = DragDropEffects.Copy;
            }
            else
            {
                e.Effect = DragDropEffects.None;
            }
		}
		private void DragDropHandler(object sender, DragEventArgs e)
		{
            if (e.Data.GetDataPresent(DataFormats.FileDrop))
            {
				var path = ((string[])e.Data.GetData(DataFormats.FileDrop))[0];

				if(Directory.Exists(path))
                {
					txtRoot.Text = path;
					toolStripStatusLabel1.Text = $"Set Root Path to {path}";
                } else
                {
					toolStripStatusLabel1.Text = "Path does not exist or is invalid.";
                }
			}
        }
		#endregion

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
					System.Diagnostics.Process.Start( Path.GetDirectoryName( Application.ExecutablePath ) + "\\ReadMe.txt" );
				}
			}
		}

		/// <summary>
		/// Does some general UI related things and checks if the Path selected exists
		/// </summary>
		/// <param name="path">The path you want to scan</param>
		/// <returns></returns>
		private bool SetRootPath(string path)
		{
			try
            {
				if (Directory.Exists(path))
				{
					toolStripStatusLabel1.Text = "";

					if (initDone)
					{
						txtLinkRoot.Text = txtRoot.Text;
						txtTitle.Text = "Snapshot of " + txtRoot.Text;
					}

					return true;
				}
				else
				{
					toolStripStatusLabel1.Text = "Root path is invalid!";

					if (initDone)
					{
						txtLinkRoot.Text = txtRoot.Text;
						txtTitle.Text = "";
					}

					return false;
				}
			} catch (Exception ex)
            {
				MessageBox.Show("Could not select folder:\n\n" + ex.Message, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
				return false;
			}

		}
    }
}
