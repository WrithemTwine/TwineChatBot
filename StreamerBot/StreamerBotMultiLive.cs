using StreamerBotLib.BotIOController;
using StreamerBotLib.Events;
using StreamerBotLib.GUI;
using StreamerBotLib.MultiLive;
using StreamerBotLib.Systems;

using System.Windows;

namespace StreamerBot
{
    public partial class StreamerBotWindow
    {
        #region MultiLive

        private void MultiLive_Data_Loaded(object sender, RoutedEventArgs e)
        {
            (MultiLive_Data.Content as MultiLiveDataGrids).DataContext = Resources["DataViews"] as GUIDataManagerViews;
            SetMultiLiveActive();

            // attach datagrid refresh when data tables change contents (different rows do an auto refresh, row content changes do not auto refresh)
            SystemsController.DataManage.OnDataCollectionUpdated += (MultiLive_Data.Content as MultiLiveDataGrids).DataManager_OnDataCollectionUpdated;
        }

        private const string MultiLiveName = "MultiUserLiveBot";

        private void SetMultiLiveActive()
        {
            // allow edits while bot is active
            (MultiLive_Data.Content as MultiLiveDataGrids).SetIsEnabled(true);
            (MultiLive_Data.Content as MultiLiveDataGrids).SetHandlers(Settings_LostFocus, TB_BotActivityLog_TextChanged);
            (MultiLive_Data.Content as MultiLiveDataGrids).SummarizeChannels += StreamerBotWindow_SummarizeChannels;
            (MultiLive_Data.Content as MultiLiveDataGrids).FindMultiChannelUserId += StreamerBotWindow_FindMultiChannelUserId;
            (MultiLive_Data.Content as MultiLiveDataGrids).AddNewMultiChannelUser += StreamerBotWindow_AddNewMultiChannelUser;
            AddMultiLiveFoundUserId += (MultiLive_Data.Content as MultiLiveDataGrids).UpdateMultiChannelUserId;
        }

        private event EventHandler<AddNewMultiChannelUserEventArgs> AddMultiLiveFoundUserId;

        private void StreamerBotWindow_AddNewMultiChannelUser(object sender, AddNewMultiChannelUserEventArgs e)
        {
            SystemsController.DataManage.PostMonitorChannel([e.LiveUser]);
        }

        private void StreamerBotWindow_FindMultiChannelUserId(object sender, AddNewMultiChannelUserEventArgs e)
        {
            e.LiveUser.UserId = BotController.GetMultiChannelUserId(e.LiveUser.UserName);
            AddMultiLiveFoundUserId?.Invoke(this, new(e.LiveUser));
        }

        private void StreamerBotWindow_SummarizeChannels(object sender, MultiLiveSummarizeEventArgs e)
        {
            SystemsController.MultiSummarize(e);
        }

        #endregion
    }
}
