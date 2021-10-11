using ChatBot_Net5.BotClients;
using ChatBot_Net5.Static;
using ChatBot_Net5.Systems;

using System;

namespace ChatBot_Net5.BotIOController
{
    public partial class BotController
    {
        private void TwitchLiveMonitor_OnBotStarted(object sender, EventArgs e)
        {
            TwitchBots currBot = sender as TwitchBots;

            // perform loading steps every time, because service is a new object when started
            RegisterHandlers();
            OnBotStarted?.Invoke(this, new() { BotName = currBot.BotClientName, Started = currBot.IsStarted });
        }

        private void TwitchLiveMonitor_OnBotStopped(object sender, EventArgs e)
        {
            TwitchBots currBot = sender as TwitchBots;

            OnBotStopped?.Invoke(this, new() { BotName = currBot.BotClientName, Stopped = currBot.IsStopped });
        }

        private void TwitchFollower_OnBotStarted(object sender, EventArgs e)
        {
            TwitchBots currBot = sender as TwitchBots;

            // perform loading steps every time, because service is a new object when started
            RegisterHandlers();
            BeginAddFollowers(); // begin adding followers back to the data table

            OnBotStarted?.Invoke(this, new() { BotName = currBot.BotClientName, Started = currBot.IsStarted });
        }

        private void TwitchFollower_OnBotStopped(object sender, EventArgs e)
        {
            TwitchBots currBot = sender as TwitchBots;

            OnBotStopped?.Invoke(this, new() { BotName = currBot.BotClientName, Stopped = currBot.IsStopped });
        }

        private void TwitchIO_OnBotStarted(object sender, EventArgs e)
        {
            TwitchBots currBot = sender as TwitchBots;

            // perform loading steps
            if (!TwitchIO.HandlersAdded) { RegisterHandlers(); }

            OptionFlags.ProcessOps = true; // required as true to spin the "SendThread" while loop, so it doesn't conclude early
            StartThreads(); // messages can be sent now the chat client is connected
            OnBotStarted?.Invoke(this, new() { BotName = currBot.BotClientName, Started = currBot.IsStarted });
        }

        private void TwitchIO_OnBotStopped(object sender, EventArgs e)
        {
            TwitchBots currBot = sender as TwitchBots;

            OptionFlags.ProcessOps = false;
            // wait until all messages are sent through the chat client before stopping
            SendThread?.Join();

            // the commands aren't available when bot is stopped - commands are stopped, only Twitch is current available chat client; should stop only when all chat bots are stopped
            SystemsController.StopElapsedTimerThread();
            OnBotStopped?.Invoke(this, new() { BotName = currBot.BotClientName, Stopped = currBot.IsStopped });
        }

        private void TwitchClip_OnBotStopped(object sender, EventArgs e)
        {
            TwitchBots currBot = sender as TwitchBots;

            OnBotStopped?.Invoke(this, new() { BotName = currBot.BotClientName, Stopped = currBot.IsStopped });
        }

        private void TwitchClip_OnBotStarted(object sender, EventArgs e)
        {
            TwitchBots currBot = sender as TwitchBots;

            RegisterHandlers();

            if (TwitchClip.IsStarted)
            {
                BeginAddClips();
            }

            OnBotStarted?.Invoke(this, new() { BotName = currBot.BotClientName, Started = currBot.IsStarted });
        }

    }
}
