using System.Collections.Generic;
using System.Linq;
using AdminStore.Models.Enums;
using ServiceLibrary.Models;
using ServiceLibrary.Models.Enums;
using UserType = ServiceLibrary.Models.Enums.UserType;

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

        public static IEnumerable<int> ParsingTypesToUserTypeArray(IEnumerable<KeyValuePair<int, UserType>> types, UserType userType)
        {
            var members = types.Where(e => e.Value == userType).Select(e => e.Key).ToList();
            return members;
        }
    }
}