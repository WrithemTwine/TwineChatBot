using StreamerBotLib.BotIOController;
using StreamerBotLib.Properties;
using StreamerBotLib.Static;
using StreamerBotLib.Systems;

using System.Configuration;
using System.Windows;
using System.Windows.Controls;

namespace StreamerBot
{
    public partial class StreamerBotWindow
    {
        #region Debug Empty Stream

        private void CheckDebug(object sender, RoutedEventArgs e)
        {
            if (StackPanel_DebugLivestream != null)
            {
                StackPanel_DebugLivestream.Visibility = Settings.Default.DebugLiveStream ? Visibility.Visible : Visibility.Collapsed;
            }
        }

        private DateTime DebugStreamStarted = DateTime.MinValue;

        private void StartDebugStream_Click(object sender, RoutedEventArgs e)
        {
            if (DebugStreamStarted == DateTime.MinValue)
            {
                DebugStreamStarted = DateTime.Now.ToLocalTime();

                string User = OptionFlags.TwitchChannelName;
                string Category = "Microsoft Flight Simulator";
                string ID = "7193";
                string Title = "Testing a debug stream";

                ThreadManager.CreateThreadStart(() =>
                {
                    Controller.HandleOnStreamOnline(User, Title, DebugStreamStarted, ID, Category, Debug: true);

                    List<Tuple<string, string>> output = SystemsController.DataManage.GetGameCategories();
                    Random random = new();
                    Tuple<string, string> itemfound = output[random.Next(output.Count)];
                    Controller.HandleOnStreamUpdate(itemfound.Item1, itemfound.Item2);
                });

                SetLiveStreamActive(true);
            }
        }

        private void EndDebugStream_Click(object sender, RoutedEventArgs e)
        {
            if (DebugStreamStarted != DateTime.MinValue)
            {
                BotController.HandleOnStreamOffline();

                DebugStreamStarted = DateTime.MinValue;

                SetLiveStreamActive(false);
            }

        }

        #endregion

        private void Button_OptionsDebugLog_EnableAll_Click(object sender, RoutedEventArgs e)
        {
            foreach (CheckBox C in ((DebugFlags)Resources["DebugFlags"]).DebugFlagsList)
            {
                C.IsChecked = true;
            }
            NotifyPropertyChanged("DebugFlagsList");
        }

        private void Button_OptionsDebugLog_DisableAll_Click(object sender, RoutedEventArgs e)
        {
            foreach (CheckBox C in ((DebugFlags)Resources["DebugFlags"]).DebugFlagsList)
            {
                C.IsChecked = false;
            }
            NotifyPropertyChanged("DebugFlagsList");
        }

    }

    /// <summary>
    /// Finds a list of "EnableDebug..." settings names and creates a list of CheckBoxes with a 'IsChecked' binding to the settings with "EnableDebug..." names.
    /// </summary>
    public class DebugFlags
    {
        /// <summary>
        /// Collection of CheckBoxes with a binding on 'IsChecked' to all Settings containing (beginning with) "EnableDebug"
        /// </summary>
        public List<CheckBox> DebugFlagsList { get; set; } = [];

        public DebugFlags()
        {
            foreach (var (D, checkBox) in from string D in (from SettingsProperty property in Settings.Default.Properties
                                                            where property.Name.Contains("EnableDebug")
                                                            orderby property.Name
                                                            select property.Name)
                                          let checkBox = new CheckBox()
                                          {
                                              Content = D,
                                              DataContext = Settings.Default
                                          }
                                          select (D, checkBox))
            {
                _ = checkBox.SetBinding(System.Windows.Controls.Primitives.ToggleButton.IsCheckedProperty, D);
                DebugFlagsList.Add(checkBox);
            }
        }

    }
}
