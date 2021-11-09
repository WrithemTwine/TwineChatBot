using StreamerBot.Data;
using StreamerBot.Enum;
using StreamerBot.Events;
using StreamerBot.Models;
using StreamerBot.Static;

using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

using TwitchLib.Api.Helix.Models.Clips.GetClips;

namespace StreamerBot.Systems
{
    public class SystemsController
    {
        public event EventHandler<PostChannelMessageEventArgs> PostChannelMessage;


        public static DataManager DataManage { get; private set; } = new();

        private StatisticsSystem Stats { get; set; }

        public SystemsController()
        {
            SystemsBase.DataManage = DataManage;
            LocalizedMsgSystem.SetDataManager(DataManage);
            Stats = new();
        }

        private void SendMessage(string message)
        {
            PostChannelMessage?.Invoke(this, new() { Msg = message });
        }

        public void UpdateFollowers(IEnumerable<Follow> Follows)
        {
            DataManage.UpdateFollowers(Follows);
        }

        public void AddNewFollowers(IEnumerable<Follow> FollowList)
        {
            string msg = LocalizedMsgSystem.GetEventMsg(ChannelEventActions.NewFollow, out bool FollowEnabled);

            while (DataManage.UpdatingFollowers) { } // spin until the 'add followers when bot starts - this.ProcessFollows()' is finished

            foreach (Follow f in FollowList.Where(f => DataManage.AddFollower(f.FromUserName, f.FollowedAt.ToLocalTime())))
            {
                if (OptionFlags.ManageFollowers)
                {
                    if (FollowEnabled)
                    {
                        SendMessage(VariableParser.ParseReplace(msg, VariableParser.BuildDictionary(new Tuple<MsgVars, string>[] { new(MsgVars.user, f.FromUserName) })));
                    }

                    Stats.AddFollow();
                    Stats.AddAutoEvents();
                }
            }
        }

        public void ManageDatabase()
        {
            SystemsBase.ManageDatabase();
        }

        public void ClearWatchTime()
        {
            SystemsBase.ClearWatchTime();
        }

        public bool StreamOnline(DateTime CurrTime)
        {
            return Stats.StreamOnline(CurrTime);
        }

        public void StreamOffline(DateTime CurrTime)
        {
            Stats.StreamOffline(CurrTime);
        }

        public void UpdateClips(List<Clip> ClipList)
        {
            Stats.ClipHelper(ClipList);
        }

        public void UpdatedStat(StreamStatType streamStat)
        {
            Stats.GetType()?.InvokeMember("Add" + streamStat.ToString(), BindingFlags.Public, null, Stats, null);
        }

        public void UpdatedStat(StreamStatType streamStat, int value)
        {
            Stats.GetType()?.InvokeMember("Add" + streamStat.ToString(), BindingFlags.Public, null, Stats, new object[] { value });
        }
    }
}
