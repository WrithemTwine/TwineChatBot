using ChatBot_Net5.Data;
using ChatBot_Net5.Static;

using System.Collections.Generic;

using TwitchLib.Api.Helix.Models.Clips.GetClips;
using TwitchLib.Api.Helix.Models.Users.GetUserFollows;

namespace ChatBot_Net5.Systems
{
    public class BotSystems
    {
        public static DataManager DataManage { get; set; }

        public static void SaveData()
        {
            DataManage.SaveData();
        }

        public static void UpdateFollowers(string ChannelName, List<Follow> Follows)
        {
            DataManage.UpdateFollowers(ChannelName, new() { { ChannelName, Follows } });
        }

        /// <summary>
        /// Add currency accrual rows for every user when a new currency type is added to the database
        /// </summary>
        public static void UpdateCurrencyTable()
        {
            DataManage.AddCurrencyRows();
        }

        public static void ManageDatabase()
        {
            // TODO: add fixes if user re-enables 'managing { users || followers || stats }' to restart functions without restarting the bot

            // if ManageUsers is False, then remove users!
            if (!OptionFlags.ManageUsers)
            {
                DataManage.RemoveAllUsers();
            }

            // if ManageFollowers is False, then remove followers!, upstream code stops the follow bot
            if (!OptionFlags.ManageFollowers)
            {
                DataManage.RemoveAllFollowers();
            }

            // when management resumes, code upstream enables the startbot process

            //  if ManageStreamStats is False, then remove all Stream Statistics!
            if (!OptionFlags.ManageStreamStats)
            {
                DataManage.RemoveAllStreamStats();
            } // when the LiveStream Online event fires again, the datacollection will restart

            if (!OptionFlags.ManageRaidData)
            {
                DataManage.RemoveAllRaidData();
            }
        }

        public static void ClearWatchTime()
        {
            DataManage.ClearWatchTime();
        }

        public static void ClearAllCurrenciesValues()
        {
            DataManage.ClearAllCurrencyValues();
        }

        public static bool AddClip(Clip c)
        {
            return DataManage.AddClip(c.Id, c.CreatedAt, c.Duration, c.GameId, c.Language, c.Title, c.Url);
        }
    }
}
