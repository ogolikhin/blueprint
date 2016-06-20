using System;
using CustomAttributes;
using Helper;
using Model;
using Model.Factories;
using NUnit.Framework;
using TestCommon;
using Utilities;

namespace ArtifactStoreTests
{
    [TestFixture]
    [Category(Categories.ArtifactStore)]
    public class ProjectMetaTests : TestBase
    {
        private IProject _project = null;
        private IUser _user = null;

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

        [TestCase]
        [TestRail(145878)]
        [Description("Runs 'GET /projects/{projectId}/meta/customtypes' with a valid projectId and token and verifies it returns 200 OK and returns expected artifact types.")]
        public void GetArtifactTypes_ReturnsExpectedArtifactTypes()
        {
            ProjectArtifactTypesResult artifactTypes = null;

            // Execute:
            Assert.DoesNotThrow(() =>
            {
                artifactTypes = Helper.ArtifactStore.GetArtifactTypes(_project, _user);
            }, "The GET /projects/{projectId}/meta/customtypes endpoint should return OK for a valid user & project.");

            // Verify expected artifact types are returned.
            Assert.IsNotEmpty(artifactTypes.ArtifactTypes, "Project '{0}' contains no artifact types!", _project.Name);
            Assert.IsNotEmpty(artifactTypes.SubArtifactTypes, "Project '{0}' contains no sub-artifact types!", _project.Name);
            Assert.IsNotEmpty(artifactTypes.PropertyTypes, "Project '{0}' contains no property types!", _project.Name);
        }

        [TestCase]
        [TestRail(145879)]
        [Description("Runs 'GET /projects/{projectId}/meta/customtypes' with a valid projectId but no token and verifies it returns 400 Bad Request.")]
        public void GetArtifactTypes_NoToken_BadRequest()
        {
            Assert.Throws<Http400BadRequestException>(() =>
            {
                Helper.ArtifactStore.GetArtifactTypes(_project);
            }, "The GET /projects/{projectId}/meta/customtypes endpoint should return 400 Bad Request when no token header is provided.");
        }

        [TestCase]
        [TestRail(145880)]
        [Description("Runs 'GET /projects/{projectId}/meta/customtypes' with a valid projectId and an unauthorized token and verifies it returns 401 Unauthorized.")]
        public void GetArtifactTypes_UnauthorizedToken_Unauthorized()
        {
            _user.Token.AccessControlToken = (new Guid()).ToString();

            Assert.Throws<Http401UnauthorizedException>(() =>
            {
                Helper.ArtifactStore.GetArtifactTypes(_project, _user);
            }, "The GET /projects/{projectId}/meta/customtypes endpoint should return 401 Unauthorized when an unauthorized token is passed.");
        }

        /*
        // TODO:  Add this test to TestRail and enable this test once we have the ability to set user permissions.
        [TestCase]
        [TestRail()]
        [Description("Runs 'GET /projects/{projectId}/meta/customtypes' with a valid projectId and token, but the user doesn't have permission to access the project and verify it returns 403 Forbidden.")]
        public void GetArtifactTypes_Forbidden()
        {
            Assert.Throws<Http403ForbiddenException>(() =>
            {
                Helper.ArtifactStore.GetArtifactTypes(_project, _user);
            }, "The GET /projects/{projectId}/meta/customtypes endpoint should return 403 Forbidden when a user doesn't have permission to access the specified project.");
        }
        */

        [TestCase]
        [TestRail(145881)]
        [Description("Runs 'GET /projects/{projectId}/meta/customtypes' with a non-existing projectId and valid token and verifies it returns 404 Not Found.")]
        public void GetArtifactTypes_NonExistingProjectId_NotFound()
        {
            IProject nonExistingProject = ProjectFactory.CreateProject(id: int.MaxValue);

            Assert.Throws<Http404NotFoundException>(() =>
            {
                Helper.ArtifactStore.GetArtifactTypes(nonExistingProject, _user);
            }, "The GET /projects/{projectId}/meta/customtypes endpoint should return 404 Not Found for non-existing Project ID.");
        }
    }
}
