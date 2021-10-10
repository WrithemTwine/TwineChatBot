
using ChatBot_Net5.Data;
using ChatBot_Net5.Enum;
using ChatBot_Net5.Static;

using System;
using System.Collections.Generic;
using System.Reflection;

using TwitchLib.Api.Helix.Models.Clips.GetClips;
using TwitchLib.Api.Helix.Models.Users.GetUserFollows;

namespace ChatBot_Net5.Systems
{
    public class SystemsController
    {
        private static SystemsBase SystemsBase { get; set; } = new();
        private static StatisticsSystem Stats { get; set; } = new();
        private static CurrencySystem Currency { get; set; } = new();
        private static CommandSystem Command { get; set; }

        public static DataManager DataManage { get { return SystemsBase.DataManage; } }
        public string Category
        {
            get { return SystemsBase.Category; }
            set { SystemsBase.Category = value; }
        }


        public SystemsController(string BotUserName = "", Action<string> SendMsgCallback = null)
        {
            SystemsBase.DataManage = new();
            SystemsBase.CallbackSendMsg = SendMsgCallback;
            SystemsBase.DataManage.OnSaveData += SystemsBase.DataManage.SaveData;

            LocalizedMsgSystem.SetDataManager(SystemsBase.DataManage);
            Command = new(BotUserName);

            SetEvents();
        }

        private void SetEvents()
        {
            Stats.BeginCurrencyClock += Stats_BeginCurrencyClock;
            Stats.BeginWatchTime += Stats_BeginWatchTime;
        }

        private void Stats_BeginCurrencyClock(object sender, EventArgs e)
        {
            Currency.StartCurrencyClock();
        }

        private void Stats_BeginWatchTime(object sender, EventArgs e)
        {
            Currency.MonitorWatchTime();
        }

        public static void ManageDatabase()
        {
            if (!OptionFlags.ManageStreamStats)
        {
                Stats.EndPostingStreamUpdates();
        }

            if (OptionFlags.ManageUsers)
            {
                // if ManageUsers is False, then remove users!
                Stats.ManageUsers();
            }

            SystemsBase.ManageDatabase();
            }

        public static void ClearWatchTime()
            {
            SystemsBase.ClearWatchTime();
            }

        public static void ClearAllCurrenciesValues()
            {
            SystemsBase.ClearAllCurrenciesValues();
            }

        public static void UpdateClips(List<Clip> ClipList)
        {
            Stats.ClipHelper(ClipList);
        }

        public static void UpdatedStat(StreamStatType streamStat)
        {
            Stats.GetType()?.InvokeMember("Add" + streamStat.ToString(), BindingFlags.Public, null, Stats, null);
        }

        public static void UpdatedStat(StreamStatType streamStat, int value)
        {
            Stats.GetType()?.InvokeMember("Add" + streamStat.ToString(), BindingFlags.Public, null, Stats, new object[] { value });
        }

        public static void UpdateFollowers(string ChannelName, List<Follow> Follows)
        {
            SystemsBase.UpdateFollowers(ChannelName, Follows);
        }

        public static void ExitSystem()
        {
            Stats.StreamOffline(DateTime.Now.ToLocalTime());
        }
    }
}
