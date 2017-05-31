using System;
using System.Threading.Tasks;
using AdminStore.Models;
using AdminStore.Models.Enums;
using AdminStore.Repositories;
using ServiceLibrary.Exceptions;
using ServiceLibrary.Helpers;

namespace AdminStore.Helpers
{
    public class UserConverter
    {
        public static async Task<User> ConvertToDbUser(UserDto user, OperationMode operationMode,
            ISqlSettingsRepository settingsRepository, int userId = 0)
        {
            var databaseUser = new User
            {
                Department = user.Department,
                Enabled = user.Enabled,
                ExpirePassword = user.ExpirePassword,
                GroupMembership = user.GroupMembership,
                Guest = user.Guest,
                Image_ImageId = user.Image_ImageId,
                Title = user.Title,
                Login = user.Login,
                Source = user.Source,
                InstanceAdminRoleId = user.InstanceAdminRoleId,
                AllowFallback = user.AllowFallback,
                DisplayName = user.DisplayName,
                FirstName = user.FirstName,
                LastName = user.LastName,
                Email = user.Email,
                UserSALT = Guid.NewGuid(),
                Id = userId,
                CurrentVersion = user.CurrentVersion
            };

            if (operationMode == OperationMode.Create)
            {
                var settings = await settingsRepository.GetUserManagementSettingsAsync();

                var decodedPassword = SystemEncryptions.Decode(user.Password);

                if ((settings.IsFederatedAuthenticationEnabled && user.AllowFallback.HasValue && user.AllowFallback.Value) || !string.IsNullOrWhiteSpace(decodedPassword))
                {
                    ValidatePassword(databaseUser, decodedPassword);
                    databaseUser.Password = HashingUtilities.GenerateSaltedHash(decodedPassword, databaseUser.UserSALT);
                }
                else
                {
                    GeneratePassword(databaseUser);
                } 
            }

            return databaseUser;
        }

        public static void ValidatePassword(User user, string decodedPassword)
        {
            string errorMessage;
            if (!PasswordValidationHelper.ValidatePassword(decodedPassword, true, out errorMessage))
            {
                throw new BadRequestException(errorMessage, ErrorCodes.BadRequest);
            }

            var passwordUppercase = decodedPassword.ToUpperInvariant();

            if (passwordUppercase == user.Login?.ToUpperInvariant())
            {
                throw new BadRequestException(ErrorMessages.PasswordSameAsLogin, ErrorCodes.PasswordSameAsLogin);
            }

            if (passwordUppercase == user.DisplayName?.ToUpperInvariant())
            {
                throw new BadRequestException(ErrorMessages.PasswordSameAsDisplayName,
                    ErrorCodes.PasswordSameAsDisplayName);
            }
        }

        private static void GeneratePassword(User user)
        {
            user.Password = Guid.NewGuid() + "ABC!@#$";
        }
    }
}
