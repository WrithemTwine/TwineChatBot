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

        private ItemCollection Curr { get; set; }

        public ManageWindows() { }


        public void AddNewItem(TableMeta tableMeta, ItemCollection CurrItem)
        {
            Curr = CurrItem;
            CurrTableRow = tableMeta;
            OpenDataGridRowWindow();
        }

        public void EditExistingItem(TableMeta tableMeta)
        {
            Curr = null;
            CurrTableRow = tableMeta;
            OpenDataGridRowWindow();
        }

        private void OpenDataGridRowWindow()
        {
            EditDataWindow = new(ActionSystem.DataManage);
            EditDataWindow.AddNewRow += EditDataWindow_AddNewRow;
            EditDataWindow.UpdatedRow += EditDataWindow_UpdatedRow;

            if (CurrTableRow.CurrEntity.TableName is "OverlayServices" or "ModeratorApprove")
            {
                EditDataWindow.SetOverlayActions(TableDataPairs);
            }

            EditDataWindow.LoadData(CurrTableRow.CurrEntity, Curr != null);
            EditDataWindow.Show();
        }

        private void EditDataWindow_AddNewRow(object sender, Events.AddNewRowEventArgs e)
        {
            Curr.Add(e.NewRow.GetModelEntity());

        }

        private void EditDataWindow_UpdatedRow(object sender, Events.UpdatedDataRowArgs e)
        {
            if (e.RowChanged)
            {
                CurrTableRow.GetUpdatedEntity(e.UpdatedData);
            }
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
