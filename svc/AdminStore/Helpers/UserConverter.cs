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
        public static async Task<User> ConvertToDbUser(UserDto user, OperationMode operationMode, ISqlSettingsRepository settingsRepository , int userId = 0)
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
                var isPasswordRequired = false;
                var settings = await settingsRepository.GetUserManagementSettingsAsync();
                if (settings.IsFederatedAuthenticationEnabled && user.AllowFallback.HasValue && user.AllowFallback.Value)
                {
                    isPasswordRequired = true;
                }

                var decodedPassword = SystemEncryptions.Decode(user.Password);

                ValidatePassword(databaseUser, decodedPassword, isPasswordRequired);

                if (!string.IsNullOrWhiteSpace(decodedPassword))
                {
                    databaseUser.Password = HashingUtilities.GenerateSaltedHash(decodedPassword, databaseUser.UserSALT);
                }
                else
                {
                    databaseUser.Password = Guid.NewGuid() + "ABC!@#$";
                }
            }

            return databaseUser;
        }

        public static void ValidatePassword(User user, string decodedPassword, bool isPasswordRequired)
        {
            string errorMessage;
            if (!PasswordValidationHelper.ValidatePassword(decodedPassword, isPasswordRequired, out errorMessage))
            {
                throw new BadRequestException(errorMessage, ErrorCodes.BadRequest);
            }

            if (!string.IsNullOrWhiteSpace(decodedPassword))
            {
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
        }
    }
}
