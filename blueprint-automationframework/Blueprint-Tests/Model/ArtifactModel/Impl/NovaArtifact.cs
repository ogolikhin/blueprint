using Newtonsoft.Json;
using NUnit.Framework;
using System;
using System.Collections.Generic;
using Utilities;

namespace Model.ArtifactModel.Impl
{
    public class NovaArtifact : NovaArtifactBase, INovaArtifact
    {

        #region Serialized JSON Properties

        public DateTime? CreatedOn { get; set; }
        public bool HasChildren { get; set; }
        public override int Id { get; set; }
        public override int? ItemTypeId { get; set; }
        public Identification LockedByUser { get; set; }
        public DateTime? LockedDateTime { get; set; }
        public override string Name { get; set; }
        public double? OrderIndex { get; set; }
        public override int? ParentId { get; set; }
        public RolePermissions? Permissions { get; set; }
        public int? PredefinedType { get; set; }
        public string Prefix { get; set; }
        public override int? ProjectId { get; set; }
        public override int? Version { get; set; }

        [JsonConverter(typeof(SerializationUtilities.ConcreteListConverter<INovaArtifact, NovaArtifact>))]
        public List<INovaArtifact> Children { get; set; }   // This is optional and can be null depending on the REST call made.

        #endregion Serialized JSON Properties

        #region Constructors

        public NovaArtifact() : base()
        {
            //base constructor
        }

        #endregion Constructors

        /// <summary>
        /// Asserts that the properties of the two NovaArtifact objects are equal.
        /// </summary>
        /// <param name="expectedArtifact">The expected artifact to compare.</param>
        /// <param name="actualArtifact">The actual artifact to compare.</param>
        /// <param name="skipIdAndVersion">(optional) Pass true to skip comparison of the Id and Version properties.</param>
        /// <param name="skipParentId">(optional) Pass true to skip comparison of the ParentId properties.</param>
        /// <param name="skipOrderIndex">(optional) Pass true to skip comparoson of the OrderIndex properties.</param>
        /// <param name="skipPublishedProperties">(optional) Pass true to skip comparison of properties that only published artifacts have.</param>
        /// <exception cref="AssertionException">If any of the properties are different.</exception>
        public static void AssertAreEqual(INovaArtifact expectedArtifact,
            INovaArtifact actualArtifact,
            bool skipIdAndVersion = true,
            bool skipParentId = false,
            bool skipOrderIndex = false,
            bool skipPublishedProperties = false)
        {
            ThrowIf.ArgumentNull(expectedArtifact, nameof(expectedArtifact));
            ThrowIf.ArgumentNull(actualArtifact, nameof(actualArtifact));

            Assert.AreEqual(expectedArtifact.HasChildren, actualArtifact.HasChildren, "Artifact HasChildren properties don't match!");
            Assert.AreEqual(expectedArtifact.ItemTypeId, actualArtifact.ItemTypeId, "Artifact ItemTypeId properties don't match!");
            Assert.AreEqual(expectedArtifact.Name, actualArtifact.Name, "Artifact Name properties don't match!");
            Assert.AreEqual(expectedArtifact.Permissions, actualArtifact.Permissions, "Artifact Permission properties don't match!");
            Assert.AreEqual(expectedArtifact.PredefinedType, actualArtifact.PredefinedType, "Artifact PredefinedType properties don't match!");
            Assert.AreEqual(expectedArtifact.Prefix, actualArtifact.Prefix, "Artifact Prefix properties don't match!");
            Assert.AreEqual(expectedArtifact.ProjectId, actualArtifact.ProjectId, "Artifact ProjectId properties don't match!");
            Assert.AreEqual(expectedArtifact.Children?.Count, actualArtifact.Children?.Count, "Artifact Children.Count properties don't match!");

            if (!skipIdAndVersion)
            {
                Assert.AreEqual(expectedArtifact.Id, actualArtifact.Id, "Artifact ID properties don't match!");
                Assert.AreEqual(expectedArtifact.Version, actualArtifact.Version, "Artifact Versions don't match!");
            }

            if (!skipParentId)
            {
                Assert.AreEqual(expectedArtifact.ParentId, actualArtifact.ParentId, "Artifact ParentId properties don't match!");
            }

            if (!skipOrderIndex)
            {
                Assert.AreEqual(expectedArtifact.OrderIndex, actualArtifact.OrderIndex, "Artifact OrderIndex properties don't match!");
            }

            if (!skipPublishedProperties)
            {
                Assert.AreEqual(expectedArtifact.LockedByUser, actualArtifact.LockedByUser, "Artifact LockedByUser properties don't match!");
                Assert.AreEqual(expectedArtifact.LockedDateTime, actualArtifact.LockedDateTime, "Artifact LockedDateTime properties don't match!");
            }
        }

        /// <summary>
        /// Asserts that this NovaArtifact object is equal to the specified OpenApiArtifact.
        /// </summary>
        /// <param name="artifact">The artifact to compare against.</param>
        /// <param name="shouldCompareVersions">(optional) Specifies whether the version property should be compared.</param>
        /// <exception cref="AssertionException">If any of the properties are different.</exception>
        public void AssertEquals(IArtifactBase artifact, bool shouldCompareVersions = true)
        {
            ThrowIf.ArgumentNull(artifact, nameof(artifact));

            Assert.AreEqual(Id, artifact.Id, "Artifact IDs don't match!");
            Assert.AreEqual(Name, artifact.Name, "Artifact Names don't match!");
            Assert.AreEqual(ParentId, artifact.ParentId, "Artifact ParentIds don't match!");
            Assert.AreEqual(ItemTypeId, artifact.ArtifactTypeId, "Artifact ArtifactTypeIds don't match!");
            Assert.AreEqual(ProjectId, artifact.ProjectId, "Artifact ProjectIds don't match!");

            if (shouldCompareVersions)
            {
                Assert.AreEqual(Version, artifact.Version, "Artifact Versions don't match!");
            }

            // NOTE: OpenApi doesn't return these properties, so we can't compare them:
            //     HasChildren, LockedDateTime, OrderIndex, LockedByUser, Permissions, PredefinedType, Prefix.
        }

        /// <summary>
        /// Asserts that this NovaArtifact object is equal to the specified ArtifactBase.
        /// </summary>
        /// <param name="artifact">The ArtifactBase to compare against.</param>
        /// <exception cref="AssertionException">If any of the properties are different.</exception>
        public void AssertEquals(INovaArtifactBase artifact)
        {
            ThrowIf.ArgumentNull(artifact, nameof(artifact));

            Assert.AreEqual(Id, artifact.Id, "The Id parameters don't match!");
            Assert.AreEqual(Name, artifact.Name, "The Name parameters don't match!");
            Assert.AreEqual(ParentId, artifact.ParentId, "The ParentId parameters don't match!");
            Assert.AreEqual(ItemTypeId, artifact.ItemTypeId, "The ItemTypeId parameters don't match!");
            Assert.AreEqual(ProjectId, artifact.ProjectId, "The ProjectId parameters don't match!");
            Assert.AreEqual(Version, artifact.Version, "The Version parameters don't match!");
        }

        /// <summary>
        /// Asserts that this NovaArtifact object is equal to the specified ArtifactDetails.
        /// </summary>
        /// <param name="artifact">The ArtifactDetails to compare against.</param>
        /// <exception cref="AssertionException">If any of the properties are different.</exception>
        public void AssertEquals(INovaArtifactDetails artifact)
        {
            ThrowIf.ArgumentNull(artifact, nameof(artifact));

            AssertEquals(artifact as INovaArtifactBase);
            Assert.AreEqual(LockedDateTime, artifact.LockedDateTime, "The LockedDateTime parameters don't match!");
            Assert.AreEqual(OrderIndex, artifact.OrderIndex, "The OrderIndex parameters don't match!");
            Assert.NotNull(artifact.Permissions, "Artifact Permissions shouldn't be null!");
            Assert.AreEqual(Permissions, artifact.Permissions, "The Permissions parameters don't match!");
            Identification.AssertEquals(LockedByUser, artifact.LockedByUser);
        }
    }
    
}
