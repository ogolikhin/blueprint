using System;
using System.Net;
using System.Threading.Tasks;
using System.Web.Http;
using AdminStore.Models;
using AdminStore.Repositories;
using ServiceLibrary.Exceptions;
using ServiceLibrary.Helpers;
using ServiceLibrary.Models;

namespace AdminStore.Helpers
{
    public static class UsersHelper
    {
        public static string SortUsers(string sortString)
        {
            var orderField = "displayName";
            var sortArray = sortString.Split(',');
            foreach (var sort in sortArray)
            {
                switch (sort)
                {
                    case "source":
                        orderField = "source";
                        break;
                    case "-source":
                        orderField = "-source";
                        break;
                    case "enabled":
                        orderField = "enabled";
                        break;
                    case "-enabled":
                        orderField = "-enabled";
                        break;
                    case "license":
                        orderField = "license";
                        break;
                    case "-license":
                        orderField = "-license";
                        break;
                    case "role":
                        orderField = "role";
                        break;
                    case "-role":
                        orderField = "-role";
                        break;
                    case "department":
                        orderField = "department";
                        break;
                    case "-department":
                        orderField = "-department";
                        break;
                    case "title":
                        orderField = "title";
                        break;
                    case "-title":
                        orderField = "-title";
                        break;
                    case "email":
                        orderField = "email";
                        break;
                    case "-email":
                        orderField = "-email";
                        break;
                    case "displayName":
                        orderField = "displayName";
                        break;
                    case "-displayName":
                        orderField = "-displayName";
                        break;
                    case "login":
                        orderField = "login";
                        break;
                    case "-login":
                        orderField = "-login";
                        break;
                }
            }
            return orderField;
        }

        public static User CreateDbUserFromDto(int userId, CreationUserDto user)
        {
            ValidateModel(user);
            var dbUserModel = PrepareDbUser(userId, user);
            return dbUserModel;
        }

        public static async Task<bool> HasValidPermissions(int sessionUserId, CreationUserDto user, ISqlPrivilegesRepository sqlPrivilegesRepository)
        {
            var userPermissions = await sqlPrivilegesRepository.GetUserPermissionsAsync(sessionUserId);
            if (!PermissionsChecker.IsFlagBelongPermissions(userPermissions, InstanceAdminPrivileges.ManageUsers))
                return false;

            if (user.InstanceAdminRoleId.HasValue && (!PermissionsChecker.IsFlagBelongPermissions(userPermissions, InstanceAdminPrivileges.AssignAdminRoles)))
            {
                return false;
            }
            return true;
        }

        private static void ValidateModel(CreationUserDto user)
        {
            if (string.IsNullOrEmpty(user.Login))
            {
                throw new BadRequestException(ErrorMessages.LoginRequired, ErrorCodes.BadRequest);
            }

            if (user.Login.Length < 4 || user.Login.Length > 256)
            {
                throw new BadRequestException(ErrorMessages.LoginFieldLimitation, ErrorCodes.BadRequest);
            }

            if (string.IsNullOrEmpty(user.DisplayName))
            {
                throw new BadRequestException(ErrorMessages.DisplayNameRequired, ErrorCodes.BadRequest);
            }

            if (user.DisplayName.Length < 2 || user.DisplayName.Length > 255)
            {
                throw new BadRequestException(ErrorMessages.DisplayNameFieldLimitation, ErrorCodes.BadRequest);
            }

            if (string.IsNullOrEmpty(user.FirstName))
            {
                throw new BadRequestException(ErrorMessages.FirstNameRequired, ErrorCodes.BadRequest);
            }

            if (user.FirstName.Length < 2 || user.FirstName.Length > 255)
            {
                throw new BadRequestException(ErrorMessages.FirstNameFieldLimitation, ErrorCodes.BadRequest);
            }

            if (string.IsNullOrEmpty(user.LastName))
            {
                throw new BadRequestException(ErrorMessages.LastNameRequired, ErrorCodes.BadRequest);
            }

            if (user.LastName.Length < 2 || user.LastName.Length > 255)
            {
                throw new BadRequestException(ErrorMessages.LastNameFieldLimitation, ErrorCodes.BadRequest);
            }

            if (!string.IsNullOrEmpty(user.Email) && (user.Email.Length < 4 || user.Email.Length > 255))
            {
                throw new BadRequestException(ErrorMessages.EmailFieldLimitation, ErrorCodes.BadRequest);
            }

            if (!string.IsNullOrEmpty(user.Title) && (user.Title.Length < 2 || user.Title.Length > 255))
            {
                throw new BadRequestException(ErrorMessages.TitleFieldLimitation, ErrorCodes.BadRequest);
            }

            if (!string.IsNullOrEmpty(user.Department) && (user.Department.Length < 1 || user.Department.Length > 255))
            {
                throw new BadRequestException(ErrorMessages.DepartmentFieldLimitation, ErrorCodes.BadRequest);
            }
        }

        private static User PrepareDbUser(int userId, CreationUserDto user)
        {
            var databaseUser = new User
            {
                Department = user.Department,
                Enabled = user.Enabled,
                ExpirePassword = user.ExpirePassword,
                GroupMembership = user.GroupMembership,
                Guest = user.Guest,
                Image_ImageId = user.ImageId,
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
                CurrentVersion = (user as UpdateUserDto)?.CurrentVersion ?? 0
            };

            if (!user.AllowFallback.HasValue || !user.AllowFallback.Value)
            {
                var decodedPasword = SystemEncryptions.Decode(user.NewPassword);
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