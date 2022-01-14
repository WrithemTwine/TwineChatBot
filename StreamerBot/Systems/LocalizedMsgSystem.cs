using StreamerBot.Culture;
using StreamerBot.Data;
using StreamerBot.Enum;
using StreamerBot.Interfaces;
using StreamerBot.Models;
using StreamerBot.Properties;
using StreamerBot.Static;

using System;
using System.Collections.Generic;
using System.Globalization;
using System.Reflection;
using System.Resources;

namespace StreamerBot.Systems
{
    /// <summary>
    /// Class to manage event messages - donated bits, subscriptions, incoming raid, etc
    /// Culture localized strings
    /// </summary>
    public static class LocalizedMsgSystem
    {
        /* //TODO: finish updating LocalizedMsgSystem description of how and where Enum & messages are used
         * 
        --------------------

        adding new languages as "Culture/Msgs.{language designation}.resx" and translating the original "Msgs.resx" strings, will provide the localized language strings

        --------------------
        enum: MsgVars
        members: included within a phrase as variables to customize the message with contextual data
        GetVar<T>(T msgVars) := 
        - except for Total & "Plural"+variable_name, currently not implemented; variables are in English so as not to complicate the message. currently, the variables can be in a non-English message and will provide the data based on the input data (like an English user name returned, a Japanese written user name returned => for the "#user" variable)
        - "Plural"+variable_name permits number recognition nounds, like 1 month or 2 months.
        - "Total" is used when discussing within a message; e.g. "2 total months" of a subscription.

        GetCommandHelp() := utilizes MsgVars enum, prefixes "Help" to each enum. When defined in localized strings, provides the help description for each variable. Returns the descriptors for all defined variables.
        --------------------
        enum: ChannelEventActions
        members: defined as what happens in Twitch during a stream when viewers interact with the stream, such as joining the channel, gifting subs and bits, follow, hosting the channel, incoming raid, and when the stream goes live.

        GetEventMsg(...)
        -looks up if the event message is enabled within the datatables - the user can disable the message
        -uses the ChannelEventActions enum, retrieves the message from the database and if unavailable, prefixes "Msg" to the enum and retrieves the string message from the localized strings table.
        The event messages are used to respond to the event during a Twitch stream and are the default messages added to the database. The variables need to remain unchanged.
        --------------------
        enum: DefaultCommand
        members: the default commands the bot supports before users add additional custom commands.

        GetVar<T>(T msgVars) :=
        - straight return the command name from the string resource list

        GetDefaultComMsg(DefaultCommand defaultCommand)
        - prepends "Msg" to the command name and retrieves a default message as a localized string
        - used to populate the 'data manager commands' table upon starting a new chat bot
        

        --------------------
        GetTwineBotAuthorInfo() - retrieves specific message about the chat bot author, can be translated, but leave " WrithemTwine ", " #url#author ", and " {0}.{1}.{2}.{3} " intact
        --------------------

         * 
         */

        private static IDataManageReadOnly _datamanager;
        private static readonly ResourceManager RM = Msgs.ResourceManager;

        /// <summary>
        /// Set the DataManager to use for extracting event messages
        /// </summary>
        public static void SetDataManager(DataManager dataManager)
        {
            _datamanager = dataManager;
        }

        /// <summary>
        /// If for some reason the data table fails and doesn't return a message, provide default user defined messages. Adjusted for current language culture
        /// </summary>
        /// <param name="dataRetrieve">Retrieve the message or if event is enabled</param>
        /// <param name="channelEventActions">Which event message to retrieve</param>
        /// <returns>A string containing variables to customize the event message.</returns>
        public static string GetEventMsg(ChannelEventActions channelEventActions, out bool Enabled, out short Multi)
        {
            Enabled = (bool)(_datamanager.GetRowData(DataRetrieve.EventEnabled, channelEventActions) ?? false);
            Multi = (short)(_datamanager.GetRowData(DataRetrieve.EventRepeat, channelEventActions) ?? short.Parse("0"));

            return (string)_datamanager.GetRowData(DataRetrieve.EventMessage, channelEventActions)
                ?? RM.GetString("Msg" + channelEventActions.ToString(), CultureInfo.CurrentCulture);
        }

        /// <summary>
        /// Retreives localized strings for the default commands.
        /// </summary>
        /// <param name="defaultCommand">The command to retrieve.</param>
        /// <returns>The localized command message string.</returns>
        public static string GetDefaultComMsg(DefaultCommand defaultCommand)
        {
            return RM.GetString("Msg" + defaultCommand.ToString(), CultureInfo.CurrentCulture);
        }

        public static string GetDefaultComParam(DefaultCommand defaultCommand)
        {
            return RM.GetString("Param" + defaultCommand.ToString(), CultureInfo.CurrentCulture);
        }

        public static string GetDefaultComParam(string defaultCommand)
        {
            return RM.GetString("Param" + defaultCommand, CultureInfo.CurrentCulture);
        }

        public static string GetVar<T>(T msgVars)
        {
            return RM.GetString(msgVars.ToString(), CultureInfo.CurrentCulture);
        }

        public static bool CheckDefaultCommand(string Command)
        {
            bool result = false;

            foreach (DefaultCommand d in System.Enum.GetValues(typeof(DefaultCommand)))
            {
                if(Command == GetVar(Command))
                {
                    result = true;
                    break;
                }
            }

            return result;
        }

        /// <summary>
        /// Specifically builds the message used for announcing Twine Chatbot connected to chat.
        /// </summary>
        /// <returns>A localized string message for announcing the bot connection message.</returns>
        public static string GetTwineBotAuthorInfo()
        {
            return VariableParser.ParseReplace(
                Msgs.TwineBotInfo,
                new Dictionary<string, string>() {
                    { VariableParser.Prefix + "url", Resources.AuthorTwitch},
                    { VariableParser.Prefix + "author", Resources.AuthorTwitch}
                }
                );
        }

        public static string DetermineHost(bool Hosting)
        {
            return Hosting ? Msgs.autohost : Msgs.host;
        }

        public static List<Command> GetCommandHelp()
        {
            List<Command> temp = new();

            foreach (MsgVars a in System.Enum.GetValues(typeof(MsgVars)))
            {
                try // not every MsgVars has a help available
                {
                    string helpvalue = RM.GetString("Help" + a.ToString(), CultureInfo.CurrentCulture);

                    if (helpvalue != null)
                    {
                        temp.Add(new() { Parameter = VariableParser.Prefix + a.ToString(), Value = helpvalue });
                    }
                }
                // catch the 'MissingManifestResourceException' where no resource contains the desired string
                catch (Exception ex)
                {
                    LogWriter.LogException(ex, MethodBase.GetCurrentMethod().Name);
                }
            }

            return temp;
        }
    }
}
