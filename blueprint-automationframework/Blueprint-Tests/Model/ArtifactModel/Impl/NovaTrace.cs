using Model.ArtifactModel.Enums;
using Newtonsoft.Json;
using NUnit.Framework;
using System;
using System.Collections.Generic;
using Utilities;

namespace Model.ArtifactModel.Impl
{
    // Taken from:  blueprint-current/Source/BluePrintSys.RC.Business.Internal/Components/Nova/Models/NovaTrace.cs
    public class NovaTrace : ITrace, INovaTrace
    {
        #region Serialized JSON properties

        public int ArtifactId { get; set; }
        public string ArtifactTypePrefix { get; set; }
        public string ArtifactName { get; set; }
        public int ItemId { get; set; }
        public string ItemTypePrefix { get; set; }
        public string ItemName { get; set; }
        public string ItemLabel { get; set; }
        public int ProjectId { get; set; }
        public string ProjectName { get; set; }

        [JsonProperty("TraceDirection")]
        public TraceDirection Direction { get; set; }
        public TraceType TraceType { get; set; }

        [JsonProperty("Suspect")]
        public bool IsSuspect { get; set; }
        public bool HasAccess { get; set; }
        public int PrimitiveItemTypePredefined { get; set; }
        public ChangeType? ChangeType { get; set; }

        #endregion Serialized JSON properties

        #region Constructors

        public NovaTrace()
        { }

        /// <summary>
        /// Constructs a new NovaTrace.
        /// </summary>
        /// <param name="targetArtifact">The target artifact of the trace.</param>
        /// <param name="targetSubArtifactId">(optional) </param>
        /// <param name="direction">(optional) The direction of the trace.</param>
        /// <param name="isSuspect">(optional) Pass true to mark the trace as suspect.</param>
        /// <param name="changeType">(optional) The change being performed (ex. creating, updating or deleting a trace).</param>
        public NovaTrace(
            INovaArtifactDetails targetArtifact,
            int? targetSubArtifactId = null,
            TraceDirection direction = TraceDirection.From,
            bool isSuspect = false,
            ChangeType changeType = Enums.ChangeType.Create)
        {
            ThrowIf.ArgumentNull(targetArtifact, nameof(targetArtifact));

            ArtifactId = targetArtifact.Id;
            ChangeType = changeType;
            Direction = direction;
            IsSuspect = isSuspect;
            ItemId = targetSubArtifactId ?? targetArtifact.Id;
            ProjectId = targetArtifact.ProjectId.Value;
            TraceType = TraceType.Manual;
        }

        /// <summary>
        /// Constructs a new NovaTrace.
        /// </summary>
        /// <param name="targetArtifact">The target artifact of the trace.</param>
        /// <param name="targetSubArtifactId">(optional) </param>
        /// <param name="direction">(optional) The direction of the trace.</param>
        /// <param name="isSuspect">(optional) Pass true to mark the trace as suspect.</param>
        /// <param name="changeType">(optional) The change being performed (ex. creating, updating or deleting a trace).</param>
        public NovaTrace(
            IArtifactBase targetArtifact,
            int? targetSubArtifactId = null,
            TraceDirection direction = TraceDirection.From,
            bool isSuspect = false,
            ChangeType changeType = Enums.ChangeType.Create)
        {
            ThrowIf.ArgumentNull(targetArtifact, nameof(targetArtifact));

            ArtifactId = targetArtifact.Id;
            ChangeType = changeType;
            Direction = direction;
            IsSuspect = isSuspect;
            ItemId = targetSubArtifactId ?? targetArtifact.Id;
            ProjectId = targetArtifact.ProjectId;
            TraceType = TraceType.Manual;
        }

        #endregion Constructors

        /// <summary>
        /// Asserts that two NovaTraces are equal.
        /// </summary>
        /// <param name="expectedTrace">The expected NovaTrace.</param>
        /// <param name="actualTrace">The actual NovaTrace.</param>
        public static void AssertTracesAreEqual(NovaTrace expectedTrace, NovaTrace actualTrace)
        {
            ThrowIf.ArgumentNull(expectedTrace, nameof(expectedTrace));
            ThrowIf.ArgumentNull(actualTrace, nameof(actualTrace));

            Assert.AreEqual(expectedTrace.ArtifactId, actualTrace.ArtifactId, "The ArtifactId properties don't match!");
            Assert.AreEqual(expectedTrace.ArtifactTypePrefix, actualTrace.ArtifactTypePrefix, "The ArtifactTypePrefix properties don't match!");
            Assert.AreEqual(expectedTrace.ArtifactName, actualTrace.ArtifactName, "The ArtifactName properties don't match!");
            Assert.AreEqual(expectedTrace.ItemId, actualTrace.ItemId, "The ItemId properties don't match!");
            Assert.AreEqual(expectedTrace.ItemTypePrefix, actualTrace.ItemTypePrefix, "The ItemTypePrefix properties don't match!");
            Assert.AreEqual(expectedTrace.ItemName, actualTrace.ItemName, "The ItemName properties don't match!");
            Assert.AreEqual(expectedTrace.ItemLabel, actualTrace.ItemLabel, "The ItemLabel properties don't match!");
            Assert.AreEqual(expectedTrace.ProjectId, actualTrace.ProjectId, "The ProjectId properties don't match!");
            Assert.AreEqual(expectedTrace.ProjectName, actualTrace.ProjectName, "The ProjectName properties don't match!");
            Assert.AreEqual(expectedTrace.Direction, actualTrace.Direction, "The Direction properties don't match!");
            Assert.AreEqual(expectedTrace.TraceType, actualTrace.TraceType, "The TraceType properties don't match!");
            Assert.AreEqual(expectedTrace.IsSuspect, actualTrace.IsSuspect, "The IsSuspect properties don't match!");
            Assert.AreEqual(expectedTrace.HasAccess, actualTrace.HasAccess, "The HasAccess properties don't match!");
            Assert.AreEqual(expectedTrace.PrimitiveItemTypePredefined, actualTrace.PrimitiveItemTypePredefined, "The PrimitiveItemTypePredefined properties don't match!");
            Assert.AreEqual(expectedTrace.ChangeType, actualTrace.ChangeType, "The ChangeType properties don't match!");
        }
    }

    // Found in:  blueprint/svc/ArtifactStore/Models/RelationshipResultSet.cs
    public class Relationships
    {
        #region Serialized JSON properties

        public List<Relationship> ManualTraces { get; } = new List<Relationship>();
        public List<Relationship> OtherTraces { get; } = new List<Relationship>();
        public bool CanEdit { get; set; }
        public int RevisionId { get; set; }

        #endregion Serialized JSON properties

        /// <summary>
        /// Asserts that the properties of the two Relationships objects are identical.
        /// </summary>
        /// <param name="expectedRelationships">The expected Relationships.</param>
        /// <param name="actualRelationships">The actual Relationships.</param>
        /// <exception cref="AssertionException">If any properties don't match.</exception>
        public static void AssertRelationshipsAreEqual(Relationships expectedRelationships, Relationships actualRelationships)
        {
            ThrowIf.ArgumentNull(expectedRelationships, nameof(expectedRelationships));
            ThrowIf.ArgumentNull(actualRelationships, nameof(actualRelationships));

            Assert.AreEqual(expectedRelationships.ManualTraces.Count, actualRelationships.ManualTraces.Count, "The number of ManualTraces don't match!");
            Assert.AreEqual(expectedRelationships.OtherTraces.Count, actualRelationships.OtherTraces.Count, "The number of OtherTraces don't match!");

            Assert.AreEqual(expectedRelationships.CanEdit, actualRelationships.CanEdit, "The CanEdit properties don't match!");
            Assert.AreEqual(expectedRelationships.RevisionId, actualRelationships.RevisionId, "The RevisionId properties don't match!");

            foreach (var expectedManualTrace in expectedRelationships.ManualTraces)
            {
                var actualManualTrace = actualRelationships.ManualTraces.Find(r => r.ArtifactId.Equals(expectedManualTrace.ArtifactId) &&
                                                                                   r.ItemId.Equals(expectedManualTrace.ItemId) &&
                                                                                   r.Direction.Equals(expectedManualTrace.Direction) &&
                                                                                   r.TraceType.Equals(expectedManualTrace.TraceType));

                Assert.NotNull(actualManualTrace,
                    "Couldn't find an actual Manual Trace matching expected Manual Trace with ArtifactId: {0}, ItemId: {1}",
                    expectedManualTrace.ArtifactId, expectedManualTrace.ItemId);

                Relationship.AssertRelationshipsAreEqual(expectedManualTrace, actualManualTrace);
            }

            foreach (var expectedOtherTrace in expectedRelationships.OtherTraces)
            {
                var actualOtherTrace = actualRelationships.OtherTraces.Find(r => r.ArtifactId.Equals(expectedOtherTrace.ArtifactId) &&
                                                                                 r.ItemId.Equals(expectedOtherTrace.ItemId) &&
                                                                                 r.Direction.Equals(expectedOtherTrace.Direction) &&
                                                                                 r.TraceType.Equals(expectedOtherTrace.TraceType));

                Assert.NotNull(actualOtherTrace,
                    "Couldn't find an actual Other Trace matching expected Other Trace with ArtifactId: {0}, ItemId: {1}",
                    expectedOtherTrace.ArtifactId, expectedOtherTrace.ItemId);

                Relationship.AssertRelationshipsAreEqual(expectedOtherTrace, actualOtherTrace);
            }
        }
    }

    // Found in:  blueprint/svc/ArtifactStore/Models/RelationshipResultSet.cs
    public class Relationship : ITrace, INovaTrace
    {
        #region Serialized JSON properties

        public int ArtifactId { get; set; }
        public string ArtifactTypePrefix { get; set; }
        public string ArtifactName { get; set; }
        public int ItemId { get; set; }
        public string ItemTypePrefix { get; set; }
        public string ItemName { get; set; }
        public string ItemLabel { get; set; }
        public int ProjectId { get; set; }
        public string ProjectName { get; set; }

        [JsonProperty("TraceDirection")]
        public TraceDirection Direction { get; set; }
        public LinkType TraceType { get; set; }

        [JsonProperty("Suspect")]
        public bool IsSuspect { get; set; }
        public bool HasAccess { get; set; } = true;
        public bool ReadOnly { get; set; }
        public int PrimitiveItemTypePredefined { get; set; }

        #endregion Serialized JSON properties

        /// <summary>
        /// Asserts that the properties of the two Relationship objects are identical.
        /// </summary>
        /// <param name="expectedRelationship">The expected Relationship.</param>
        /// <param name="actualRelationship">The actual Relationship.</param>
        /// <exception cref="AssertionException">If any properties don't match.</exception>
        public static void AssertRelationshipsAreEqual(Relationship expectedRelationship, Relationship actualRelationship)
        {
            ThrowIf.ArgumentNull(expectedRelationship, nameof(expectedRelationship));
            ThrowIf.ArgumentNull(actualRelationship, nameof(actualRelationship));

            Assert.AreEqual(expectedRelationship.ArtifactId, actualRelationship.ArtifactId, "The ArtifactId properties don't match!");
            Assert.AreEqual(expectedRelationship.ArtifactTypePrefix, actualRelationship.ArtifactTypePrefix, "The ArtifactTypePrefix properties don't match!");
            Assert.AreEqual(expectedRelationship.ArtifactName, actualRelationship.ArtifactName, "The ArtifactName properties don't match!");
            Assert.AreEqual(expectedRelationship.ItemId, actualRelationship.ItemId, "The ItemId properties don't match!");
            Assert.AreEqual(expectedRelationship.ItemTypePrefix, actualRelationship.ItemTypePrefix, "The ItemTypePrefix properties don't match!");
            Assert.AreEqual(expectedRelationship.ItemName, actualRelationship.ItemName, "The ItemName properties don't match!");
            Assert.AreEqual(expectedRelationship.ItemLabel, actualRelationship.ItemLabel, "The ItemLabel properties don't match!");
            Assert.AreEqual(expectedRelationship.ProjectId, actualRelationship.ProjectId, "The ProjectId properties don't match!");
            Assert.AreEqual(expectedRelationship.ProjectName, actualRelationship.ProjectName, "The ProjectName properties don't match!");
            Assert.AreEqual(expectedRelationship.Direction, actualRelationship.Direction, "The Direction properties don't match!");
            Assert.AreEqual(expectedRelationship.TraceType, actualRelationship.TraceType, "The TraceType properties don't match!");
            Assert.AreEqual(expectedRelationship.IsSuspect, actualRelationship.IsSuspect, "The IsSuspect properties don't match!");
            Assert.AreEqual(expectedRelationship.HasAccess, actualRelationship.HasAccess, "The HasAccess properties don't match!");
            Assert.AreEqual(expectedRelationship.ReadOnly, actualRelationship.ReadOnly, "The ReadOnly properties don't match!");
            Assert.AreEqual(expectedRelationship.PrimitiveItemTypePredefined, actualRelationship.PrimitiveItemTypePredefined, "The PrimitiveItemTypePredefined properties don't match!");
        }
    }

    // Found in:  blueprint/svc/ArtifactStore/Models/RelationshipResultSet.cs  as ItemIdItemNameParentId
    public class TracePathItem
    {
        public int ItemId { get; set; }
        public int VersionProjectId { get; set; }
        [JsonProperty(NullValueHandling = NullValueHandling.Include)]   // Dev always sends ParentId, even if it's null.
        public int? ParentId { get; set; }
        public string Name { get; set; }
        public int ItemTypeId { get; set; }
    }

    // Found in:  blueprint/svc/ArtifactStore/Models/RelationshipResultSet.cs  as RelationshipExtendedInfo
    public class TraceDetails
    {
        public int ArtifactId { get; set; }
        [JsonProperty(NullValueHandling = NullValueHandling.Include)]   // Dev always sends Description, even if it's null.
        public string Description { get; set; }
        public List<TracePathItem> PathToProject { get; } = new List<TracePathItem>();
    }

    public enum TraceDirection
    {
        To,
        From,
        TwoWay
    }

    // Found in:  blueprint-current/Source/BluePrintSys.RC.Service.Business/Models/Api/TraceType.cs
    /// <summary>
    /// Type of trace.
    /// </summary>
    [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Naming", "CA1714:FlagsEnumsShouldHavePluralNames")]
    [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1008:EnumsShouldHaveZeroValue")]
    [Flags]
    public enum TraceType
    {
        /// <summary>
        /// Parent
        /// </summary>
        Parent = 0,
        /// <summary>
        /// Child
        /// </summary>
        Child = 1,
        /// <summary>
        /// Manual
        /// </summary>
        Manual = 2,
        /// <summary>
        /// All other traces including inherits from etc
        /// </summary>
        Other = 4,
        /// <summary>
        /// Reuse
        /// </summary>
        Reuse = 8,

        // -----------------------------------

        // NOTE: The following values are not a part of TraceType in: blueprint-current/Source/BluePrintSys.RC.Service.Business/Models/Api/TraceType.cs

        ActorInherits = 16, // Came from LinkType in:  blueprint-current/Source/BluePrintSys.RC.Data.AccessAPI/Model/LinkType.cs
        DocReference = 32   // Came from LinkType in:  blueprint-current/Source/BluePrintSys.RC.Data.AccessAPI/Model/LinkType.cs
    }

    // Found in:  blueprint-current/Source/BluePrintSys.RC.Data.AccessAPI/Model/LinkType.cs
    [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Naming", "CA1714:FlagsEnumsShouldHavePluralNames")]
    [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1008:EnumsShouldHaveZeroValue")]
    [Flags]
    public enum LinkType
    {
        ParentChild = 0x1,
        Manual = 0x2,

        // for now we don't store SubArtifact in the DB. just return it from [TraceabilityMatrix].[GetRawData]
        SubArtifact = 0x4,

        // below there are all OTHER link types
        Association = 0x8,
        [JsonProperty("ActorInheritsFrom")]
        ActorInherits = 0x10,
        DocumentReference = 0x20,

        GlossaryReference = 0x40,
        ShapeConnector = 0x80,

        BaselineReference = 0x100,
        ReviewPackageReference = 0x200,

        Reuse = 0x400
    }
}
