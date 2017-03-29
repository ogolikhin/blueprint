using Common;
using Model.ArtifactModel.Impl.PredefinedProperties;
using Newtonsoft.Json;
using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using Model.Common.Enums;
using Utilities;
using Model.ArtifactModel.Enums;
using Model.NovaModel.Components.RapidReview;

namespace Model.ArtifactModel.Impl
{
    public abstract class NovaArtifactBase : INovaArtifactBase
    {
        /// <summary>If this artifact was deleted, this will contain all related artifacts that were also deleted with it.</summary>
        [JsonIgnore]
        public List<int> DeletedArtifactIds { get; } = new List<int>();

        #region Serialized properties

        public abstract int Id { get; set; }
        public abstract int? ItemTypeId { get; set; }
        public abstract string Name { get; set; }
        public abstract int? ParentId { get; set; }
        public abstract int? ProjectId { get; set; }
        public abstract int? Version { get; set; }

        #endregion Serialized properties

        #region IArtifactObservable methods

        [JsonIgnore]
        public List<INovaArtifactObserver> NovaArtifactObservers { get; private set; }

        /// <seealso cref="RegisterObserver(INovaArtifactObserver)"/>
        public void RegisterObserver(INovaArtifactObserver observer)
        {
            if (NovaArtifactObservers == null)
            {
                NovaArtifactObservers = new List<INovaArtifactObserver>();
            }

            NovaArtifactObservers.Add(observer);
        }

        /// <seealso cref="UnregisterObserver(INovaArtifactObserver)"/>
        public void UnregisterObserver(INovaArtifactObserver observer)
        {
            NovaArtifactObservers?.Remove(observer);
        }

        /// <seealso cref="NotifyArtifactDeleted(List{INovaArtifactBase})"/>
        public void NotifyArtifactDeleted(List<INovaArtifactBase> deletedArtifactsList)
        {
            ThrowIf.ArgumentNull(deletedArtifactsList, nameof(deletedArtifactsList));

            // Notify the observers about any artifacts that were deleted as a result of this publish.
            foreach (var deletedArtifact in deletedArtifactsList)
            {
                var artifactIds = ((NovaArtifact) deletedArtifact).DeletedArtifactIds;

                Logger.WriteDebug("*** Notifying observers about deletion of artifact IDs: {0}", string.Join(", ", artifactIds));
                deletedArtifact.NovaArtifactObservers?.ForEach(o => o.NotifyArtifactDeleted(artifactIds));
            }
        }

        /// <seealso cref="NotifyArtifactPublished(List{INovaArtifactResponse})"/>
        public void NotifyArtifactPublished(List<INovaArtifactResponse> publishedArtifactsList)
        {
            ThrowIf.ArgumentNull(publishedArtifactsList, nameof(publishedArtifactsList));

            // Notify the observers about any artifacts that were deleted as a result of this publish.
            IEnumerable<int> publishedArtifactIds =
                from result in publishedArtifactsList
                select result.Id;

            // Convert to a list to remove the "Possible multiple enumeration" warning.
            var artifactIds = publishedArtifactIds as IList<int> ?? publishedArtifactIds.ToList();

            Logger.WriteDebug("*** Notifying observers about publish of artifact IDs: {0}", string.Join(", ", artifactIds));
            NovaArtifactObservers?.ForEach(o => o.NotifyArtifactPublished(artifactIds));
        }

        #endregion IArtifactObservable methods
    }

    public class NovaArtifactDetails : NovaArtifactBase, INovaArtifactDetails
    {
        // This function is used by Newtonsoft.Json to determine when to serialize property. See help for Newtonsoft.Json.Serialization.
        public bool ShouldSerializeAttachmentValues()
        {
            return AttachmentValues.Count > 0;
        }

        #region Serialized JSON Properties

        // NOTE: Keep the properties in this order so the shouldControlJsonChanges option in RestApiFacade works properly.  This is the order of the incoming JSON.

        public override int? ProjectId { get; set; }
        public override int? Version { get; set; }
        public List<AttachmentValue> AttachmentValues { get; } = new List<AttachmentValue>();

        [JsonProperty(NullValueHandling = NullValueHandling.Include)]   // Dev always sends CreatedOn, even if it's null.
        public DateTime? CreatedOn { get; set; }
        [JsonProperty(NullValueHandling = NullValueHandling.Include)]   // Dev always sends LastEditedOn, even if it's null.
        public DateTime? LastEditedOn { get; set; }
        [JsonProperty(NullValueHandling = NullValueHandling.Include)]   // Dev always sends CreatedBy, even if it's null.
        public Identification CreatedBy { get; set; }

        public ItemIndicatorFlags? IndicatorFlags { get; set; }

        [JsonProperty(NullValueHandling = NullValueHandling.Include)]   // Dev always sends LastEditedBy, even if it's null.
        public Identification LastEditedBy { get; set; }

        public DateTime? LastSavedOn { get; set; }
        public bool? LastSaveInvalid { get; set; }
        public RolePermissions? Permissions { get; set; }
        public Identification LockedByUser { get; set; }
        public DateTime? LockedDateTime { get; set; }
        public override int Id { get; set; }
        public override string Name { get; set; }

        [JsonProperty(NullValueHandling = NullValueHandling.Include)]   // Dev always sends Description, even if it's null.
        public string Description { get; set; }
        public override int? ParentId { get; set; }
        public double? OrderIndex { get; set; }
        public override int? ItemTypeId { get; set; }
        public string ItemTypeName { get; set; }
        public int ItemTypeVersionId { get; set; }
        public int? ItemTypeIconId { get; set; }
        public string Prefix { get; set; }
        public List<CustomProperty> CustomPropertyValues { get; set; } = new List<CustomProperty>();
        public List<CustomProperty> SpecificPropertyValues { get; set; } = new List<CustomProperty>();
        public int? PredefinedType { get; set; }

        public List<NovaTrace> Traces { get; set; }

        public List<NovaSubArtifact> SubArtifacts { get; set; }

        // TODO: found following properties when capturing PATCH /svc/bpartifactstore/artifacts/{artifactID}: 
        // SubArtifacts, Traces, DocRefValue
        // Maybe need to add for future integration test...

        #endregion Serialized JSON Properties

        #region Constructors

        // ReSharper disable once RedundantBaseConstructorCall
        // ReSharper disable once EmptyConstructor
        public NovaArtifactDetails() : base()
        {
            //base constructor
        }

        #endregion Constructors

        #region Other properties

        /// <summary>
        /// Gets or sets the DocumentFile property for Artifact of Document type.
        /// TODO: replace this and GetActorInheritance function with generic function
        /// </summary>
        [JsonIgnore]
        public DocumentFileValue DocumentFile
        {
            get
            {
                // Finding DocumentFile among other properties
                var documentFileProperty = SpecificPropertyValues.FirstOrDefault(
                    p => p.PropertyType == PropertyTypePredefined.DocumentFile);

                if ((documentFileProperty == null) || (documentFileProperty.CustomPropertyValue == null))
                {
                    return null;
                }

                // Deserialization
                //string documentFilePropertyString = documentFileProperty.CustomPropertyValue.ToString();
                //var documentFilePropertyValue = JsonConvert.DeserializeObject<DocumentFileValue>(documentFilePropertyString);
                //CheckIsJsonChanged<DocumentFileValue>(documentFileProperty);

                return (DocumentFileValue)documentFileProperty.CustomPropertyValue;
            }

            set
            {
                // Finding DocumentFile among other properties
                var documentFileProperty = SpecificPropertyValues.FirstOrDefault(
                    p => p.PropertyType == PropertyTypePredefined.DocumentFile);

                if (documentFileProperty != null)   // TODO: Should this throw an exception instead?
                {
                    documentFileProperty.CustomPropertyValue = value;
                }
            }
        }

        #endregion Other properties

        /// <summary>
        /// Asserts that the specified INovaArtifactBase object is equal to the specified IArtifactBase.
        /// </summary>
        /// <param name="novaArtifactBase1">The INovaArtifactBase to compare against.</param>
        /// <param name="novaArtifactBase2">The IArtifactBase to compare against.</param>
        /// <exception cref="AssertionException">If any of the properties are different.</exception>
        public static void AssertArtifactsEqual(INovaArtifactBase novaArtifactBase1, INovaArtifactBase novaArtifactBase2)
        {
            ThrowIf.ArgumentNull(novaArtifactBase1, nameof(novaArtifactBase1));
            ThrowIf.ArgumentNull(novaArtifactBase2, nameof(novaArtifactBase2));

            Assert.AreEqual(novaArtifactBase1.Id, novaArtifactBase2.Id, "The Id parameters don't match!");
            Assert.AreEqual(novaArtifactBase1.Name, novaArtifactBase2.Name, "The Name  parameters don't match!");
            Assert.AreEqual(novaArtifactBase1.ParentId, novaArtifactBase2.ParentId, "The ParentId  parameters don't match!");
            Assert.AreEqual(novaArtifactBase1.ItemTypeId, novaArtifactBase2.ItemTypeId, "The ItemTypeId  parameters don't match!");
            Assert.AreEqual(novaArtifactBase1.ProjectId, novaArtifactBase2.ProjectId, "The ProjectId  parameters don't match!");
            Assert.AreEqual(novaArtifactBase1.Version, novaArtifactBase2.Version, "The Version  parameters don't match!");
        }
    }

    /// <summary>
    /// This is the class returned by some ArtifactStore REST calls.
    /// </summary>
    public class NovaArtifactResponse : NovaArtifactBase, INovaArtifactResponse
    {
        #region Serialized JSON Properties

        // NOTE: Keep the properties in this order so the shouldControlJsonChanges option in RestApiFacade works properly.  This is the order of the incoming JSON.

        public override int? ProjectId { get; set; }
        public override int? Version { get; set; }
        [JsonProperty(NullValueHandling = NullValueHandling.Include)]   // Dev always sends CreatedOn, even if it's null.
        public DateTime? CreatedOn { get; set; }
        [JsonProperty(NullValueHandling = NullValueHandling.Include)]   // Dev always sends LastEditedOn, even if it's null.
        public DateTime? LastEditedOn { get; set; }
        [JsonProperty(NullValueHandling = NullValueHandling.Include)]   // Dev always sends CreatedBy, even if it's null.
        public Identification CreatedBy { get; set; }
        [JsonProperty(NullValueHandling = NullValueHandling.Include)]   // Dev always sends LastEditedBy, even if it's null.
        public Identification LastEditedBy { get; set; }

        public ItemIndicatorFlags? IndicatorFlags { get; set; }

        public override int Id { get; set; }
        public override string Name { get; set; }
        [JsonProperty(NullValueHandling = NullValueHandling.Include)]   // Dev always sends Description, even if it's null.
        public string Description { get; set; }
        public override int? ParentId { get; set; }
        public double OrderIndex { get; set; }
        public override int? ItemTypeId { get; set; }
        public int? ItemTypeIconId { get; set; }
        public string Prefix { get; set; }
        public int PredefinedType { get; set; }

        #endregion Serialized JSON Properties
    }

    public class NovaProject : INovaProject
    {
        #region Serialized JSON Properties

        // NOTE: Keep the properties in this order so the shouldControlJsonChanges option in RestApiFacade works properly.  This is the order of the incoming JSON.

        public int Id { get; set; }
        public string Name { get; set; }
        [JsonProperty(NullValueHandling = NullValueHandling.Include)]   // Dev always sends Description, even if it's null.
        public string Description { get; set; }

        public ItemIndicatorFlags? IndicatorFlags { get; set; }

        #endregion Serialized JSON Properties
    }

    // TODO: This file is getting way too big. We should move all other classes into their own files

    /// <summary>
    /// This class is returned by Nova calls such as: Discard, Publish...
    /// </summary>
    public class NovaArtifactsAndProjectsResponse : INovaArtifactsAndProjectsResponse
    {
        #region Serialized JSON Properties

        /// <summary>
        /// The artifacts that were published.
        /// </summary>
        [JsonConverter(typeof(SerializationUtilities.ConcreteListConverter<INovaArtifactResponse, NovaArtifactResponse>))]
        public List<INovaArtifactResponse> Artifacts { get; set; } = new List<INovaArtifactResponse>();

        /// <summary>
        /// The projects where the published artifacts exist.
        /// </summary>
        [JsonConverter(typeof(SerializationUtilities.ConcreteListConverter<INovaProject, NovaProject>))]
        public List<INovaProject> Projects { get; set; } = new List<INovaProject>();

        #endregion Serialized JSON Properties
    }

    public class NovaDiagramArtifact : NovaArtifactBase
    {
        #region Serialized JSON Properties

        // NOTE: Keep the properties in this order so the shouldControlJsonChanges option in RestApiFacade works properly.  This is the order of the incoming JSON.

        public override int? ProjectId { get; set; }
        public override int? Version { get; set; }

        [JsonProperty(NullValueHandling = NullValueHandling.Include)]   // Dev always sends CreatedOn, even if it's null.
        public DateTime? CreatedOn { get; set; }
        [JsonProperty(NullValueHandling = NullValueHandling.Include)]   // Dev always sends LastEditedOn, even if it's null.
        public DateTime? LastEditedOn { get; set; }
        [JsonProperty(NullValueHandling = NullValueHandling.Include)]   // Dev always sends CreatedBy, even if it's null.
        public Identification CreatedBy { get; set; }
        [JsonProperty(NullValueHandling = NullValueHandling.Include)]   // Dev always sends LastEditedBy, even if it's null.
        public Identification LastEditedBy { get; set; }

        public DateTime? LastSavedOn { get; set; }
        public int Permissions { get; set; }
        public Identification LockedByUser { get; set; }
        public DateTime? LockedDateTime { get; set; }
        public override int Id { get; set; }
        public override string Name { get; set; }

        [JsonProperty(NullValueHandling = NullValueHandling.Include)]   // Dev always sends Description, even if it's null.
        public string Description { get; set; }
        public override int? ParentId { get; set; }
        public double? OrderIndex { get; set; }
        public override int? ItemTypeId { get; set; }
        public int ItemTypeVersionId { get; set; }
        public string Prefix { get; set; }
        public List<CustomProperty> CustomPropertyValues { get; } = new List<CustomProperty>();
        public List<CustomProperty> SpecificPropertyValues { get; } = new List<CustomProperty>();
        public int? PredefinedType { get; set; }

        public string DiagramType { get; set; }
        public double Width { get; set; }
        public double Height { get; set; }
        public List<object> Shapes { get; set; }
        public List<object> Connections { get; set; }
        public int LibraryVersion { get; set; }

        #endregion Serialized JSON Properties
    }

    public class NovaGlossaryArtifact : NovaArtifactBase
    {
        #region Serialized JSON Properties

        // NOTE: Keep the properties in this order so the shouldControlJsonChanges option in RestApiFacade works properly.  This is the order of the incoming JSON.

        public override int? ProjectId { get; set; }
        public override int? Version { get; set; }

        [JsonProperty(NullValueHandling = NullValueHandling.Include)]   // Dev always sends CreatedOn, even if it's null.
        public DateTime? CreatedOn { get; set; }
        [JsonProperty(NullValueHandling = NullValueHandling.Include)]   // Dev always sends LastEditedOn, even if it's null.
        public DateTime? LastEditedOn { get; set; }
        [JsonProperty(NullValueHandling = NullValueHandling.Include)]   // Dev always sends CreatedBy, even if it's null.
        public Identification CreatedBy { get; set; }
        [JsonProperty(NullValueHandling = NullValueHandling.Include)]   // Dev always sends LastEditedBy, even if it's null.
        public Identification LastEditedBy { get; set; }

        public DateTime? LastSavedOn { get; set; }
        public int Permissions { get; set; }
        public Identification LockedByUser { get; set; }
        public DateTime? LockedDateTime { get; set; }
        public override int Id { get; set; }
        public override string Name { get; set; }

        [JsonProperty(NullValueHandling = NullValueHandling.Include)]   // Dev always sends Description, even if it's null.
        public string Description { get; set; }
        public override int? ParentId { get; set; }
        public double? OrderIndex { get; set; }
        public override int? ItemTypeId { get; set; }
        public int ItemTypeVersionId { get; set; }
        public string Prefix { get; set; }
        public List<CustomProperty> CustomPropertyValues { get; } = new List<CustomProperty>();
        public List<CustomProperty> SpecificPropertyValues { get; } = new List<CustomProperty>();
        public int? PredefinedType { get; set; }

        public List<object> SubArtifacts { get; set; }

        #endregion Serialized JSON Properties
    }

    public class NovaUseCaseArtifact : NovaArtifactBase
    {
        #region Serialized JSON Properties

        // NOTE: Keep the properties in this order so the shouldControlJsonChanges option in RestApiFacade works properly.  This is the order of the incoming JSON.

        public override int? ProjectId { get; set; }
        public override int? Version { get; set; }

        [JsonProperty(NullValueHandling = NullValueHandling.Include)]   // Dev always sends CreatedOn, even if it's null.
        public DateTime? CreatedOn { get; set; }
        [JsonProperty(NullValueHandling = NullValueHandling.Include)]   // Dev always sends LastEditedOn, even if it's null.
        public DateTime? LastEditedOn { get; set; }
        [JsonProperty(NullValueHandling = NullValueHandling.Include)]   // Dev always sends CreatedBy, even if it's null.
        public Identification CreatedBy { get; set; }
        [JsonProperty(NullValueHandling = NullValueHandling.Include)]   // Dev always sends LastEditedBy, even if it's null.
        public Identification LastEditedBy { get; set; }

        public DateTime? LastSavedOn { get; set; }
        public int Permissions { get; set; }
        public Identification LockedByUser { get; set; }
        public DateTime? LockedDateTime { get; set; }
        public override int Id { get; set; }
        public override string Name { get; set; }

        [JsonProperty(NullValueHandling = NullValueHandling.Include)]   // Dev always sends Description, even if it's null.
        public string Description { get; set; }
        public override int? ParentId { get; set; }
        public double? OrderIndex { get; set; }
        public override int? ItemTypeId { get; set; }
        public int ItemTypeVersionId { get; set; }
        public string Prefix { get; set; }
        public List<CustomProperty> CustomPropertyValues { get; } = new List<CustomProperty>();
        public List<CustomProperty> SpecificPropertyValues { get; } = new List<CustomProperty>();
        public int? PredefinedType { get; set; }

        public Step PreCondition { get; set; }
        public List<Step> Steps { get; set; }
        public Step PostCondition { get; set; }

        #endregion Serialized JSON Properties
    }

    public class CustomProperty
    {
        [JsonProperty("name", NullValueHandling = NullValueHandling.Include)]
        public string Name { get; set; }

        public int? PropertyTypeId { get; set; }

        public int? PropertyTypeVersionId { get; set; }

        [JsonProperty("PropertyTypePredefined")]
        public PropertyTypePredefined PropertyType { get; set; }

        public bool? IsMultipleAllowed { get; set; }

        public bool? IsRichText { get; set; }

        public int? PrimitiveType { get; set; }

        [JsonProperty("value", NullValueHandling = NullValueHandling.Ignore)]
        public object CustomPropertyValue { get; set; }

        [JsonProperty("isReuseReadOnly", NullValueHandling = NullValueHandling.Ignore)]
        public bool? IsReuseReadOnly { get; set; }
    }

    public class Identification
    {
        [JsonProperty("id")]
        public int Id { get; set; }

        [JsonProperty("displayName")]
        public string DisplayName { get; set; }

        [JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
        public bool? IsGroup { get; set; }

        /// <summary>
        /// Asserts that this object equals a specified UsersOrGroups object.
        /// </summary>
        /// <param name="userOrGroup">The User or Group to compare.</param>
        public void AssertEquals(UsersAndGroups userOrGroup)
        {
            ThrowIf.ArgumentNull(userOrGroup, nameof(userOrGroup));

            Assert.AreEqual(Id, userOrGroup?.Id, "The Id properties of the user or group don't match!");
            Assert.AreEqual(DisplayName, userOrGroup?.DisplayName, "The DisplayName properties of the user or group don't match!");

            if (IsGroup != null)
            {
                Assert.AreEqual(IsGroup.Value, (userOrGroup.Type == UsersAndGroupsType.Group),
                    "IsGroup is {0}, but the UsersAndGroups type is {1}!", IsGroup.Value, userOrGroup.Type);
            }
        }

        /// <summary>
        /// Asserts that both Identification objects are equal.
        /// </summary>
        /// <param name="identification1">The first Identification to compare.</param>
        /// <param name="identification2">The second Identification to compare.</param>
        public static void AssertEquals(Identification identification1, Identification identification2)
        {
            if ((identification1 == null) || (identification2 == null))
            {
                Assert.AreEqual(identification1, identification2, "One Identification is null but the other isn't!");
            }
            else
            {
                Assert.AreEqual(identification1.Id, identification2.Id, "The Id properties don't match!");
                Assert.AreEqual(identification1.DisplayName, identification2.DisplayName, "The DisplayName don't match!");

                Assert.AreEqual(identification1.IsGroup.HasValue, identification2.IsGroup.HasValue,
                    "One of the IsGroup properties is null but the other isn't!");

                if ((identification1.IsGroup != null) && (identification2.IsGroup != null))
                {
                    Assert.AreEqual(identification1.IsGroup.Value, identification2.IsGroup.Value,
                        "The IsGroup properties don't match!");
                }
            }
        }
    }
}
