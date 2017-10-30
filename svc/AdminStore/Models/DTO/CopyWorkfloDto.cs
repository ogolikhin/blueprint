using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using ServiceLibrary.Exceptions;
using ServiceLibrary.Helpers;

namespace AdminStore.Models.DTO
{
    public class CopyWorkfloDto
    {
        public string Name { get; set; }

        public void Validate()
        {
            if (Name == null || Name.Length < 4 || Name.Length > 24)
            {
                throw new BadRequestException(ErrorMessages.WorkflowNameError, ErrorCodes.BadRequest);
            }
        }
    }
}