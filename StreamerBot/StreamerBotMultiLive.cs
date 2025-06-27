using StreamerBotLib.BotIOController;
using StreamerBotLib.GUI;
using StreamerBotLib.Models.Events;
using StreamerBotLib.Systems.MultiLive;

using System.Windows;

namespace StreamerBot
{
    public partial class StreamerBotWindow
    {
        #region MultiLive

        private void MultiLive_Data_Loaded(object sender, RoutedEventArgs e)
        {
            GUIDataManagerViews.DataViewsUpdated += (MultiLive_Data.Content as MultiLiveDataGrids).DataManager_OnDataCollectionUpdated;


            // allow edits while bot is active
            (MultiLive_Data.Content as MultiLiveDataGrids).SetIsEnabled(true);
            (MultiLive_Data.Content as MultiLiveDataGrids).SetHandlers(Settings_LostFocus, TB_BotActivityLog_TextChanged);
            (MultiLive_Data.Content as MultiLiveDataGrids).SummarizeChannels += StreamerBotWindow_SummarizeChannels;
            (MultiLive_Data.Content as MultiLiveDataGrids).FindMultiChannelUserId += StreamerBotWindow_FindMultiChannelUserId;
            (MultiLive_Data.Content as MultiLiveDataGrids).AddNewMultiChannelUser += StreamerBotWindow_AddNewMultiChannelUser;
            AddMultiLiveFoundUserId += (MultiLive_Data.Content as MultiLiveDataGrids).UpdateMultiChannelUserId;
        }

        private const string MultiLiveName = "MultiUserLiveBot";

        private event EventHandler<AddNewMultiChannelUserEventArgs> AddMultiLiveFoundUserId;

        private void StreamerBotWindow_AddNewMultiChannelUser(object sender, AddNewMultiChannelUserEventArgs e)
        {
            Controller.AddNewMonitorChannel([e.LiveUser]);
        }

        private void StreamerBotWindow_FindMultiChannelUserId(object sender, AddNewMultiChannelUserEventArgs e)
        {
            e.LiveUser.UserId = BotController.GetMultiChannelUserId(e.LiveUser.UserName);
            AddMultiLiveFoundUserId?.Invoke(this, new(e.LiveUser));
        }

        private bool SummarizeChannels = false;

        private void StreamerBotWindow_SummarizeChannels(object sender, MultiLiveSummarizeEventArgs e)
        {
            if (!SummarizeChannels) // some reason, this event is being called three times, prevent extra calls
            {
                SummarizeChannels = true;
                Controller.MultiChannelSummarize(e);
                SummarizeChannels = false;
            }
        }

        #endregion
    }
}
