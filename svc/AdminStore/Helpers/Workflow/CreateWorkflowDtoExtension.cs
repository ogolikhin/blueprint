using AdminStore.Models.DTO;
using ServiceLibrary.Exceptions;
using ServiceLibrary.Helpers;
using ServiceLibrary.Models.Workflow;

namespace AdminStore.Helpers.Workflow
{
    public static class CreateWorkflowDtoExtension
    {
        public static void Validate(this CreateWorkflowDto dto)
        {
            if (dto.Name == null || dto.Name.Length < 4 || dto.Name.Length > 24)
            {
                throw new BadRequestException(ErrorMessages.WorkflowNameError, ErrorCodes.BadRequest);
            }
            if (dto.Description != null && dto.Description.Length > WorkflowConstants.MaxDescriptionLength)
            {
                throw new BadRequestException(ErrorMessages.WorkflowDescriptionLimit, ErrorCodes.BadRequest);
            }
        }
    }
}