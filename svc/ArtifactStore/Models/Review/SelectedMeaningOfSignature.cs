using ArtifactStore.Helpers;

namespace ArtifactStore.Models.Review
{
    public class SelectedMeaningOfSignature
    {
        public string Label { get; set; }
        public int RoleId { get; set; }
        public int MeaningOfSignatureId { get; set; }

        public SelectedMeaningOfSignature()
        {
        }

        public SelectedMeaningOfSignature(ParticipantMeaningOfSignatureResult mos)
        {
            Label = mos.GetMeaningOfSignatureDisplayValue();
            RoleId = mos.RoleId;
            MeaningOfSignatureId = mos.MeaningOfSignatureId;
        }
    }
}
