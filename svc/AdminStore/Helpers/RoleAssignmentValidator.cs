using ServiceLibrary.Exceptions;
using ServiceLibrary.Helpers;
using ServiceLibrary.Models;

namespace AdminStore.Helpers
{
    public class RoleAssignmentValidator
    {
        public static void ValidateModel(CreateRoleAssignment roleAssignment)
        {
            if (roleAssignment.RoleId < 1)
            {
                throw new BadRequestException(ErrorMessages.RoleNameIsRequiredField, ErrorCodes.BadRequest);
            }

            if (roleAssignment.GroupId < 1)
            {
                throw new BadRequestException(ErrorMessages.GroupIsRequiredField, ErrorCodes.BadRequest);
            }
        }
    }
}