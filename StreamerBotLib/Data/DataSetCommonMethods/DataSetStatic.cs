using StreamerBotLib.GUI;
using StreamerBotLib.Static;

using System.Collections.Generic;
using System.Data;
using System.Linq;

namespace StreamerBotLib.Data.DataSetCommonMethods
{
    internal static class DataSetStatic
    {

        #region Get Data

        internal static List<string> GetTableFields(DataTable dataTable)
        {
#if LogDataManager_Actions
            LogWriter.DataActionLog(MethodBase.GetCurrentMethod().Name, $"Get all the fields for table {dataTable.TableName}.");
#endif
            lock (GUIDataManagerLock.Lock)
            {
                return new(from DataColumn dataColumn in dataTable.Columns
                           select dataColumn.ColumnName);
            }
        }

        internal static DataRow GetRow(DataTable dataTable, string Filter = null, string Sort = null)
        {
#if LogDataManager_Actions
            LogWriter.DataActionLog(MethodBase.GetCurrentMethod().Name, $"Get single row data for {dataTable.TableName} with specified filter {Filter ?? "null"} and sort {Sort}.");
#endif

            lock (GUIDataManagerLock.Lock)
            {
                return dataTable.Select(Filter, Sort).FirstOrDefault();
            }
        }

        internal static DataRow[] GetRows(DataTable dataTable, string Filter = null, string Sort = null)
        {
#if LogDataManager_Actions
            LogWriter.DataActionLog(MethodBase.GetCurrentMethod().Name, $"Get multiple row data for {dataTable.TableName} with specified filter {Filter ?? "null"} and sort {Sort}.");
#endif

            lock (GUIDataManagerLock.Lock)
            {
                return dataTable.Select(Filter, Sort);
            }
        }

        internal static List<object> GetRowsDataColumn(DataTable dataTable, DataColumn dataColumn)
        {
#if LogDataManager_Actions
            LogWriter.DataActionLog(MethodBase.GetCurrentMethod().Name, $"Get column data for table {dataTable.TableName} and the column {dataColumn.ColumnName}.");
#endif

            lock (GUIDataManagerLock.Lock)
            {
                return new(from DataRow row in dataTable.Select()
                           select row[dataColumn]);
            }
        }

        internal static string GetKey(DataTable dataTable, string Table)
        {
#if LogDataManager_Actions
            LogWriter.DataActionLog(MethodBase.GetCurrentMethod().Name, $"Check table {Table} and get the keys.");
#endif

            // TODO: better error check this method, espeically for null key fields or multiple key fields
            lock (GUIDataManagerLock.Lock)
            {
                string key = "";

                if (Table != null && Table != "")
                {
                    DataColumn[] k = dataTable?.PrimaryKey;
                    if (k?.Length > 1)
                    {
                        foreach (var d in from DataColumn d in k
                                          where d.ColumnName != "Id"
                                          select d)
                        {
                            key = d.ColumnName;
                        }
                    }
                    else if (k?.Length == 1)
                    {
                        key = k?[0].ColumnName;
                    }
                }
                return key;
            }
        }

        internal static List<string> GetTableFields(DataTable dataTable, string TableName)
        {
#if LogDataManager_Actions
            LogWriter.DataActionLog(MethodBase.GetCurrentMethod().Name, $"Get all the fields for table {TableName}.");
#endif
            lock (GUIDataManagerLock.Lock)
            {
                List<string> fields = new(from DataColumn dataColumn in dataTable.Columns
                                          select dataColumn.ColumnName);
                return fields;
            }
        }
        #endregion

        internal static void SetDataRowFieldRow(DataRow dataRow, string dataColumn, object value)
        {
            lock (GUIDataManagerLock.Lock)
            {
                dataRow[dataColumn] = value;
            }
        }

        internal static void SetDataTableFieldRows(DataTable dataTable, DataColumn dataColumn, object value, string Filter = null)
        {
#if LogDataManager_Actions
            LogWriter.DataActionLog(MethodBase.GetCurrentMethod().Name, $"Set data for table {dataTable.TableName} and column {dataColumn.ColumnName} to value {value}, and with the filter {Filter ?? "null"}.");
#endif

            lock (GUIDataManagerLock.Lock)
            {
                bool found = false;
                foreach (DataRow row in dataTable.Select(Filter))
                {
                    row[dataColumn] = value;
                    found = true;
                }
                if (found)
                {
                    dataTable.AcceptChanges();
                }
            }
        }

        #region Delete Data

        /// <summary>
        /// Delete the provided rows.
        /// </summary>
        /// <param name="dataRows">Enumerable list of DataRows to perform a delete on each item.</param>
        internal static void DeleteDataRows(IEnumerable<DataRow> dataRows)
        {
#if LogDataManager_Actions
            LogWriter.DataActionLog(MethodBase.GetCurrentMethod().Name, $"Delete all provided data rows.");
#endif

            lock (GUIDataManagerLock.Lock)
            {
                List<DataTable> temp = new(); // manage tables with deleted rows
                foreach (DataRow D in dataRows)
                {
                    D.Delete();
                    temp.UniqueAdd(D.Table); // track the tables with rows deleted
                }

                // accept the changes for every table with deleted rows
                temp.ForEach((T) => T.AcceptChanges());
            }
        }

        internal static bool DeleteDataRow(DataTable table, string Filter)
        {
#if LogDataManager_Actions
            LogWriter.DataActionLog(MethodBase.GetCurrentMethod().Name, $"Delete data row for table {table.TableName} and the filter {Filter}.");
#endif

            lock (GUIDataManagerLock.Lock)
            {
                bool result = false;
                DataRow temp = table.Select(Filter).FirstOrDefault();

                if (temp != null)
                {
                    result = true;
                    temp.Delete();
                    table.AcceptChanges();
                }

                return result;
            }
        }

        #endregion

    }
}
