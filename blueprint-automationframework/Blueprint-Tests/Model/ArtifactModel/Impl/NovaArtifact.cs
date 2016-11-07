using System;
using System.Collections.Generic;
using Newtonsoft.Json;
using NUnit.Framework;
using Utilities;
using static Model.ArtifactModel.Impl.NovaArtifactDetails;

namespace Model.ArtifactModel.Impl
{
    public class NovaArtifact : NovaArtifactBase, INovaArtifact
    {

        #region Serialized JSON Properties
            
        public bool HasChildren { get; set; }
        public override int Id { get; set; }
        public override int? ItemTypeId { get; set; }
        public string ItemTypeName { get; set; }
        public Identification LockedByUser { get; set; }
        public DateTime? LockedDateTime { get; set; }
        public override string Name { get; set; }
        public double OrderIndex { get; set; }
        public override int? ParentId { get; set; }
        public int Permissions { get; set; }
        public int PredefinedType { get; set; }
        public string Prefix { get; set; }
        public override int? ProjectId { get; set; }
        public override int? Version { get; set; }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")] // This property can be null, so setter is needed.
        [JsonConverter(typeof(Deserialization.ConcreteListConverter<INovaArtifact, NovaArtifact>))]
        public List<INovaArtifact> Children { get; set; }   // This is optional and can be null depending on the REST call made.

        #endregion Serialized JSON Properties

        #region Constructors

        public NovaArtifact() : base()
        {
            //base constructor
        }

        #endregion Constructors

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
            Assert.AreEqual(Permissions, artifact.Permissions, "The Permissions parameters don't match!");
            Identification.AssertEquals(LockedByUser, artifact.LockedByUser);
        }
    }
    
}
