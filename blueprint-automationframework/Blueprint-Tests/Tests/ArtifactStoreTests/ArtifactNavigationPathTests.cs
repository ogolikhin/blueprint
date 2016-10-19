using CustomAttributes;
using Helper;
using Model;
using Model.ArtifactModel;
using Model.Factories;
using NUnit.Framework;
using System.Collections.Generic;
using TestCommon;
using System.Linq;
using Model.Impl;
using Model.ArtifactModel.Enums;
using Model.ArtifactModel.Impl;

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
            _project.GetAllNovaArtifactTypes(Helper.ArtifactStore, _user);
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
            VerifyParentInformation(basicArtifactInfoList, artifact.ParentId);
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
            VerifyParentInformation(basicArtifactInfoList, artifact.ParentId);
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
            VerifyParentInformation(basicArtifactInfoList, childArtifact.ParentId);
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

        [TestCase(BaseArtifactType.Actor, 2)]
        [TestRail(183607)]
        [Description("Create & publish an artifact within a folder. Verify the basic artifact information returned from folder has correct values.")]
        public void ArtifactNavigation_PublishedArtifactInAFolder_ReturnsArtifactInfo_200OK(BaseArtifactType artifactType, int numberOfVersions)
        {
            // Setup:
            var folder = Helper.CreateAndPublishArtifact(_project, _user, BaseArtifactType.PrimitiveFolder);
            var artifact = Helper.CreateAndPublishArtifact(_project, _user, artifactType, folder, numberOfVersions: numberOfVersions);

            List<INovaVersionControlArtifactInfo> basicArtifactInfoList = null;

            // Execute:
            Assert.DoesNotThrow(() => basicArtifactInfoList = Helper.ArtifactStore.GetNavigationPath(_user, artifact.Id),
                                "'GET {0}' should return 200 OK when passed a valid artifact ID!", SVC_PATH);

            // Verify:
            VerifyParentInformation(basicArtifactInfoList, artifact.ParentId);
        }

        [TestCase(BaseArtifactType.UseCase, 2)]
        [TestCase(BaseArtifactType.Process, 2)]
        [TestRail(183608)]
        [Description("Create & publish an artifact with subartifacts. Verify the basic artifact information returned from artifact has correct values.")]
        public void ArtifactNavigation_PublishedArtifactWithSubArtifacts_ReturnsArtifactInfo_200OK(BaseArtifactType artifactType, int numberOfVersions)
        {
            // Setup:
            var artifact = Helper.CreateAndPublishArtifact(_project, _user, artifactType, numberOfVersions: numberOfVersions);

            List<INovaSubArtifact> subArtifacts = Helper.ArtifactStore.GetSubartifacts(_user, artifact.Id);
            Assert.IsTrue(subArtifacts.Count > 0, "There is no sub-artifact in this artifact");

            List<INovaVersionControlArtifactInfo> basicArtifactInfoList = null;

            // Execute:
            Assert.DoesNotThrow(() => basicArtifactInfoList = Helper.ArtifactStore.GetNavigationPath(_user, subArtifacts.First().Id),
                                "'GET {0}' should return 200 OK when passed a valid artifact ID!", SVC_PATH);

            // Verify:
            VerifyParentInformation(basicArtifactInfoList, subArtifacts.First().ParentId);
        }

        [TestCase(BaselineAndCollectionTypePredefined.ArtifactCollection)]
        [TestCase(BaselineAndCollectionTypePredefined.CollectionFolder)]
        [TestRail(183630)]
        [Description("Create & publish a collection artifact. Verify the basic artifact information returned from Collections folder has correct values.")]
        public void ArtifactNavigation_Collection_ReturnsArtifactInfo_200OK(ItemTypePredefined artifactType)
        {
            // Setup:
            var collectionFolder = GetDefaultCollectionFolder(_project, _user);

            var collection = ArtifactStore.CreateArtifact(Helper.ArtifactStore.Address,
                _user, artifactType, "Collection test", _project, collectionFolder.Id);
            
            List<INovaVersionControlArtifactInfo> basicArtifactInfoList = null;

            // Execute:
            Assert.DoesNotThrow(() => basicArtifactInfoList = Helper.ArtifactStore.GetNavigationPath(_user, collection.Id),
                                "'GET {0}' should return 200 OK when passed a valid artifact ID!", SVC_PATH);

            // Verify:
            VerifyParentInformation(basicArtifactInfoList, collection.ParentId);
        }

        [Ignore(IgnoreReasons.UnderDevelopment)] //Artifacts for Baseline and Review need to be added to Custom Data project
        [TestCase(96384)]
        [TestRail(0)]
        [Description("Verify the basic artifact information returned from baseline folder has correct values.")]
        public void ArtifactNavigation_Baseline_ReturnsArtifactInfo_200OK(int id)
        {
            // Setup:
            var basicArtifactInfo = Helper.ArtifactStore.GetVersionControlInfo(_user, id);

            List<INovaVersionControlArtifactInfo> basicArtifactInfoList = null;

            Assert.IsNotNull(basicArtifactInfo, "Cannot navigate to artifact which id is null!");

            // Execute:
            Assert.DoesNotThrow(() => basicArtifactInfoList = Helper.ArtifactStore.GetNavigationPath(_user, basicArtifactInfo.Id),
                                "'GET {0}' should return 200 OK when passed a valid artifact ID!", SVC_PATH);

            // Verify:
            VerifyParentInformation(basicArtifactInfoList, basicArtifactInfo.ParentId);
        }

        [TestCase(BaseArtifactType.Actor, 2)]
        [TestRail(183641)]
        [Description("Create & publish an artifact within a chain of 10 folders. Verify the basic artifact information returned from folder has correct values.")]
        public void ArtifactNavigation_PublishedArtifactInAChainOfFolders_ReturnsArtifactInfo_200OK(BaseArtifactType artifactType, int numberOfVersions)
        {
            // Setup:
            BaseArtifactType[] artifactTypeChain = { BaseArtifactType.PrimitiveFolder, BaseArtifactType.PrimitiveFolder, BaseArtifactType.PrimitiveFolder, BaseArtifactType.PrimitiveFolder,
                                                     BaseArtifactType.PrimitiveFolder, BaseArtifactType.PrimitiveFolder, BaseArtifactType.PrimitiveFolder, BaseArtifactType.PrimitiveFolder,
                                                     BaseArtifactType.PrimitiveFolder, BaseArtifactType.PrimitiveFolder};

            var folders = Helper.CreatePublishedArtifactChain(_project, _user, artifactTypeChain);

            var artifact = Helper.CreateAndPublishArtifact(_project, _user, artifactType, folders.Last(), numberOfVersions: numberOfVersions);

            List<INovaVersionControlArtifactInfo> basicArtifactInfoList = null;

            // Execute:
            Assert.DoesNotThrow(() => basicArtifactInfoList = Helper.ArtifactStore.GetNavigationPath(_user, artifact.Id),
                                "'GET {0}' should return 200 OK when passed a valid artifact ID!", SVC_PATH);

            // Verify:
            VerifyParentInformation(basicArtifactInfoList, artifact.ParentId);
        }

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
        private void VerifyParentInformation(List<INovaVersionControlArtifactInfo> basicArtifactInfo, int? id)
        {
            INovaVersionControlArtifactInfo parentArtifactInfo = null;

            basicArtifactInfo.Reverse();

            foreach (var artifactinfo in basicArtifactInfo)
            {
                Assert.IsNotNull(id, "Cannot verify values of artifact with id value null!");

                parentArtifactInfo = Helper.ArtifactStore.GetVersionControlInfo(_user, (int)id);

                Assert.AreEqual(parentArtifactInfo.ItemTypeId, artifactinfo.ItemTypeId, "the item type is not item type of a parent!");
                Assert.AreEqual(parentArtifactInfo.Name, artifactinfo.Name, "The name is not the name of the parent!");
                Assert.AreEqual(parentArtifactInfo.ProjectId, artifactinfo.ProjectId, "The project id is not the project id of the parent!");

                if (id != _project.Id)
                    id = (int)parentArtifactInfo.ParentId;
            }
        }

        /// <summary>
        /// Gets the default Collections folder for the project and returns only the Id, PredefinedType, ProjectId and ItemTypeId.
        /// </summary>
        /// <param name="project">The project whose collections folder you want to get.</param>
        /// <param name="user">The user to authenticate with.</param>
        /// <returns>The default Collections folder for the project.</returns>
        private INovaArtifactBase GetDefaultCollectionFolder(IProject project, IUser user)
        {
            INovaArtifact collectionFolder = project.GetDefaultCollectionFolder(Helper.ArtifactStore.Address, user);

            return new NovaArtifactDetails
            {
                Id = collectionFolder.Id,
                PredefinedType = collectionFolder.PredefinedType,
                ProjectId = project.Id,
                ItemTypeId = collectionFolder.ItemTypeId
            };
        }

        #endregion private calls
    }
}
