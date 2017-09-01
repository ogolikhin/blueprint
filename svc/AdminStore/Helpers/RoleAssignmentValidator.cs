using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using AdminStore.Models.Enums;
using ServiceLibrary.Exceptions;
using ServiceLibrary.Helpers;
using ServiceLibrary.Models;

namespace AdminStore.Helpers
{
    public class RoleAssignmentValidator
    {
        public static void ValidateModel(RoleAssignmentDTO roleAssignment, OperationMode operationMode = OperationMode.Create, int? roleAssignmentId = null)
        {
            if (roleAssignment.RoleId < 1)
            {
                throw new BadRequestException(ErrorMessages.RoleNameIsRequiredField, ErrorCodes.BadRequest);
            }

            if (roleAssignment.GroupId < 1)
            {
                throw new BadRequestException(ErrorMessages.GroupIsRequiredField, ErrorCodes.BadRequest);
            }

            if (operationMode == OperationMode.Edit)
            {
                if (roleAssignmentId.HasValue && roleAssignmentId < 1)
                {
                    throw new BadRequestException(ErrorMessages.RoleAssignmentNotFound, ErrorCodes.BadRequest);
                }
            }
        }

    }
}