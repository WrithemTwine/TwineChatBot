using ChatBot_Net5.Events;
using ChatBot_Net5.Systems;

namespace ChatBot_Net5.BotIOController
{
    public partial class BotController
    {
        private void BotController_OnCompletedDownloadFollowers(object sender, DownloadedFollowersEventArgs e)
        {
            SystemsController.UpdateFollowers(e.ChannelName, e.FollowList);
        }

        private void BotController_OnClipFound(object sender, ClipFoundEventArgs e)
        {

            SystemsController.UpdateClips(ClipList);
        }
    }
}
