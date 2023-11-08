using Snap2HTMLNG.Shared.Builder;
using Snap2HTMLNG.Shared.Models;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Reflection;

namespace Snap2HTMLNG.CommandLine
{
    internal class Program
    {

        private static bool _validationCheck = false;
        static void Main(string[] args)
        {

#if DEBUG
            // Information
            Shared.Utils.CommandLine.Helpers.WriteDebug($"  THIS IS A PREVIEW BUILD.");
            // New Lines
            Console.WriteLine("");
#endif

            List<CommandLineModel> normalizedArgs = Shared.Utils.CommandLine.Helpers.CommandLineSplit(args);

            if(normalizedArgs.Exists(x => x.Name == "help"))
            {
                HelpInformation();
                return;
            }

            InitialValidation(normalizedArgs);

            // Check if the initial validation checks have passed
            if(_validationCheck)
            {
                string rootDirectory = normalizedArgs.Find(x => x.Name == "path").Value;// + @"\";
                string saveDirectory = normalizedArgs.Find(x => x.Name == "output").Value;

                // Check if the user has requested a randomized file name and set if they have
                if(normalizedArgs.Exists(x =>x.Name == "randomize"))
                {
                    Shared.Utils.CommandLine.Helpers.WriteInformation($"Randomized file name requested...");

                    string randomFileName = $"{Guid.NewGuid()}.html";

                    saveDirectory = $@"{saveDirectory}\{randomFileName}";

                    Shared.Utils.CommandLine.Helpers.WriteInformation($"Randomized file set to {randomFileName}");

                    Shared.Utils.CommandLine.Helpers.WriteInformation($"Output path is now: {saveDirectory}");

                }

                // Validate that the Scan Path actually exists
                if (!Directory.Exists(rootDirectory))
                {
                    Shared.Utils.CommandLine.Helpers.WriteError($"Your scan path {rootDirectory} does not exist or Snap2HTML-NG does not have access.");
                    return;
                }

                // Check if the Save location actually exists
                if (!Directory.Exists(Path.GetDirectoryName(saveDirectory)))
                {
                    Shared.Utils.CommandLine.Helpers.WriteError($"Your save path {saveDirectory} does not exist or Snap2HTML-NG does not have access.");
                    return;
                }

                // Check if the rest of the settigns have been passed and assign
                bool skipHidden = !normalizedArgs.Exists(x => x.Name == "hidden"); // if found, do not skip
                bool skipSystem = !normalizedArgs.Exists(x => x.Name == "system"); // if found, do not skip

                // Check if the user wants to link files to the root which allows them to be easily selected when viewing in a browser
                bool linkFilesToRoot = false;
                string linkDirectory = "";
                if (normalizedArgs.Exists(x => x.Name == "link"))
                {
                    linkFilesToRoot = true;
                    linkDirectory = normalizedArgs.Find(x => x.Name == "link").Value;
                }

                string title = $"Snapshot of {saveDirectory}";
                if (normalizedArgs.Exists(x => x.Name == "title"))
                {
                    title = normalizedArgs.Find(x => x.Name == "title").Value;
                }

                string searchPattern = "*"; // default is all
                if (normalizedArgs.Exists(x => x.Name == "pattern"))
                {
                    searchPattern = normalizedArgs.Find(x => x.Name == "pattern").Value;
                }

#if DEBUG
                foreach (var normalizedArg in normalizedArgs)
                {
                    Shared.Utils.CommandLine.Helpers.WriteDebug($"Name: {normalizedArg.Name}, Value: {normalizedArg.Value}");
                }
#endif
                // Create the settings model and assign the relevant information to each property
                UserSettingsModel usm = new UserSettingsModel
                {
                    RootDirectory = rootDirectory,
                    Title = title,
                    OutputFile = saveDirectory,
                    SkipHiddenItems = skipHidden,
                    SkipSystemItems = skipSystem,
                    OpenInBrowserAfterCapture = false, // this will always be false in console mode
                    LinkFiles = linkFilesToRoot,
                    LinkRoot = linkDirectory,
                    SearchPattern = searchPattern
                };

                DataBuilder.Build(usm, ApplicationInformation().ProductName, ApplicationInformation().ProductVersion);


                Console.ReadKey();
            }
        }


        /// <summary>
        /// Checks if the user has passed through any arguments, as well as if they have included the two REQUIRED arguments
        /// </summary>
        /// <param name="normalizedArgs">
        /// <see cref="List{T}"/> of arguments using <see cref="CommandLineModel"/>
        /// </param>
        static void InitialValidation(List<CommandLineModel> normalizedArgs)
        {
            // Check if we have included any arguments at all, otherwise exit out.
            if (normalizedArgs.Count == 0)
            {
                Shared.Utils.CommandLine.Helpers.WriteError("No arguments have been supplied that are recognized.  Use -h for help.");
                _validationCheck = false;
                return;
            }

            // Check if we have included the REQUIRED argument -path:, otherwise exit out.
            if (!normalizedArgs.Exists(x => x.Name == "path"))
            {
                Shared.Utils.CommandLine.Helpers.WriteError("You are missing the required argument '-path:'. Use -h for help.");
                _validationCheck = false;
                return;
            }

            // Check if we have included the REQUIRED argument -output:, otherwise exit out.
            if (!normalizedArgs.Exists(x => x.Name == "output"))
            {
                Shared.Utils.CommandLine.Helpers.WriteError("You are missing the required argument '-output:'. Use -h for help.");
                _validationCheck = false;
                return;
            }

            _validationCheck = true;
        }
    
        /// <summary>
        /// Returns Help information for using the Command Line
        /// </summary>
        static void HelpInformation()
        {
            Console.ForegroundColor = ConsoleColor.White;

            // Information
            Console.WriteLine(" Application Information");
            Console.WriteLine($" {ApplicationInformation().ProductName} v{ApplicationInformation().ProductVersion}");

            // New Lines
            Console.WriteLine("");

            // Description
            Console.WriteLine(" Description:");
            Console.WriteLine("     Help information for Snap2HTML-NG.Console");

            // New Lines
            Console.WriteLine("");

            // Usage
            Console.WriteLine(" Usage:");
            Console.WriteLine("     Snap2HTML-NG.Console [options]");

            // New Lines
            Console.WriteLine("");

            // Options
            Console.WriteLine(" Options:");
            Console.WriteLine("     -path:          [Required] The directory you want to scan");
            Console.WriteLine("     -output:        [Required] The directory where you want to save the file, including the filename unless using -randomize");
            Console.WriteLine("     -link:          [Optional] The directory where you want to link files in the html file");
            Console.WriteLine("     -title:         [Optional] The title of the file which appears at the top of the html file");
            Console.WriteLine("     -hidden         [Optional] Hides Hidden files from the scan, default is TRUE");
            Console.WriteLine("     -system         [Optional] Hides System files from the scan, default is TRUE");
            Console.WriteLine("     -help, -h       [Optional] Shows this information");
            Console.WriteLine("     -pattern        [Optional] Search pattern to only return certain files, default is *");
            Console.WriteLine("     -randomize      [Optional] Generates a random file name instead of needing to specify one in -output");

            // New Lines
            Console.WriteLine("");

            // Examples
            Console.WriteLine(" Examples:");
            Console.WriteLine("     Snap2HTML-NG.Console -path:\"C:\\John.Doe\\Downloads\" -output:\"C:\\John.Doe\\Desktop\\Downloads.html\" ");
            Console.WriteLine("     Snap2HTML-NG.Console -path:\"C:\\John.Doe\\Downloads\" -output:\"C:\\John.Doe\\Desktop\" -randomize ");
            Console.WriteLine("     Snap2HTML-NG.Console -path:\"C:\\John.Doe\\Downloads\" -output:\"C:\\John.Doe\\Desktop\" -link:\"C:\\John.Doe\\Downloads\" -randomize ");
            Console.WriteLine("     Snap2HTML-NG.Console -path:\"C:\\John.Doe\\Downloads\" -output:\"C:\\John.Doe\\Desktop\" -link:\"C:\\John.Doe\\Downloads\" -randomize -pattern:\"*.mp4\"");
            Console.WriteLine("     Snap2HTML-NG.Console -path:\"C:\\John.Doe\\Videos\" -output:\"C:\\John.Doe\\Desktop\\videos.html\" -link:\"C:\\John.Doe\\Videos\" -pattern:\"*.mp4\" -title:\"Home Videos\"");

        }

        /// <summary>
        /// Gets the application information using <see cref="FileVersionInfo"/> and <see cref="Assembly"/>
        /// </summary>
        /// <returns>
        /// <see cref="FileVersionInfo"/>
        /// </returns>
        static FileVersionInfo ApplicationInformation()
        {
            // Get product name etc.
            Assembly assembly = Assembly.GetExecutingAssembly();
            FileVersionInfo fvi = FileVersionInfo.GetVersionInfo(assembly.Location);

            return fvi;
        }
    }
}
