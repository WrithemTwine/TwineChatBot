using StreamerBotLib.DataSQL.TableMeta;
using StreamerBotLib.Models.Events;
using StreamerBotLib.Systems;

namespace StreamerBotLib.GUI.Windows
{
    public class ManageWindows
    {
        private EditData EditDataWindow { get; set; }

        private Dictionary<string, List<string>> TableDataPairs { get; } = [];

        private TableMeta CurrTableRow { get; set; }

        internal static EventHandler<AddNewRowEventArgs> DataGridUpdatedRowHandler { get; set; }

        public ManageWindows() { }

        public void AddNewItem(TableMeta tableMeta)
        {
            CurrTableRow = tableMeta;
            OpenDataGridRowWindow(true);
        }

        public void EditExistingItem(TableMeta tableMeta)
        {
            CurrTableRow = tableMeta;
            OpenDataGridRowWindow(false);
        }

        private void OpenDataGridRowWindow(bool NewRow)
        {
            EditDataWindow = new(ActionSystem.DataManage);
            EditDataWindow.AddNewRow += DataGridUpdatedRowHandler; // hookup adding a new row to a table

            if (CurrTableRow.CurrEntity.TableName is "OverlayServices" or "ModeratorApprove")
            {
                EditDataWindow.SetOverlayActions(TableDataPairs);
            }

            EditDataWindow.LoadData(CurrTableRow.CurrEntity, NewRow);
            EditDataWindow.Show();
        }

        public void SetTableData(Dictionary<string, List<string>> SourceData)
        {
            TableDataPairs.Clear();
            foreach (var D in SourceData)
            {
                TableDataPairs.Add(D.Key, D.Value);
            }
        }
    }
}
