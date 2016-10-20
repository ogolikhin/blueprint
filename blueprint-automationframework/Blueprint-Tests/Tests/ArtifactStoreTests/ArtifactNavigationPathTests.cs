using CustomAttributes;
using Helper;
using Model;
using Model.ArtifactModel;
using Model.Factories;
using NUnit.Framework;
using System.Collections.Generic;
using TestCommon;
using System.Linq;

namespace ArtifactStoreTests
{
    [TestFixture]
    [Category(Categories.ArtifactStore)]
    public class ArtifactNavigationPathTests : TestBase
    {
        private const string SVC_PATH = RestPaths.Svc.ArtifactStore.Artifacts_id_.NAVIGATION_PATH;
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

        [TestCase(BaseArtifactType.Actor, 2)]
        [TestCase(BaseArtifactType.PrimitiveFolder, 2)]
        [TestRail(183596)]
        [Description("Create & publish an artifact.  Verify the basic artifact information returned from parent has correct values.")]
        public void ArtifactNavigation_PublishedArtifact_ReturnsParentArtifactInfo_200OK(BaseArtifactType artifactType, int numberOfVersions)
        {
            // Setup:
            var artifact = Helper.CreateAndPublishArtifact(_project, _user, artifactType, numberOfVersions: numberOfVersions);

            List<INovaVersionControlArtifactInfo> basicArtifactInfoList = null;

            // Execute:
            Assert.DoesNotThrow(() => basicArtifactInfoList = Helper.ArtifactStore.GetNavigationPath(_user, artifact.Id),
                                "'GET {0}' should return 200 OK when passed a valid artifact ID!", SVC_PATH);

            // Verify:
            VerifyParentInformation(basicArtifactInfoList[0], artifact.ParentId);
        }

        [TestCase(BaseArtifactType.Actor)]
        [TestCase(BaseArtifactType.PrimitiveFolder)]
        [TestRail(183597)]
        [Description("Create & save an artifact.  Verify the basic artifact information returned from parent has correct values.")]
        public void ArtifactNavigation_SavedArtifact_ReturnsParentArtifactInfo_200OK(BaseArtifactType artifactType)
        {
            // Setup:
            var artifact = Helper.CreateAndSaveArtifact(_project, _user, artifactType);

            List<INovaVersionControlArtifactInfo> basicArtifactInfoList = null;

            // Execute:
            Assert.DoesNotThrow(() => basicArtifactInfoList = Helper.ArtifactStore.GetNavigationPath(_user, artifact.Id),
                                "'GET {0}' should return 200 OK when passed a valid artifact ID!", SVC_PATH);

            // Verify:
            VerifyParentInformation(basicArtifactInfoList[0], artifact.ParentId);
        }

        [TestCase(BaseArtifactType.Actor, 2)]
        [TestRail(183598)]
        [Description("Create & publish an artifact and its child.  Verify the basic artifact information returned from parent has correct values.")]
        public void ArtifactNavigation_PublishedArtifactWithAChild_ReturnsParentArtifactInfo_200OK(BaseArtifactType artifactType, int numberOfVersions)
        {
            // Setup:
            var parentArtifact = Helper.CreateAndPublishArtifact(_project, _user, artifactType, numberOfVersions: numberOfVersions);
            var childArtifact = Helper.CreateAndPublishArtifact(_project, _user, artifactType, parentArtifact, numberOfVersions: numberOfVersions);

            List<INovaVersionControlArtifactInfo> basicArtifactInfoList = null;

            // Execute:
            Assert.DoesNotThrow(() => basicArtifactInfoList = Helper.ArtifactStore.GetNavigationPath(_user, childArtifact.Id),
                                "'GET {0}' should return 200 OK when passed a valid artifact ID!", SVC_PATH);

            // Verify:
            VerifyParentInformation(basicArtifactInfoList.Last(), childArtifact.ParentId);
        }

        [TestCase]
        [TestRail(183599)]
        [Description("Verify the basic artifact information returned from project is empty.")]
        public void ArtifactNavigation_Project_ReturnsArtifactInfo_200OK()
        {
            List<INovaVersionControlArtifactInfo> basicArtifactInfoList = null;

            // Execute:
            Assert.DoesNotThrow(() => basicArtifactInfoList = Helper.ArtifactStore.GetNavigationPath(_user, _project.Id),
                                "'GET {0}' should return 200 OK when passed a valid artifact ID!", SVC_PATH);

            // Verify:
            Assert.IsEmpty(basicArtifactInfoList, "Project should not have a parent information!");
        }

        //TODO Test for artifact in a folder
        //TODO Test for sub-artifact
        //TODO Test for collection/baseline/review
        //TODO Test for artifact in a long chain of 10 or more folders
        //TODO Test for artifact in a long chain of 10 or more child artifacts
        //TODO Test for artifact in a long chain of mixwd folders and child artifacts. Use TestCase(TestCaseSources.AllArtifactTypesForOpenApiRestMethods)]
        //TODO Test for project in a folder
        //TODO Test for sub-artifact at end of a chain of artifacts.

        #endregion 200 OK tests

        #region Negative tests
        //TODO 400 - The session token is missing or malformed
        //TODO 401 - The session token is invalid.
        //TODO 403 - The user does not have permissions to view the artifact.
        //TODO 404 - An artifact for the specified id is not found, does not exist or is deleted
        #endregion Negative tests

        #region private calls

        /// <summary>
        /// Verifies that an artifact ancestor in a path returns proper values
        /// </summary>
        /// <param name="basicArtifactInfo">Basic information about ancestor artifact/project.</param>
        /// <param name="id">Id of artifact or sub-artifact.</param>
        private void VerifyParentInformation(INovaVersionControlArtifactInfo basicArtifactInfo, int id)
        {
            INovaVersionControlArtifactInfo parentArtifactInfo = null;

            parentArtifactInfo = Helper.ArtifactStore.GetVersionControlInfo(_user, id);

            Assert.AreEqual(parentArtifactInfo.ItemTypeId, basicArtifactInfo.ItemTypeId, "the item type is not item type of a parent!");
            Assert.AreEqual(parentArtifactInfo.Name, basicArtifactInfo.Name, "The name is not the name of the parent!");
            Assert.AreEqual(parentArtifactInfo.ProjectId, basicArtifactInfo.ProjectId, "The project id is not the project id of the parent!");
        }

        #endregion private calls
    }
}
