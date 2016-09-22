using Model.ArtifactModel.Impl.PredefinedProperties;
using Newtonsoft.Json;
using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using Utilities;

namespace Model.ArtifactModel.Impl
{
    public class NovaArtifactDetails : INovaArtifactDetails
    {
        #region Serialized JSON Properties

        public Identification CreatedBy { get; set; }
        public DateTime? CreatedOn { get; set; }
        public DateTime? LastSavedOn { get; set; }
        public List<CustomProperty> CustomPropertyValues { get; } = new List<CustomProperty>();
        public string Description { get; set; }
        public int Id { get; set; }
        public int ItemTypeId { get; set; }
        public int ItemTypeVersionId { get; set; }
        public Identification LastEditedBy { get; set; }
        public DateTime? LastEditedOn { get; set; }
        public Identification LockedByUser { get; set; }
        public DateTime? LockedDateTime { get; set; }
        public string Name { get; set; }
        public double OrderIndex { get; set; }
        public int ParentId { get; set; }
        public int Permissions { get; set; }
        public int ProjectId { get; set; }
        public int Version { get; set; }
        public List<CustomProperty> SpecificPropertyValues { get; } = new List<CustomProperty>();   // XXX: Right now ArtifactStore always returns an empty list for this.

        #endregion Serialized JSON Properties

        #region Constructors

        public NovaArtifactDetails() : base()
        {
            //base constructor
        }

        #endregion Constructors

        /// <summary>
        /// Asserts that this NovaArtifactDetails object is equal to the specified ArtifactBase.
        /// </summary>
        /// <param name="artifact">The ArtifactBase to compare against.</param>
        /// <exception cref="AssertionException">If any of the properties are different.</exception>
        public void AssertEquals(IArtifactBase artifact)
        {
            ThrowIf.ArgumentNull(artifact, nameof(artifact));

            Assert.AreEqual(Id, artifact.Id, "The Id parameters don't match!");
            Assert.AreEqual(Name, artifact.Name, "The Name  parameters don't match!");
            Assert.AreEqual(ParentId, artifact.ParentId, "The ParentId  parameters don't match!");
            Assert.AreEqual(ItemTypeId, artifact.ArtifactTypeId, "The ItemTypeId  parameters don't match!");
            Assert.AreEqual(ProjectId, artifact.ProjectId, "The ProjectId  parameters don't match!");
            Assert.AreEqual(Version, artifact.Version, "The Version  parameters don't match!");
        }

        /// <summary>
        /// Asserts that this NovaArtifactDetails object is equal to the specified NovaArtifactDetails.
        /// </summary>
        /// <param name="artifact">The NovaArtifactDetails to compare against.</param>
        /// <exception cref="AssertionException">If any of the properties are different.</exception>
        public void AssertEquals(NovaArtifactDetails artifact)
        {
            ThrowIf.ArgumentNull(artifact, nameof(artifact));

            Assert.AreEqual(Id, artifact.Id, "The Id parameters don't match!");
            Assert.AreEqual(Name, artifact.Name, "The Name  parameters don't match!");
            Assert.AreEqual(Description, artifact.Description, "The Description  parameters don't match!");
            Assert.AreEqual(ParentId, artifact.ParentId, "The ParentId  parameters don't match!");
            Assert.AreEqual(Permissions, artifact.Permissions, "The Permissions  parameters don't match!");
            Assert.AreEqual(OrderIndex, artifact.OrderIndex, "The OrderIndex  parameters don't match!");
            Assert.AreEqual(ItemTypeId, artifact.ItemTypeId, "The ItemTypeId  parameters don't match!");
            Assert.AreEqual(ItemTypeVersionId, artifact.ItemTypeVersionId, "The ItemTypeVersionId  parameters don't match!");
            Assert.AreEqual(LockedDateTime, artifact.LockedDateTime, "The LockedDateTime  parameters don't match!");
            Assert.AreEqual(ProjectId, artifact.ProjectId, "The ProjectId  parameters don't match!");
            Assert.AreEqual(Version, artifact.Version, "The Version  parameters don't match!");
            Assert.AreEqual(CreatedOn, artifact.CreatedOn, "The CreatedOn  parameters don't match!");
            Assert.AreEqual(LastEditedOn, artifact.LastEditedOn, "The LastEditedOn  parameters don't match!");

            Identification.AssertEquals(CreatedBy, artifact.CreatedBy);
            Identification.AssertEquals(LastEditedBy, artifact.LastEditedBy);
            Identification.AssertEquals(LockedByUser, artifact.LockedByUser);

            Assert.AreEqual(CustomPropertyValues.Count, artifact.CustomPropertyValues.Count, "The number of Custom Properties is different!");
            Assert.AreEqual(SpecificPropertyValues.Count, artifact.SpecificPropertyValues.Count, "The number of Specific Property Values is different!");

            // Now compare each property in CustomProperties & SpecificPropertyValues.
            foreach (CustomProperty property in CustomPropertyValues)
            {
                Assert.That(artifact.CustomPropertyValues.Exists(p => p.Name == property.Name),
                "Couldn't find a CustomProperty named '{0}'!", property.Name);
            }

            foreach (CustomProperty property in SpecificPropertyValues)
            {
                Assert.That(artifact.SpecificPropertyValues.Exists(p => p.Name == property.Name),
                "Couldn't find a SpecificPropertyValue named '{0}'!", property.Name);
            }
        }

        /// <summary>
        /// Returns ActorInheritanceValue. It represents information from Inherited from field for Actor.
        /// </summary>
        /// <exception cref="FormatException">Throws FormatException if ActorInheritanceValue doesn't correspond to server JSON.</exception>
        public ActorInheritanceValue ActorInheritance
        {
            get
            {
                // Finding ActorInheritence among other properties
                CustomProperty actorInheritanceProperty = SpecificPropertyValues.FirstOrDefault(
                p => p.PropertyType == PropertyTypePredefined.ActorInheritance);
                if (actorInheritanceProperty == null)
                {
                    return null;
                }
                // Deserialization
                string actorInheritancePropertyString = actorInheritanceProperty.CustomPropertyValue.ToString();
                var actorInheritanceValue = JsonConvert.DeserializeObject<ActorInheritanceValue>(actorInheritancePropertyString);

                CheckIsJSONChanged<ActorInheritanceValue>(actorInheritanceProperty);

                return actorInheritanceValue;
            }

            set
            {
                Assert.IsNotNull(value);
                throw new NotImplementedException();
            }
        }

        /// <summary>
        /// DocumentFile property for Artifact of Document type
        /// TODO: replace this and GetActorInheritance function with generic function
        /// </summary>
        public DocumentFileValue DocumentFile
        {
            /// <summary>
            /// Returns DocumentFile property for Artifact of Document type
            /// </summary>
            get
            {
                // Finding DocumentFile among other properties
                CustomProperty documentFileProperty = SpecificPropertyValues.FirstOrDefault(
                    p => p.PropertyType == PropertyTypePredefined.DocumentFile);

                if (documentFileProperty == null)
                {
                    return null;
                }

                // Deserialization
                //string documentFilePropertyString = documentFileProperty.CustomPropertyValue.ToString();
                //var documentFilePropertyValue = JsonConvert.DeserializeObject<DocumentFileValue>(documentFilePropertyString);
                //CheckIsJSONChanged<DocumentFileValue>(documentFileProperty);

                return (DocumentFileValue)documentFileProperty.CustomPropertyValue;
            }

            /// <summary>
            /// Sets DocumentFile property for Artifact of Document type
            /// </summary>
            /// <param name="value">DocumentFile property.</param>
            set
            {
                // Finding DocumentFile among other properties
                CustomProperty documentFileProperty = SpecificPropertyValues.FirstOrDefault(
                    p => p.PropertyType == PropertyTypePredefined.DocumentFile);
                documentFileProperty.CustomPropertyValue = value;
            }
        }

        private static void CheckIsJSONChanged<TClass>(CustomProperty property)
        {
            // Deserialization
            string specificPropertyString = property.CustomPropertyValue.ToString();
            var specificPropertyValue = JsonConvert.DeserializeObject<TClass>(specificPropertyString);

            // Try to serialize and compare with JSON from the server
            string serializedObject = JsonConvert.SerializeObject(specificPropertyValue, Formatting.Indented);
            bool isJSONChanged = !(string.Equals(specificPropertyString, serializedObject, StringComparison.OrdinalIgnoreCase));
            string msg = Common.I18NHelper.FormatInvariant("JSON for {0} has been changed!", nameof(TClass));
            Assert.IsFalse(isJSONChanged, msg);
        }

        public class Identification
        {
            [JsonProperty("id")]
            public int Id { get; set; }

            [JsonProperty("displayName")]
            public string DisplayName { get; set; }

            /// <summary>
            /// Asserts that this object equals a specified UsersOrGroups object.
            /// </summary>
            /// <param name="userOrGroup">The User or Group to compare.</param>
            public void AssertEquals(UsersAndGroups userOrGroup)
            {
                ThrowIf.ArgumentNull(userOrGroup, nameof(userOrGroup));

                Assert.AreEqual(Id, userOrGroup?.Id, "The Id properties of the user or group don't match!");
                Assert.AreEqual(DisplayName, userOrGroup?.DisplayName, "The DisplayName properties of the user or group don't match!");
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
                }
            }
        }

        public class CustomProperty
        {
            public string Name { get; set; }

            [JsonProperty("value")]
            public object CustomPropertyValue { get; set; }

            public int PropertyTypeId { get; set; }

            public int PropertyTypeVersionId { get; set; }

            [JsonProperty("PropertyTypePredefined")]
            public PropertyTypePredefined PropertyType { get; set; }
        }



        [SuppressMessage("Microsoft.Design", "CA1008:EnumsShouldHaveZeroValue")]
        public enum PropertyTypePredefined
        {
            ActorInheritance = 4128,
            DocumentFile = 4129
        }
    }

    /// <summary>
    /// This is the class returned by some ArtifactStore REST calls.
    /// </summary>
    public class NovaArtifactResponse : INovaArtifactResponse
    {
        #region Serialized JSON Properties

        public NovaArtifactDetails.Identification CreatedBy { get; set; }
        public DateTime? CreatedOn { get; set; }
        public string Description { get; set; }
        public int Id { get; set; }
        public int ItemTypeId { get; set; }
        public NovaArtifactDetails.Identification LastEditedBy { get; set; }
        public DateTime? LastEditedOn { get; set; }
        public string Name { get; set; }
        public double OrderIndex { get; set; }
        public int ParentId { get; set; }
        public int PredefinedType { get; set; }
        public string Prefix { get; set; }
        public int ProjectId { get; set; }
        public int Version { get; set; }

        #endregion Serialized JSON Properties
    }

    public class NovaProject : INovaProject
    {
        #region Serialized JSON Properties

        public string Description { get; set; }
        public int Id { get; set; }
        public string Name { get; set; }

        #endregion Serialized JSON Properties
    }

    /// <summary>
    /// This class is returned by Nova Publish.
    /// </summary>
    public class NovaPublishResponse : INovaPublishResponse
    {
        #region Serialized JSON Properties

        /// <summary>
        /// The artifacts that were published.
        /// </summary>
        [SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]     // Ignore this warning for now.
        [JsonConverter(typeof(Deserialization.ConcreteListConverter<INovaArtifactResponse, NovaArtifactResponse>))]
        public List<INovaArtifactResponse> Artifacts { get; set; } = new List<INovaArtifactResponse>();

        /// <summary>
        /// The projects where the published artifacts exist.
        /// </summary>
        [SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]     // Ignore this warning for now.
        [JsonConverter(typeof(Deserialization.ConcreteListConverter<INovaProject, NovaProject>))]
        public List<INovaProject> Projects { get; set; } = new List<INovaProject>();

        #endregion Serialized JSON Properties
    }
}
