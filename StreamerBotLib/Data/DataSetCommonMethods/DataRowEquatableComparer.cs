using System.Data;
using System.Diagnostics.CodeAnalysis;

namespace StreamerBotLib.Data.DataSetCommonMethods
{
    internal class DataRowEquatableComparer : IEqualityComparer<DataRow>
    {
        public bool Equals(DataRow x, DataRow y)
        {
            bool found = false;
            if (x.GetType() == y.GetType())
            {
                found = true;

                foreach (DataColumn column in y.Table.Columns)
                {
                    if (!x[column].Equals(y[column]))
                    {
                        found = false;
                    }
                }
            }
            else
            {
                found = false;
            }

            return found;
        }

        public int GetHashCode([DisallowNull] DataRow obj)
        {
            throw new NotImplementedException();
        }
    }
}
