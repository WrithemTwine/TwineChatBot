
using System;
using System.Collections.Generic;
using System.Globalization;

namespace ChatBot_Net5.Static
{
    // specific partial class for the common messages processes
    public static class FormatData
    {
        /// <summary>
        /// Takes the incoming string integer and determines plurality >1 and returns the appropriate word, e.g. 1 viewers [sic], 1 viewer.
        /// </summary>
        /// <param name="src">Representative number</param>
        /// <param name="single">Singular version of the word to return</param>
        /// <param name="plural">Plural version of the word to return</param>
        /// <returns>The source number and the version of word to match the plurality of src.</returns>
        internal static string Plurality(string src, string single, string plural)
        {
            return Plurality(Convert.ToInt32(src, CultureInfo.CurrentCulture), single, plural);
        }

        /// <summary>
        /// Takes the incoming integer and determines plurality >1 and returns the appropriate word, e.g. 1 viewers [sic], 1 viewer.
        /// </summary>
        /// <param name="src">Representative number</param>
        /// <param name="single">Singular version of the word to return</param>
        /// <param name="plural">Plural version of the word to return</param>
        /// <returns>The source number and the version of word to match the plurality of src.</returns>
        internal static string Plurality(int src, string single, string plural)
        {
            return src.ToString(CultureInfo.CurrentCulture) + " " + (src != 1 ? plural : single);
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

            foreach(string k in datakeys.Keys)
            {
                if(datakeys[k] != 0)
                {
                    output += Plurality(datakeys[k], k, k + "s") + ", ";
                }
            }

            return output.LastIndexOf(',')==-1 ? "no time available" : output.Remove(output.LastIndexOf(','));
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
