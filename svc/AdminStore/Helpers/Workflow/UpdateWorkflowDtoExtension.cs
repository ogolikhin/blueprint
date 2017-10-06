using AdminStore.Models.Workflow;
using ServiceLibrary.Exceptions;
using ServiceLibrary.Helpers;

namespace AdminStore.Helpers.Workflow
{
    public static class UpdateWorkflowDtoExtension
    {
        public static void Validate(this UpdateWorkflowDto dto)
        {
            if (dto.Name == null || dto.Name.Length < 4 || dto.Name.Length > 24)
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