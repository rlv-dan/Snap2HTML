using Snap2HTMLNG.Shared.Models;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Text;
using System;
using Snap2HTMLNG.Shared.Utils.Legacy;
using Snap2HTMLNG.Shared.Settings;
using System.Diagnostics;
using static System.Net.Mime.MediaTypeNames;
using System.Windows.Forms;
using System.Drawing;

namespace Snap2HTMLNG.Shared.Builder
{
    public class DataBuilder
    {
        /// <summary>
        /// Builds the data array for the template
        /// </summary>
        /// <param name="content">List of Directories per <see cref="Model.SnappedFolder"/>></param>
        /// <param name="startIndex">Integer</param>
        /// <param name="writer">StreamWriter</param>
        /// <param name="bgWorker">[Optional] BackgroundWorker Instance, only used by the GUI.</param>
        public static void BuildJavascriptContentArray(List<SnappedFolder> content, int startIndex, StreamWriter writer, BackgroundWorker bgWorker = null)
        {
            // Prevents background worker calls if we're using the Command Line
            bool commandLine = false;
            if(bgWorker == null)
            {
                commandLine = true;
            }

            //  Data format:
            //    Each index in "dirs" array is an array representing a directory:
            //      First item in array: "directory path*always 0*directory modified date"
            //        Note that forward slashes are used instead of (Windows style) backslashes
            //      Then, for each each file in the directory: "filename*size of file*file modified date"
            //      Second to last item in array tells the total size of directory content
            //      Last item in array refrences IDs to all subdirectories of this dir (if any).
            //        ID is the item index in dirs array.
            //    Note: Modified date is in UNIX format

            var lineBreakSymbol = "";   // Could be set to \n to make the html output more readable, at the expense of increased size

            // Assign an ID to each folder. This is equal to the index in the JS data array
            var dirIndexes = new Dictionary<string, string>();
            for (var i = 0; i < content.Count; i++)
            {
                dirIndexes.Add(content[i].GetFullPath(), (i + startIndex).ToString());
            }

            // Build a lookup table with subfolder IDs for each folder
            var subdirs = new Dictionary<string, List<string>>();
            foreach (var dir in content)
            {
                // add all folders as keys
                subdirs.Add(dir.GetFullPath(), new List<string>());
            }
            if (!subdirs.ContainsKey(content[0].Path) && content[0].Name != "")
            {
                // ensure that root folder is not missed missed
                subdirs.Add(content[0].Path, new List<string>());
            }
            foreach (var dir in content)
            {
                if (dir.Name != "")
                {
                    try
                    {
                        // for each folder, add its index to its parent folder list of subdirs
                        subdirs[dir.Path].Add(dirIndexes[dir.GetFullPath()]);
                    }
                    catch (Exception ex)
                    {
                        // orphan file or folder?
                    }
                }
            }

            // Generate the data array
            var result = new StringBuilder();
            foreach (var currentDir in content)
            {
                result.Append("D.p([" + lineBreakSymbol);

                var sDirWithForwardSlash = currentDir.GetFullPath().Replace(@"\", "/");
                result.Append("\"").Append(Helpers.MakeCleanJsString(sDirWithForwardSlash)).Append("*").Append("0").Append("*").Append(currentDir.GetProp("Modified")).Append("\"," + lineBreakSymbol);

                long dirSize = 0;

                foreach (var currentFile in currentDir.Files)
                {
                    result.Append("\"").Append(Helpers.MakeCleanJsString(currentFile.Name)).Append("*").Append(currentFile.GetProp("Size")).Append("*").Append(currentFile.GetProp("Modified")).Append("\"," + lineBreakSymbol);
                    dirSize += Helpers.ParseLong(currentFile.GetProp("Size"));
                }

                // Add total dir size
                result.Append("").Append(dirSize).Append("," + lineBreakSymbol);

                // Add reference to subdirs
                result.Append("\"").Append(String.Join("*", subdirs[currentDir.GetFullPath()].ToArray())).Append("\"" + lineBreakSymbol);

                // Finalize
                result.Append("])");
                result.Append("\n");

                // Write result in chunks to limit memory consumtion
                if (result.Length > 10240)
                {
                    writer.Write(result.ToString());
                    result.Clear();
                }

                // If we're using the GUI, account for BGworker cancellation
                if(!commandLine)
                {
                    if (bgWorker.CancellationPending)
                    {
                        return;
                    }
                }
            }

            writer.Write(result.ToString());

            return;
        }

        /// <summary>
        /// Helper functions used by the Background Worker.  Must be STATIC to avoid Thread problems
        /// </summary>
        /// <param name="settings">Application Settings</param>
        /// <param name="bgWorker">[Optional] BackgroundWorker Instance, only used by the GUI.</param>
        /// <returns></returns>
        public static List<SnappedFolder> GetContent(BackgroundWorker bgWorker = null)
        {
            // Prevents background worker calls if we're using the Command Line
            bool commandLine = false;
            if (bgWorker == null)
            {
                commandLine = true;
            }

            var stopwatch = new Stopwatch();
            stopwatch.Start();

            var result = new List<SnappedFolder>();

            // Get all folders
            var dirs = new List<string>();
            dirs.Insert(0, XmlConfigurator.Read("RootFolder"));
            DirSearch(XmlConfigurator.Read("RootFolder"), dirs, bool.Parse(XmlConfigurator.Read("SkipHiddenItems")), bool.Parse(XmlConfigurator.Read("SkipSystemItems")), stopwatch, bgWorker);
            dirs = Helpers.SortDirList(dirs);

            // If we're using the GUI, account for BGworker cancellation
            if (!commandLine)
            {
                if (bgWorker.CancellationPending)
                {
                    return null;
                }
            }

            var totFiles = 0;

            stopwatch.Restart();

            try
            {
                string modified_date;
                string created_date;

                // Parse each folder
                for (int d = 0; d < dirs.Count; d++)
                {
                    // Get folder properties
                    var dirName = dirs[d];

                    var currentDir = new SnappedFolder(Path.GetFileName(dirName), Path.GetDirectoryName(dirName));
                    if (dirName == Path.GetPathRoot(dirName))
                    {
                        currentDir = new SnappedFolder("", dirName);
                    }

                    modified_date = "";
                    created_date = "";
                    try
                    {
                        modified_date = Helpers.ToUnixTimestamp(Directory.GetLastWriteTime(dirName).ToLocalTime()).ToString();
                        created_date = Helpers.ToUnixTimestamp(Directory.GetCreationTime(dirName).ToLocalTime()).ToString();
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine("{0} Exception caught.", ex);
                    }
                    currentDir.Properties.Add("Modified", modified_date);
                    currentDir.Properties.Add("Created", created_date);

                    // Get files in folder
                    List<string> files;
                    try
                    {
                        files = new List<string>(Directory.GetFiles(dirName, XmlConfigurator.Read("SearchPattern"), SearchOption.TopDirectoryOnly));
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine("{0} Exception caught.", ex);
                        result.Add(currentDir);
                        continue;
                    }
                    files.Sort();

                    // Get file properties
                    foreach (string sFile in files)
                    {
                        totFiles++;
                        if (stopwatch.ElapsedMilliseconds >= 50)
                        {
                            // if we're using the gui, report progress
                            if(!commandLine)
                            {
                                bgWorker.ReportProgress(0, "Reading files... " + totFiles + " (" + sFile + ")");
                            }
                            stopwatch.Restart();
                        }

                        if(!commandLine)
                        {
                            if (bgWorker.CancellationPending)
                            {
                                return null;
                            }
                        }

                        var currentFile = new SnappedFile(Path.GetFileName(sFile));
                        try
                        {
                            System.IO.FileInfo fi = new System.IO.FileInfo(sFile);
                            var isHidden = (fi.Attributes & System.IO.FileAttributes.Hidden) == System.IO.FileAttributes.Hidden;
                            var isSystem = (fi.Attributes & System.IO.FileAttributes.System) == System.IO.FileAttributes.System;

                            if (isHidden && bool.Parse(XmlConfigurator.Read("SkipHiddenItems")) || (isSystem && bool.Parse(XmlConfigurator.Read("SkipSystemItems"))))
                            {
                                continue;
                            }

                            currentFile.Properties.Add("Size", fi.Length.ToString());

                            modified_date = "-";
                            created_date = "-";
                            try
                            {
                                modified_date = Helpers.ToUnixTimestamp(fi.LastWriteTime.ToLocalTime()).ToString();
                                created_date = Helpers.ToUnixTimestamp(fi.CreationTime.ToLocalTime()).ToString();
                            }
                            catch (Exception ex)
                            {
                                Console.WriteLine("{0} Exception caught.", ex);
                            }

                            currentFile.Properties.Add("Modified", modified_date);
                            currentFile.Properties.Add("Created", created_date);

                        }
                        catch (Exception ex)
                        {
                            Console.WriteLine("{0} Exception caught.", ex);
                        }

                        currentDir.Files.Add(currentFile);
                    }

                    result.Add(currentDir);
                }
            }
            catch (System.Exception ex)
            {
                Console.WriteLine("{0} exception caught: {1}", ex, ex.Message);
            }

            return result;
        }

        /// <summary>
        /// Recursive Function to get all directories and sub directories of given path
        /// </summary>
        /// <param name="sDir">Master Directory</param>
        /// <param name="lstDirs"></param>
        /// <param name="skipHidden">true or false</param>
        /// <param name="skipSystem">true or false</param>
        /// <param name="stopwatch">Timer</param>
        /// <param name="bgWorker">[Optional] BackgroundWorker Instance, only used by the GUI.</param>
        public static void DirSearch(string sDir, List<string> lstDirs, bool skipHidden, bool skipSystem, Stopwatch stopwatch, BackgroundWorker bgWorker = null)
        {
            // Prevents background worker calls if we're using the Command Line
            bool commandLine = false;
            if (bgWorker == null)
            {
                commandLine = true;
            }

            // If we're using the GUI, account for bgWorker cancellation
            if (!commandLine && bgWorker.CancellationPending)
                return;

            Console.WriteLine(sDir);

            try
            {
                foreach (string d in System.IO.Directory.GetDirectories(sDir))
                { 
                    bool includeThisFolder = true;

                    //if( d.ToUpper().EndsWith( "SYSTEM VOLUME INFORMATION" ) ) includeThisFolder = false;

                    // exclude folders that have the system or hidden attr set (if required)
                    if (skipHidden || skipSystem)
                    {
                        var attr = new DirectoryInfo(d).Attributes;

                        if (skipHidden)
                        {
                            if ((attr & FileAttributes.Hidden) == FileAttributes.Hidden)
                            {
                                includeThisFolder = false;
                            }
                        }

                        if (skipSystem)
                        {
                            if ((attr & FileAttributes.System) == FileAttributes.System)
                            {
                                includeThisFolder = false;
                            }
                        }
                    }


                    if (includeThisFolder)
                    {
                        lstDirs.Add(d);

                        if (stopwatch.ElapsedMilliseconds >= 50)
                        {
                            if(!commandLine)
                            {
                                bgWorker.ReportProgress(0, $"Getting directories {lstDirs.Count} ({d})");
                            } else
                            {
                                Console.WriteLine($"Getting directories {lstDirs.Count} ({d})");
                            }
                            stopwatch.Restart();
                        }

                        DirSearch(d, lstDirs, skipHidden, skipSystem, stopwatch, bgWorker);
                    }
                }
            }
            catch (System.Exception ex)
            {
                Console.WriteLine("ERROR in DirSearch(): " + ex.Message);
            }
        }

        /// <summary>
        /// Builds the start of the HTML file by replacing the placeholders with their respsective values.
        /// </summary>
        /// <param name="template">
        /// template.html with replace values 
        /// </param>
        /// <param name="productName">Application Name</param>
        /// <param name="version">Application Version</param>
        /// <param name="totalFiles">Integer of Total Files found</param>
        /// <param name="totalDirectories">Integer of Total Directories found</param>
        /// <param name="totalSize">Long of total size of all files</param>
        /// <returns></returns>
        public static StringBuilder BuildHtml(StringBuilder template, string productName, string version, int totalFiles, int totalDirectories, long totalSize)
        {
            // Build HTML
            template.Replace("[TITLE]", XmlConfigurator.Read("Title"));
            template.Replace("[APP LINK]", "https://github.com/laim/Snap2HTML-NG");
            template.Replace("[APP NAME]", productName);
            template.Replace("[APP VER]", version.Split('.')[0] + "." + version.Split('.')[1]);
            template.Replace("[GEN TIME]", DateTime.Now.ToString("t"));
            template.Replace("[GEN DATE]", DateTime.Now.ToString("d"));
            template.Replace("[NUM FILES]", totalFiles.ToString());
            template.Replace("[NUM DIRS]", totalDirectories.ToString());
            template.Replace("[TOT SIZE]", totalSize.ToString());
            template.Replace("[SEARCH_PATTERN]", XmlConfigurator.Read("SearchPattern"));

            if (bool.Parse(XmlConfigurator.Read("LinkFiles")))
            {
                template.Replace("[LINK FILES]", "true");
                template.Replace("[LINK ROOT]", XmlConfigurator.Read("LinkRoot").Replace(@"\", "/"));
                template.Replace("[SOURCE ROOT]", XmlConfigurator.Read("RootFolder").Replace(@"\", "/"));

                string link_root = XmlConfigurator.Read("LinkRoot").Replace(@"\", "/");
                if (Helpers.IsWildcardMatch(@"?:/*", link_root, false))  // "file://" is needed in the browser if path begins with drive letter, else it should not be used
                {
                    template.Replace("[LINK PROTOCOL]", @"file://");
                }
                else if (link_root.StartsWith("//"))  // for UNC paths e.g. \\server\path
                {
                    template.Replace("[LINK PROTOCOL]", @"file://///");
                }
                else
                {
                    template.Replace("[LINK PROTOCOL]", "");
                }

            }
            else
            {
                template.Replace("[LINK FILES]", "false");
                template.Replace("[LINK PROTOCOL]", "");
                template.Replace("[LINK ROOT]", "");
                template.Replace("[SOURCE ROOT]", XmlConfigurator.Read("RootFolder").Replace(@"\", "/"));
            }

            return template;
        }
    
        public static void Build(string applicationName, string applicationVersion, BackgroundWorker bgWorker = null)
        {
            // Get the directories and files
            // TODO: Does this need switched to only include bgWorker if its passed?
            // might need to do a null reference check
            var content = GetContent(bgWorker);

            // Prevents background worker calls if we're using the Command Line
            bool commandLine = false;
            if(bgWorker == null)
            {
                commandLine = true;
            }

            // If we're using the GUI, account for bgWorker cancellation
            if (!commandLine && bgWorker.CancellationPending)
            {
                bgWorker.ReportProgress(0, "Cancelled via User Request");
                return;
            }

            // Check if content is null, if so, cancel out.
            if(content == null)
            {
                if(!commandLine)
                {
                    bgWorker.ReportProgress(0, "Error reading source");
                    return;
                } else
                {
                    Console.WriteLine("Error reading source");
                    return;
                }
            }

            // Calculate some statistics to include in the template
            int totalDirs = 0;
            int totalFiles = 0;
            long totalSize = 0;

            // Loop through the directories and get the total count of directories,
            // total files and the files total size
            foreach(var directory in content)
            {
                totalDirs++;
                foreach(var file in directory.Files)
                {
                    totalFiles++;
                    totalSize += Helpers.ParseLong(file.GetProp("Size"));
                }
            }

            // Generate the actual output
            if(!commandLine)
            {
                bgWorker.ReportProgress(0, "Generating HTML File");
            } else
            {
                Console.WriteLine("Generating HTML file");
            }

            // Read the template into memory
            var template = new StringBuilder();
            try
            {
                using (StreamReader reader = new StreamReader("template.html", Encoding.UTF8))
                {
                    template.Append(reader.ReadToEnd());
                }
            } catch (Exception ex)
            {
                if(!commandLine)
                {
                    MessageBox.Show("Failed to open 'Template.html' for reading:\n\n" + ex.Message, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    bgWorker.ReportProgress(0, "An error occurred...");
                    return;
                } else
                {
                    Console.ForegroundColor = ConsoleColor.Red;
                    Console.WriteLine($"Failed to open Template.html, error {ex.Message}");
                    Console.ForegroundColor = ConsoleColor.White;
                }
            }

            // Build the intial section of the template
            var builtHtml = BuildHtml(template, applicationName, applicationVersion, totalFiles, totalDirs, totalSize);

            // Write the file / directory content to the file
            try
            {
                using (StreamWriter writer = new StreamWriter(XmlConfigurator.Read("OutputFile"), false, Encoding.UTF8))
                {
                    writer.AutoFlush = true;

                    var builtTemplate = builtHtml.ToString();
                    var dataStart = builtTemplate.IndexOf("[DIR DATA]");

                    writer.Write(builtTemplate.Substring(0, dataStart));

                    BuildJavascriptContentArray(content, 0, writer, bgWorker);

                    if(!commandLine)
                    {
                        if(bgWorker.CancellationPending)
                        {
                            bgWorker.ReportProgress(0, "Cancelled via User Request");
                            return;
                        }

                        writer.Write(builtTemplate.Substring(dataStart + 10));
                    }

                    builtHtml = null;
                    template = null;

                    // Check if we're using the GUI and if we want to open in the browser after the data capture.
                    // This does not work in command line mode as we assume that you'd be running it via a task or something.
                    if(!commandLine && bool.Parse(XmlConfigurator.Read("OpenInBrowserAfterCapture")))
                    {
                        Process.Start(XmlConfigurator.Read("OutputFile"));
                    }
                }
            } catch (Exception ex)
            {
                if (!commandLine)
                {
                    MessageBox.Show("Failed to open file for writing:\n\n" + ex.Message, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    bgWorker.ReportProgress(0, "An error occurred...");
                    return;
                }
                else
                {
                    Console.ForegroundColor = ConsoleColor.Red;
                    Console.WriteLine($"Failed to open file for writing, error {ex.Message}");
                    Console.ForegroundColor = ConsoleColor.White;
                }
            }

            if(!commandLine)
            {
                Cursor.Current = Cursors.Default;
                bgWorker.ReportProgress(100, "Ready");
            }
            
        }
    }
}
