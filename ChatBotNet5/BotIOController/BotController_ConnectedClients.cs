using ChatBot_Net5.Data;

using System;

namespace ChatBot_Net5.BotIOController
{
    public partial class BotController
    {
        private void TwitchLiveMonitor_OnBotStarted(object sender, EventArgs e)
        {
            // perform loading steps every time, because service is a new object when started
            RegisterHandlers();
        }

        private void TwitchLiveMonitor_OnBotStopped(object sender, System.EventArgs e)
        {
            
        }

        private void TwitchFollower_OnBotStarted(object sender, EventArgs e)
        {
            // perform loading steps every time, because service is a new object when started
            RegisterHandlers();

            if (OptionFlags.FirstFollowerProcess && TwitchFollower.IsStarted)
            {
                BeginAddFollowers(); // begin adding followers back to the data table
            }
        }

        private void TwitchFollower_OnBotStopped(object sender, System.EventArgs e)
        {

        }

        private void TwitchIO_OnBotStarted(object sender, EventArgs e)
        {
            // perform loading steps
            if(!TwitchIO.HandlersAdded) RegisterHandlers();
            
            OptionFlags.ProcessOps = true; // required as true to spin the "SendThread" while loop, so it doesn't conclude early
            StartProcMsgThread(); // messages can be sent now the chat client is connected
            SetProcessCommands(); // commands can be processed when received through the chat
        } 
        
        private void TwitchIO_OnBotStopped(object sender, EventArgs e)
        {
            OptionFlags.ProcessOps = false;
            // wait until all messages are sent through the chat client before stopping
            SendThread?.Join(); 
        }
    }
}
