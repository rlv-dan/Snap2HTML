using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Web;
using System.Windows.Forms;

namespace Snap2HTML
{
	public partial class frmMain : Form
	{
		// This runs on a separate thread from the GUI
		private void backgroundWorker_DoWork( object sender, DoWorkEventArgs args )
		{
			var settings = (SnapSettings)args.Argument;

            // Read template file
            var sbTemplate = new StringBuilder();
            try
            {
                using (System.IO.StreamReader reader = new System.IO.StreamReader(Utils.GetTemplatePath(), Encoding.UTF8))
                {
                    sbTemplate.Append(reader.ReadToEnd());
                }
            }
            catch (System.Exception ex)
            {
                MessageBox.Show("Failed to open 'Template.html' for reading:\n\n" + ex.Message, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                backgroundWorker.ReportProgress(0, "An error occurred...");
                return;
            }

            // Open output file for writing (do it first to ensure we can write to it)
            System.IO.StreamWriter writer;
            try
            {
                writer = new System.IO.StreamWriter(settings.outputFile, false, Encoding.UTF8);
            }
            catch (Exception ex)
            {
                MessageBox.Show("Failed to open output file for writing:\n\n" + ex.Message, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                backgroundWorker.ReportProgress(0, "An error occurred...");
                return;
            }

            // Get files & folders
            var content = GetContent( settings, backgroundWorker );
			if( content == null )
			{
				backgroundWorker.ReportProgress( 0, "User cancelled" );
				return;
			}

            backgroundWorker.ReportProgress(0, "Sorting...");
            var sortedContent = content.OrderByNatural(d => d.FullPath).ToList();

            long totFiles = sortedContent.Select(d => d.Files.Count).Sum();
            long totSize = sortedContent.Select(d => d.Size).Sum();

            // Metadata
            var rootDirMetaDataObj = new JsMetadata()
            {
                title = settings.title,
                timestamp = Utils.ToUnixTimestamp(DateTime.Now),
                sourceDir = settings.rootFolder,
                linkRoot = settings.linkFiles ? settings.linkRoot : "",
                numFiles = totFiles,
                numDirs = sortedContent.Count,
                totBytes = totSize,
            };
            var rootDirMetaData = Utils.ToJSON(rootDirMetaDataObj);


            // Let's generate the output!

            backgroundWorker.ReportProgress( 0, "Generating HTML file..." );

			// Build HTML
			sbTemplate.Replace( "[PAGE TITLE]", HttpUtility.HtmlEncode(settings.title));
			sbTemplate.Replace( "[PAGE TITLE JS]", HttpUtility.JavaScriptStringEncode(settings.title) );
			sbTemplate.Replace( "[BODY TITLE]", HttpUtility.HtmlEncode(settings.title).Replace(@"\", @"\<wbr>") );   // <wbr> = Word Break Opportunity
            sbTemplate.Replace( "[APP LINK]", "https://www.rlvision.com" );
			sbTemplate.Replace( "[APP NAME]", Application.ProductName );
			sbTemplate.Replace( "[APP VER]", Application.ProductVersion.Split( '.' )[0] + "." + Application.ProductVersion.Split( '.' )[1] );
			sbTemplate.Replace("[DATA VER]", "2");
			sbTemplate.Replace( "[GEN TIME]", DateTime.Now.ToString( "t" ) );
			sbTemplate.Replace( "[GEN DATE]", DateTime.Now.ToString( "d" ) );
			sbTemplate.Replace( "[GEN TIMESTAMP]", Utils.ToUnixTimestamp(DateTime.Now).ToString() );
			sbTemplate.Replace( "[NUM FILES]", totFiles.ToString() );
			sbTemplate.Replace( "[NUM DIRS]", sortedContent.Count.ToString() );
			sbTemplate.Replace( "[TOT SIZE]", Utils.BytesToFilesize(totSize) );
			sbTemplate.Replace( "[TOT BYTES]", totSize.ToString() );

            try
            {
                writer.AutoFlush = true;

                var template = sbTemplate.ToString();
                var startOfDataMarker = template.IndexOf("[DIR DATA]");
                var endOfDataMarker = startOfDataMarker + 10;

                writer.Write(template.Substring(0, startOfDataMarker));

                WriteJavascriptContentArray(settings.rootFolder, rootDirMetaData, sortedContent, writer, backgroundWorker);

                if (backgroundWorker.CancellationPending)
                {
                    backgroundWorker.ReportProgress(0, "User cancelled");
                    return;
                }

                writer.Write(template.Substring(endOfDataMarker));
            }
            catch (Exception ex)
            {
                MessageBox.Show("An error occurred when generating contents:\n\n" + ex.Message, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                backgroundWorker.ReportProgress(0, "An error occurred...");
                return;
            }

			sbTemplate = null;
            writer.Dispose();

            if (settings.openInBrowser)
            {
                Process.Start(settings.outputFile);
            }

            // Report errors back to main thread
            var errors = sortedContent.Where(d => d.Error != null).ToList();
			args.Result = errors;

            // Ready!
            Cursor.Current = Cursors.Default;
			backgroundWorker.ReportProgress( 100, "Ready!" );
		}


		// --- Helper functions (must be static to avoid thread problems) ---

        private static List<SnappedFolder> GetContent(SnapSettings settings, BackgroundWorker bgWorker)
        {
            var stopwatch = new Stopwatch();
            stopwatch.Start();

            var result = new Dictionary<string, SnappedFolder>();
            var queue = new Queue<DirectoryInfo>();

			string rootFolder = settings.rootFolder;

            if (!rootFolder.StartsWith(@"\\"))
            {
                // extended-length path prefix \\?\ = support paths longer than MAX_LENGTH
                // UNC prefix \\servername = network path
                // Long UNC(?) \\?\UNC\
                rootFolder = @"\\?\" + rootFolder;
            }

            queue.Enqueue(new DirectoryInfo(rootFolder));

            var rootFolderLength = new SnappedFolder(queue.Peek()).FullPath.Length;

            do
            {
                var next = queue.Dequeue();

                var currentDir = new SnappedFolder(next);

                try
                {
                    var items = next.GetFileSystemInfos("*", SearchOption.TopDirectoryOnly);
                    var sortedItems = items.OrderByNatural(item => item.Name);

                    foreach (var item in sortedItems)
                    {
                        var includeItem = true;
                        if (settings.skipHiddenItems && (item.Attributes & FileAttributes.Hidden) == FileAttributes.Hidden) includeItem = false;
                        if (settings.skipSystemItems && (item.Attributes & FileAttributes.System) == FileAttributes.System) includeItem = false;

                        if (includeItem)
                        {
                            if (item is DirectoryInfo)
                            {
                                queue.Enqueue(item as DirectoryInfo);
                            }
                            else if (item is FileInfo)
                            {
                                currentDir.Files.Add(new SnappedFile(item as FileInfo));
                            }
                        }
                    }

					currentDir.Size = currentDir.DeepSize = currentDir.Files.Sum(f => f.Size);
                }
                catch (UnauthorizedAccessException ex)
                {
                    currentDir.Error = new SnapError()
                    {
                        ErrorMessage = ex.Message,
                        UnauthorizedAccessException = true,
                        ReparsePoint = (next.Attributes & FileAttributes.ReparsePoint) == FileAttributes.ReparsePoint
                    };
                }
                catch (Exception ex)
                {
                    currentDir.Error = new SnapError()
                    {
                        ErrorMessage = ex.Message,
                    };
                }

                result.Add(currentDir.FullPath, currentDir);

                // For each parent folder, add the size of this folder
                var subPath = Path.GetDirectoryName(currentDir.FullPath.TrimEnd('\\'));
                while (subPath != null && subPath.Length >= rootFolderLength)
                {
					result[subPath].DeepSize += currentDir.Size;
                    subPath = Path.GetDirectoryName(subPath);
                }

                if (bgWorker.CancellationPending)
				{
                    return null;
				}

                if (stopwatch.ElapsedMilliseconds >= 100)
                {
                    bgWorker.ReportProgress(0, next.FullName.Substring(4)); // Substring(4) removes the long path indicator \\?\
                    stopwatch.Restart();
                }

            } while (queue.Count > 0);



            return result.Select(kvp => kvp.Value).ToList();
        }

        private static void WriteJavascriptContentArray(string rootFolder, string rootMetadata, List<SnappedFolder> content, StreamWriter writer, BackgroundWorker bgWorker )
		{
            // Data version: 2
            // For an explanation of the data format, see Developer.txt file

            // Assign an ID to each folder. This is equal to the index in the JS data array. Setting indexOffset > 0 allows having root folders.
            var dirIndexes = new Dictionary<string, long>();
            for ( var i = 0; i < content.Count; i++ )
			{
				dirIndexes.Add( content[i].FullPath, i );
			}

			// Build a lookup table with all folders and its subfolder's IDs
			var subdirs = new Dictionary<string, List<long>>();
			foreach( var dir in content )
			{
				// Add all folders as keys
				subdirs.Add( dir.FullPath, new List<long>() );
			}
			foreach( var dir in content )
            {
				try
				{
					// For each folder, add its index to its parent folder list of subdirs
					if(dir.Path != null)
					{
						subdirs[dir.Path].Add(dirIndexes[dir.FullPath]);
					}
				}
				catch( Exception ex )
				{
					// Orphan file or folder?
				}
			}
            
            // Generate the data array
            var result = new StringBuilder();
            foreach( var dir in content )
            {
                result.Append( "p([" );

                var folderSize = dir.Error == null ? dir.DeepSize : -1;

                // Add the folder: name*size*modified
                result.Append( "\"" )
                      .Append(HttpUtility.JavaScriptStringEncode(dir.Name))
                      .Append( "*" )
                      .Append( Utils.DecimalToArbitrarySystem(folderSize, 36) )
                      .Append( "*" )
                      .Append(Utils.DecimalToArbitrarySystem(dir.Modified, 36) )
                      .Append( "\"," );

                // Add reference to parent folder
                var parent = dir.FullPath != rootFolder ? dirIndexes[dir.Path] : -1;
                result.Append(parent)
                      .Append(",");

                // Add references to all subdirs
                result.Append( "\"" )
                      .Append( String.Join( "*", subdirs[dir.FullPath].ToArray() ) )
                      .Append( "\"" );

                //Add each file: name*size*modified
                foreach (var file in dir.Files)
                {
                    result.Append(",\"")
                          .Append(HttpUtility.JavaScriptStringEncode(file.Name))
                          .Append("*")
                          .Append(Utils.DecimalToArbitrarySystem(file.Size, 36))
                          .Append("*")
                          .Append(Utils.DecimalToArbitrarySystem(file.Modified, 36))
                          .Append("\"");
                }

                // Special case for rootfolders: Add the metadata as the last item
                if(dir.FullPath == rootFolder)
                {
                    result.Append(",")
                          .Append(rootMetadata);
                }

                // Close array
                result.Append( "])\n" );

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

