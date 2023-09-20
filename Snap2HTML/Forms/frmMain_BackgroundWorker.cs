using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Text;
using System.Windows.Forms;
using System.IO;
using System.Diagnostics;

namespace Snap2HTML
{
	public partial class frmMain : Form
	{
		// This runs on a separate thread from the GUI
		private void backgroundWorker_DoWork( object sender, DoWorkEventArgs e )
		{
			var settings = (Model.SnapSettings)e.Argument;

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
					totSize += Utils.ParseLong( file.GetProp( "Size" ) );
				}
			}
	
			// Let's generate the output

			backgroundWorker.ReportProgress( 0, "Generating HTML file..." );

			// Read template
			var sbTemplate = new StringBuilder();
			try
			{
				using( System.IO.StreamReader reader = new System.IO.StreamReader( System.IO.Path.GetDirectoryName( Application.ExecutablePath ) + System.IO.Path.DirectorySeparatorChar + "template.html", Encoding.UTF8 ) )
				{
					sbTemplate.Append(reader.ReadToEnd());
				}
			}
			catch( System.Exception ex )
			{
				MessageBox.Show( "Failed to open 'Template.html' for reading:\n\n" + ex.Message, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error );
				backgroundWorker.ReportProgress( 0, "An error occurred..." );
				return;
			}

			// Build HTML
			sbTemplate.Replace( "[TITLE]", settings.title );
			sbTemplate.Replace( "[APP LINK]", "https://github.com/laim/Snap2HTML-NG");
			sbTemplate.Replace( "[APP NAME]", Application.ProductName );
			sbTemplate.Replace( "[APP VER]", Application.ProductVersion.Split( '.' )[0] + "." + Application.ProductVersion.Split( '.' )[1] );
			sbTemplate.Replace( "[GEN TIME]", DateTime.Now.ToString( "t" ) );
			sbTemplate.Replace( "[GEN DATE]", DateTime.Now.ToString( "d" ) );
			sbTemplate.Replace( "[NUM FILES]", totFiles.ToString() );
			sbTemplate.Replace( "[NUM DIRS]", totDirs.ToString() );
			sbTemplate.Replace( "[TOT SIZE]", totSize.ToString() );
			sbTemplate.Replace("[SEARCH_PATTERN]", settings.searchPattern);

			if( settings.linkFiles )
			{
				sbTemplate.Replace( "[LINK FILES]", "true" );
				sbTemplate.Replace( "[LINK ROOT]", settings.linkRoot.Replace( @"\", "/" ) );
				sbTemplate.Replace( "[SOURCE ROOT]", settings.rootFolder.Replace( @"\", "/" ) );

				string link_root = settings.linkRoot.Replace( @"\", "/" );
				if( Utils.IsWildcardMatch( @"?:/*", link_root, false ) )  // "file://" is needed in the browser if path begins with drive letter, else it should not be used
				{
					sbTemplate.Replace( "[LINK PROTOCOL]", @"file://" );
				}
				else if( link_root.StartsWith( "//" ) )  // for UNC paths e.g. \\server\path
				{
					sbTemplate.Replace( "[LINK PROTOCOL]", @"file://///" );
				}
				else
				{
					sbTemplate.Replace( "[LINK PROTOCOL]", "" );
				}

			}
			else
			{
				sbTemplate.Replace( "[LINK FILES]", "false" );
				sbTemplate.Replace( "[LINK PROTOCOL]", "" );
				sbTemplate.Replace( "[LINK ROOT]", "" );
				sbTemplate.Replace( "[SOURCE ROOT]", settings.rootFolder.Replace( @"\", "/" ) );
			}

			// Write output file
			try
			{
				using( System.IO.StreamWriter writer = new System.IO.StreamWriter( settings.outputFile, false, Encoding.UTF8 ) )
				{
					writer.AutoFlush = true;

					var template = sbTemplate.ToString();
					var startOfData = template.IndexOf( "[DIR DATA]" );

					writer.Write(template.Substring(0, startOfData));

					BuildJavascriptContentArray( content, 0, writer, backgroundWorker );

					if( backgroundWorker.CancellationPending )
					{
						backgroundWorker.ReportProgress( 0, "User cancelled" );
						return;
					}

					writer.Write( template.Substring( startOfData + 10) );
				}

				sbTemplate = null;

				if( settings.openInBrowser )
				{
					System.Diagnostics.Process.Start( settings.outputFile );
				}
			}
			catch( Exception ex )
			{
				MessageBox.Show( "Failed to open file for writing:\n\n" + ex.Message, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error );
				backgroundWorker.ReportProgress( 0, "An error occurred..." );
				return;
			}

			// Ready!
			Cursor.Current = Cursors.Default;
			backgroundWorker.ReportProgress( 100, "Ready!" );
		}


		// --- Helper functions (must be static to avoid thread problems) ---

		private static List<Model.SnappedFolder> GetContent(Model.SnapSettings settings, BackgroundWorker bgWorker )
		{
			var stopwatch = new Stopwatch();
			stopwatch.Start();

			var result = new List<Model.SnappedFolder>();

			// Get all folders
			var dirs = new List<string>();
			dirs.Insert( 0, settings.rootFolder );
			DirSearch( settings.rootFolder, dirs, settings.skipHiddenItems, settings.skipSystemItems, stopwatch, bgWorker );
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
					var currentDir = new Model.SnappedFolder( Path.GetFileName( dirName ), Path.GetDirectoryName( dirName ) );
					if( dirName == Path.GetPathRoot( dirName ) )
					{
						currentDir = new Model.SnappedFolder( "", dirName );
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
						files = new List<string>(Directory.GetFiles(dirName, settings.searchPattern, SearchOption.TopDirectoryOnly));
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

						var currentFile = new Model.SnappedFile( Path.GetFileName( sFile ) );
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

		// Recursive function to get all folders and subfolders of given path path
		private static void DirSearch( string sDir, List<string> lstDirs, bool skipHidden, bool skipSystem, Stopwatch stopwatch, BackgroundWorker backgroundWorker )
		{
			if( backgroundWorker.CancellationPending ) return;

			try
			{
				foreach( string d in System.IO.Directory.GetDirectories( sDir ) )
				{
					bool includeThisFolder = true;

					//if( d.ToUpper().EndsWith( "SYSTEM VOLUME INFORMATION" ) ) includeThisFolder = false;

					// exclude folders that have the system or hidden attr set (if required)
					if( skipHidden || skipSystem )
					{
						var attr = new DirectoryInfo( d ).Attributes;

						if( skipHidden )
						{
							if( ( attr & FileAttributes.Hidden ) == FileAttributes.Hidden )
							{
								includeThisFolder = false;
							}
						}

						if( skipSystem )
						{
							if( ( attr & FileAttributes.System ) == FileAttributes.System )
							{
								includeThisFolder = false;
							}
						}
					}


					if( includeThisFolder )
					{
						lstDirs.Add( d );

						if( stopwatch.ElapsedMilliseconds >= 50 )
						{
							backgroundWorker.ReportProgress( 0, "Getting folders... " + lstDirs.Count + " (" + d + ")" );
							stopwatch.Restart();
						}

						DirSearch( d, lstDirs, skipHidden, skipSystem, stopwatch, backgroundWorker );
					}
				}
			}
			catch( System.Exception ex )
			{
				Console.WriteLine( "ERROR in DirSearch():" + ex.Message );
			}
		}

		private static void BuildJavascriptContentArray( List<Model.SnappedFolder> content, int startIndex, StreamWriter writer, BackgroundWorker bgWorker )
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

			var lineBreakSymbol = "";	// Could be set to \n to make the html output more readable, at the expense of increased size

			// Assign an ID to each folder. This is equal to the index in the JS data array
			var dirIndexes = new Dictionary<string, string>();
			for( var i = 0; i < content.Count; i++ )
			{
				dirIndexes.Add( content[i].GetFullPath(), ( i + startIndex ).ToString() );
			}

			// Build a lookup table with subfolder IDs for each folder
			var subdirs = new Dictionary<string, List<string>>();
			foreach( var dir in content )
			{
				// add all folders as keys
				subdirs.Add( dir.GetFullPath(), new List<string>() );
			}
			if( !subdirs.ContainsKey( content[0].Path ) && content[0].Name != "" )
			{
				// ensure that root folder is not missed missed
				subdirs.Add( content[0].Path, new List<string>() );
			}
			foreach( var dir in content )
			{
				if( dir.Name != "" )
				{
					try
					{
						// for each folder, add its index to its parent folder list of subdirs
						subdirs[dir.Path].Add( dirIndexes[dir.GetFullPath()] );
					}
					catch( Exception ex )
					{
						// orphan file or folder?
					}
				}
			}

			// Generate the data array
			var result = new StringBuilder();
			foreach( var currentDir in content )
			{
				result.Append( "D.p([" + lineBreakSymbol );

				var sDirWithForwardSlash = currentDir.GetFullPath().Replace( @"\", "/" );
				result.Append( "\"" ).Append( Utils.MakeCleanJsString( sDirWithForwardSlash ) ).Append( "*" ).Append( "0" ).Append( "*" ).Append( currentDir.GetProp( "Modified" ) ).Append( "\"," + lineBreakSymbol );

				long dirSize = 0;

				foreach( var currentFile in currentDir.Files )
				{
					result.Append( "\"" ).Append( Utils.MakeCleanJsString( currentFile.Name ) ).Append( "*" ).Append( currentFile.GetProp( "Size" ) ).Append( "*" ).Append( currentFile.GetProp( "Modified" ) ).Append( "\"," + lineBreakSymbol );
					dirSize += Utils.ParseLong( currentFile.GetProp( "Size" ) );
				}

				// Add total dir size
				result.Append( "" ).Append( dirSize ).Append( "," + lineBreakSymbol );

				// Add reference to subdirs
				result.Append( "\"" ).Append( String.Join( "*", subdirs[currentDir.GetFullPath()].ToArray() ) ).Append( "\"" + lineBreakSymbol );

				// Finalize
				result.Append( "])" );
				result.Append( "\n" );

				// Write result in chunks to limit memory consumtion
				if( result.Length > 10240 )
				{
					writer.Write( result.ToString() );
					result.Clear();
				}

				if( bgWorker.CancellationPending )
				{
					return;
				}
			}

			writer.Write( result.ToString() );

			return;
		}

	}
}

