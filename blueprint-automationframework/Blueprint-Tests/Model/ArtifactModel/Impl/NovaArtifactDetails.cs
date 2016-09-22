﻿using Model.ArtifactModel.Impl.PredefinedProperties;
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
        /// Asserts that this INovaArtifactDetails object is equal to the specified IArtifactBase.
        /// </summary>
        /// <param name="artifact">The IArtifactBase to compare against.</param>
        /// <exception cref="AssertionException">If any of the properties are different.</exception>
        public void AssertEquals(IArtifactBase artifact)
        {
            AssertEquals(this, artifact);
        }

        /// <summary>
        /// Asserts that the specified INovaArtifactDetails object is equal to the specified IArtifactBase.
        /// </summary>
        /// <param name="artifactDetails">The INovaArtifactDetails to compare against.</param>
        /// <param name="artifact">The IArtifactBase to compare against.</param>
        /// <exception cref="AssertionException">If any of the properties are different.</exception>
        public static void AssertEquals(INovaArtifactBase artifactDetails, IArtifactBase artifact)
        {
            ThrowIf.ArgumentNull(artifactDetails, nameof(artifactDetails));
            ThrowIf.ArgumentNull(artifact, nameof(artifact));

            Assert.AreEqual(artifactDetails.Id, artifact.Id, "The Id parameters don't match!");
            Assert.AreEqual(artifactDetails.Name, artifact.Name, "The Name  parameters don't match!");
            Assert.AreEqual(artifactDetails.ParentId, artifact.ParentId, "The ParentId  parameters don't match!");
            Assert.AreEqual(artifactDetails.ItemTypeId, artifact.ArtifactTypeId, "The ItemTypeId  parameters don't match!");
            Assert.AreEqual(artifactDetails.ProjectId, artifact.ProjectId, "The ProjectId  parameters don't match!");
            Assert.AreEqual(artifactDetails.Version, artifact.Version, "The Version  parameters don't match!");
        }

        /// <summary>
        /// Asserts that this INovaArtifactDetails object is equal to the specified INovaArtifactDetails.
        /// </summary>
        /// <param name="artifact">The INovaArtifactDetails to compare against.</param>
        /// <exception cref="AssertionException">If any of the properties are different.</exception>
        public void AssertEquals(INovaArtifactDetails artifact)
        {
            AssertEquals(this, artifact);
        }

        /// <summary>
        /// Asserts that both INovaArtifactDetails objects are equal.
        /// </summary>
        /// <param name="artifact1">The first INovaArtifactDetails to compare against.</param>
        /// <param name="artifact2">The second INovaArtifactDetails to compare against.</param>
        /// <exception cref="AssertionException">If any of the properties are different.</exception>
        public static void AssertEquals(INovaArtifactDetails artifact1, INovaArtifactDetails artifact2)
        {
            ThrowIf.ArgumentNull(artifact1, nameof(artifact1));
            ThrowIf.ArgumentNull(artifact2, nameof(artifact2));

            Assert.AreEqual(artifact1.Id, artifact2.Id, "The Id parameters don't match!");
            Assert.AreEqual(artifact1.Name, artifact2.Name, "The Name  parameters don't match!");
            Assert.AreEqual(artifact1.Description, artifact2.Description, "The Description  parameters don't match!");
            Assert.AreEqual(artifact1.ParentId, artifact2.ParentId, "The ParentId  parameters don't match!");
            Assert.AreEqual(artifact1.Permissions, artifact2.Permissions, "The Permissions  parameters don't match!");
            Assert.AreEqual(artifact1.OrderIndex, artifact2.OrderIndex, "The OrderIndex  parameters don't match!");
            Assert.AreEqual(artifact1.ItemTypeId, artifact2.ItemTypeId, "The ItemTypeId  parameters don't match!");
            Assert.AreEqual(artifact1.ItemTypeVersionId, artifact2.ItemTypeVersionId, "The ItemTypeVersionId  parameters don't match!");
            Assert.AreEqual(artifact1.LockedDateTime, artifact2.LockedDateTime, "The LockedDateTime  parameters don't match!");
            Assert.AreEqual(artifact1.ProjectId, artifact2.ProjectId, "The ProjectId  parameters don't match!");
            Assert.AreEqual(artifact1.Version, artifact2.Version, "The Version  parameters don't match!");
            Assert.AreEqual(artifact1.CreatedOn, artifact2.CreatedOn, "The CreatedOn  parameters don't match!");
            Assert.AreEqual(artifact1.LastEditedOn, artifact2.LastEditedOn, "The LastEditedOn  parameters don't match!");

            Identification.AssertEquals(artifact1.CreatedBy, artifact2.CreatedBy);
            Identification.AssertEquals(artifact1.LastEditedBy, artifact2.LastEditedBy);
            Identification.AssertEquals(artifact1.LockedByUser, artifact2.LockedByUser);

            Assert.AreEqual(artifact1.CustomPropertyValues.Count, artifact2.CustomPropertyValues.Count, "The number of Custom Properties is different!");
            Assert.AreEqual(artifact1.SpecificPropertyValues.Count, artifact2.SpecificPropertyValues.Count, "The number of Specific Property Values is different!");

            // Now compare each property in CustomProperties & SpecificPropertyValues.
            foreach (CustomProperty property in artifact1.CustomPropertyValues)
            {
                Assert.That(artifact2.CustomPropertyValues.Exists(p => p.Name == property.Name),
                "Couldn't find a CustomProperty named '{0}'!", property.Name);
            }

            foreach (CustomProperty property in artifact1.SpecificPropertyValues)
            {
                Assert.That(artifact2.SpecificPropertyValues.Exists(p => p.Name == property.Name),
                "Couldn't find a SpecificPropertyValue named '{0}'!", property.Name);
            }
        }

        /// <summary>
        /// Asserts that this INovaArtifactDetails object is equal to the specified INovaArtifactResponse.
        /// </summary>
        /// <param name="artifact">The INovaArtifactResponse to compare against.</param>
        /// <exception cref="AssertionException">If any of the properties are different.</exception>
        public void AssertEquals(INovaArtifactResponse artifact)
        {
            AssertEquals(this, artifact);
        }

        /// <summary>
        /// Asserts that the specified INovaArtifactDetails object is equal to the specified INovaArtifactResponse.
        /// </summary>
        /// <param name="artifact1">The first INovaArtifactDetails to compare against.</param>
        /// <param name="artifact2">The second INovaArtifactResponse to compare against.</param>
        /// <exception cref="AssertionException">If any of the properties are different.</exception>
        public static void AssertEquals(INovaArtifactDetails artifact1, INovaArtifactResponse artifact2)
        {
            ThrowIf.ArgumentNull(artifact1, nameof(artifact1));
            ThrowIf.ArgumentNull(artifact2, nameof(artifact2));

            Assert.AreEqual(artifact1.Id, artifact2.Id, "The Id parameters don't match!");
            Assert.AreEqual(artifact1.Name, artifact2.Name, "The Name  parameters don't match!");
            Assert.AreEqual(artifact1.Description, artifact2.Description, "The Description  parameters don't match!");
            Assert.AreEqual(artifact1.ParentId, artifact2.ParentId, "The ParentId  parameters don't match!");
            Assert.AreEqual(artifact1.OrderIndex, artifact2.OrderIndex, "The OrderIndex  parameters don't match!");
            Assert.AreEqual(artifact1.ItemTypeId, artifact2.ItemTypeId, "The ItemTypeId  parameters don't match!");
            Assert.AreEqual(artifact1.ProjectId, artifact2.ProjectId, "The ProjectId  parameters don't match!");
            Assert.AreEqual(artifact1.Version, artifact2.Version, "The Version  parameters don't match!");
            Assert.AreEqual(artifact1.CreatedOn, artifact2.CreatedOn, "The CreatedOn  parameters don't match!");
            Assert.AreEqual(artifact1.LastEditedOn, artifact2.LastEditedOn, "The LastEditedOn  parameters don't match!");

            Identification.AssertEquals(artifact1.CreatedBy, artifact2.CreatedBy);
            Identification.AssertEquals(artifact1.LastEditedBy, artifact2.LastEditedBy);
        }

        /// <summary>
        /// Returns ActorInheritanceValue. It represents information from Inherited from field for Actor.
        /// </summary>
        /// <exception cref="FormatException">Throws FormatException if ActorInheritanceValue doesn't correspond to server JSON.</exception>
        [SuppressMessage("Microsoft.Design", "CA1024:ChangeToProperty")]
        public ActorInheritanceValue GetActorInheritance()
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

            // Try to serialize and compare with JSON from the server
            string serializedObject = JsonConvert.SerializeObject(actorInheritanceValue, Formatting.Indented);
            bool isJSONChanged = !(string.Equals(actorInheritancePropertyString, serializedObject, StringComparison.OrdinalIgnoreCase));
            if (isJSONChanged)
            {
                    string msg = Common.I18NHelper.FormatInvariant("JSON for {0} has been changed!", nameof(ActorInheritanceValue));
                    throw new FormatException(msg);
            }
            //
            return actorInheritanceValue;
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

        // NOTE: Keep the properties in this order so the shouldControlJsonChanges option in RestApiFacade works properly.  This is the order of the incoming JSON.

        public int ProjectId { get; set; }
        public int Version { get; set; }
        public DateTime? CreatedOn { get; set; }
        public DateTime? LastEditedOn { get; set; }
        public NovaArtifactDetails.Identification CreatedBy { get; set; }
        public NovaArtifactDetails.Identification LastEditedBy { get; set; }
        public int Id { get; set; }
        public string Name { get; set; }
        public string Description { get; set; }
        public int ParentId { get; set; }
        public double OrderIndex { get; set; }
        public int ItemTypeId { get; set; }
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
        public string Description { get; set; }

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
