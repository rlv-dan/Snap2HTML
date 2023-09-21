# Search
The built in search box accepts the following modifiers:

    Wildcards * and ? can be used. * matches zero or more characters. ? matches
    exactly one character.

    Prefix your search with > to search only the current folder. >> searches 
    the current folder and its sub folders.
 
**Tip**: Search for * to list all files. This is especially useful together with the export functionality to get all data out of the html file.

# File Linking

Linking allows you open the listed files directly in your web browser. This is designed to be flexible, which also sometimes makes it tricky to get right.

Only files can be linked. Folders are automatically linked to browse the path in the snapshot.

Different browsers handle local links in different ways, usually for security reasons. 

For example, Internet Explorer will not let you open links to files on your local machine at all. (*You can however copy the
link and paste into the location field.*)

Here are some examples that shows how to use it:

## Link to fully qualified local path
**Root folder**:		"c:\my_root\"

**Link to**:			"c:\my_root\"

**Use snapshot from**:	[anywhere locally]

## Link to relative local path
**Root folder**:		"c:\my_root\"

**Link to**:			"my_root\"

**Use snapshot from**:	"c:\snapshot.html"

## Link to same folder as snapshot is saved in
**Root folder**:		"c:\my_root\"

**Link to**:			[leave textbox empty]

**Use snapshot from**:	"c:\my_root\snapshot.html"

## Link to a web server with mirror of local folder
**Root folder**:		"c:\my_www_root\"

**Link to**:			"http://www.example.com/"

**Use snapshot from**:	[anywhere]

## Link to a relative path on a web server with mirror of local folder
**Root folder**:		"c:\my_www_root\subfolder"

**Link to**:			"subfolder/"

**Use snapshot from**:	"http://www.example.com/snapshot.html"

# Command Line
You can automate Snap2HTML by starting it from the command line with the 
following options:

## Simple
    Snap2HTMl.exe "c:\path\to\root\folder"

**Note**:   Starts the program with the given root path already set

## Full

    Snap2HTMl.exe -path:"root folder path" -outfile:"filename" [-link:"link to path"] [-title:"page title"] [-hidden] [-system] [-silent]

### Paramters

    -path:"root folder path"   - The root path to load.

**Example**: -path:"c:\temp"
                                         
    -outfile:"filename"        - The filename to save the snapshot as. Don't forget the html extension! 

**Example**: -outfile:"c:\temp\out.html"

    -link:"link to path"       - The path to link files to. 

**Example**: -link:"c:\temp"
                                         
    -title:"page title"        - Set the page title. If omitted, title is generated based on path.
          
    -hidden                    - Include hidden items

    -system                    - Include system items

    -silent                    - Run without showing the window (only if both -path and -outfile are used)

**Notes**:    

When both -path and -outfile are specified, the program will automatically start generating the snapshot, and quit when done.

Always surround paths and filenames with quotes ("")!
            
In silent mode, in case or error the program will just quit without telling why.

Do not include the [square brackets] when you write your command  line. (Square brackets signify optional command line parameters)

# Template Design
If you know html and javascript you may want to have a look at the file "template.html" in the application folder. This is the base for the output, and you can modify it with your own enhancements and design changes. 

If you make something nice you are welcome, to send it to me and I might distribute it with future versions of the program or add a link below!

- Showcases:
    - Amstrad CPC Memory Engraved: https://acpc.me (Amazing!)