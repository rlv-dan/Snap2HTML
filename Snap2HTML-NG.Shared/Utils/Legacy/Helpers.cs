using System;
using System.Collections.Generic;
using System.Text;
using System.Text.RegularExpressions;

namespace Snap2HTMLNG.Shared.Utils
{
    /// <summary>
    /// <para>Legacy Utilities.  This are taken from Snap2HTML's original Source Code but may still be in used.</para>
    /// <para>They have had updated comments added and some optimizations made, but should not be used for new features in Snap2HTML-NG.</para>
    /// </summary>
    public static class Helpers
    {
        /// <summary>
        /// Sorts Directories Correctly, even if they contain a space or period.
        /// </summary>
        /// <param name="lstDirs">List of Directories in <c>List&lt;string&gt;</c> format</param>
        /// <returns></returns>
        public static List<string> SortDirList(List<string> lstDirs)
        {
            for (int n = 0; n < lstDirs.Count; n++)
            {
                lstDirs[n] = lstDirs[n].Replace(" ", "1|1");
                lstDirs[n] = lstDirs[n].Replace(".", "2|2");
            }
            lstDirs.Sort();
            for (int n = 0; n < lstDirs.Count; n++)
            {
                lstDirs[n] = lstDirs[n].Replace("1|1", " ");
                lstDirs[n] = lstDirs[n].Replace("2|2", ".");
            }
            return lstDirs;
        }

        /// <summary>
        /// Replaces Characters that may appear in Filenames and Paths that have a special meaning in Javascript.
        /// <para>Info on u2028/u2029: <see href="https://en.wikipedia.org/wiki/Newline#Unicode"/></para>
        /// </summary>
        /// <param name="s">The Character</param>
        /// <returns>
        /// Cleaned Character
        /// </returns>
        public static string MakeCleanJsString(string s)
        {
            return s.Replace("\\", "\\\\")
                    .Replace("&", "&amp;")
                    .Replace("\u2028", "")
                    .Replace("\u2029", "")
                    .Replace("\u0004", "");
        }

        /// <summary>
        /// Tests a string against a wildacrd pattern.  Use ? and * as wildcards.
        /// </summary>
        /// <param name="wildcard">The wild card, <c>?</c> or <c>*</c></param>
        /// <param name="text">The text to match</param>
        /// <param name="isCaseSensitive"><c>true</c> or <c>false</c> for case sensitivity</param>
        /// <returns>
        /// <c>true</c> or <c>false</c>
        /// </returns>
        public static bool IsWildcardMatch(string wildcard, string text, bool isCaseSensitive)
        {
            StringBuilder sb = new StringBuilder(wildcard.Length + 10);
            sb.Append("^");
            for (int i = 0; i < wildcard.Length; i++)
            {
                char c = wildcard[i];
                switch (c)
                {
                    case '*':
                        sb.Append(".*");
                        break;
                    case '?':
                        sb.Append(".");
                        break;
                    default:
                        sb.Append(Regex.Escape(wildcard[i].ToString()));
                        break;
                }
            }
            sb.Append("$");
            Regex regex = isCaseSensitive
                ? new Regex(sb.ToString(), options: RegexOptions.None)
                : new Regex(sb.ToString(), options: RegexOptions.IgnoreCase);

            return regex.IsMatch(text);
        }

        /// <summary>
        /// Converts a <c>DateTime</c> value to Unix Timestamp
        /// </summary>
        /// <param name="value"></param>
        /// <returns>
        /// Unix Timestamp <see href="https://en.wikipedia.org/wiki/Unix_time"/>
        /// </returns>
        public static int ToUnixTimestamp(DateTime value)
        {
            return (int)Math.Truncate(value.ToUniversalTime().Subtract(new DateTime(1970, 1, 1)).TotalSeconds);
        }

        public static long ParseLong(string s)
        {
            return Int64.TryParse(s, out long num) ? num : 0;
        }
    }
}
