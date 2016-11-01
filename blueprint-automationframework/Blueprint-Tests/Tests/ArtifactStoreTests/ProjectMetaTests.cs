using System;
using System.Collections.Generic;
using Common;
using CustomAttributes;
using Helper;
using Model;
using Model.ArtifactModel.Impl;
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

        [TestFixtureSetUp]
        public void SetUp()
        {
            Helper = new TestHelper();
            _user = Helper.CreateUserAndAuthenticate(TestHelper.AuthenticationTokenTypes.BothAccessControlAndOpenApiTokens);
            _project = ProjectFactory.GetProject(_user);
        }

        [TestFixtureTearDown]
        public void TearDown()
        {
            Helper?.Dispose();
        }

        [TestCase]
        [TestRail(145878)]
        [Description("Runs 'GET /projects/{projectId}/meta/customtypes' with a valid projectId and token and verifies it returns 200 OK and returns expected artifact types.")]
        public void GetArtifactTypes_ReturnsExpectedArtifactTypes()
        {
            // Setup:
            ProjectCustomArtifactTypesResult artifactTypes = null;

            IUser viewer = Helper.CreateUserWithProjectRolePermissions(TestHelper.ProjectRole.Viewer, _project);

            // Execute:
            Assert.DoesNotThrow(() =>
            {
                artifactTypes = Helper.ArtifactStore.GetCustomArtifactTypes(_project, viewer);
            }, "The GET /projects/{projectId}/meta/customtypes endpoint should return OK for a valid user & project.");

            // Verify:  expected artifact types are returned.
            Assert.IsNotEmpty(artifactTypes.ArtifactTypes, "Project '{0}' contains no artifact types!", _project.Name);
            Assert.IsNotEmpty(artifactTypes.SubArtifactTypes, "Project '{0}' contains no sub-artifact types!", _project.Name);
            Assert.IsNotEmpty(artifactTypes.PropertyTypes, "Project '{0}' contains no property types!", _project.Name);

            // These are just a small number of expected types to look for.
            List<string> expectedArtifactTypeNames = new List<string>
            {
                "Actor", "Document", "Glossary", "Process", "Storyboard"
            };

            List<string> expectedSubArtifactTypeNames = new List<string>
            {
                "Document: Bookmark", "Glossary: Term", "Use Case: Pre Condition", "Process: Shape", "Storyboard: Connector"
            };

            // Search for specific Artifact types.
            foreach (string expectedTypeName in expectedArtifactTypeNames)
            {
                Assert.That(artifactTypes.ArtifactTypes.Exists(a => a.Name == expectedTypeName),
                    "Couldn't find '{0}' in list of artifact types.", expectedTypeName);
            }

            // Search for specific Sub-artifact types.
            foreach (string expectedTypeName in expectedSubArtifactTypeNames)
            {
                Assert.That(artifactTypes.SubArtifactTypes.Exists(a => a.Name == expectedTypeName),
                    "Couldn't find '{0}' in list of sub-artifact types.", expectedTypeName);
            }

            // Search for specific Property types.
            Assert.That(artifactTypes.PropertyTypes.Exists(p => p.PrimitiveType == PropertyPrimitiveType.Text),
                "There were no Text property types returned!");
            Assert.That(artifactTypes.PropertyTypes.Exists(p => p.PrimitiveType == PropertyPrimitiveType.Number),
                "There were no Number property types returned!");
        }

        [TestCase]
        [TestRail(145879)]
        [Description("Runs 'GET /projects/{projectId}/meta/customtypes' with a valid projectId but no token and verifies it returns 400 Bad Request.")]
        public void GetArtifactTypes_NoToken_BadRequest()
        {
            // Execute:
            var ex = Assert.Throws<Http400BadRequestException>(() =>
            {
                Helper.ArtifactStore.GetCustomArtifactTypes(_project);
            }, "The GET /projects/{projectId}/meta/customtypes endpoint should return 400 Bad Request when no token header is provided.");

            // Verify:
            Assert.AreEqual("{\"message\":\"Token is missing or malformed.\"}", ex.RestResponse.Content);
        }

        [TestCase]
        [TestRail(145880)]
        [Description("Runs 'GET /projects/{projectId}/meta/customtypes' with a valid projectId and an unauthorized token and verifies it returns 401 Unauthorized.")]
        public void GetArtifactTypes_UnauthorizedToken_Unauthorized()
        {
            // Setup: Create a user with an unauthorized token.
            IUser unauthorizedUser = UserFactory.CreateUserOnly();
            string newToken = (new Guid()).ToString();
            unauthorizedUser.SetToken(newToken);
            unauthorizedUser.Token.AccessControlToken = newToken;

            // Execute:
            var ex = Assert.Throws<Http401UnauthorizedException>(() =>
            {
                Helper.ArtifactStore.GetCustomArtifactTypes(_project, unauthorizedUser);
            }, "The GET /projects/{projectId}/meta/customtypes endpoint should return 401 Unauthorized when an unauthorized token is passed.");

            // Verify:
            Assert.AreEqual("{\"message\":\"Token is invalid.\"}", ex.RestResponse.Content);
        }

        [TestCase(InstanceAdminRole.AdministerALLProjects)]
        [TestCase(InstanceAdminRole.AssignInstanceAdministrators)]
        [TestCase(InstanceAdminRole.BlueprintAnalytics)]
        [TestCase(InstanceAdminRole.Email_ActiveDirectory_SAMLSettings)]
        [TestCase(InstanceAdminRole.InstanceStandardsManager)]
        [TestCase(InstanceAdminRole.LogGatheringAndLicenseReporting)]
        [TestCase(InstanceAdminRole.ManageAdministratorRoles)]
        [TestCase(InstanceAdminRole.ProvisionProjects)]
        [TestCase(InstanceAdminRole.ProvisionUsers)]
        [TestCase(null)]
        [TestRail(145905)]
        [Description("Runs 'GET /projects/{projectId}/meta/customtypes' with a valid projectId and token, but the user doesn't have permission to access the project and verify it returns 403 Forbidden.")]
        public void GetArtifactTypes_InsufficientPermissions_Forbidden(InstanceAdminRole? role)
        {
            // Setup: Create a user that isn't a Default Instance Admin (i.e. doesn't have access to the project).
            IUser forbiddenUser = UserFactory.CreateUserAndAddToDatabase(role);
            Helper.AdminStore.AddSession(forbiddenUser);

            // Execute:
            var ex = Assert.Throws<Http403ForbiddenException>(() =>
            {
                Helper.ArtifactStore.GetCustomArtifactTypes(_project, forbiddenUser);
            }, "The GET /projects/{projectId}/meta/customtypes endpoint should return 403 Forbidden when a user doesn't have permission to access the specified project.");

            // Verify:
            ArtifactStoreHelper.ValidateServiceError(ex.RestResponse, ErrorCodes.UnauthorizedAccess,
                I18NHelper.FormatInvariant("The user does not have permissions for Project (Id:{0}).", _project.Id));
        }

        [TestCase]
        [TestRail(190764)]
        [Description("Runs 'GET /projects/{projectId}/meta/customtypes' with a existing projectId with all permissions except of Read one. Verifies it returns 403 Forbidden.")]
        public void GetArtifactTypes_NoReadPermissionToProject_Forbidden()
        {
            // Create a user without permission to the project.
            var userWithoutFullAccessPermissions = Helper.CreateUserWithProjectRolePermissions(TestHelper.ProjectRole.None, _project);

            //Assign all permissions except read
            Helper.AssignProjectRolePermissionsToUser(userWithoutFullAccessPermissions, 
                RolePermissions.CanReport | 
                RolePermissions.Comment | 
                RolePermissions.CreateRapidReview | 
                RolePermissions.Delete | 
                RolePermissions.DeleteAnyComment | 
                RolePermissions.ExcelUpdate | 
                RolePermissions.Reuse | 
                RolePermissions.Edit | 
                RolePermissions.Share | 
                RolePermissions.StealLock | 
                RolePermissions.Trace, 
                _project);

            var ex = Assert.Throws<Http403ForbiddenException>(() =>
            {
                Helper.ArtifactStore.GetCustomArtifactTypes(_project, userWithoutFullAccessPermissions);
            }, "The GET /projects/{projectId}/meta/customtypes endpoint should return 403 Forbidden for user without read permission to the project.");

            // Verify:
            ArtifactStoreHelper.ValidateServiceError(ex.RestResponse, ErrorCodes.UnauthorizedAccess,
                I18NHelper.FormatInvariant("The user does not have permissions for Project (Id:{0}).", _project.Id));
        }

        [TestCase]
        [TestRail(145881)]
        [Description("Runs 'GET /projects/{projectId}/meta/customtypes' with a non-existing projectId and valid token and verifies it returns 404 Not Found.")]
        public void GetArtifactTypes_NonExistingProjectId_NotFound()
        {
            IProject nonExistingProject = ProjectFactory.CreateProject(id: int.MaxValue);

            var ex = Assert.Throws<Http404NotFoundException>(() =>
            {
                Helper.ArtifactStore.GetCustomArtifactTypes(nonExistingProject, _user);
            }, "The GET /projects/{projectId}/meta/customtypes endpoint should return 404 Not Found for non-existing Project ID.");

            // Verify:
            ArtifactStoreHelper.ValidateServiceError(ex.RestResponse, ErrorCodes.ResourceNotFound,
                I18NHelper.FormatInvariant("Project (Id:{0}) is not found.", nonExistingProject.Id));
        }
    }
}
