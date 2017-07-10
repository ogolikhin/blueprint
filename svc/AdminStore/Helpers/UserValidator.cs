using System.Text.RegularExpressions;
using AdminStore.Models;
using AdminStore.Models.Enums;
using ServiceLibrary.Exceptions;
using ServiceLibrary.Helpers;

namespace AdminStore.Helpers
{
    public class UserValidator
    {
        #region Constants

        public const int MinLoginLength = 4;
        public const int MaxLoginLength = 255;
        public const int MinDisplayNameLength = 1;
        public const int MaxDisplayNameLength = 255;
        public const int MinFirstNameLength = 1;
        public const int MaxFirstNameLength = 255;
        public const int MinLastNameLength = 1;
        public const int MaxLastNameLength = 255;
        public const int MinEmailLength = 4;
        public const int MaxEmailLength = 255;
        public const int MinTitleLength = 1;
        public const int MaxTitleLength = 255;
        public const int MinDepartmentLength = 1;
        public const int MaxDepartmentLength = 255;

        #endregion Constants

        public static void ValidateModel(UserDto user, OperationMode operationMode)
        {
            if (string.IsNullOrWhiteSpace(user.Login))
            {
                throw new BadRequestException(ErrorMessages.LoginRequired, ErrorCodes.BadRequest);
            }

            user.Login = user.Login.Trim();

            if (user.Login.Length < MinLoginLength || user.Login.Length > MaxLoginLength)
            {
                throw new BadRequestException(ErrorMessages.LoginFieldLimitation, ErrorCodes.BadRequest);
            }

            if (!IsValidLogin(user.Login) || IsReservedLogin(user.Login))
            {
                throw new BadRequestException(ErrorMessages.LoginInvalid, ErrorCodes.BadRequest);
            }

            if (string.IsNullOrWhiteSpace(user.DisplayName))
            {
                throw new BadRequestException(ErrorMessages.DisplayNameRequired, ErrorCodes.BadRequest);
            }

            user.DisplayName = user.DisplayName.Trim();

            if (user.DisplayName.Length < MinDisplayNameLength || user.DisplayName.Length > MaxDisplayNameLength)
            {
                throw new BadRequestException(ErrorMessages.DisplayNameFieldLimitation, ErrorCodes.BadRequest);
            }

            if (!string.IsNullOrWhiteSpace(user.FirstName))
            {
                user.FirstName = user.FirstName.Trim();

                if (user.FirstName.Length < MinFirstNameLength || user.FirstName.Length > MaxFirstNameLength)
                {
                    throw new BadRequestException(ErrorMessages.FirstNameFieldLimitation, ErrorCodes.BadRequest);
                }
            }

            if (!string.IsNullOrWhiteSpace(user.LastName))
            {
                user.LastName = user.LastName.Trim();

                if (user.LastName.Length < MinLastNameLength || user.LastName.Length > MaxLastNameLength)
                {
                    throw new BadRequestException(ErrorMessages.LastNameFieldLimitation, ErrorCodes.BadRequest);
                }
            }

            if (!string.IsNullOrEmpty(user.Email))
            {
                user.Email = user.Email.Trim();

                if (user.Email.Length < MinEmailLength || user.Email.Length > MaxEmailLength)
                {
                    throw new BadRequestException(ErrorMessages.EmailFieldLimitation, ErrorCodes.BadRequest);
                }

                if (!IsValidEmail(user.Email))
                {
                    throw new BadRequestException(ErrorMessages.EmailFormatIncorrect, ErrorCodes.BadRequest);
                }
            }

            if (!string.IsNullOrEmpty(user.Title))
            {
                user.Title = user.Title.Trim();

                if (user.Title.Length < MinTitleLength || user.Title.Length > MaxTitleLength)
                {
                    throw new BadRequestException(ErrorMessages.TitleFieldLimitation, ErrorCodes.BadRequest);
                }
            }

            if (!string.IsNullOrEmpty(user.Department))
            {
                user.Department = user.Department.Trim();

                if (user.Department.Length < MinDepartmentLength || user.Department.Length > MaxDepartmentLength)
                {
                    throw new BadRequestException(ErrorMessages.DepartmentFieldLimitation, ErrorCodes.BadRequest);
                }
            }

            if (user.Source != UserGroupSource.Database)
            {
                if (operationMode == OperationMode.Create)
                {
                    throw new BadRequestException(ErrorMessages.CreationOnlyDatabaseUsers, ErrorCodes.BadRequest);
                }

                throw new BadRequestException(ErrorMessages.SourceFieldValueShouldBeOnlyDatabase, ErrorCodes.BadRequest);
            }
        }

        private static bool IsValidLogin(string login)
        {
            var loginRegex = new Regex("^[a-zA-Z0-9_" + @"!\#\$%&'\*\+=\?\^`\{\|}~\ ,\\<>;/\.\-@""\[\]():" + "]*$");
            return loginRegex.IsMatch(login);
        }

        private static bool IsReservedLogin(string login)
        {
            return login == ServiceConstants.ExpiredUserKey ||
                   login == ServiceConstants.UserLogout ||
                   login == ServiceConstants.InvalidUserKey;
        }

        private static bool IsValidEmail(string email)
        {
            var emailRegex = new Regex(@"^([\w-\.\']+)@((\[[0-9]{1,3}\.[0-9]{1,3}\.[0-9]{1,3}\.)|(([\w-]+\.)+))([a-zA-Z]{2,4}|[0-9]{1,3})(\]?)$");
            return emailRegex.IsMatch(email);
        }
    }
}