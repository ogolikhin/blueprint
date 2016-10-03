using CustomAttributes;
using Helper;
using Model;
using Model.ArtifactModel;
using Model.ArtifactModel.Impl;
using Model.Factories;
using Model.Impl;
using NUnit.Framework;
using System.Collections.Generic;
using TestCommon;
using Utilities;

namespace ArtifactStoreTests
{
    [TestFixture]
    [Category(Categories.ArtifactStore)]
    public class MoveArtifactTests : TestBase
    {
        private const string SVC_PATH = RestPaths.Svc.ArtifactStore.Artifacts_id_.TO_id_;

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

        #region 200 OK tests

        [TestCase(BaseArtifactType.PrimitiveFolder)]
        [TestRail(182346)]
        [Description("Create & publish 3 artifacts. Create chain : grandparent, parent and child. Move parent artifact with a child to be a child of the project.  Verify the moved artifact is returned with the updated Parent ID.")]
        public void MoveArtifact_PublishedArtifactWithDependentChildBecomesChildOfProject_ReturnsArtifactDetails_200OK(BaseArtifactType artifactType)
        {
            // Setup: 
            IArtifact grandParentArtifact = Helper.CreateAndPublishArtifact(_project, _user, artifactType);            
            IArtifact parentArtifact = Helper.CreateAndPublishArtifact(_project, _user, artifactType, grandParentArtifact);
            Helper.CreateAndPublishArtifact(_project, _user, artifactType, parentArtifact);

            INovaArtifactDetails movedArtifactDetails = null;

            parentArtifact.Lock();

            // Execute:
            Assert.DoesNotThrow(() =>
            {
                movedArtifactDetails = ArtifactStore.MoveArtifact(Helper.BlueprintServer.Address, parentArtifact, _project.Id, _user);
            }, "'POST {0}' should return 200 OK when called with a valid token!", SVC_PATH);

            // Verify:
            INovaArtifactDetails artifactDetails = Helper.ArtifactStore.GetArtifactDetails(_user, parentArtifact.Id);
            NovaArtifactDetails.AssertEquals(artifactDetails, movedArtifactDetails);
            Assert.AreEqual(_project.Id, movedArtifactDetails.ParentId, "Parent Id of moved artifact is not the same as project Id");
        }

        [TestCase(BaseArtifactType.Process)]
        [TestRail(182373)]

        [Description("Create & publish 2 artifacts.  Move one artifact to be a child of the other.  Verify the moved artifact is returned with the updated Parent ID.")]
        public void MoveArtifact_PublishedArtifactBecomesChildOfPublishedArtifact_ReturnsArtifactDetails_200OK(BaseArtifactType artifactType)
        {
            // Setup:
            IArtifact artifact = Helper.CreateAndPublishArtifact(_project, _user, artifactType);
            IArtifact newParentArtifact = Helper.CreateAndPublishArtifact(_project, _user, artifactType);

            artifact.Lock();
            INovaArtifactDetails movedArtifactDetails = null;

            // Execute:
            Assert.DoesNotThrow(() =>
            {
                movedArtifactDetails = Helper.ArtifactStore.MoveArtifact(artifact, newParentArtifact, _user);
            }, "'POST {0}' should return 200 OK when called with a valid token!", SVC_PATH);

            // Verify:
            INovaArtifactDetails artifactDetails = Helper.ArtifactStore.GetArtifactDetails(_user, artifact.Id);
            NovaArtifactDetails.AssertEquals(artifactDetails, movedArtifactDetails);
            Assert.AreEqual(newParentArtifact.Id, movedArtifactDetails.ParentId, "Parent Id of moved artifact is not the same as project Id");

        }

        [TestCase(BaseArtifactType.Process)]
        [TestRail(182381)]
        [Description("Create & publish an artifact.  Move the artifact to the same location.  Verify the moved artifact is returned with the updated Parent ID.")]
        public void MoveArtifact_PublishedArtifact_FromProjectRootToProjectRoot_VerifyParentDidNotChange_200OK(BaseArtifactType artifactType)
        {
            INovaArtifactDetails movedArtifactDetails = null;

            // Setup:
            IArtifact artifact = Helper.CreateAndPublishArtifact(_project, _user, artifactType);

            artifact.Lock();

            // Execute:
            Assert.DoesNotThrow(() =>
            {
                movedArtifactDetails = ArtifactStore.MoveArtifact(Helper.BlueprintServer.Address, artifact, _project.Id, _user);
            }, "'POST {0}' should return 200 OK when called with a valid token!", SVC_PATH);

            // Verify:
            INovaArtifactDetails artifactDetails = Helper.ArtifactStore.GetArtifactDetails(_user, artifact.Id);
            NovaArtifactDetails.AssertEquals(artifactDetails, movedArtifactDetails);
            Assert.AreEqual(_project.Id, movedArtifactDetails.ParentId, "Parent Id of moved artifact is not the same as project Id");

        }

        [TestCase(BaseArtifactType.Process)]
        [TestRail(182402)]
        [Description("Create & publish 2 artifacts.  Move one artifact to be a child of the other.  Send current version of artifact with the message. erify the moved artifact is returned with the updated Parent ID..")]
        public void MoveArtifact_PublishedArtifactBecomesChildOfPublishedArtifact_SendCurrentVersion_200OK(BaseArtifactType artifactType)
        {
            const int CURRENT_VERSION_OF_ARTIFACT = 2;
            INovaArtifactDetails movedArtifactDetails = null;

            // Setup:
            IArtifact artifact = Helper.CreateAndPublishArtifact(_project, _user, artifactType, numberOfVersions: CURRENT_VERSION_OF_ARTIFACT);
            IArtifact newParentArtifact = Helper.CreateAndPublishArtifact(_project, _user, artifactType);

            artifact.Lock();
            // Execute:
            Assert.DoesNotThrow(() =>
            {
                movedArtifactDetails = Helper.ArtifactStore.MoveArtifact(artifact, newParentArtifact, _user, CURRENT_VERSION_OF_ARTIFACT);
            }, "'POST {0}' should return 200 OK when called with current version!", SVC_PATH);

            // Verify:
            INovaArtifactDetails artifactDetails = Helper.ArtifactStore.GetArtifactDetails(_user, artifact.Id);
            NovaArtifactDetails.AssertEquals(artifactDetails, movedArtifactDetails);
            Assert.AreEqual(newParentArtifact.Id, movedArtifactDetails.ParentId, "Parent Id of moved artifact is not the same as project Id");

        }

        #endregion 200 OK tests

        #region 401 Unauthorized tests

        [TestCase(BaseArtifactType.Process)]
        [TestRail(182380)]
        [Description("Create & publish 2 artifacts.  Move one artifact to be a child of the other with invalid token in a request.  Verify response returns code 401 Unauthorized.")]
        public void MoveArtifact_PublishedArtifactMoveToParentArtifactWithInvalidToken_401Unauthorized(BaseArtifactType artifactType)
        {
            // Setup:
            IArtifact artifact = Helper.CreateAndPublishArtifact(_project, _user, artifactType);
            IArtifact newParentArtifact = Helper.CreateAndPublishArtifact(_project, _user, artifactType);

            IUser userWithBadToken = Helper.CreateUserWithInvalidToken(TestHelper.AuthenticationTokenTypes.AccessControlToken);

            artifact.Lock();

            // Execute:
            var ex = Assert.Throws<Http401UnauthorizedException>(() =>
            {
                Helper.ArtifactStore.MoveArtifact(artifact, newParentArtifact, userWithBadToken);
            }, "'POST {0}' should return 401 Unauthorized when called with a invalid token!", SVC_PATH);

            // Verify:
            const string expectedExceptionMessage = "Unauthorized call";
            Assert.That(ex.RestResponse.Content.Contains(expectedExceptionMessage),
                "{0} was not found in returned message of move published artifact(s) which has invalid token.", expectedExceptionMessage);
        }

        #endregion 401 Unauthorized tests

        #region 403 Forbidden tests

        [TestCase(BaseArtifactType.Process)]
        [TestRail(182405)]
        [Description("Create & publish 2 artifacts.  Each one in different project. Move the artifact to be a child of the other in different project. Verify returned code 403 Forbidden.")]
        public void MoveArtifact_MoveOneArtifactToBeAChildOfAnotherInDifferentProject_403Forbidden(BaseArtifactType artifactType)
        {
            // Setup:
            var projects = ProjectFactory.GetProjects(_user, numberOfProjects: 2);
            Assert.GreaterOrEqual(projects.Count, 2, "This test requires at least 2 projects to exist!");

            IProject firstProject = projects[0];
            IProject secondProject = projects[1];

            IArtifact artifact1 = Helper.CreateAndPublishArtifact(firstProject, _user, artifactType);
            IArtifact artifact2 = Helper.CreateAndPublishArtifact(secondProject, _user, artifactType);

            artifact1.Lock();

            // Execute:
            var ex = Assert.Throws<Http403ForbiddenException>(() => Helper.ArtifactStore.MoveArtifact(artifact1, artifact2, _user),
                "'POST {0}' should return 409 Conflict when artifact moved to itself", SVC_PATH);

            // Verify:
            string expectedExceptionMessage = "Cannot move artifact to a different project.";
            Assert.That(ex.RestResponse.Content.Contains(expectedExceptionMessage),
                "{0} when user tries to move an artifact to different project", expectedExceptionMessage);
        }

        [Ignore(IgnoreReasons.UnderDevelopment)] //Not implemented yet. There is no ability to test Baseline and Review. This will be tested manually
//        [TestCase(BaseArtifactType.Collection)]
        [TestRail(182408)]
        [Description("Create & publish 2 artifacts of unsupported artifact type. Move an artifact to be a child of the other one.   Verify returned code 403 Forbidden.")]
        public void MoveArtifact_PublishedArtifactCannotBeMovedForUnsupportedArtifactTypes_403Forbidden(BaseArtifactType artifactType)
        {
            // Setup: 
            IArtifact artifact1 = Helper.CreateAndPublishArtifact(_project, _user, artifactType);
            IArtifact artifact2 = Helper.CreateAndPublishArtifact(_project, _user, artifactType);

            artifact1.Lock();

            // Execute:
            var ex = Assert.Throws<Http403ForbiddenException>(() => Helper.ArtifactStore.MoveArtifact(artifact1, artifact2, _user),

                "'POST {0}' should return 403 Forbidden when user tries to move artifact of unsupported artifact type", SVC_PATH);

            // Verify:
            string expectedExceptionMessage = "Cannot move baselines, collections or reviews.";
            Assert.That(ex.RestResponse.Content.Contains(expectedExceptionMessage),
                "{0} when user tries to move an artifact of unsupported artifact type", expectedExceptionMessage);
        }

        [TestCase(BaseArtifactType.Process)]
        [TestRail(182416)]
        [Description("Create & publish 2 artifacts. Move an artifact to be a child of the other one with user that does not have proper permissions.  Verify returned code 403 Forbidden.")]
        public void MoveArtifact_PublishedArtifactCannotBeMovedForUserWithoutProperPermissions_403Forbidden(BaseArtifactType artifactType)
        {
            // Setup:
            IArtifact artifact1 = Helper.CreateAndPublishArtifact(_project, _user, artifactType);
            IArtifact artifact2 = Helper.CreateAndPublishArtifact(_project, _user, artifactType);
            IUser userWithoutPermissions = Helper.CreateUserAndAuthenticate(TestHelper.AuthenticationTokenTypes.AccessControlToken,
                InstanceAdminRole.BlueprintAnalytics);

            artifact1.Lock();

            // Execute:
            var ex = Assert.Throws<Http403ForbiddenException>(() => Helper.ArtifactStore.MoveArtifact(artifact1, artifact2, userWithoutPermissions),
                "'POST {0}' should return 403 Forbidden when user tries to move artifact without proper permissions", SVC_PATH);

            // Verify:
            string expectedExceptionMessage = "You do not have permission to access the artifact (ID: " + artifact1.Id + ")";
            Assert.That(ex.RestResponse.Content.Contains(expectedExceptionMessage),
                "{0} when user tries to move an artifact without proper permissions", expectedExceptionMessage);
        }

        [TestCase(BaseArtifactType.Process)]
        [TestRail(182424)]
        [Description("Create & publish 2 artifacts. Move an artifact to be a child of the other one to which user does not have proper permissions.  Verify returned code 403 Forbidden.")]
        public void MoveArtifact_PublishedArtifactCannotBeMovedToArtifactWhichUserDoesNotHaveProperPermissions_403Forbidden(BaseArtifactType artifactType)
        {
            // Setup:
            IArtifact artifact1 = Helper.CreateAndPublishArtifact(_project, _user, artifactType);
            IArtifact artifact2 = Helper.CreateAndPublishArtifact(_project, _user, artifactType);

            // Create a user without permission to the artifact.
            IUser userWithoutPermissions = Helper.CreateUserAndAddToDatabase(instanceAdminRole: null);

            IProjectRole noneRole = ProjectRoleFactory.CreateProjectRole(_project, RolePermissions.None);
            IProjectRole authorRole = ProjectRoleFactory.CreateProjectRole(
                _project,
                RolePermissions.Delete |
                RolePermissions.Edit |
                RolePermissions.CanReport |
                RolePermissions.Comment |
                RolePermissions.DeleteAnyComment |
                RolePermissions.CreateRapidReview |
                RolePermissions.ExcelUpdate |
                RolePermissions.Read |
                RolePermissions.Reuse |
                RolePermissions.Share |
                RolePermissions.Trace);

            IGroup authorsGroup = Helper.CreateGroupAndAddToDatabase();
            authorsGroup.AddUser(userWithoutPermissions);
            authorsGroup.AssignRoleToProjectOrArtifact(_project, role: authorRole);

            IGroup viewersGroup = Helper.CreateGroupAndAddToDatabase();
            viewersGroup.AddUser(userWithoutPermissions);
            viewersGroup.AssignRoleToProjectOrArtifact(_project, role: noneRole, artifact: artifact2);

            Helper.AdminStore.AddSession(userWithoutPermissions);

            artifact1.Lock(userWithoutPermissions);
            try
            {
                // Execute:
                var ex = Assert.Throws<Http403ForbiddenException>(() => Helper.ArtifactStore.MoveArtifact(artifact1, artifact2, userWithoutPermissions),
                    "'POST {0}' should return 403 Forbidden when user tries to move artifact to artifact to which has/she no permissions", SVC_PATH);

                // Verify:
                string expectedExceptionMessage = "You do not have permission to access the artifact (ID: " + artifact2.Id + ")";
                Assert.That(ex.RestResponse.Content.Contains(expectedExceptionMessage),
                    "{0} when user tries to move an artifact without proper permissions", expectedExceptionMessage);

            }
            finally
            {
                Helper.ArtifactStore.DeleteArtifact(artifact1, userWithoutPermissions);
                Helper.ArtifactStore.PublishArtifact(artifact1, userWithoutPermissions);
            }
        }

        #endregion 403 Forbidden tests

        #region 404 Not Found tests

//        [TestCase(BaseArtifactType.Process, 0)]   Need to be fixed
        [TestCase(BaseArtifactType.Process, int.MaxValue)]
        [TestRail(182429)]
        [Description("Create & publish an artifact. Move an artifact to be a child of the non existing artifact.  Verify returned code 404 Not Found.")]
        public void MoveArtifact_CannotMoveArtifactToNonExistingArtifact_404NotFound(BaseArtifactType artifactType, int artifactId)
        {
            // Setup:
            IArtifact artifact = Helper.CreateAndPublishArtifact(_project, _user, artifactType);

            artifact.Lock();

            // Execute:
            var ex = Assert.Throws<Http404NotFoundException>(() => ArtifactStore.MoveArtifact(Helper.BlueprintServer.Address, artifact, artifactId, _user),
                "'POST {0}' should return 404 Not Found when user tries to move artifact to one that does not exist", SVC_PATH);

            // Verify:
            string expectedExceptionMessage = "You have attempted to access an artifact that does not exist or has been deleted.";
            Assert.That(ex.RestResponse.Content.Contains(expectedExceptionMessage),
                "{0} when user tries to move an artifact to artifact that does not exist", expectedExceptionMessage);
        }

        [TestCase(BaseArtifactType.Process)]
        [TestRail(182432)]
        [Description("Create & publish second artifacts. Delete 2nd one. Move first artifact to be a child of second artifact.  Verify returned code 404 Not Found.")]
        public void MoveArtifact_CannotMoveArtifactToDeletedArtifact_404NotFound(BaseArtifactType artifactType)
        {
            // Setup:
            IArtifact artifact1 = Helper.CreateAndPublishArtifact(_project, _user, artifactType);
            IArtifact artifact2 = Helper.CreateAndPublishArtifact(_project, _user, artifactType);

            artifact2.Delete();
            artifact1.Lock();

            // Execute:
            var ex = Assert.Throws<Http404NotFoundException>(() => Helper.ArtifactStore.MoveArtifact(artifact1, artifact2, _user),
                "'POST {0}' should return 404 Not Found when user tries to move artifact to one that does not exist", SVC_PATH);

            // Verify:
            string expectedExceptionMessage = "You have attempted to access an artifact that does not exist or has been deleted.";
            Assert.That(ex.RestResponse.Content.Contains(expectedExceptionMessage),
                "{0} when user tries to move an artifact to artifact that was removed", expectedExceptionMessage);
        }

        [TestCase(BaseArtifactType.Process)]
        [TestRail(182394)]
        [Description("Create & publish 2 artifacts. Delete first one. Move first artifact to be a child of second artifact.  Verify returned code 404 Not Found.")]
        public void MoveArtifact_CannotMoveDeletedArtifactToArtifact_404NotFound(BaseArtifactType artifactType)
        {
            // Setup:
            IArtifact artifact1 = Helper.CreateAndPublishArtifact(_project, _user, artifactType);
            IArtifact artifact2 = Helper.CreateAndPublishArtifact(_project, _user, artifactType);

            artifact1.Delete();

            // Execute:
            var ex = Assert.Throws<Http404NotFoundException>(() => Helper.ArtifactStore.MoveArtifact(artifact1, artifact2, _user),
                "'POST {0}' should return 404 Not Found when user tries to move artifact to one that does not exist", SVC_PATH);

            // Verify:
            string expectedExceptionMessage = "You have attempted to access an artifact that does not exist or has been deleted.";
            Assert.That(ex.RestResponse.Content.Contains(expectedExceptionMessage),
                "{0} when user tries to move an artifact to artifact that was removed", expectedExceptionMessage);
        }

        #endregion 404 Not Found tests

        #region 409 Conflict tests

        [TestCase(BaseArtifactType.Process, int.MaxValue)]
        [TestCase(BaseArtifactType.Process, 0)]
        [TestCase(BaseArtifactType.Process, 1)]
        [TestRail(182378)]
        [Description("Create & publish 2 artifacts.  Move one artifact to be a child of the other.  Send not current version of artifact with the message. Verify returned code 409 Conflict.")]
        public void MoveArtifact_PublishedArtifactBecomesChildOfPublishedArtifact_SendIncorrectVersion_409Conflict(BaseArtifactType artifactType, int artifactVersion)

        {
            // Setup:
            IArtifact artifact = Helper.CreateAndPublishArtifact(_project, _user, artifactType, numberOfVersions : 2);
            IArtifact newParentArtifact = Helper.CreateAndPublishArtifact(_project, _user, artifactType);

            artifact.Lock();
            // Execute:
            var ex = Assert.Throws<Http409ConflictException>(() => Helper.ArtifactStore.MoveArtifact(artifact, newParentArtifact, _user, artifactVersion),
                "'POST {0}' should return 409 Conflict when called with incorrect version!", SVC_PATH);

            // Verify:
            string expectedExceptionMessage = "Cannot move a historical version of an artifact. Please refresh.";
            Assert.That(ex.RestResponse.Content.Contains(expectedExceptionMessage),
                "{0} did not find version in returned message of move artifact call due to incorrect one sent with the request.", expectedExceptionMessage);
        }

        [TestCase(BaseArtifactType.Process)]
        [TestRail(182401)]
        [Description("Create & publish 2 artifacts.  Do not put lock on artifact that would be moved.  Move the artifact to be a child of the other. Verify returned code 409 Conflict.")]
        public void MoveArtifact_MoveOneArtifactToBeAChildOfAnother_DoNotSetLock_409Conflict(BaseArtifactType artifactType)
        {
            // Setup:
            IArtifact parentArtifact = Helper.CreateAndPublishArtifact(_project, _user, artifactType);
            IArtifact childArtifact = Helper.CreateAndPublishArtifact(_project, _user, artifactType);

            // Execute:
            var ex = Assert.Throws<Http409ConflictException>(() => Helper.ArtifactStore.MoveArtifact(parentArtifact, childArtifact, _user),
                "'POST {0}' should return 409 Conflict when parent moved to its child and was not locked", SVC_PATH);

            // Verify:
            string expectedExceptionMessage = "Cannot move an artifact that has not been locked.";
            Assert.That(ex.RestResponse.Content.Contains(expectedExceptionMessage),
                "{0} when user tries to move parent artifact to its child without previously locking it", expectedExceptionMessage);
        }

        [TestCase(BaseArtifactType.Process, 1)]
        [TestCase(BaseArtifactType.Process, 2)]
        [TestCase(BaseArtifactType.Process, 3)]
        [TestRail(182406)]
        [Description("Create & publish number of artifacts.  Move the first created artifact to be a child of one of its descendents. If one created it will be circular to itself. Verify returned code 409 Conflict.")]
        public void MoveArtifact_CreateCircularDependency_409Conflict(BaseArtifactType artifactType, int numberOfArtifacts)
        {
            // Setup:
            List<BaseArtifactType> artifactTypes = CreateListOfArtifactTypes(numberOfArtifacts, artifactType);

            var artifactList = Helper.CreatePublishedArtifactChain(_project, _user, artifactTypes.ToArray());

            Assert.IsNotNull(artifactList, "Artifact List is not created");

            IArtifact firstArtifact = artifactList[0];
            IArtifact lastArtifact = null ;

            firstArtifact.Lock();

            if (artifactList.Count == 1)
                lastArtifact = firstArtifact;
            else
                lastArtifact = artifactList[artifactList.Count - 1];

            // Execute:
            var ex = Assert.Throws<Http409ConflictException>(() => Helper.ArtifactStore.MoveArtifact(firstArtifact, lastArtifact, _user),
                "'POST {0}' should return 409 Conflict when artifact moved to one of its descendents", SVC_PATH);

            // Verify:
            string expectedExceptionMessage = "This move will result in a circular relationship between the artifact and its new parent.";
            Assert.That(ex.RestResponse.Content.Contains(expectedExceptionMessage),
                "{0} when user tries to move an artifact to one of its descendents", expectedExceptionMessage);
        }

        #endregion 409 Conflict tests

        #region private call

        /// <summary>
        /// Creates a list of artifact types.
        /// </summary>
        /// <param name="numberOfArtifacts">The number of artifact types to add to the list.</param>
        /// <param name="artifactType">The artifact type.</param>
        /// <returns>A list of artifact types.</returns>
        private static List<BaseArtifactType> CreateListOfArtifactTypes(int numberOfArtifacts, BaseArtifactType artifactType)
        {
            List<BaseArtifactType> artifactTypes = new List<BaseArtifactType>();

            for (int i = 0; i < numberOfArtifacts; i++)
            {
                artifactTypes.Add(artifactType);
            }

            return artifactTypes;
        }

        #endregion private call
    }
}
