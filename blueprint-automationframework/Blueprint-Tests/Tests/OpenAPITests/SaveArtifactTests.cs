using Common;
using CustomAttributes;
using Helper;
using Model;
using Model.ArtifactModel;
using Model.ArtifactModel.Impl;
using Model.Factories;
using NUnit.Framework;
using TestCommon;

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

            var artifactDetails = ArtifactStoreHelper.AddRandomImageToArtifactProperty(artifact, _authorUser, Helper.ArtifactStore);

            var openApiArtifact = OpenApiArtifact.GetArtifact(Helper.BlueprintServer.Address, _project, artifact.Id, _authorUser);
            var description = openApiArtifact.Properties.Find(p => p.Name == "Description");

            Assert.AreNotEqual(artifactDetails.Description, description.TextOrChoiceValue,
                "The Nova artifact description should be different than in OpenAPI because the Image HTML tag should be converted to plain text in OpenAPI.");
            Assert.That(description.TextOrChoiceValue.Contains("[Image ="),
                "The embedded image tag should be converted to [Image = {guid}] in OpenAPI!");

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

        #endregion Save artifact with image tests
    }
}
