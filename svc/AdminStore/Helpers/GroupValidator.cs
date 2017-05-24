﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using System.Web;
using AdminStore.Models;
using AdminStore.Models.Enums;
using ServiceLibrary.Exceptions;
using ServiceLibrary.Helpers;

namespace AdminStore.Helpers
{
    public class GroupValidator
    {
        public static void ValidateModel(GroupDto group, OperationMode operationMode)
        {
            if (string.IsNullOrEmpty(group.Name))
            {
                throw new BadRequestException(ErrorMessages.GroupName, ErrorCodes.BadRequest);
            }

            if (group.Name.Length < 4 || group.Name.Length > 255)
            {
                throw new BadRequestException(ErrorMessages.GroupNameFieldLimitation, ErrorCodes.BadRequest);
            }

            if (!string.IsNullOrEmpty(group.Email))
            {
                if (group.Email.Length < 4 || group.Email.Length > 255)
                {
                    throw new BadRequestException(ErrorMessages.GroupEmailFieldLimitation, ErrorCodes.BadRequest);
                }

                var emailRegex =
                    new Regex(
                        @"^([\w-\.\']+)@((\[[0-9]{1,3}\.[0-9]{1,3}\.[0-9]{1,3}\.)|(([\w-]+\.)+))([a-zA-Z]{2,4}|[0-9]{1,3})(\]?)$");
                if (!emailRegex.IsMatch(group.Email))
                {
                    throw new BadRequestException(ErrorMessages.GroupEmailFormatIncorrect, ErrorCodes.BadRequest);
                }
            }

            if (group.GroupSource != UserGroupSource.Database)
            {
                if (operationMode == OperationMode.Create)
                {
                    throw new BadRequestException(ErrorMessages.CreationOnlyDatabaseGroup, ErrorCodes.BadRequest);
                }
                else
                {
                    throw new BadRequestException(ErrorMessages.SourceFieldValueForGroupsShouldBeOnlyDatabase, ErrorCodes.BadRequest);
                }
            }

            if (operationMode == OperationMode.Create)
            {
                if (group.ProjectId != null && (group.License != LicenseType.None))
                {
                    throw new BadRequestException(ErrorMessages.CreationGroupWithScopeAndLicenseIdSimultaneously, ErrorCodes.BadRequest);
                }

                if (group.ProjectId == null && (group.License != LicenseType.Collaborator && group.License != LicenseType.Author && group.License != LicenseType.None))
                {
                    throw new BadRequestException(ErrorMessages.CreationGroupsOnlyWithCollaboratorOrAuthorOrNoneLicenses, ErrorCodes.BadRequest);
                }
            }
            else
            {
                if (group.ProjectId != null)
                {
                    throw new BadRequestException(ErrorMessages.TheScopeCannotBeChanged, ErrorCodes.BadRequest);
                }

                if (group.License != LicenseType.Collaborator && group.License != LicenseType.Author && group.License != LicenseType.None)
                {
                    throw new BadRequestException(ErrorMessages.UpdateGroupsOnlyWithCollaboratorOrAuthorOrNoneLicenses, ErrorCodes.BadRequest);
                }
            }
        }
    }
}