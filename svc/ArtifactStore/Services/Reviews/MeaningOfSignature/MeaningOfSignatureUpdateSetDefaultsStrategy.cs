using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ArtifactStore.Models.Review;

namespace ArtifactStore.Services.Reviews.MeaningOfSignature
{
    public class MeaningOfSignatureUpdateSetDefaultsStrategy : IMeaningOfSignatureUpdateStrategy
    {
        public IEnumerable<MeaningOfSignatureUpdate> GetMeaningOfSignatureUpdates(int participantId,
                                                                                  Dictionary<int, List<ParticipantMeaningOfSignatureResult>> possibleMeaningOfSignatures,
                                                                                  IEnumerable<MeaningOfSignatureParameter> meaningOfSignatureParameters)
        {
            return possibleMeaningOfSignatures[participantId].Select(pmos => new MeaningOfSignatureUpdate()
            {
                Adding = true,
                MeaningOfSignature = pmos
            });
        }
    }
}
