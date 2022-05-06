using StreamerBotLib.Interfaces;

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
        #region Check DataSet Schema

        /// <summary>
        /// Check if the provided table exists within the database system.
        /// </summary>
        /// <param name="table">The table name to check.</param>
        /// <returns><i>true</i> - if database contains the supplied table, <i>false</i> - if database doesn't contain the supplied table.</returns>
        public bool CheckTable(string table)
        {
            lock (_DataSource)
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
            lock (_DataSource)
            {
                return _DataSource.Tables[table].Columns.Contains(field);
            }
        }

        #endregion

        #region Get Data
        public List<string> GetTableNames()
        {
            List<string> names = new(from DataTable table in _DataSource.Tables
                                     select table.TableName);
            return names;
        }


        public List<string> GetTableFields(string TableName)
        {
            List<string> fields = new(from DataColumn dataColumn in _DataSource.Tables[TableName].Columns
                                      select dataColumn.ColumnName);
            return fields;
        }


        public List<string> GetTableFields(DataTable dataTable)
        {
            return new(from DataColumn dataColumn in dataTable.Columns
                                      select dataColumn.ColumnName);
        }


        public string GetKey(string Table)
        {
            // TODO: better error check this method, espeically for null key fields or multiple key fields

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
                else if(k?.Length == 1)
                {
                    key = k?[0].ColumnName;
                }
            }
            return key;
        }


        public DataRow GetRow(DataTable dataTable, string Filter = null, string Sort = null)
        {
            lock (_DataSource)
            {
                return dataTable.Select(Filter, Sort).FirstOrDefault();
            }
        }


        public DataRow[] GetRows(DataTable dataTable, string Filter = null, string Sort = null)
        {
            lock (_DataSource)
            {
                return dataTable.Select(Filter, Sort);
            }
        }


        public List<object> GetRowsDataColumn(DataTable dataTable, DataColumn dataColumn)
        {
            lock (_DataSource)
            {
                return new(from DataRow row in dataTable.Select()
                           select row[dataColumn]);
            }
        }

        #endregion

        #region Update Data
        
        /// <summary>
        /// When user edits rows, this notification initiates the save process.
        /// </summary>
        /// <param name="RowChanged">True or False based on whether data changed.</param>
        public void PostUpdatedDataRow(bool RowChanged)
        {
            if (RowChanged)
            {
                NotifySaveData();
            }
        }

        public void SetDataTableFieldRows(DataTable dataTable, DataColumn dataColumn, object value, string Filter = null)
        {
            lock (_DataSource)
            {
                foreach(DataRow row in dataTable.Select(Filter))
                {
                    row[dataColumn] = value;
                }
            }
            NotifySaveData();
        }

        #endregion

        #region Delete Data

        /// <summary>
        /// Delete the provided rows.
        /// </summary>
        /// <param name="dataRows">Enumerable list of DataRows to perform a delete on each item.</param>
        public void DeleteDataRows(IEnumerable<DataRow> dataRows)
        {
            lock (_DataSource)
            {
                foreach (DataRow D in dataRows)
                {
                    D.Delete();
                }
            }
            NotifySaveData();
        }

        public bool DeleteDataRow(DataTable table, string Filter)
        {
            bool result = false;
            lock (_DataSource)
            {
                DataRow temp = table.Select(Filter).FirstOrDefault();
                
                if(temp!=null)
                {
                    result = true;
                    temp.Delete();
                    NotifySaveData();
                }
            }

            return result;
        }

        #endregion
    }
}
