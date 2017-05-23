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
        public static void ValidateModel(GroupDto group)
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
                throw new BadRequestException(ErrorMessages.CreationOnlyDatabaseGroup, ErrorCodes.BadRequest);
            }

            if ((group.License != LicenseType.Collaborator && group.License != LicenseType.Author))
            {
                throw new BadRequestException(ErrorMessages.CreationGroupsOnlyWithCollaboratorAndAuthorLicenses, ErrorCodes.BadRequest);
            }
        }
    }
}