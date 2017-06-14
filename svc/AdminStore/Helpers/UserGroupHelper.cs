using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using ServiceLibrary.Models;
using ServiceLibrary.Models.Enums;

namespace AdminStore.Helpers
{
    public class UserGroupHelper
    {
        public static string SortUsergroups(Sorting sorting)
        {
            var defaultSortColumn = "displayName";

            var sortableColumns = new HashSet<string>
            {
                defaultSortColumn,
                "source",
                "type",
                "scope",
                "licenseType",
                "email",
                "userName"
            };

            var column = sorting.Sort;
            var sortColumn = !string.IsNullOrWhiteSpace(column) && sortableColumns.Contains(column)
                ? column
                : defaultSortColumn;

            return sorting.Order == SortOrder.Desc ? "-" + sortColumn : sortColumn;
        }
    }
}