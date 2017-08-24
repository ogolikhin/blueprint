using System.Collections.Generic;
using ServiceLibrary.Models;
using ServiceLibrary.Models.Enums;

namespace AdminStore.Helpers
{
    public class SortingHelper
    {
        public static string SortWorkflows(Sorting sorting)
        {
            var defaultSortColumn = "name";

            var sortableColumns = new HashSet<string>
            {
                defaultSortColumn,
                "status",
                "createdBy",
                "lastModified",
                "dateCreated"
            };

            var column = sorting.Sort;
            var sortColumn = !string.IsNullOrWhiteSpace(column) && sortableColumns.Contains(column)
                ? column
                : defaultSortColumn;

            return sorting.Order == SortOrder.Desc ? "-" + sortColumn : sortColumn;
        }
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
        public static string SortUsers(Sorting sorting)
        {
            var defaultSortColumn = "login";
            var sortableColumns = new HashSet<string>
            {
                "login",
                "email",
                "license",
                "role",
                "department",
                "title",
                "source",
                "enabled",
                "licenseType",
                "displayName",
                "instanceAdminRoleName"
            };
            var column = sorting.Sort;
            var sortColumn = !string.IsNullOrWhiteSpace(column) && sortableColumns.Contains(column)
                ? column
                : defaultSortColumn;

            return sorting.Order == SortOrder.Desc ? "-" + sortColumn : sortColumn;
        }

        public static string SortProjectRolesAssignments(Sorting sorting)
        {
            var defaultSortColumn = "groupName";

            var sortableColumns = new HashSet<string>
            {
                defaultSortColumn,
                "roleName",
            };

            var column = sorting.Sort;
            var sortColumn = !string.IsNullOrWhiteSpace(column) && sortableColumns.Contains(column)
                ? column
                : defaultSortColumn;

            return sorting.Order == SortOrder.Desc ? "-" + sortColumn : sortColumn;
        }

        public static string SortProjectGroups(Sorting sorting)
        {
            var defaultSortColumn = "name";

            var sortableColumns = new HashSet<string>
            {
                defaultSortColumn,
                "scope"
            };

            var column = sorting.Sort;
            var sortColumn = !string.IsNullOrWhiteSpace(column) && sortableColumns.Contains(column)
                ? column
                : defaultSortColumn;

            return sorting.Order == SortOrder.Desc ? "-" + sortColumn : sortColumn;
        }
    }
}