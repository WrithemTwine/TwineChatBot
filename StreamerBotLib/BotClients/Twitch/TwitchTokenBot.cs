using StreamerBotLib.Static;

using System;
using System.Threading;

using TwitchLib.Api.Auth;

namespace StreamerBotLib.BotClients.Twitch
{
    internal class TwitchTokenBot : TwitchBotsBase
    {
        private Auth AuthBot;

        private bool TokenRenewalStarted;
        private bool AbortRenewToken;

        private const int MaxInterval = 30 * 60 * 1000;  // 30 mins * 60 s/min * 1000 ms/sec

        public TwitchTokenBot()
        {
            AbortRenewToken = false;
        }

        public override bool StartBot()
        {
            AbortRenewToken = false;
            StartRenewToken();
            return true;
        }

        public override bool StopBot()
        {
            AbortRenewToken = true;
            return true;
        }

        private void StartRenewToken()
        {
            if (!TokenRenewalStarted)
            {
                TokenRenewalStarted = true;
                ThreadManager.CreateThreadStart(RenewToken);
            }
        }

        private void RenewToken()
        {
            Random IntervalRandom = new Random();

            while (!AbortRenewToken)
            {
                CheckToken();

                Thread.Sleep(IntervalRandom.Next(MaxInterval / 4, MaxInterval));
            }
        }

        internal void CheckToken()
        {
            // TODO: add renewal token details 
        }
    }
}
