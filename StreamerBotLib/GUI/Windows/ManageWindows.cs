using StreamerBotLib.Systems;

using System.Data;

namespace StreamerBotLib.GUI.Windows
{
    public class ManageWindows
    {
        private EditData EditDataWindow { get; set; }

        public ManageWindows()
        {
        }

        public void DataGridAddNewItem(DataTable dataTable)
        {
            DataGridOpenRowWindow(dataTable);
        }

        public void DataGridEditItem(DataTable dataTable, DataRow dataRow)
        {
            DataGridOpenRowWindow(dataTable, dataRow);
        }

        private void DataGridOpenRowWindow(DataTable dataTable, DataRow dataRow = null)
        {
            EditDataWindow = new();
            EditDataWindow.UpdatedDataRow += EditDataWindow_UpdatedDataRow;
            EditDataWindow.LoadData(dataTable, dataRow);
            EditDataWindow.Show();
        }

        private void EditDataWindow_UpdatedDataRow(object sender, Events.UpdatedDataRowArgs e)
        {
            SystemsController.PostUpdatedDataRow(e.UpdatedDataRow);
        }
    }
}
