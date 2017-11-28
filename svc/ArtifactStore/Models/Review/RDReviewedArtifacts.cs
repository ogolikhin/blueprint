using System;
using System.Collections.Generic;
using System.Runtime.Serialization;
using ServiceLibrary.Models.ProjectMeta;

namespace ArtifactStore.Models.Review
{
    [DataContract(Namespace = "http://www.blueprintsys.com/raptor/reviews")]
    internal class RDReviewedArtifacts
    {
        [DataMember(Name = "SCMOSV", EmitDefaultValue = false)]
        public List<SelectedMeaningOfSignatureXml> SelectedCompletionMeaningOfSignatureValues { get; set; }

        [DataMember]
        public List<ReviewArtifactXml> ReviewedArtifacts { get; set; }
    }

    [DataContract(Namespace = "http://www.blueprintsys.com/raptor/reviews", Name = "RA")]
    internal class ReviewArtifactXml
    {
        [DataMember(Name = "A", EmitDefaultValue = false)]
        public string Approval { get; set; }

        [DataMember(Name = "AF", EmitDefaultValue = false)]
        public ApprovalType ApprovalFlag { get; set; }

        [DataMember(Name = "Id", EmitDefaultValue = false)]
        public int ArtifactId { get; set; }

        [DataMember(Name = "V", EmitDefaultValue = false)]
        public int ArtifactVersion { get; set; }

        [DataMember(Name = "VS", EmitDefaultValue = false)]
        public ViewStateType ViewState { get; set; }

        [DataMember(Name = "EO", EmitDefaultValue = false)]
        public DateTime? ESignedOn { get; set; }

        [DataMember(Name = "SMSFE", EmitDefaultValue = false)]
        public List<SelectedMeaningOfSignatureXml> SelectedMeaningofSignatureValues { get; set; }
    }

    [DataContract(Namespace = "Blueprint.Reviews", Name = "RESMI")]
    internal class SelectedMeaningOfSignatureXml
    {
        [DataMember(Name = "MOSRID")]
        public int? RoleId { get; set; }

        [DataMember(Name = "MOSRN")]
        public string RoleName { get; set; }

        [DataMember(Name = "MoSEID")]
        public int? MeaningOfSignatureId { get; set; }

        [DataMember(Name = "MOSEV")]
        public string MeaningOfSignatureValue { get; set; }
    }
}
