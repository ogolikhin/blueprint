﻿using System.Collections.Generic;
using System.Threading.Tasks;
using AdminStore.Models;
using AdminStore.Models.Enums;
using AdminStore.Repositories;
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

        public static async Task<User> CreateDbUserFromDto(UserDto user, OperationMode operationMode, ISqlSettingsRepository settingsRepository, int userId = 0)
        {
            UserValidator.ValidateModel(user, operationMode);
            var dbUserModel = await UserConverter.ConvertToDbUser(user, operationMode, settingsRepository, userId);
            return dbUserModel;
        }
    }
}