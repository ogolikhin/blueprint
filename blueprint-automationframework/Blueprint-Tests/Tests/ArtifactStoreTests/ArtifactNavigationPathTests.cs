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
        [Description("Create & publish an artifact.  Verify get artifact navigation path call returns project information.")]
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
        [Description("Create & save an artifact.  Verify get artifact navigation path call returns project information.")]
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
        [Description("Create & publish an artifact and its child.  Verify get artifact navigation path call returns project parent and project information.")]
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
        [Description("Verify get artifact navigation path call for project returns an empty list.")]
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
        [Description("Create & publish an artifact within a folder. Verify get artifact navigation path call returns folder and project information")]
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
        [Description("Create & publish an artifact with subartifacts. Verify get artifact navigation path call for sub-artifact returns artifact and project information")]
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
        [Description("Create & publish a collection artifact. Verify get artifact navigation path call for collection returns Collections folder and project information.")]
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
        [Description("Verify get artifact navigation path call for Baseline returns Baseline & Review folder and project information")]
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

        [Test, TestCaseSource(typeof(TestCaseSources), nameof(TestCaseSources.AllArtifactTypesForOpenApiRestMethods))]
        [TestRail(183641)]
        [Description("Create & publish an artifact within a chain of 10 folders. Verify get artifact navigation path call for artifact in a chain of folders returns information about all ancestor folders and project.")]
        public void ArtifactNavigation_PublishedArtifactInAChainOfFolders_ReturnsArtifactInfo_200OK(BaseArtifactType artifactType)
        {
            // Setup:
            BaseArtifactType[] artifactTypeChain = { BaseArtifactType.PrimitiveFolder, BaseArtifactType.PrimitiveFolder, BaseArtifactType.PrimitiveFolder, BaseArtifactType.PrimitiveFolder,
                                                     BaseArtifactType.PrimitiveFolder, BaseArtifactType.PrimitiveFolder, BaseArtifactType.PrimitiveFolder, BaseArtifactType.PrimitiveFolder,
                                                     BaseArtifactType.PrimitiveFolder, BaseArtifactType.PrimitiveFolder};

            var folders = Helper.CreatePublishedArtifactChain(_project, _user, artifactTypeChain);

            var artifact = Helper.CreateAndPublishArtifact(_project, _user, artifactType, folders.Last());

            List<INovaVersionControlArtifactInfo> basicArtifactInfoList = null;

            // Execute:
            Assert.DoesNotThrow(() => basicArtifactInfoList = Helper.ArtifactStore.GetNavigationPath(_user, artifact.Id),
                                "'GET {0}' should return 200 OK when passed a valid artifact ID!", SVC_PATH);

            // Verify:
            VerifyParentInformation(basicArtifactInfoList, artifact.ParentId);
        }

        [TestCase(BaseArtifactType.Actor, 2)]
        [TestRail(184481)]
        [Description("Create & publish an artifact within a chain of 10 child artifacts. Verify get artifact navigation path call for artifact in a chain returns information about all ancestor artifacts and project.")]
        public void ArtifactNavigation_PublishedArtifactInAChainOfArtifacts_ReturnsArtifactInfo_200OK(BaseArtifactType artifactType, int numberOfVersions)
        {
            // Setup:
            BaseArtifactType[] artifactTypeChain = { BaseArtifactType.Process, BaseArtifactType.Process, BaseArtifactType.Process, BaseArtifactType.Process,
                                                     BaseArtifactType.Process, BaseArtifactType.Process, BaseArtifactType.Process, BaseArtifactType.Process,
                                                     BaseArtifactType.Process, BaseArtifactType.Process};

            var folders = Helper.CreatePublishedArtifactChain(_project, _user, artifactTypeChain);

            var artifact = Helper.CreateAndPublishArtifact(_project, _user, artifactType, folders.Last(), numberOfVersions: numberOfVersions);

            List<INovaVersionControlArtifactInfo> basicArtifactInfoList = null;

            // Execute:
            Assert.DoesNotThrow(() => basicArtifactInfoList = Helper.ArtifactStore.GetNavigationPath(_user, artifact.Id),
                                "'GET {0}' should return 200 OK when passed a valid artifact ID!", SVC_PATH);

            // Verify:
            VerifyParentInformation(basicArtifactInfoList, artifact.ParentId);
        }

        [TestCase(BaseArtifactType.Actor, 2)]
        [TestRail(184482)]
        [Description("Create & publish an artifact with chains of 10 child artifacts and 10 folders. Move chain of artifacts to one folder before the last. Verify get artifact navigation path call for artifacts and folders in a chain returns information about all ancestor artifacts and project.")]
        public void ArtifactNavigation_PublishedArtifactInAChainOfArtifactsAndFolders_ReturnsArtifactInfo_200OK(BaseArtifactType artifactType, int numberOfVersions)
        {
            // Setup:
            BaseArtifactType[] artifactChain = { BaseArtifactType.Process, BaseArtifactType.Process, BaseArtifactType.Process, BaseArtifactType.Process,
                                                     BaseArtifactType.Process, BaseArtifactType.Process, BaseArtifactType.Process, BaseArtifactType.Process,
                                                     BaseArtifactType.Process, BaseArtifactType.Process};

            BaseArtifactType[] folderChain = { BaseArtifactType.PrimitiveFolder, BaseArtifactType.PrimitiveFolder, BaseArtifactType.PrimitiveFolder, BaseArtifactType.PrimitiveFolder,
                                                     BaseArtifactType.PrimitiveFolder, BaseArtifactType.PrimitiveFolder, BaseArtifactType.PrimitiveFolder, BaseArtifactType.PrimitiveFolder,
                                                     BaseArtifactType.PrimitiveFolder, BaseArtifactType.PrimitiveFolder};

            var artifacts = Helper.CreatePublishedArtifactChain(_project, _user, artifactChain);
            var folders = Helper.CreatePublishedArtifactChain(_project, _user, folderChain);

            artifacts.First().Lock(_user);
            Helper.ArtifactStore.MoveArtifact(artifacts.First(), folders[folders.Count - 2], _user);

            var artifact = Helper.CreateAndPublishArtifact(_project, _user, artifactType, artifacts.Last(), numberOfVersions: numberOfVersions);

            List<INovaVersionControlArtifactInfo> basicArtifactInfoList = null;

            // Execute:
            Assert.DoesNotThrow(() => basicArtifactInfoList = Helper.ArtifactStore.GetNavigationPath(_user, artifact.Id),
                                "'GET {0}' should return 200 OK when passed a valid artifact ID!", SVC_PATH);

            // Verify:
            VerifyParentInformation(basicArtifactInfoList, artifact.ParentId);
        }

        [TestCase(BaseArtifactType.Actor)]
        [TestRail(184483)]
        [Description("Create & save an artifact with chains of 8 child artifacts and 8 folders. Move chain of artifacts to one folder before the last. Verify get artifact navigation path call for artifacts and folders in a chain returns information about all ancestor artifacts and project.")]
        public void ArtifactNavigation_SavedArtifactInAChainOfArtifactsAndFolders_ReturnsArtifactInfo_200OK(BaseArtifactType artifactType)
        {
            // Setup:
            BaseArtifactType[] artifactChain = { BaseArtifactType.Process, BaseArtifactType.Process, BaseArtifactType.Process, BaseArtifactType.Process,
                                                     BaseArtifactType.Process, BaseArtifactType.Process, BaseArtifactType.Process, BaseArtifactType.Process};

            BaseArtifactType[] folderChain = { BaseArtifactType.PrimitiveFolder, BaseArtifactType.PrimitiveFolder, BaseArtifactType.PrimitiveFolder, BaseArtifactType.PrimitiveFolder,
                                                     BaseArtifactType.PrimitiveFolder, BaseArtifactType.PrimitiveFolder, BaseArtifactType.PrimitiveFolder, BaseArtifactType.PrimitiveFolder};

            var artifacts = Helper.CreateSavedArtifactChain(_project, _user, artifactChain);
            var folders = Helper.CreateSavedArtifactChain(_project, _user, folderChain);

            artifacts.First().Lock(_user);
            Helper.ArtifactStore.MoveArtifact(artifacts.First(), folders[folders.Count - 2], _user);

            var artifact = Helper.CreateAndSaveArtifact(_project, _user, artifactType, artifacts.Last());

            List<INovaVersionControlArtifactInfo> basicArtifactInfoList = null;

            // Execute:
            Assert.DoesNotThrow(() => basicArtifactInfoList = Helper.ArtifactStore.GetNavigationPath(_user, artifact.Id),
                                "'GET {0}' should return 200 OK when passed a valid artifact ID!", SVC_PATH);

            // Verify:
            VerifyParentInformation(basicArtifactInfoList, artifact.ParentId);
        }

        [TestCase(BaseArtifactType.UseCase)]
        [TestRail(184484)]
        [Description("Create & publish an artifact with sub-artifacts and chains of 8 child artifacts and 8 folders. Move chain of artifacts to one folder before the last. Verify get artifact navigation path call for artifacts and folders in a chain returns information about all ancestor artifacts and project.")]
        public void ArtifactNavigation_PublishedArtifactWithSubArtifactInAChainOfArtifactsAndFolders_ReturnsArtifactInfo_200OK(BaseArtifactType artifactType)
        {
            // Setup:
            BaseArtifactType[] artifactChain = { BaseArtifactType.Process, BaseArtifactType.Process, BaseArtifactType.Process, BaseArtifactType.Process,
                                                     BaseArtifactType.Process, BaseArtifactType.Process, BaseArtifactType.Process, BaseArtifactType.Process};

            BaseArtifactType[] folderChain = { BaseArtifactType.PrimitiveFolder, BaseArtifactType.PrimitiveFolder, BaseArtifactType.PrimitiveFolder, BaseArtifactType.PrimitiveFolder,
                                                     BaseArtifactType.PrimitiveFolder, BaseArtifactType.PrimitiveFolder, BaseArtifactType.PrimitiveFolder, BaseArtifactType.PrimitiveFolder};

            var artifacts = Helper.CreatePublishedArtifactChain(_project, _user, artifactChain);
            var folders = Helper.CreatePublishedArtifactChain(_project, _user, folderChain);

            artifacts.First().Lock(_user);
            Helper.ArtifactStore.MoveArtifact(artifacts.First(), folders[folders.Count - 2], _user);

            var artifact = Helper.CreateAndPublishArtifact(_project, _user, artifactType, artifacts.Last());

            List<INovaSubArtifact> subArtifacts = Helper.ArtifactStore.GetSubartifacts(_user, artifact.Id);
            Assert.IsTrue(subArtifacts.Count > 0, "There is no sub-artifact in this artifact");

            List<INovaVersionControlArtifactInfo> basicArtifactInfoList = null;

            // Execute:
            Assert.DoesNotThrow(() => basicArtifactInfoList = Helper.ArtifactStore.GetNavigationPath(_user, subArtifacts.Last().Id),
                                "'GET {0}' should return 200 OK when passed a valid artifact ID!", SVC_PATH);

            // Verify:
            VerifyParentInformation(basicArtifactInfoList, subArtifacts.Last().ParentId);
        }

        //TODO Test for project in a folder
        //TODO Move Artifact


        #endregion 200 OK tests

        #region Negative tests
        //TODO 400 - The session token is missing or malformed
        //TODO 401 - The session token is invalid.            
        //TODO 403 - The user does not have permissions to view the artifact.
        //TODO 404 - An artifact for the specified id is not found, does not exist or is deleted
        #endregion Negative tests

        #region private calls

        /// <summary>
        /// Verifies that an artifact ancestors in a path return proper values
        /// </summary>
        /// <param name="basicArtifactInfo">List of artifact basic information about ancestors artifact.</param>
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
