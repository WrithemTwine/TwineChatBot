using StreamerBotLib.Interfaces;
using StreamerBotLib.Systems;

using System.Data;

namespace StreamerBotLib.GUI.Windows
{
    public class ManageWindows
    {
        private EditData EditDataWindow { get; set; }

        private Dictionary<string, List<string>> TableDataPairs = new();

        public ManageWindows()
        {
        }

        public void DataGridAddNewItem(IDataManagerReadOnly dataManageReadOnly, DataTable dataTable)
        {
            DataGridOpenRowWindow(dataManageReadOnly, dataTable);
        }

        public void DataGridEditItem(IDataManagerReadOnly dataManageReadOnly, DataTable dataTable, DataRow dataRow)
        {
            DataGridOpenRowWindow(dataManageReadOnly, dataTable, dataRow);
        }

        public void SetTableData(Dictionary<string, List<string>> SourceData)
        {
            TableDataPairs.Clear();
            foreach (var D in SourceData)
            {
                TableDataPairs.Add(D.Key, D.Value);
            }
        }

        private void DataGridOpenRowWindow(IDataManagerReadOnly dataManageReadOnly, DataTable dataTable, DataRow dataRow = null)
        {
            EditDataWindow = new(dataManageReadOnly);
            EditDataWindow.UpdatedDataRow += EditDataWindow_UpdatedDataRow;

            if (dataTable.TableName is "OverlayServices" or "ModeratorApprove")
            {
                EditDataWindow.SetOverlayActions(TableDataPairs);
            }

            EditDataWindow.LoadData(dataTable, dataRow);
            EditDataWindow.Show();
        }

        private void EditDataWindow_UpdatedDataRow(object sender, Events.UpdatedDataRowArgs e)
        {
            SystemsController.PostUpdatedDataRow(e.RowChanged);
        }
    }
}
