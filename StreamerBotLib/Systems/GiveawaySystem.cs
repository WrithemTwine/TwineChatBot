using StreamerBotLib.Models;
using StreamerBotLib.Models.Enums;
using StreamerBotLib.Static;
using StreamerBotLib.Systems.Overlay.Enums;

namespace StreamerBotLib.Systems
{
    public partial class ActionSystem
    {
        #region Giveaway

        private bool GiveawayStarted = false;
        private readonly List<LiveUser> GiveawayCollectionList = [];

        /// <summary>
        /// Initialize and start accepting giveaway entries
        /// </summary>
        public void BeginGiveaway()
        {
            LogWriter.DebugLog("BeginGiveaway", DebugLogTypes.SystemController, "Starting giveaway.");

            GiveawayStarted = true;
            GiveawayCollectionList.Clear();
            GiveawayCollection.Clear();

            SendMessage(OptionFlags.GiveawayBegMsg, OptionFlags.GiveawayAnnounceBegMsg);
        }

        /// <summary>
        /// Adds a viewer DisplayName to the active giveaway list. The giveaway must be started through <code>BeginGiveaway()</code>.
        /// </summary>
        /// <param name="DisplayName"></param>
        public void ManageGiveaway(LiveUser User)
        {
            LogWriter.DebugLog("ManageGiveaway", DebugLogTypes.SystemController, "Managing giveaway.");

            if (GiveawayStarted && ((OptionFlags.GiveawayMultiUser && GiveawayCollectionList.FindAll((e) => e == User).Count < OptionFlags.GiveawayMaxEntries) || GiveawayCollectionList.UniqueAdd(User)))
            {
                LogWriter.DebugLog("ManageGiveaway", DebugLogTypes.SystemController, "Adding user to giveaway list.");
                GiveawayCollection.Add(User);
            }

            LogWriter.DebugLog("ManageGiveaway", DebugLogTypes.SystemController, "Checking for max entries for user.");
            while (GiveawayCollectionList.FindAll((e) => e == User).Count > OptionFlags.GiveawayMaxEntries)
            {
                LogWriter.DebugLog("ManageGiveaway", DebugLogTypes.SystemController, "Removing extra user entries from giveaway list.");
                GiveawayCollectionList.RemoveAt(GiveawayCollectionList.FindLastIndex((s) => s == User));
            }
        }

        /// <summary>
        /// End the Giveaway event.
        /// </summary>
        public void EndGiveaway()
        {
            LogWriter.DebugLog("EndGiveaway", DebugLogTypes.SystemController, "Ending giveaway.");
            GiveawayStarted = false;
            SendMessage(OptionFlags.GiveawayEndMsg, OptionFlags.GiveawayAnnounceEndMsg);
        }

        /// <summary>
        /// Pick a winner and send the winner notice to the channel chat
        /// </summary>
        public void PostGiveawayResult()
        {
            LogWriter.DebugLog("PostGiveawayResult", DebugLogTypes.SystemController, "Posting giveaway result.");
            Random random = new();

            string DisplayName = "";

            if (GiveawayCollectionList.Count > 0)
            {
                List<LiveUser> WinnerList = [];
                int x = 0;
                while (x < OptionFlags.GiveawayCount)
                {
                    LogWriter.DebugLog("PostGiveawayResult", DebugLogTypes.SystemController, "Picking winner.");
                    LiveUser winner = GiveawayCollectionList[random.Next(GiveawayCollectionList.Count)];
                    GiveawayCollectionList.RemoveAll((w) => w == winner);
                    WinnerList.Add(winner);
                    // DisplayName += (OptionFlags.GiveawayCount > 1 && x > 0 ? ", " : "") + winner;
                    if (OptionFlags.ManageGiveawayUsers)
                    {
                        LogWriter.DebugLog("PostGiveawayResult", DebugLogTypes.SystemController, "Posting giveaway data to database.");
                        DataManage.PostGiveawayData(winner.UserId, DateTime.Now.ToLocalTime());
                    }
                    x++;
                }

                DisplayName = string.Join(", ", WinnerList);

                if (DisplayName != "")
                {
                    LogWriter.DebugLog("PostGiveawayResult", DebugLogTypes.SystemController, "Sending winner message.");
                    SendMessage(
                        VariableParser.ParseReplace(
                            OptionFlags.GiveawayWinMsg ?? "",
                            VariableParser.BuildDictionary(
                                new Tuple<MsgVars, string>[]
                                {
                                new(MsgVars.winner, DisplayName)
                                }
                                )), OptionFlags.GiveawayAnnounceWinMsg);

                    foreach (LiveUser W in WinnerList)
                    {
                        LogWriter.DebugLog("PostGiveawayResult", DebugLogTypes.SystemController, "Checking for overlay event.");
                        CheckForOverlayEvent(OverlayTypes.Giveaway, OverlayTypes.Giveaway.ToString(), W);
                    }

                }
            }
        }

        #endregion
    }
}
