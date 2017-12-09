using AdminStore.Models.DTO;
using ServiceLibrary.Exceptions;
using ServiceLibrary.Helpers;
using ServiceLibrary.Models.Workflow;

namespace AdminStore.Helpers.Workflow
{
    public static class CopyWorkflowDtoExtension
    {
        public static void Validate(this CopyWorkflowDto dto)
        {
            if (dto.Name == null || dto.Name.Length < WorkflowConstants.MinNameLength || dto.Name.Length > WorkflowConstants.MaxNameLength)
            {
                throw new BadRequestException(ErrorMessages.WorkflowNameError, ErrorCodes.BadRequest);
            }
        }
    }
}