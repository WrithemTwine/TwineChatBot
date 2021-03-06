﻿using ChatBot_Net5.Enum;
using ChatBot_Net5.Properties;

using System;
using System.Collections.Generic;

namespace ChatBot_Net5.Static
{
    /// <summary>
    /// Provides dictionaries to use for parsing user provided parse strings and to provide common dictionary structure
    /// </summary>
    public static class VariableParser
    {
        internal static string Prefix = "#";

        internal static string ConvertVars(MsgVars[] msgVars)
        {
            string x="";

            foreach(MsgVars m in msgVars)
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
        internal static Dictionary<string, string> BuildDictionary<T>(Tuple<T, string>[] paramArray)
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
        internal static void AddData<T>(ref Dictionary<string, string> dictionary, Tuple<T, string>[] paramArray)
        {
            foreach (Tuple<T, string> t in paramArray)
            {
                dictionary.Add(Prefix + t.Item1.ToString(), t.Item2);
            }
        }

        /// <summary>
        /// Replace in a message the keys from a dictionary for the matching values, must begin with the key token
        /// </summary>
        /// <param name="message">The message to replace, may contain usage variables.</param>
        /// <param name="dictionary">The dictionary containing the parse keys.</param>
        /// <returns></returns>
        internal static string ParseReplace(string message, Dictionary<string, string> dictionary)
        {
            string temp = ""; // build the message to return

            string[] words = message.Split(' ');    // tokenize the message by ' ' delimiters

            // submethod to replace the found key with paired value
            string Rep(string key)
            {
                while (key.Contains(Prefix))
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
                }
                return key;
            }

            // review the incoming string message for all of the keys in the dictionary, replace with paired value
            for (int x = 0; x < words.Length; x++)
            {
                temp += (words[x].StartsWith(Prefix, StringComparison.CurrentCulture) ? Rep(words[x]) : words[x]) + " ";
            }

            return temp.Trim();
        }
    }
}
