using AdminStore.Models.DTO;
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

            if (!folder.ParentFolderId.HasValue)
            {
                throw new BadRequestException(ErrorMessages.LocationIsRequired, ErrorCodes.BadRequest);
            }

            if (folder.Id == folder.ParentFolderId.Value)
            {
                throw new BadRequestException(ErrorMessages.FolderReferenceToItself, ErrorCodes.BadRequest);
            }
        }
    }
}
