using StreamerBotLib.Models;
using StreamerBotLib.Static;

using System;
using System.Windows;
using System.Windows.Controls;

namespace StreamerBot
{
    public partial class StreamerBotWindow
    {
        #region Tab Size Changes

        private bool UserFollowTabEventAdded = false;
        private bool InOutRaidsEventAdded = false;

        private void GridReSizeEventHandlers()
        {
            void CheckSetting(bool OptionTabSetting, Grid targetGrid, ref bool EventHandleAdded)
            {
                if (OptionTabSetting && !EventHandleAdded)
                {
                    targetGrid.SizeChanged += Grid_SizeChanged;
                    EventHandleAdded = true;
                }
                else
                {
                    targetGrid.SizeChanged -= Grid_SizeChanged;
                    EventHandleAdded = false;
                }
            }

            CheckSetting(OptionFlags.GridTabifyUserFollow, Grid_UserData_UserFollow, ref UserFollowTabEventAdded);
            CheckSetting(OptionFlags.GridTabifyStreamRaids, Grid_SD_Raids, ref InOutRaidsEventAdded);
        }

        private void Grid_SizeChanged(object sender, SizeChangedEventArgs e)
        {
            GUITabifyRecord record = new() { SourceGrid = (sender as Grid) };
            int GridWidth = 0;

            foreach (UIElement element in record.SourceGrid.Children)
            {
                if (element.GetType() == typeof(TextBlock))
                {
                    GridWidth = Convert.ToInt32(((TextBlock)element).Text);
                }
                else if (element.GetType() == typeof(TabControl))
                {
                    record.TargetTabControl = (TabControl)element;
                }
            }

            if (e.NewSize.Width <= GridWidth && record.TargetTabControl.Visibility == Visibility.Collapsed)
            {
                GridSizingHelper(record, true);
            }
            else if (e.NewSize.Width > GridWidth && record.TargetTabControl.Visibility == Visibility.Visible)
            {
                GridSizingHelper(record, false);
            }

        }

        private void GridSizingHelper(GUITabifyRecord tabifyRecord, bool Tabify)
        {
            if (Tabify)
            {
                tabifyRecord.TargetTabControl.Visibility = Visibility.Visible;

                int x = 0;
                foreach(UIElement element in tabifyRecord.SourceGrid.Children)
                {
                    if (element.GetType() == typeof(DockPanel))
                    {
                        ((TabItem)tabifyRecord.TargetTabControl.Items[x]).Content = element;
                        x++;
                    }
                }

                for(int y = 0; y < 2; y++)
                {
                    tabifyRecord.SourceGrid.Children.Remove((UIElement)((TabItem)tabifyRecord.TargetTabControl.Items[y]).Content);
                }
            }
            else
            {
                tabifyRecord.TargetTabControl.Visibility = Visibility.Collapsed;

                for (int y = 0; y < 2; y++)
                { 
                    UIElement dockpanelitem = (UIElement)((TabItem)tabifyRecord.TargetTabControl.Items[y]).Content;
                    ((TabItem)tabifyRecord.TargetTabControl.Items[y]).Content = null;
                    tabifyRecord.SourceGrid.Children.Add(dockpanelitem);
                }
            }
        }

        #endregion

    }
}
