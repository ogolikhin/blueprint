using System.Collections.Generic;
using System.Linq;
using ArtifactStore.Models.Review;

namespace ArtifactStore.Services.Reviews.MeaningOfSignature
{
    public class MeaningOfSignatureUpdateSetDefaultsStrategy : IMeaningOfSignatureUpdateStrategy
    {
        public IEnumerable<MeaningOfSignatureUpdate> GetMeaningOfSignatureUpdates(int participantId,
                                                                                  Dictionary<int, List<ParticipantMeaningOfSignatureResult>> possibleMeaningOfSignatures,
                                                                                  IEnumerable<MeaningOfSignatureParameter> meaningOfSignatureParameters)
        {
            List<ParticipantMeaningOfSignatureResult> possibleMeaningOfSignaturesForParticipant;

            return possibleMeaningOfSignatures.TryGetValue(participantId, out possibleMeaningOfSignaturesForParticipant) ?
                possibleMeaningOfSignaturesForParticipant.Select(pmos => new MeaningOfSignatureUpdate()
                {
                    Adding = true,
                    MeaningOfSignature = pmos
                })
                : new MeaningOfSignatureUpdate[0];
        }
    }
}
