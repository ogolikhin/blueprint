using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using AdminStore.Models;
using ServiceLibrary.Exceptions;
using ServiceLibrary.Helpers;

namespace AdminStore.Helpers
{
    public class UserConverter
    {
        public static User ConvertToDbUser(UserDto user, int userId = 0)
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
                UserId = userId,
                CurrentVersion = user.CurrentVersion
            };

            if (!user.AllowFallback.HasValue || !user.AllowFallback.Value)
            {
                var decodedPasword = SystemEncryptions.Decode(user.Password);
                string errorMessage;
                var isValidPassword = PasswordValidationHelper.ValidatePassword(decodedPasword, true, out errorMessage);
                if (!isValidPassword)
                {
                    throw new BadRequestException(errorMessage, ErrorCodes.BadRequest);
                }
                databaseUser.Password = HashingUtilities.GenerateSaltedHash(decodedPasword, databaseUser.UserSALT);
            }
            else
            {
                databaseUser.Password = null;
            }
            return databaseUser;
        }
    }
}