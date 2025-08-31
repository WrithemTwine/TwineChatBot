using StreamerBotLib.Models;
using StreamerBotLib.Models.Enums;
using StreamerBotLib.Models.Events;
using StreamerBotLib.Static;

namespace StreamerBotLib.Systems
{
    public partial class ActionSystem
    {
        #region MultiLive 

        internal void SetMultiStatusLog(ref List<string> log)
        {
            DataManage.SetMultiLiveStatusLog(ref log);
        }

        public void PostMultiLiveLog(string message)
        {
            DataManage.PostMultiLiveLog(message);
        }

        public void AddNewMonitorChannel(IEnumerable<LiveUser> liveUsers)
        {
            LogWriter.DebugLog("AddNewMonitorChannel", DebugLogTypes.SystemController, "Adding new monitor channel.");
            DataManage.PostMonitorChannel(liveUsers);
        }

        public IEnumerable<string> GetMonitorChannels(Platform platform)
        {
            LogWriter.DebugLog("GetMonitorChannels", DebugLogTypes.MultiLiveSystem, $"Getting monitor channels for platform: {platform}.");
            return DataManage.GetMultiChannelIds(platform);
        }

        /// <summary>
        /// Summarize the multi-live data.
        /// </summary>
        /// <param name="multiLiveSummarizeEventArgs">Defines data, if null then all date records are summarized, and 
        /// a callback action to invoke after querying the database. 
        /// See also: <seealso cref="MultiLiveSummarizeEventArgs"/></param>
        public void MultiSummarize(MultiLiveSummarizeEventArgs multiLiveSummarizeEventArgs)
        {
            LogWriter.DebugLog("MultiSummarize", DebugLogTypes.SystemController, "Summarizing multi-live data.");
            if (multiLiveSummarizeEventArgs.Data == null)
            {
                DataManage.SummarizeStreamData();
                multiLiveSummarizeEventArgs.CallbackAction.Invoke();
            }
            else
            {
                DataManage.SummarizeStreamData(multiLiveSummarizeEventArgs.Data);
                multiLiveSummarizeEventArgs.CallbackAction.Invoke();
            }
        }

        public IEnumerable<Tuple<WebhooksSource, Uri>> GetMultiWebHooks()
        {
            return DataManage.GetMultiWebHooks();
        }

        public bool CheckMultiStreamDate(string userId, Platform platform, DateTime currTime)
        {
            LogWriter.DebugLog("CheckMultiStreamDate", DebugLogTypes.MultiLiveSystem, $"Checking multi-stream date for {userId} on {platform}.");
            return DataManage.CheckMultiStreamDate(userId, platform, currTime);
        }

        public bool PostMultiStreamDate(LiveUser liveUser, DateTime startTime)
        {
            LogWriter.DebugLog("PostMultiStreamData", DebugLogTypes.SystemController, $"Posting multi-stream data for {liveUser.UserName} on {liveUser.Platform}.");
            return DataManage.PostMultiStreamDate(liveUser, startTime);
        }

#if DEBUG
        internal void DebugAddNewMultiLiveData()
        {
            Random rand = new();
            var Ids = GetMonitorChannels(Platform.Twitch).ToList();
            DateTime now = DateTime.Now.ToLocalTime();

            if (Ids.Count > 0)
            {
                int idx = rand.Next(0, Ids.Count - 1);
                var userId = Ids[idx];
                LiveUser liveUser = DataManage.GetUserById(userId, Platform.Twitch);
                PostMultiStreamDate(liveUser, now);
            }
        }
#endif

        #endregion
    }
}
