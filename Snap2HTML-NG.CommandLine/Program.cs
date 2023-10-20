using Snap2HTMLNG.Shared.Models;
using System;
using System.Collections.Generic;
using System.IO;

namespace Snap2HTMLNG.CommandLine
{
    internal class Program
    {
        static void Main(string[] args)
        {
            List<CommandLineModel> normalizedArgs = Shared.Utils.CommandLine.Helpers.CommandLineSplit(args);

            InitialValidation(normalizedArgs);

            string scanPath = normalizedArgs.Find(x => x.Name == "path").Value;
            string savePath = normalizedArgs.Find(x => x.Name == "output").Value;
            
            // Validate that the Scan Path actually exists
            if(!Directory.Exists(scanPath))
            {
                Shared.Utils.CommandLine.Helpers.WriteError($"Your scan path {scanPath} does not exist or Snap2HTML-NG does not have access.");
                return;
            }

            // Check if the Save location actually exists
            if(!Directory.Exists(Path.GetDirectoryName(savePath)))
            {
                Shared.Utils.CommandLine.Helpers.WriteError($"Your save path {savePath} does not exist or Snap2HTML-NG does not have access.");
                return;
            }

            // Check if the rest of the settigns have been passed and assign
            bool skipHidden = !normalizedArgs.Exists(x => x.Name == "hidden"); // if found, do not skip
            bool skipSystem = !normalizedArgs.Exists(x => x.Name == "system"); // if found, do not skip
            
            // Check if the user wants to link files to the root which allows them to be easily selected when viewing in a browser
            bool linkFilesToRoot = false;
            if(normalizedArgs.Exists(x => x.Name == "link"))
            {
                linkFilesToRoot = true;
                string linkPath = normalizedArgs.Find(x => x.Name == "link").Value;
            }

            string title = $"Snapshot of {savePath}";
            if(normalizedArgs.Exists(x => x.Name == "title"))
            {
                title = normalizedArgs.Find(x => x.Name == "title").Value;
            }

            // TODO: Migrate the running code in GUI so that we can run this here without
            // having to open or have any requirement on having the GUI on the machine

            //foreach (var normalizedArg in normalizedArgs)
            //{
            //    Console.WriteLine($"Name: {normalizedArg.Name}, Value: {normalizedArg.Value}");
            //}

            Console.ReadKey();
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
                Shared.Utils.CommandLine.Helpers.WriteError("No arguments have been supplied that are recognized.");
                return;
            }

            // Check if we have included the REQUIRED argument -path:, otherwise exit out.
            if (!normalizedArgs.Exists(x => x.Name == "path"))
            {
                Shared.Utils.CommandLine.Helpers.WriteError("You are missing the required argument '-path:'");
                return;
            }

            // Check if we have included the REQUIRED argument -output:, otherwise exit out.
            if (!normalizedArgs.Exists(x => x.Name == "output"))
            {
                Shared.Utils.CommandLine.Helpers.WriteError("You are missing the required argument '-output:'");
            }
        }
    }
}
