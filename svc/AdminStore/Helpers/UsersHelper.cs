using AdminStore.Models;
using AdminStore.Models.Enums;

namespace AdminStore.Helpers
{
    public static class UsersHelper
    {
        public static string SortUsers(Sorting sorting)
        {
            var orderField = "displayName";
            switch (sorting.Sort)
            {
                case "source":
                    orderField = sorting.Order == "asc" ? "source" : "-source";
                    break;
                case "enabled":
                    orderField = sorting.Order == "asc" ? "enabled" : "-enabled";
                    break;
                case "license":
                    orderField = sorting.Order == "asc" ? "license" : "-license";
                    break;
                case "role":
                    orderField = sorting.Order == "asc" ? "role" : "-role";
                    break;
                case "department":
                    orderField = sorting.Order == "asc" ? "department" : "-department";
                    break;
                case "title":
                    orderField = sorting.Order == "asc" ? "title" : "-title";
                    break;
                case "email":
                    orderField = sorting.Order == "asc" ? "email" : "-email";
                    break;
                case "displayName":
                    orderField = sorting.Order == "asc" ? "displayName" : "-displayName";
                    break;
                case "login":
                    orderField = sorting.Order == "asc" ? "login" : "-login";
                    break;
            }

            return orderField;
        }

        public static User CreateDbUserFromDto(UserDto user, UserOperationMode userOperationMode, int userId = 0)
        {
            UserValidator.ValidateModel(user);
            var dbUserModel = UserConverter.ConvertToDbUser(user, userOperationMode, userId);
            return dbUserModel;
        }
    }
}