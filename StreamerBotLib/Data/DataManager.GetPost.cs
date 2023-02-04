#if DEBUG
#define noLogDataManager_Actions
#endif

using StreamerBotLib.Data.DataSetCommonMethods;
using StreamerBotLib.GUI;
using StreamerBotLib.Interfaces;

using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;

namespace StreamerBotLib.Data
{
    /// <summary>
    /// Manages the database - get data and post-new or updated-data
    /// </summary>
    public partial class DataManager : IDataManageReadOnly
    {
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

        #endregion

        #region DeleteData

        public void DeleteDataRows(IEnumerable<DataRow> dataRows)
        {
            DataSetStatic.DeleteDataRows(dataRows);
        }
        #endregion

        #region Check DataSet Schema

        /// <summary>
        /// Check if the provided table exists within the database system.
        /// </summary>
        /// <param name="table">The table name to check.</param>
        /// <returns><i>true</i> - if database contains the supplied table, <i>false</i> - if database doesn't contain the supplied table.</returns>
        private bool CheckTable(string table)
        {
#if LogDataManager_Actions
            LogWriter.DataActionLog(MethodBase.GetCurrentMethod().Name, $"Check if table {table} is in the database.");
#endif

            lock (GUIDataManagerLock.Lock)
            {
                return _DataSource.Tables.Contains(table);
            }
        }


        #endregion

        #region Interface

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

        public List<object> GetRowsDataColumn(string dataTable, string dataColumn)
        {
            if (!CheckTable(dataTable) || !CheckField(dataTable, dataColumn))
            {
                throw new ArgumentException($"The data table {dataTable} or data table column {dataColumn} or both were not found in the database.");
            }
            return DataSetStatic.GetRowsDataColumn(_DataSource.Tables[dataTable], _DataSource.Tables[dataTable].Columns[dataColumn]);
        }

        public string GetKey(string Table)
        {
            return DataSetStatic.GetKey(_DataSource.Tables[Table], Table);
        }

        /// <summary>
        /// Gets a single column and all rows for the provided table and column.
        /// </summary>
        /// <param name="dataTable">The string name of the table.</param>
        /// <param name="dataColumn">The string name of the column of the table.</param>
        /// <returns></returns>
        /// <exception cref="ArgumentException">Occurs if either the table, column, or both names are invalid and not found in the database.</exception>
        public IEnumerable<string> GetKeys(string Table)
        {
#if LogDataManager_Actions
            LogWriter.DataActionLog(MethodBase.GetCurrentMethod().Name, $"Check table {Table} and get the keys.");
#endif

            // TODO: better error check this method, espeically for null key fields or multiple key fields
            List<string> keys = new();

            lock (GUIDataManagerLock.Lock)
            {
                if (Table is not null and not "")
                {
                    DataColumn[] k = _DataSource?.Tables[Table]?.PrimaryKey;
                    if (k?.Length > 1)
                    {
                        keys.AddRange(from d in
                                          from DataColumn d in k
                                          where d.ColumnName != "Id"
                                          select d
                                      select d.ColumnName);
                    }
                    else if (k?.Length == 1)
                    {
                        keys.Add(k?[0].ColumnName);
                    }
                }
                return keys;
            }
        }

        public List<string> GetTableFields(string TableName)
        {
            return DataSetStatic.GetTableFields(_DataSource.Tables[TableName], TableName);
        }

        public List<string> GetTableNames()
        {
#if LogDataManager_Actions
            LogWriter.DataActionLog(MethodBase.GetCurrentMethod().Name, $"Get the names of all database tables.");
#endif
            lock (GUIDataManagerLock.Lock)
            {
                return new(from DataTable table in _DataSource.Tables
                                         select table.TableName);
            }
        }
        #endregion
    }
}
