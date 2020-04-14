﻿using System;
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
		// This runs on a separate thread from the GUI
		private void backgroundWorker_DoWork( object sender, DoWorkEventArgs e )
		{
			var settings = (SnapSettings)e.Argument;

			// Get files & folders
			var content = GetContent( settings, backgroundWorker );
			if( backgroundWorker.CancellationPending )
			{
				backgroundWorker.ReportProgress( 0, "User cancelled" );
				return;
			}
			if( content == null )
			{
				backgroundWorker.ReportProgress( 0, "Error reading source" );
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
	
			// Convert to string with JS data object
			var jsContent = BuildJavascriptContentArray( content, 0, backgroundWorker );
			if( backgroundWorker.CancellationPending )
			{
				backgroundWorker.ReportProgress( 0, "User cancelled" );
				return;
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
			sbContent.Replace( "[TITLE]", settings.title );
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
				sbContent.Replace( "[LINK ROOT]", settings.linkRoot.Replace( @"\", "/" ) );
				sbContent.Replace( "[SOURCE ROOT]", settings.rootFolder.Replace( @"\", "/" ) );

				string link_root = settings.linkRoot.Replace( @"\", "/" );
				if( Utils.IsWildcardMatch( @"?:/*", link_root, false ) )  // "file://" is needed in the browser if path begins with drive letter, else it should not be used
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
				sbContent.Replace( "[SOURCE ROOT]", settings.rootFolder.Replace( @"\", "/" ) );
			}

			// Write output file
			try
			{
				using( System.IO.StreamWriter writer = new System.IO.StreamWriter( settings.outputFile ) )
				{
					writer.Write( sbContent.ToString() );
				}

				if( settings.openInBrowser )
				{
					System.Diagnostics.Process.Start( settings.outputFile );
				}
			}
			catch( Exception ex )
			{
				MessageBox.Show( "Failed to open file for writing:\n\n" + ex, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error );
				backgroundWorker.ReportProgress( 0, "An error occurred..." );
				return;
			}

			// Ready!
			Cursor.Current = Cursors.Default;
			backgroundWorker.ReportProgress( 100, "Ready!" );
		}

		private static List<SnappedFolder> GetContent( SnapSettings settings, BackgroundWorker bgWorker )
		{
			var stopwatch = new Stopwatch();
			stopwatch.Start();

			var result = new List<SnappedFolder>();

			// Get all folders
			var dirs = new List<string>();
			dirs.Insert( 0, settings.rootFolder );
			Utils.DirSearch( settings.rootFolder, dirs, settings.skipHiddenItems, settings.skipSystemItems, stopwatch, bgWorker );
			dirs = Utils.SortDirList( dirs );

			if( bgWorker.CancellationPending )
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
						modified_date = Utils.ToUnixTimestamp( System.IO.Directory.GetLastWriteTime( dirName ).ToLocalTime() ).ToString();
						created_date = Utils.ToUnixTimestamp( System.IO.Directory.GetCreationTime( dirName ).ToLocalTime() ).ToString();
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
							bgWorker.ReportProgress( 0, "Reading files... " + totFiles + " (" + sFile + ")" );
							stopwatch.Restart();
						}

						if( bgWorker.CancellationPending )
						{
							return null;
						}

						var currentFile = new SnappedFile( Path.GetFileName( sFile ) );
						try
						{
							System.IO.FileInfo fi = new System.IO.FileInfo( sFile );
							var isHidden = ( fi.Attributes & System.IO.FileAttributes.Hidden ) == System.IO.FileAttributes.Hidden;
							var isSystem = ( fi.Attributes & System.IO.FileAttributes.System ) == System.IO.FileAttributes.System;

							if( ( isHidden && settings.skipHiddenItems ) || ( isSystem && settings.skipSystemItems ) )
							{
								continue;
							}

							currentFile.Properties.Add( "Size", fi.Length.ToString() );

							modified_date = "-";
							created_date = "-";
							try
							{
								modified_date = Utils.ToUnixTimestamp( fi.LastWriteTime.ToLocalTime() ).ToString();
								created_date = Utils.ToUnixTimestamp( fi.CreationTime.ToLocalTime() ).ToString();
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

		private static string BuildJavascriptContentArray(List<SnappedFolder> content, int startIndex, BackgroundWorker bgWorker)
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

			bgWorker.ReportProgress( 0, "Processing content..." );

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
				sbCurrentDirArrays.Append( "\"" ).Append( Utils.MakeCleanJsString( sDirWithForwardSlash ) ).Append( "*" ).Append( "0" ).Append( "*" ).Append( currentDir.GetProp( "Modified" ) ).Append( "\"," + lineBreakSymbol );

				long dirSize = 0;

				foreach( var currentFile in currentDir.Files )
				{
					sbCurrentDirArrays.Append( "\"" ).Append( Utils.MakeCleanJsString( currentFile.Name ) ).Append( "*" ).Append( currentFile.GetProp( "Size" ) ).Append( "*" ).Append( currentFile.GetProp( "Modified" ) ).Append( "\"," + lineBreakSymbol );
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

				if( bgWorker.CancellationPending )
				{
					return null;
				}
			}

			return result.ToString();
		}

	}
}
