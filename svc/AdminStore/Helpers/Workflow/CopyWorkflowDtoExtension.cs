using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using AdminStore.Models.DTO;
using ServiceLibrary.Exceptions;
using ServiceLibrary.Helpers;

namespace AdminStore.Helpers.Workflow
{
    public static class CopyWorkflowDtoExtension
    {
        public static void Validate(this CopyWorkfloDto dto)
        {
            if (dto.Name == null || dto.Name.Length < 4 || dto.Name.Length > 24)
            {
                throw new BadRequestException(ErrorMessages.WorkflowNameError, ErrorCodes.BadRequest);
            }
        }
    }
}