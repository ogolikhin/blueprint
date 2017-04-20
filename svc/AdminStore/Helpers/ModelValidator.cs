﻿using System;
using AdminStore.Models;
using ServiceLibrary.Exceptions;
using ServiceLibrary.Helpers;

namespace AdminStore.Helpers
{
    public class ModelValidator
    {
        public static void ValidateUserModel(CreationUserDto user)
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

        public static void ValidateUserPassword(string password)
        {           
            string errorMessage;
            var isValidPassword = PasswordValidationHelper.ValidatePassword(password, true, out errorMessage);
            if (!isValidPassword)
            {
                throw new BadRequestException(errorMessage, ErrorCodes.BadRequest);
            }
        }
    }
}
