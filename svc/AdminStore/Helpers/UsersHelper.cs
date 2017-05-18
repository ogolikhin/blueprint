using System.Collections.Generic;
using AdminStore.Models;
using AdminStore.Models.Enums;

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
                "enabled"
            };
            var column = sorting.Sort;
            var sortColumn = !string.IsNullOrWhiteSpace(column) && sortableColumns.Contains(column)
                ? column
                : defaultSortColumn;

            return sorting.Order == SortOrder.Desc ? "-" + sortColumn : sortColumn;
        }

        public static User CreateDbUserFromDto(UserDto user, UserOperationMode userOperationMode, int userId = 0)
        {
            UserValidator.ValidateModel(user, userOperationMode);
            var dbUserModel = UserConverter.ConvertToDbUser(user, userOperationMode, userId);
            return dbUserModel;
        }
    }
}