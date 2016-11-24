using System.Collections.Generic;
using CustomAttributes;
using Helper;
using Model;
using Model.ArtifactModel;
using Model.ArtifactModel.Enums;
using Model.ArtifactModel.Impl;
using Model.Factories;
using NUnit.Framework;
using TestCommon;

namespace ArtifactStoreTests
{
    [TestFixture]
    [Category(Categories.ArtifactStore)]
    public class CollectionTests : TestBase
    {
        private IUser _adminUser = null;
        private IUser _authorUser = null;
        private IProject _project = null;

        [SetUp]
        public void SetUp()
        {
            Helper = new TestHelper();
            _adminUser = Helper.CreateUserAndAuthenticate(TestHelper.AuthenticationTokenTypes.BothAccessControlAndOpenApiTokens);
            _project = ProjectFactory.GetProject(_adminUser);
            _project.GetAllNovaArtifactTypes(Helper.ArtifactStore, _adminUser);
            _authorUser = Helper.CreateUserWithProjectRolePermissions(TestHelper.ProjectRole.Author, _project);
        }

        [TearDown]
        public void TearDown()
        {
            Helper?.Dispose();
        }

        [TestCase()]
        [TestRail(195436)]
        [Description("Create new collection, get collection content and validate it.")]
        public void CreateEmptyCollection_GetCollectionContent_Validate()
        {
            CreateCollectionGetCollectionArtifact(_project, _authorUser);
        }

        [TestCase()]
        [TestRail(195437)]
        [Description("Create new collection, publish new artifact, add artifact to collection and save changes, check collection content.")]
        public void CreateEmptyCollection_AddArtifactToCollectionAndSave_ValidateCollectionContent()
        {
            // Setup:
            var collectionArtifact = CreateCollectionGetCollectionArtifact(_project, _authorUser);
            var artifactToAdd = Helper.CreateAndPublishArtifact(_project, _authorUser, BaseArtifactType.Actor);
            var collection = Helper.ArtifactStore.GetCollection(_authorUser, collectionArtifact.Id);

            // Execute:
            collection.UpdateArtifacts(artifactsIdsToAdd: new List<int> { artifactToAdd.Id });
            collectionArtifact.Lock(_authorUser);
            Assert.DoesNotThrow(() => { Artifact.UpdateArtifact(collectionArtifact, _authorUser, collection); },
                "Updating collection content should throw no error.");

            // Verify:
            collection = Helper.ArtifactStore.GetCollection(_authorUser, collection.Id);
            Assert.AreEqual(1, collection.Artifacts.Count, "Collection should have 1 artifact");
            CheckCollectionArtifactsHaveExpectedValues(collection.Artifacts, new List<IArtifact> { artifactToAdd });
        }

        /// <summary>
        /// Creates empty collection and return corresponding IArtifact.
        /// </summary>
        /// <param name="project">Project to create collection.</param>
        /// <param name="user">The user to authenticate with.</param>
        /// <param name="parentId">(optional) Id of artifact under which collection should be created (no check for valid location).</param>
        /// <param name="name">(optional) The name of collection.</param>
        /// <returns>IArtifact which corresponds to the created collection.</returns>
        private IArtifact CreateCollectionGetCollectionArtifact(IProject project, IUser user, int? parentId = null, string name = null)
        {
            var collectionFolder = _project.GetDefaultCollectionFolder(Helper.ArtifactStore.Address, _authorUser);
            parentId = collectionFolder.Id;

            var collectionArtifact = Helper.CreateWrapAndSaveNovaArtifact(project, user,
                ItemTypePredefined.ArtifactCollection, parentId.Value, name: name);

            Collection collection = null;
            Assert.DoesNotThrow(() =>
                collection = Helper.ArtifactStore.GetCollection(_authorUser, collectionArtifact.Id),
                "GetCollection shouldn't throw no error.");
            Assert.AreEqual(0, collection.Artifacts.Count, "Collection should be empty.");
            Assert.IsFalse(collection.IsCreated, "RapidReview shouldn't be created.");

            return collectionArtifact;
        }

        /// <summary>
        /// Compares list of CollectionItem with list of IArtifact.
        /// </summary>
        /// <param name="collectionArtifacts">List of CollectionItem.</param>
        /// <param name="expectedArtifacts">List of IArtifact.</param>
        private static void CheckCollectionArtifactsHaveExpectedValues (List<CollectionItem> collectionArtifacts,
            List<IArtifact> expectedArtifacts)
        {
            Assert.AreEqual(collectionArtifacts.Count, expectedArtifacts.Count, "Number of artifacts should be the same");
            for (int i = 0; i < collectionArtifacts.Count; i++)
            {
                Assert.AreEqual(expectedArtifacts[i].Id, collectionArtifacts[i].Id, "Id should have expected vaule");
                Assert.AreEqual(expectedArtifacts[i].ArtifactTypeId, collectionArtifacts[i].ItemTypeId);
                Assert.AreEqual(expectedArtifacts[i].Name, collectionArtifacts[i].Name);
                /* Assert.AreEqual(expectedArtifacts[i].BaseArtifactType, collectionArtifacts[i].ItemTypePredefined);
                ItemTypePredefined is a number defined in enum ItemTypePredefined
                TODO: make proper comparison */
            }
        }
    }
}