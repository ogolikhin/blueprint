using System;
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
        public const int MinEmailLength = 4;
        public const int MaxEmailLength = 255;

        public static void ValidateModel(GroupDto group, OperationMode operationMode)
        {
            if (string.IsNullOrWhiteSpace(group.Name))
            {
                throw new BadRequestException(ErrorMessages.GroupName, ErrorCodes.BadRequest);
            }

            group.Name = group.Name.Trim();

            if (!string.IsNullOrEmpty(group.Email))
            {
                group.Email = group.Email.Trim();

                if (group.Email.Length < MinEmailLength || group.Email.Length > MaxEmailLength)
                {
                    throw new BadRequestException(ErrorMessages.EmailFieldLimitation, ErrorCodes.BadRequest);
                }

                var emailRegex =
                    new Regex(
                        @"^([\w-\.\']+)@((\[[0-9]{1,3}\.[0-9]{1,3}\.[0-9]{1,3}\.)|(([\w-]+\.)+))([a-zA-Z]{2,4}|[0-9]{1,3})(\]?)$");
                if (!emailRegex.IsMatch(group.Email))
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
                else
                {
                    throw new BadRequestException(ErrorMessages.SourceFieldValueForGroupsShouldBeOnlyDatabase, ErrorCodes.BadRequest);
                }
            }

            if (operationMode == OperationMode.Create)
            {
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
                if (group.ProjectId != null)
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