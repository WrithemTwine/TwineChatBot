#define EDIT_EFC

using Microsoft.Win32;

using StreamerBotLib.Enums;
using StreamerBotLib.Events;
using StreamerBotLib.Interfaces;
using StreamerBotLib.Models;
using StreamerBotLib.Overlay.Enums;
using StreamerBotLib.Overlay.Static;
using StreamerBotLib.Static;

using System.ComponentModel;
using System.IO;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Input;
using System.Windows.Media;

namespace StreamerBotLib.GUI.Windows
{
    /// <summary>
    /// Shows new Window for Adding or Editing DataGrid data rows
    /// </summary>
    public partial class EditData : Window, INotifyPropertyChanged
    {
        private bool IsClosing;
        private bool AddedStringEvent;

        private IDataManagerReadOnly DataManage { get; set; }
        private bool IsNewRow { get; set; }
        private List<EditPopupTime> DateList { get; set; } = [];

        private ListBox CategoryListView;
        private CheckBox CurrCheckedItem = null;
        private ComboBox TableElement;
        private ComboBox KeyFieldElement;
        private ComboBox DataFieldElement;

        private ComboBox ModActionTypeElement;
        private ComboBox ModActionElement;
        private ComboBox ModPerformTypeElement;
        private ComboBox ModPerformElement;

        private ComboBox OverlayTypeElement;
        private ComboBox OverlayActionTypeElement;
        private const string OverlayTypeName = "ComboBox_OverlayType";

        private Dictionary<string, List<string>> MediaOverlayEventActions { get; set; } = new();

#if !EDIT_EFC
        private DataRow SaveDataRow { get; set; }

        public event EventHandler<UpdatedDataRowArgs> UpdatedDataRow;
#endif

        public event PropertyChangedEventHandler PropertyChanged;

        private void UpdatePropertyChanged(string propname)
        {
            PropertyChanged?.Invoke(this, new(propname));
        }

        /// <summary>
        /// Constructor, initalizes window
        /// </summary>
        public EditData(IDataManagerReadOnly dataManageReadOnly)
        {
            InitializeComponent();
            IsNewRow = false;
            DataManage = dataManageReadOnly;
        }

#if EDIT_EFC
        internal event EventHandler<AddNewRowEventArgs> AddNewRow;
        internal event EventHandler<UpdatedDataRowArgs> UpdatedRow;

        /// <summary>
        /// Supply the data to populate into the Popup Window.
        /// </summary>
        /// <param name="dataTable">The DataTable used in the current context.</param>
        /// <param name="dataRow">An existing row of data. If null, specifies the user is adding a new data row.</param>
        public void LoadData(IDatabaseTableMeta databaseTableMeta, bool NewRow)
        {
            Title = NewRow ? $"Add new {databaseTableMeta.GetType().Name} Row" : $"Edit {databaseTableMeta.GetType().Name} Row";

            const int NameWidth = 150;
            IsNewRow = NewRow;
            //SaveDataRow = dataRow ?? dataTable.NewRow();

            bool CheckLockTable = databaseTableMeta.TableName is "Commands" or "ChannelEvents";

            foreach (string dataColumn in databaseTableMeta.Meta.Keys)
            {
                AddedStringEvent = false;

                UIElement dataname = new StackPanel() { Orientation = Orientation.Vertical };
                ((StackPanel)dataname).Children.Add(new Label() { Content = dataColumn, Width = NameWidth });

                object datavalue = databaseTableMeta.Values[dataColumn];

                UIElement dataout = Convert(datavalue, dataColumn, databaseTableMeta.TableName, databaseTableMeta.Meta[dataColumn], CheckLockTable);

                // Check after adding "dataout" via Convert() whether we added the string counting event - add labels to hold the string count value
                if (AddedStringEvent)
                {
                    StackPanel CountRows = new() { Orientation = Orientation.Horizontal };
                    CountRows.Children.Add(new Label() { Content = "Current Chars: " });
                    CountRows.Children.Add(new Label());

                    ((StackPanel)dataname).Children.Add(CountRows);
                }

                UIElement datatable = new Label() { Content = databaseTableMeta.TableName, Visibility = Visibility.Collapsed };

                StackPanel row = new() { Orientation = Orientation.Horizontal };
                row.Children.Add(dataname);
                row.Children.Add(dataout);
                row.Children.Add(datatable);

                ListBox_DataList.Items.Add(row);
            }
        }

        /// <summary>
        /// Method is responsible for mapping the data row into UIElements suitable for displaying in the popup window.
        /// </summary>
        /// <param name="datavalue">The value of the object to set in the UIElement.</param>
        /// <param name="dataColumn">The data column containing information about the current item to be stored.</param>
        /// <param name="LockedTable">True or False to indicate the data is locked from edit - standard messages - but is shown so the user can see context.</param>
        /// <returns>The UIElement suitable for the datatype, CheckBox for bool, ComboBox for list (e.g. enum types), TextBox for strings, dates, and ints.</returns>
        private UIElement Convert(object datavalue, string dataColumnName, string TableName, Type ColumnType, bool LockedTable = false)
        {
            const int ValueWidth = 325;
            UIElement dataout = null;

            PopupEditTableDataType dataType = CheckColumn(dataColumnName, ColumnType);

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
                    EditPopupTime currColumn = new() { Time = (DateTime)datavalue };
                    DateList.Add(currColumn);

                    Binding DateBind = new("Time")
                    {
                        Source = currColumn,
                        Mode = BindingMode.TwoWay
                    };

                    dataout = new TextBox()
                    {
                        // TODO: add 'edit row' popup window, a datetime validator for textbox
                        Text = datavalue.ToString(),
                        Width = ValueWidth
                    };

                    ((TextBox)dataout).SetBinding(TextBox.TextProperty, DateBind);

                    break;
                case PopupEditTableDataType.comboenum:
                    List<string> enumlist = new();

                    Dictionary<string, Array> ColEnums =
                        new()
                        {
                            { "Permission", Enum.GetValues(typeof(ViewerTypes)) },
                            { "ViewerTypes", Enum.GetValues(typeof(ViewerTypes)) },
                            { "Kind", Enum.GetValues(typeof(WebhooksKind)) },
                            { "action", Enum.GetValues(typeof(CommandAction)) },
                            { "sort", Enum.GetValues(typeof(CommandSort)) },
                            { "ModAction", Enum.GetValues(typeof(ModActions)) },
                            { "MsgType", Enum.GetValues(typeof(MsgTypes)) },
                            { "BanReason", Enum.GetValues(typeof(BanReasons)) },
                            { "OverlayType", Enum.GetValues(typeof(OverlayTypes)) },
                            { "ModActionType", Enum.GetValues(typeof(ModActionType)) },
                            { "ModPerformType", Enum.GetValues(typeof(ModPerformType)) },
                            { "TickerName", Enum.GetValues(typeof(OverlayTickerItem)) },
                            {"Platform", Enum.GetValues(typeof(Platform)) }
                        };

                    foreach (var E in ColEnums[dataColumnName])
                    {
                        enumlist.Add(E.ToString());
                    }

                    dataout = new ComboBox()
                    {
                        ItemsSource = enumlist,
                        SelectedValue = datavalue,
                        VerticalAlignment = VerticalAlignment.Center,
                        Width = ValueWidth
                    };

                    if (dataColumnName == "OverlayType")
                    {
                        OverlayTypeElement = (ComboBox)dataout;
                        ((ComboBox)dataout).Name = nameof(OverlayTypeElement);
                        ((ComboBox)dataout).SelectionChanged += OverlayTypeElement_SelectionChanged;
                    }
                    else if (dataColumnName == "ModActionType")
                    {
                        ModActionTypeElement = (ComboBox)dataout;
                        ((ComboBox)dataout).Name = nameof(ModActionTypeElement);
                        ((ComboBox)dataout).SelectionChanged += OverlayTypeElement_SelectionChanged;
                    }
                    else if (dataColumnName == "ModPerformType")
                    {
                        ModPerformTypeElement = (ComboBox)dataout;
                        ((ComboBox)dataout).Name = nameof(ModPerformTypeElement);
                        ((ComboBox)dataout).SelectionChanged += OverlayTypeElement_SelectionChanged;
                    }
                    break;
                case PopupEditTableDataType.combolist:
                    List<string> combolistcollection = [];
                    ComboBox CurrTypeElem = null;

                    dataout = new ComboBox()
                    {
                        SelectedValue = datavalue.ToString(),
                        Width = ValueWidth,
                        VerticalAlignment = VerticalAlignment.Center
                    };

                    if (dataColumnName == "ModActionName")
                    {
                        CurrTypeElem = ModActionTypeElement;
                        ModActionElement = (ComboBox)dataout;
                    }
                    else if (dataColumnName == "ModPerformAction")
                    {
                        CurrTypeElem = ModPerformTypeElement;
                        ModPerformElement = (ComboBox)dataout;
                    }

                    if (CurrTypeElem != null && CurrTypeElem.SelectedValue != null)
                    {
                        ((ComboBox)dataout).ItemsSource = MediaOverlayEventActions[CurrTypeElem.SelectedValue.ToString()];
                    }

                    break;
                case PopupEditTableDataType.combotable:
                    List<string> combocollection = [];
                    if (TableName == "CategoryList")
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
                        if (dataColumnName == "Category")
                        {
                            List<CheckBox> CBcombocollection = new();
                            List<string> selection = [.. ((string)datavalue).Split(", ")];
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
                            dataout = new ListBox()
                            {
                                Width = ValueWidth,
                                VerticalAlignment = VerticalAlignment.Center,
                                ItemsSource = CBcombocollection,
                                MaxHeight = 200
                            };
                            CategoryListView = (ListBox)dataout;
                        }
                        else if (dataColumnName == "table")
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
                        else if (dataColumnName == "key_field")
                        {
                            ComboBox GUItable = TableElement;

                            if (GUItable != null && GUItable.SelectedValue != null)
                            {
                                combocollection.AddRange(DataManage.GetKeys((string)GUItable.SelectedValue));
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
                        else if (dataColumnName == "data_field")
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
                        else if (dataColumnName == "currency_field")
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
                case PopupEditTableDataType.combooverlayaction:
                    dataout = new ComboBox()
                    {
                        SelectedValue = datavalue.ToString(),
                        Width = ValueWidth,
                        VerticalAlignment = VerticalAlignment.Center
                    };
                    if (dataColumnName == "OverlayAction")
                    {
                        OverlayActionTypeElement = (ComboBox)dataout;
                    }

                    if (OverlayTypeElement != null && OverlayTypeElement.SelectedValue != null)
                    {
                        ((ComboBox)dataout).ItemsSource = MediaOverlayEventActions[OverlayTypeElement.SelectedValue.ToString()];
                    }

                    break;
                case PopupEditTableDataType.filebrowse:
                    dataout = new TextBox()
                    {
                        Text = datavalue.ToString(),
                        ToolTip = "Paste full path to file, double-click to browse to file.",
                        MinWidth = 200
                    };
                    ((TextBox)dataout).MouseDoubleClick += FileBrowser_TextBox_MouseDoubleClick;

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

                    //if (!dataColumn.ReadOnly)
                    //{
                    ((TextBox)dataout).TextChanged += EditData_TextChanged_TextBoxLen;
                    ((TextBox)dataout).Loaded += EditData_Loaded_TextBoxLen;
                    AddedStringEvent = true;
                    //}
                    break;
            }

            //if (dataout != null)
            //{
            //    dataout.IsEnabled = !dataColumn.ReadOnly;
            //}

            if (LockedTable && (dataColumnName is
                "Id" or "CmdName" or "AllowParam"
                or "Usage" or "lookupdata" or "table"
                or "key_field" or "data_field"
                or "unit" or "action"
                or "top" or "Name"))
            {
                dataout.IsEnabled = false;
            }
            else if (dataout.GetType() == typeof(TextBox))
            {
                dataout.PreviewMouseLeftButtonDown += PreviewMouseLeftButton_SelectAll;
            }

            return dataout;
        }

#else

        /// <summary>
        /// Supply the data to populate into the Popup Window.
        /// </summary>
        /// <param name="dataTable">The DataTable used in the current context.</param>
        /// <param name="dataRow">An existing row of data. If null, specifies the user is adding a new data row.</param>
        public void LoadData(DataTable dataTable, DataRow dataRow = null)
        {
            Title = dataRow == null ? $"Add new {dataTable.TableName} Row" : $"Edit {dataTable.TableName} Row";

            const int NameWidth = 150;
            bool CheckLockTable = false;
            IsNewRow = dataRow == null;
            SaveDataRow = dataRow ?? dataTable.NewRow();

            if (SaveDataRow.Table.TableName is "Commands" or "ChannelEvents")
            {
                List<string> builtInCmds = new();

                foreach (DefaultCommand d in Enum.GetValues(typeof(DefaultCommand)))
                {
                    builtInCmds.Add(d.ToString());
                }

                if (builtInCmds.Contains(SaveDataRow.Table.Columns.Contains("CmdName") ? SaveDataRow["CmdName"].ToString() : "") || SaveDataRow.Table.TableName is "ChannelEvents")
                {
                    CheckLockTable = true;
                }
            }

            foreach (DataColumn dataColumn in dataTable.Columns)
            {
                AddedStringEvent = false;

                UIElement dataname = new StackPanel() { Orientation = Orientation.Vertical };
                ((StackPanel)dataname).Children.Add(new Label() { Content = dataColumn.ColumnName, Width = NameWidth });

                object datavalue = DBNull.Value.Equals(SaveDataRow[dataColumn]) ? "" : SaveDataRow[dataColumn];

                UIElement dataout = Convert(datavalue, dataColumn, CheckLockTable);

                // Check after adding "dataout" via Convert() whether we added the string counting event - add labels to hold the string count value
                if (AddedStringEvent)
                {
                    StackPanel CountRows = new() { Orientation = Orientation.Horizontal };
                    CountRows.Children.Add(new Label() { Content = "Current Chars: " });
                    CountRows.Children.Add(new Label());

                    ((StackPanel)dataname).Children.Add(CountRows);
                }

                UIElement datatable = new Label() { Content = dataTable.TableName, Visibility = Visibility.Collapsed };

                StackPanel row = new() { Orientation = Orientation.Horizontal };
                row.Children.Add(dataname);
                row.Children.Add(dataout);
                row.Children.Add(datatable);

                ListBox_DataList.Items.Add(row);
            }
        }

        /// <summary>
        /// Handles when the user clicks the Apply button, and saves the resulting data.
        /// Retrieves the data from the popup window and converts it back to the DataTable.
        /// 
        /// Added this functionality due to unreliable data edits in 'DataGrid' objects.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void ApplyButton_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                Dictionary<string, string> SaveData = new();

                foreach (StackPanel data in ListBox_DataList.Items)
                {
                    string name = ((Label)((StackPanel)data.Children[0]).Children[0]).Content.ToString();
                    string result = ConvertBack(data.Children[1]);

                    SaveData.Add(name, result);
                }

                lock (SaveDataRow)
                {
                    if (SaveDataRow.Table.TableName == "OverlayServices")
                    {
                        try
                        {
                            SaveData["MediaFile"] = FileCopy(SaveData["MediaFile"], SaveData["OverlayType"]) ?? (string)SaveDataRow.Table.Columns["MediaFile"].DefaultValue;
                            SaveData["ImageFile"] = FileCopy(SaveData["ImageFile"], SaveData["OverlayType"]) ?? (string)SaveDataRow.Table.Columns["ImageFile"].DefaultValue;
                        }
                        catch (Exception ex)
                        {
                            LogWriter.LogException(ex, MethodBase.GetCurrentMethod().Name);
                        }
                    }

                    foreach (DataColumn dataColumn in SaveDataRow.Table.Columns)
                    {
                        if (!dataColumn.ReadOnly)
                        {
                            SaveDataRow[dataColumn] = SaveData[dataColumn.ColumnName];
                        }
                    }
                    if (IsNewRow) // if this row is new, we have to add it to the DataTable, and not just modify it.
                    {
                        SaveDataRow.Table.Rows.Add(SaveDataRow);
                    }
                }

                UpdatedDataRow?.Invoke(this, new() { RowChanged = true });
            }
            catch (Exception ex)
            {
                LogWriter.LogException(ex, MethodBase.GetCurrentMethod().Name);
            }
            Close();
        }


        /// <summary>
        /// Method is responsible for mapping the data row into UIElements suitable for displaying in the popup window.
        /// </summary>
        /// <param name="datavalue">The value of the object to set in the UIElement.</param>
        /// <param name="dataColumn">The data column containing information about the current item to be stored.</param>
        /// <param name="LockedTable">True or False to indicate the data is locked from edit - standard messages - but is shown so the user can see context.</param>
        /// <returns>The UIElement suitable for the datatype, CheckBox for bool, ComboBox for list (e.g. enum types), TextBox for strings, dates, and ints.</returns>
        private UIElement Convert(object datavalue, DataColumn dataColumn, bool LockedTable = false)
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
                    EditPopupTime currColumn = new() { Time = (DateTime)datavalue };
                    DateList.Add(currColumn);

                    Binding DateBind = new("Time")
                    {
                        Source = currColumn,
                        Mode = BindingMode.TwoWay
                    };

                    dataout = new TextBox()
                    {
                        // TODO: add 'edit row' popup window, a datetime validator for textbox
                        Text = datavalue.ToString(),
                        Width = ValueWidth
                    };

                    ((TextBox)dataout).SetBinding(TextBox.TextProperty, DateBind);

                    break;
                case PopupEditTableDataType.comboenum:
                    List<string> enumlist = new();

                    Dictionary<string, Array> ColEnums =
                        new()
                        {
                            { "Permission", Enum.GetValues(typeof(ViewerTypes)) },
                            { "ViewerTypes", Enum.GetValues(typeof(ViewerTypes)) },
                            { "Kind", Enum.GetValues(typeof(WebhooksKind)) },
                            { "action", Enum.GetValues(typeof(CommandAction)) },
                            { "sort", Enum.GetValues(typeof(CommandSort)) },
                            { "ModAction", Enum.GetValues(typeof(ModActions)) },
                            { "MsgType", Enum.GetValues(typeof(MsgTypes)) },
                            { "BanReason", Enum.GetValues(typeof(BanReasons)) },
                            { "OverlayType", Enum.GetValues(typeof(OverlayTypes)) },
                            { "ModActionType", Enum.GetValues(typeof(ModActionType)) },
                            { "ModPerformType", Enum.GetValues(typeof(ModPerformType)) },
                            { "TickerName", Enum.GetValues(typeof(OverlayTickerItem)) },
                            {"Platform", Enum.GetValues(typeof(Platform)) }
                        };

                    foreach (var E in ColEnums[dataColumn.ColumnName])
                    {
                        enumlist.Add(E.ToString());
                    }

                    dataout = new ComboBox()
                    {
                        ItemsSource = enumlist,
                        SelectedValue = datavalue,
                        VerticalAlignment = VerticalAlignment.Center,
                        Width = ValueWidth
                    };

                    if (dataColumn.ColumnName == "OverlayType")
                    {
                        OverlayTypeElement = (ComboBox)dataout;
                        ((ComboBox)dataout).Name = nameof(OverlayTypeElement);
                        ((ComboBox)dataout).SelectionChanged += OverlayTypeElement_SelectionChanged;
                    }
                    else if (dataColumn.ColumnName == "ModActionType")
                    {
                        ModActionTypeElement = (ComboBox)dataout;
                        ((ComboBox)dataout).Name = nameof(ModActionTypeElement);
                        ((ComboBox)dataout).SelectionChanged += OverlayTypeElement_SelectionChanged;
                    }
                    else if (dataColumn.ColumnName == "ModPerformType")
                    {
                        ModPerformTypeElement = (ComboBox)dataout;
                        ((ComboBox)dataout).Name = nameof(ModPerformTypeElement);
                        ((ComboBox)dataout).SelectionChanged += OverlayTypeElement_SelectionChanged;
                    }
                    break;
                case PopupEditTableDataType.combolist:
                    List<string> combolistcollection = new();
                    ComboBox CurrTypeElem = null;


                    dataout = new ComboBox()
                    {
                        SelectedValue = datavalue.ToString(),
                        Width = ValueWidth,
                        VerticalAlignment = VerticalAlignment.Center
                    };

                    if (dataColumn.ColumnName == "ModActionName")
                    {
                        CurrTypeElem = ModActionTypeElement;
                        ModActionElement = (ComboBox)dataout;
                    }
                    else if (dataColumn.ColumnName == "ModPerformAction")
                    {
                        CurrTypeElem = ModPerformTypeElement;
                        ModPerformElement = (ComboBox)dataout;
                    }

                    if (CurrTypeElem != null && CurrTypeElem.SelectedValue != null)
                    {
                        ((ComboBox)dataout).ItemsSource = MediaOverlayEventActions[CurrTypeElem.SelectedValue.ToString()];
                    }

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
                            List<string> selection = [.. ((string)datavalue).Split(", ")];
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
                            dataout = new ListBox()
                            {
                                Width = ValueWidth,
                                VerticalAlignment = VerticalAlignment.Center,
                                ItemsSource = CBcombocollection,
                                MaxHeight = 200
                            };
                            CategoryListView = (ListBox)dataout;
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

                            if (GUItable != null && GUItable.SelectedValue != null)
                            {
                                combocollection.AddRange(DataManage.GetKeys((string)GUItable.SelectedValue));
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
                case PopupEditTableDataType.combooverlayaction:
                    dataout = new ComboBox()
                    {
                        SelectedValue = datavalue.ToString(),
                        Width = ValueWidth,
                        VerticalAlignment = VerticalAlignment.Center
                    };
                    if (dataColumn.ColumnName == "OverlayAction")
                    {
                        OverlayActionTypeElement = (ComboBox)dataout;
                    }

                    if (OverlayTypeElement != null && OverlayTypeElement.SelectedValue != null)
                    {
                        ((ComboBox)dataout).ItemsSource = MediaOverlayEventActions[OverlayTypeElement.SelectedValue.ToString()];
                    }

                    break;
                case PopupEditTableDataType.filebrowse:
                    dataout = new TextBox()
                    {
                        Text = datavalue.ToString(),
                        ToolTip = "Paste full path to file, double-click to browse to file.",
                        MinWidth = 200
                    };
                    ((TextBox)dataout).MouseDoubleClick += FileBrowser_TextBox_MouseDoubleClick;

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

                    if (!dataColumn.ReadOnly)
                    {
                        ((TextBox)dataout).TextChanged += EditData_TextChanged_TextBoxLen;
                        ((TextBox)dataout).Loaded += EditData_Loaded_TextBoxLen;
                        AddedStringEvent = true;
                    }
                    break;
            }

            if (dataout != null)
            {
                dataout.IsEnabled = !dataColumn.ReadOnly;
            }

            if (LockedTable && (dataColumn.ColumnName is
                "Id" or "CmdName" or "AllowParam"
                or "Usage" or "lookupdata" or "table"
                or "key_field" or "data_field"
                or "unit" or "action"
                or "top" or "Name"))
            {
                dataout.IsEnabled = false;
            }
            else if (dataout.GetType() == typeof(TextBox))
            {
                dataout.PreviewMouseLeftButtonDown += PreviewMouseLeftButton_SelectAll;
            }

            return dataout;
        }

#endif

        /// <summary>
        /// A helper method for ComboBoxes related to Commands. Some CombobBoxes need updating when the user selects a certain table - such as updating the table key fields and the data field list.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void TableData_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            KeyFieldElement.ItemsSource = new List<string>() { DataManage.GetKey((string)TableElement.SelectedValue) };
            UpdatePropertyChanged(nameof(KeyFieldElement));

            DataFieldElement.ItemsSource = DataManage.GetTableFields((string)TableElement.SelectedValue);
            UpdatePropertyChanged(nameof(DataFieldElement));
        }

        private void ApplyButton_Click(object sender, RoutedEventArgs e)
        {

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
            return resultfile;
        }

        /// <summary>
        /// Event to handle if the user cancels the addition. 
        /// Clears the saved row so it doesn't interfere with other data edits.
        /// Closes the current window.
        /// </summary>
        /// <param name="sender">Object sending the event.</param>
        /// <param name="e">Results of the action sending the event.</param>
        private void CancelButton_Click(object sender, RoutedEventArgs e)
        {
            if (!IsClosing)
            {
                Close();
            }
        }

        /// <summary>
        /// Handles event when user closes the window.
        /// Effectively, closing the window is clicking the cancel button.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void Window_Closing(object sender, CancelEventArgs e)
        {
            IsClosing = true;
            CancelButton_Click(this, new());
        }

        private void EditData_Loaded_TextBoxLen(object sender, RoutedEventArgs e)
        {
            CalculateTextBoxTextLength(sender);
        }

        private static void CalculateTextBoxTextLength(object sender)
        {
            StackPanel CurrRow = (StackPanel)VisualTreeHelper.GetParent((DependencyObject)sender);

            /*
             UIElement dataname
                    <StackPanel Orientation="Vertical">
                        <Label Content="Data Name Value" />
                        <StackPanel Orientation="Horizontal">
                            <Label Content="Current Chars: " />
                            <Label />
                        </StackPanel>
                    </StackPanel> 
             */

            Label CountLable = (Label)((StackPanel)((StackPanel)CurrRow.Children[0]).Children[1]).Children[1];
            CountLable.Content = ((TextBox)sender).Text.Length;
        }

        private void EditData_TextChanged_TextBoxLen(object sender, TextChangedEventArgs e)
        {
            CalculateTextBoxTextLength(sender);
        }

        private void OverlayTypeElement_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            ComboBox ActionType = (ComboBox)sender;
            ComboBox ActionNameElement = null;

            switch (ActionType.Name)
            {
                case nameof(OverlayTypeElement):
                    ActionNameElement = OverlayActionTypeElement;
                    break;
                case nameof(ModActionTypeElement):
                    ActionNameElement = ModActionElement;
                    break;
                case nameof(ModPerformTypeElement):
                    ActionNameElement = ModPerformElement;
                    break;
            }

            if (ActionType != null && ActionType.SelectedValue.ToString() != "" && ActionNameElement != null)
            {
                ActionNameElement.ItemsSource = MediaOverlayEventActions[ActionType.SelectedValue.ToString()];
            }
        }


        public void SetOverlayActions(Dictionary<string, List<string>> keyValuePairs)
        {
            MediaOverlayEventActions.Clear();

            foreach (var K in keyValuePairs)
            {
                MediaOverlayEventActions.Add(K.Key, K.Value);
            }
        }

        private void FileBrowser_TextBox_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            TextBox saveMediaPath = sender as TextBox;

            OpenFileDialog pickFile = new()
            {
                Multiselect = false,
                CheckFileExists = true,
                DereferenceLinks = true,
                Title = "Select media file (picture/video) for Overlay event! (needs to be viewable in a webpage)",
                InitialDirectory = OptionFlags.MediaOverlayMRUPathSelect
            };


            if (pickFile.ShowDialog() == true)
            {
                saveMediaPath.Text = pickFile.FileName;
                OptionFlags.MediaOverlayMRUPathSelect = Path.GetDirectoryName(saveMediaPath.Text);
            }
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
                ListBox CatList = CategoryListView;

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

        /// <summary>
        /// Helper method to map column names to datatypes. The default is text.
        /// </summary>
        /// <param name="ColumnName">The name of the column to be verified.</param>
        /// <returns>A datatype in line with UIElement types.</returns>
        private PopupEditTableDataType CheckColumn(string ColumnName, Type ColumnType)
        {
            switch (ColumnName)
            {
                case "ModActionName" or "ModPerformAction":
                    return PopupEditTableDataType.combolist;
                case "Category" or "table" or "key_field" or "data_field" or "currency_field":
                    return PopupEditTableDataType.combotable;
                case "OverlayAction":
                    return PopupEditTableDataType.combooverlayaction;
            }

            if (ColumnType.ToString().Contains("Enums"))
            {
                return PopupEditTableDataType.comboenum;
            }
            else if (ColumnType == typeof(DateTime))
            {
                return PopupEditTableDataType.datestring;
            }
            else if (ColumnType == typeof(bool))
            {
                return PopupEditTableDataType.databool;
            }
            else if (ColumnName is "MediaFile" or "ImageFile")
            {
                return PopupEditTableDataType.filebrowse;
            }
            else
            {
                return PopupEditTableDataType.text;
            }
        }

        /// <summary>
        /// Reads from the UIElement and returns the data value.
        /// </summary>
        /// <param name="dataElement">The UIElement to retrieve the data value.</param>
        /// <returns>The data value from the element, as a string.</returns>
        private static string ConvertBack(UIElement dataElement)
        {
            string result;
            if (dataElement.GetType() == typeof(CheckBox))
            {
                result = ((CheckBox)dataElement).IsChecked.Value.ToString();
            }
            else if (dataElement.GetType() == typeof(DatePicker))
            {
                result = ((DatePicker)dataElement).Text;
            }
            else if (dataElement.GetType() == typeof(ComboBox))
            {
                result = (string)((ComboBox)dataElement).SelectedValue;
            }
            else if (dataElement.GetType() == typeof(ListBox))
            {
                List<string> selections = new();
                foreach (CheckBox c in ((ListBox)dataElement).ItemsSource)
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

        /// <summary>
        /// Event to allow the user to click a TextBox and the cursor highlights all text.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private async void PreviewMouseLeftButton_SelectAll(object sender, MouseButtonEventArgs e)
        {
            await Application.Current.Dispatcher.InvokeAsync((sender as TextBox).SelectAll);
        }
    }
}
