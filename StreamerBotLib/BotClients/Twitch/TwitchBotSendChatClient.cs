using StreamerBotLib.Models.Enums;
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
        private readonly Queue<string> newSendMsg = [];

        internal TwitchBotSendChatClient(TwitchTokenBot TokenBot)
        {
            LogWriter.DebugLog(".ctor_TwitchBotSendChatClient", DebugLogTypes.TwitchBotSendChat, "Building the TwitchBotSendChatClient.");

            BotClientName = Bots.TwitchBotSendChatClient;
            tokenBot = TokenBot;
        }

        /// <summary>
        /// Send message to Twitch Streamer's channel
        /// </summary>
        /// <param name="message">Message to send to the channel. Can be over 500 long, code 
        /// will word break the message to fit within 500 and send as many messages as needed.</param>
        public override Task Send(string message, bool Announcement = false)
        {
            return Task.Run(async () =>
            {
                tokenBot.UpdateActiveTokens(BotType.BotAccount, true);

                LogWriter.DebugLog("Send", DebugLogTypes.TwitchBotSendChat, "Sending a message.");

                newSendMsg.Clear();

                string prefix = (message.StartsWith("/me ") && !Announcement ? "/me " : ""); // exclude prefix if announcement
                string tempSend = message.Replace("/me ", "");

                while (tempSend.Length > SingleChatLength)
                {
                    string temp = tempSend[..SingleChatLength];
                    string AddSend = temp[..(temp.LastIndexOf(' '))];
                    newSendMsg.Enqueue(AddSend);
                    tempSend = prefix + tempSend.Replace(AddSend, "").Trim();
                }

                if (tempSend.Length > 0)
                {
                    newSendMsg.Enqueue(tempSend);
                }

                if (Announcement)
                {
                    await Task.Run(async () =>
                    {
                        while (newSendMsg.TryDequeue(out string Msg))
                        { // TODO: correct desync from awaiting the send announcement and the loop continuing as it waits for the removed message to send count to 0
                            Action send = new(async () =>
                            {
                                await Task.Delay(2000);
                                await tokenBot.BotHelixApi.Helix.Chat.SendChatAnnouncementAsync(OptionFlags.TwitchStreamerUserId, OptionFlags.TwitchBotUserId, Msg);
                            });

                            try
                            {
                                LogWriter.DebugLog("Send-Announcement", DebugLogTypes.TwitchBotSendChat, "Sending announcement.");
                                send.Invoke();
                            }
                            catch (Exception ex)
                            {
                                LogWriter.DebugLog("Send-Announcement", DebugLogTypes.TwitchBotSendChat, "Found exception. Checking the access token.");
                                tokenBot.CheckToken();
                                LogWriter.LogException(ex, "Send-Announcement");
                                await Task.Delay(1000);
                                send.Invoke();
                            }
                        }
                    });
                }
                else
                {
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
                                    _ = newSendMsg.Dequeue(); // dequeue first item when sent
                                    x = 0;
                                }
                            }
                            catch (TokenExpiredException ex)
                            {
                                LogWriter.DebugLog("Send", DebugLogTypes.TwitchBotSendChat, "Found exception. Checking the access token.");
                                LogWriter.LogException(ex, "Send");
                                tokenBot.CheckToken();
                                Task.Delay(500 * (x + 1));
                                x++;
                            }
                            catch (Exception ex)
                            {
                                LogWriter.DebugLog("Send", DebugLogTypes.TwitchBotSendChat, "Found exception. Checking the access token.");
                                tokenBot.CheckToken();
                                LogWriter.LogException(ex, "Send");
                                Task.Delay(500 * (x + 1));
                                x++;
                            }
                        }
                    });
                }
            });
        }
    }
}
