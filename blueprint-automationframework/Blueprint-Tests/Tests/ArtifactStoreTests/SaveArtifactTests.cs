using System;
using CustomAttributes;
using Helper;
using Model;
using Model.ArtifactModel;
using Model.ArtifactModel.Impl;
using Model.Factories;
using NUnit.Framework;
using TestCommon;
using Utilities;

namespace ArtifactStoreTests
{
    [TestFixture]
    [Category(Categories.ArtifactStore)]
    [Explicit(IgnoreReasons.UnderDevelopment)]
    public class SaveArtifactTests : TestBase
    {
        private IUser _user = null;
        private IProject _project = null;

        [SetUp]
        public void SetUp()
        {
            Helper = new TestHelper();
            _user = Helper.CreateUserAndAuthenticate(TestHelper.AuthenticationTokenTypes.BothAccessControlAndOpenApiTokens);
            _project = ProjectFactory.GetProject(_user);
        }

        [TearDown]
        public void TearDown()
        {
            Helper?.Dispose();
        }

        [TestCase(BaseArtifactType.Actor)]
        [TestCase(BaseArtifactType.BusinessProcess)]
        [TestCase(BaseArtifactType.Document)]
        [TestCase(BaseArtifactType.DomainDiagram)]
        [TestCase(BaseArtifactType.GenericDiagram)]
        [TestCase(BaseArtifactType.Glossary)]
        [TestCase(BaseArtifactType.PrimitiveFolder)]
        [TestCase(BaseArtifactType.Process)]
        [TestCase(BaseArtifactType.Storyboard)]
        [TestCase(BaseArtifactType.TextualRequirement)]
        [TestCase(BaseArtifactType.UIMockup)]
        [TestCase(BaseArtifactType.UseCase)]
        [TestCase(BaseArtifactType.UseCaseDiagram)]
        [TestRail(154745)]
        [Description("Create & save an artifact.  Get the artifact.  Verify the artifact returned has the same properties as the artifact we saved.")]
        public void SaveArtifact_ValidArtifact_CanGetArtifact(BaseArtifactType artifactType)
        {
            // Setup:
            IArtifact artifact = Helper.CreateArtifact(_project, _user, artifactType);

            // Execute:
            Assert.DoesNotThrow(() => artifact.Save(),
                "Exception caught while trying to save an artifact of type: '{0}'!", artifactType);

            // Verify:
            IOpenApiArtifact openApiArtifact = OpenApiArtifact.GetArtifact(Helper.BlueprintServer.Address, _project, artifact.Id, _user);
            TestHelper.AssertArtifactsAreEqual(artifact, openApiArtifact);
        }

        [TestCase]
        [TestRail(154746)]
        [Description("Create & save an artifact but don't send a 'Session-Token' header in the request.  Verify 400 Bad Request is returned.")]
        public void SaveArtifact_NoTokenHeader_400BadRequest()
        {
            // Setup:
            IArtifact artifact = Helper.CreateArtifact(_project, _user, BaseArtifactType.Process);
            IUser userWithNoToken = Helper.CreateUserAndAddToDatabase();

            // Execute & Verify:
            Assert.Throws<Http400BadRequestException>(() => artifact.Save(userWithNoToken),
                "'POST {0}' should return 400 Bad Request if no Session-Token header is passed!",
                RestPaths.Svc.ArtifactStore.ARTIFACTS);
        }

        [TestCase]
        [TestRail(154747)]
        [Description("Create & save an artifact but pass an unauthorized token.  Verify 401 Unauthorized is returned.")]
        public void SaveArtifact_UnauthorizedToken_401Unauthorized()
        {
            // Setup:
            IArtifact artifact = Helper.CreateArtifact(_project, _user, BaseArtifactType.Process);
            IUser userWithBadToken = Helper.CreateUserWithInvalidToken(TestHelper.AuthenticationTokenTypes.AccessControlToken);

            // Execute & Verify:
            Assert.Throws<Http401UnauthorizedException>(() => artifact.Save(userWithBadToken),
                "'POST {0}' should return 401 Unauthorized if an invalid token is passed!",
                RestPaths.Svc.ArtifactStore.ARTIFACTS);
        }

        [TestCase]
        [TestRail(154748)]
        [Description("Create & save an artifact as a user that doesn't have permission to add artifacts to the project.  Verify 403 Forbidden is returned.")]
        public void SaveArtifact_UserWithoutPermissions_403Forbidden()
        {
            // Setup:
            IArtifact artifact = Helper.CreateArtifact(_project, _user, BaseArtifactType.Process);
            IUser userWithoutPermission = Helper.CreateUserAndAuthenticate(TestHelper.AuthenticationTokenTypes.AccessControlToken,
                InstanceAdminRole.BlueprintAnalytics);

            // Execute & Verify:
            Assert.Throws<Http403ForbiddenException>(() => artifact.Save(userWithoutPermission),
                "'POST {0}' should return 403 Forbidden if the user doesn't have permission to add artifacts!",
                RestPaths.Svc.ArtifactStore.ARTIFACTS);
        }

        [TestCase(0)]
        [TestCase(int.MaxValue)]
        [TestRail(154749)]
        [Description("Create & save an artifact with a non-existent Project ID.  Verify 404 Not Found is returned.")]
        public void SaveArtifact_NonExistentProjectId_404NotFound(int projectId)
        {
            // Setup:
            IProject nonExistentProject = ProjectFactory.CreateProject(id: projectId);
            IArtifact artifact = Helper.CreateArtifact(nonExistentProject, _user, BaseArtifactType.Process);

            // Execute & Verify:
            Assert.Throws<Http404NotFoundException>(() => artifact.Save(),
                "'POST {0}' should return 404 Not Found if the Project ID doesn't exist!",
                RestPaths.Svc.ArtifactStore.ARTIFACTS);
        }
    }
}
