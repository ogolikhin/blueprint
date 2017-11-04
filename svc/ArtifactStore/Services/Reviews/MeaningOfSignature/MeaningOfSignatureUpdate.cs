using System;
using ArtifactStore.Models.Review;

namespace ArtifactStore.Services.Reviews.MeaningOfSignature
{
    public class MeaningOfSignatureUpdate
    {
        public bool Adding { get; set; }
        public ParticipantMeaningOfSignatureResult MeaningOfSignature { get; set; }
    }
}
