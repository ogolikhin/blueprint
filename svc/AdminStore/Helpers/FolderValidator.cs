using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using AdminStore.Models;
using AdminStore.Models.DTO;
using AdminStore.Models.Enums;
using ServiceLibrary.Exceptions;
using ServiceLibrary.Helpers;

namespace AdminStore.Helpers
{
    public class FolderValidator
    {
        public const int MinNameLength = 128;

        public static void ValidateModel(FolderDto folder)
        {
            if (string.IsNullOrWhiteSpace(folder.Name))
            {
                throw new BadRequestException(ErrorMessages.FolderNameLimitation, ErrorCodes.BadRequest);
            }

            folder.Name = folder.Name.Trim();

            if (folder.Name.Length > MinNameLength)
            {
                throw new BadRequestException(ErrorMessages.FolderNameLimitation, ErrorCodes.BadRequest);
            }

            if (folder.ParentFolderId.HasValue && folder.Id == folder.ParentFolderId.Value)
            {
                throw new BadRequestException(ErrorMessages.FolderReferenceToItself, ErrorCodes.BadRequest);
            }
        }
    }
}