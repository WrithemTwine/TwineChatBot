using StreamerBotLib.Models.Enums;
using StreamerBotLib.Static;

using System.Collections.Concurrent;

using TwitchLib.Api.Core.Exceptions;

namespace StreamerBotLib.BotClients.Twitch
{
    /// <summary>
    /// Use the bot account to send chat messages to the streamer chat.
    /// </summary>
    public class TwitchBotSendChatClient : TwitchBotsBase
    {
        private object _SendChatLock = new();

        private class SendChatMessage
        {
            public string Message { get; set; }
            public bool IsAnnouncement { get; set; }
            public SendChatMessage(string message, bool isAnnouncement)
            {
                Message = message;
                IsAnnouncement = isAnnouncement;
            }
        }

        private readonly TwitchTokenBot tokenBot;

        private const int SingleChatLength = 500;

        private readonly ConcurrentQueue<SendChatMessage> newSendMsg = [];

        internal TwitchBotSendChatClient(TwitchTokenBot TokenBot)
        {
            LogWriter.DebugLog(".ctor_TwitchBotSendChatClient", DebugLogTypes.TwitchBotSendChat, "Building the TwitchBotSendChatClient.");

            BotClientName = Bots.TwitchBotSendChatClient;
            tokenBot = TokenBot;
        }

        /// <summary>
        /// Wait until the EventSub subscriptions are refreshed before trying to send queued messages.
        /// </summary>
        /// <param name="sender">Unused</param>
        /// <param name="e">Unused</param>
        public void TokenUpdatedEventSubUpdated(object sender, EventArgs e)
        {
            ThreadManager.CreateThreadStart("TokenUpdatedEventSubUpdated", async () =>
            {
                LogWriter.DebugLog("TokenUpdatedEventSubUpdated", DebugLogTypes.TwitchBotSendChat, "Token updated event received, waiting 5 seconds before sending queued messages.");
                await Task.Delay(5000); // wait 5 seconds to ensure EventSub is updated

                await SendChat(); // attempt to send any queued messages after token refresh and EventSub update
            });
        }

        public override Task Send(string message, bool Announcement = false)
        {
            return Task.Run(async () =>
            {
                tokenBot.UpdateActiveTokens(BotType.BotAccount, true);

                LogWriter.DebugLog("Send", DebugLogTypes.TwitchBotSendChat, "Sending a message.");

                #region tokenize the message, max send length of 500 total characters, including switches & whitespace

                string prefix = message.StartsWith("/me ") ? "/me " : "";
                string tempSend = message;

                while (tempSend.Length > SingleChatLength)
                {
                    string temp = tempSend[..SingleChatLength];
                    string AddSend = temp[..(temp.LastIndexOf(' '))];
                    newSendMsg.Enqueue(new(AddSend, Announcement));
                    tempSend = prefix + tempSend.Replace(AddSend, "").Trim();
                }

                if (tempSend.Length > 0)
                {
                    newSendMsg.Enqueue(new(tempSend, Announcement));
                }

                #endregion end tokenize

                await SendChat();
            });
        }

        /// <summary>
        /// Performs the actual send of the chat message(s) to Twitch, with appropriate handling for token expiration and scope issues. Designed to be run in a separate thread to avoid blocking the main thread, especially during token refresh scenarios. The method will loop through the queued messages and attempt to send them, removing each message from the queue only after a successful send to ensure no messages are lost due to exceptions. If a token-related exception is caught, it will trigger a token check and break out of the loop, allowing the TokenUpdatedEventSubUpdated event handler to manage the retry logic after the token is refreshed.
        /// </summary>
        /// <returns></returns>
        private async Task SendChat()
        {
            lock (_SendChatLock)
            {
                if (IsActive == true) return; // already running, exit to avoid multiple concurrent send loops
                IsActive = true;
            }

            SendChatMessage firstmsg = null;

            while (!newSendMsg.IsEmpty)
            {
                firstmsg = newSendMsg.First(); // refer to first message without dequeuing, due to potential exceptions

                LogWriter.DebugLog("Send", DebugLogTypes.TwitchBotSendChat, "Message retrieved from queue and ready to send.");

                try
                {
                    int timeDelay = 2000;
                    if (firstmsg.IsAnnouncement)
                    {
                        LogWriter.DebugLog("Send-Announcement", DebugLogTypes.TwitchBotSendChat, "Sending announcement.");

                        await tokenBot.BotHelixApi.Helix.Chat.SendChatAnnouncementAsync(OptionFlags.TwitchStreamerUserId, OptionFlags.TwitchBotUserId, firstmsg.Message);
                        timeDelay = 2000;
                    }
                    else
                    {
                        await tokenBot.BotHelixApi.Helix.Chat.SendChatMessage(
                            new()
                            {
                                BroadcasterId = OptionFlags.TwitchStreamerUserId,
                                SenderId = OptionFlags.TwitchBotUserId,
                                Message = firstmsg.Message
                            }
                            );
                        timeDelay = 500;
                    }

                    await Task.Delay(timeDelay);

                    newSendMsg.TryDequeue(out _); // remove the message after successful send & no exceptions
                }

                catch (TokenExpiredException ex)
                {
                    LogWriter.LogException(ex, "Send");
                    ThreadManager.CreateThreadStart("Send", () => { tokenBot.CheckToken(); });
                    break; // break out of the send loop, captured a replay within the TokenUpdatedEventSubUpdated event handler
                }
                catch (BadScopeException ex)
                {
                    LogWriter.LogException(ex, "Send");
                    ThreadManager.CreateThreadStart("Send", () => { tokenBot.CheckToken(); });
                    break; // break out of the send loop, captured a replay within the TokenUpdatedEventSubUpdated event handler
                }
                catch (Exception ex)
                {
                    LogWriter.LogException(ex, "Send");
                    break; // break out of the send loop, captured a replay within the TokenUpdatedEventSubUpdated event handler
                }
            }

            lock (_SendChatLock)
            {
                IsActive = false;
            }
        }
    }
}
