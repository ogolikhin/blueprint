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
            var artifactDetails = ArtifactStoreHelper.AddRandomImageToArtifactProperty(artifact, _authorUser, Helper.ArtifactStore, numberOfImagesToAdd: numberOfImagesToAdd);

            var openApiArtifact = OpenApiArtifact.GetArtifact(Helper.BlueprintServer.Address, _project, artifact.Id, _authorUser);
            var description = openApiArtifact.Properties.Find(p => p.Name == nameof(NovaArtifactDetails.Description));

            VerifyImagesAreEmbeddedInArtifactAndGetImageIds(artifactDetails.Description, description, numberOfImagesToAdd);

            // Append some text to the Description.
            const string textToAppend = "<p>Appending some text here</p>";
            description.TextOrChoiceValue = I18NHelper.FormatInvariant("{0}{1}", description.TextOrChoiceValue, textToAppend);

            // Execute:
            Assert.DoesNotThrow(() => OpenApiArtifact.UpdateArtifact(openApiArtifact, _authorUser, updateWithRandomDescription: false),
                "OpenAPI Save method shouldn't fail.");

            // Verify:
            var artifactDetailsAfter = Helper.ArtifactStore.GetArtifactDetails(_authorUser, artifact.Id);

            Assert.AreNotEqual(artifactDetailsAfter.Description, description.TextOrChoiceValue,
                "The Nova artifact description should be different than in OpenAPI because the [Image = ID] tag should be converted back to HTML in Nova.");
            Assert.That(artifactDetailsAfter.Description.Contains("<p><img src=\"/svc/bpartifactstore/images/"),
                "The embedded image didn't get converted back to HTML!");
            Assert.That(artifactDetailsAfter.Description.Contains(textToAppend), "The new appended text didn't get saved properly!");
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
            var artifactDetails = ArtifactStoreHelper.AddRandomImageToArtifactProperty(artifact, _authorUser, Helper.ArtifactStore, numberOfImagesToAdd: numberOfImagesToAdd);

            var openApiArtifact = OpenApiArtifact.GetArtifact(Helper.BlueprintServer.Address, _project, artifact.Id, _authorUser);
            var description = openApiArtifact.Properties.Find(p => p.Name == nameof(NovaArtifactDetails.Description));

            var imageIds = VerifyImagesAreEmbeddedInArtifactAndGetImageIds(artifactDetails.Description, description, numberOfImagesToAdd);

            // Swap the images.
            const string tempValue = "### Temp string ###";
            description.TextOrChoiceValue = description.TextOrChoiceValue.Replace(imageIds[0], tempValue);
            description.TextOrChoiceValue = description.TextOrChoiceValue.Replace(imageIds[1], imageIds[0]);
            description.TextOrChoiceValue = description.TextOrChoiceValue.Replace(tempValue, imageIds[1]);

            // Execute:
            Assert.DoesNotThrow(() => OpenApiArtifact.UpdateArtifact(openApiArtifact, _authorUser, updateWithRandomDescription: false),
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
            var artifact1Details = ArtifactStoreHelper.AddRandomImageToArtifactProperty(artifact1, _authorUser, Helper.ArtifactStore, numberOfImagesToAdd: numberOfImagesToAdd);
            var artifact2Details = ArtifactStoreHelper.AddRandomImageToArtifactProperty(artifact2, _authorUser, Helper.ArtifactStore, numberOfImagesToAdd: numberOfImagesToAdd);

            var openApiArtifact1 = OpenApiArtifact.GetArtifact(Helper.BlueprintServer.Address, _project, artifact1.Id, _authorUser);
            var openApiArtifact2 = OpenApiArtifact.GetArtifact(Helper.BlueprintServer.Address, _project, artifact2.Id, _authorUser);
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
                OpenApiArtifact.UpdateArtifact(openApiArtifact1, _authorUser, updateWithRandomDescription: false);
                OpenApiArtifact.UpdateArtifact(openApiArtifact2, _authorUser, updateWithRandomDescription: false);
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
            var artifactDetails = ArtifactStoreHelper.AddRandomImageToArtifactProperty(artifact, _authorUser, Helper.ArtifactStore, numberOfImagesToAdd: numberOfImagesToAdd);

            // Add some text after the image.
            const string textToAppend = "<p>Appending some text here</p>";
            artifactDetails.Description = artifactDetails.Description.Replace("</html>", textToAppend + "</html>");
            Helper.ArtifactStore.UpdateArtifact(_authorUser, _project, artifactDetails as NovaArtifactDetails);

            var openApiArtifact = OpenApiArtifact.GetArtifact(Helper.BlueprintServer.Address, _project, artifact.Id, _authorUser);
            var description = openApiArtifact.Properties.Find(p => p.Name == nameof(NovaArtifactDetails.Description));

            var imageIds = VerifyImagesAreEmbeddedInArtifactAndGetImageIds(artifactDetails.Description, description, numberOfImagesToAdd);

            // Delete the [Image] tag.
            string imageTag = I18NHelper.FormatInvariant("[Image = {0}]", imageIds[0]);
            Assert.That(description.TextOrChoiceValue.Contains(imageTag), "Couldn't find '{0}' in the description!", imageTag);
            description.TextOrChoiceValue = description.TextOrChoiceValue.Replace(imageTag, string.Empty);

            // Execute:
            Assert.DoesNotThrow(() => OpenApiArtifact.UpdateArtifact(openApiArtifact, _authorUser, updateWithRandomDescription: false),
                "OpenAPI Save method shouldn't fail.");

            // Verify:
            var artifactDetailsAfter = Helper.ArtifactStore.GetArtifactDetails(_authorUser, artifact.Id);

            Assert.That(artifactDetailsAfter.Description.Contains(textToAppend),
                "The appended text wasn't found in the description!");
            Assert.IsFalse(artifactDetailsAfter.Description.Contains(imageIds[0]),
                "The embedded image ID was found in the description, even though we removed it!");
        }

        // TODO: Add a test where an image is copied to another Rich Text field.
        // TODO: Add a test where an image is copied to a single line Rich Text field.  How should that behave?

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
