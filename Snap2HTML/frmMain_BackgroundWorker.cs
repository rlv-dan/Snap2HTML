using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Text;
using System.Windows.Forms;
using CommandLine.Utility;
using System.IO;
using System.Diagnostics;

namespace Snap2HTML
{
	public partial class frmMain : Form
	{
		private void backgroundWorker_DoWork( object sender, DoWorkEventArgs e )
		{
			var skipHidden = ( chkHidden.CheckState == CheckState.Unchecked );
			var skipSystem = ( chkSystem.CheckState == CheckState.Unchecked );

			// Get files & folders
			var content = GetContent( txtRoot.Text, skipHidden, skipSystem );
			if( content == null )
			{
				backgroundWorker.ReportProgress( 0, "Error reading source" );
				return;
			}
			if( backgroundWorker.CancellationPending )
			{
				backgroundWorker.ReportProgress( 0, "User cancelled" );
				return;
			}

			// Convert to string with JS data object
			var jsContent = BuildJavascriptContentArray( content, 0 );
			if( backgroundWorker.CancellationPending )
			{
				backgroundWorker.ReportProgress( 0, "User cancelled" );
				return;
			}

			// Calculate some stats
			int totDirs = 0;
			int totFiles = 0;
			long totSize = 0;
			foreach( var folder in content )
			{
				totDirs++;
				foreach( var file in folder.Files )
				{
					totFiles++;
					totSize += Int64.Parse( file.GetProp( "Size" ) );
				}
			}

			// Let's generate the output

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
			sbContent.Replace( "[DIR DATA]", jsContent );
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


		// --------------------------------------------------------------------

		public class SnappedFile
		{
			public SnappedFile( string name )
			{
				this.Name = name;
				this.Properties = new Dictionary<string, string>();
			}

			public string Name { get; set; }
			public Dictionary<string, string> Properties { get; set; }

			public string GetProp( string key )
			{
				if( this.Properties.ContainsKey( key ) )
					return this.Properties[key];
				else
					return "";
			}

		}

		public class SnappedFolder
		{
			public SnappedFolder( string name, string path )
			{
				this.Name = name;
				this.Path = path;
				this.Properties = new Dictionary<string, string>();
				this.Files = new List<SnappedFile>();
				this.FullPath = ( this.Path + "\\" + this.Name ).Replace( "\\\\", "\\" );
			}

			public string Name { get; set; }
			public string Path { get; set; }
			public string FullPath { get; set; }
			public Dictionary<string, string> Properties { get; set; }
			public List<SnappedFile> Files { get; set; }

			public string GetProp( string key )
			{
				if( this.Properties.ContainsKey( key ) )
					return this.Properties[key];
				else
					return "";
			}
		}

		// --------------------------------------------------------------------

		private List<SnappedFolder> GetContent( string rootFolder, bool skipHidden, bool skipSystem )
		{
			var stopwatch = new Stopwatch();
			stopwatch.Start();

			var result = new List<SnappedFolder>();

			// Get all folders
			var dirs = new List<string>();
			dirs.Insert( 0, rootFolder );
			DirSearch( rootFolder, dirs, skipHidden, skipSystem, stopwatch );
			dirs = SortDirList( dirs );

			if( backgroundWorker.CancellationPending )
			{
				return null;
			}

			var totFiles = 0;

			stopwatch.Restart();

			try
			{
				string modified_date;
				string created_date;

				// Parse each folder
				for( int d = 0; d < dirs.Count; d++ )
				{
					// Get folder properties
					var dirName = dirs[d];
					var currentDir = new SnappedFolder( Path.GetFileName( dirName ), Path.GetDirectoryName( dirName ) );
					if( dirName == Path.GetPathRoot( dirName ) )
					{
						currentDir = new SnappedFolder( "", dirName );
					}

					modified_date = "";
					created_date = "";
					try
					{
						modified_date = ToUnixTimestamp(System.IO.Directory.GetLastWriteTime( dirName ).ToLocalTime()).ToString();
						created_date = ToUnixTimestamp( System.IO.Directory.GetCreationTime( dirName ).ToLocalTime() ).ToString();
					}
					catch( Exception ex )
					{
						Console.WriteLine( "{0} Exception caught.", ex );
					}
					currentDir.Properties.Add( "Modified", modified_date );
					currentDir.Properties.Add( "Created", created_date );


					// Get files in folder
					List<string> files;
					try
					{
						files = new List<string>( System.IO.Directory.GetFiles( dirName, "*.*", System.IO.SearchOption.TopDirectoryOnly ) );
					}
					catch( Exception ex )
					{
						Console.WriteLine( "{0} Exception caught.", ex );
						result.Add( currentDir );
						continue;
					}
					files.Sort();

					// Get file properties
					foreach( string sFile in files )
					{
						totFiles++;
						if(stopwatch.ElapsedMilliseconds >= 50)
						{
							backgroundWorker.ReportProgress( 0, "Reading files... " + totFiles + " (" + sFile + ")" );
							stopwatch.Restart();
						}

						if( backgroundWorker.CancellationPending )
						{
							return null;
						}

						var currentFile = new SnappedFile( Path.GetFileName( sFile ) );
						try
						{
							System.IO.FileInfo fi = new System.IO.FileInfo( sFile );
							var isHidden = ( fi.Attributes & System.IO.FileAttributes.Hidden ) == System.IO.FileAttributes.Hidden;
							var isSystem = ( fi.Attributes & System.IO.FileAttributes.System ) == System.IO.FileAttributes.System;

							if( ( isHidden && skipHidden ) || ( isSystem && skipSystem ) )
							{
								continue;
							}

							currentFile.Properties.Add( "Size", fi.Length.ToString() );

							modified_date = "-";
							created_date = "-";
							try
							{
								modified_date = ToUnixTimestamp( fi.LastWriteTime.ToLocalTime() ).ToString();
								created_date = ToUnixTimestamp( fi.CreationTime.ToLocalTime() ).ToString();
							}
							catch( Exception ex )
							{
								Console.WriteLine( "{0} Exception caught.", ex );
							}

							currentFile.Properties.Add( "Modified", modified_date );
							currentFile.Properties.Add( "Created", created_date );

						}
						catch( Exception ex )
						{
							Console.WriteLine( "{0} Exception caught.", ex );
						}

						currentDir.Files.Add( currentFile );
					}

					result.Add( currentDir );
				}
			}
			catch( System.Exception ex )
			{
				Console.WriteLine( "{0} exception caught: {1}", ex, ex.Message );
			}

			return result;
		}

		private string BuildJavascriptContentArray(List<SnappedFolder> content, int startIndex)
		{
			//  Data format:
			//    Each index in "dirs" array is an array representing a directory:
			//      First item in array: "directory path*always 0*directory modified date"
			//        Note that forward slashes are used instead of (Windows style) backslashes
			//      Then, for each each file in the directory: "filename*size of file*file modified date"
			//      Second to last item in array tells the total size of directory content
			//      Last item in array refrences IDs to all subdirectories of this dir (if any).
			//        ID is the item index in dirs array.
			//    Note: Modified date is in UNIX format

			backgroundWorker.ReportProgress( 0, "Processing content..." );

			var result = new StringBuilder();

			var lineBreakSymbol = "";	// Could be set to \n to make the html output more readable, at the expense of increased size


			// Assign an ID to each folder. This is equal to the index in the JS data array
			var dirIndexes = new Dictionary<string, string>();
			for(var i=0; i<content.Count; i++)
			{
				dirIndexes.Add( content[i].FullPath, ( i + startIndex ).ToString() );
			}

			// Build a lookup table with subfolder IDs for each folder
			var subdirs = new Dictionary<string, List<string>>();
			foreach( var dir in content )
			{
				subdirs.Add( dir.FullPath, new List<string>() );
			}
			if( !subdirs.ContainsKey( content[0].Path ) && content[0].Name != "" )
			{
				subdirs.Add( content[0].Path, new List<string>() );
			}
			foreach( var dir in content )
			{
				if( dir.Name != "" )
				{
					try
					{
						subdirs[dir.Path].Add( dirIndexes[dir.FullPath] );
					}
					catch( Exception ex )
					{
						// orphan file or folder?
					}
				}
			}


			// Generate the data array

			foreach( var currentDir in content )
			{
				var sbCurrentDirArrays = new StringBuilder();
				sbCurrentDirArrays.Append( "D.p([" + lineBreakSymbol );

				var sDirWithForwardSlash = currentDir.FullPath.Replace( @"\", "/" );
				sbCurrentDirArrays.Append( "\"" ).Append( MakeCleanJsString( sDirWithForwardSlash ) ).Append( "*" ).Append( "0" ).Append( "*" ).Append( currentDir.GetProp("Modified") ).Append( "\"," + lineBreakSymbol );

				long dirSize = 0;

				foreach( var currentFile in currentDir.Files )
				{
					sbCurrentDirArrays.Append( "\"" ).Append( MakeCleanJsString( currentFile.Name ) ).Append( "*" ).Append( currentFile.GetProp( "Size" ) ).Append( "*" ).Append( currentFile.GetProp("Modified") ).Append( "\"," + lineBreakSymbol );
					try
					{
						dirSize += Int64.Parse( currentFile.GetProp("Size") );
					}
					catch( Exception ex)
					{
					}
				}

				// Add total dir size
				sbCurrentDirArrays.Append( "" ).Append( dirSize ).Append( "," + lineBreakSymbol );

				sbCurrentDirArrays.Append( "\"" ).Append( String.Join( "*", subdirs[currentDir.FullPath].ToArray() ) ).Append( "\"" + lineBreakSymbol );	// subdirs

				// Finalize
				sbCurrentDirArrays.Append( "])" );
				sbCurrentDirArrays.Append( "\n" );
				result.Append( sbCurrentDirArrays.ToString() );

				if( backgroundWorker.CancellationPending )
				{
					return null;
				}
			}

			return result.ToString();
		}

	}
}

