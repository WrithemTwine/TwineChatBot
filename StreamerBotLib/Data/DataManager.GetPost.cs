#if DEBUG
#define noLogDataManager_Actions
#endif

using StreamerBotLib.GUI;
using StreamerBotLib.Interfaces;
using StreamerBotLib.Static;

using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Reflection;
using System;

namespace StreamerBotLib.Data
{
    /// <summary>
    /// Manages the database - get data and post-new or updated-data
    /// </summary>
    public partial class DataManager : IDataManageReadOnly
    {
        #region Check DataSet Schema

        /// <summary>
        /// Check if the provided table exists within the database system.
        /// </summary>
        /// <param name="table">The table name to check.</param>
        /// <returns><i>true</i> - if database contains the supplied table, <i>false</i> - if database doesn't contain the supplied table.</returns>
        public bool CheckTable(string table)
        {
#if LogDataManager_Actions
            LogWriter.DataActionLog(MethodBase.GetCurrentMethod().Name, $"Check if table {table} is in the database.");
#endif

            lock (GUIDataManagerLock.Lock)
            {
                return _DataSource.Tables.Contains(table);
            }
        }

        /// <summary>
        /// Check if the provided field is part of the supplied table.
        /// </summary>
        /// <param name="table">The table to check.</param>
        /// <param name="field">The field within the table to see if it exists.</param>
        /// <returns><i>true</i> - if table contains the supplied field, <i>false</i> - if table doesn't contain the supplied field.</returns>
        public bool CheckField(string table, string field)
        {
#if LogDataManager_Actions
            LogWriter.DataActionLog(MethodBase.GetCurrentMethod().Name, $"Check if field {field} is in table {table}.");
#endif

            lock (GUIDataManagerLock.Lock)
            {
                return _DataSource.Tables[table].Columns.Contains(field);
            }
        }

        #endregion

        #region Get Data
        public List<string> GetTableNames()
        {
#if LogDataManager_Actions
            LogWriter.DataActionLog(MethodBase.GetCurrentMethod().Name, $"Get the names of all database tables.");
#endif
            lock (GUIDataManagerLock.Lock)
            {
                List<string> names = new(from DataTable table in _DataSource.Tables
                                         select table.TableName);
                return names;
            }
        }


        public List<string> GetTableFields(string TableName)
        {
#if LogDataManager_Actions
            LogWriter.DataActionLog(MethodBase.GetCurrentMethod().Name, $"Get all the fields for table {TableName}.");
#endif
            lock (GUIDataManagerLock.Lock)
            {
                List<string> fields = new(from DataColumn dataColumn in _DataSource.Tables[TableName].Columns
                                          select dataColumn.ColumnName);
                return fields;
            }
        }


        public List<string> GetTableFields(DataTable dataTable)
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


        public string GetKey(string Table)
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
                    DataColumn[] k = _DataSource?.Tables[Table]?.PrimaryKey;
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


        public DataRow GetRow(DataTable dataTable, string Filter = null, string Sort = null)
        {
#if LogDataManager_Actions
            LogWriter.DataActionLog(MethodBase.GetCurrentMethod().Name, $"Get single row data for {dataTable.TableName} with specified filter {Filter ?? "null"} and sort {Sort}.");
#endif

            lock (GUIDataManagerLock.Lock)
            {
                return dataTable.Select(Filter, Sort).FirstOrDefault();
            }
        }


        public DataRow[] GetRows(DataTable dataTable, string Filter = null, string Sort = null)
        {
#if LogDataManager_Actions
            LogWriter.DataActionLog(MethodBase.GetCurrentMethod().Name, $"Get multiple row data for {dataTable.TableName} with specified filter {Filter ?? "null"} and sort {Sort}.");
#endif

            lock (GUIDataManagerLock.Lock)
            {
                return dataTable.Select(Filter, Sort);
            }
        }


        public List<object> GetRowsDataColumn(DataTable dataTable, DataColumn dataColumn)
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

        /// <summary>
        /// Gets a single column and all rows for the provided table and column.
        /// </summary>
        /// <param name="dataTable">The string name of the table.</param>
        /// <param name="dataColumn">The string name of the column of the table.</param>
        /// <returns></returns>
        /// <exception cref="ArgumentException">Occurs if either the table, column, or both names are invalid and not found in the database.</exception>
        public List<object> GetRowsDataColumn(string dataTable, string dataColumn)
        {
            if (!CheckTable(dataTable) || !CheckField(dataTable, dataColumn))
            {
                throw new ArgumentException($"The data table {dataTable} or data table column {dataColumn} or both were not found in the database.");
            }
            return GetRowsDataColumn(_DataSource.Tables[dataTable], _DataSource.Tables[dataTable].Columns[dataColumn]);
        }

        #endregion

        #region Update Data

        /// <summary>
        /// When user edits rows, this notification initiates the save process.
        /// </summary>
        /// <param name="RowChanged">True or False based on whether data changed.</param>
        public void PostUpdatedDataRow(bool RowChanged)
        {
#if LogDataManager_Actions
            LogWriter.DataActionLog(MethodBase.GetCurrentMethod().Name, $"Notify if the row changed, {RowChanged}.");
#endif
            lock (GUIDataManagerLock.Lock)
            {
                if (RowChanged)
                {
                    NotifySaveData();
                }
            }
        }

        public void SetDataTableFieldRows(DataTable dataTable, DataColumn dataColumn, object value, string Filter = null)
        {
#if LogDataManager_Actions
            LogWriter.DataActionLog(MethodBase.GetCurrentMethod().Name, $"Set data for table {dataTable.TableName} and column {dataColumn.ColumnName} to value {value}, and with the filter {Filter ?? "null"}.");
#endif

            lock (GUIDataManagerLock.Lock)
            {
                foreach (DataRow row in dataTable.Select(Filter))
                {
                    row[dataColumn] = value;
                }
                NotifySaveData();
            }
        }

        #endregion

        #region Delete Data

        /// <summary>
        /// Delete the provided rows.
        /// </summary>
        /// <param name="dataRows">Enumerable list of DataRows to perform a delete on each item.</param>
        public void DeleteDataRows(IEnumerable<DataRow> dataRows)
        {
#if LogDataManager_Actions
            LogWriter.DataActionLog(MethodBase.GetCurrentMethod().Name, $"Delete all provided data rows.");
#endif

            lock (GUIDataManagerLock.Lock)
            {
                foreach (DataRow D in dataRows)
                {
                    D.Delete();
                }
                NotifySaveData();
            }
        }

        public bool DeleteDataRow(DataTable table, string Filter)
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
                    NotifySaveData();
                }

                return result;
            }
        }

        #endregion
    }
}
