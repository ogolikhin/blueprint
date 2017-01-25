﻿using Common;
using CustomAttributes;
using Helper;
using Model;
using Model.ArtifactModel;
using Model.ArtifactModel.Impl;
using Model.Factories;
using NUnit.Framework;
using System.Collections.Generic;
using TestCommon;
using Utilities;

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
            _authorUser = Helper.CreateUserWithProjectRolePermissions(TestHelper.ProjectRole.AuthorFullAccess, _project);
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
        public void GetCollection_CreateEmptyCollection_ValidateReturnedCollection()
        {
            // Setup:
            var collectionArtifact = Helper.CreateAndSaveCollection(_project, _authorUser);

            // Execute: 
            Collection collection = null;
            Assert.DoesNotThrow(() => collection = Helper.ArtifactStore.GetCollection(_authorUser, collectionArtifact.Id),
                "Get Collection shouldn't return an error.");

            // Verify: Collection is empty and rapid review should not be created for the collection
            Assert.AreEqual(0, collection.Artifacts.Count, "Collection should be empty.");

            // IsCreated is a bolean parameter indicating if Rapid Review is created or not for the collection
            Assert.IsFalse(collection.IsCreated, "RapidReview shouldn't be created.");
        }

        [TestCase()]
        [TestRail(195437)]
        [Description("Create new collection, publish new artifact, add artifact to collection and save changes, check collection content.")]
        public void CreateEmptyCollection_AddArtifactToCollectionAndSave_ValidateCollectionContent()
        {
            // Setup:
            var collectionArtifact = Helper.CreateAndSaveCollection(_project, _authorUser);
            var artifactToAdd = Helper.CreateAndPublishArtifact(_project, _authorUser, BaseArtifactType.Actor);
            var collection = Helper.ArtifactStore.GetCollection(_authorUser, collectionArtifact.Id);

            // Execute:
            collection.UpdateArtifacts(artifactsIdsToAdd: new List<int> { artifactToAdd.Id });
            collectionArtifact.Lock(_authorUser);
            Assert.DoesNotThrow(() => {
                Artifact.UpdateArtifact(collectionArtifact, _authorUser, collection);
            }, "Updating collection content should throw no error.");

            // Verify:
            collection = Helper.ArtifactStore.GetCollection(_authorUser, collection.Id);
            Assert.AreEqual(1, collection.Artifacts.Count, "Collection should have 1 artifact");
            ArtifactStoreHelper.ValidateCollection(collection, new List<IArtifactBase> { artifactToAdd });
        }

        [TestCase()]
        [TestRail(195445)]
        [Description("Create new collection, add 2 artifacts to collection and save changes, remove 1 artifact and save, check collection content.")]
        public void UpdateCollection_AddTwoArtifactsToCollectionAndSave_RemoveOneArtifactFromCollectionAndSave_CheckContent()
        {
            // Setup:
            var collectionArtifact = Helper.CreateAndSaveCollection(_project, _authorUser);
            var artifactToAdd = Helper.CreateAndSaveArtifact(_project, _authorUser, BaseArtifactType.Actor);
            var artifactToRemove = Helper.CreateAndSaveArtifact(_project, _authorUser, BaseArtifactType.Process);
            var collection = Helper.ArtifactStore.GetCollection(_authorUser, collectionArtifact.Id);

            collection.UpdateArtifacts(artifactsIdsToAdd: new List<int> { artifactToAdd.Id, artifactToRemove.Id });
            collectionArtifact.Lock(_authorUser);
            Artifact.UpdateArtifact(collectionArtifact, _authorUser, collection);

            // Execute:
            collection.UpdateArtifacts(artifactsIdsToRemove: new List<int> { artifactToRemove.Id });

            Assert.DoesNotThrow(() => {
                Artifact.UpdateArtifact(collectionArtifact, _authorUser, collection);
            }, "Updating collection content should throw no error.");

            // Verify:
            collection = Helper.ArtifactStore.GetCollection(_authorUser, collection.Id);
            Assert.AreEqual(1, collection.Artifacts.Count, "Collection should have 1 artifact");
            ArtifactStoreHelper.ValidateCollection(collection, new List<IArtifactBase> { artifactToAdd });
        }

        [TestCase(1)]
        [TestCase(5)]
        [TestRail(195472)]
        [Description("Create new collection, publish new artifact, add artifact to collection and publish changes, check collection content.")]
        public void UpdateCollection_AddPublishedArtifactsToCollection_Publish_ValidateCollectionContent(int artifactsNumber)
        {
            // Setup:
            var collectionArtifact = Helper.CreateAndSaveCollection(_project, _authorUser);
            var artifactsToAdd = new List<IArtifact>();
            for (int i = 0; i < artifactsNumber; i++)
            {
                artifactsToAdd.Add(Helper.CreateAndPublishArtifact(_project, _authorUser, BaseArtifactType.Actor));
            }
            var collection = Helper.ArtifactStore.GetCollection(_authorUser, collectionArtifact.Id);

            // Execute:
            collection.UpdateArtifacts(artifactsIdsToAdd: artifactsToAdd.ConvertAll(artifact => artifact.Id));
            collectionArtifact.Lock(_authorUser);
            Artifact.UpdateArtifact(collectionArtifact, _authorUser, collection);
            Assert.DoesNotThrow(() => {
                Helper.ArtifactStore.PublishArtifact(collectionArtifact, _authorUser);
            }, "Publishing collection should throw no error.");

            // Verify:
            collection = Helper.ArtifactStore.GetCollection(_adminUser, collection.Id);
            Assert.AreEqual(artifactsNumber, collection.Artifacts.Count, "Collection should have expected number of artifacts.");
            ArtifactStoreHelper.ValidateCollection(collection, artifactsToAdd.ConvertAll(a => (IArtifactBase)a));
        }

        [Test, TestCaseSource(typeof(TestCaseSources), nameof(TestCaseSources.AllArtifactTypesForOpenApiRestMethods))]
        [TestRail(230662)]
        [Description("Create new collection, publish new artifact, add artifact to collection, check collection content.")]
        public void AddArtifactToCollection_PublishedArtifact_ValidateCollectionContent(BaseArtifactType artifactTypeToAdd)
        {
            // Setup:
            var collectionArtifact = Helper.CreateAndSaveCollection(_project, _authorUser);
            var artifactToAdd = Helper.CreateAndPublishArtifact(_project, _authorUser, artifactTypeToAdd);
            
            int numberOfAddedArtifacts = 0;

            // Execute:
            Assert.DoesNotThrow(() => {
                    numberOfAddedArtifacts = Helper.ArtifactStore.AddArtifactToCollection(_authorUser, artifactToAdd.Id, collectionArtifact.Id);
            }, "Adding artifact to collection shouldn't throw an error.");
            
            // Verify:
            Assert.AreEqual(1, numberOfAddedArtifacts, "AddArtifactToCollection should return expected number added artifacts");
            var collection = Helper.ArtifactStore.GetCollection(_authorUser, collectionArtifact.Id);
            Assert.AreEqual(1, collection.Artifacts.Count, "Collection should have expected number of artifacts.");
            ArtifactStoreHelper.ValidateCollection(collection, new List<IArtifactBase> { artifactToAdd });
        }

        [TestCase(true, 2)]
        [TestCase(false, 1)]
        [TestRail(230663)]
        [Description("Create new collection, publish new artifact with child artifact, add artifact to collection," + 
        "check collection content - artifact with its child should be added.")]
        public void AddArtifactToCollection_PublishedArtifactWithChild_ValidateCollectionContent(bool includeDescendants,
            int expectedNumberOfAddedArtifacts)
        {
            // Setup:
            var collectionArtifact = Helper.CreateAndSaveCollection(_project, _authorUser);
            var artifact = Helper.CreateAndPublishArtifact(_project, _authorUser, BaseArtifactType.Actor);
            var childArtifact = Helper.CreateAndPublishArtifact(_project, _authorUser, BaseArtifactType.Actor, artifact);

            var addedArtifacts = new List<IArtifact> { artifact };
            if (includeDescendants)
            {
                addedArtifacts.Add(childArtifact);
            }

            int numberOfAddedArtifacts = 0;

            // Execute:
            Assert.DoesNotThrow(() => {
                numberOfAddedArtifacts = Helper.ArtifactStore.AddArtifactToCollection(_authorUser, artifact.Id, collectionArtifact.Id,
                includeDescendants: includeDescendants);
            }, "Adding artifact to collection shouldn't throw an error.");

            // Verify:
            Assert.AreEqual(expectedNumberOfAddedArtifacts, numberOfAddedArtifacts, "AddArtifactToCollection should return expected number added artifacts");
            var collection = Helper.ArtifactStore.GetCollection(_authorUser, collectionArtifact.Id);
            Assert.AreEqual(expectedNumberOfAddedArtifacts, collection.Artifacts.Count, "Collection should have expected number of artifacts.");
            ArtifactStoreHelper.ValidateCollection(collection, addedArtifacts.ConvertAll(a => (IArtifactBase)a));
        }

        [TestCase]
        [TestRail(230673)]
        [Description("Create new collection, create and save new artifact, add artifact to collection, check collection content.")]
        public void AddArtifactToCollection_SavedArtifact_ValidateCollectionContent()
        {
            // Setup:
            var collectionArtifact = Helper.CreateAndSaveCollection(_project, _authorUser);
            var artifact = Helper.CreateAndSaveArtifact(_project, _authorUser, BaseArtifactType.Process);

            int numberOfAddedArtifacts = 0;

            // Execute:
            Assert.DoesNotThrow(() => {
                numberOfAddedArtifacts = Helper.ArtifactStore.AddArtifactToCollection(_authorUser, artifact.Id, collectionArtifact.Id);
            }, "Adding artifact to collection shouldn't throw an error.");

            // Verify:
            Assert.AreEqual(1, numberOfAddedArtifacts, "AddArtifactToCollection should return expected number added artifacts");
            var collection = Helper.ArtifactStore.GetCollection(_authorUser, collectionArtifact.Id);
            Assert.AreEqual(1, collection.Artifacts.Count, "Collection should have expected number of artifacts.");
            ArtifactStoreHelper.ValidateCollection(collection, new List<IArtifactBase> { artifact });
        }

        [TestCase]
        [TestRail(230674)]
        [Description("Create new collection, create and save new artifact, add artifact to collection," + 
            "check artifact's description from collection content.")]
        public void AddArtifactToCollection_SavedArtifact_CheckDescriptionFromCollectionContent()
        {
            // Setup:
            var collectionArtifact = Helper.CreateAndSaveCollection(_project, _authorUser);
            var artifact = Helper.CreateAndSaveArtifact(_project, _authorUser, BaseArtifactType.Actor);
            var artifactDetails = Helper.ArtifactStore.GetArtifactDetails(_authorUser, artifact.Id);
            const string descriptionText = "Description";
            ArtifactStoreHelper.SetArtifactTextProperty(artifactDetails, _authorUser, Helper.ArtifactStore, descriptionText);

            int numberOfAddedArtifacts = 0;

            // Execute:
            Assert.DoesNotThrow(() => {
                numberOfAddedArtifacts = Helper.ArtifactStore.AddArtifactToCollection(_authorUser, artifact.Id, collectionArtifact.Id);
            }, "Adding artifact to collection shouldn't throw an error.");

            // Verify:
            Assert.AreEqual(1, numberOfAddedArtifacts, "AddArtifactToCollection should return expected number added artifacts");
            var collection = Helper.ArtifactStore.GetCollection(_authorUser, collectionArtifact.Id);
            Assert.AreEqual(1, collection.Artifacts.Count, "Collection should have expected number of artifacts.");
            ArtifactStoreHelper.ValidateCollection(collection, new List<IArtifactBase> { artifact });
            Assert.AreEqual(descriptionText, collection.Artifacts[0].Description, "Description should have expected value.");
        }

        #endregion Positive Tests

        #region 40x tests

        [TestCase]
        [TestRail(230664)]
        [Description("Create and publish new collection, lock collection by other user," +
        "try to add artifact to collection. 409 exception should be returned. Check error message.")]
        public void AddArtifactToCollection_CollectionLockedByOtherUser_Returns409()
        {
            // Setup:
            var collectionArtifact = Helper.CreateAndSaveCollection(_project, _authorUser);

            Helper.ArtifactStore.PublishArtifact(collectionArtifact, _authorUser);
            collectionArtifact.Lock(_adminUser);

            var artifact = Helper.CreateAndPublishArtifact(_project, _authorUser, BaseArtifactType.Actor);
            
            // Execute:
            var ex = Assert.Throws<Http409ConflictException>(() => {
                Helper.ArtifactStore.AddArtifactToCollection(_authorUser, artifact.Id, collectionArtifact.Id, includeDescendants: true);
            }, "Adding artifact to collection shouldn't throw an error.");

            // Verify:
            string messageText = I18NHelper.FormatInvariant("Failed to lock Collection: {0}.", collectionArtifact.Id);
            TestHelper.ValidateServiceError(ex.RestResponse, InternalApiErrorCodes.LockedByOtherUser, messageText);
        }

        [TestCase]
        [TestRail(230675)]
        [Description("Create new collection, delete published artifact (don't publish information about deletion)," +
        "add this artifact to collection - 404 exception should be returned. Check exception message.")]
        public void AddArtifactToCollection_ArtifactMarkedForDeletion_Returns404()
        {
            // Setup:
            var collectionArtifact = Helper.CreateAndSaveCollection(_project, _authorUser);
            Helper.ArtifactStore.PublishArtifact(collectionArtifact, _authorUser);
            
            var artifact = Helper.CreateAndPublishArtifact(_project, _authorUser, BaseArtifactType.Actor);
            artifact.Delete(_authorUser);

            // Execute:
            var ex = Assert.Throws<Http404NotFoundException>(() => {
                Helper.ArtifactStore.AddArtifactToCollection(_authorUser, artifact.Id, collectionArtifact.Id, includeDescendants: true);
            }, "Adding artifact to collection shouldn't throw an error.");

            // Verify:
            const string messageText = "You have attempted to access an artifact that does not exist or has been deleted.";
            TestHelper.ValidateServiceError(ex.RestResponse, InternalApiErrorCodes.ItemNotFound, messageText);
        }

        #endregion 40x tests
    }
}
