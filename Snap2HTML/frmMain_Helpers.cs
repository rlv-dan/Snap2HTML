using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Text;
using System.Windows.Forms;
using CommandLine.Utility;
using System.IO;

namespace Snap2HTML
{
	public partial class frmMain : Form
	{
		// Sets the root path input box and makes related gui parts ready to use
		private void SetRootPath( string path, bool pathIsValid = true )
		{
			if( pathIsValid )
			{
				txtRoot.Text = path;
				cmdCreate.Enabled = true;
				toolStripStatusLabel1.Text = "";
				if( initDone )
				{
					txtLinkRoot.Text = txtRoot.Text;
					txtTitle.Text = "Snapshot of " + txtRoot.Text;
				}
			}
			else
			{
				txtRoot.Text = "";
				cmdCreate.Enabled = false;
				toolStripStatusLabel1.Text = "";
				if( initDone )
				{
					txtLinkRoot.Text = txtRoot.Text;
					txtTitle.Text = "";
				}
			}
		}

		// Recursive function to get all folders and subfolders of given path path
		private void DirSearch( string sDir, List<string> lstDirs, bool skipHidden, bool skipSystem )
		{
			if( backgroundWorker.CancellationPending ) return;

			try
			{
				foreach( string d in System.IO.Directory.GetDirectories( sDir ) )
				{
					bool includeThisFolder = true;

					if( d.ToUpper().EndsWith( "SYSTEM VOLUME INFORMATION" ) ) includeThisFolder = false;

					// exclude folders that have the system or hidden attr set (if required)
					if( skipHidden || skipSystem )
					{
						var attr = new DirectoryInfo( d ).Attributes;

						if( skipHidden )
						{
							if( ( attr & FileAttributes.Hidden ) == FileAttributes.Hidden )
							{
								includeThisFolder = false;
							}
						}

						if( skipSystem )
						{
							if( ( attr & FileAttributes.System ) == FileAttributes.System )
							{
								includeThisFolder = false;
							}
						}						
					}


					if( includeThisFolder )
					{
						lstDirs.Add( d );

						if( lstDirs.Count % 9 == 0 ) // for performance don't update gui for each file
						{
							backgroundWorker.ReportProgress( 0, "Aquiring folders... " + lstDirs.Count + " (" + d + ")" );
						}

						DirSearch( d, lstDirs, skipHidden, skipSystem );
					}
				}
			}
			catch( System.Exception ex )
			{
				Console.WriteLine( "ERROR in DirSearch():" + ex.Message );
			}
		}

		// Hack to sort folders correctly even if they have spaces/periods in them
		private List<string> SortDirList( List<string> lstDirs )
		{
			for( int n = 0; n < lstDirs.Count; n++ )
			{
				lstDirs[n] = lstDirs[n].Replace( " ", "1|1" );
				lstDirs[n] = lstDirs[n].Replace( ".", "2|2" );
			}
			lstDirs.Sort();
			for( int n = 0; n < lstDirs.Count; n++ )
			{
				lstDirs[n] = lstDirs[n].Replace( "1|1", " " );
				lstDirs[n] = lstDirs[n].Replace( "2|2", "." );
			}
			return lstDirs;
		}

		// Replaces characters that may appear in filenames/paths that have special meaning to JavaScript
		// Info on u2028/u2029: https://en.wikipedia.org/wiki/Newline#Unicode
		private string MakeCleanJsString( string s )
		{
			return s.Replace( "\\", "\\\\" )
					.Replace( "&", "&amp;" )
					.Replace( "\u2028", "" )
					.Replace( "\u2029", "" );
		}

		// Test string for matches against a wildcard pattern. Use ? and * as wildcards. (Wrapper around RegEx)
		private bool IsWildcardMatch( String wildcard, String text, bool casesensitive )
		{
			System.Text.StringBuilder sb = new System.Text.StringBuilder( wildcard.Length + 10 );
			sb.Append( "^" );
			for( int i = 0; i < wildcard.Length; i++ )
			{
				char c = wildcard[i];
				switch( c )
				{
					case '*':
						sb.Append( ".*" );
						break;
					case '?':
						sb.Append( "." );
						break;
					default:
						sb.Append( System.Text.RegularExpressions.Regex.Escape( wildcard[i].ToString() ) );
						break;
				}
			}
			sb.Append( "$" );
			System.Text.RegularExpressions.Regex regex;
			if( casesensitive )
				regex = new System.Text.RegularExpressions.Regex( sb.ToString(), System.Text.RegularExpressions.RegexOptions.None );
			else
				regex = new System.Text.RegularExpressions.Regex( sb.ToString(), System.Text.RegularExpressions.RegexOptions.IgnoreCase );

			return regex.IsMatch( text );
		}
	}
}
