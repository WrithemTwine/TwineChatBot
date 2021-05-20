using System;

namespace ChatBot_Net5.BotIOController
{
    public partial class BotController
    {
        private void TwitchLiveMonitor_OnBotStarted(object sender, EventArgs e)
        {
            // perform loading steps
            RegisterHandlers();
        }

        private void TwitchFollower_OnBotStarted(object sender, EventArgs e)
        {
            // perform loading steps
            RegisterHandlers();

            if (OptionFlags.FirstFollowerProcess && TwitchFollower.IsStarted)
            {
                BeginAddFollowers(); // begin adding followers back to the data table
            }
        }

        private void TwitchIO_OnBotStarted(object sender, EventArgs e)
        {
            // perform loading steps
            RegisterHandlers();


            OptionFlags.ProcessOps = true; // required as true to spin the "SendThread" while loop, so it doesn't conclude early
            SetThread();
            SetProcessCommands();
        } 
        
        private void TwitchIO_OnBotStopped(object sender, EventArgs e)
        {
            OptionFlags.ProcessOps = false;
            SendThread?.Join();
        }
    }
}
