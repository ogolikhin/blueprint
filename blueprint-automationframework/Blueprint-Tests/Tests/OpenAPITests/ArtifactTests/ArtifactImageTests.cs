using System;
using System.Linq;
using Common;
using CustomAttributes;
using Helper;
using Model;
using Model.ArtifactModel;
using Model.ArtifactModel.Enums;
using Model.Factories;
using NUnit.Framework;
using TestCommon;
using Utilities;

namespace OpenAPITests.ArtifactTests
{
    [TestFixture]
    [Category(Categories.OpenApi)]
    public class ArtifactImageTests : TestBase
    {
        private IUser _adminUser = null;
        private IProject _project = null;

        private const string REST_PATH = RestPaths.OpenApi.Projects_id_.Artifacts_id_.IMAGE;
        private const string ARTIFACT_NOT_FOUND = "The requested artifact is not found.";

        [SetUp]
        public void SetUp()
        {
            Helper = new TestHelper();
            _adminUser = Helper.CreateUserAndAuthenticate(TestHelper.AuthenticationTokenTypes.BothAccessControlAndOpenApiTokens);
            _project = ProjectFactory.GetProject(_adminUser);
        }

        [TearDown]
        public void TearDown()
        {
            Helper?.Dispose();
        }

        #region 200 OK tests

        [Category(Categories.CustomData)]
        [Category(Categories.GoldenData)]
        [TestCase(ItemTypePredefined.BusinessProcess, 33)]
        [TestCase(ItemTypePredefined.DomainDiagram, 31)]
        [TestCase(ItemTypePredefined.GenericDiagram, 49)]
        [TestCase(ItemTypePredefined.Process, 34)]
        [TestCase(ItemTypePredefined.Storyboard, 32)]
        [TestCase(ItemTypePredefined.UIMockup, 22)]
        [TestCase(ItemTypePredefined.UseCase, 17)]
        [TestCase(ItemTypePredefined.UseCaseDiagram, 29)]
        [TestRail(305017)]
        [Description("Create & publish a Diagram artifact.  Get the artifact image with OpenAPI.  Verify the image is returned in PNG format.")]
        public void GetArtifactImage_PublishedDiagramImage_ImageIsReturned(ItemTypePredefined itemType, int artifactId)
        {
            // Setup:
            var customDataProject = ArtifactStoreHelper.GetCustomDataProject(_adminUser);
            var viewerUser = Helper.CreateUserWithProjectRolePermissions(TestHelper.ProjectRole.Viewer, customDataProject);
            var artifact = Helper.ArtifactStore.GetArtifactDetails(viewerUser, artifactId);

            Assert.AreEqual((int)itemType, artifact.PredefinedType, "The predefined type of the Golden Data artifact is wrong.");

            // Execute:
            IFile file = null;

            Assert.DoesNotThrow(() => file = Helper.OpenApi.GetArtifactImage(viewerUser, customDataProject.Id, artifact.Id),
                "'GET {0}' should return 200 OK when valid parameters are passed!", REST_PATH);

            // Verify:
            ValidateReturnedImage(artifact.Name, file);
        }

        [TestCase(BaseArtifactType.BusinessProcess)]
        [TestCase(BaseArtifactType.DomainDiagram)]
        [TestCase(BaseArtifactType.GenericDiagram)]
        [TestCase(BaseArtifactType.Process)]
        [TestCase(BaseArtifactType.Storyboard)]
        [TestCase(BaseArtifactType.UIMockup)]
        [TestCase(BaseArtifactType.UseCase)]
        [TestCase(BaseArtifactType.UseCaseDiagram)]
        [TestRail(305020)]
        [Description("Create an unpublished Diagram artifact.  Get the artifact image with OpenAPI.  Verify the image is returned in PNG format.")]
        public void GetArtifactImage_SavedDiagramImage_ImageIsReturned(BaseArtifactType artifactType)
        {
            // Setup:
            var artifact = Helper.CreateAndSaveOpenApiArtifact(_project, _adminUser, artifactType);

            // Execute:
            IFile file = null;

            Assert.DoesNotThrow(() => file = Helper.OpenApi.GetArtifactImage(_adminUser, _project.Id, artifact.Id),
                "'GET {0}' should return 200 OK when valid parameters are passed!", REST_PATH);

            // Verify:
            ValidateReturnedImage(artifact.Name, file);
        }

        #endregion 200 OK tests

        #region 401 Unauthorized tests

        [TestCase(null)]
        [TestCase(CommonConstants.InvalidToken)]
        [TestRail(305026)]
        [Description("Create & publish a Diagram artifact.  Try to get the artifact image with OpenAPI with a user that has an invalid or missing token.  " +
                     "Verify it returns 404 Not Found.")]
        public void GetArtifactImage_BadOrMissingToken_401Unauthorized(string token)
        {
            // Setup:
            var artifact = Helper.CreateAndPublishNovaArtifact(_adminUser, _project, ItemTypePredefined.Process);
            var userWithBadToken = Helper.CreateUserWithInvalidToken(TestHelper.AuthenticationTokenTypes.OpenApiToken, badToken: token);

            // Execute:
            var ex = Assert.Throws<Http401UnauthorizedException>(() => Helper.OpenApi.GetArtifactImage(userWithBadToken, _project.Id, artifact.Id),
                "'GET {0}' should return 401 Unauthorized when called with a bad or missing token header!", REST_PATH);

            // Verify:
            TestHelper.ValidateBodyContents(ex.RestResponse, "Unauthorized call.");
        }

        #endregion 401 Unauthorized tests

        #region 404 Not Found tests

        [TestCase(ItemTypePredefined.Actor)]
        [TestCase(ItemTypePredefined.Document)]
        [TestCase(ItemTypePredefined.Glossary)]
        [TestCase(ItemTypePredefined.PrimitiveFolder)]
        [TestCase(ItemTypePredefined.TextualRequirement)]
        [TestRail(305019)]
        [Description("Create an artifact that is NOT a Diagram.  Try to get the artifact image with OpenAPI.  Verify it returns 404 Not Found.")]
        public void GetArtifactImage_NonDiagramArtifact_404NotFound(ItemTypePredefined itemType)
        {
            // Setup:
            var viewerUser = Helper.CreateUserWithProjectRolePermissions(TestHelper.ProjectRole.Viewer, _project);
            var artifact = Helper.CreateAndPublishNovaArtifact(_adminUser, _project, itemType);

            // Execute:
            var ex = Assert.Throws<Http404NotFoundException>(() => Helper.OpenApi.GetArtifactImage(viewerUser, _project.Id, artifact.Id),
                "'GET {0}' should return 404 Not Found when a non-diagram artifact passed!", REST_PATH);

            // Verify:
            TestHelper.ValidateServiceErrorMessage(ex.RestResponse, "Image Is Not Supported For Specified Artifact");
        }

        [TestCase(BaselineAndCollectionTypePredefined.ArtifactBaseline)]
        [TestCase(BaselineAndCollectionTypePredefined.BaselineFolder)]
        [TestRail(305021)]
        [Description("Create a Baseline or Baseline Folder artifact.  Try to get the artifact image with OpenAPI.  Verify it returns 404 Not Found.")]
        public void GetArtifactImage_BaselineOrBaselineFolder_404NotFound(BaselineAndCollectionTypePredefined itemType)
        {
            // Setup:
            var artifact = Helper.CreateBaselineOrBaselineFolderOrReview(_adminUser, _project, itemType);

            // Execute:
            var ex = Assert.Throws<Http404NotFoundException>(() => Helper.OpenApi.GetArtifactImage(_adminUser, _project.Id, artifact.Id),
                "'GET {0}' should return 404 Not Found when a non-diagram artifact passed!", REST_PATH);

            // Verify:
            TestHelper.ValidateServiceErrorMessage(ex.RestResponse, ARTIFACT_NOT_FOUND);
        }

        [TestCase(BaselineAndCollectionTypePredefined.ArtifactCollection)]
        [TestCase(BaselineAndCollectionTypePredefined.CollectionFolder)]
        [TestRail(305022)]
        [Description("Create a Collection or Collection Folder artifact.  Try to get the artifact image with OpenAPI.  Verify it returns 404 Not Found.")]
        public void GetArtifactImage_CollectionOrCollectionFolder_404NotFound(BaselineAndCollectionTypePredefined itemType)
        {
            // Setup:
            var artifact = Helper.CreateCollectionOrCollectionFolder(_project, _adminUser, itemType);

            // Execute:
            var ex = Assert.Throws<Http404NotFoundException>(() => Helper.OpenApi.GetArtifactImage(_adminUser, _project.Id, artifact.Id),
                "'GET {0}' should return 404 Not Found when a non-diagram artifact passed!", REST_PATH);

            // Verify:
            TestHelper.ValidateServiceErrorMessage(ex.RestResponse, ARTIFACT_NOT_FOUND);
        }

        [TestCase]
        [TestRail(305023)]
        [Description("Try to get the image for a project OpenAPI.  Verify it returns 404 Not Found.")]
        public void GetArtifactImage_Project_404NotFound()
        {
            // Execute:
            var ex = Assert.Throws<Http404NotFoundException>(() => Helper.OpenApi.GetArtifactImage(_adminUser, _project.Id, _project.Id),
                "'GET {0}' should return 404 Not Found when a non-diagram artifact passed!", REST_PATH);

            // Verify:
            TestHelper.ValidateServiceErrorMessage(ex.RestResponse, ARTIFACT_NOT_FOUND);
        }

        [TestCase(int.MaxValue)]
        [TestRail(305024)]
        [Description("Try to get the image for a non-existing artifact ID in OpenAPI.  Verify it returns 404 Not Found.")]
        public void GetArtifactImage_NonExistingArtifactId_404NotFound(int artifactId)
        {
            // Execute:
            var ex = Assert.Throws<Http404NotFoundException>(() => Helper.OpenApi.GetArtifactImage(_adminUser, _project.Id, artifactId),
                "'GET {0}' should return 404 Not Found when a non-existing artifact ID is passed!", REST_PATH);

            // Verify:
            TestHelper.ValidateServiceErrorMessage(ex.RestResponse, ARTIFACT_NOT_FOUND);
        }

        [TestCase(int.MaxValue)]
        [TestRail(305029)]
        [Description("Create & publish a Diagram artifact.  Try to get the image for a non-existing project ID in OpenAPI.  Verify it returns 404 Not Found.")]
        public void GetArtifactImage_NonExistingProjectId_404NotFound(int projectId)
        {
            // Setup:
            var artifact = Helper.CreateAndPublishNovaArtifact(_adminUser, _project, ItemTypePredefined.Process);

            // Execute:
            var ex = Assert.Throws<Http404NotFoundException>(() => Helper.OpenApi.GetArtifactImage(_adminUser, projectId, artifact.Id),
                "'GET {0}' should return 404 Not Found when a non-existing artifact ID is passed!", REST_PATH);

            // Verify:
            TestHelper.ValidateServiceErrorMessage(ex.RestResponse, "The requested project is not found.");
        }

        [TestCase]
        [TestRail(305025)]
        [Description("Create a Diagram artifact in project1.  Try to get the artifact image from OpenAPI but pass project2.  Verify it returns 404 Not Found.")]
        public void GetArtifactImage_WrongProjectId_404NotFound()
        {
            // Setup:
            var projects = ProjectFactory.GetProjects(_adminUser, numberOfProjects: 2);
            var artifact = Helper.CreateAndPublishNovaArtifact(_adminUser, projects[0], ItemTypePredefined.Process);

            // Execute:
            var ex = Assert.Throws<Http404NotFoundException>(() => Helper.OpenApi.GetArtifactImage(_adminUser, projects[1].Id, artifact.Id),
                "'GET {0}' should return 404 Not Found when the wrong project ID is passed for an artifact ID!", REST_PATH);

            // Verify:
            TestHelper.ValidateServiceErrorMessage(ex.RestResponse, ARTIFACT_NOT_FOUND);
        }

        [TestCase]
        [TestRail(305027)]
        [Description("Create & publish a Diagram artifact.  Try to get the artifact image with OpenAPI using a user with no permission to the project.  " +
                     "Verify it returns 404 Not Found.")]
        public void GetArtifactImage_UserWithoutPermissionToProject_404NotFound()
        {
            // Setup:
            var artifact = Helper.CreateAndPublishNovaArtifact(_adminUser, _project, ItemTypePredefined.Process);
            var userWithNoPermission = Helper.CreateUserWithProjectRolePermissions(TestHelper.ProjectRole.None, _project);

            // Execute:
            var ex = Assert.Throws<Http404NotFoundException>(() => Helper.OpenApi.GetArtifactImage(userWithNoPermission, _project.Id, artifact.Id),
                "'GET {0}' should return 404 Not Found when the user has no permission to the project!", REST_PATH);

            // Verify:
            TestHelper.ValidateServiceErrorMessage(ex.RestResponse, ARTIFACT_NOT_FOUND);
        }

        [TestCase]
        [TestRail(305028)]
        [Description("Create & publish a Diagram artifact.  Try to get the artifact image with OpenAPI using a user with no permission to the artifact.  " +
                     "Verify it returns 404 Not Found.")]
        public void GetArtifactImage_UserWithoutPermissionToArtifact_404NotFound()
        {
            // Setup:
            var artifact = Helper.CreateAndPublishNovaArtifact(_adminUser, _project, ItemTypePredefined.Process);
            var userWithNoPermission = Helper.CreateUserWithProjectRolePermissions(TestHelper.ProjectRole.AuthorFullAccess, _project);
            Helper.AssignProjectRolePermissionsToUser(userWithNoPermission, TestHelper.ProjectRole.None, _project, artifact);

            // Execute:
            var ex = Assert.Throws<Http404NotFoundException>(() => Helper.OpenApi.GetArtifactImage(userWithNoPermission, _project.Id, artifact.Id),
                "'GET {0}' should return 404 Not Found when the user has no permission to the artifact!", REST_PATH);

            // Verify:
            TestHelper.ValidateServiceErrorMessage(ex.RestResponse, ARTIFACT_NOT_FOUND);
        }

        #endregion 404 Not Found tests

        #region Private methods

        /// <summary>
        /// Verifies that the returned file has the right name, size date, file type and has some data.
        /// It DOES NOT verify the exact contents of the image though.
        /// </summary>
        /// <param name="artifactName">The name of the artifact whose image was retrieved.</param>
        /// <param name="file">The image returned by OpenAPI.</param>
        private static void ValidateReturnedImage(string artifactName, IFile file)
        {
            const string expectedFileType = "image/png";
            string expectedFileName = I18NHelper.FormatInvariant("{0}.png", artifactName);

            Assert.NotNull(file, "No file was returned!");
            Assert.AreEqual(expectedFileType, file.FileType, "The file type should be {0}", expectedFileType);
            Assert.AreEqual(expectedFileName, file.FileName, "The filename should be: {0}", expectedFileName);
            Assert.Greater(file.Content.Count(), 1000, "The file size looks suspiciously too small!");
            Assert.LessOrEqual(file.LastModifiedDate, DateTime.UtcNow, "The file date should be <= now!");
        }

        #endregion Private methods
    }
}
