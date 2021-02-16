using System;
using System.Collections.Generic;
using System.Globalization;

namespace ChatBot_Net5.BotIOController
{
    // specific partial class for the common messages processes
    public sealed partial class BotController
    {
        internal static string ParseReplace(string message, Dictionary<string,string> dictionary)
        {
            string temp = "";

            string[] words = message.Split(' ');


            string Rep(string key)
            {
                dictionary.TryGetValue(key, out string value);
                return value ?? "";
            }

            for(int x=0; x<words.Length; x++)
            {
                temp += words[x].StartsWith("#", System.StringComparison.CurrentCulture) ? Rep(words[x]) : words[x];
            }

            return temp;
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
            return src.ToString(CultureInfo.CurrentCulture) + " " + (src > 1 ? plural : single);
        }

    }
}
