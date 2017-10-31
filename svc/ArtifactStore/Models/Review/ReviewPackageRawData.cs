using System;
using System.Collections.Generic;
using System.Runtime.Serialization;

namespace ArtifactStore.Models.Review
{
    [DataContract(Namespace = "http://www.blueprintsys.com/raptor/reviews")]
    public class ReviewPackageRawData : IExtensibleDataObject
    {
        [DataMember(EmitDefaultValue = false)]
        public DateTime? EndDate;

        [DataMember(EmitDefaultValue = false)]
        public bool IsAllowToMarkReviewAsCompleteWhenAllArtifactsReviewed;

        [DataMember(EmitDefaultValue = false)]
        public bool IsBaselineFollowUpReview;

        [DataMember(EmitDefaultValue = false)]
        public bool IsESignatureEnabled;

        [DataMember(EmitDefaultValue = false)]
        public bool IsFollowUpReview;

        [DataMember(EmitDefaultValue = false)]
        public bool IsIgnoreFolder;

        [DataMember(EmitDefaultValue = false)]
        public bool IsMoSEnabled;

        [DataMember(EmitDefaultValue = false)]
        public bool ShowOnlyDescription;

        [DataMember(EmitDefaultValue = false)]
        public ReviewPackageStatus Status;

        // Intentionally spelt wrong to be compatible with existing system
        [DataMember(Name = "Reviwers", EmitDefaultValue = false)]
        public List<ReviewerRawData> Reviewers;

        public ExtensionDataObject ExtensionData { get; set; }
    }

    [DataContract(Namespace = "http://www.blueprintsys.com/raptor/reviews")]
    public class ReviewerRawData : IExtensibleDataObject
    {
        [DataMember]
        public ReviewParticipantRole Permission;

        [DataMember]
        public int UserId;

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        [DataMember(EmitDefaultValue = false)]
        public List<ParticipantMeaningOfSignature> SelectedRoleMoSAssignments { get; set; }

        public ExtensionDataObject ExtensionData { get; set; }
    }

    [DataContract(Name = "MoS", Namespace = "http://schemas.datacontract.org/2004/07/BluePrintSys.RC.Service.Business.Reviews")]
    public class ParticipantMeaningOfSignature : IExtensibleDataObject
    {
        [DataMember(Name = "MoSUIT")]
        public int GroupId { get; set; }

        [DataMember(Name = "MoSURI")]
        public int ParticipantId { get; set; }

        [DataMember(Name = "MoSP")]
        public int ReviewId { get; set; }

        [DataMember(Name = "MoSI")]
        public int RoleAssignmentId { get; set; }

        [DataMember(Name = "MoSRI")]
        public int RoleId { get; set; }

        [DataMember(Name = "MoSRN")]
        public string RoleName { get; set; }

        [DataMember(Name = "MoSE")]
        public int MeaningOfSignatureId { get; set; }

        [DataMember(Name = "MoSEV")]
        public string MeaningOfSignatureValue { get; set; }


        public ExtensionDataObject ExtensionData { get; set; }
    }

    // If the structure of this class changes we need to update the xsd in the database in DataAnalytics.sql file too for performance reasons.
    // The xsd is created by using xsd.exe utility file from Microsoft. We extracted all related classes, built them and then ran xsd.exe on resulting dll.

    [DataContract(Namespace = "http://www.blueprintsys.com/raptor/reviews")]
    internal class RDReviewContents : IExtensibleDataObject
    {
        [DataMember(EmitDefaultValue = false)]
        public List<RDArtifact> Artifacts;

        public ExtensionDataObject ExtensionData { get; set; }
    }

    [DataContract(Name = "CA", Namespace = "http://www.blueprintsys.com/raptor/reviews")]
    internal class RDArtifact : IExtensibleDataObject
    {
        [DataMember]
        public int Id;

        [DataMember(Name = "ANR", EmitDefaultValue = false)]
        public bool ApprovalNotRequested;

        [DataMember(EmitDefaultValue = false)]
        public SimulationSettingsRawData Settings;

        public ExtensionDataObject ExtensionData { get; set; }
    }

    [DataContract(Namespace = "http://www.blueprintsys.com/raptor/reviews")]
    internal class SimulationSettingsRawData : IExtensibleDataObject
    {
        [DataMember(EmitDefaultValue = false)]
        public bool SelectedUseCaseOnly;

        [DataMember(EmitDefaultValue = false)]
        public int UseCaseLevel;

        [DataMember(EmitDefaultValue = false)]
        public bool UseAutomaticNavigation;

        [DataMember(EmitDefaultValue = false)]
        public int AutomaticNavigationInterval;

        [DataMember(EmitDefaultValue = false)]
        public ActivityDiagramFlags ActivityDiagram;

        [DataMember(EmitDefaultValue = false)]
        public DisplayFlags Display;

        [DataMember(EmitDefaultValue = false)]
        public string MainContentTab;

        public ExtensionDataObject ExtensionData { get; set; }
    }
}
