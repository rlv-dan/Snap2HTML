using System;
using System.Collections.Generic;
using System.ComponentModel;
//using System.Data;
//using System.Drawing;
//using System.Text;
//using System.Windows.Forms;
//using CommandLine.Utility;
using System.IO;
using System.Diagnostics;

namespace Snap2HTML
{
	class Utils
	{
		// Hack to sort folders correctly even if they have spaces/periods in them
		public static List<string> SortDirList( List<string> lstDirs )
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
		public static string MakeCleanJsString( string s )
		{
			return s.Replace( "\\", "\\\\" )
					.Replace( "&", "&amp;" )
					.Replace( "\u2028", "" )
					.Replace( "\u2029", "" )
					.Replace( "\u0004", "" );
		}

		// Test string for matches against a wildcard pattern. Use ? and * as wildcards. (Wrapper around RegEx)
		public static bool IsWildcardMatch( String wildcard, String text, bool casesensitive )
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

		public static int ToUnixTimestamp( DateTime value )
		{
			return (int)Math.Truncate( ( value.ToUniversalTime().Subtract( new DateTime( 1970, 1, 1 ) ) ).TotalSeconds );
		}

		public static long ParseLong(string s)
		{
			long num;
			if( Int64.TryParse( s, out num ) )
			{
				return num;
			}
			return 0;
		}
	}
}
