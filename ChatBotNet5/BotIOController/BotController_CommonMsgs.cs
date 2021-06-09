using System;
using System.Collections.Generic;
using System.Globalization;

namespace ChatBot_Net5.BotIOController
{
    // specific partial class for the common messages processes
    public sealed partial class BotController
    {
        private const string codekey = "#";

        /// <summary>
        /// Replace in a message the keys from a dictionary for the matching values, must begin with the key token
        /// </summary>
        /// <param name="message"></param>
        /// <param name="dictionary"></param>
        /// <returns></returns>
        internal static string ParseReplace(string message, Dictionary<string, string> dictionary)
        {
            string temp = ""; // build the message to return

            string[] words = message.Split(' ');    // tokenize the message by ' ' delimiters

            // submethod to replace the found key with paired value
            string Rep(string key)
            {
                string hit = "";

                foreach (string s in dictionary.Keys)
                {
                    if (key.Contains(s))
                    {
                        hit = s;
                        break;
                    }
                }

                // if the value doesn't work, return the key used to try to parse - the intended phrase wasn't part of the parsing values
                return dictionary.TryGetValue(hit, out string value) ? key.Replace(hit, (hit == codekey+"user" ? "@" : "") + value) : key;
            }

            // review the incoming string message for all of the keys in the dictionary, replace with paired value
            for (int x = 0; x < words.Length; x++)
            {
                temp += (words[x].StartsWith(codekey, StringComparison.CurrentCulture) ? Rep(words[x]) : words[x]) + " ";
            }

            return temp.Trim();
        }

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
