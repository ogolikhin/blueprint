﻿using CustomAttributes;
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
        public void ArtifactNavigation_PublishedArtifact_ReturnsProjectInfo(BaseArtifactType artifactType, int numberOfVersions)
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
        public void ArtifactNavigation_SavedArtifact_ReturnsProjectInfo(BaseArtifactType artifactType)
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
        public void ArtifactNavigation_PublishedArtifactWithAChild_ReturnsParentArtifactAndProjectInfo(BaseArtifactType artifactType, int numberOfVersions)
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
        public void ArtifactNavigation_Project_ReturnsEmptyList()
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
        public void ArtifactNavigation_PublishedArtifactInAFolder_ReturnsFolderAndProjectInfo(BaseArtifactType artifactType, int numberOfVersions)
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
        public void ArtifactNavigation_SubArtifactIdOfPublishedArtifact_ReturnsArtifactAndProjectInfo(BaseArtifactType artifactType, int numberOfVersions)
        {
            // Setup:
            var artifact = Helper.CreateAndPublishArtifact(_project, _user, artifactType, numberOfVersions: numberOfVersions);

            List<INovaSubArtifact> subArtifacts = Helper.ArtifactStore.GetSubartifacts(_user, artifact.Id);
            Assert.IsTrue(subArtifacts.Count > 0, "There is no sub-artifact in this artifact");

            List<INovaVersionControlArtifactInfo> basicArtifactInfoList = null;

            // Execute:
            Assert.DoesNotThrow(() => basicArtifactInfoList = Helper.ArtifactStore.GetNavigationPath(_user, subArtifacts.First().Id),
                                "'GET {0}' should return 200 OK when passed a valid sub-artifact ID!", SVC_PATH);

            // Verify:
            VerifyParentInformation(basicArtifactInfoList, subArtifacts.First().ParentId);
        }

        [TestCase(BaselineAndCollectionTypePredefined.ArtifactCollection)]
        [TestCase(BaselineAndCollectionTypePredefined.CollectionFolder)]
        [TestRail(183630)]
        [Description("Create & publish a collection artifact. Verify get artifact navigation path call for collection returns Collections folder and project information.")]
        public void ArtifactNavigation_Collection_ReturnsCollectionFolderAndProjectInfo(ItemTypePredefined artifactType)
        {
            // Setup:
            INovaArtifact collectionFolder = _project.GetDefaultCollectionFolder(Helper.ArtifactStore.Address, _user);

            var collection = ArtifactStore.CreateArtifact(Helper.ArtifactStore.Address,
                _user, artifactType, "Collection test", _project, collectionFolder.Id);

            List<INovaVersionControlArtifactInfo> basicArtifactInfoList = null;

            IArtifact fakeArtifact = null;

            try
            {
                fakeArtifact = ArtifactFactory.CreateArtifact(
                    _project, _user, BaseArtifactType.Actor, collection.Id);   // Don't use Helper because this isn't a real artifact, it's just wrapping the bad artifact ID.

                // Execute:
                Assert.DoesNotThrow(() => basicArtifactInfoList = Helper.ArtifactStore.GetNavigationPath(_user, collection.Id),
                                    "'GET {0}' should return 200 OK when passed a valid artifact ID!", SVC_PATH);

                // Verify:
                VerifyParentInformation(basicArtifactInfoList, collection.ParentId);
            }
            finally
            {
                fakeArtifact.Discard(_user);
            }
        }

        [Ignore(IgnoreReasons.UnderDevelopment)] //Artifacts for Baseline and Review need to be added to Custom Data project
        [Category(Categories.CustomData)]
        [TestCase(96384)]
        [TestRail(185119)]
        [Description("Verify get artifact navigation path call for Baseline returns Baseline & Review folder and project information")]
        public void ArtifactNavigation_Baseline_ReturnsCollectionFolderAndProjectInfo(int id)
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

        [TestCase(BaseArtifactType.Actor, 10, BaseArtifactType.PrimitiveFolder)]
        [TestRail(183641)]
        [Description("Create & publish an artifact within a chain of 10 folders. Verify get artifact navigation path call for artifact in a chain of folders returns information about all ancestor folders and project.")]
        public void ArtifactNavigation_PublishedArtifactInAChainOfFolders_ReturnsListOfFoldersAndProjectInfo(BaseArtifactType artifactType, int numberOfArtifacts, BaseArtifactType folderType)
        {
            // Setup:
            List<BaseArtifactType> artifactTypes = CreateListOfArtifactTypes(numberOfArtifacts, folderType);

            var folders = Helper.CreatePublishedArtifactChain(_project, _user, artifactTypes.ToArray());

            var artifact = Helper.CreateAndPublishArtifact(_project, _user, artifactType, folders.Last());

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
        #endregion private calls
    }
}
