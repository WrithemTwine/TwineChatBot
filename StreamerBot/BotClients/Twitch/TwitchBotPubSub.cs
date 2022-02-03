﻿using StreamerBot.Enums;
using StreamerBot.Static;

using System;
using System.Reflection;

using TwitchLib.PubSub;

namespace StreamerBot.BotClients.Twitch
{
    public class TwitchBotPubSub : TwitchBotsBase
    {
        public TwitchPubSub TwitchPubSub { get; private set; } = new();

        private string UserId;

        public TwitchBotPubSub()
        {
            BotClientName = Bots.TwitchPubSub;

            TwitchPubSub.OnPubSubServiceConnected += TwitchPubSub_OnPubSubServiceConnected;
        }

        public override bool StartBot()
        {
            bool Connected;

            try
            {
                if (IsStopped || !IsStarted)
                {
                    RefreshSettings();
                    UserId = BotsTwitch.TwitchBotUserSvc.GetUserId(TwitchChannelName).Result;

                    TwitchPubSub.Connect();
                    Connected = true;
                    IsStarted = true;
                    IsStopped = false;
                    InvokeBotStarted();
                }
                else
                {
                    Connected = false;
                }
            }
            catch (Exception ex)
            {
                LogWriter.LogException(ex, MethodBase.GetCurrentMethod().Name);

                Connected = false;
            }

            return Connected;
        }

        public override bool StopBot()
        {
            bool Stopped;

            try
            {
                if (IsStarted)
                {
                    IsStarted = false;
                    IsStopped = true;
                    TwitchPubSub.Disconnect();
                    RefreshSettings();
                    InvokeBotStopped();
                }
                Stopped = true;
            }
            catch (Exception ex)
            {
                LogWriter.LogException(ex, MethodBase.GetCurrentMethod().Name);
                Stopped = false;
            }

            return Stopped;
        }

        /// <summary>
        /// Event handler used to send all of the user selected 'listen to topics' to the server. This must be performed within 15 seconds of the connection, otherwise, the Twitch server disconnects the connection.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void TwitchPubSub_OnPubSubServiceConnected(object sender, EventArgs e)
        {

            // add Listen to Topics here
            if (OptionFlags.TwitchPubSubChannelPoints)
            {
                TwitchPubSub.ListenToChannelPoints(UserId);
            }

            // send the topics to listen
            TwitchPubSub.SendTopics(TwitchAccessToken);
        }

    }
}