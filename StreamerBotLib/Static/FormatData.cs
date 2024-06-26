﻿
using StreamerBotLib.Enums;
using StreamerBotLib.Systems;

using System.Globalization;
using System.Text;

namespace StreamerBotLib.Static
{
    // specific partial class for the common messages processes
    public static class FormatData
    {
        /// <summary>
        /// Provide a plurality-enabled version of a word based on a certain number. Such as 
        /// "there 'is' 1 bag." "there 'are' 2 bags."
        /// </summary>
        /// <param name="src">Contains the number as a string to generate the context-sensitive word.</param>
        /// <param name="msgVars">A plural word pair defined value ["singular", "plural"] to find in Msgs.resx.</param>
        /// <returns>The plural context of the provided words based on number.</returns>
        public static string PluralityOnlyWord(string src, MsgVars msgVars)
        {
            return PluralityOnlyWord(Convert.ToInt32(src), msgVars);
        }

        /// <summary>
        /// Provide a plurality-enabled version of a word based on a certain number. Such as 
        /// "there 'is' 1 bag." "there 'are' 2 bags."
        /// </summary>
        /// <param name="src">Contains the number as an int to generate the context-sensitive word.</param>
        /// <param name="msgVars">A plural word pair defined value ["singular", "plural"] to find in Msgs.resx.</param>
        /// <returns>The plural context of the provided words based on number.</returns>
        public static string PluralityOnlyWord(int src, MsgVars msgVars)
        {

            return PluralityOnlyWord(src, msgVars);
        }

        /// <summary>
        /// Provide a plurality-enabled version of a word based on a certain number. Such as 
        /// "there 'is' 1 bag." "there 'are' 2 bags."
        /// </summary>
        /// <param name="src">Contains the number as a double to generate the context-sensitive word.</param>
        /// <param name="msgVars">A plural word pair defined value ["singular", "plural"] to find in Msgs.resx.</param>
        /// <returns>The plural context of the provided words based on number.</returns>
        public static string PluralityOnlyWord(double src, MsgVars msgVars)
        {
            string[] Plural = LocalizedMsgSystem.GetVar(msgVars).Split(',');

            return Plural[src == 1.0 ? 0 : 1];
        }

        /// <summary>
        /// Takes the incoming string integer and determines plurality >1 and returns the appropriate word, e.g. 1 viewers [sic] => 1 viewer.
        /// </summary>
        /// <param name="src">Representative number</param>
        /// <param name="single">Singular version of the word to return</param>
        /// <param name="plural">Plural version of the word to return</param>
        /// <returns>The source number and the version of word to match the plurality of src.</returns>
        public static string Plurality(string src, MsgVars msgVars, string Prefix = null, string Suffix = null)
        {
            return Plurality(Convert.ToInt32(src, CultureInfo.CurrentCulture), msgVars, Prefix, Suffix);
        }

        /// <summary>
        /// Takes the incoming integer and determines plurality >1 and returns the appropriate word, e.g. 1 viewers [sic] => 1 viewer in the Cultural Language format.
        /// </summary>
        /// <param name="src">Representative number</param>
        /// <param name="msgVars">Specifies which resource string to use</param>
        /// <param name="Prefix">The string to add to the beginning of the determined phrase</param>
        /// <param name="Suffix">The string to add to the end of the determined phrase</param>
        /// <returns>a string containing the culture adjusted plurality of the supplied number</returns>
        public static string Plurality(int src, MsgVars msgVars, string Prefix = null, string Suffix = null)
        {
            return Plurality((double)src, msgVars, Prefix, Suffix);
        }

        /// <summary>
        /// Takes the incoming integer and determines plurality >1 and returns the appropriate word, e.g. 1 viewers [sic] => 1 viewer in the Cultural Language format.
        /// </summary>
        /// <param name="src">Representative number</param>
        /// <param name="msgVars">Specifies which resource string to use</param>
        /// <param name="Prefix">The string to add to the beginning of the determined phrase</param>
        /// <param name="Suffix">The string to add to the end of the determined phrase</param>
        /// <returns>a string containing the culture adjusted plurality of the supplied number</returns>
        public static string Plurality(double src, MsgVars msgVars, string Prefix = null, string Suffix = null)
        {
            string[] plural = LocalizedMsgSystem.GetVar(msgVars).Split(',');

            StringBuilder sb = new();
            sb = sb.Append(string.Format(CultureInfo.CurrentCulture, src % 1 == 0 ? "{0:N0}" : "{0:N}", src));

            if (Prefix != null)
            {
                sb = sb.Append(' ' + Prefix);
            }

            sb = sb.Append(' ');

            sb = sb.Append(plural[src == 1.0 ? 0 : 1]);

            if (Suffix != null)
            {
                sb = sb.Append(Suffix + " ");
            }

            return sb.ToString();
        }

        /// <summary>
        /// Converts the elements of a timespan into a formatted string.
        /// </summary>
        /// <param name="timeSpan">The timespan containing the data to format.</param>
        /// <returns>A string from <paramref name="timeSpan"/> data with 'x days, y hours, and z minutes'; and the time unit is adjusted to plurality, e.g. 0 days, 1 day, 2 days etc</returns>
        public static string FormatTimes(TimeSpan timeSpan)
        {
            const double yeardays = 365.242;
            const double monthdays = yeardays / 12;

            string output = "";

            Dictionary<string, int> datakeys = new()
            {
                { "year", (int)Math.Floor(timeSpan.Days / yeardays) },
                { "month", (int)Math.Floor((timeSpan.Days % yeardays) / monthdays) },
                { "day", timeSpan.Days % (int)monthdays },
                { "hour", timeSpan.Hours },
                { "minute", timeSpan.Minutes }
            };

            foreach (string k in datakeys.Keys)
            {
                if (datakeys[k] != 0)
                {
                    output += Plurality(datakeys[k], (MsgVars)Enum.Parse(typeof(MsgVars), "Plural" + k)) + ", ";
                }
            }

            return string.Format(LocalizedMsgSystem.GetVar(MsgVars.or), Plurality(Math.Round(timeSpan.TotalHours, 2), MsgVars.Pluralhour), output.LastIndexOf(',') == -1 ? "no time available" : output.Remove(output.LastIndexOf(',')));
        }

        /// <summary>
        /// Calculates difference between a past date and DateTime.Now.ToLocalTime().
        /// </summary>
        /// <param name="pastdate">the historic date of some activity</param>
        /// <returns>the elapsed time since the historic date to now</returns>
        public static string FormatTimes(DateTime pastdate)
        {
            return FormatTimes(DateTime.Now.ToLocalTime() - pastdate.ToLocalTime());
        }

        public static string AddEscapeFormat(string SrcText)
        {
            return SrcText.Replace("'", "''");
        }

    }
}
