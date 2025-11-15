using StreamerBotLib.BotIOController;
using StreamerBotLib.Models.Events;
using StreamerBotLib.Systems.MultiLive;

using System.Windows;
using System.Windows.Controls;

namespace StreamerBot
{
    public partial class StreamerBotWindow
    {
        #region MultiLive

        private void MultiLive_Data_Loaded(object sender, RoutedEventArgs e)
        {
            Controller.HandleOnDataCollectionUpdated((MultiLive_Data.Content as MultiLiveDataGrids).DataManager_OnDataCollectionUpdated);

            // allow edits while bot is active
            (MultiLive_Data.Content as MultiLiveDataGrids).SetIsEnabled(true);
            (MultiLive_Data.Content as MultiLiveDataGrids).SetHandlers(Settings_LostFocus, TB_BotActivityLog_TextChanged);
            (MultiLive_Data.Content as MultiLiveDataGrids).SummarizeChannels += StreamerBotWindow_SummarizeChannels;
            (MultiLive_Data.Content as MultiLiveDataGrids).FindMultiChannelUserId += StreamerBotWindow_FindMultiChannelUserId;
            (MultiLive_Data.Content as MultiLiveDataGrids).AddNewMultiChannelUser += StreamerBotWindow_AddNewMultiChannelUser;
            (MultiLive_Data.Content as MultiLiveDataGrids).GUISaveEdits = Controller.GUISaveDataGridEdits;
            (MultiLive_Data.Content as MultiLiveDataGrids).PreviewKeyDownDeleteRows += MultiLive_DG_PreviewKeyDown_Click;
            (MultiLive_Data.Content as MultiLiveDataGrids).MenuItemDeleteClick += MenuItem_DeleteClick;
            (MultiLive_Data.Content as MultiLiveDataGrids).MenuItemEnabledClick += DataGridContextMenu_EnableItems_Click;
            (MultiLive_Data.Content as MultiLiveDataGrids).MenuItemDisabledClick += DataGridContextMenu_DisableItems_Click;
            AddMultiLiveFoundUserId += (MultiLive_Data.Content as MultiLiveDataGrids).UpdateMultiChannelUserId;

#if DEBUG
            (MultiLive_Data.Content as MultiLiveDataGrids).DebugAddNewMultiLiveData += StreamerBotWindow_DebugAddNewMultiLiveData;
#endif

            ((Frame)sender).Loaded -= MultiLive_Data_Loaded;
        }

#if DEBUG
        private void StreamerBotWindow_DebugAddNewMultiLiveData(object sender, EventArgs e)
        {
            Controller.DebugAddNewMultiLiveData();
        }
#endif

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
