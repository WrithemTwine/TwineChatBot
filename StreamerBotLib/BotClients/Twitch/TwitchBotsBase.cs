﻿using StreamerBotLib.Static;

using System;

namespace StreamerBotLib.BotClients.Twitch
{
    /// <summary>
    /// Abstract base class for all Twitch type Bots
    /// </summary>
    public abstract class TwitchBotsBase : IOModule
    {
        /// <summary>
        /// App Client Id for bot account used in the connection, connects with Access Token
        /// </summary>
        public static string TwitchClientID
        {
            get => OptionFlags.TwitchTokenUseAuth ? OptionFlags.TwitchAuthClientId : OptionFlags.TwitchBotClientId; 
            set => OptionFlags.TwitchBotClientId = value;
        }

        /// <summary>
        /// Name of the Bot Account
        /// </summary>
        public static string TwitchBotUserName
        {
            get => OptionFlags.TwitchBotUserName; set => OptionFlags.TwitchBotUserName = value;
        }

        /// <summary>
        /// User Id of the Bot Account
        /// </summary>
        public static string TwitchBotUserId { get; set; }

        /// <summary>
        /// Channel name used in the connection
        /// </summary>
        public static string TwitchChannelName
        {
            get => OptionFlags.TwitchChannelName; set => OptionFlags.TwitchChannelName = value;
        }

        /// <summary>
        /// UserId of the Channel Name
        /// </summary>
        public static string TwitchChannelId { get; set; }

        /// <summary>
        /// Token used for the connection.
        /// </summary>
        public static string TwitchAccessToken
        {
            get => OptionFlags.TwitchTokenUseAuth ? OptionFlags.TwitchAuthBotAccessToken : OptionFlags.TwitchBotAccessToken;
            set
            {
                if (OptionFlags.TwitchTokenUseAuth)
                {
                    OptionFlags.TwitchAuthBotAccessToken = value;
                }
                else
                {
                    OptionFlags.TwitchBotAccessToken = value;
                }
            }
        }

        /// <summary>
        /// Refresh token used to generate a new access token.
        /// </summary>
        public static string TwitchRefreshToken
        {
            get => OptionFlags.TwitchTokenUseAuth ? OptionFlags.TwitchAuthBotRefreshToken : OptionFlags.TwitchRefreshToken;
            set
            {
                if (OptionFlags.TwitchTokenUseAuth)
                {
                    OptionFlags.TwitchAuthBotRefreshToken = value;
                }
                else
                {
                    OptionFlags.TwitchRefreshToken = value;
                }
            }
        }

        public static string TwitchStreamClientId
        {
            get => OptionFlags.TwitchTokenUseAuth ? OptionFlags.TwitchAuthStreamerClientId : OptionFlags.TwitchStreamClientId;
        }

        public static string TwitchStreamerAccessToken
        {
            get => OptionFlags.TwitchTokenUseAuth ? OptionFlags.TwitchAuthStreamerAccessToken : OptionFlags.TwitchStreamOauthToken;
            set
            {
                if (OptionFlags.TwitchTokenUseAuth)
                {
                    OptionFlags.TwitchAuthStreamerAccessToken = value;
                } else
                {
                    OptionFlags.TwitchStreamOauthToken = value;
                }
            }
        }

        public static string TwitchStreamerRefreshToken
        {
            get => OptionFlags.TwitchAuthStreamerRefreshToken;
            set => OptionFlags.TwitchAuthStreamerRefreshToken = value;
        }

        /// <summary>
        /// the date by which to generate/refresh a new access token.
        /// </summary>
        public static DateTime TwitchRefreshDate => OptionFlags.TwitchRefreshDate;

        /// <summary>
        /// The poll time in seconds to check the channel for new followers
        /// </summary>
        public static double TwitchFrequencyFollowerTime => OptionFlags.TwitchFrequencyFollow;

        /// <summary>
        /// The poll time in seconds to check for channel going live
        /// </summary>
        public static double TwitchFrequencyLiveNotifyTime => OptionFlags.TwitchGoLiveFrequency;

        /// <summary>
        /// The poll time in seconds to check the channel for new clips
        /// </summary>
        public static double TwitchFrequencyClipTime => OptionFlags.TwitchFrequencyClipTime;

        internal virtual void SetTokenBot(TwitchTokenBot tokenBot)
        {
            throw new NotImplementedException();
        }
    }
}
