﻿using AdminStore.Models.DTO;
using ServiceLibrary.Exceptions;
using ServiceLibrary.Helpers;
using ServiceLibrary.Models.Workflow;

namespace AdminStore.Helpers.Workflow
{
    public static class UpdateWorkflowDtoExtension
    {
        public static void Validate(this UpdateWorkflowDto dto)
        {
            if (dto.Name == null || dto.Name.Length < WorkflowConstants.MinNameLength || dto.Name.Length > WorkflowConstants.MaxNameLength)
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
