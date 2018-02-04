﻿using AdminStore.Models;
using AdminStore.Models.Enums;
using ServiceLibrary.Exceptions;
using ServiceLibrary.Helpers;

namespace AdminStore.Helpers
{
    public class GroupValidator
    {
        private const int MinNameLength = 1;
        private const int MaxNameLength = 255;
        private const int MinEmailLength = 4;
        private const int MaxEmailLength = 255;

        public static void ValidateModel(GroupDto group, OperationMode operationMode, int? existingGroupProjectId = null)
        {
            if (string.IsNullOrWhiteSpace(group.Name))
            {
                throw new BadRequestException(ErrorMessages.GroupNameRequired, ErrorCodes.BadRequest);
            }

            group.Name = group.Name.Trim();

            if (group.Name.Length < MinNameLength || group.Name.Length > MaxNameLength)
            {
                throw new BadRequestException(ErrorMessages.GroupNameFieldLimitation, ErrorCodes.BadRequest);
            }

            if (!string.IsNullOrEmpty(group.Email))
            {
                group.Email = group.Email.Trim();

                if (group.Email.Length < MinEmailLength || group.Email.Length > MaxEmailLength)
                {
                    throw new BadRequestException(ErrorMessages.EmailFieldLimitation, ErrorCodes.BadRequest);
                }

                if (!UserManagementHelper.IsValidEmail(group.Email))
                {
                    throw new BadRequestException(ErrorMessages.GroupEmailFormatIncorrect, ErrorCodes.BadRequest);
                }
            }

            if (group.Source != UserGroupSource.Database)
            {
                if (operationMode == OperationMode.Create)
                {
                    throw new BadRequestException(ErrorMessages.CreationOnlyDatabaseGroup, ErrorCodes.BadRequest);
                }

                throw new BadRequestException(ErrorMessages.SourceFieldValueForGroupsShouldBeOnlyDatabase, ErrorCodes.BadRequest);
            }

            if (operationMode == OperationMode.Create)
            {
                if (group.ProjectId != null && group.ProjectId <= 0)
                {
                    throw new BadRequestException(ErrorMessages.ProjectIdIsInvalid, ErrorCodes.BadRequest);
                }

                if (group.ProjectId != null && (group.LicenseType != LicenseType.None))
                {
                    throw new BadRequestException(ErrorMessages.CreationGroupWithScopeAndLicenseIdSimultaneously, ErrorCodes.BadRequest);
                }

                if (group.ProjectId == null && (group.LicenseType != LicenseType.Collaborator && group.LicenseType != LicenseType.Author && group.LicenseType != LicenseType.None))
                {
                    throw new BadRequestException(ErrorMessages.CreationGroupsOnlyWithCollaboratorOrAuthorOrNoneLicenses, ErrorCodes.BadRequest);
                }
            }
            else
            {
                if (group.ProjectId != existingGroupProjectId)
                {
                    throw new BadRequestException(ErrorMessages.TheScopeCannotBeChanged, ErrorCodes.BadRequest);
                }

                if (group.LicenseType != LicenseType.Collaborator && group.LicenseType != LicenseType.Author && group.LicenseType != LicenseType.None)
                {
                    throw new BadRequestException(ErrorMessages.UpdateGroupsOnlyWithCollaboratorOrAuthorOrNoneLicenses, ErrorCodes.BadRequest);
                }
            }
        }
    }
}