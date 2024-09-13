using StreamerBotLib.DataSQL.TableMeta;
using StreamerBotLib.Systems;

using System.Windows.Controls;

namespace StreamerBotLib.GUI.Windows
{
    public class ManageWindows
    {
        private EditData EditDataWindow { get; set; }

        private Dictionary<string, List<string>> TableDataPairs { get; } = [];

        private TableMeta CurrTableRow { get; set; }

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
