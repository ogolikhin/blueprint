using AdminStore.Models.DTO;
using ServiceLibrary.Exceptions;
using ServiceLibrary.Helpers;

namespace AdminStore.Helpers.Workflow
{
    public static class CreateWorkflowDtoExtension
    {
        public static void Validate(this CreateWorkflowDto dto)
        {
            if (dto == null)
            {
                throw new BadRequestException(ErrorMessages.CreateWorkfloModelIsEmpty, ErrorCodes.BadRequest);
            }
            if (dto.Name == null || dto.Name.Length < 4 || dto.Name.Length > 64)
            {
                throw new BadRequestException(ErrorMessages.WorkflowNameError, ErrorCodes.BadRequest);
            }
            if (dto.Description != null && dto.Description.Length > 400)
            {
                throw new BadRequestException(ErrorMessages.WorkflowDescriptionLimit, ErrorCodes.BadRequest);
            }
        }
    }
}