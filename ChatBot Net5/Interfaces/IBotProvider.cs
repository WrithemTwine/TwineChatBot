using ChatBot_Net5.Properties;
using System;
using System.Collections.Generic;

namespace ChatBot_Net5.Interfaces
{
    /// <summary>
    /// The different types of BotLevels used from a data provider to define bot message limits.
    /// </summary>
    public enum BotLevelType {Public, Whisper, Platform, Channel, Team};


    // Define each supported Bot Provider to plug into the application
   public interface IBotProvider
    {
        /// <summary>
        /// Property stating the Provider Name for using with the Chat Bot
        /// </summary>
        string ProviderName { get; set; }

        /// <summary>
        /// Key,Value Dictionary specifying user access levels
        /// </summary>
        /// <returns>Types of users to determine if command is available to user.</returns>
        Dictionary<string, string> GetUserLevels();

        /// <summary>
        /// Get URI connection data
        /// </summary>
        /// <returns>The connection URI for this provider</returns>
        Uri GetProviderURI();

        /// <summary>
        /// Get Provider Commands - commands platform supports
        /// </summary>
        /// <returns>Dictionary<string,string> object with commands the provider supports</returns>
        Dictionary<string, string> GetProviderCommands();

        /// <summary>
        /// Get Bot message limits - defines time limits for bot levels: {status name}:{message number limit in interval}:{interval defined}
        /// </summary>
        /// <returns>List<BotLevels> object with the different bot message limit details.</returns>
        List<BotLevel> GetBotLevels();
    }

    public class BotLevel : IEqualityComparer<BotLevel>
    {
        public BotLevelType LevelType { get; set; }                     // the type of BotLevel - if it's public or private/whisper
        public string LevelName { get; set; } = "none";     // the name of the bot message leve
        public int MessageLimitCount { get; set; } = 0;     // the number of bot messages allowed in a time limit
        public TimeSpan MessageSendDuration { get; set; } = TimeSpan.Zero;   // specify the time limit for a number of messages

        /// <summary>
        /// Determine if two Bot Level objects are equal.
        /// </summary>
        /// <param name="x">First BotLevel object to compare.</param>
        /// <param name="y">Second BotLevel object to compare</param>
        /// <returns>Whether every matching property is equal between the objects.</returns>
        public bool Equals(BotLevel x, BotLevel y)
        {
            return (x?.LevelType == y?.LevelType) 
                && (x?.LevelName == y?.LevelName) 
                && (x?.MessageLimitCount == y?.MessageLimitCount) 
                && (x?.MessageSendDuration == y?.MessageSendDuration);
        }

        /// <summary>
        /// Returns the HashCode for this object.
        /// </summary>
        /// <param name="obj">Object to convert to hash code.</param>
        /// <returns>HashCode for the supplied object</returns>
        public int GetHashCode(BotLevel obj)
        {
            if (obj == null) { throw new ArgumentNullException( Resources.UDC_ParamName, Resources.UDC_ParamException ); }
            return $"{obj.LevelType}{obj.LevelName}{obj.MessageLimitCount}{obj.MessageSendDuration}".GetHashCode();
        }
    }

}
