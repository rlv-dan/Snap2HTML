using CommandLine.Utility;
using Snap2HTML.Properties;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Runtime;
using System.Windows.Forms;

namespace Snap2HTML
{
    public partial class frmMain : Form
    {
		private bool initDone = false;
		private bool runningAutomated = false;
		private bool silentMode = false;

        public frmMain()
        {
            InitializeComponent();
        }

		private void frmMain_Load( object sender, EventArgs e )
		{
			var version = Application.ProductVersion.Split('.')[0] + "." + Application.ProductVersion.Split('.')[1];
            this.Text = $"{Application.ProductName} {version} (Press F1 for Help)";
			labelAboutVersion.Text = $"version {version}";

            // Initialize some settings
            int left = Settings.Default.WindowLeft;
			int top = Settings.Default.WindowTop;			
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

			// Setup drag & drop handlers
			tabPage1.DragDrop += DragDropHandler;
			tabPage1.DragEnter += DragEnterHandler;
			tabPage1.AllowDrop = true;
			foreach( Control cnt in tabPage1.Controls )
			{
				cnt.DragDrop += DragDropHandler;
				cnt.DragEnter += DragEnterHandler;
				cnt.AllowDrop = true;
			}

			HighDpiHelper.AdjustControlImagesDpiScale(this);

            Opacity = 0;	// For silent mode

			initDone = true;
		}

		private void frmMain_Shown( object sender, EventArgs e )
        {
            // Parse command line
            var commandLine = Environment.CommandLine;

            commandLine = commandLine.Replace( "-output:", "-outfile:" );	// correct wrong parameter to avoid confusion
            var splitCommandLine = Arguments.SplitCommandLine(commandLine);
            var arguments = new Arguments(splitCommandLine);

            // First test for single argument (ie path only)
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

				if (arguments.Exists("silent"))
				{
					this.silentMode = true;
				}

                settings.rootFolder = arguments.Single( "path" );
				settings.outputFile = arguments.Single( "outfile" );
				
				// First validate paths
				if( !System.IO.Directory.Exists( settings.rootFolder ) )
				{
					if( !this.silentMode)
					{
						MessageBox.Show( "Input path does not exist: " + settings.rootFolder, "Automation Error", MessageBoxButtons.OK, MessageBoxIcon.Error );
					}
					Application.Exit();
				}
				if( !System.IO.Directory.Exists( System.IO.Path.GetDirectoryName(settings.outputFile) ) )
				{
					if( !this.silentMode)
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

            if (!System.IO.File.Exists(Utils.GetTemplatePath()))
            {
                if (!this.silentMode)
                {
                    MessageBox.Show("Template file was not found:\n\n" + Utils.GetTemplatePath(), "Load Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
                Application.Exit();
            }


            // Keep window hidden in silent mode
            if (this.silentMode && this.runningAutomated )
			{
				Visible = false;
			}
			else
			{
				Opacity = 100;
			}

            if ( this.runningAutomated )
			{
				StartProcessing( settings );
			}
        }

		private void frmMain_FormClosing( object sender, FormClosingEventArgs e )
		{
			if( backgroundWorker.IsBusy ) e.Cancel = true;

			if( !this.runningAutomated ) // don't save settings when automated through command line
			{
				Settings.Default.WindowLeft = this.Left;
				Settings.Default.WindowTop = this.Top;
				Settings.Default.Save();
			}
        }

        private void cmdBrowse_Click(object sender, EventArgs e)
        {
            folderBrowserDialog1.RootFolder = Environment.SpecialFolder.Desktop;	// allows selecting network paths too
			folderBrowserDialog1.SelectedPath = txtRoot.Text;
			folderBrowserDialog1.Description = "Select the root folder to create a snapshot from:";
            if (folderBrowserDialog1.ShowDialog() == DialogResult.OK)
			{
				try
				{
					SetRootPath( folderBrowserDialog1.SelectedPath );
				}
				catch( Exception ex )
				{
					MessageBox.Show( "Could not select folder:\n\n" + ex.Message, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error );
					SetRootPath( "", false );
				}
            }
        }

        private void txtRoot_Leave(object sender, EventArgs e)
        {
            try
            {
				if (Directory.Exists(txtRoot.Text) == true)
				{
                    if (Utils.IsWildcardMatch("?:", txtRoot.Text, false))
                    {
                        txtRoot.Text += @"\";
                    }
                    SetRootPath(txtRoot.Text);
				}
            }
            catch (Exception)
            {
            }
        }

        private void cmdCreate_Click(object sender, EventArgs e)
		{
			if (System.IO.Directory.Exists(txtRoot.Text) == false)
			{
				if(silentMode == false)
				{
                    MessageBox.Show("Path does not exist:\n\n" + txtRoot.Text, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
                return;
			}

			// Ask for output file
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

			// Begin generating html
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
			// Ensure source path format
			settings.rootFolder = System.IO.Path.GetFullPath( settings.rootFolder );
			if (settings.rootFolder.EndsWith(@"\"))
			{
				settings.rootFolder = settings.rootFolder.Substring(0, settings.rootFolder.Length - 1);
			}
			if (Utils.IsWildcardMatch("?:", settings.rootFolder, false))
			{
				settings.rootFolder += @"\";    // add backslash to path if only letter and colon eg "c:"
			}

			Cursor.Current = Cursors.WaitCursor;
			this.Text = "Snap2HTML (Working... Press Escape to Cancel)";
			tabControl1.Enabled = false;
			backgroundWorker.RunWorkerAsync( argument: settings );
		}

		private void backgroundWorker_ProgressChanged( object sender, ProgressChangedEventArgs args )
		{
			var path = args.UserState.ToString();
            var shortenedPath = Utils.ShortenPath(path, toolStripStatusLabel1.Font, statusStrip1.Width - 20);
            toolStripStatusLabel1.Text = shortenedPath;
		}

		private void backgroundWorker_RunWorkerCompleted( object sender, RunWorkerCompletedEventArgs args )
		{
            if (!this.silentMode) 
			{
                if (args.Result != null)
                {
                    var errorFolders = (List<SnappedFolder>)args.Result;
					if (errorFolders.Count > 0)
					{
						var yesno = MessageBox.Show(errorFolders.Count + " folder(s) could not be read. Show details?", "Errors Reported", MessageBoxButtons.YesNo, MessageBoxIcon.Warning);
						if (yesno == DialogResult.Yes)
						{
							var errorForm = new frmErrors(errorFolders);
							errorForm.ShowDialog();
						}
					}
				}
            }

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
			System.Diagnostics.Process.Start( @"https://www.rlvision.com" );
		}
		private void linkLabel3_LinkClicked( object sender, LinkLabelLinkClickedEventArgs e )
		{
			System.Diagnostics.Process.Start( @"https://rlvision.com/exif/about.php" );
		}
		private void linkLabel2_LinkClicked( object sender, LinkLabelLinkClickedEventArgs e )
		{
			System.Diagnostics.Process.Start( @"https://www.rlvision.com/flashren/about.php" );
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
					txtLinkRoot.Text = Utils.PathToFileUri(path);
					txtTitle.Text = "Snapshot of " + path;
				}
			}
			else
			{
				txtRoot.Text = "";
				cmdCreate.Enabled = false;
				toolStripStatusLabel1.Text = "";
				if( initDone )
				{
					txtLinkRoot.Text = "";
					txtTitle.Text = "";
				}
			}
		}

    }
}
