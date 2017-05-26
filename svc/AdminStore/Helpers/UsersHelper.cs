using System.Collections.Generic;
using System.Linq;
using AdminStore.Models;
using AdminStore.Models.Enums;
using ServiceLibrary.Models;
using ServiceLibrary.Models.Enums;

namespace AdminStore.Helpers
{
    public static class UsersHelper
    {
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

        public static User CreateDbUserFromDto(UserDto user, OperationMode operationMode, int userId = 0)
        {
            UserValidator.ValidateModel(user, operationMode);
            var dbUserModel = UserConverter.ConvertToDbUser(user, operationMode, userId);
            return dbUserModel;
        }

        public static string ReplaceWildcardCharacters(string search)
        {
            var data = new[]
            {
                new {key = "_", value = "[_]"},
                new {key = "%", value = "[%]"}
            };

            return data.Aggregate(search, (current, row) => current.Replace(row.key, row.value));
        }
    }
}