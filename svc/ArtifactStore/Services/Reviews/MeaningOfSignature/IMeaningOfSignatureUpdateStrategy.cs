using System;
using System.Collections.Generic;
using ArtifactStore.Models.Review;

namespace ArtifactStore.Services.Reviews.MeaningOfSignature
{
    public interface IMeaningOfSignatureUpdateStrategy
    {
        bool ClearMeaningOfSignaturesIfNoUpdates { get; }

        IEnumerable<MeaningOfSignatureUpdate> GetMeaningOfSignatureUpdates(int participantId,
                                                                           Dictionary<int, List<ParticipantMeaningOfSignatureResult>> possibleMeaningOfSignatures,
                                                                           IEnumerable<MeaningOfSignatureParameter> meaningOfSignatureParameters);
    }
}
