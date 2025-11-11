using StreamerBotLib.Models.Enums;
using StreamerBotLib.Static;

using System.Collections.Concurrent;
using System.Threading.Tasks;

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

        private List<string> newSendMsg = [];

        private bool CurrAnnouncement = false;

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

            IsActive = true;
            while (newSendMsg.Count > 0)
            {
                string firstmsg = newSendMsg[0]; // refer to first message without dequeuing, due to potential exceptions
                LogWriter.DebugLog("Send", DebugLogTypes.TwitchBotSendChat, "Message retrieved from queue and ready to send.");

                try
                {
                    if (CurrAnnouncement)
                    {
                        LogWriter.DebugLog("Send-Announcement", DebugLogTypes.TwitchBotSendChat, "Sending announcement.");

                        await tokenBot.BotHelixApi.Helix.Chat.SendChatAnnouncementAsync(OptionFlags.TwitchStreamerUserId, OptionFlags.TwitchBotUserId, firstmsg);
                        await Task.Delay(2000);
                    }
                    else
                    {
                        await tokenBot.BotHelixApi.Helix.Chat.SendChatMessage(OptionFlags.TwitchStreamerUserId, OptionFlags.TwitchBotUserId, firstmsg);
                        await Task.Delay(500);
                    }
                }

                catch (TokenExpiredException ex)
                {
                    LogWriter.LogException(ex, "Send_TokenUpdatedEventSubUpdated");
                    ThreadManager.CreateThreadStart("Send_TokenUpdatedEventSubUpdated", () => { tokenBot.CheckToken(); });
                }
                catch (BadScopeException ex)
                {
                    LogWriter.LogException(ex, "Send_TokenUpdatedEventSubUpdated");
                    ThreadManager.CreateThreadStart("Send_TokenUpdatedEventSubUpdated", () => { tokenBot.CheckToken(); });
                }
                catch (Exception ex)
                {
                    LogWriter.LogException(ex, "Send_TokenUpdatedEventSubUpdated");
                }

                newSendMsg.Remove(firstmsg); // remove the message after successful send & no exceptions
            }
            IsActive = false;

            });
        }

        public override Task Send(string message, bool Announcement = false)
        {
            return Task.Run(async () =>
            {
                CurrAnnouncement = Announcement;
                tokenBot.UpdateActiveTokens(BotType.BotAccount, true);

                LogWriter.DebugLog("Send", DebugLogTypes.TwitchBotSendChat, "Sending a message.");

                #region tokenize the message, max send length of 500 total characters, including switches & whitespace
                // Clear any previous messages
                newSendMsg.Clear();

                string prefix = (message.StartsWith("/me ") && !CurrAnnouncement ? "/me " : ""); // exclude prefix if announcement
                string tempSend = message.Replace("/me ", "");

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
                #endregion end tokenize

                bool clean = false;

                IsActive = true;
                while (newSendMsg.Count > 0)
                {
                    string firstmsg = newSendMsg[0]; // refer to first message without dequeuing, due to potential exceptions
                    LogWriter.DebugLog("Send", DebugLogTypes.TwitchBotSendChat, "Message retrieved from queue and ready to send.");

                    try
                    {
                        if (CurrAnnouncement)
                        {
                            LogWriter.DebugLog("Send-Announcement", DebugLogTypes.TwitchBotSendChat, "Sending announcement.");

                            await tokenBot.BotHelixApi.Helix.Chat.SendChatAnnouncementAsync(OptionFlags.TwitchStreamerUserId, OptionFlags.TwitchBotUserId, firstmsg);
                            await Task.Delay(2000);
                        }
                        else
                        {
                            await tokenBot.BotHelixApi.Helix.Chat.SendChatMessage(OptionFlags.TwitchStreamerUserId, OptionFlags.TwitchBotUserId, firstmsg);
                            await Task.Delay(500);
                        }
                        clean = true;
                    }

                    catch (TokenExpiredException ex)
                    {
                        LogWriter.LogException(ex, "Send");
                        ThreadManager.CreateThreadStart("Send", () => { tokenBot.CheckToken(); });
                    }
                    catch (BadScopeException ex)
                    {
                        LogWriter.LogException(ex, "Send");
                        ThreadManager.CreateThreadStart("Send", () => { tokenBot.CheckToken(); });
                    }
                    catch (Exception ex)
                    {
                        LogWriter.LogException(ex, "Send");
                    }

                    if (clean)
                    {
                        newSendMsg.Remove(firstmsg); // remove the message after successful send & no exceptions
                    }
                    clean = false;
                }
                IsActive = false;
            });
        }
    }
}
