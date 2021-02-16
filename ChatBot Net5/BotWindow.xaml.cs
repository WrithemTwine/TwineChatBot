﻿using ChatBot_Net5.BotIOController;
using ChatBot_Net5.Clients;
using ChatBot_Net5.Models;
using ChatBot_Net5.Properties;
using ChatBot_Net5.Data;

using System;
using System.Diagnostics;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;

namespace ChatBot_Net5
{
    /// <summary>
    /// Interaction logic for BotWindow.xaml
    /// </summary>
    public partial class BotWindow : Window
    {
        private readonly ChatPopup CP;

        public BotWindow()
        {
            // move settings to the newest version, if the application version upgrades
            if (Settings.Default.UpgradeRequired )
            { 
                Settings.Default.Upgrade();
                Settings.Default.UpgradeRequired = false;
                Settings.Default.Save();
            }

            InitializeComponent();

            CP = new ChatPopup
            {
                Page_ChatPopup_RichText = RichTextBox_ChatBox
            };
            CP.Page_ChatPopup_RichText.Opacity = Slider_PopOut_Opacity.Value;

        }

        private void DG_AddingNewItem(object sender, AddingNewItemEventArgs e)
        {

        }

        private void Button_PreviewMouseLeftButtonDown_DGJoinList(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {

        }

        private void OnWindowClosing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            (Resources["ControlBot"] as BotController).ExitSave();
            Settings.Default.Save();
        }

        private void BC_Twitch_StartBot(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            BotController io = (sender as RadioButton).DataContext as BotController;
            io.StartBot();
            ToggleInputEnabled();
        }

        private void ToggleInputEnabled()
        {
            TB_Twitch_AccessToken.IsEnabled = !TB_Twitch_AccessToken.IsEnabled;
            TB_Twitch_BotUser.IsEnabled = !TB_Twitch_BotUser.IsEnabled;
            TB_Twitch_Channel.IsEnabled = !TB_Twitch_Channel.IsEnabled;
            TB_Twitch_ClientID.IsEnabled = !TB_Twitch_ClientID.IsEnabled;
            Btn_Twitch_RefreshDate.IsEnabled = !Btn_Twitch_RefreshDate.IsEnabled;
            Slider_TimePollSeconds.IsEnabled = !Slider_TimePollSeconds.IsEnabled;
        }

        private void BC_Twitch_StopBot(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            BotController io = (sender as RadioButton).DataContext as BotController;
            io.StartBot();
            ToggleInputEnabled();
        }

        private void PopOutChatButton_Click(object sender, RoutedEventArgs e)
        {
            CP.Visibility = Visibility.Visible;
            CP.Height = 500;
            CP.Width = 300;
        }

        private void Hyperlink_RequestNavigate(object sender, System.Windows.Navigation.RequestNavigateEventArgs e)
        {
            Process.Start(new ProcessStartInfo(e.Uri.AbsoluteUri));
            e.Handled = true;
        }

        private void Button_Click(object sender, RoutedEventArgs e) => Twitch_RefreshDate.Content = DateTime.Now.AddDays(60);

        private void TextBox_LostFocus(object sender, RoutedEventArgs e) => CheckFocus();

        /// <summary>
        /// Check the conditions for starting the bot, where the data fields require data before the bot can be successfully started.
        /// </summary>
        internal void CheckFocus()
        {
            if (TB_Twitch_Channel.Text.Length != 0 && TB_Twitch_BotUser.Text.Length != 0 && TB_Twitch_ClientID.Text.Length != 0 && TB_Twitch_AccessToken.Text.Length != 0)
            {
                Radio_Twitch_StartBot.IsEnabled = true;
                Radio_Twitch_StopBot.IsEnabled = true;
            }
        }

        private void TextBox_SourceUpdated(object sender, System.Windows.Data.DataTransferEventArgs e) => CheckFocus();

        private void DG_CommonMsgs_AutoGeneratedColumns(object sender, EventArgs e)
        {
            DataGrid dg = (sender as DataGrid);

            switch (dg.Name)
            {
                case "DG_CommonMsgs":
                    foreach (DataGridColumn dc in dg.Columns)
                    {
                        if (dc.Header.ToString() == "Name")
                        {
                            dc.IsReadOnly = true;
                        }
                    }
                    break;
            }

        }

        private void Window_Loaded(object sender, RoutedEventArgs e) => CheckFocus();

        private void RichTextBox_ChatBox_DataContextChanged(object sender, DependencyPropertyChangedEventArgs e)
        {
            RichTextBox r = (sender as RichTextBox);
            r.Document = (r.DataContext as FlowDocument);
        }

        private void TextBlock_TwitchBotLog_TextChanged(object sender, TextChangedEventArgs e) => (sender as TextBox).ScrollToEnd();
    }
}
