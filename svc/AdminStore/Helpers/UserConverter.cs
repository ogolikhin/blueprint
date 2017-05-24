using System;
using AdminStore.Models;
using AdminStore.Models.Enums;
using ServiceLibrary.Exceptions;
using ServiceLibrary.Helpers;

namespace AdminStore.Helpers
{
    public class UserConverter
    {
        public static User ConvertToDbUser(UserDto user, OperationMode operationMode, int userId = 0)
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
                var decodedPasword = SystemEncryptions.Decode(user.Password);

                string errorMessage;
                var isValidPassword = PasswordValidationHelper.ValidatePassword(decodedPasword, true, out errorMessage);
                if (!isValidPassword)
                {
                    throw new BadRequestException(errorMessage, ErrorCodes.BadRequest);
                }

                var passwordUppercase = decodedPasword.ToUpperInvariant();

                if (passwordUppercase == user.Login?.ToUpperInvariant())
                {
                    throw new BadRequestException(ErrorMessages.PasswordSameAsLogin, ErrorCodes.PasswordSameAsLogin);
                }

                if (passwordUppercase == user.DisplayName?.ToUpperInvariant())
                {
                    throw new BadRequestException(ErrorMessages.PasswordSameAsDisplayName, ErrorCodes.PasswordSameAsDisplayName);
                }

                databaseUser.Password = HashingUtilities.GenerateSaltedHash(decodedPasword, databaseUser.UserSALT);
            }

            return databaseUser;
        }

        public static void ValidatePassword(User user, string encodedPassword)
        {
            var decodedPasword = SystemEncryptions.Decode(encodedPassword);
            string errorMessage;
            var isValidPassword = PasswordValidationHelper.ValidatePassword(decodedPasword, true, out errorMessage);
            if (!isValidPassword)
            {
                throw new BadRequestException(errorMessage, ErrorCodes.BadRequest);
            }
            var passwordUppercase = decodedPasword.ToUpperInvariant();

            if (passwordUppercase == user.Login?.ToUpperInvariant())
            {
                throw new BadRequestException(ErrorMessages.PasswordSameAsLogin, ErrorCodes.PasswordSameAsLogin);
            }
            if (passwordUppercase == user.DisplayName?.ToUpperInvariant())
            {
                throw new BadRequestException(ErrorMessages.PasswordSameAsDisplayName, ErrorCodes.PasswordSameAsDisplayName);
            }
        }
    }
}