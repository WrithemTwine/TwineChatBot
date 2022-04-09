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

        public void PostUpdatedDataRow(bool RowChanged)
        {
            if (RowChanged)
            {
                NotifySaveData();
            }
        }



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

    }
}
