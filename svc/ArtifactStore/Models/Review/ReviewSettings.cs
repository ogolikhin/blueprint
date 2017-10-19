using System;

namespace ArtifactStore.Models.Review
{
    public class ReviewSettings
    {
        public DateTime? EndDate { get; set; }

        public bool ShowOnlyDescription { get; set; }

        public bool CanMarkAsComplete { get; set; }

        public bool RequireESignature { get; set; }

        public bool RequireMeaningOfSignature { get; set; }
    }
}
