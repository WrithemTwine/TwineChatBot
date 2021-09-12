namespace ChatBot_Net5.BotIOController
{
    public partial class BotController
    {
        private void Stats_PostChannelMessage(object sender, Events.PostChannelMessageEventArgs e)
        {
            Send(e.Msg);
        }


    }
}
