
using StreamerBotLib.Themes;

using System;
using System.Windows;

namespace StreamerBot
{
    public partial class StreamerBotWindow : Window
    {
        /// <summary>
        /// Handles the RadioButton 'Checked' event for user selected app theme.
        /// </summary>
        /// <param name="sender">Object sending the event.</param>
        /// <param name="e">Parameters to the event.</param>
        private void RadioButton_Theme_Checked(object sender, RoutedEventArgs e)
        {
            SetTheme();
        }

        /// <summary>
        /// Updates the current theme per the user's selection.
        /// </summary>
        private void SetTheme()
        {
            Application.Current.Resources.MergedDictionaries[0].Source = new(ThemeSelector.GetCurrentTheme(), UriKind.Absolute);
        }
    }
}
