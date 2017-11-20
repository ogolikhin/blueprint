using System;
using System.Collections.Generic;
using System.Runtime.Serialization;
using ServiceLibrary.Models.ProjectMeta;

namespace ArtifactStore.Models.Review
{
    [DataContract(Namespace = "http://www.blueprintsys.com/raptor/reviews")]
    internal class RDReviewedArtifacts
    {
        [DataMember]
        public List<ReviewArtifactXml> ReviewedArtifacts;
    }

    [DataContract(Namespace = "http://www.blueprintsys.com/raptor/reviews", Name = "RA")]
    internal class ReviewArtifactXml
    {
        [DataMember(Name = "A", EmitDefaultValue = false)]
        public string Approval;

        [DataMember(Name = "AF", EmitDefaultValue = false)]
        public ApprovalType ApprovalFlag;

        [DataMember(Name = "Id", EmitDefaultValue = false)]
        public int ArtifactId;

        [DataMember(Name = "V", EmitDefaultValue = false)]
        public int ArtifactVersion;

        [DataMember(Name = "VS", EmitDefaultValue = false)]
        public ViewStateType ViewState;

        [DataMember(Name = "EO", EmitDefaultValue = false)]
        public DateTime? ESignedOn;

        [DataMember(Name = "SMS", EmitDefaultValue = false)]
        public List<SelectedMeaningOfSignatureValue> SelectedMeaningofSignatureValues { get; set; }
    }

    [DataContract(Namespace = "Blueprint.Reviews", Name = "RESMI")]
    internal class SelectedMeaningOfSignatureValue
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
