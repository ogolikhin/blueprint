using System;
using System.Collections.Generic;
using System.Globalization;
using Common;
using Model.ArtifactModel;
using Model.ArtifactModel.Impl;
using Newtonsoft.Json;
using NUnit.Framework;
using Utilities;

namespace Model.Impl
{
    public class ArtifactDetails
    {
        #region Properties

        public int Id { get; set; }
        public string Name { get; set; }
        public string Description { get; set; }
        public int ParentId { get; set; }
        public int Permissions { get; set; }    // Bit flags of this enum:  blueprint-current/Source/BluePrintSys.RC.Data.AccessAPI/Model/RolePermissions.cs

        /// <summary>This is the order of the artifact in the parent/child tree.</summary>
        public double OrderIndex { get; set; }
        public int ItemTypeId { get; set; }
        public int ItemTypeVersionId { get; set; }
        public DateTime? LockedDateTime { get; set; }
        public int ProjectId { get; set; }
        public int Version { get; set; }
        public DateTime? CreatedOn { get; set; }
        public DateTime? LastEditedOn { get; set; }
        public Identification CreatedBy { get; set; }
        public Identification LastEditedBy { get; set; }
        public Identification LockedByUser { get; set; }
        public List<CustomProperty> CustomProperties { get; } = new List<CustomProperty>();
        public List<CustomProperty> SpecificPropertyValues { get; } = new List<CustomProperty>();   // XXX: Right now ArtifactStore always returns an empty list for this.

        #endregion Properties

        /// <summary>
        /// Wraps text within the body tags of HTML tags if it's not already inside HTML tags.
        /// </summary>
        /// <param name="textToWrap">The text you want to wrap.</param>
        /// <param name="shouldWrap">(optional) If you pass false, this function doesn't modify the string.</param>
        /// <returns>The HTML wrapped string.</returns>
        private static string HtmlWrapper(string textToWrap, bool shouldWrap = true)
        {
            if (shouldWrap && (textToWrap != null))
            {
                if (!textToWrap.StartsWithOrdinal("<html>"))
                {
                    textToWrap = I18NHelper.FormatInvariant("<html><head></head><body>{0}</body></html>", textToWrap);
                }
            }

            return textToWrap;
        }

        /// <summary>
        /// Asserts that this ArtifactDetails object is equal to the specified artifact.
        /// </summary>
        /// <param name="artifact">The artifact to compare against.</param>
        /// <param name="shouldWrapWithHtmlTags">(optional) Specifies whether certain properties should be wrapped with HTML tags before comparing them.</param>
        /// <param name="shouldCompareVersions">(optional) Specifies whether the version property should be compared.</param>
        /// <exception cref="AssertionException">If any of the properties are different.</exception>
        public void AssertAreEqual(IArtifactBase artifact, bool shouldWrapWithHtmlTags = true, bool shouldCompareVersions = true)
        {
            ThrowIf.ArgumentNull(artifact, nameof(artifact));

            Assert.AreEqual(Id, artifact.Id,
                "Artifact Details ID doesn't match artifact ID");

            Assert.AreEqual(Name, artifact.Name,
                "Artifact Details Name doesn't match artifact Name");

            var property = artifact.Properties.Find(p => p.Name == "Description");
            Assert.AreEqual(HtmlWrapper(Description, shouldWrapWithHtmlTags), HtmlWrapper(property?.TextOrChoiceValue, shouldWrapWithHtmlTags),
                "Artifact Details Description doesn't match artifact Description.");

            Assert.AreEqual(ParentId, artifact.ParentId,
                "Artifact Details ParentId doesn't match artifact ParentId.");

            Assert.AreEqual(ItemTypeId, artifact.ArtifactTypeId,
                "Artifact Details ArtifactTypeId doesn't match artifact ArtifactTypeId.");

            Assert.AreEqual(ProjectId, artifact.ProjectId,
                "Artifact Details ProjectId doesn't match artifact ProjectId.");

            if (shouldCompareVersions)
            {
                Assert.AreEqual(Version, artifact.Version,
                    "Artifact Details Version doesn't match artifact Version");
            }

            property = artifact.Properties.Find(p => p.Name == "Created On");
            Assert.AreEqual(CreatedOn.Value.ToUniversalTime(), DateTime.Parse(property?.DateValue, CultureInfo.InvariantCulture).ToUniversalTime(),
                "Artifact Details CreatedOn doesn't match artifact CreatedOn.");

            property = artifact.Properties.Find(p => p.Name == "Last Edited On");
            Assert.AreEqual(LastEditedOn.Value.ToUniversalTime(), DateTime.Parse(property?.DateValue, CultureInfo.InvariantCulture).ToUniversalTime(),
                "Artifact Details LastEditedOn doesn't match artifact LastEditedOn.");

            property = artifact.Properties.Find(p => p.Name == "Created By");
            CreatedBy.AssertEquals(property.UsersAndGroups.Find(p => p.Id == CreatedBy?.Id));

            property = artifact.Properties.Find(p => p.Name == "Last Edited By");
            LastEditedBy.AssertEquals(property?.UsersAndGroups.Find(p => p.Id == LastEditedBy?.Id));

            // NOTE: OpenApi doesn't return these properties, so we can't compare them:
            //     OrderIndex, ArtifactTypeVersionId, LockedByUser.
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

            Assert.AreEqual(Id, userOrGroup.Id, "The Id properties don't match!");
            Assert.AreEqual(DisplayName, userOrGroup.DisplayName, "The DisplayName properties don't match!");
        }
    }

    public class CustomProperty
    {
        public string Name { get; set; }
        public string PropertyTypeId { get; set; }
        public string PropertyTypeVersionId { get; set; }
        public string PropertyTypePredefined { get; set; }
        public List<string> Value { get; } = new List<string>();
    }
}
