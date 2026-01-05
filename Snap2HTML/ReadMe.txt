
 --- Snap2HTML ----------------------------------------------------------------
 
  Freeware by RL Vision (c) 2011-2026
  Homepage: https://www.rlvision.com
  
  Portable:
    - Just unzip and run
    - Settings are saved in the application folder

  Free Open Source Software:
    - Source code available at GitHub: https://github.com/rlv-dan/Snap2HTML
  

 --- Table of Contents --------------------------------------------------------

  - About
  - Search
  - Linking Files
  - Command Line
  - Template Design
  - Good to Know
  - Version History
  - End User License Agreement
  - Privacy Policy


 --- About --------------------------------------------------------------------

  This application takes a "snapshot" of the folder structure on your
  harddrive and saves it as an HTML file. What's unique about Snap2HTML is
  that the HTML file uses modern techniques to make it feel like a "real"
  application, displaying a treeview with folders that you can navigate to 
  view the files contained within. There is also a built in file search and
  ability to export data as plain text, csv or json. Still, everything is 
  contained in a single HTML file that you can easily store or distribute.
  
  Snap2html file listings can be used in many ways. One is as a complement
  to your backups (note however that this program does not backup your
  files! It only creates a list of the files and directories). You can
  also keep a file list of e.g. external HDDs and other computers, in case 
  you need to look something up or to save for historic reasons and 
  documentation. When helping your friends with their computer problems 
  you can ask them to send a snapshot of their folders so you can better 
  understand their problem. It's really up to you to decide what Snap2HTML 
  can be used for!


 --- Search -------------------------------------------------------------------

  The search box accepts the following modifiers:
  
    Wildcards:
        * matches zero or more characters
        ? matches exactly one character
  
    Prefixes:
        >  only search the current folder
        >> only search the current folder including sub folders
        f: only search for files
        d: only search for folders

  Tip: Search for * to list all files. This is especially useful together with 
  the export functionality to get all data out of the html file. 


 --- Linking Files ------------------------------------------------------------

  Linking allows you open the listed files directly in your web browser. This 
  is designed to be flexible, but it can be tricky to get right. Here are 
  some examples that shows how to construct links. By default Snap2HTML will 
  help by generating a fully qualified link to the root folder (first example):

  
	-> Link to fully qualified local path
		Root folder:		"C:\my_root\"
		Link to:			"file:///C:/my_root"
		Use snapshot from:	[anywhere locally]

	-> Link to relative local path
		Root folder:		"C:\my_root\"
		Link to:			"./my_root"
		Use snapshot from:	"C:\snapshot.html"

	-> Link to same folder as snapshot is saved in
		Root folder:		"C:\my_root\"
		Link to:			"."
		Use snapshot from:	"c:\my_root\snapshot.html"

	-> Link to a web server with mirror of a local folder
		Root folder:		"C:\my_root\"
		Link to:			"https://www.example.com/mirror_of_my_root/"
		Use snapshot from:	[anywhere]

	-> Link to a relative path on a web server with mirror of local folder
		Root folder:		"C:\my_root\"
		Link to:			"./mirror_of_my_root"
		Use snapshot from:	"https://www.example.com/snapshot.html"

    -> Link to a folder on the network (UNC path)
		Root folder:		"\\srv123\my_root"
		Link to:			"file://srv123/my_root"
		Use snapshot from:	[anywhere on local network]

    -> Link to a local file from a snapshot located on a web server
        Not allowed due to browser security policies

  Notes:
    
	Only files can be linked. Folders are automatically linked to browse the
	path in the snapshot.

    It is possible to specify that only certain files should be linked by 
    editing the template (see "viewOptions").

    Different browsers handle local links in different ways, usually for 
    security reasons. For example, Internet Explorer will not let you open
    links to files on your local machine at all. (You can however copy the
    link and paste into the location field.)


 --- Command Line -------------------------------------------------------------

  You can automate Snap2HTML by starting it from the command line with the 
  following options:

  Simple:   Snap2HTMl.exe "c:\path\to\root\folder"
    
              Starts the program with the given root path already set


  Full:     Snap2HTMl.exe -path:"root folder path" -outfile:"filename"
                          [-link:"link to path"] [-title:"page title"] 
						  [-hidden] [-system] [-silent]

              -path:"root folder path"   - The root path to load.
                                           Example: -path:"c:\temp"
                                         
              -outfile:"filename"        - The filename to save the snapshot as.
                                           Don't forget the html extension!
                                           Example: -outfile:"c:\temp\out.html"

              -link:"link to path"       - The path to link files to.
                                           Example: -link:"c:/temp"
                                         
              -title:"page title"        - Set the page title. If omitted, title 
                                           is generated based on path.
			  
              -hidden                    - Include hidden items

              -system                    - Include system items

			  -silent                    - Run without showing the window (only
                                           if both -path and -outfile are used)

  Example:  .\Snap2HTMl.exe -path:"D:\Downloads" -outfile:"C:\Temp\DL.html" -title:"Latest Files" -silent


  Notes:    When both -path and -outfile are specified, the program will 
            automatically start generating the snapshot, and quit when done.

            Parameters must be in lower case. For example -Silent will not work.

            Always surround paths and filenames with quotes ("")!
            
            In silent mode, no message boxes are displayed in case of error.
            If the error is fatal it will just quit without telling why.

            Do not include the [square brackets] when you write your command 
            line. (Square brackets signify optional command line parameters)


 --- Good to Know ------------------------------------------------------------

  The generated html file is completetly self contained and does not require 
  any other file or and internet connection to work.

  Folders that you are not authorized to read are reported as errors. Running 
  Snap2HTML as Administrator should minimize these problems, but Windows may 
  still contain folders that even administrators are not allowed to access.
  
  Displayed file sizes are logical, not the "size on disk". The actual (physical) 
  file size may be different, depending on features such as file compression, 
  cloud storage, disk fragmentation and sparse files.

  Snap2html can handle huge file listings. Hundreds of thousands files & folders 
  is not a problem. My biggest test contained 1/2 million folders with 4 million 
  files. It may take a few seconds to load, and when using the search the first 
  time it will take a while to warm up. But it works fine!

  The generated html file can get quite big. It may not be suitable to put online.
  But remember that web servers automatically compress (gzip) files before sending,
  so the actualy transferred data will be about 1/4 of the original size!

  When linking files, browsers open media they recognize in their internal players. 
  If you do not want this, browsers have settings to open in the system default 
  player instead.

  Supposedly, setting the following registry key can help if network shares do 
  not show up in the folder select dialog:
    [HKEY_LOCAL_MACHINE\SOFTWARE\Microsoft\Windows\CurrentVersion\Policies\System]
    "EnableLinkedConnections" = dword:00000001
  Reboot the system to apply the change.

  The generated html requires JavaScript to run, but I have intentionally been 
  conservative when it comes to new features to make it possible to run on 
  older machines. It has been tested to run on Internet Explorer 11.

 
 --- Version History ---------------------------------------------------------

  v2.52 (2026-01-05)
    Replace \ with / to help users link correctly when using commandline
    Include missing config file needed for high dpi support

  v2.51 (2026-01-03)
    Fix a linking issue

  v2.5 (2025-12-30)
    Complete rewrite of disk scanning code
      - Support long paths (260+ characters)
      - Speed improvements (~5x-8x faster)
      - Better error recovery (no more missing files!)
      - Errors are reported after scanning
    HTML Template
      - Faster initial display and data loading, especially for large files
      - Optimized data format, saving about 1/3 in size
      - Technically support multiple root folders
      - Added search modifiers "f:" and "d:" to limit to files/directories
      - Highlight search results
      - Natural sorting (1 sorts before 10)
      - Works better on mobile views
      - Folders with read errors are displayed
      - Larger base font size, sharper images and other visual enhancements
      - Added some display options that users can tweak inside the template
      - Expose some functionality for developers
      - A lot of code cleanup and refactoring
    Windows Application
      - Upgrade application to .Net Framework 4.8
      - High DPI support
      - Allow manually editing the root path

  v2.14 (2020-08-09)
	UNC paths work again (including linking)
	Searches returning lots of items should be a bit faster now
    And a few smaller fixes as usual

  v2.13 (2020-05-05)
    Fixed an encoding issue causing EncoderFallbackException while saving

  v2.12 (2020-04-29)
    Reduced memory consumsion when generating HTML
	Parent folder link [..] is now sticky
	Reworked command line code to fix issues in 2.11
	A few small tweaks too

  v2.11 (2020-04-18)
    Fixed a threading issue that caused the program to hang on some systems

  v2.1 (2020-03-16)
    Export functionality now has a button to save directly to disk
    Added -silent option to run completely hidden from command line
    Dates now show formatted in user's locale independent of who created
      the file listing. Sort by date also works better.
    8 or so other bug fixes and minor enhancements
    Internal code refactoring in preparation for future features

  v2.0 (2017-04-22)
	Added export functionality to get data "out" of the HTML
	Export data as plain text, CSV or JSON
	Added support for searching with wildcards
	Search can be limited to current folder and/or subfolders
	Breadcrumb path is now clickable
	Current path is tracked in URL. Copy URL to link directly to that folder
	Opens previous folder when going back after opening a linked file
	Data format was tweaked to give slightly smaller output
	Fixed some bugs concerning filenames with odd characters
	Many other tweaks and fixes to improve the HTML template

  v1.9 (2013-07-24)
    Major overhaul of HTML template
    MUCH faster HTML loading
	Reduced HTML size by about 1/3
	Folders are now also displayed in the HTML file list
	Added option to set page title
	Application now saves it settings (in application folder)
	GUI enhancements: Drag & Drop a source folder, tooltips
	Many smaller fixes to both application and HTML

  v1.92 (2014-06-12)
    Fixed various bugs reported by users lately
	Slight changes to the internals of the template file

  v1.91 (2013-12-29)
    Smaller change to hide root folder when linking files

  v1.51 (2012-07-11)
	Improved error handling

  v1.5 (2012-06-18)
	Added command line support
	Files can now be linked to a target of your choice
	Option to automatically open snapshots when generated
	Several bugfixes and tweaks

  v1.2 (2011-08-18)
	Fixed some folder sorting problems
	Better error handling when permissions do not allow reading

  v1.1 (2011-08-11)
	Added tooltips when hovering folders
	Bugfixes

  v1.0 (2011-07-25)
	Initial release


 --- End User License Agreement -----------------------------------------------
  
  RL Vision can not be held responsible for any damages whatsoever, direct or 
  indirect, caused by this software or other material from RL Vision.


 --- Privacy Policy ----------------------------------------------------------

  Snap2HTML does not connect to the internet. It does not phone home, check 
  for updates, submit telemetry, spy on you or any other crap like that. It 
  simply doesn't do anything behind your back and any data related to the 
  program is yours. As it should be.

