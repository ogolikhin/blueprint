using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;

namespace ArtifactStore.Models.Review
{
    [DataContract(Namespace = "http://www.blueprintsys.com/raptor/reviews")]
    internal class ReviewPackageRawData : IExtensibleDataObject
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
        public ReviewPackageStatus Status;

        //Intentionally spelt wrong to be compatible with existing system
        [DataMember(Name = "Reviwers", EmitDefaultValue = false)]
        public List<ReviewerRawData> Reviewers;

        public ExtensionDataObject ExtensionData { get; set; }
    }

    [DataContract(Namespace = "http://www.blueprintsys.com/raptor/reviews")]
    internal class ReviewerRawData : IExtensibleDataObject
    {
        [DataMember]
        public ReviewParticipantRole Permission;

        [DataMember]
        public int UserId;

        public ExtensionDataObject ExtensionData { get; set; }
    }

    // If the structure of this class changes we need to update the xsd in the database in DataAnalytics.sql file too for performance reasons.
    // The xsd is created by using xsd.exe utility file from Microsoft. We extracted all related classes, built them and then ran xsd.exe on resulting dll.

    [DataContract(Namespace = "http://www.blueprintsys.com/raptor/reviews")]
    internal class RDReviewContents : IExtensibleDataObject
    {
        [DataMember(EmitDefaultValue = false)]
        public List<RDArtifact> Artifacts;

        public ExtensionDataObject ExtensionData
        {
            get;
            set;
        }
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

        public ExtensionDataObject ExtensionData
        {
            get;
            set;
        }
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

        public ExtensionDataObject ExtensionData
        {
            get;
            set;
        }
    }
}