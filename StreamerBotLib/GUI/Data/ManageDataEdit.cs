using StreamerBotLib.DataSQL.TableMeta;
using StreamerBotLib.Models.Events;
using StreamerBotLib.Static;
using StreamerBotLib.Systems;

namespace StreamerBotLib.GUI.Data
{
    public class ManageDataEdit
    {
        private DataBot dataBot { get; }
        //private ManageDataWindow DataEditWindow { get; set; }
        private TableMeta CurrTableRow { get; set; }
        private Dictionary<string, List<string>> _TableDataPairs = [];

        private bool IsNewRow;

        internal event EventHandler<AddNewRowEventArgs> DataAddNewRowEvent;
        internal event EventHandler<UpdatedDataRowArgs> DataEditRowEvent;

        private bool _setTableData, _openNewWindow;

        public ManageDataEdit(DataBot dataBot)
        {
            this.dataBot = dataBot;

            DataAddNewRowEvent += this.dataBot.DataGridUpdatedRow;
            DataEditRowEvent += (sender, e) =>
            {
                this.dataBot.GUISaveDataGridEdits((e.UpdatedData.TableName is "DG_BuiltInCommands" or "DG_BuiltInResponses"), e.UpdatedData.TableName);
            };
        }

        public void SetTableData(Dictionary<string, List<string>> SourceData)
        {
            _TableDataPairs.Clear();
            foreach (var D in SourceData)
            {
                _TableDataPairs.Add(D.Key, D.Value);
            }
            _setTableData = true;

            if (_openNewWindow) // If the window was already requested to open, open it now that the data is set
            {
                _openNewWindow = false;
                OpenGridWindow();
            }
        }

        public void EditItem(TableMeta tableMeta, bool isNewRow)
        {
            CurrTableRow = tableMeta;
            IsNewRow = isNewRow;

            OpenGridWindow();
        }

        private void OpenGridWindow()
        {
            // two entry points, if the table data is set, skip this, otherwise, cancel but wait for the table data and the other method starts this method
            if (!_setTableData)
            {
                _openNewWindow = true;
                return;
            }

            ThreadManager.AddTaskToGUIDispatcher(() =>
            {
                ManageDataWindow DataEditWindow = new()
                {
                    Title = string.Format(
                            (IsNewRow
                            ? LocalizedMsgSystem.GetVar("MsgPopupNewRow")
                            : LocalizedMsgSystem.GetVar("MsgPopupEditRow")), CurrTableRow.CurrEntity.TableName),
                    SetTableMeta = CurrTableRow,
                    SetDataBot = dataBot,
                    TableDataPairs = _TableDataPairs
                };

                DataEditWindow.SaveRecordEvent +=
                    // save a new record - add to the database, an existing record is already tracked in EF Core so it just needs EFCore to save changes
                    IsNewRow ?
                      (sender, e) => DataAddNewRowEvent?.Invoke(sender, new AddNewRowEventArgs(CurrTableRow.CurrEntity))
                    : (sender, e) => DataEditRowEvent?.Invoke(sender, new UpdatedDataRowArgs(CurrTableRow.CurrEntity));


                DataEditWindow.CancelRecordEvent +=
                    // discard a canceled new row, but save any existing record because the GUI needs to sync any adjustments the user made.
                    IsNewRow ?
                      (sender, e) => { return; }
                : (sender, e) => DataEditRowEvent?.Invoke(sender, new UpdatedDataRowArgs(CurrTableRow.CurrEntity));

                DataEditWindow.TableDataPairs = _TableDataPairs;
                DataEditWindow.ShowDialog();
            });
        }
    }
}
