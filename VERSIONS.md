# Versions
This is the version history for Snap2HTML.  NOT for Snap2HTML-NG.  I have merely left this here for tracking purposes.

### v1.0 (2011-07-25)
- Initial release

### v1.1 (2011-08-11)
- Added tooltips when hovering folders
- Bugfixes

### v1.2 (2011-08-18)
- Fixed some folder sorting problems
- Better error handling when permissions do not allow reading

### v1.5 (2012-06-18)
- Added command line support
- Files can now be linked to a target of your choice
- Option to automatically open snapshots when generated
- Several bugfixes and tweaks

### v1.51 (2012-07-11)
- Improved error handling

### v1.9 (2013-07-24)
- Major overhaul of HTML template
- MUCH faster HTML loading
- Reduced HTML size by about 1/3
- Folders are now also displayed in the HTML file list
- Added option to set page title
- Application now saves it settings (in application folder)
- GUI enhancements: Drag & Drop a source folder, tooltips
- Many smaller fixes to both application and HTML

### v1.91 (2013-12-29)
    Smaller change to hide root folder when linking files

### v1.92 (2014-06-12)
- Fixed various bugs reported by users lately
- Slight changes to the internals of the template file

### v2.0 (2017-04-22)
- Added export functionality to get data "out" of the HTML
- Export data as plain text, CSV or JSON
- Added support for searching with wildcards
- Search can be limited to current folder and/or subfolders
- Breadcrumb path is now clickable
- Current path is tracked in URL. Copy URL to link directly to that folder
- Opens previous folder when going back after opening a linked file
- Data format was tweaked to give slightly smaller output
- Fixed some bugs concerning filenames with odd characters
- Many other tweaks and fixes to improve the HTML template

###  v2.1 (2020-03-16)
- Export functionality now has a button to save directly to disk
- Added -silent option to run completely hidden from command line
- Dates now show formatted in user's locale independent of who created the file listing. Sort by date also works better.
- 8 or so other bug fixes and minor enhancements
- Internal code refactoring in preparation for future features

### v2.11 (2020-04-18)
- Fixed a threading issue that caused the program to hang on some systems

### v2.12 (2020-04-29)
- Reduced memory consumsion when generating HTML
- Parent folder link [..] is now sticky
- Reworked command line code to fix issues in 2.11
- A few small tweaks too

### v2.13 (2020-05-05)
- Fixed an encoding issue causing EncoderFallbackException while saving

### v2.14 (2020-08-09)
- UNC paths work again (including linking)
- Searches returning lots of items should be a bit faster now
= And a few smaller fixes as usual