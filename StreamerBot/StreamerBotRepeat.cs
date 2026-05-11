using StreamerBotLib.GUI.Helpers;
using StreamerBotLib.Models.Repeat;
using StreamerBotLib.Properties;
using StreamerBotLib.Static;

using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;

namespace StreamerBot
{
    public partial class StreamerBotWindow
    {
        private void RadioButton_RepeatCommand_SerialMode_Loaded(object sender, RoutedEventArgs e)
        {
            switch (RadioButton_RepeatCommand_SerialMode?.IsChecked)
            {
                case true:
                    GroupBox_Options_RepeatSerialModeSettings.Visibility = Visibility.Visible;
                    break;
                case false or null:
                    GroupBox_Options_RepeatSerialModeSettings.Visibility = Visibility.Collapsed;
                    break;
            }
        }

        private void RadioButton_RepeatCommand_ParallelMode_Checked(object sender, RoutedEventArgs e)
        {
            Controller.ActivateRepeatTimers();
        }

        private void RadioButton_RepeatCommand_SerialMode_Checked(object sender, RoutedEventArgs e)
        {
            if (GroupBox_Options_RepeatSerialModeSettings != null)
            {
                GroupBox_Options_RepeatSerialModeSettings.Visibility = Visibility.Visible;
            }

            Controller.ActivateRepeatTimers();
        }

        private void Button_Options_RepeatSerialCommandList_Add_Click(object sender, RoutedEventArgs e)
        {
            ComboBox selectedCommand = ((ComboBox)(((StackPanel)((Button)sender).Parent).Children[1]));

            if (selectedCommand.SelectedItem != null)
            {
                OptionFlags.RepeatSerialSaveDataString.Add(selectedCommand.Text);
                Settings.Default.Save();
                ListBox_Options_RepeatSerialCommandList.ItemsSource = OptionFlags.RepeatSerialSaveData;
                Controller.UpdateRepeatCommands();
            }
        }

        private void Button_Options_RepeatSeralCommandList_Remove_Click(object sender, RoutedEventArgs e)
        {
            OptionFlags.RepeatSerialSaveData = (List<RepeatCommandGUISelect>)ListBox_Options_RepeatSerialCommandList.ItemsSource;
            ListBox_Options_RepeatSerialCommandList.ItemsSource = OptionFlags.RepeatSerialSaveData;
            Controller.UpdateRepeatCommands();
        }

        private void RadioButton_RepeatCommand_SerialMode_Unchecked(object sender, RoutedEventArgs e)
        {
            GroupBox_Options_RepeatSerialModeSettings.Visibility = Visibility.Collapsed;
        }


        private Point _dragStartPoint;
        private InsertionAdorner _currentAdorner;

        private void ListBox_Options_RepeatSerialCommandList_PreviewMouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            _dragStartPoint = e.GetPosition(null);
        }

        private void ListBoxItem_Options_RepeatSerialCommandList_PreviewMouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            if (sender is ListBoxItem item && item.DataContext is RepeatCommandGUISelect data)
            {
                _dragStartPoint = e.GetPosition(null);
                item.IsSelected = true;

                // Optional: mark the dragged item
                data.IsSelected = true;
            }
        }

        private void ListBox_Options_RepeatSerialCommandList_PreviewMouseMove(object sender, MouseEventArgs e)
        {
            if (e.LeftButton != MouseButtonState.Pressed) return;

            Point mousePos = e.GetPosition(null);
            Vector diff = _dragStartPoint - mousePos;

            if (Math.Abs(diff.X) > SystemParameters.MinimumHorizontalDragDistance ||
                Math.Abs(diff.Y) > SystemParameters.MinimumVerticalDragDistance)
            {
                if (sender is ListBoxItem item && item.DataContext is RepeatCommandGUISelect data)
                {
                    DragDrop.DoDragDrop(item, data, DragDropEffects.Move);
                }
            }
        }

        private void ListBoxItem_Options_RepeatSerialCommandList_PreviewMouseMove(object sender, MouseEventArgs e)
        {
            if (e.LeftButton != MouseButtonState.Pressed)
                return;

            Point mousePos = e.GetPosition(null);
            Vector diff = _dragStartPoint - mousePos;

            if (Math.Abs(diff.X) > SystemParameters.MinimumHorizontalDragDistance ||
                Math.Abs(diff.Y) > SystemParameters.MinimumVerticalDragDistance)
            {
                if (sender is ListBoxItem item && item.DataContext != null)
                {
                    // Start drag with the actual data item
                    DragDrop.DoDragDrop(item, item.DataContext, DragDropEffects.Move);
                }
            }
        }

        private void ListBox_Options_RepeatSerialCommandList_Drop(object sender, DragEventArgs e)
        {
            RemoveAdorner();

            if (!e.Data.GetDataPresent(typeof(RepeatCommandGUISelect))) return;

            var droppedData = e.Data.GetData(typeof(RepeatCommandGUISelect)) as RepeatCommandGUISelect;
            var listBox = sender as ListBox;
            var targetData = GetItemAtCurrentPosition(listBox, e.GetPosition(listBox));
            var items = listBox?.ItemsSource as IList<RepeatCommandGUISelect>;

            if (targetData == null || droppedData == targetData || items == null) return;

            var targetItem = FindListBoxItem(listBox, targetData);
            bool insertBelow = targetItem != null && IsMouseBelowMidpoint(targetItem, e.GetPosition(targetItem));

            int removeIndex = items.IndexOf(droppedData);
            int targetIndex = items.IndexOf(targetData);

            if (removeIndex < 0 || targetIndex < 0) return;

            if (insertBelow)
                targetIndex++;

            items.RemoveAt(removeIndex);
            if (removeIndex < targetIndex)
                targetIndex--;

            items.Insert(targetIndex, droppedData);

            // Optional: reset IsSelected flags if you use them only for drag
            foreach (var item in items)
            {
                item.IsSelected = false;
            }

            listBox.Items.Refresh();
            OptionFlags.RepeatSerialSaveData = (List<RepeatCommandGUISelect>)ListBox_Options_RepeatSerialCommandList.ItemsSource;
        }

        // Helper: Get item under mouse
        private RepeatCommandGUISelect GetItemAtCurrentPosition(ListBox listBox, Point point)
        {
            var hitTest = listBox.InputHitTest(point) as DependencyObject;
            while (hitTest != null && !(hitTest is ListBoxItem))
            {
                hitTest = VisualTreeHelper.GetParent(hitTest);
            }

            var listBoxItem = hitTest as ListBoxItem;
            return listBoxItem?.DataContext as RepeatCommandGUISelect;
        }

        private void ListBox_Options_RepeatSerialCommandList_DragOver(object sender, DragEventArgs e)
        {
            if (sender is not ListBox listBox) return;

            var targetData = GetItemAtCurrentPosition(listBox, e.GetPosition(listBox));
            if (targetData == null)
            {
                RemoveAdorner();
                e.Effects = DragDropEffects.None;
                return;
            }

            var targetListBoxItem = FindListBoxItem(listBox, targetData);
            if (targetListBoxItem == null)
            {
                RemoveAdorner();
                return;
            }

            bool isBelow = IsMouseBelowMidpoint(targetListBoxItem, e.GetPosition(targetListBoxItem));

            RemoveAdorner();

            SolidColorBrush glyphBrush = null;
            if (TryFindResource("GlyphColor") is Color color)
            {
                glyphBrush = new SolidColorBrush(color);
            }
            _currentAdorner = new InsertionAdorner(targetListBoxItem, isBelow, glyphBrush);
            AdornerLayer.GetAdornerLayer(targetListBoxItem)?.Add(_currentAdorner);

            e.Effects = DragDropEffects.Move;
            e.Handled = true;
        }

        private ListBoxItem FindListBoxItem(ListBox listBox, RepeatCommandGUISelect data)
        {
            return listBox.ItemContainerGenerator.ContainerFromItem(data) as ListBoxItem;
        }

        private bool IsMouseBelowMidpoint(ListBoxItem item, Point mousePosRelativeToItem)
        {
            return mousePosRelativeToItem.Y > (item.ActualHeight / 2);
        }

        private void RemoveAdorner()
        {
            if (_currentAdorner != null)
            {
                AdornerLayer.GetAdornerLayer(_currentAdorner.AdornedElement)?.Remove(_currentAdorner);
                _currentAdorner = null;
            }
        }
    }
}
