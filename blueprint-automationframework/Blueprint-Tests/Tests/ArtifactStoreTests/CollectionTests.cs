﻿using System.Collections.Generic;
using CustomAttributes;
using Helper;
using Model;
using Model.Impl;
using Model.ArtifactModel;
using Model.ArtifactModel.Enums;
using Model.ArtifactModel.Impl;
using Model.Factories;
using NUnit.Framework;
using TestCommon;
using Utilities;
using Common;

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

        #region Positive Tests

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

        [TestCase()]
        [TestRail(195445)]
        [Description("Create new collection, add 2 artifacts to collection and save changes, remove 1 artifact and save, check collection content.")]
        public void UpdateCollection_AddTwoArtifactsToCollectionAndSave_RemoveOneArtifactFromCollectionAndSave_CheckContent()
        {
            // Setup:
            var collectionArtifact = CreateCollectionGetCollectionArtifact(_project, _authorUser);
            var artifactToAdd = Helper.CreateAndSaveArtifact(_project, _authorUser, BaseArtifactType.Actor);
            var artifactToRemove = Helper.CreateAndSaveArtifact(_project, _authorUser, BaseArtifactType.Process);
            var collection = Helper.ArtifactStore.GetCollection(_authorUser, collectionArtifact.Id);

            collection.UpdateArtifacts(artifactsIdsToAdd: new List<int> { artifactToAdd.Id, artifactToRemove.Id });
            collectionArtifact.Lock(_authorUser);
            Artifact.UpdateArtifact(collectionArtifact, _authorUser, collection);

            // Execute:
            collection.UpdateArtifacts(artifactsIdsToRemove: new List<int> { artifactToRemove.Id });
            
            Assert.DoesNotThrow(() => { Artifact.UpdateArtifact(collectionArtifact, _authorUser, collection); },
                "Updating collection content should throw no error.");

            // Verify:
            collection = Helper.ArtifactStore.GetCollection(_authorUser, collection.Id);
            Assert.AreEqual(1, collection.Artifacts.Count, "Collection should have 1 artifact");
            CheckCollectionArtifactsHaveExpectedValues(collection.Artifacts, new List<IArtifact> { artifactToAdd });
        }

        [TestCase(1)]
        [TestCase(5)]
        [TestRail(195472)]
        [Description("Create new collection, publish new artifact, add artifact to collection and publish changes, check collection content.")]
        public void UpdateCollection_AddPublishedArtifactsToCollection_Publish_ValidateCollectionContent(int artifactsNumber)
        {
            // Setup:
            var collectionArtifact = CreateCollectionGetCollectionArtifact(_project, _authorUser);
            List<IArtifact> artifactsToAdd = new List<IArtifact>();
            for (int i = 0; i < artifactsNumber; i++)
            {
                artifactsToAdd.Add(Helper.CreateAndPublishArtifact(_project, _authorUser, BaseArtifactType.Actor));
            }
            var collection = Helper.ArtifactStore.GetCollection(_authorUser, collectionArtifact.Id);

            // Execute:
            collection.UpdateArtifacts(artifactsIdsToAdd: artifactsToAdd.ConvertAll(artifact => artifact.Id));
            collectionArtifact.Lock(_authorUser);
            Artifact.UpdateArtifact(collectionArtifact, _authorUser, collection);
            Assert.DoesNotThrow(() => { Helper.ArtifactStore.PublishArtifact(collectionArtifact, _authorUser); },
                "Publishing collection should throw no error.");

            // Verify:
            collection = Helper.ArtifactStore.GetCollection(_adminUser, collection.Id);
            Assert.AreEqual(artifactsNumber, collection.Artifacts.Count, "Collection should have expected number of artifacts.");
            CheckCollectionArtifactsHaveExpectedValues(collection.Artifacts, artifactsToAdd);
        }

        [TestCase]
        [TestRail(230662)]
        [Description("Create new collection, publish new artifact, add artifact to collection, check collection content.")]
        public void AddArtifactToCollection_PublishedArtifact_ValidateCollectionContent()
        {
            // Setup:
            var collectionArtifact = CreateCollectionGetCollectionArtifact(_project, _authorUser);
            var artifact = Helper.CreateAndPublishArtifact(_project, _authorUser, BaseArtifactType.Process);

            int numberOfAddedArtifacts = 0;
            
            // Execute:
            Assert.DoesNotThrow(() => { numberOfAddedArtifacts = Helper.ArtifactStore.AddArtifactToCollection(_authorUser,
                artifact.Id, collectionArtifact.Id); }, "Adding artifact to collection shouldn't throw an error.");

            // Verify:
            Assert.AreEqual(1, numberOfAddedArtifacts, "AddArtifactToCollection should return expected number added artifacts");
            var collection = Helper.ArtifactStore.GetCollection(_authorUser, collectionArtifact.Id);
            Assert.AreEqual(1, collection.Artifacts.Count, "Collection should have expected number of artifacts.");
            CheckCollectionArtifactsHaveExpectedValues(collection.Artifacts, new List<IArtifact> { artifact});
        }

        [TestCase]
        [TestRail(230663)]
        [Description("Create new collection, publish new artifact with child artifact, add artifact to collection," + 
        "check collection content - artifact with its child should be added.")]
        public void AddArtifactToCollection_PublishedArtifactWithChild_ValidateCollectionContent()
        {
            // Setup:
            var collectionArtifact = CreateCollectionGetCollectionArtifact(_project, _authorUser);
            var artifact = Helper.CreateAndPublishArtifact(_project, _authorUser, BaseArtifactType.Actor);
            var childArtifact = Helper.CreateAndPublishArtifact(_project, _authorUser, BaseArtifactType.Actor, artifact);

            int numberOfAddedArtifacts = 0;

            // Execute:
            Assert.DoesNotThrow(() => { numberOfAddedArtifacts = Helper.ArtifactStore.AddArtifactToCollection(_authorUser,
                artifact.Id, collectionArtifact.Id, includeDescendants: true); }, "Adding artifact to collection shouldn't throw an error.");

            // Verify:
            Assert.AreEqual(2, numberOfAddedArtifacts, "AddArtifactToCollection should return expected number added artifacts");
            var collection = Helper.ArtifactStore.GetCollection(_authorUser, collectionArtifact.Id);
            Assert.AreEqual(2, collection.Artifacts.Count, "Collection should have expected number of artifacts.");
            CheckCollectionArtifactsHaveExpectedValues(collection.Artifacts, new List<IArtifact> { artifact, childArtifact });
        }

        #endregion Positive Tests

        #region 409 tests

        [TestCase]
        [TestRail(230664)]
        [Description("Create new collection, ," +
        ".")]
        public void AddArtifactToCollection_CollectionLockedByOtherUser_Returns409()
        {
            // Setup:
            var collectionArtifact = CreateCollectionGetCollectionArtifact(_project, _authorUser);
            Helper.ArtifactStore.PublishArtifact(collectionArtifact, _authorUser);
            collectionArtifact.Lock(_adminUser);

            var artifact = Helper.CreateAndPublishArtifact(_project, _authorUser, BaseArtifactType.Actor);
            
            // Execute:
            var ex = Assert.Throws<Http409ConflictException>(() => { Helper.ArtifactStore.AddArtifactToCollection(_authorUser,
                artifact.Id, collectionArtifact.Id, includeDescendants: true);
            }, "Adding artifact to collection shouldn't throw an error.");

            // Verify:
            var serviceErrorMessage = Deserialization.DeserializeObject<ServiceErrorMessage>(ex.RestResponse.Content);
            string messageText = I18NHelper.FormatInvariant("Failed to lock Collection: {0}.", collectionArtifact.Id);
            Assert.AreEqual(InternalApiErrorCodes.LockedByOtherUser, serviceErrorMessage.ErrorCode, "Error code should have expected value.");
            Assert.AreEqual(messageText, serviceErrorMessage.Message, "Error text should have expected value.");
        }

        #endregion 409 tests

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
            // fake type as far as we don't have Collection in OpenApi
            var collectionArtifact = Helper.CreateWrapAndSaveNovaArtifact(project, user,
                ItemTypePredefined.ArtifactCollection, parentId.Value, baseType: BaseArtifactType.PrimitiveFolder,
                name: name);

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
                Assert.AreEqual(expectedArtifacts[i].BaseArtifactType.ToItemTypePredefined(), collectionArtifacts[i].ItemTypePredefined);
            }
        }
    }
}