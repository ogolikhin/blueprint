using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using Common;
using CustomAttributes;
using Helper;
using Model;
using Model.ArtifactModel;
using Model.ArtifactModel.Impl;
using Model.Factories;
using Model.OpenApiModel.Services;
using NUnit.Framework;
using TestCommon;
using Utilities;

namespace OpenAPITests
{
    [TestFixture]
    [Category(Categories.OpenApi)]
    public class SaveArtifactTests : TestBase
    {
        private IUser _adminUser = null;
        private IUser _authorUser = null;
        private IProject _project = null;

        [SetUp]
        public void SetUp()
        {
            Helper = new TestHelper();
            _adminUser = Helper.CreateUserAndAuthenticate(TestHelper.AuthenticationTokenTypes.BothAccessControlAndOpenApiTokens);
            _project = ProjectFactory.GetProject(_adminUser);
            _authorUser = Helper.CreateUserWithProjectRolePermissions(TestHelper.ProjectRole.Author, _project);
        }

        [TearDown]
        public void TearDown()
        {
            Helper?.Dispose();
        }

        #region Save artifact with image tests

        [TestCase]
        [TestRail(227108)]
        [Category(Categories.ArtifactStore)]
        [Description("Create an artifact and add an embedded image into one of it's Rich Text properties (using Nova).  Append text to the Rich Text property and " +
                     "save the artifact (using OpenAPI).  Verify that the image is still embedded.")]
        public void Save_ArtifactWithImageInRichTextProperty_AppendText_ImageIsStillEmbedded()
        {
            // Setup:
            var artifact = Helper.CreateAndPublishArtifact(_project, _authorUser, BaseArtifactType.Process);
            artifact.Lock(_authorUser);

            const int numberOfImagesToAdd = 1;
            var artifactDetails = ArtifactStoreHelper.AddRandomImageToArtifactProperty(_authorUser, Helper.ArtifactStore, artifact.Id,
                numberOfImagesToAdd: numberOfImagesToAdd);

            var openApiArtifact = OpenApi.GetArtifact(Helper.BlueprintServer.Address, _project, artifact.Id, _authorUser);
            var description = openApiArtifact.Properties.Find(p => p.Name == nameof(NovaArtifactDetails.Description));

            VerifyImagesAreEmbeddedInArtifactAndGetImageIds(artifactDetails.Description, description, numberOfImagesToAdd);

            // Append some text to the Description.
            const string textToAppend = "<p>Appending some text here</p>";
            description.TextOrChoiceValue = I18NHelper.FormatInvariant("{0}{1}", description.TextOrChoiceValue, textToAppend);

            // Execute:
            Assert.DoesNotThrow(() => OpenApiArtifact.UpdateArtifactDescription(openApiArtifact, _authorUser, updateWithRandomDescription: false),
                "OpenAPI Save method shouldn't fail.");

            // Verify:
            var artifactDetailsAfter = Helper.ArtifactStore.GetArtifactDetails(_authorUser, artifact.Id);

            Assert.AreNotEqual(artifactDetailsAfter.Description, description.TextOrChoiceValue,
                "The Nova artifact description should be different than in OpenAPI because the [Image = ID] tag should be converted back to HTML in Nova.");
            Assert.That(artifactDetailsAfter.Description.Contains("<p><img src=\"/svc/bpartifactstore/images/"),
                "The embedded image didn't get converted back to HTML!");
            Assert.That(artifactDetailsAfter.Description.Contains(textToAppend), "The new appended text didn't get saved properly!");
        }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Maintainability", "CA1506:AvoidExcessiveClassCoupling")]
        [TestCase]
        [TestRail(227236)]
        [Category(Categories.ArtifactStore)]
        [Description("Create an artifact and add an embedded image into one of it's Rich Text Custom properties (using Nova).  Append text to the Rich Text property and " +
                     "save the artifact (using OpenAPI).  Verify that the image is still embedded.")]
        public void Save_ArtifactWithImageInRichTextCustomProperty_AppendText_ImageIsStillEmbedded()
        {
            // Setup:
            _project.GetAllNovaArtifactTypes(Helper.ArtifactStore, _adminUser);

            const string artifactTypeName = "ST-User Story";
            const string propertyName = "ST-Acceptance Criteria";

            var artifact = Helper.CreateWrapAndPublishNovaArtifact(_project, _authorUser, Model.ArtifactModel.Enums.ItemTypePredefined.TextualRequirement,
                artifactTypeName: artifactTypeName);
            artifact.Lock(_authorUser);

            const int numberOfImagesToAdd = 1;
            var artifactDetails = ArtifactStoreHelper.AddRandomImageToArtifactProperty(_authorUser, Helper.ArtifactStore, artifact.Id, 
                propertyName: propertyName, numberOfImagesToAdd: numberOfImagesToAdd);

            var openApiArtifact = OpenApi.GetArtifact(Helper.BlueprintServer.Address, _project, artifact.Id, _authorUser);
            var openApiProperty = openApiArtifact.Properties.Find(p => p.Name == propertyName);
            var novaProperty = artifactDetails.CustomPropertyValues.Find(p => p.Name == propertyName);

            var images = VerifyImagesAreEmbeddedInArtifactAndGetImageIds((string)novaProperty.CustomPropertyValue, openApiProperty, numberOfImagesToAdd);

            // Append some text to the Custom Property.
            const string textToAppend = "<p>Appending some text here</p>";
            string imageTag = I18NHelper.FormatInvariant("<p>[Image = {0}]</p>", images[0]);
            openApiProperty.TextOrChoiceValue = openApiProperty.TextOrChoiceValue.Replace(imageTag, imageTag + textToAppend);

            // Execute:
            var propertiesToUpdate = new List<OpenApiPropertyForUpdate>();
            propertiesToUpdate.Add(new OpenApiPropertyForUpdate
            {
                PropertyTypeId = openApiProperty.PropertyTypeId,
                TextOrChoiceValue = openApiProperty.TextOrChoiceValue
            });

            Assert.DoesNotThrow(() => OpenApiArtifact.UpdateArtifact(openApiArtifact, _authorUser, propertiesToUpdate),
                "OpenAPI Save method shouldn't fail.");

            // Verify:
            var artifactDetailsAfter = Helper.ArtifactStore.GetArtifactDetails(_authorUser, artifact.Id);
            var novaPropertyAfter = artifactDetailsAfter.CustomPropertyValues.Find(p => p.Name == propertyName);
            var novaPropertyValueAfter = (string) novaPropertyAfter.CustomPropertyValue;

            Assert.AreNotEqual(novaPropertyValueAfter, openApiProperty.TextOrChoiceValue,
                "The Nova artifact '{0}' property should be different than in OpenAPI because the [Image = ID] tag should be converted back to HTML in Nova.",
                propertyName);
            Assert.That(novaPropertyValueAfter.Contains("<p><img src=\"/svc/bpartifactstore/images/"),
                "The embedded image didn't get converted back to HTML!");
            Assert.That(novaPropertyValueAfter.Contains(textToAppend), "The new appended text didn't get saved properly!");
        }

        [TestCase]
        [TestRail(227119)]
        [Category(Categories.ArtifactStore)]
        [Description("Create an artifact and add two embedded images into one of it's Rich Text properties (using Nova).  Swap the image IDs in the Rich Text " +
                     "property and save the artifact (using OpenAPI).  Verify that the images are still embedded and their positions are swapped.")]
        public void Save_ArtifactWithTwoImagesInRichTextProperty_SwapImages_ImagesAreStillEmbeddedAndSwapped()
        {
            // Setup:
            var artifact = Helper.CreateAndPublishArtifact(_project, _authorUser, BaseArtifactType.Process);
            artifact.Lock(_authorUser);

            const int numberOfImagesToAdd = 2;
            var artifactDetails = ArtifactStoreHelper.AddRandomImageToArtifactProperty(_authorUser, Helper.ArtifactStore, artifact.Id,
                numberOfImagesToAdd: numberOfImagesToAdd);

            var openApiArtifact = OpenApi.GetArtifact(Helper.BlueprintServer.Address, _project, artifact.Id, _authorUser);
            var description = openApiArtifact.Properties.Find(p => p.Name == nameof(NovaArtifactDetails.Description));

            var imageIds = VerifyImagesAreEmbeddedInArtifactAndGetImageIds(artifactDetails.Description, description, numberOfImagesToAdd);

            // Swap the images.
            const string tempValue = "### Temp string ###";
            description.TextOrChoiceValue = description.TextOrChoiceValue.Replace(imageIds[0], tempValue);
            description.TextOrChoiceValue = description.TextOrChoiceValue.Replace(imageIds[1], imageIds[0]);
            description.TextOrChoiceValue = description.TextOrChoiceValue.Replace(tempValue, imageIds[1]);

            // Execute:
            Assert.DoesNotThrow(() => OpenApiArtifact.UpdateArtifactDescription(openApiArtifact, _authorUser, updateWithRandomDescription: false),
                "OpenAPI Save method shouldn't fail.");

            // Verify:
            var artifactDetailsAfter = Helper.ArtifactStore.GetArtifactDetails(_authorUser, artifact.Id);

            Assert.AreNotEqual(artifactDetailsAfter.Description, description.TextOrChoiceValue,
                "The Nova artifact description should be different than in OpenAPI because the [Image = ID] tag should be converted back to HTML in Nova.");
            Assert.That(artifactDetailsAfter.Description.Contains("<p><img src=\"/svc/bpartifactstore/images/"),
                "The embedded image didn't get converted back to HTML!");

            var newImageIds = GetImageIdsFromHtml(artifactDetailsAfter.Description);
            string allImageIdsBefore = string.Join("#", imageIds);
            string allImageIdsAfter = string.Join("#", newImageIds.Reverse<string>());  // Reverse the order of the new image IDs to match the original order.

            Assert.AreEqual(allImageIdsBefore, allImageIdsAfter, "The order of the images didn't get reversed!");
        }

        [TestCase]
        [TestRail(227121)]
        [Category(Categories.ArtifactStore)]
        [Description("Create 2 artifacts and add an embedded image into a Rich Text property of each artifact (using Nova).  Swap the image IDs in the Rich Text property " +
                     "between the 2 artifacts and save the artifact (using OpenAPI).  Verify that the images are NOT embedded (they are converted to plain text), " +
                     "but their positions are swapped.")]
        public void Save_TwoArtifactsWithImagesInRichTextProperty_SwapImagesBetweenArtifacts_ImagesAreNotEmbeddedButAreSwapped()
        {
            // Setup:
            var artifact1 = Helper.CreateAndPublishArtifact(_project, _authorUser, BaseArtifactType.Process);
            var artifact2 = Helper.CreateAndPublishArtifact(_project, _authorUser, BaseArtifactType.TextualRequirement);
            artifact1.Lock(_authorUser);
            artifact2.Lock(_authorUser);

            const int numberOfImagesToAdd = 1;
            var artifact1Details = ArtifactStoreHelper.AddRandomImageToArtifactProperty(_authorUser, Helper.ArtifactStore, artifact1.Id,
                numberOfImagesToAdd: numberOfImagesToAdd);
            var artifact2Details = ArtifactStoreHelper.AddRandomImageToArtifactProperty(_authorUser, Helper.ArtifactStore, artifact2.Id,
                numberOfImagesToAdd: numberOfImagesToAdd);

            var openApiArtifact1 = OpenApi.GetArtifact(Helper.BlueprintServer.Address, _project, artifact1.Id, _authorUser);
            var openApiArtifact2 = OpenApi.GetArtifact(Helper.BlueprintServer.Address, _project, artifact2.Id, _authorUser);
            var description1 = openApiArtifact1.Properties.Find(p => p.Name == nameof(NovaArtifactDetails.Description));
            var description2 = openApiArtifact2.Properties.Find(p => p.Name == nameof(NovaArtifactDetails.Description));

            var artifact1ImageIds = VerifyImagesAreEmbeddedInArtifactAndGetImageIds(artifact1Details.Description, description1, numberOfImagesToAdd);
            var artifact2ImageIds = VerifyImagesAreEmbeddedInArtifactAndGetImageIds(artifact2Details.Description, description2, numberOfImagesToAdd);

            // Swap the images.
            description1.TextOrChoiceValue = description1.TextOrChoiceValue.Replace(artifact1ImageIds[0], artifact2ImageIds[0]);
            description2.TextOrChoiceValue = description2.TextOrChoiceValue.Replace(artifact2ImageIds[0], artifact1ImageIds[0]);

            // Execute:
            Assert.DoesNotThrow(() =>
            {
                OpenApiArtifact.UpdateArtifactDescription(openApiArtifact1, _authorUser, updateWithRandomDescription: false);
                OpenApiArtifact.UpdateArtifactDescription(openApiArtifact2, _authorUser, updateWithRandomDescription: false);
            },
                "OpenAPI Save method shouldn't fail.");

            // Verify:
            var artifact1DetailsAfter = Helper.ArtifactStore.GetArtifactDetails(_authorUser, artifact1.Id);
            var artifact2DetailsAfter = Helper.ArtifactStore.GetArtifactDetails(_authorUser, artifact2.Id);

            Assert.IsFalse(artifact1DetailsAfter.Description.Contains("<p><img src=\"/svc/bpartifactstore/images/"),
                "The embedded image shouldn't get converted back to HTML if swapped between artifacts!");
            Assert.IsFalse(artifact2DetailsAfter.Description.Contains("<p><img src=\"/svc/bpartifactstore/images/"),
                "The embedded image shouldn't get converted back to HTML if swapped between artifacts!");

            string expectedImageTag = I18NHelper.FormatInvariant("[Image = {0}]", artifact2ImageIds[0]);
            Assert.That(artifact1DetailsAfter.Description.Contains(expectedImageTag), "The [Image = ID] tag didn't get saved properly!");

            expectedImageTag = I18NHelper.FormatInvariant("[Image = {0}]", artifact1ImageIds[0]);
            Assert.That(artifact2DetailsAfter.Description.Contains(expectedImageTag), "The [Image = ID] tag didn't get saved properly!");
        }

        [TestCase]
        [TestRail(227202)]
        [Category(Categories.ArtifactStore)]
        [Description("Create an artifact and add text and an embedded image into one of it's Rich Text properties (using Nova).  Remove the [Image] tag from the Rich Text " +
                     "property and save the artifact (using OpenAPI).  Verify that the image is removed in the Nova artifact.")]
        public void Save_ArtifactWithImageInRichTextProperty_RemoveImage_ImageIsRemoved()
        {
            // Setup:
            var artifact = Helper.CreateAndPublishArtifact(_project, _authorUser, BaseArtifactType.Process);
            artifact.Lock(_authorUser);

            const int numberOfImagesToAdd = 1;
            var artifactDetails = ArtifactStoreHelper.AddRandomImageToArtifactProperty(_authorUser, Helper.ArtifactStore, artifact.Id,
                numberOfImagesToAdd: numberOfImagesToAdd);

            // Add some text after the image.
            const string textToAppend = "<p>Appending some text here</p>";
            artifactDetails.Description = artifactDetails.Description.Replace("</html>", textToAppend + "</html>");
            Helper.ArtifactStore.UpdateArtifact(_authorUser, artifactDetails as NovaArtifactDetails);

            var openApiArtifact = OpenApi.GetArtifact(Helper.BlueprintServer.Address, _project, artifact.Id, _authorUser);
            var description = openApiArtifact.Properties.Find(p => p.Name == nameof(NovaArtifactDetails.Description));

            var imageIds = VerifyImagesAreEmbeddedInArtifactAndGetImageIds(artifactDetails.Description, description, numberOfImagesToAdd);

            // Delete the [Image] tag.
            string imageTag = I18NHelper.FormatInvariant("[Image = {0}]", imageIds[0]);
            Assert.That(description.TextOrChoiceValue.Contains(imageTag), "Couldn't find '{0}' in the description!", imageTag);
            description.TextOrChoiceValue = description.TextOrChoiceValue.Replace(imageTag, string.Empty);

            // Execute:
            Assert.DoesNotThrow(() => OpenApiArtifact.UpdateArtifactDescription(openApiArtifact, _authorUser, updateWithRandomDescription: false),
                "OpenAPI Save method shouldn't fail.");

            // Verify:
            var artifactDetailsAfter = Helper.ArtifactStore.GetArtifactDetails(_authorUser, artifact.Id);

            Assert.That(artifactDetailsAfter.Description.Contains(textToAppend),
                "The appended text wasn't found in the description!");
            Assert.IsFalse(artifactDetailsAfter.Description.Contains(imageIds[0]),
                "The embedded image ID was found in the description, even though we removed it!");
        }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Maintainability", "CA1506:AvoidExcessiveClassCoupling")]
        [TestCase]
        [TestRail(227233)]
        [Category(Categories.ArtifactStore)]
        [Description("Create an artifact that has at least 2 multi-line Rich Text fields and add an embedded image into one of them (using Nova).  Copy the [Image = ID] " +
                     "to another multi-line Rich Text field and save the artifact (using OpenAPI).  Verify that the image is only embedded in the original Rich Text " +
                     "field and remains in plain text in the 2nd Rich Text field.")]
        public void Save_ArtifactWithImageInRichTextProperty_ImageCopiedToAnotherRichTextField_ImageIsEmbeddedInOriginalRichTextFieldOnly()
        {
            // Setup:
            _project.GetAllNovaArtifactTypes(Helper.ArtifactStore, _adminUser);

            // The 'ST-Title' property of 'ST-User Story' is the oly single-line Rich Text property.
            const string artifactTypeName = "ST-User Story";
            const string multiLineRTProperty = "ST-Acceptance Criteria";

            var artifact = Helper.CreateWrapAndPublishNovaArtifact(_project, _authorUser, Model.ArtifactModel.Enums.ItemTypePredefined.TextualRequirement,
                artifactTypeName: artifactTypeName);
            artifact.Lock(_authorUser);

            const int numberOfImagesToAdd = 1;
            var artifactDetails = ArtifactStoreHelper.AddRandomImageToArtifactProperty(_authorUser, Helper.ArtifactStore, artifact.Id,
                numberOfImagesToAdd: numberOfImagesToAdd);

            var openApiArtifact = OpenApi.GetArtifact(Helper.BlueprintServer.Address, _project, artifact.Id, _authorUser);
            var description = openApiArtifact.Properties.Find(p => p.Name == nameof(NovaArtifactDetails.Description));
            var otherRichTextProperty = openApiArtifact.Properties.Find(p => p.Name == multiLineRTProperty);

            // Verify that the properties we found are different.
            Assert.AreNotEqual(description.PropertyTypeId, otherRichTextProperty.PropertyTypeId,
                "The PropertyTypeId of the two properties should be different!");

            var imageIds = VerifyImagesAreEmbeddedInArtifactAndGetImageIds(artifactDetails.Description, description, numberOfImagesToAdd);

            // Copy Description (and image) to another Rich Text property.
            otherRichTextProperty.TextOrChoiceValue = description.TextOrChoiceValue;

            // Execute:
            var propertiesToUpdate = new List<OpenApiPropertyForUpdate>();
            propertiesToUpdate.Add(new OpenApiPropertyForUpdate
            {
                PropertyTypeId = description.PropertyTypeId,
                TextOrChoiceValue = description.TextOrChoiceValue
            });
            propertiesToUpdate.Add(new OpenApiPropertyForUpdate
            {
                PropertyTypeId = otherRichTextProperty.PropertyTypeId,
                TextOrChoiceValue = otherRichTextProperty.TextOrChoiceValue
            });

            Assert.DoesNotThrow(() => OpenApiArtifact.UpdateArtifact(openApiArtifact, _authorUser, propertiesToUpdate),
                "OpenAPI Save method shouldn't fail.");

            // Verify:
            var artifactDetailsAfter = Helper.ArtifactStore.GetArtifactDetails(_authorUser, artifact.Id);

            Assert.AreNotEqual(artifactDetailsAfter.Description, description.TextOrChoiceValue,
                "The Nova artifact description should be different than in OpenAPI because the [Image = ID] tag should be converted back to HTML in Nova.");
            Assert.That(artifactDetailsAfter.Description.Contains("<p><img src=\"/svc/bpartifactstore/images/"),
                "The embedded image didn't get converted back to HTML!");
            Assert.That(artifactDetailsAfter.Description.Contains(imageIds[0]), "The image ID wasn't found in the Description!");

            // Verify the other property didn't get its [Image = ID] converted to HTML.
            var otherNovaRichTextProperty = artifactDetailsAfter.CustomPropertyValues.Find(p => p.Name == multiLineRTProperty);
            string otherNovaRichTextPropertyValue = (string)otherNovaRichTextProperty.CustomPropertyValue;

            Assert.AreNotEqual(artifactDetailsAfter.Description, otherNovaRichTextPropertyValue,
                "The 'Description' and '{0}' properties should be different!", multiLineRTProperty);

            Assert.IsFalse(otherNovaRichTextPropertyValue.Contains("<p><img src=\"/svc/bpartifactstore/images/"),
                "The embedded image shouldn't get converted back to HTML if copied to other properties in the same artifact!");

            string expectedImageTag = I18NHelper.FormatInvariant("[Image = {0}]", imageIds[0]);
            Assert.That(otherNovaRichTextPropertyValue.Contains(expectedImageTag), "The '{0}' property should contain {1}!",
                multiLineRTProperty, expectedImageTag);
        }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Maintainability", "CA1506:AvoidExcessiveClassCoupling")]
        [TestCase]
        [TestRail(227234)]
        [Category(Categories.ArtifactStore)]
        [Description("Create an artifact that has at least 2 Rich Text fields (1 multi-line and 1 single-line) and add an embedded image into the multi-line property (using Nova).  " +
                     "Copy the [Image = ID] from the multi-line Rich Text property to the single-line Rich Text field and save the artifact (using OpenAPI).  " +
                     "Verify that the image is embedded in the multi-line Rich Text property but plain text in the single-line Rich Text property.")]
        public void Save_ArtifactWithImageInRichTextProperty_ImageCopiedToAnotherSingleLineRichTextField_ImageIsPlainTextInSingleLineProperty()
        {
            // Setup:
            _project.GetAllNovaArtifactTypes(Helper.ArtifactStore, _adminUser);

            // The 'ST-Title' property of 'ST-User Story' is the oly single-line Rich Text property.
            const string artifactTypeName = "ST-User Story";
            const string singleLineRTProperty = "ST-Title";

            var artifact = Helper.CreateWrapAndPublishNovaArtifact(_project, _authorUser, Model.ArtifactModel.Enums.ItemTypePredefined.TextualRequirement,
                artifactTypeName: artifactTypeName);
            artifact.Lock(_authorUser);

            const int numberOfImagesToAdd = 1;
            var artifactDetails = ArtifactStoreHelper.AddRandomImageToArtifactProperty(_authorUser, Helper.ArtifactStore, artifact.Id,
                numberOfImagesToAdd: numberOfImagesToAdd);

            var openApiArtifact = OpenApi.GetArtifact(Helper.BlueprintServer.Address, _project, artifact.Id, _authorUser);
            var description = openApiArtifact.Properties.Find(p => p.Name == nameof(NovaArtifactDetails.Description));
            var otherRichTextProperty = openApiArtifact.Properties.Find(p => p.Name == singleLineRTProperty);

            // Verify that the properties we found are different.
            Assert.AreNotEqual(description.PropertyTypeId, otherRichTextProperty.PropertyTypeId,
                "The PropertyTypeId of the two properties should be different!");

            var imageIds = VerifyImagesAreEmbeddedInArtifactAndGetImageIds(artifactDetails.Description, description, numberOfImagesToAdd);

            // Copy Description (and image) to another Rich Text property.
            otherRichTextProperty.TextOrChoiceValue = description.TextOrChoiceValue;

            // Execute:
            var propertiesToUpdate = new List<OpenApiPropertyForUpdate>();
            propertiesToUpdate.Add(new OpenApiPropertyForUpdate
            {
                PropertyTypeId = description.PropertyTypeId,
                TextOrChoiceValue = description.TextOrChoiceValue
            });
            propertiesToUpdate.Add(new OpenApiPropertyForUpdate
            {
                PropertyTypeId = otherRichTextProperty.PropertyTypeId,
                TextOrChoiceValue = otherRichTextProperty.TextOrChoiceValue
            });

            Assert.DoesNotThrow(() => OpenApiArtifact.UpdateArtifact(openApiArtifact, _authorUser, propertiesToUpdate),
                "OpenAPI Save method shouldn't fail.");

            // Verify:
            var artifactDetailsAfter = Helper.ArtifactStore.GetArtifactDetails(_authorUser, artifact.Id);

            Assert.AreNotEqual(artifactDetailsAfter.Description, description.TextOrChoiceValue,
                "The Nova artifact description should be different than in OpenAPI because the [Image = ID] tag should be converted back to HTML in Nova.");
            Assert.That(artifactDetailsAfter.Description.Contains("<p><img src=\"/svc/bpartifactstore/images/"),
                "The embedded image didn't get converted back to HTML!");
            Assert.That(artifactDetailsAfter.Description.Contains(imageIds[0]), "The image ID wasn't found in the Description!");

            // Verify the other property didn't get its [Image = ID] converted to HTML.
            var otherNovaRichTextProperty = artifactDetailsAfter.CustomPropertyValues.Find(p => p.Name == singleLineRTProperty);
            string otherNovaRichTextPropertyValue = (string) otherNovaRichTextProperty.CustomPropertyValue;

            Assert.AreNotEqual(artifactDetailsAfter.Description, otherNovaRichTextPropertyValue,
                "The 'Description' and '{0}' properties should be different!", singleLineRTProperty);

            Assert.IsFalse(otherNovaRichTextPropertyValue.Contains("<p><img src=\"/svc/bpartifactstore/images/"),
                "The embedded image shouldn't get converted back to HTML if copied to other properties in the same artifact!");

            string expectedImageTag = I18NHelper.FormatInvariant("[Image = {0}]", imageIds[0]);
            Assert.That(otherNovaRichTextPropertyValue.Contains(expectedImageTag), "The '{0}' property should contain {1}!",
                singleLineRTProperty, expectedImageTag);
        }

        // TODO: Add test that embeds an image, then delete image from FileStore, then get artifact with OpenAPI and verify that image is removed.

        #endregion Save artifact with image tests

        #region Private functions

        /// <summary>
        /// Gets all the Image IDs found in the HTML code.
        /// </summary>
        /// <param name="htmlCode">The HTML string.</param>
        /// <returns>A list of Image IDs that were found.</returns>
        private static List<string> GetImageIdsFromHtml(string htmlCode)
        {
            ThrowIf.ArgumentNull(htmlCode, nameof(htmlCode));

            string pattern = I18NHelper.FormatInvariant(@"<img src=""/{0}/([A-Za-z0-9-]+)""", RestPaths.Svc.ArtifactStore.IMAGES);

            // Get all the Image IDs found in the regex search.
            var matches = Regex.Matches(htmlCode, pattern);
            return (from Match match in matches select match.Groups[1].Value).ToList();
        }

        /// <summary>
        /// Verifies that the specified number of images were added to the Nova artifact and that they were converted to [Image = ID] tags in OpenAPI.
        /// Then it returns a list of Image IDs that exist in the property (in the order they were found).
        /// </summary>
        /// <param name="novaArtifactPropertyValue">The contents of the property from a Nova call.</param>
        /// <param name="openApiProperty">The OpenAPI property.</param>
        /// <param name="numberOfImagesAdded">The number of images that were added.</param>
        /// <returns>A list of Image IDs that exist in the property (in the order they were found).</returns>
        private static List<string> VerifyImagesAreEmbeddedInArtifactAndGetImageIds(string novaArtifactPropertyValue,
            OpenApiProperty openApiProperty,
            int numberOfImagesAdded)
        {
            Assert.AreNotEqual(novaArtifactPropertyValue, openApiProperty.TextOrChoiceValue,
                "The Nova artifact {0} should be different than in OpenAPI because the Image HTML tag should be converted to plain text in OpenAPI.",
                openApiProperty.Name);

            var imageIds = GetImageIdsFromHtml(novaArtifactPropertyValue);

            Assert.AreEqual(numberOfImagesAdded, imageIds.Count, "There should be {0} <img> tags found in the Nova artifact {1} property!",
                numberOfImagesAdded, openApiProperty.Name);
            Assert.That(openApiProperty.TextOrChoiceValue.Contains("[Image ="),
                "The embedded image tag should be converted to [Image = {guid}] in OpenAPI!");
            Assert.IsFalse(openApiProperty.TextOrChoiceValue.Contains("<img src"),
                "There should be no '<img>' HTML tags in the {0} property in OpenAPI!", openApiProperty.Name);

            return imageIds;
        }

        #endregion Private functions
    }
}
