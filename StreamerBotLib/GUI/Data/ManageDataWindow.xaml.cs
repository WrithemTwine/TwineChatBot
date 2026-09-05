using Microsoft.Win32;

using StreamerBotLib.DataSQL.Models.Converters;
using StreamerBotLib.DataSQL.TableMeta;
using StreamerBotLib.Models.Enums;
using StreamerBotLib.Static;
using StreamerBotLib.Systems;
using StreamerBotLib.Systems.Overlay.Static;

using System.ComponentModel;
using System.IO;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;

namespace StreamerBotLib.GUI.Data
{
    /// <summary>
    /// Interaction logic for ManageDataWindow.xaml
    /// </summary>
    public partial class ManageDataWindow : Window
    {
        public event EventHandler<EventArgs> SaveRecordEvent;
        public event EventHandler<EventArgs> CancelRecordEvent;

        private TableMeta _tableMeta;

        public TableMeta SetTableMeta
        {
            set
            {
                _tableMeta = value;
                DataContext = _tableMeta.BindingList;
            }
        }

        public DataBot SetDataBot { private get; set; }

        private readonly string FilePathInfo = LocalizedMsgSystem.GetVar("HelpPath");

        internal Dictionary<string, List<string>> TableDataPairs { get; set; }

        public ManageDataWindow()
        {
            InitializeComponent();
        }

        #region Helper Methods
        /// <summary>
        /// Event to allow the user to click a TextBox and the cursor highlights all text.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private async void PreviewMouseLeftButton_SelectAll(object sender, MouseButtonEventArgs e)
        {
            await Application.Current.Dispatcher.InvokeAsync((sender as TextBox).SelectAll);
        }

        #endregion

        #region Data Table lookup operations

        private void ComboBox_SelectTable_Initialized(object sender, EventArgs e)
        {
            (sender as ComboBox).ItemsSource = new List<string>((Enum.GetNames<DataTables>()));
        }

        private void ComboBox_SelectTable_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            SetDataBot.GetTableFields((sender as ComboBox).SelectedItem.ToString(),
                (fields) =>
                    ThreadManager.AddTaskToGUIDispatcher(() =>
                    {
                        foreach (var combo in new List<ComboBox>
                          { FindComboBoxInList("KeyField"),
                            FindComboBoxInList("CurrencyField"),
                            FindComboBoxInList("DataField") })
                        {
                            combo.ItemsSource = fields;
                        }
                    }
            ));
        }

        private ComboBox _comboboxActionType, _comboBoxPerformType, _comboBoxActionName, _comboBoxPerformName;

        private void ComboBox_ModActionType_Initialized(object sender, EventArgs e)
        {
            if ((sender as ComboBox).DataContext is TableMeta.EntityData data)
            {
                if (data.Name == "ModActionType")
                {
                    _comboboxActionType = sender as ComboBox;
                }
                else if (data.Name == "ModPerformType")
                {
                    _comboBoxPerformType = sender as ComboBox;
                }
                (sender as ComboBox).ItemsSource = Enum.GetValues(data.ColType);
            }
        }

        private void ComboBox_ModPerformName_Initialized(object sender, EventArgs e)
        {
            if ((sender as ComboBox).DataContext is TableMeta.EntityData data)
            {
                if (data.Name == "ModActionName")
                {
                    _comboBoxActionName = sender as ComboBox;
                }
                else if (data.Name == "ModPerformAction")
                {
                    _comboBoxPerformName = sender as ComboBox;
                }
            }
        }

        private void ComboBox_SelectModActionType_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            void UpdateComboBoxItems(ComboBox source, ComboBox target)
            {
                target?.ItemsSource = TableDataPairs[source.SelectedItem?.ToString()];
            }

            if (_comboboxActionType == sender)
            {
                UpdateComboBoxItems(_comboboxActionType, _comboBoxActionName);
            }
            else
            {
                UpdateComboBoxItems(_comboBoxPerformType, _comboBoxPerformName);
            }
        }

        //private void ListBox_SelectCategory_Initialized(object sender, EventArgs e)
        //{
        //    SetDataBot.GetGameCategories((categories) =>
        //    {
        //        ThreadManager.AddTaskToGUIDispatcher(() =>
        //        {
        //            (sender as ListBox).ItemsSource = (from C in categories select C.CategoryName).ToList();
        //        });
        //    });
        //}

        private TextBox _categoryTextBox;
        private ListBox _categoryListBox;
        private TableMeta.EntityData _categoryEntityData;

        private void TextBox_CategoryText_Initialized(object sender, EventArgs e)
        {
            _categoryTextBox = sender as TextBox;
            var entityData = _categoryTextBox.DataContext as TableMeta.EntityData;

            if (entityData != null)
            {
                _categoryTextBox.Text = new CategoryConverter().Convert(
                    entityData.Value, typeof(string), null, null) as string;
            }
        }

        private void ListBox_CategoryList_Initialized(object sender, EventArgs e)
        {
            _categoryListBox = sender as ListBox;
        }

        private void Category_TextBox_PreviewMouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            _categoryTextBox ??= sender as TextBox;
            _categoryEntityData = _categoryTextBox.DataContext as TableMeta.EntityData;

            // Build the CheckBox list from the current value
            var checkBoxes = new EditConvertCategory().Convert(
                _categoryEntityData.Value,
                typeof(List<CheckBox>),
                null, null) as List<CheckBox>;

            // Attach the single handler that owns the “All” logic
            foreach (var cb in checkBoxes)
            {
                cb.Checked += CategoryCheckBox_Clicked;
                cb.Unchecked += CategoryCheckBox_Clicked;
            }

            _categoryListBox.ItemsSource = checkBoxes;

            _categoryTextBox.Visibility = Visibility.Collapsed;
            _categoryListBox.Visibility = Visibility.Visible;
            _categoryListBox.Focus();
        }

        private void Category_ListBox_PreviewLeftMouseDown(object sender, MouseButtonEventArgs e)
        {
            // Keep focus inside the ListBox when clicking items
            e.Handled = true;

            // Find the actual CheckBox that was clicked (square or label)
            DependencyObject current = e.OriginalSource as DependencyObject;
            while (current != null && current is not CheckBox && current is not ListBox)
                current = VisualTreeHelper.GetParent(current);

            if (current is CheckBox checkBox)
            {
                checkBox.IsChecked = !checkBox.IsChecked;
            }
        }

        private void CategoryCheckBox_Clicked(object sender, RoutedEventArgs e)
        {
            var checkBox = sender as CheckBox;
            if (_categoryListBox.ItemsSource is not ICollection<CheckBox> items) return;

            bool allJustChecked = (string)checkBox.Content == "All" && checkBox.IsChecked == true;

            List<string> selected = [];

            if (allJustChecked)
            {
                // User turned “All” on → clear everything else
                foreach (var cb in items)
                {
                    cb.IsChecked = (string)cb.Content == "All";
                }
                selected = ["All"];
            }
            else
            {
                // User clicked a normal category
                var allItem = items.FirstOrDefault(c => (string)c.Content == "All");
                allItem?.IsChecked = false;

                selected = [.. items
                    .Where(c => c.IsChecked == true)
                    .Select(c => (string)c.Content)];

                // If nothing left checked, force “All”
                if (selected.Count == 0)
                {
                    allItem?.IsChecked = true;
                    selected = ["All"];
                }
            }

            _categoryListBox.SelectedItem = selected;
        }

        private void CategoryList_LostFocus(object sender, RoutedEventArgs e)
        {
            if (_categoryTextBox == null || _categoryListBox == null) return;

            // Prefer the list we just built

            // Fallback: read directly from the checkboxes
            if (_categoryListBox.SelectedItem is not List<string> selected)
            {
                var items = _categoryListBox.ItemsSource as ICollection<CheckBox>;
                selected = items?
                    .Where(c => c.IsChecked == true)
                    .Select(c => (string)c.Content)
                    .ToList() ?? ["All"];
            }

            if (selected.Count == 0)
            {
                selected = ["All"];
            }

            if (_categoryEntityData != null)
            {
                _categoryEntityData.Value = selected;
            }

            _categoryTextBox.Text = new CategoryConverter().Convert(
                selected, typeof(string), null, null) as string;

            _categoryListBox.Visibility = Visibility.Collapsed;
            _categoryTextBox.Visibility = Visibility.Visible;
        }

        //private static void ListBox_Edit_SetSelectedItem(ListBox CatList)
        //{
        //    CheckBox CurrBox = (CheckBox)CatList.SelectedItem;
        //    List<string> selected = [];
        //    CurrBox.IsChecked = !CurrBox.IsChecked;

        //    bool? SelectAll = null;

        //    // first check what is currently selected
        //    // if "All" was now selected, work with the other items
        //    if (CurrBox.Content.ToString() == "All")
        //    {
        //        SelectAll = CurrBox.IsChecked == true;
        //    }
        //    // if other items were selected, then we uncheck "All"
        //    else if (CurrBox.IsChecked == true)
        //    {
        //        SelectAll = false;
        //    }

        //    // step through every checkbox item
        //    foreach (CheckBox c in CatList.ItemsSource)
        //    {
        //        // look for the "All" item, set it to the found item
        //        if (c.Content.ToString() == "All")
        //        {
        //            c.IsChecked = SelectAll;
        //        }
        //        // if the "All" item is selected, uncheck all of the other items
        //        else if (SelectAll == true)
        //        {
        //            c.IsChecked = false;
        //        }

        //        if (c.IsChecked == true)
        //        {
        //            //    CatList.SelectedItems.Add(c.Content);
        //            selected.Add(c.Content.ToString());
        //        }
        //    }

        //    if (selected.Count == 0 || selected.Contains("All"))
        //    {
        //        CatList.SelectedItem = new List<string> { "All" };
        //    }
        //    else
        //    {
        //        CatList.SelectedItem = selected;
        //    }
        //}

        private void ComboBox_SelectEnum_Initialized(object sender, EventArgs e)
        {
            ComboBox combo = sender as ComboBox;
            var data = combo.DataContext as TableMeta.EntityData;

            combo.ItemsSource = Enum.GetValues(data.ColType);

            if (data.Name == "OverlayType")
            {
                // set initial list if a value is already selected
                UpdateOverlayActionList(combo.SelectedItem?.ToString());

                combo.SelectionChanged += (s, args) =>
                {
                    UpdateOverlayActionList((s as ComboBox).SelectedItem?.ToString());
                };
            }
        }

        private void UpdateOverlayActionList(string overlayType)
        {
            if (string.IsNullOrEmpty(overlayType) || !TableDataPairs.ContainsKey(overlayType))
                return;

            // Find the OverlayAction ComboBox that was generated by the template
            var overlayActionCombo = FindComboBoxInList("OverlayAction");
            if (overlayActionCombo != null)
            {
                overlayActionCombo.ItemsSource = TableDataPairs[overlayType];
            }
        }

        private ComboBox FindComboBoxInList(string fieldName)
        {
            foreach (var item in ListBox_DataList.Items)
            {
                var container = ListBox_DataList.ItemContainerGenerator.ContainerFromItem(item) as ListBoxItem;
                if (container == null) continue;

                var combo = FindVisualChild<ComboBox>(container, fieldName);
                if (combo != null)
                    return combo;
            }
            return null;
        }

        private static T FindVisualChild<T>(DependencyObject parent, string fieldName) where T : FrameworkElement
        {
            if (parent == null) return null;

            for (int i = 0; i < VisualTreeHelper.GetChildrenCount(parent); i++)
            {
                var child = VisualTreeHelper.GetChild(parent, i);

                if (child is T element && element.DataContext is TableMeta.EntityData data && data.Name == fieldName)
                    return element;

                var result = FindVisualChild<T>(child, fieldName);
                if (result != null)
                    return result;
            }
            return null;
        }

        #endregion

        #region File Operations

        private void FileBrowser_TextBox_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            TextBox saveMediaPath = sender as TextBox;

            OpenFileDialog pickFile = new()
            {
                Multiselect = false,
                CheckFileExists = true,
                DereferenceLinks = true,
                Title = "Select media file (picture/video) for Overlay event! (needs to be viewable in a webpage)",
                InitialDirectory = Directory.Exists(OptionFlags.MediaOverlayMRUPathSelect) ? OptionFlags.MediaOverlayMRUPathSelect : Directory.GetCurrentDirectory()
            };


            if (pickFile.ShowDialog() == true)
            {
                saveMediaPath.Text = pickFile.FileName;
                OptionFlags.MediaOverlayMRUPathSelect = Path.GetDirectoryName(saveMediaPath.Text);
            }
        }

        /// <summary>
        /// Copy the file to a subfolder named with the OverlayType.
        /// </summary>
        /// <param name="FileName">Path and file from which to copy.</param>
        /// <param name="OverlayType">The name of the overlaytype, which becomes the subfolder name to hold the file in the current application folder.</param>
        private static string FileCopy(string FileName, string OverlayType)
        {
            string resultfile;
            string CopyFile = Path.Combine(PublicConstants.BaseOverlayPath, OverlayType, Path.GetFileName(FileName)).Replace("_", " ").Replace(" ", ""); // replace '_' to prevent issues with converting class object to string to class object

            if (FileName == "")
            {
                resultfile = null;
            }
            else if (!File.Exists(CopyFile) && File.Exists(FileName))
            {
                Directory.CreateDirectory(Path.GetDirectoryName(CopyFile));
                File.Copy(FileName, CopyFile, false);
                resultfile = CopyFile;
            }
            else if (!File.Exists(FileName))
            {
                resultfile = FileName;
            }
            else
            {
                resultfile = CopyFile;
            }
            return Path.GetRelativePath(Directory.GetCurrentDirectory(), resultfile);
        }

        #endregion

        #region Window Closing Events

        private bool _isClosing = false;

        private void Button_OKClick(object sender, RoutedEventArgs e)
        {
            try
            {
                lock (_tableMeta.CurrEntity)
                {
                    if (_tableMeta.CurrEntity.TableName == "OverlayServices")
                    {
                        try
                        {
                            _tableMeta.CurrEntity.Values["MediaFile"] = ProcessFile("MediaFile");
                            _tableMeta.CurrEntity.Values["ImageFile"] = ProcessFile("ImageFile");
                        }
                        catch (Exception ex)
                        {
                            LogWriter.LogException(ex, "Button_OKClick");
                        }
                    }
                    SaveRecordEvent?.Invoke(this, new());
                }
            }
            catch (Exception ex)
            {
                LogWriter.LogException(ex, "Button_OKClick");
            }
            Close();

            object ProcessFile(string Key)
            {
                if (((string)_tableMeta.CurrEntity.Values[Key]) == FilePathInfo)
                {
                    return _tableMeta.CurrEntity.Values[Key];
                }
                else
                {
                    return (string)_tableMeta.CurrEntity.Values[Key] == null ? FilePathInfo : FileCopy((string)_tableMeta.CurrEntity.Values[Key], _tableMeta.CurrEntity.Values["OverlayType"].ToString());
                }
            }
        }

        private void CancelButton_Click(object sender, RoutedEventArgs e)
        {
            if (_isClosing) return;

            CancelRecordEvent?.Invoke(this, new EventArgs());
            _isClosing = true;
            Close();
        }

        /// <summary>
        /// Handles event when user closes the window.
        /// Effectively, closing the window is clicking the cancel button.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void Window_Closing(object sender, CancelEventArgs e)
        {
            if (_isClosing) return;

            _isClosing = true;
            CancelRecordEvent?.Invoke(this, new EventArgs());
        }


        #endregion

    }
}
