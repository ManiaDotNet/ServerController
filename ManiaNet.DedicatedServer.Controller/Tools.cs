using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace ManiaNet.DedicatedServer.Controller
{
    public static class Tools
    {
        /// <summary>
        /// Formats a given int of milliseconds to a displayable record like 43512 -> 0:43.51
        /// </summary>
        /// <param name="time">The time in milliseconds</param>
        /// <param name="format">Optinally a different format, e.g. to add a third thausandth</param>
        /// <param name="sign">Add a + or a - in front of the output</param>
        /// <returns></returns>
        public static string FormatMilliseconds(int time, string format = "mm:ss.ff", bool sign = false) {
            string result = "";
            bool negative = time < 0;
            result = TimeSpan.FromMilliseconds(Math.Abs(time)).ToString(format);
            if (result[0] == '0')
                result = result.Substring(1);
            if (sign)
                result = (negative ? "-" : "+") + result;
            return result;
        }

        /// <summary>
        /// Removes Maniaplanet/TMF text formatting
        /// </summary>
        /// <param name="input">String to remove formatting from</param>
        /// <param name="level">c - colors, l - links, f - format (bold, italic, ...), a - all
        /// Combinations possible</param>
        /// <returns></returns>
        public static string StripFormatting(string input, char[] level)
        {
            string result = input;
            if (level.Contains('c') || level.Contains('a'))
                result = Regex.Replace(result, @"\$([0-9a-fA-F]{3}|g)", "");
            if (level.Contains('l') || level.Contains('a'))
                result = Regex.Replace(result, @"\$(l|h|p)(\[.*?\])?(.*?)\$\1", "$3");
            if (level.Contains('f') || level.Contains('a'))
                result = Regex.Replace(result, @"\$[nwmoiszt]", "");
            return result;
        }
    }
}
