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
		private void backgroundWorker_DoWork( object sender, DoWorkEventArgs e )
		{
			backgroundWorker.ReportProgress( 0, "Reading folders..." );
			var sbDirArrays = new StringBuilder();
			int prevDepth = -100;

			// Get all folders
			var dirs = new List<string>();
			dirs.Insert( 0, txtRoot.Text );
			var skipHidden = ( chkHidden.CheckState == CheckState.Unchecked );
			var skipSystem = ( chkSystem.CheckState == CheckState.Unchecked );
			DirSearch( txtRoot.Text, dirs, skipHidden, skipSystem );
			dirs = SortDirList( dirs );

			if( backgroundWorker.CancellationPending )
			{
				backgroundWorker.ReportProgress( 0, "User cancelled" );
				return;
			}

			int totDirs = 0;
			dirs.Add( "*EOF*" );
			long totSize = 0;
			int totFiles = 0;
			var lineBreakSymbol = "";	// could set to \n to make html more readable at the expense of increased size

			// Get files in folders
			for( int d = 0; d < dirs.Count; d++ )
			{
				string currentDir = dirs[d];

				try
				{
					int newDepth = currentDir.Split( System.IO.Path.DirectorySeparatorChar ).Length;
					if( currentDir.Length < 64 && currentDir == System.IO.Path.GetPathRoot( currentDir ) ) newDepth--;	// fix reading from rootfolder, <64 to avoid going over MAX_PATH

					prevDepth = newDepth;

					var sbCurrentDirArrays = new StringBuilder();

					if( currentDir != "*EOF*" )
					{
						bool no_problem = true;

						try
						{
							var files = new List<string>( System.IO.Directory.GetFiles( currentDir, "*.*", System.IO.SearchOption.TopDirectoryOnly ) );
							files.Sort();
							int f = 0;

							string last_write_date = "-";
							last_write_date = System.IO.Directory.GetLastWriteTime( currentDir ).ToLocalTime().ToString();
							long dir_size = 0;

							sbCurrentDirArrays.Append( "D.p([" + lineBreakSymbol );
							var sDirWithForwardSlash = currentDir.Replace( @"\", "/" );
							sbCurrentDirArrays.Append( "\"" ).Append( MakeCleanJsString( sDirWithForwardSlash ) ).Append( "*" ).Append( dir_size ).Append( "*" ).Append( last_write_date ).Append( "\"," + lineBreakSymbol );
							f++;
							long dirSize = 0;
							foreach( string sFile in files )
							{
								bool bInclude = true;
								long fi_length = 0;
								last_write_date = "-";
								try
								{
									System.IO.FileInfo fi = new System.IO.FileInfo( sFile );
									if( ( fi.Attributes & System.IO.FileAttributes.Hidden ) == System.IO.FileAttributes.Hidden && chkHidden.CheckState != CheckState.Checked ) bInclude = false;
									if( ( fi.Attributes & System.IO.FileAttributes.System ) == System.IO.FileAttributes.System && chkSystem.CheckState != CheckState.Checked ) bInclude = false;
									fi_length = fi.Length;

									try
									{
										last_write_date = fi.LastWriteTime.ToLocalTime().ToString();
									}
									catch( Exception ex )
									{
										Console.WriteLine( "{0} Exception caught.", ex );
									}
								}
								catch( Exception ex )
								{
									Console.WriteLine( "{0} Exception caught.", ex );
									bInclude = false;
								}

								if( bInclude )
								{
									sbCurrentDirArrays.Append( "\"" ).Append( MakeCleanJsString( System.IO.Path.GetFileName( sFile ) ) ).Append( "*" ).Append( fi_length ).Append( "*" ).Append( last_write_date ).Append( "\"," + lineBreakSymbol );
									totSize += fi_length;
									dirSize += fi_length;
									totFiles++;
									f++;

									if( totFiles % 9 == 0 )
									{
										backgroundWorker.ReportProgress( 0, "Reading files... " + totFiles + " (" + sFile + ")" );
									}

								}
								if( backgroundWorker.CancellationPending )
								{
									backgroundWorker.ReportProgress( 0, "Operation Cancelled!" );
									return;
								}
							}

							// Add total dir size
							sbCurrentDirArrays.Append( "" ).Append( dirSize ).Append( "," + lineBreakSymbol );

							// Add subfolders
							string subdirs = "";
							List<string> lstSubDirs = new List<string>( System.IO.Directory.GetDirectories( currentDir ) );
							lstSubDirs = SortDirList( lstSubDirs );
							foreach( string sTmp in lstSubDirs )
							{
								int i = dirs.IndexOf( sTmp );
								if( i != -1 ) subdirs += i + "*";
							}
							if( subdirs.EndsWith( "*" ) ) subdirs = subdirs.Remove( subdirs.Length - 1 );
							sbCurrentDirArrays.Append( "\"" ).Append( subdirs ).Append( "\"" + lineBreakSymbol );	// subdirs
							sbCurrentDirArrays.Append( "])" );
							sbCurrentDirArrays.Append( "\n" );
						}
						catch( Exception ex )
						{
							Console.WriteLine( "{0} Exception caught.", ex );
							no_problem = false;
						}

						if( no_problem == false )	// We need to keep folder even if error occurred for integrity
						{
							var sDirWithForwardSlash = currentDir.Replace( @"\", "/" );
							sbCurrentDirArrays = new StringBuilder();
							sbCurrentDirArrays.Append( "D.p([\"" ).Append( MakeCleanJsString( sDirWithForwardSlash ) ).Append( "*0*-\"," + lineBreakSymbol );
							sbCurrentDirArrays.Append( "0," + lineBreakSymbol );	// total dir size
							sbCurrentDirArrays.Append( "\"\"" + lineBreakSymbol );	// subdirs
							sbCurrentDirArrays.Append( "])\n" );
							no_problem = true;
						}

						if( no_problem )
						{
							sbDirArrays.Append( sbCurrentDirArrays.ToString() );
							totDirs++;
						}
					}

				}
				catch( System.Exception ex )
				{
					Console.WriteLine( "{0} exception caught: {1}", ex, ex.Message );
				}

			}

			// -- Generate Output --

			backgroundWorker.ReportProgress( 0, "Generating HTML file..." );

			// Read template
			var sbContent = new StringBuilder();
			try
			{
				using( System.IO.StreamReader reader = new System.IO.StreamReader( System.IO.Path.GetDirectoryName( Application.ExecutablePath ) + System.IO.Path.DirectorySeparatorChar + "template.html" ) )
				{
					sbContent.Append( reader.ReadToEnd() );
				}
			}
			catch( System.Exception ex )
			{
				MessageBox.Show( "Failed to open 'Template.html' for reading...", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error );
				backgroundWorker.ReportProgress( 0, "An error occurred..." );
				return;
			}

			// Build HTML
			sbContent.Replace( "[DIR DATA]", sbDirArrays.ToString() );
			sbContent.Replace( "[TITLE]", txtTitle.Text );
			sbContent.Replace( "[APP LINK]", "http://www.rlvision.com" );
			sbContent.Replace( "[APP NAME]", Application.ProductName );
			sbContent.Replace( "[APP VER]", Application.ProductVersion.Split( '.' )[0] + "." + Application.ProductVersion.Split( '.' )[1] );
			sbContent.Replace( "[GEN TIME]", DateTime.Now.ToString( "t" ) );
			sbContent.Replace( "[GEN DATE]", DateTime.Now.ToString( "d" ) );
			sbContent.Replace( "[NUM FILES]", totFiles.ToString() );
			sbContent.Replace( "[NUM DIRS]", totDirs.ToString() );
			sbContent.Replace( "[TOT SIZE]", totSize.ToString() );
			if( chkLinkFiles.Checked )
			{
				sbContent.Replace( "[LINK FILES]", "true" );
				sbContent.Replace( "[LINK ROOT]", txtLinkRoot.Text.Replace( @"\", "/" ) );
				sbContent.Replace( "[SOURCE ROOT]", txtRoot.Text.Replace( @"\", "/" ) );

				string link_root = txtLinkRoot.Text.Replace( @"\", "/" );
				if( IsWildcardMatch( @"?:/*", link_root, false ) )  // "file://" is needed in the browser if path begins with drive letter, else it should not be used
				{
					sbContent.Replace( "[LINK PROTOCOL]", @"file://" );
				}
				else
				{
					sbContent.Replace( "[LINK PROTOCOL]", "" );
				}
			}
			else
			{
				sbContent.Replace( "[LINK FILES]", "false" );
				sbContent.Replace( "[LINK PROTOCOL]", "" );
				sbContent.Replace( "[LINK ROOT]", "" );
				sbContent.Replace( "[SOURCE ROOT]", txtRoot.Text.Replace( @"\", "/" ) );
			}

			// Write output file
			try
			{
				using( System.IO.StreamWriter writer = new System.IO.StreamWriter( saveFileDialog1.FileName ) )
				{
					writer.Write( sbContent.ToString() );
				}

				if( chkOpenOutput.Checked == true )
				{
					System.Diagnostics.Process.Start( saveFileDialog1.FileName );
				}
			}
			catch( System.Exception excpt )
			{
				MessageBox.Show( "Failed to open file for writing:\n\n" + excpt, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error );
				backgroundWorker.ReportProgress( 0, "An error occurred..." );
				return;
			}

			// Ready!
			Cursor.Current = Cursors.Default;
			backgroundWorker.ReportProgress( 100, "Ready!" );
		}

		private void backgroundWorker_ProgressChanged( object sender, ProgressChangedEventArgs e )
		{
			toolStripStatusLabel1.Text = e.UserState.ToString();
		}

		private void backgroundWorker_RunWorkerCompleted( object sender, RunWorkerCompletedEventArgs e )
		{
			Cursor.Current = Cursors.Default;
			tabControl1.Enabled = true;
			this.Text = "Snap2HTML";

			// Quit when finished if automated via command line
			if( outFile != "" )
			{
				Application.Exit();
			}
		}
	}
}
