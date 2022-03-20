using StreamerBotLib.Events;
using StreamerBotLib.Models;

using System;
using System.Collections.Generic;
using System.Data;
using System.Windows;
using System.Windows.Controls;

namespace StreamerBotLib.GUI.Windows
{
    /// <summary>
    /// Shows new Window for Adding or Editing DataGrid data rows
    /// </summary>
    public partial class EditData : Window
    {
        private bool IsClosing = false;

        private DataRow SaveDataRow { get; set; }

        public event EventHandler<UpdatedDataRowArgs> UpdatedDataRow;

        public EditData()
        {
            InitializeComponent();
        }

        public void UpdateDictionary(DataTable dataTable, DataRow dataRow = null) //Dictionary<string, string> keyValuePairs, List<string> ReadOnlyKeys)
        {
            Title = dataRow == null ? $"Add new {dataTable.TableName} Row" : $"Edit {dataTable.TableName} Row";

            SaveDataRow = dataRow ?? dataTable.NewRow();

            foreach (DataColumn dataColumn in dataTable.Columns)
            {
                UIElement dataname = new Label() { Content = dataColumn.ColumnName, Width = 150 };
                object datavalue = DBNull.Value == SaveDataRow[dataColumn] ? "" : SaveDataRow[dataColumn];
                UIElement dataout;
                Type datatype = dataColumn.DataType;

        //TODO: Massively expand this convert to and from DataRow
                if (datatype == typeof(bool))
                {
                    dataout = new CheckBox() { IsChecked = (bool?)datavalue, VerticalAlignment = VerticalAlignment.Center, Width = 250 };
                }
                else if (datatype == typeof(DateTime))
                {
                    dataout = new DatePicker() { Text = ((DateTime) datavalue).ToString("yyyy'-'MM'-'dd'T'HH':'mm':'ss"), VerticalAlignment = VerticalAlignment.Center, Width = 250 };
                }
                else //if (datatype == typeof(string) || datatype == typeof(int))
                {
                    dataout = new TextBox() { Text = datavalue.ToString(), VerticalAlignment = VerticalAlignment.Center, Width = 250, TextWrapping = TextWrapping.Wrap };
                }

                dataout.IsEnabled = !dataColumn.ReadOnly;

                StackPanel row = new() { Orientation = Orientation.Horizontal };
                row.Children.Add(dataname);
                row.Children.Add(dataout);

                ListBox_DataList.Items.Add(row);
            }
        }

        private void ApplyButton_Click(object sender, RoutedEventArgs e)
        {
            Dictionary<string, string> SaveData = new();

            foreach (StackPanel data in ListBox_DataList.Items)
            {
                string name = ((Label)data.Children[0]).Content.ToString();
                string result = "";

                if (data.Children[1].GetType() == typeof(CheckBox))
                {
                    result = ((CheckBox)data.Children[1]).IsChecked.Value.ToString();
                }
                else if (data.Children[1].GetType() == typeof(DatePicker))
                {
                    result = ((DatePicker)data.Children[1]).Text;
                }
                else
                {
                    result = ((TextBox)data.Children[1]).Text;
                }

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

        private void Window_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            IsClosing = true;
            CancelButton_Click(this, new());
        }
    }
}
