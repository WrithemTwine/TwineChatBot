using StreamerBotLib.Enums;
using StreamerBotLib.Events;

using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace StreamerBot
{
    public partial class StreamerBotWindow
    {
        #region Giveaway

        private delegate void RefreshChannelPoints();

        private void TabItem_Giveaways_Loaded(object sender, RoutedEventArgs e)
        {
            //BeginGiveawayChannelPtsUpdate();
            CheckGiveawayFocusStatus();
        }

        private void GuiTwitchBot_GiveawayEvents(object sender, BotStartStopEventArgs e)
        {
            if (e.BotName == Bots.TwitchChatBot)
            {
                BeginGiveawayChannelPtsUpdate();
            }
        }

        private void Button_RefreshChannelPoints_Click(object sender, RoutedEventArgs e)
        {
            Button_Giveaway_RefreshChannelPoints.IsEnabled = false;
            Button_ChannelPts_Refresh.IsEnabled = false;
            ChannelPtRetrievalDate = DateTime.MinValue; // reset the retrieve date to force retrieval
            BeginGiveawayChannelPtsUpdate();
        }

        private void BeginGiveawayChannelPtsUpdate()
        {
            if (DateTime.Now >= ChannelPtRetrievalDate + ChannelPtRefresh)
            {
                _ = Dispatcher.BeginInvoke(new RefreshBotOp(UpdateData), Button_Giveaway_RefreshChannelPoints, new Action<string>((s) => guiTwitchBot.GetChannelPoints(UserName: s)));
                ChannelPtRetrievalDate = DateTime.Now;
            }
        }

        private void TwitchBotUserSvc_GetChannelPoints(object sender, OnGetChannelPointsEventArgs e)
        {
            Dispatcher.Invoke(() =>
            {
                UpdateGiveawayList(e.ChannelPointNames);
                UpdateOverlayChannelPointList(e.ChannelPointNames);
            });
        }

        private void UpdateGiveawayList(List<string> ChannelPointNames)
        {
            ComboBox_Giveaway_ChanPts.ItemsSource = ChannelPointNames;
            ComboBox_ChannelPoints.ItemsSource = ChannelPointNames;
            Button_Giveaway_RefreshChannelPoints.IsEnabled = true;
            Button_ChannelPts_Refresh.IsEnabled = true;
        }

        private void Button_GiveawayBegin_Click(object sender, RoutedEventArgs e)
        {
            GiveawayTypes givetype = GiveawayTypes.None;
            if (RadioButton_GiveawayCommand.IsChecked == true)
            {
                givetype = GiveawayTypes.Command;
            }
            else if (RadioButton_GiveawayCustomRewards.IsChecked == true)
            {
                givetype = GiveawayTypes.CustomRewards;
            }

            string ItemName = "";

            switch (givetype)
            {
                case GiveawayTypes.Command:
                    ItemName = (string)ComboBox_Giveaway_Coms.SelectedValue;
                    break;
                case GiveawayTypes.CustomRewards:
                    ItemName = (string)ComboBox_Giveaway_ChanPts.SelectedValue;
                    break;
            }

            Controller.HandleGiveawayBegin(givetype, ItemName);
            Giveaway_Toggle(false);

            Button_GiveawayBegin.IsEnabled = false;
            Button_GiveawayEnd.IsEnabled = true;
        }

        private void Giveaway_Toggle(bool Enabled = true)
        {
            RadioButton_GiveawayCustomRewards.IsEnabled = Enabled;
            RadioButton_GiveawayCommand.IsEnabled = Enabled;
            ComboBox_Giveaway_ChanPts.IsEnabled = Enabled;
            ComboBox_Giveaway_Coms.IsEnabled = Enabled;
        }

        private void Button_GiveawayEnd_Click(object sender, RoutedEventArgs e)
        {
            Controller.HandleGiveawayEnd();
            Giveaway_Toggle();
            Button_GiveawayBegin.IsEnabled = true;
            Button_GiveawayEnd.IsEnabled = false;
            Button_GiveawayPickWinner.IsEnabled = true;
        }

        private void Button_GiveawayPickWinner_Click(object sender, RoutedEventArgs e)
        {
            Controller.HandleGiveawayWinner();
            Giveaway_Toggle();
            Button_GiveawayBegin.IsEnabled = true;
        }

        private void ComboBox_Giveaway_PreviewMouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            switch (((ComboBox)sender).Name)
            {
                case "ComboBox_Giveaway_ChanPts":
                    RadioButton_GiveawayCustomRewards.IsChecked = true;
                    break;
                case "ComboBox_Giveaway_Coms":
                    RadioButton_GiveawayCommand.IsChecked = true;
                    break;
            }
        }

        private void ComboBox_Giveaway_DropDownClosed(object sender, EventArgs e)
        {
            CheckGiveawayFocusStatus();
        }

        private void CheckGiveawayFocusStatus()
        {
            if (Radio_Twitch_StartBot.IsChecked == true &&
                ((RadioButton_GiveawayCustomRewards.IsChecked == true && (string)ComboBox_Giveaway_ChanPts.SelectedValue != "" && Radio_Twitch_PubSubBotStart.IsChecked == true)
                || (RadioButton_GiveawayCommand.IsChecked == true && (string)ComboBox_Giveaway_Coms.SelectedValue != "")))
            {
                Button_GiveawayBegin.IsEnabled = true;
            }
        }

        #endregion
    }
}
