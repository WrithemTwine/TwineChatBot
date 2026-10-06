using StreamerBotLib.DataSQL.TableMeta;
using StreamerBotLib.Models.Events;
using StreamerBotLib.Static;
using StreamerBotLib.Systems;
using StreamerBotLib.Systems.Overlay.Enums;

using System.Windows.Controls;

namespace StreamerBotLib.GUI.Data
{
    public class ManageDataEdit
    {
        private DataBot dataBot { get; }
        //private ManageDataWindow DataEditWindow { get; set; }
        private TableMeta CurrTableRow { get; set; }
        private bool IsNewRow;

        internal event EventHandler<AddNewRowEventArgs> DataAddNewRowEvent;
        internal event EventHandler<UpdatedDataRowArgs> DataEditRowEvent;

        public ManageDataEdit(DataBot dataBot)
        {
            this.dataBot = dataBot;

            DataAddNewRowEvent += this.dataBot.DataGridUpdatedRow;
            DataEditRowEvent += (sender, e) =>
            {
                e.UpdatedData.GetEditedEntity();
                this.dataBot.GUISaveDataGridEdits((e.UpdatedData.CurrEntity.TableName is "DG_BuiltInCommands" or "DG_BuiltInResponses"), e.UpdatedData.CurrEntity.TableName);
                if (e.UpdatedData.CurrEntity.TableName == "OverlayServices")
                {
                    this.dataBot.SyncOverlayActionSelections((OverlayTypes)e.UpdatedData.CurrEntity.Values["OverlayType"], (string)e.UpdatedData.CurrEntity.Values["OverlayAction"]);
                }
            };
        }

        public void EditItem(TableMeta tableMeta, bool isNewRow, DataGrid EditSource = null)
        {
            CurrTableRow = tableMeta;
            IsNewRow = isNewRow;

            OpenGridWindow();
        }

        private void OpenGridWindow()
        {
            ThreadManager.AddTaskToGUIDispatcher(() =>
            {
                string titleText = (IsNewRow
                            ? LocalizedMsgSystem.GetVar("MsgPopupNewRow")
                            : LocalizedMsgSystem.GetVar("MsgPopupEditRow"));

                ManageDataWindow DataEditWindow = new()
                {
                    Title = titleText.Replace("{0}", CurrTableRow.CurrEntity.TableName),
                    SetTableMeta = CurrTableRow,
                    SetDataBot = dataBot
                };

                DataEditWindow.SaveRecordEvent +=
                    // save a new record - add to the database, an existing record is already tracked in EF Core so it just needs EFCore to save changes
                    IsNewRow ?
                      (sender, e) => DataAddNewRowEvent?.Invoke(sender, new AddNewRowEventArgs(CurrTableRow.CurrEntity))
                    : (sender, e) => DataEditRowEvent?.Invoke(sender, new UpdatedDataRowArgs(CurrTableRow));


                DataEditWindow.CancelRecordEvent +=
                    // discard a canceled new row, but save any existing record because the GUI needs to sync any adjustments the user made.
                    IsNewRow ?
                      (sender, e) => { return; }
                : (sender, e) => { return; };

                DataEditWindow.ShowDialog();
            });
        }
    }
}
