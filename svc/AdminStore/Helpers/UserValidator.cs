using System.Text.RegularExpressions;
using AdminStore.Models;
using AdminStore.Models.Enums;
using ServiceLibrary.Exceptions;
using ServiceLibrary.Helpers;

namespace AdminStore.Helpers
{
    public class UserValidator
    {
        public static void ValidateModel(UserDto user, OperationMode operationMode)
        {
            if (string.IsNullOrWhiteSpace(user.Login))
            {
                throw new BadRequestException(ErrorMessages.LoginRequired, ErrorCodes.BadRequest);
            }

            if (user.Login.Length < 4 || user.Login.Length > 255)
            {
                throw new BadRequestException(ErrorMessages.LoginFieldLimitation, ErrorCodes.BadRequest);
            }

            if (IsReservedUserName(user.Login))
            {
                throw new BadRequestException(ErrorMessages.LoginInvalid, ErrorCodes.BadRequest);
            }

            if (string.IsNullOrWhiteSpace(user.DisplayName))
            {
                throw new BadRequestException(ErrorMessages.DisplayNameRequired, ErrorCodes.BadRequest);
            }

            if (user.DisplayName.Length < 2 || user.DisplayName.Length > 255)
            {
                throw new BadRequestException(ErrorMessages.DisplayNameFieldLimitation, ErrorCodes.BadRequest);
            }

            if ((!string.IsNullOrWhiteSpace(user.FirstName)) && (user.FirstName.Length < 2 || user.FirstName.Length > 255))
            {
                throw new BadRequestException(ErrorMessages.FirstNameFieldLimitation, ErrorCodes.BadRequest);
            }

            if (!string.IsNullOrWhiteSpace(user.LastName) && (user.LastName.Length < 2 || user.LastName.Length > 255))
            {
                throw new BadRequestException(ErrorMessages.LastNameFieldLimitation, ErrorCodes.BadRequest);
            }

            if (!string.IsNullOrWhiteSpace(user.Email))
            {
                if ((user.Email.Length < 4 || user.Email.Length > 255))
                {
                    throw new BadRequestException(ErrorMessages.EmailFieldLimitation, ErrorCodes.BadRequest);
                }

                var emailRegex = new Regex(@"^([\w-\.\']+)@((\[[0-9]{1,3}\.[0-9]{1,3}\.[0-9]{1,3}\.)|(([\w-]+\.)+))([a-zA-Z]{2,4}|[0-9]{1,3})(\]?)$");
                if (!emailRegex.IsMatch(user.Email))
                {
                    throw new BadRequestException(ErrorMessages.EmailFormatIncorrect, ErrorCodes.BadRequest);
                }
            }

            if (!string.IsNullOrWhiteSpace(user.Title) && (user.Title.Length < 2 || user.Title.Length > 255))
            {
                throw new BadRequestException(ErrorMessages.TitleFieldLimitation, ErrorCodes.BadRequest);
            }

            if (!string.IsNullOrWhiteSpace(user.Department) && (user.Department.Length < 1 || user.Department.Length > 255))
            {
                throw new BadRequestException(ErrorMessages.DepartmentFieldLimitation, ErrorCodes.BadRequest);
            }

            if (user.Source != UserGroupSource.Database)
            {
                if (operationMode == OperationMode.Create)
                {
                    throw new BadRequestException(ErrorMessages.CreationOnlyDatabaseUsers, ErrorCodes.BadRequest);
                }
                else
                {
                    throw new BadRequestException(ErrorMessages.SourceFieldValueShouldBeOnlyDatabase, ErrorCodes.BadRequest);
                }
            }
        }

        private static bool IsReservedUserName(string userName)
        {
            return userName == ServiceConstants.ExpiredUserKey ||
                   userName == ServiceConstants.UserLogout ||
                   userName == ServiceConstants.InvalidUserKey;
        }
    }
}