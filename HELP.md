# Search
The built in search box accepts the following modifiers:

Wildcards \* and ? can be used. \* matches zero or more characters. ? matches exactly one character.

Prefix your search with > to search only the current folder. >> searches the current folder and its sub folders.
 
**Tip**: Search for \* to list all files. This is especially useful together with the export functionality to get all data out of the html file.

# Search Pattern
The Search Pattern in the GUI is a new feature to Snap2HTML-NG.  This allows you to only pull files that match a certain cirteria.  Similar to the Search function in the HTML file;

Wildcards \* and ? can be used.  \* matches zero or more characters in that position.  ? matches exactly one character in that position.

Characters other than the wildcard are literal characters.  The Search Pattern "\*w" searches for all file names in the path ending with the letter "W".  The Search Pattern "l\*" searches for all file names beginning with the letter "L".

Note due to how .NET Framework is designed, if you use an aserisk wild card for specifying file types, such as "\*.mp4", this will return files with extensions that match and _begin_ with mp4.  If you use the ? wildcard somewhere within the search pattern, it will resolve this issue.

For example using "\*.txt" will return "\*.txt, \*.txtt" etc.

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
Starting with Snap2HTML-NG version 3, the Command Line process has completely changed and been split out to it's own application.

## Usage:
     Snap2HTML-NG.CLI [options]

 ## Options:
     -path:          [Required] The directory you want to scan
     -output:        [Required] The directory where you want to save the file, including the filename unless using -randomize
     -link:          [Optional] The directory where you want to link files in the html file
     -title:         [Optional] The title of the file which appears at the top of the html file
     -hidden         [Optional] Hides Hidden files from the scan, default is TRUE
     -system         [Optional] Hides System files from the scan, default is TRUE
     -help, -h       [Optional] Shows this information
     -pattern        [Optional] Search pattern to only return certain files, default is *
     -randomize      [Optional] Generates a random file name instead of needing to specify one in -output

 ## Examples:
     Snap2HTML-NG.CLI -path:"C:\Users\%username%\Downloads" -output:"C:\Users\%username%\Desktop\Downloads.html"
     Snap2HTML-NG.CLI -path:"C:\Users\%username%\Pictures" -output:"C:\Users\%username%\Desktop" -randomize
     Snap2HTML-NG.CLI -path:"C:\Users\%username%\Downloads" -output:"C:\Users\%username%\Desktop" -link:"C:\Users\%username%\Downloads" -randomize
     Snap2HTML-NG.CLI -path:"C:\Users\%username%\Downloads" -output:"C:\Users\%username%\Desktop" -link:"C:\Users\%username%\Downloads" -randomize -pattern:"*.mp4"
     Snap2HTML-NG.CLI -path:"C:\Users\%username%\Videos" -output:"C:\Users\%username%\Desktop\videos.html" -link:"C:\Users\%username%\Videos" -pattern:"*.mp4" -title:"Home Videos"

**Notes**:    

Always surround paths and filenames with quotes ("")!
            
# Template Design
If you know html and javascript you may want to have a look at the file "template.html" in the application folder. This is the base for the output, and you can modify it with your own enhancements and design changes. 

If you make something nice you are welcome, to send it to me and I might distribute it with future versions of the program or add a link below!

- Showcases:
    - Amstrad CPC Memory Engraved: https://acpc.me (Amazing!)