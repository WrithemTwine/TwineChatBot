using StreamerBotLib.Enums;
using StreamerBotLib.Properties;

using System;
using System.Collections.Generic;

namespace StreamerBotLib.Static
{
    /// <summary>
    /// Provides dictionaries to use for parsing user provided parse strings and to provide common dictionary structure
    /// </summary>
    public static class VariableParser
    {
        public static readonly string Prefix = "#";

        public static string ConvertVars(MsgVars[] msgVars)
        {
            string x = "";

            foreach (MsgVars m in msgVars)
            {
                x += Prefix + m.ToString() + ",";
            }

            return x.Remove(x.LastIndexOf(','));
        }

        /// <summary>
        /// Build a dictionary with the incoming string,string data pair, add a prefix to the first string.
        /// </summary>
        /// <param name="paramArray">A Tuple with the first item is a key (which will be prefixed) and second item is the value.</param>
        /// <returns>A key,value Dictionary with the specified prefix.</returns>
        public static Dictionary<string, string> BuildDictionary<T>(Tuple<T, string>[] paramArray)
        {
            Dictionary<string, string> dict = new();

            foreach (Tuple<T, string> t in paramArray)
            {
                dict.Add(Prefix + t.Item1.ToString(), t.Item2);
            }

            return dict;
        }

        /// <summary>
        /// Modifies an existing variable parsing dictionary to add datapoints with a prefix to the first item in the supplied Tuples.
        /// </summary>
        /// <param name="dictionary">The dictionary to modify.</param>
        /// <param name="paramArray">The Tuple array to add to the dictionary.</param>
        public static void AddData<T>(ref Dictionary<string, string> dictionary, Tuple<T, string>[] paramArray)
        {
            foreach (Tuple<T, string> t in paramArray)
            {
                if (t.Item2 != null)
                {
                    dictionary.Add(Prefix + t.Item1.ToString(), t.Item2);
                }
            }
        }

        /// <summary>
        /// Replace in a message the keys from a dictionary for the matching values, must begin with the key token
        /// </summary>
        /// <param name="message">The message to replace, may contain usage variables.</param>
        /// <param name="dictionary">The dictionary containing the parse keys.</param>
        /// <returns>The message replaced with the dictionary key,value pairs.</returns>
        public static string ParseReplace(string message, Dictionary<string, string> dictionary, bool HTMLMarkup = false)
        {
            string Markup(string variable, string value)
            {
                return $"<span class=\"{variable[1..]}\">{value}</span>";
            }

            if (message != null)
            {
                foreach (string k in dictionary.Keys)
                {
                    if (message.Contains(k))
                    {
                        message = message.Replace(k,
                            (HTMLMarkup ?

                            Markup(k, (k == Prefix + MsgVars.url.ToString() ? Resources.TwitchHomepage : "") + // prefix URL with Twitch URL
                            dictionary[k]) :

                            (k == Prefix + MsgVars.user.ToString() ? "@" : "") +  // prefix username with @
                            (k == Prefix + MsgVars.url.ToString() ? Resources.TwitchHomepage : "") + // prefix URL with Twitch URL
                            dictionary[k]));
                    }
                }
            }
            return message ?? "";
#if OLD_CODE
            string temp = ""; // build the message to return

            string[] words = message.Split(' ');    // tokenize the message by ' ' delimiters

            // submethod to replace the found key with paired value
            string Rep(string key)
            {
                int prefcount = key.Split(Prefix).Length;
                while (prefcount > 0) // count and loop through the number of prefixes, sometimes there's a prefix in the message but not meant to exchange a variable. with just one prefix and no exchange, this becomes an infinite loop.
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

                    key = dictionary.TryGetValue(hit, out string value) ?
                        key.Replace(hit,
                        (hit == Prefix + MsgVars.user.ToString() ? "@" : "") +  // prefix username with @
                        (hit == Prefix + MsgVars.url.ToString() ? Resources.TwitchHomepage : "") + // prefix URL with Twitch URL
                        value)
                        : key;
                    prefcount--;
                }
                return key;
            }

            // review the incoming string message for all of the keys in the dictionary, replace with paired value
            for (int x = 0; x < words.Length; x++)
            {
                temp += (words[x].StartsWith(Prefix, StringComparison.CurrentCulture) && dictionary != null ? Rep(words[x]) : words[x]) + " ";
            }

            return temp.Trim();
#endif
        }
    }
}
