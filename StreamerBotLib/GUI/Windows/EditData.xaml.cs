using StreamerBotLib.Data;
using StreamerBotLib.Enums;
using StreamerBotLib.Events;
using StreamerBotLib.Systems;

using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace StreamerBotLib.GUI.Windows
{
    /// <summary>
    /// Shows new Window for Adding or Editing DataGrid data rows
    /// </summary>
    public partial class EditData : Window, INotifyPropertyChanged
    {
        private bool IsClosing;

        private DataManager DataManage { get; set; } = SystemsController.DataManage;
        private DataRow SaveDataRow { get; set; }

        private ListView CategoryListView;
        private CheckBox CurrCheckedItem = null;
        private ComboBox TableElement;
        private ComboBox KeyFieldElement;
        private ComboBox DataFieldElement;

        public event EventHandler<UpdatedDataRowArgs> UpdatedDataRow;
        public event PropertyChangedEventHandler PropertyChanged;

        private void UpdatePropertyChanged(string propname)
        {
            PropertyChanged?.Invoke(this, new(propname));
        }

        public EditData()
        {
            InitializeComponent();
        }

        public void LoadData(DataTable dataTable, DataRow dataRow = null)
        {
            Title = dataRow == null ? $"Add new {dataTable.TableName} Row" : $"Edit {dataTable.TableName} Row";

            const int NameWidth = 150;
            bool CheckLockTable = false;
            SaveDataRow = dataRow ?? dataTable.NewRow();

            if (SaveDataRow.Table.TableName is "Commands" or "ChannelEvents")
            {
                List<string> builtInCmds = new();

                foreach (DefaultCommand d in Enum.GetValues(typeof(DefaultCommand)))
                {
                    builtInCmds.Add(d.ToString());
                }

                if (builtInCmds.Contains(dataRow["CmdName"].ToString()) || dataRow.Table.TableName is "ChannelEvents")
                {
                    CheckLockTable = true;
                }
            }


            foreach (DataColumn dataColumn in dataTable.Columns)
            {
                UIElement dataname = new Label() { Content = dataColumn.ColumnName, Width = NameWidth };
                object datavalue = DBNull.Value.Equals(SaveDataRow[dataColumn]) ? "" : SaveDataRow[dataColumn];

                UIElement dataout = Convert(datavalue, dataColumn, CheckLockTable);
                UIElement datatable = new Label() { Content = dataTable.TableName, Visibility = Visibility.Collapsed };

                StackPanel row = new() { Orientation = Orientation.Horizontal };
                row.Children.Add(dataname);
                row.Children.Add(dataout);
                row.Children.Add(datatable);

                ListBox_DataList.Items.Add(row);
            }
        }

        private void ApplyButton_Click(object sender, RoutedEventArgs e)
        {
            Dictionary<string, string> SaveData = new();

            foreach (StackPanel data in ListBox_DataList.Items)
            {
                string name = ((Label)data.Children[0]).Content.ToString();
                string result = ConvertBack(data.Children[1]);

                SaveData.Add(name, result);
            }

            foreach (DataColumn dataColumn in SaveDataRow.Table.Columns)
            {
                if (!dataColumn.ReadOnly)
                {
                    SaveDataRow[dataColumn] = SaveData[dataColumn.ColumnName];
                }
            }

            UpdatedDataRow?.Invoke(this, new() { UpdatedDataRow = SaveDataRow });
            Close();
        }

        private void CancelButton_Click(object sender, RoutedEventArgs e)
        {
            SaveDataRow = null;
            if (!IsClosing)
            {
                Close();
            }
        }

        private void Window_Closing(object sender, CancelEventArgs e)
        {
            IsClosing = true;
            CancelButton_Click(this, new());
        }

        public UIElement Convert(object datavalue, DataColumn dataColumn, bool LockedTable = false)
        {
            const int ValueWidth = 325;
            UIElement dataout = null;

            PopupEditTableDataType dataType = CheckColumn(dataColumn.ColumnName);

            switch (dataType)
            {
                case PopupEditTableDataType.databool:
                    dataout = new CheckBox()
                    {
                        IsChecked = (bool?)datavalue,
                        VerticalAlignment = VerticalAlignment.Center,
                        Width = ValueWidth
                    };
                    break;
                case PopupEditTableDataType.datestring:
                    dataout = new TextBox()
                    {
                        Text = $"{(DateTime)datavalue:yyyy-MM-dd HH-mm-ss}",
                        Width = ValueWidth
                    };
                    break;
                case PopupEditTableDataType.comboenum:
                    List<string> enumlist = new();

                    if (dataColumn.ColumnName is "Permission" or "ViewerTypes")
                    {
                        foreach (ViewerTypes s in Enum.GetValues(typeof(ViewerTypes)))
                        {
                            enumlist.Add(s.ToString());
                        }
                    }
                    else if (dataColumn.ColumnName == "Kind")
                    {
                        foreach (WebhooksKind s in Enum.GetValues(typeof(WebhooksKind)))
                        {
                            enumlist.Add(s.ToString());
                        }
                    }
                    else if (dataColumn.ColumnName == "action")
                    {
                        foreach (CommandAction s in Enum.GetValues(typeof(CommandAction)))
                        {
                            enumlist.Add(s.ToString());
                        }
                    }
                    else if (dataColumn.ColumnName == "sort")
                    {
                        foreach (DataSort s in Enum.GetValues(typeof(DataSort)))
                        {
                            enumlist.Add(s.ToString());
                        }
                    }
                    else if (dataColumn.ColumnName == "ModAction")
                    {
                        foreach (ModActions s in Enum.GetValues(typeof(ModActions)))
                        {
                            enumlist.Add(s.ToString());
                        }
                    }
                    else if (dataColumn.ColumnName == "MsgType")
                    {
                        foreach (MsgTypes s in Enum.GetValues(typeof(MsgTypes)))
                        {
                            enumlist.Add(s.ToString());
                        }
                    }
                    else if (dataColumn.ColumnName == "BanReason")
                    {
                        foreach (BanReason s in Enum.GetValues(typeof(BanReason)))
                        {
                            enumlist.Add(s.ToString());
                        }
                    }

                    dataout = new ComboBox()
                    {
                        ItemsSource = enumlist,
                        SelectedValue = datavalue,
                        VerticalAlignment = VerticalAlignment.Center,
                        Width = ValueWidth
                    };
                    break;
                case PopupEditTableDataType.combotable:
                    List<string> combocollection = new();
                    if (dataColumn.Table.TableName == "CategoryList")
                    {
                        dataout = new TextBox()
                        {
                            Text = datavalue.ToString(),
                            VerticalAlignment = VerticalAlignment.Center,
                            Width = ValueWidth,
                            TextWrapping = TextWrapping.Wrap
                        };
                    }
                    else
                    {
                        if (dataColumn.ColumnName == "Category")
                        {
                            List<CheckBox> CBcombocollection = new();
                            List<string> selection = new();
                            selection.AddRange(((string)datavalue).Split(", "));
                            foreach (Tuple<string, string> tuple in DataManage.GetGameCategories())
                            {
                                CheckBox item = new()
                                {
                                    Content = tuple.Item2,
                                    IsChecked = selection.Contains(tuple.Item2)
                                };
                                item.Checked += EditData_Category_Checked;
                                item.Unchecked += EditData_Category_Checked;

                                CBcombocollection.Add(
                                        item
                                    );
                            }
                            dataout = new ListView()
                            {
                                Width = ValueWidth,
                                VerticalAlignment = VerticalAlignment.Center,
                                ItemsSource = CBcombocollection,
                                MaxHeight = 200
                            };
                            CategoryListView = (ListView)dataout;
                        }
                        else if (dataColumn.ColumnName == "table")
                        {
                            combocollection.AddRange(DataManage.GetTableNames());

                            dataout = new ComboBox()
                            {
                                Name = "table",
                                Width = ValueWidth,
                                VerticalAlignment = VerticalAlignment.Center,
                                ItemsSource = combocollection,
                                SelectedValue = datavalue,
                            };
                            ((ComboBox)dataout).SelectionChanged += TableData_SelectionChanged;

                            TableElement = (ComboBox)dataout;
                        }
                        else if (dataColumn.ColumnName == "key_field")
                        {
                            ComboBox GUItable = TableElement;

                            if (GUItable != null)
                            {
                                combocollection.Add(DataManage.GetKey((string)GUItable.SelectedValue));
                            }

                            dataout = new ComboBox()
                            {
                                Name = "keyfield",
                                Width = ValueWidth,
                                VerticalAlignment = VerticalAlignment.Center,
                                ItemsSource = combocollection,
                                SelectedValue = datavalue
                            };

                            KeyFieldElement = (ComboBox)dataout;
                        }
                        else if (dataColumn.ColumnName == "data_field")
                        {
                            ComboBox GUItable = TableElement;

                            if (GUItable != null && GUItable.SelectedValue != null)
                            {
                                combocollection.AddRange(DataManage.GetTableFields((string)GUItable.SelectedValue));
                            }

                            dataout = new ComboBox()
                            {
                                Name = "datafield",
                                Width = ValueWidth,
                                VerticalAlignment = VerticalAlignment.Center,
                                ItemsSource = combocollection,
                                SelectedValue = datavalue
                            };
                            DataFieldElement = (ComboBox)dataout;
                        }
                        else if (dataColumn.ColumnName == "currency_field")
                        {
                            ComboBox GUItable = TableElement;

                            if (GUItable != null)
                            {
                                if ((string)GUItable.SelectedValue == "Currency")
                                {
                                    combocollection.AddRange(DataManage.GetCurrencyNames());
                                }
                            }

                            dataout = new ComboBox()
                            {
                                Name = "datafield",
                                Width = ValueWidth,
                                VerticalAlignment = VerticalAlignment.Center,
                                ItemsSource = combocollection,
                                SelectedValue = datavalue
                            };
                        }
                    }
                    break;
                case PopupEditTableDataType.text:
                default:
                    dataout = new TextBox()
                    {
                        Text = datavalue.ToString(),
                        VerticalAlignment = VerticalAlignment.Center,
                        Width = ValueWidth,
                        TextWrapping = TextWrapping.Wrap
                    };
                    break;
            }

            if (dataout != null)
            {
                dataout.IsEnabled = !dataColumn.ReadOnly;
            }

            if (LockedTable && (dataColumn.ColumnName is "Id" or "CmdName" or "AllowParam" or "Usage" or "lookupdata" or "table" or "key_field" or "data_field" or "currency_field" or "unit" or "action" or "top" or "Name"))
            {
                dataout.IsEnabled = false;
            } 
            else if(dataout.GetType() == typeof(TextBox))
            {
                dataout.PreviewMouseLeftButtonDown += PreviewMouseLeftButton_SelectAll;
            }

            return dataout;
        }

         /// <summary>
        /// Check the Category checkboxes and ensure only "All" or the other items are selected, but not both.
        /// </summary>
        /// <param name="sender">The sending CheckBox.</param>
        /// <param name="e">Selection changed events.</param> 
        private void EditData_Category_Checked(object sender, RoutedEventArgs e)
        {
            if (sender != CurrCheckedItem && CurrCheckedItem != null)
            {
                e.Handled = true;
            }
            else
            {
                CheckBox CurrBox = sender as CheckBox;
                CurrCheckedItem = CurrBox;
                ListView CatList = CategoryListView;

                bool? SelectAll = null;

                // first check what is currently selected
                // if "All" was now selected, work with the other items
                if (CurrBox.Content.ToString() == "All")
                {
                    SelectAll = CurrBox.IsChecked == true;
                }
                // if other items were selected, then we uncheck "All"
                else if (CurrBox.IsChecked == true)
                {
                    SelectAll = false;
                }

                // step through every checkbox item
                foreach (CheckBox c in CatList.ItemsSource)
                {
                    // look for the "All" item, set it to the found item
                    if (c.Content.ToString() == "All")
                    {
                        c.IsChecked = SelectAll;
                    }
                    // if the "All" item is selected, uncheck all of the other items
                    else if (SelectAll == true)
                    {
                        c.IsChecked = false;
                    }
                }
                CurrCheckedItem = null;
            }
        }

        private void TableData_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            KeyFieldElement.ItemsSource = new List<string>() { DataManage.GetKey((string)TableElement.SelectedValue) };
            UpdatePropertyChanged(nameof(KeyFieldElement));

            DataFieldElement.ItemsSource = DataManage.GetTableFields((string)TableElement.SelectedValue);
            UpdatePropertyChanged(nameof(DataFieldElement));
        }

        private PopupEditTableDataType CheckColumn(string ColumnName)
        {
            switch (ColumnName)
            {
                case "IsFollower" or "AddMe" or "IsEnabled" or "AllowParam" or "AddEveryone" or "lookupdata":
                    return PopupEditTableDataType.databool;
                case "Permission" or "Kind" or "action" or "sort" or "MsgType" or "ModAction" or "ViewerTypes" or "BanReason":
                    return PopupEditTableDataType.comboenum;
                case "":
                    return PopupEditTableDataType.combolist;
                case "Category" or "table" or "key_field" or "data_field" or "currency_field":
                    return PopupEditTableDataType.combotable;
                case "FollowedDate" or "FirstDateSeen" or "CurrLoginDate" or "LastDateSeen" or "CreatedAt" or "DateTime" or "StreamStart" or "StreamEnd":
                    return PopupEditTableDataType.datestring;
                default:
                    return PopupEditTableDataType.text;
            }
        }

        public string ConvertBack(UIElement dataElement)
        {
            string result = "";

            if (dataElement.GetType() == typeof(CheckBox))
            {
                result = ((CheckBox)dataElement).IsChecked.Value.ToString();
            }
            else if (dataElement.GetType() == typeof(DatePicker))
            {
                result = ((DatePicker)dataElement).Text;
            }
            else if(dataElement.GetType() == typeof(ComboBox))
            {
                result = (string)((ComboBox)dataElement).SelectedValue;
            }
            else if (dataElement.GetType() == typeof(ListBox))
            {
                result = ((string[])((ListBox)dataElement).SelectedValue).ToString();
            } else if (dataElement.GetType() == typeof(ListView))
            {
                List<string> selections = new();
                foreach(CheckBox c in ((ListView)dataElement).ItemsSource )
                {
                    if (c.IsChecked == true)
                    {
                        selections.Add((string)c.Content);
                    }
                }

                result = string.Join(", ", selections);
            }
            else
            {
                result = ((TextBox)dataElement).Text;
            }

            return result;
        }

        private async void PreviewMouseLeftButton_SelectAll(object sender, MouseButtonEventArgs e)
        {
            await Application.Current.Dispatcher.InvokeAsync((sender as TextBox).SelectAll);
        }
    }
}
