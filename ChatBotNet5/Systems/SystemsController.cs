using ChatBot_Net5.BotClients.TwitchLib.Events.ClipService;
using ChatBot_Net5.Data;
using ChatBot_Net5.Enum;
using ChatBot_Net5.Events;
using ChatBot_Net5.Static;

using System;
using System.Collections.Generic;
using System.Reflection;

using TwitchLib.Api.Helix.Models.Clips.GetClips;
using TwitchLib.Api.Helix.Models.Users.GetUserFollows;
using TwitchLib.Api.Services.Events.FollowerService;
using TwitchLib.Client.Models;

namespace ChatBot_Net5.Systems
{
    public class SystemsController
    {
        private static SystemsBase SystemsBase { get; set; } = new();
        private static StatisticsSystem Stats { get; set; } = new();
        private static CurrencySystem Currency { get; set; } = new();
        private static CommandSystem Command { get; set; }

        public static DataManager DataManage { get { return SystemsBase.DataManage; } }
        public static string Category
        {
            get { return SystemsBase.Category; }
            set { SystemsBase.Category = value; }
        }

        public SystemsController(string BotUserName = "", Action<string> SendMsgCallback = null, EventHandler<TimerCommandsEventArgs> ProcessCommands_OnRepeatEventOccured = null, EventHandler<UserJoinEventArgs> ProcessCommands_UserJoinCommand = null)
        {
            SystemsBase.DataManage = new();
            SystemsBase.CallbackSendMsg = SendMsgCallback;
            SystemsBase.DataManage.OnSaveData += SystemsBase.DataManage.SaveData;

            LocalizedMsgSystem.SetDataManager(SystemsBase.DataManage);
            Command = new(BotUserName);

            SetEvents(ProcessCommands_OnRepeatEventOccured, ProcessCommands_UserJoinCommand);
        }

        private void SetEvents(EventHandler<TimerCommandsEventArgs> ProcessCommands_OnRepeatEventOccured, EventHandler<UserJoinEventArgs> ProcessCommands_UserJoinCommand)
        {
            Stats.BeginCurrencyClock += Stats_BeginCurrencyClock;
            Stats.BeginWatchTime += Stats_BeginWatchTime;

            Command.OnRepeatEventOccured += ProcessCommands_OnRepeatEventOccured;
            Command.UserJoinCommand += ProcessCommands_UserJoinCommand;
        }

        public static void ExitSystem()
        {
            Stats.StreamOffline(DateTime.Now.ToLocalTime());
        }

        #region Data Manage Operations

        public static void ClearWatchTime()
        {
            SystemsBase.ClearWatchTime();
        }

        public static void ClearAllCurrenciesValues()
        {
            SystemsBase.ClearAllCurrenciesValues();
        }

        public static void UpdateFollowers(string ChannelName, IEnumerable<Follow> Follows)
        {
            SystemsBase.UpdateFollowers(ChannelName, Follows);
        }

        #endregion

        #region Currency System

        private void Stats_BeginCurrencyClock(object sender, EventArgs e)
        {
            Currency.StartCurrencyClock();
        }

        private void Stats_BeginWatchTime(object sender, EventArgs e)
        {
            Currency.MonitorWatchTime();
        }

        #endregion

        #region Command System

        public static string ParseCommand(string CommandText, List<string> ArgumentsAsList, ChatMessage ChatMessage)
        {
            return Command.ParseCommand(CommandText, ArgumentsAsList, ChatMessage);
        }

        public static void StopElapsedTimerThread()
        {
            Command.StopElapsedTimerThread();
        }

        public static bool CheckShout(string Username, out string response, bool AutoShout = true)
        {
            return Command.CheckShout(Username, out response, AutoShout);
        }

        #endregion

        #region Stats System

        public static bool StreamOnline(DateTime CurrTime)
        {
            return Stats.StreamOnline(CurrTime);
        }

        public static void StreamOffline(DateTime CurrTime)
        {
            Stats.StreamOffline(CurrTime);
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

        public static bool UserJoined(string User, DateTime CurrTime)
        {
            return Stats.UserJoined(User, CurrTime);
        }

        public static void UserLeft(string User, DateTime CurrTime)
        {
            Stats.UserLeft(User, CurrTime);
        }

        public static bool UserChat(string Username)
        {
            return Stats.UserChat(Username);
        }

        public static void SubJoined(string UserName)
        {
            Stats.SubJoined(UserName);
        }

        public static void VIPJoined(string UserName)
        {
            Stats.VIPJoined(UserName);
        }

        public static void ModJoined(string UserName)
        {
            Stats.ModJoined(UserName);
        }

        public static void SetCategory(string GameId, string GameName)
        {
            SetCategory(GameId, GameName);
        }

        public void PostIncomingRaid(string UserName, DateTime RaidTime, string Viewers, string GameName)
        {
            Stats.PostIncomingRaid(UserName, RaidTime, Viewers, GameName);
        }

        public void FollowerService_OnNewFollowersDetected(object sender, OnNewFollowersDetectedArgs e)
        {
            Stats.AddNewFollower(e.NewFollowers);
        }

        public void ClipMonitorService_OnNewClipFound(object sender, OnNewClipsDetectedArgs e)
        {
            Stats.ClipHelper(e.Clips);
        }

        #endregion

    }
}
