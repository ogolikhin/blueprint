using System.Linq;
using System.Threading.Tasks;
using AdminStore.Models;
using AdminStore.Models.Enums;
using AdminStore.Repositories;

namespace AdminStore.Helpers
{
    public static class UsersHelper
    {
        public static async Task<User> CreateDbUserFromDtoAsync(UserDto user, OperationMode operationMode, ISqlSettingsRepository settingsRepository, int userId = 0)
        {
            UserValidator.ValidateModel(user, operationMode);
            var dbUserModel = await UserConverter.ConvertToDbUser(user, operationMode, settingsRepository, userId);
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