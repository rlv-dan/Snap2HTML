using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Runtime.Serialization.Json;
using System.Security.Principal;
using System.Text;
using System.Text.RegularExpressions;
using System.Windows.Forms;

namespace Snap2HTML
{
	public static class Utils
	{
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

        public static string BytesToFilesize(long bytes)
        {
            double kilobyte = 1024;
            double megabyte = kilobyte * 1024;
            double gigabyte = megabyte * 1024;
            double terabyte = gigabyte * 1024;

            if ((bytes >= 0) && (bytes < kilobyte))
                return bytes + " bytes";
            else if ((bytes >= kilobyte) && (bytes < megabyte))
                return Math.Round(bytes / kilobyte, 0) + " KB";
            else if ((bytes >= megabyte) && (bytes < gigabyte))
                return Math.Round((bytes / megabyte), 1) + " MB";
            else if ((bytes >= gigabyte) && (bytes < terabyte))
                return Math.Round((bytes / gigabyte), 2) + " GB";
            else if (bytes >= terabyte)
                return Math.Round((bytes / terabyte), 2) + " TB";
            else
                return bytes + " bytes";
        }

        // From https://stackoverflow.com/a/67876947/1087811
        public static string ShortenPath(string s_Path, Font i_Font, int s32_Width)
        {
            TextRenderer.MeasureText(s_Path, i_Font, new Size(s32_Width, 100),
                                     TextFormatFlags.PathEllipsis | TextFormatFlags.ModifyString);

            // Windows inserts a '\0' character into the string instead of shortening the string
            int s32_Nul = s_Path.IndexOf((char)0);
            if (s32_Nul > 0)
                s_Path = s_Path.Substring(0, s32_Nul);
            return s_Path;
        }

        public static string GetTemplatePath()
        {
            return System.IO.Path.GetDirectoryName(System.Windows.Forms.Application.ExecutablePath) + System.IO.Path.DirectorySeparatorChar + "template.html";
        }

        public static bool IsAdministrator()
        {
            using (WindowsIdentity identity = WindowsIdentity.GetCurrent())
            {
                WindowsPrincipal principal = new WindowsPrincipal(identity);
                return principal.IsInRole(WindowsBuiltInRole.Administrator);
            }
        }

        /// <summary>
        /// Converts the given decimal number to the numeral system with the
        /// specified radix (in the range [2, 36]).
        /// </summary>
        /// <param name="decimalNumber">The number to convert.</param>
        /// <param name="radix">The radix of the destination numeral system (in the range [2, 36]).</param>
        /// <returns>The number converted to the specified radix</returns>
        /// <remarks>Source: https://stackoverflow.com/a/10981113/1087811</remarks>
        public static string DecimalToArbitrarySystem(long decimalNumber, int radix)
        {
            const int BitsInLong = 64;
            const string Digits = "0123456789ABCDEFGHIJKLMNOPQRSTUVWXYZ";

            if (radix < 2 || radix > Digits.Length)
                throw new ArgumentException("The radix must be >= 2 and <= " + Digits.Length.ToString());

            if (decimalNumber == 0)
                return "0";

            int index = BitsInLong - 1;
            long currentNumber = Math.Abs(decimalNumber);
            char[] charArray = new char[BitsInLong];

            while (currentNumber != 0)
            {
                int remainder = (int)(currentNumber % radix);
                charArray[index--] = Digits[remainder];
                currentNumber = currentNumber / radix;
            }

            string result = new String(charArray, index + 1, BitsInLong - index - 1);
            if (decimalNumber < 0)
            {
                result = "-" + result;
            }

            return result;
        }

        public static string PathToFileUri(string path)
        {
            /*
	            Notes from Wikipedia:
	            A valid file URI must begin with either 
		            file:/path (no hostname)
		            file:///path (empty hostname)
		            file://hostname/path
	            There are two ways that Windows UNC filenames (such as \\server\folder\data.xml) can be represented. 
		            2-slash format:	file://server/folder/data.xml
		            4-slash format: file:////server/folder/data.xml
		            Microsoft .NET generally uses the 2-slash form
	            file://path (i.e. two slashes, without a hostname) is never correct
	            Characters
                    The forward slash is a general, system-independent way of separating the parts
	                The colon should be used, and should not be replaced with a vertical bar
	                Characters such as the hash (#) or question mark (?) which are part of the filename should be percent-encoded.
	                Characters which are not allowed in URIs, but which are allowed in filenames, must also be percent-encoded. For example, any of "{}`^ " and all control characters.
	                Characters which are allowed in both URIs and filenames must NOT be percent-encoded.
             */

            // Based on https://stackoverflow.com/a/74852300/1087811

            path = path.Replace("%", $"%{(int)'%':X2}")
                       .Replace("[", $"%{(int)'[':X2}")
                       .Replace("]", $"%{(int)']':X2}");

            var uri = new UriBuilder()
            {
                Scheme = "file",
                Host = string.Empty,
                Path = path
            }.Uri;
            
            return uri.AbsoluteUri;
        }

        // Natural Sort
        // from https://stackoverflow.com/a/22323356/1087811
        public static IEnumerable<T> OrderByNatural<T>(this IEnumerable<T> items, Func<T, string> selector)
        {
            var regex = new Regex(@"\d+", RegexOptions.Compiled);

            int maxDigits = items
                          .SelectMany(i => regex.Matches(selector(i)).Cast<Match>().Select(digitChunk => (int?)digitChunk.Value.Length))
                          .Max() ?? 0;

            return items.OrderBy(i => regex.Replace(selector(i), match => match.Value.PadLeft(maxDigits, '0')), StringComparer.CurrentCultureIgnoreCase);
        }


        public static string ToJSON<T>(T Obj)
        {
            using (var ms = new MemoryStream())
            {
                DataContractJsonSerializer serialiser = new DataContractJsonSerializer(typeof(T));
                serialiser.WriteObject(ms, Obj);
                byte[] json = ms.ToArray();
                return Encoding.UTF8.GetString(json, 0, json.Length);
            }
        }

        public static T FromJSON<T>(string Json)
        {
            using (var ms = new MemoryStream(Encoding.UTF8.GetBytes(Json)))
            {
                DataContractJsonSerializer serialiser = new DataContractJsonSerializer(typeof(T));
                var deserializedObj = (T)serialiser.ReadObject(ms);
                return deserializedObj;
            }
        }
    }
}
