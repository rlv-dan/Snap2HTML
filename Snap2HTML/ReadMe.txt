
 --- Snap2HTML ----------------------------------------------------------------
 
  Freeware by RL Vision (c) 2011-2020
  Homepage: http://www.rlvision.com
  
  Portable:
    - Just unzip and run
    - Settings are saved in the application folder

  Free Open Source Software:
    - Source code available at GitHub: https://github.com/rlv-dan
  

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

  The built in search box accepts the following modifiers:
  
    Wildcards * and ? can be used. * matches zero or more characters. ? matches
    exactly one character.
  
    Prefix your search with > to search only the current folder. >> searches 
    the current folder and its sub folders.
 
  Tip: Search for * to list all files. This is especially useful together with 
  the export functionality to get all data out of the html file.


 --- Linking Files ------------------------------------------------------------

  Linking allows you open the listed files directly in your web browser. 
  This is designed to be flexible, which also sometimes makes it tricky
  to get right. Here are some examples that shows how to use it:

	-> Link to fully qualified local path
		Root folder:		"c:\my_root\"
		Link to:			"c:\my_root\"
		Use snapshot from:	[anywhere locally]

	-> Link to relative local path
		Root folder:		"c:\my_root\"
		Link to:			"my_root\"
		Use snapshot from:	"c:\snapshot.html"

	-> Link to same folder as snapshot is saved in
		Root folder:		"c:\my_root\"
		Link to:			[leave textbox empty]
		Use snapshot from:	"c:\my_root\snapshot.html"

	-> Link to a web server with mirror of local folder
		Root folder:		"c:\my_www_root\"
		Link to:			"http://www.example.com/"
		Use snapshot from:	[anywhere]

	-> Link to a relative path on a web server with mirror of local folder
		Root folder:		"c:\my_www_root\subfolder"
		Link to:			"subfolder/"
		Use snapshot from:	"http://www.example.com/snapshot.html"

  Notes:
    
	Only files can be linked. Folders are automatically linked to browse the
	path in the snapshot.

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
                                           Example: -link:"c:\temp"
                                         
              -title:"page title"        - Set the page title. If omitted, title 
                                           is generated based on path.
			  
              -hidden                    - Include hidden items

              -system                    - Include system items

			  -silent                    - Run without showing the window (only
                                           if both -path and -outfile are used)

  Notes:    When both -path and -outfile are specified, the program will 
            automatically start generating the snapshot, and quit when done.

            Always surround paths and filenames with quotes ("")!
            
            In silent mode, in case or error the program will just quit 
            without telling why.

            Do not include the [square brackets] when you write your command 
            line. (Square brackets signify optional command line parameters)


  --- Template Design ---------------------------------------------------------

  If you know html and javascript you may want to have a look at the file
  "template.html" in the application folder. This is the base for the
  output, and you can modify it with your own enhancements and design changes. 
  If you make something nice you are welcome, to send it to me and I might 
  distribute it with future versions of the program or add a link below!

  Showcases:
    Amstrad CPC Memory Engraved: https://acpc.me (Amazing!)


  --- Known Problems ----------------------------------------------------------

  The finished HTML file contains embedded javascript. Web browsers (especially 
  Internet Explorer) may limit execution of scripts as a security measure. 
  If the page is stuck on "loading..." (with no cpu activity - large files may 
  take a while to load) this is probably your problem.

  One user reported needing to start Snap2HTML with "Run as Administrator"
  on Win7 Basic, otherwise it would hang when clicking on the browse for 
  folders button.

  Only files you have access to can be read. Try "Run as Admin" if files are 
  missing.

  Internet Explorer may fail to load very large files. The problems seems to 
  be a hard limit in some versions of IE. I have seen this problem in IE11 
  myself. Being a hard limit there is no easy solution right now.

  Large file tables can be slow to render and appear to have hung the browser,
  especially in Internet Explorer. The same can happen when navigating away 
  from such a large folder and freeing the memory.

 
  --- Version History ---------------------------------------------------------

  v1.0 (2011-07-25)
	Initial release

  v1.1 (2011-08-11)
	Added tooltips when hovering folders
	Bugfixes

  v1.2 (2011-08-18)
	Fixed some folder sorting problems
	Better error handling when permissions do not allow reading

  v1.5 (2012-06-18)
	Added command line support
	Files can now be linked to a target of your choice
	Option to automatically open snapshots when generated
	Several bugfixes and tweaks

  v1.51 (2012-07-11)
	Improved error handling

  v1.9 (2013-07-24)
    Major overhaul of HTML template
    MUCH faster HTML loading
	Reduced HTML size by about 1/3
	Folders are now also displayed in the HTML file list
	Added option to set page title
	Application now saves it settings (in application folder)
	GUI enhancements: Drag & Drop a source folder, tooltips
	Many smaller fixes to both application and HTML

  v1.91 (2013-12-29)
    Smaller change to hide root folder when linking files

  v1.92 (2014-06-12)
    Fixed various bugs reported by users lately
	Slight changes to the internals of the template file

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

  v2.1 (2020-03-16)
    Export functionality now has a button to save directly to disk
    Added -silent option to run completely hidden from command line
    Dates now show formatted in user's locale independent of who created
      the file listing. Sort by date also works better.
    8 or so other bug fixes and minor enhancements
    Internal code refactoring in preparation for future features

  v2.11 (2020-04-18)
    Fixed a threading issue that caused the program to hang on some systems

  v2.12 (2020-04-29)
    Reduced memory consumsion when generating HTML
	Parent folder link [..] is now sticky
	Reworked command line code to fix issues in 2.11
	A few small tweaks too


 --- End User License Agreement -----------------------------------------------
  
  RL Vision can not be held responsible for any damages whatsoever, direct or 
  indirect, caused by this software or other material from RL Vision.


  --- Privacy Notice ----------------------------------------------------------

  Snap2HTML does not connect to the internet. It does not phone home, check 
  for updates, submit telemetry, spy on you or any other crap like that. It 
  simply doesn't do anything behind your back and any data related to the 
  program is yours. As it should be.
