using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ArtifactStore.Models.Review;
using ServiceLibrary.Exceptions;
using ServiceLibrary.Helpers;

namespace ArtifactStore.Services.Reviews.MeaningOfSignature
{
    public class MeaningOfSignatureUpdateSpecificStrategy : IMeaningOfSignatureUpdateStrategy
    {
        public IEnumerable<MeaningOfSignatureUpdate> GetMeaningOfSignatureUpdates(int participantId,
                                                                           Dictionary<int, List<ParticipantMeaningOfSignatureResult>> possibleMeaningOfSignatures,
                                                                           IEnumerable<MeaningOfSignatureParameter> meaningOfSignatureParameters)
        {
            var updates = new List<MeaningOfSignatureUpdate>();

            foreach (var meaningOfSignatureParameter in meaningOfSignatureParameters.Where(mosp => mosp.ParticipantId == participantId))
            {
                var meaningOfSignature = possibleMeaningOfSignatures[participantId].FirstOrDefault(mos => mos.RoleAssignmentId == meaningOfSignatureParameter.RoleAssignmentId);

                if (meaningOfSignature == null)
                {
                    throw new ConflictException("Could not update meaning of signature because meaning of signature is not possible for a participant.", ErrorCodes.MeaningOfSignatureNotPossible);
                }

                updates.Add(new MeaningOfSignatureUpdate()
                {
                    Adding = meaningOfSignatureParameter.Adding,
                    MeaningOfSignature = meaningOfSignature
                });
            }

            return updates;
        }
    }
}
