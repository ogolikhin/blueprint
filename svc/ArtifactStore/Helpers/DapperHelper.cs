﻿using System.Collections.Generic;
using System.Data;

using static Dapper.SqlMapper;

namespace ArtifactStore.Helpers
{
    public static class DapperHelper
    {
        public static ICustomQueryParameter GetIntCollectionTableValueParameter(IEnumerable<int> itemIds)
        {
            DataTable itemIdsTable = new DataTable();
            itemIdsTable.Columns.Add("Int32Value", typeof(int));
            foreach (int itemId in itemIds)
            {
                itemIdsTable.Rows.Add(itemId);
            }
            return itemIdsTable.AsTableValuedParameter("[dbo].[Int32Collection]");
        }
    }
}