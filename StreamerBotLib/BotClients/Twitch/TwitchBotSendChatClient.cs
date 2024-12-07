using StreamerBotLib.Enums;
using StreamerBotLib.Static;

using TwitchLib.Api.Core.Exceptions;

namespace StreamerBotLib.BotClients.Twitch
{
    /// <summary>
    /// Use the bot account to send chat messages to the streamer chat.
    /// </summary>
    public class TwitchBotSendChatClient : TwitchBotsBase
    {
        private readonly TwitchTokenBot tokenBot;

        private const int SingleChatLength = 500;
        private readonly Queue<Task> TaskSend = new();
        private readonly List<string> newSendMsg = [];

        internal TwitchBotSendChatClient(TwitchTokenBot TokenBot)
        {
            BotClientName = Bots.TwitchBotSendChatClient;
            tokenBot = TokenBot;
        }

        /// <summary>
        /// Send message to Twitch Streamer's channel
        /// </summary>
        /// <param name="message">Message to send to the channel. Can be over 500 long, code 
        /// will word break the message to fit within 500 and send as many messages as needed.</param>
        public override Task Send(string message)
        {
            return Task.Run(async () =>
            {
                LogWriter.DebugLog("Send", DebugLogTypes.TwitchBotSendChat, "Sending a message.");

                newSendMsg.Clear();

                string tempSend = message;

                string prefix = (message.StartsWith("/me ") ? "/me " : "");

                while (tempSend.Length > SingleChatLength)
                {
                    string temp = tempSend[..SingleChatLength];
                    string AddSend = temp[..(temp.LastIndexOf(' '))];
                    newSendMsg.Add(AddSend);
                    tempSend = prefix + tempSend.Replace(AddSend, "").Trim();
                }

                if (tempSend.Length > 0)
                {
                    newSendMsg.Add(tempSend);
                }

                await Task.Run(() =>
                {
                    int x = 0; // loop x times and stop, to prevent infinite loop
                    const int MaxTime = 5; // try this many times and stop the loop

                    while (newSendMsg.Count > 0 && x < MaxTime)
                    {
                        try
                        {
                            string firstmsg = newSendMsg.FirstOrDefault();
                            if (firstmsg != default && tokenBot.BotHelixApi.Helix.Chat.SendChatMessage(OptionFlags.TwitchStreamerUserId, OptionFlags.TwitchBotUserId, firstmsg).Result.Data[0].IsSent)
                            {
                                newSendMsg.Remove(firstmsg);
                                x = 0;
                            }
                        }
                        catch (TokenExpiredException ex)
                        {
                            LogWriter.LogException(ex, "Send");
                            tokenBot.CheckToken();
                            Thread.Sleep(500 * (x + 1));
                            x++;
                        }
                        catch (Exception ex)
                        {
                            LogWriter.DebugLog("Send", DebugLogTypes.TwitchBotSendChat, "Found exception.");

                            LogWriter.LogException(ex, "Send");
                            Thread.Sleep(500 * (x + 1));
                            x++;
                        }
                    }
                });
            });
        }
    }
}
