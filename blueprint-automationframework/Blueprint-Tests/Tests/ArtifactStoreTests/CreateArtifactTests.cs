using System.Collections.Generic;
using System.Net;
using CustomAttributes;
using Helper;
using Model;
using Model.ArtifactModel;
using Model.ArtifactModel.Enums;
using Model.ArtifactModel.Impl;
using Model.Factories;
using Newtonsoft.Json;
using NUnit.Framework;
using TestCommon;
using Utilities;
using Utilities.Facades;
using Utilities.Factories;

namespace ArtifactStoreTests
{
    [TestFixture]
    [Category(Categories.ArtifactStore)]
    public class CreateArtifactTests : TestBase
    {
        private const string SVC_PATH = RestPaths.Svc.ArtifactStore.Artifacts.CREATE;

        private IUser _user = null;
        private IProject _project = null;

        [SetUp]
        public void SetUp()
        {
            Helper = new TestHelper();
            _user = Helper.CreateUserAndAuthenticate(TestHelper.AuthenticationTokenTypes.BothAccessControlAndOpenApiTokens);
            _project = ProjectFactory.GetProject(_user);
            _project.GetAllNovaArtifactTypes(Helper.ArtifactStore, _user);
        }

        [TearDown]
        public void TearDown()
        {
            Helper?.Dispose();
        }

        #region 200 OK tests

        [TestCase(ArtifactTypePredefined.Actor)]
        //        [TestCase(ArtifactTypePredefined.Baseline)]
        [TestCase(ArtifactTypePredefined.BusinessProcess)]
        //        [TestCase(ArtifactTypePredefined.DataElement)]
        [TestCase(ArtifactTypePredefined.Document)]
        [TestCase(ArtifactTypePredefined.DomainDiagram)]
        [TestCase(ArtifactTypePredefined.GenericDiagram)]
        [TestCase(ArtifactTypePredefined.Glossary)]
        [TestCase(ArtifactTypePredefined.PrimitiveFolder)]
        //        [TestCase(ArtifactTypePredefined.Project)]
        [TestCase(ArtifactTypePredefined.Storyboard)]
        [TestCase(ArtifactTypePredefined.TextualRequirement)]
        [TestCase(ArtifactTypePredefined.UIMockup)]
        [TestCase(ArtifactTypePredefined.UseCase)]
        [TestCase(ArtifactTypePredefined.UseCaseDiagram)]

        //        [TestCase(BaselineAndCollectionTypePredefined.ArtifactBaseline)]
        [TestCase(BaselineAndCollectionTypePredefined.ArtifactCollection, Explicit = true, IgnoreReason = IgnoreReasons.ProductBug)]
        //        [TestCase(BaselineAndCollectionTypePredefined.ArtifactReviewPackage)]
        //        [TestCase(BaselineAndCollectionTypePredefined.BaselineFolder)]
        [TestCase(BaselineAndCollectionTypePredefined.CollectionFolder, Explicit = true, IgnoreReason = IgnoreReasons.ProductBug)]
        [TestRail(154745)]
        [Description("Create an artifact.  Get the artifact.  Verify the artifact returned has the same properties as the artifact we created.")]
        public void CreateArtifact_UnpublishedArtifact_CanGetArtifact(ItemTypePredefined artifactType)
        {
            // Execute:
            INovaArtifactDetails newArtifact = null;

            Assert.DoesNotThrow(() =>
                newArtifact = CreateArtifactWithRandomName(artifactType, _user, _project),
                "Exception caught while trying to create an artifact of type: '{0}'!", artifactType);

            // Verify:
            var artifactDetails = Helper.ArtifactStore.GetArtifactDetails(_user, newArtifact.Id);
            artifactDetails.AssertEquals(newArtifact);
        }

        // TODO: Create an artifact and specify the OrderIndex before another artifact.
        // TODO: Create an artifact and specify the OrderIndex same as another artifact.
        // TODO: Create an artifact and specify the OrderIndex after another artifact.
        // TODO: Create Collections.

        #endregion 200 OK tests

        #region Negative tests

        // TODO: Send an artifact without one of the following: Name, ProjectId, ItemTtypeId, or ParentId.  Verify 400 Bad Request.
        // TODO: Send a corrupt JSON body.  Verify 400 Bad Request.
        // TODO: Create user with access to project but not to a folder and try to create artifact in that folder.  Verify 403 Forbidden.
        // TODO: Pass non-existent ItemTypeId.  Verify 404 Not Found.
        // TODO: Pass non-existent ParentId.  Verify 404 Not Found.
        // TODO: Create a folder with a parent other than a Project or Folder.  Verify 409 Conflict.
        // TODO: Create an artifact with ProjectID x with a Parent that exists in project y.  Verify ?? Error.

        [TestCase]
        [TestRail(154746)]
        [Description("Create an artifact but don't send a 'Session-Token' header in the request.  Verify 401 Unauthorized is returned.")]
        public void CreateArtifact_NoTokenHeader_401Unauthorized()
        {
            // Setup:
            IUser userWithNoToken = Helper.CreateUserAndAddToDatabase();

            // Execute & Verify:
            Assert.Throws<Http401UnauthorizedException>(() => CreateArtifactWithRandomName(ItemTypePredefined.Process, userWithNoToken, _project),
                "'POST {0}' should return 401 Unauthorized if no Session-Token header is passed!", SVC_PATH);
        }

        [TestCase]
        [TestRail(154747)]
        [Description("Create an artifact but pass an unauthorized token.  Verify 401 Unauthorized is returned.")]
        public void CreateArtifact_UnauthorizedToken_401Unauthorized()
        {
            // Setup:
            IUser userWithBadToken = Helper.CreateUserWithInvalidToken(TestHelper.AuthenticationTokenTypes.AccessControlToken);

            // Execute & Verify:
            Assert.Throws<Http401UnauthorizedException>(() => CreateArtifactWithRandomName(ItemTypePredefined.Process, userWithBadToken, _project),
                "'POST {0}' should return 401 Unauthorized if an invalid token is passed!", SVC_PATH);
        }

        [Explicit(IgnoreReasons.ProductBug)]    // Fails with: "Artifact's Project is not found or does not exist."
        [TestCase]
        [TestRail(154748)]
        [Description("Create an artifact as a user that doesn't have permission to add artifacts to the project.  Verify 403 Forbidden is returned.")]
        public void CreateArtifact_UserWithoutPermissions_403Forbidden()
        {
            // Setup:
            IUser userWithoutPermission = Helper.CreateUserAndAuthenticate(TestHelper.AuthenticationTokenTypes.AccessControlToken,
                InstanceAdminRole.BlueprintAnalytics);

            // Execute & Verify:
            Assert.Throws<Http403ForbiddenException>(() => CreateInvalidArtifact(userWithoutPermission,
                _project, (int)ItemTypePredefined.Process),
                "'POST {0}' should return 403 Forbidden if the user doesn't have permission to add artifacts!", SVC_PATH);
        }

        [TestCase(0, Explicit = true, IgnoreReason = IgnoreReasons.ProductBug)] // Fails with: "DItemType with Id: 4114 was deleted by some other user. Please refresh."
        [TestCase(int.MaxValue)]
        [TestRail(154749)]
        [Description("Create an artifact with a non-existent Project ID.  Verify 404 Not Found is returned.")]
        public void CreateArtifact_NonExistentProjectId_404NotFound(int projectId)
        {
            // Setup:
            // Create a Project with a fake ID that shouldn't exist.
            IProject fakeProject = ProjectFactory.CreateProject(id: projectId);

            // Execute & Verify:
            Assert.Throws<Http404NotFoundException>(() => CreateInvalidArtifact(_user, fakeProject, (int)ItemTypePredefined.Process),
                "'POST {0}' should return 404 Not Found if the Project ID doesn't exist!", SVC_PATH);
        }

        #endregion Negative tests

        #region Private functions

        /// <summary>
        /// Creates a new artifact with a random name.
        /// </summary>
        /// <param name="artifactType">The type of artifact to create.</param>
        /// <param name="user">The user to authenticate with.</param>
        /// <param name="project">The project where the artifact will be created.</param>
        /// <returns>The artifact that was created.</returns>
        private INovaArtifactDetails CreateArtifactWithRandomName(ItemTypePredefined artifactType, IUser user, IProject project)
        {
            string artifactName = RandomGenerator.RandomAlphaNumericUpperAndLowerCase(10);
            return Helper.ArtifactStore.CreateArtifact(user, artifactType, artifactName, project);
        }

        /// <summary>
        /// Tries to create an invalid artifact.
        /// </summary>
        /// <param name="user">The user to authenticate with.</param>
        /// <param name="project">The project where the artifact will be created.</param>
        /// <param name="itemTypeId">The ItemType ID of the artifact.</param>
        /// <param name="artifactName">The name of the artifact.</param>
        /// <param name="parentId">The parent ID of the artifact.</param>
        /// <param name="orderIndex">The order index of the artifact.</param>
        /// <returns>The artifact that was created.</returns>
        private INovaArtifactDetails CreateInvalidArtifact(IUser user,
            IProject project,
            int itemTypeId,
            string artifactName = null,
            int? parentId = null,
            double? orderIndex = null)
        {
            artifactName = artifactName ?? RandomGenerator.RandomAlphaNumericUpperAndLowerCase(10);

            NovaArtifactDetails artifactDetails = new NovaArtifactDetails
            {
                Name = artifactName,
                ProjectId = project.Id,
                ItemTypeId = itemTypeId,
                ParentId = parentId ?? project.Id,
                OrderIndex = orderIndex
            };
            
            string jsonBody = JsonConvert.SerializeObject(artifactDetails);

            RestResponse response = CreateInvalidArtifact(user, jsonBody);
            INovaArtifactDetails createdArtifact = JsonConvert.DeserializeObject<NovaArtifactDetails>(response.Content);
            
            return createdArtifact;
        }
        
        /// <summary>
        /// Tries to create an invalid artifact.
        /// </summary>
        /// <param name="user">The user to authenticate with.</param>
        /// <param name="jsonBody">The JSON body of the request.</param>
        /// <returns>The REST response.</returns>
        private RestResponse CreateInvalidArtifact(IUser user, string jsonBody)
        {
            RestApiFacade restApi = new RestApiFacade(Helper.BlueprintServer.Address, user?.Token?.AccessControlToken);

            // Set expectedStatusCodes to 201 Created.
            List<HttpStatusCode> expectedStatusCodes = new List<HttpStatusCode> { HttpStatusCode.Created };
            const string contentType = "application/json";

            var response = restApi.SendRequestBodyAndGetResponse(
                SVC_PATH,
                RestRequestMethod.POST,
                jsonBody,
                contentType,
                expectedStatusCodes: expectedStatusCodes);

            return response;
        }
        
        #endregion Private functions
    }
}
