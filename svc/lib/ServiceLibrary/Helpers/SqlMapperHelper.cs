using System.Collections.Generic;
using System.Data;
using System.Globalization;
using Dapper;

namespace ServiceLibrary.Helpers
{
    public class SqlMapperHelper
    {
        public static SqlMapper.ICustomQueryParameter ToInt32Collection(IEnumerable<int> items)
        {
            var dataTable = new DataTable { Locale = CultureInfo.InvariantCulture };
            dataTable.Columns.Add("Int32Value", typeof(int));
            if (items != null)
            {
                foreach (var item in items)
                {
                    dataTable.Rows.Add(item);
                }
            }

            return dataTable.AsTableValuedParameter("[dbo].[INT32COLLECTION]");
        }
    }
}
