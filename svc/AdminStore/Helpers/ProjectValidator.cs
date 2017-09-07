using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using AdminStore.Models.DTO;
using ServiceLibrary.Exceptions;
using ServiceLibrary.Helpers;

namespace AdminStore.Helpers
{
    public class ProjectValidator
    {
        public const int MinNameLength = 1;
        public const int MaxNameLength = 128;

        public static void ValidateModel(ProjectDto projectDto)
        {
            if (string.IsNullOrWhiteSpace(projectDto.Name))
            {
                throw new BadRequestException(ErrorMessages.ProjectNameLimitation, ErrorCodes.BadRequest);
            }

            projectDto.Name = projectDto.Name.Trim();

            if (projectDto.Name.Length < MinNameLength || projectDto.Name.Length > MaxNameLength)
            {
                throw new BadRequestException(ErrorMessages.ProjectNameLimitation, ErrorCodes.BadRequest);
            }

            if (projectDto.ParentFolderId < 1)
            {
                throw new BadRequestException(ErrorMessages.LocationIsRequired, ErrorCodes.BadRequest);
            }

        }
    }
}