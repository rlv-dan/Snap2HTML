using Snap2HTMLNG.Shared.Models;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Snap2HTMLNG.Shared.Utils.CommandLine
{
    public class Helpers
    {

        /// <summary>
        /// Splits out the arguments passed to the command line and creates a <see cref="List{T}"/> based on <see cref="CommandLineModel"/>
        /// </summary>
        /// <param name="args">
        /// <see cref="string"/> array of arguments from Command Line
        /// </param>
        /// <example>
        /// 
        /// <code>
        /// List&lt;CommandLineModel&gt; list = CommandLineSplit(args);
        /// </code>
        /// 
        /// </example>
        /// <returns>
        /// <see cref="List{T}"/> based on CommandLineModel
        /// </returns>
        public static List<CommandLineModel> CommandLineSplit(string[] args)
        {

            List<CommandLineModel> list = new List<CommandLineModel>();

            // args are in the following format
            // -argname:value
            foreach (string arg in args)
            {
                // check if the user has passed a path
                if(arg.StartsWith("-path:"))
                {
                    list.Add(new CommandLineModel { Name = "path", Value = arg.Split(new char[] { ':' }, 2).Last() });
                }

                // check if the user has passed the output location
                // this is the full path and file name (C:\Temp\Output.html)
                if(arg.StartsWith("-output:"))
                {
                    list.Add(new CommandLineModel { Name = "output", Value = arg.Split(new char[] { ':' }, 2).Last() });
                }

                // Check if the user wants us to link the files in the document for easy viewing
                if(arg.StartsWith("-link:"))
                {
                    list.Add(new CommandLineModel { Name = "link", Value = arg.Split(new char[] { ':' }, 2).Last() });
                }

                // Title, if omitted it will be automatically generated
                if(arg.StartsWith("-title:"))
                {
                    list.Add(new CommandLineModel { Name = "title", Value = arg.Split(new char[] { ':' }, 2).Last() });
                }

                // Include hidden files or not
                if(arg.StartsWith("-hidden"))
                {
                    list.Add(new CommandLineModel { Name = "hidden", Value = "" });
                }

                // Include system files or not
                if(arg.StartsWith("-system"))
                {
                    list.Add(new CommandLineModel { Name = "system", Value = "" });
                }


            }

            return list;
        }
    
    
        /// <summary>
        /// Changes the foreground color of the console to red, writes the message, then changes it back to white.
        /// </summary>
        /// <param name="message">
        /// Desired error message to show the user as a <see cref="string"/>
        /// </param>
        public static void WriteError(string message)
        {
            Console.ForegroundColor = ConsoleColor.Red;
            Console.WriteLine($"{message}");
            Console.ForegroundColor = ConsoleColor.White;
        }
    }
}
