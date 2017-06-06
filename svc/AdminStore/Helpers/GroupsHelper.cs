using System.Collections.Generic;
using ServiceLibrary.Models;
using ServiceLibrary.Models.Enums;

namespace AdminStore.Helpers
{
    public class GroupsHelper
    {
        public static string SortGroups(Sorting sorting)
        {
            var defaultSortColumn = "name";

            var sortableColumns = new HashSet<string>
            {
                defaultSortColumn,
                "scope",
                "licenseType",
                "email",
                "groupType",
                "source",
                "displayName",
                "login"
            };
            
            var column = sorting.Sort;
            var sortColumn = !string.IsNullOrWhiteSpace(column) && sortableColumns.Contains(column)
                ? column
                : defaultSortColumn;

            return sorting.Order == SortOrder.Desc ? "-" + sortColumn : sortColumn;
        }      
    }
}