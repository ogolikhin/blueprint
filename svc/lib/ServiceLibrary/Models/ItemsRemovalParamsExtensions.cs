using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ServiceLibrary.Exceptions;
using ServiceLibrary.Helpers;

namespace ServiceLibrary.Models
{
    public static class ItemsRemovalParamsExtensions
    {
        public static void Validate(this ItemsRemovalParams removalParams)
        {
            if (removalParams == null || (removalParams.ItemIds.IsEmpty() && removalParams.SelectionType == SelectionType.Selected))
            {
                throw new BadRequestException(
                    ErrorMessages.Collections.RemoveArtifactsInvalidParameters, ErrorCodes.BadRequest);
            }
        }
    }
}
