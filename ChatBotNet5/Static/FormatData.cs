﻿
using ChatBot_Net5.Enum;
using ChatBot_Net5.Systems;

using System;
using System.Collections.Generic;
using System.Globalization;

namespace ChatBot_Net5.Static
{
    // specific partial class for the common messages processes
    public static class FormatData
    {
        /// <summary>
        /// Takes the incoming string integer and determines plurality >1 and returns the appropriate word, e.g. 1 viewers [sic] => 1 viewer.
        /// </summary>
        /// <param name="src">Representative number</param>
        /// <param name="single">Singular version of the word to return</param>
        /// <param name="plural">Plural version of the word to return</param>
        /// <returns>The source number and the version of word to match the plurality of src.</returns>
        internal static string Plurality(string src, MsgVars msgVars, string Prefix = null, string Suffix = null)
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
        internal static string Plurality(int src, MsgVars msgVars, string Prefix = null, string Suffix = null)
        {
            string[] plural = LocalizedMsgSystem.GetVar(msgVars).Split(',');
            return src.ToString(CultureInfo.CurrentCulture) + " " + (Prefix + " ") ?? string.Empty + (src != 1 ? plural[1] : plural[0]) + (" " + Suffix) ?? string.Empty;
        }

        /// <summary>
        /// Takes the incoming integer and determines plurality >1 and returns the appropriate word, e.g. 1 viewers [sic], 1 viewer.
        /// </summary>
        /// <param name="src">Representative number</param>
        /// <param name="single">Singular version of the word to return</param>
        /// <param name="plural">Plural version of the word to return</param>
        /// <returns>The source number and the version of word to match the plurality of src.</returns>
        private static string Plurality(int src, string singular, string plural)
        {
            return src.ToString(CultureInfo.CurrentCulture) + " " + (src != 1 ? plural : singular);
        }

        /// <summary>
        /// Converts the elements of a timespan into a formatted string.
        /// </summary>
        /// <param name="timeSpan">The timespan containing the data to format.</param>
        /// <returns>A string from <paramref name="timeSpan"/> data with 'x days, y hours, and z minutes'; and the time unit is adjusted to plurality, e.g. 0 days, 1 day, 2 days etc</returns>
        internal static string FormatTimes(TimeSpan timeSpan)
        {
            string output = "";

            Dictionary<string, int> datakeys = new()
            {
                { "day", timeSpan.Days },
                { "hour", timeSpan.Hours },
                { "minute", timeSpan.Minutes }
            };

            foreach (string k in datakeys.Keys)
            {
                if (datakeys[k] != 0)
                {
                    output += Plurality(datakeys[k], k, k + "s") + ", ";
                }
            }

            return output.LastIndexOf(',') == -1 ? "no time available" : output.Remove(output.LastIndexOf(','));
        }

        /// <summary>
        /// Calculates difference between a past date and DateTime.Now.
        /// </summary>
        /// <param name="pastdate">the historic date of some activity</param>
        /// <returns>the elapsed time since the historic date to now</returns>
        internal static string FormatTimes(DateTime pastdate)
        {
            return FormatTimes(DateTime.Now - pastdate);
        }
    }
}
