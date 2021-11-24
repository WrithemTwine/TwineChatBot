using StreamerBot.Static;
using StreamerBot.Systems;

using System.IO;

using Xunit;

namespace TestStreamerBot
{
    public class UnitTest1
    {
        private bool setFile;
        private static readonly string DataFileXML = "ChatDataStore.xml";

        private string result = string.Empty;

        private SystemsController systemsController;

        private void Initialize()
        {
            if (!setFile)
            {
                if (File.Exists(DataFileXML))
                {
                    File.Delete(DataFileXML);
                }
                systemsController = new();
                systemsController.PostChannelMessage += SystemsController_PostChannelMessage;
                OptionFlags.SetSettings();

                OptionFlags.FirstUserChatMsg = true;
                OptionFlags.FirstUserJoinedMsg = false;

                setFile = true;
            }
        }

        private void SystemsController_PostChannelMessage(object sender, StreamerBot.Events.PostChannelMessageEventArgs e)
        {
            result = e.Msg;
        }

        [Fact]
        public void UserJoined()
        {
            Initialize();

            Assert.True(OptionFlags.FirstUserChatMsg);
            Assert.False(OptionFlags.FirstUserJoinedMsg);

            systemsController.UserJoined(new() { "DarkStreamPhantom" });
            Assert.Empty(result);
        }

        [Fact]
        public void UserChat()
        {
            Initialize();

            Assert.True(OptionFlags.FirstUserChatMsg);
            Assert.False(OptionFlags.FirstUserJoinedMsg);

            systemsController.UserJoined(new() { "DarkStreamPhantom" });

            result = string.Empty;

            systemsController.AddChat("DarkStreamPhantom");
            Assert.Empty(result);
        }
    }
}
