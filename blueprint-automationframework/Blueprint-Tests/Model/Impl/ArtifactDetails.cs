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
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Maintainability", "CA1502:AvoidExcessiveComplexity")]
        public void AssertEquals(IArtifactBase artifact, bool shouldWrapWithHtmlTags = true, bool shouldCompareVersions = true)
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
            const string createdOnMismatch = "Artifact Details CreatedOn doesn't match artifact CreatedOn.";

            if (property?.DateValue == null)
            {
                Assert.AreEqual(CreatedOn, property?.DateValue, createdOnMismatch);
            }
            else
            {
                Assert.AreEqual(CreatedOn.Value.ToUniversalTime(), DateTime.Parse(property?.DateValue, CultureInfo.InvariantCulture).ToUniversalTime(),
                    createdOnMismatch);
            }

            property = artifact.Properties.Find(p => p.Name == "Last Edited On");
            const string lastEditedOnMistatch = "Artifact Details LastEditedOn doesn't match artifact LastEditedOn.";

            if (property?.DateValue == null)
            {
                Assert.AreEqual(LastEditedOn, property?.DateValue, lastEditedOnMistatch);
            }
            else
            {
                Assert.AreEqual(LastEditedOn.Value.ToUniversalTime(), DateTime.Parse(property?.DateValue, CultureInfo.InvariantCulture).ToUniversalTime(),
                    lastEditedOnMistatch);
            }

            property = artifact.Properties.Find(p => p.Name == "Created By");
            CreatedBy.AssertEquals(property?.UsersAndGroups?.Find(p => p.Id == CreatedBy?.Id));

            property = artifact.Properties.Find(p => p.Name == "Last Edited By");
            LastEditedBy.AssertEquals(property?.UsersAndGroups?.Find(p => p.Id == LastEditedBy?.Id));

            // NOTE: OpenApi doesn't return these properties, so we can't compare them:
            //     OrderIndex, ArtifactTypeVersionId, LockedByUser.
        }

        /// <summary>
        /// Asserts that this ArtifactDetails object is equal to the specified ArtifactDetails.
        /// </summary>
        /// <param name="artifact">The ArtifactDetails to compare against.</param>
        /// <exception cref="AssertionException">If any of the properties are different.</exception>
        public void AssertEquals(ArtifactDetails artifact)
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

            Assert.AreEqual(CustomProperties.Count, artifact.CustomProperties.Count, "The number of Custom Properties is different!");
            Assert.AreEqual(SpecificPropertyValues.Count, artifact.SpecificPropertyValues.Count, "The number of Specific Property Values is different!");

            // Now compare each property in CustomProperties & SpecificPropertyValues.
            foreach (CustomProperty property in CustomProperties)
            {
                Assert.That(artifact.CustomProperties.Exists(p => p.Name == property.Name),
                    "Couldn't find a CustomProperty named '{0}'!", property.Name);
            }

            foreach (CustomProperty property in SpecificPropertyValues)
            {
                Assert.That(artifact.SpecificPropertyValues.Exists(p => p.Name == property.Name),
                    "Couldn't find a SpecificPropertyValue named '{0}'!", property.Name);
            }
        }

        /// <summary>
        /// Copies all properties from the source artifact into this object.
        /// </summary>
        /// <param name="sourceArtifact">The artifact that contains the properties to copy from.</param>
        public void DeepCopy(ArtifactDetails sourceArtifact)
        {
            ThrowIf.ArgumentNull(sourceArtifact, nameof(sourceArtifact));

            Id = sourceArtifact.Id;
            Name = sourceArtifact.Name;
            Description = sourceArtifact.Description;
            ParentId = sourceArtifact.ParentId;
            Permissions = sourceArtifact.Permissions;
            OrderIndex = sourceArtifact.OrderIndex;
            ItemTypeId = sourceArtifact.ItemTypeId;
            ItemTypeVersionId = sourceArtifact.ItemTypeVersionId;
            LockedDateTime = sourceArtifact.LockedDateTime;
            ProjectId = sourceArtifact.ProjectId;
            Version = sourceArtifact.Version;
            CreatedOn = sourceArtifact.CreatedOn;
            LastEditedOn = sourceArtifact.LastEditedOn;
            CreatedBy = sourceArtifact.CreatedBy;
            LastEditedBy = sourceArtifact.LastEditedBy;
            LockedByUser = sourceArtifact.LockedByUser;

            CustomProperties.Clear();
            SpecificPropertyValues.Clear();

            CustomProperties.AddRange(sourceArtifact.CustomProperties);
            SpecificPropertyValues.AddRange(sourceArtifact.SpecificPropertyValues);
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
        public string PropertyTypeId { get; set; }
        public string PropertyTypeVersionId { get; set; }
        public string PropertyTypePredefined { get; set; }
        public List<string> Value { get; } = new List<string>();
    }
}
