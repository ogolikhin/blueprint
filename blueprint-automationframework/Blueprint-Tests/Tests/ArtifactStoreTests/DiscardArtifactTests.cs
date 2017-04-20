using CustomAttributes;
using Helper;
using Model;
using Model.ArtifactModel;
using Model.Factories;
using Newtonsoft.Json;
using NUnit.Framework;
using System.Collections.Generic;
using System.Linq;
using Common;
using Model.Impl;
using TestCommon;
using Utilities;
using Model.ArtifactModel.Enums;
using Model.ModelHelpers;

namespace ArtifactStoreTests
{
    [TestFixture]
    [Category(Categories.ArtifactStore)]
    public class DiscardArtifactTests : TestBase
    {
        private IUser _user = null;
        private IProject _project = null;
        private const string DISCARD_PATH = RestPaths.Svc.ArtifactStore.Artifacts.DISCARD;
        private const string UPDATE_ARTIFACT_ID_PATH = RestPaths.Svc.ArtifactStore.ARTIFACTS_id_;
        
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

        #region 200 OK Tests

        [TestCase(2, BaseArtifactType.Actor)]
        [TestRail(166133)]
        [Description("Create & save multiple artifacts.  Discard all saved artifacts.  Verify all saved artifacts are discarded.")]
        public void DiscardArtifacts_MultipleSavedArtifacts_VerifyArtifactsAreDiscarded(int numberOfArtifacts, BaseArtifactType artifactType)
        {
            // Setup:
            var author = Helper.CreateUserWithProjectRolePermissions(TestHelper.ProjectRole.Author, _project);

            var savedArtifacts = Helper.CreateAndSaveMultipleArtifacts(_project, author, artifactType, numberOfArtifacts);
            INovaArtifactsAndProjectsResponse discardArtifactResponse = null;

            // Execute:
            Assert.DoesNotThrow(() => discardArtifactResponse = Helper.ArtifactStore.DiscardArtifacts(savedArtifacts, author),
                "'POST {0}' should return 200 OK after saved artifacts are discarded!", DISCARD_PATH);

            // Verify:
            Assert.AreEqual(savedArtifacts.Count, discardArtifactResponse.Artifacts.Count,
                "There should be {0} artifacts returned in discard results!", savedArtifacts.Count);
            Assert.AreEqual(1, discardArtifactResponse.Projects.Count, "There should be 1 project returned in discard results!");
            VerifyDiscardResponse(discardArtifactResponse, savedArtifacts);
        }

        [TestCase(2, BaseArtifactType.Actor)]
        [TestRail(166134)]
        [Description("Create & save multiple artifacts.  Discard with all=true.  Verify all saved artifacts are discarded.")]
        public void DiscardAllArtifacts_MultipleSavedArtifacts_VerifyArtifactsAreDiscarded(int numberOfArtifacts, BaseArtifactType artifactType)
        {
            // Setup:
            var author = Helper.CreateUserWithProjectRolePermissions(TestHelper.ProjectRole.Author, _project);

            var savedArtifacts = Helper.CreateAndSaveMultipleArtifacts(_project, author, artifactType, numberOfArtifacts);
            INovaArtifactsAndProjectsResponse discardArtifactResponse = null;

            // Execute:
            Assert.DoesNotThrow(
                () => discardArtifactResponse = Helper.ArtifactStore.DiscardArtifacts(artifacts: null, user: author, all: true),
                "'POST {0}?all=true' should return 200 OK after saved artifacts are discarded!", DISCARD_PATH);

            try
            {
                // Verify:
                Assert.AreEqual(savedArtifacts.Count, discardArtifactResponse.Artifacts.Count,
                    "There should be {0} artifacts returned in discard results!", savedArtifacts.Count);
                Assert.AreEqual(1, discardArtifactResponse.Projects.Count, "There should be 1 project returned in discard results!");
                VerifyDiscardResponse(discardArtifactResponse, savedArtifacts);
            }
            finally
            {
                // Need to manually update the IsSaved flags so the TearDown doesn't fail.
                savedArtifacts.ForEach(a => a.IsSaved = false);
            }
        }

        [TestCase(2, BaseArtifactType.Actor)]
        [TestRail(166135)]
        [Description("Create & publish multiple artifacts, then change & save them.  Discard all the changed artifacts.  " +
            "Verify all changed artifacts are discarded.")]
        public void DiscardArtifacts_MultiplePublishedArtifactsWithDrafts_VerifyArtifactsAreDiscarded(int numberOfArtifacts, BaseArtifactType artifactType)
        {
            // Setup:
            var author = Helper.CreateUserWithProjectRolePermissions(TestHelper.ProjectRole.Author, _project);

            var changedPublishedArtifacts = Helper.CreateAndPublishMultipleArtifacts(_project, author, artifactType, numberOfArtifacts);

            foreach (var publishedArtifact in changedPublishedArtifacts.ConvertAll(x => (IArtifact)x))
            {
                publishedArtifact.Save();
            }

            INovaArtifactsAndProjectsResponse discardArtifactResponse = null;

            // Execute:
            Assert.DoesNotThrow(() => discardArtifactResponse = Helper.ArtifactStore.DiscardArtifacts(changedPublishedArtifacts, author),
                "'POST {0}' should return 200 OK when discarding published artifacts with drafts!", DISCARD_PATH);

            // Verify:
            Assert.AreEqual(changedPublishedArtifacts.Count, discardArtifactResponse.Artifacts.Count,
                "There should be {0} artifacts returned in discard results!", changedPublishedArtifacts.Count);
            Assert.AreEqual(1, discardArtifactResponse.Projects.Count, "There should be 1 project returned in discard results!");
            VerifyDiscardResponse(discardArtifactResponse, changedPublishedArtifacts);
        }

        [TestCase(2, BaseArtifactType.Actor)]
        [TestRail(166136)]
        [Description("Create & publish multiple artifacts, then change & save them.  Discard with all=true.  Verify all changed artifacts are discarded.")]
        public void DiscardAllArtifacts_MultiplePublishedArtifactsWithDrafts_VerifyArtifactsAreDiscarded(
            int numberOfArtifacts, BaseArtifactType artifactType)
        {
            // Setup:
            var author = Helper.CreateUserWithProjectRolePermissions(TestHelper.ProjectRole.Author, _project);

            var changedPublishedArtifacts = Helper.CreateAndPublishMultipleArtifacts(_project, author, artifactType, numberOfArtifacts);

            foreach (var publishedArtifact in changedPublishedArtifacts.ConvertAll(x => (IArtifact)x))
            {
                publishedArtifact.Save();
            }

            INovaArtifactsAndProjectsResponse discardArtifactResponse = null;

            // Execute:
            Assert.DoesNotThrow(() => discardArtifactResponse = Helper.ArtifactStore.DiscardArtifacts(changedPublishedArtifacts, author, all: true),
                "'POST {0}?all=true' should return 200 OK after saved artifacts are discarded!", DISCARD_PATH);

            // Verify:
            Assert.AreEqual(changedPublishedArtifacts.Count, discardArtifactResponse.Artifacts.Count,
                "There should be {0} artifacts returned in discard results!", changedPublishedArtifacts.Count);
            Assert.AreEqual(1, discardArtifactResponse.Projects.Count, "There should be 1 project returned in discard results!");
            VerifyDiscardResponse(discardArtifactResponse, changedPublishedArtifacts);
        }

        [TestCase(2, BaseArtifactType.Actor)]
        [TestRail(166137)]
        [Description("Create & publish multiple artifacts.  Discard with all=true.  Verify no discarded projects or artifacts are returned.")]
        public void DiscardAllArtifacts_MultiplePublishedArtifacts_VerifyNothingToDiscard(int numberOfArtifacts, BaseArtifactType artifactType)
        {
            // Setup:
            var author = Helper.CreateUserWithProjectRolePermissions(TestHelper.ProjectRole.Author, _project);

            var publishedArtifacts = Helper.CreateAndPublishMultipleArtifacts(_project, author, artifactType, numberOfArtifacts);

            INovaArtifactsAndProjectsResponse discardArtifactResponse = null;

            // Execute:
            Assert.DoesNotThrow(() => discardArtifactResponse = Helper.ArtifactStore.DiscardArtifacts(publishedArtifacts, author, all: true),
                "'POST {0}?all=true' should return 200 OK after saved artifacts are discarded!", DISCARD_PATH);

            // Verify:
            Assert.AreEqual(0, discardArtifactResponse.Artifacts.Count, "There should be no artifacts returned in discard results!");
            Assert.AreEqual(0, discardArtifactResponse.Projects.Count, "There should be no projects returned in discard results!");
        }

        [TestCase(2, BaseArtifactType.Actor)]
        [TestRail(166138)]
        [Description("Create a list of saved artifacts as well as published artifacts.  Discard with all=true.  Verify saved artifacts are discarded.")]
        public void DiscardAllArtifacts_MixedListOfPublishedAndSavedArtifacts_SavedArtifactsAreDiscarded(int numberOfArtifacts, BaseArtifactType artifactType)
        {
            // Setup:
            var author = Helper.CreateUserWithProjectRolePermissions(TestHelper.ProjectRole.Author, _project);

            var publishedArtifacts = Helper.CreateAndPublishMultipleArtifacts(_project, author, artifactType, numberOfArtifacts);

            var savedArtifacts = Helper.CreateAndSaveMultipleArtifacts(_project, author, artifactType, numberOfArtifacts);

            var mixedArtifacts = new List<IArtifactBase>();
            mixedArtifacts.AddRange(publishedArtifacts);
            mixedArtifacts.AddRange(savedArtifacts);

            INovaArtifactsAndProjectsResponse discardArtifactResponse = null;

            // Execute:
            Assert.DoesNotThrow(() => discardArtifactResponse = Helper.ArtifactStore.DiscardArtifacts(mixedArtifacts, author, all: true),
                "'POST {0}?all=true' should return 200 OK after saved artifacts are discarded!", DISCARD_PATH);

            // Verify:
            Assert.AreEqual(savedArtifacts.Count, discardArtifactResponse.Artifacts.Count,
                "There should be {0} artifacts returned in discard results!", savedArtifacts.Count);
            Assert.AreEqual(1, discardArtifactResponse.Projects.Count, "There should be 1 project returned in discard results!");
            VerifyDiscardResponse(discardArtifactResponse, savedArtifacts);
        }

        #region Custom data tests 

        [Category(Categories.CustomData)]
        [TestCase(2, BaseArtifactType.Actor, "value\":10.0", "value\":999.0")]  //Insert value into Numeric field which is out of range 
        [TestCase(2, BaseArtifactType.Actor, "value\":\"20", "value\":\"21")]   //Insert value into Date field which is out of range 
        [TestRail(182270)] 
        [Description("Create & publish multiple artifacts.  Update all with out of range properties.  Discard the artifacts.  " +
            "Verify all changed artifacts are discarded.")] 
        public void DiscardArtifact_PublishedArtifactsInDraftWithOutOfRangeProperties_ChangedArtifactsAreDiscarded(
            int numberOfArtifacts, BaseArtifactType artifactType, string toChange, string changeTo)
        {
            // Setup: 
            var projectCustomData = ArtifactStoreHelper.GetCustomDataProject(_user);

            var author = Helper.CreateUserWithProjectRolePermissions(TestHelper.ProjectRole.Author, projectCustomData);

            var publishedArtifacts = Helper.CreateAndPublishMultipleArtifacts(projectCustomData, author, artifactType, numberOfArtifacts); 
  
            for (int i = 0; i < numberOfArtifacts; i++) 
            {
                var artifactDetails = Helper.ArtifactStore.GetArtifactDetails(author, publishedArtifacts[i].Id);

                string requestBody = JsonConvert.SerializeObject(artifactDetails);
                requestBody = requestBody.Replace(toChange, changeTo);

                var artifact = (IArtifact)publishedArtifacts[i];
                artifact.Lock(author);

                Assert.DoesNotThrow(() => ArtifactStoreHelper.UpdateInvalidArtifact(Helper.BlueprintServer.Address, requestBody, artifact.Id, author), 
                    "'PATCH {0}' should return 200 OK if properties are out of range!", UPDATE_ARTIFACT_ID_PATH); 
            }

            INovaArtifactsAndProjectsResponse discardArtifactResponse = null; 
    
            // Execute: 
            Assert.DoesNotThrow(() => discardArtifactResponse = Helper.ArtifactStore.DiscardArtifacts(publishedArtifacts, author),
                "'POST {0}' should return 200 OK when discarding artifacts with out of range properties!", DISCARD_PATH);

            // Verify:
            Assert.AreEqual(publishedArtifacts.Count, discardArtifactResponse.Artifacts.Count,
                "There should be {0} artifacts returned in discard results!", publishedArtifacts.Count);
            Assert.AreEqual(1, discardArtifactResponse.Projects.Count, "There should be 1 project returned in discard results!");
            VerifyDiscardResponse(discardArtifactResponse, publishedArtifacts);
        }

        [Category(Categories.CustomData)]
        [TestCase(2, BaseArtifactType.Actor, "value\":10.0", "value\":999.0")]  //Insert value into Numeric field which is out of range
        [TestCase(2, BaseArtifactType.Actor, "value\":\"20", "value\":\"21")]   //Insert value into Date field which is out of range
        [TestRail(182271)]
        [Description("Create & publish multiple artifacts.  Update one with out of range properties.  Discard with all=true.  " +
            "Verify the changed artifact is discarded.")]
        public void DiscardAllArtifacts_PublishedArtifacts_OneInDraftWithOutOfRangeProperties_ChangedArtifactIsDiscarded(
            int numberOfArtifacts, BaseArtifactType artifactType, string toChange, string changeTo)
        {
            // Setup:
            var projectCustomData = ArtifactStoreHelper.GetCustomDataProject(_user);

            var author = Helper.CreateUserWithProjectRolePermissions(TestHelper.ProjectRole.AuthorFullAccess, projectCustomData);

            var publishedArtifacts = Helper.CreateAndPublishMultipleArtifacts(projectCustomData, author, artifactType, numberOfArtifacts);
            var firstArtifact = (IArtifact)publishedArtifacts[0];

            var artifactDetails = Helper.ArtifactStore.GetArtifactDetails(author, firstArtifact.Id);

            string requestBody = JsonConvert.SerializeObject(artifactDetails);
            requestBody = requestBody.Replace(toChange, changeTo);

            firstArtifact.Lock(author);

            Assert.DoesNotThrow(() => ArtifactStoreHelper.UpdateInvalidArtifact(Helper.BlueprintServer.Address, requestBody, firstArtifact.Id, author),
                "'PATCH {0}' should return 200 OK if properties are out of range!", UPDATE_ARTIFACT_ID_PATH);

            INovaArtifactsAndProjectsResponse discardArtifactResponse = null;

            // Execute:
            Assert.DoesNotThrow(() => discardArtifactResponse = Helper.ArtifactStore.DiscardArtifacts(artifacts: null, user: author, all : true),
                "'POST {0}?all=true' should return 200 OK when discarding artifacts with out of range properties!", DISCARD_PATH);

            // Verify:
            Assert.AreEqual(1, discardArtifactResponse.Artifacts.Count, "There should be 1 artifacts returned in discard results!");
            Assert.AreEqual(1, discardArtifactResponse.Projects.Count, "There should be 1 project returned in discard results!");
            VerifyDiscardResponse(discardArtifactResponse, new List<IArtifactBase> { firstArtifact });
        }

        #endregion Custom data tests

        [TestCase(4, ItemTypePredefined.Process)]
        [TestRail(182307)]
        [Description("Create & publish a chain of artifacts.  Move the child artifacts.  Discard with all=true.  Verify all moved artifacts are discarded.")]
        public void DiscardAllArtifacts_ChainOfPublishedArtifacts_MoveChildArtifacts_MovedArtifactAreDiscarded(
            int numberOfArtifacts, ItemTypePredefined artifactType)
        {
            // Setup:
            var author = Helper.CreateUserWithProjectRolePermissions(TestHelper.ProjectRole.Author, _project);

            var artifactTypes = CreateListOfArtifactTypes(numberOfArtifacts, artifactType);

            var artifactChain = Helper.CreatePublishedArtifactChain(_project, author, artifactTypes.ToArray());

            var changedArtifacts = new List<ArtifactWrapper>();

            for (int i = 1; i < numberOfArtifacts; i++)
            {
                artifactChain[i].Lock(author);
                artifactChain[i].MoveArtifact(author, artifactChain[0].Id);
                changedArtifacts.Add(artifactChain[i]);
            }

            INovaArtifactsAndProjectsResponse discardArtifactResponse = null;

            // Execute:
            Assert.DoesNotThrow(() => discardArtifactResponse = Helper.ArtifactStore.DiscardArtifacts(artifacts: null, user: author, all: true),
                "'POST {0}?all=true' should return 200 OK if the dependent children of the artifact have been moved!", DISCARD_PATH);

            // Verify:
            Assert.AreEqual(changedArtifacts.Count, discardArtifactResponse.Artifacts.Count,
                "There should be {0} artifacts returned in discard results!", changedArtifacts.Count);
            Assert.AreEqual(1, discardArtifactResponse.Projects.Count, "There should be 1 project returned in discard results!");
            VerifyDiscardResponse(discardArtifactResponse, changedArtifacts);
        }

        [TestCase(3, ItemTypePredefined.Process)]
        [TestRail(182330)]
        [Description("Create & publish chains of artifacts in different projects.  Move the child artifacts.  Discard with all=true.  " +
            "Verify all moved artifacts are discarded.")]
        public void DiscardAllArtifacts_ChainsOfPublishedArtifactsInDifferentProjects_MoveChildArtifacts_MovedArtifactAreDiscarded(
            int numberOfArtifacts, ItemTypePredefined artifactType)
        {
            // Setup:
            var artifactTypes = CreateListOfArtifactTypes(numberOfArtifacts, artifactType);

            var projects = ProjectFactory.GetProjects(_user, numberOfProjects: 2);

            var author = Helper.CreateUserWithProjectRolePermissions(TestHelper.ProjectRole.AuthorFullAccess, projects);

            var artifactChainInProject1 = Helper.CreatePublishedArtifactChain(projects[0], author, artifactTypes.ToArray());
            var artifactChainInProject2 = Helper.CreatePublishedArtifactChain(projects[1], author, artifactTypes.ToArray());

            var changedArtifacts = new List<ArtifactWrapper>();

            for (int i = 1; i < numberOfArtifacts; i++)
            {
                artifactChainInProject1[i].Lock(author);
                artifactChainInProject1[i].MoveArtifact(author, artifactChainInProject1[0].Id);
                changedArtifacts.Add(artifactChainInProject1[i]);

                artifactChainInProject2[i].Lock(author);
                artifactChainInProject2[i].MoveArtifact(author, artifactChainInProject2[0].Id);
                changedArtifacts.Add(artifactChainInProject2[i]);
            }

            INovaArtifactsAndProjectsResponse discardArtifactResponse = null;

            // Execute:
            Assert.DoesNotThrow(() => discardArtifactResponse = Helper.ArtifactStore.DiscardArtifacts(artifacts: null, user: author, all: true),
                "'POST {0}?all=true' should return 200 OK if the changed artifacts are discarded in different projects!", DISCARD_PATH);

            // Verify:
            Assert.AreEqual(changedArtifacts.Count, discardArtifactResponse.Artifacts.Count,
                "There should be {0} artifacts returned in discard results!", changedArtifacts.Count);
            Assert.AreEqual(2, discardArtifactResponse.Projects.Count, "There should be 2 projects returned in discard results!");
            VerifyDiscardResponse(discardArtifactResponse, changedArtifacts, author);
        }

        [TestCase(3, ItemTypePredefined.Process)]
        [TestRail(182316)]
        [Description("Create & publish a chain of artifacts.  Delete the last artifact.  Discard the deleted artifact.  Verify the deleted artifact is discarded.")]
        public void DiscardArtifact_DeleteLastArtifactInChainOfPublishedArtifacts_SendDeletedArtifact_ArtifactSuccessfullyDiscarded(
            int numberOfArtifacts, ItemTypePredefined artifactType)
        {
            // Setup:
            var author = Helper.CreateUserWithProjectRolePermissions(TestHelper.ProjectRole.AuthorFullAccess, _project);

            var artifactTypes = CreateListOfArtifactTypes(numberOfArtifacts, artifactType);

            var artifactChain = Helper.CreatePublishedArtifactChain(_project, author, artifactTypes.ToArray());

            var lastArtifact = artifactChain.Last();
            lastArtifact.Delete(author);

            INovaArtifactsAndProjectsResponse discardArtifactResponse = null;

            // Execute:
            Assert.DoesNotThrow(() => discardArtifactResponse = lastArtifact.Discard(author),
                "'POST {0}' should return 200 OK when discarding artifacts with unpublished deletes!", DISCARD_PATH);

            // Verify:
            Assert.AreEqual(1, discardArtifactResponse.Artifacts.Count, "Only 1 artifact should be returned in discard results!");
            Assert.AreEqual(1, discardArtifactResponse.Projects.Count, "Only 1 project should be returned in discard results!");
            VerifyDiscardResponse(discardArtifactResponse, new List<ArtifactWrapper> { lastArtifact });
        }

        [TestCase(3, ItemTypePredefined.Process)]
        [TestRail(182317)]
        [Description("Create & publish a chain of artifacts.  Delete the last artifact.  Discard with all=true.  Verify the deleted artifact is discarded.")]
        public void DiscardAllArtifacts_DeleteLastArtifactInChainOfPublishedArtifacts_SendEmptyList_ArtifactSuccessfullyDiscarded(
            int numberOfArtifacts, ItemTypePredefined artifactType)
        {
            // Setup:
            var author = Helper.CreateUserWithProjectRolePermissions(TestHelper.ProjectRole.AuthorFullAccess, _project);

            var artifactTypes = CreateListOfArtifactTypes(numberOfArtifacts, artifactType);

            var artifactChain = Helper.CreatePublishedArtifactChain(_project, author, artifactTypes.ToArray());
            var lastArtifact = artifactChain.Last();
            lastArtifact.Delete(author);

            INovaArtifactsAndProjectsResponse discardArtifactResponse = null;

            // Execute:
            Assert.DoesNotThrow(() => discardArtifactResponse = Helper.ArtifactStore.DiscardArtifacts(artifacts: null, user: author, all: true),
                "'POST {0}?all=true' should return 200 OK if an artifact was deleted!", DISCARD_PATH);

            // Verify:
            Assert.AreEqual(1, discardArtifactResponse.Artifacts.Count, "Only 1 artifact should be returned in discard results!");
            Assert.AreEqual(1, discardArtifactResponse.Projects.Count, "Only 1 project should be returned in discard results!");
            VerifyDiscardResponse(discardArtifactResponse, new List<ArtifactWrapper> { lastArtifact });
        }

        [TestCase]
        [TestRail(267244)]
        [Description("Create and publish Baseline, add artifact to Baseline, Discard Baseline - Baseline should be empty.")]
        public void DiscardArtifact_BaselineWithAddedArtifact_ReturnsBaselineToEmpty()
        {
            // Setup:
            var artifactToAdd = Helper.CreateNovaArtifactInSpecificState(_user, _project, TestHelper.TestArtifactState.Published, ItemTypePredefined.Actor,
                _project.Id);

            var baselineArtifact = Helper.CreateBaseline(_user, _project);
            Helper.ArtifactStore.PublishArtifacts(new List<int> { baselineArtifact.Id }, _user);
            Helper.ArtifactStore.AddArtifactToBaseline(_user, artifactToAdd.Id, baselineArtifact.Id);

            INovaArtifactsAndProjectsResponse discardArtifactResponse = null;

            // Execute:
            Assert.DoesNotThrow(() => {
                discardArtifactResponse = ArtifactStore.DiscardArtifacts(Helper.ArtifactStore.Address,
                    new List<int> { baselineArtifact.Id }, _user);
            }, "Discard of Baseline shouldn't throw an error.");

            // Verify:
            Assert.AreEqual(1, discardArtifactResponse.Artifacts.Count, "Expected number of artifacts should be discarded.");
            var discardedBaseline = Helper.ArtifactStore.GetBaseline(_user, baselineArtifact.Id);
            Assert.IsEmpty(discardedBaseline.Artifacts, "After Discard baseline should be empty.");
        }

        #endregion 200 OK Tests

        #region 400 Bad Request tests

        [TestCase]
        [TestRail(166145)]
        [Description("Send empty list of artifacts and Discard, checks returned result is 400 Bad Request.")]
        public void DiscardArtifacts_EmptyArtifactList_400BadRequest()
        {
            // Setup:
            var artifacts = new List<IArtifactBase>();

            // Execute:
            var ex = Assert.Throws<Http400BadRequestException>(() => Helper.ArtifactStore.DiscardArtifacts(artifacts, _user),
            "'POST {0}' should return 400 Bad Request if body of the request does not have any artifact ids!", DISCARD_PATH);
            
            // Verify:
            TestHelper.ValidateServiceError(ex.RestResponse, InternalApiErrorCodes.IncorrectInputParameters, "The list of artifact Ids is empty.");
        }

        #endregion 400 Bad Request tests

        #region 401 Unauthorized tests

        [TestCase]
        [TestRail(166148)]
        [Description("Create & save a single artifact.  Discard the artifact with wrong token.  Verify it returns 401 Unauthorized.")]
        public void DiscardArtifact_InvalidToken_401Unauthorized()
        {
            // Setup:
            var artifact = Helper.CreateAndSaveArtifact(_project, _user, BaseArtifactType.Actor);

            var userWithBadToken = Helper.CreateUserWithInvalidToken(TestHelper.AuthenticationTokenTypes.AccessControlToken);

            // Execute:
            var ex = Assert.Throws<Http401UnauthorizedException>(() => Helper.ArtifactStore.DiscardArtifact(artifact, userWithBadToken),
                "'POST {0}' should return 401 Unauthorized if a token is invalid!", DISCARD_PATH);

            // Verify:
            const string expectedExceptionMessage = "Unauthorized call";
            Assert.That(ex.RestResponse.Content.Contains(expectedExceptionMessage),
                "{0} was not found in returned message of discard published artifact(s) which has invalid token.", expectedExceptionMessage);
        }

        #endregion 401 Unauthorized tests

        #region 404 Not Found tests

        [TestCase]
        [TestRail(166152)]
        [Description("Create & publish an artifact, then delete & publish the artifact.  Try to discard the deleted artifact.  Verify it returns 404 Not Found.")]
        public void DiscardArtifact_PublishedArtifactDeleted_404NotFound()
        {
            // Setup:
            var artifact = Helper.CreateAndPublishArtifact(_project, _user, BaseArtifactType.Process);

            artifact.Delete(_user);
            artifact.Publish(_user);

            // Execute:
            var ex = Assert.Throws<Http404NotFoundException>(() => Helper.ArtifactStore.DiscardArtifact(artifact, _user),
                "'POST {0}' should return 404 Not Found if the Artifact was deleted!", DISCARD_PATH);

            // Verify:
            string expectedExceptionMessage = "Artifact with ID " + artifact.Id + " is deleted.";
            TestHelper.ValidateServiceError(ex.RestResponse, InternalApiErrorCodes.ItemNotFound, expectedExceptionMessage);
        }

        [TestCase(0, "Item with ID {0} is not an artifact.")]
        [TestCase(int.MaxValue, "Item with ID {0} is not found.")]
        [TestRail(166153)]
        [Description("Try to discard an artifact with a non-existent Artifact ID.  Verify 404 Not Found is returned.")]
        public void DiscardArtifact_NonExistentArtifactId_404NotFound(int nonExistentArtifactId, string expectedErrorMessage)
        {
            // Setup:
            var artifact = Helper.CreateArtifact(_project, _user, BaseArtifactType.Process);

            // Replace ProjectId with a fake ID that shouldn't exist.
            artifact.Id = nonExistentArtifactId;

            // Execute:
            var ex = Assert.Throws<Http404NotFoundException>(() => Helper.ArtifactStore.DiscardArtifact(artifact, _user),
            "'POST {0}' should return 404 Not Found if the Artifact ID doesn't exist", DISCARD_PATH);

            // Verify:
            string expectedExceptionMessage = I18NHelper.FormatInvariant(expectedErrorMessage, artifact.Id);
            TestHelper.ValidateServiceError(ex.RestResponse, InternalApiErrorCodes.ItemNotFound, expectedExceptionMessage);
        }

        #endregion 404 Not Found tests

        #region 409 Conflict tests

        [TestCase(2, BaseArtifactType.Actor)]
        [TestRail(166139)]
        [Description("Create & publish some artiacts.  Discard the published artifacts.  Verify it returns 409 Conflict with 'nothing to discard' message.")]
        public void DiscardArtifacts_DiscardPublishedArtifactsWithNoChildren_409Conflict(int numberOfArtifacts, BaseArtifactType artifactType)
        {
            // Setup:
            var publishedArtifacts = Helper.CreateAndPublishMultipleArtifacts(_project, _user, artifactType, numberOfArtifacts);

            // Execute:
            var ex = Assert.Throws<Http409ConflictException>(() => Helper.ArtifactStore.DiscardArtifacts(publishedArtifacts, _user),
                "We should get a 409 Conflict when a user trying to discard published artifact(s) which has nothing to discard!");

            TestHelper.ValidateServiceError(ex.RestResponse, InternalApiErrorCodes.CannotDiscard,
                I18NHelper.FormatInvariant("Artifact with ID {0} has nothing to discard.", publishedArtifacts[0].Id));
        }

        [TestCase(2, BaseArtifactType.Actor)]
        [TestRail(166141)]
        [Description("Set mixed list of saved and published artifacts.  Discard the artifacts.  Verify it returns 409 Conflict with 'nothing to discard' message.")]
        public void DiscardArtifacts_DiscardMixedListOfSavedPublishedArtifacts_409Conflict(int numberOfArtifacts, BaseArtifactType artifactType)
        {
            // Setup:
            var savedArtifacts = Helper.CreateAndSaveMultipleArtifacts(_project, _user, artifactType, numberOfArtifacts);

            var publishedArtifacts = Helper.CreateAndPublishMultipleArtifacts(_project, _user, artifactType, numberOfArtifacts);

            var mixedArtifacts = new List<IArtifactBase>();
            mixedArtifacts.AddRange(savedArtifacts);
            mixedArtifacts.AddRange(publishedArtifacts);

            // Execute:
            var ex = Assert.Throws<Http409ConflictException>(() => Helper.ArtifactStore.DiscardArtifacts(mixedArtifacts, _user),
                "We should get a 409 Conflict when a user trying to discard published artifact(s) which has nothing to discard!");

            // Validation: Exception should contain expected message.
            TestHelper.ValidateServiceError(ex.RestResponse, InternalApiErrorCodes.CannotDiscard,
                I18NHelper.FormatInvariant("Artifact with ID {0} has nothing to discard.", mixedArtifacts[2].Id));
        }

        [TestCase(ItemTypePredefined.Process, ItemTypePredefined.TextualRequirement, ItemTypePredefined.Glossary, ItemTypePredefined.UseCase)]
        [TestRail(166158)]
        [Description("Create & publish a chain of parent & child artifacts, move all children to parent.  Discard last child artifact.  Verify it returns 409 Conflict.")]
        public void DiscardArtifact_PublishChainOfParentAndChildArtifacts_MoveChildrenToParent_OnlyDiscardLastChild_409Conflict(params ItemTypePredefined[] artifactTypes)
        {
            ThrowIf.ArgumentNull(artifactTypes, nameof(artifactTypes));

            // Setup:
            var artifactChain = Helper.CreatePublishedArtifactChain(_project, _user, artifactTypes);

            for (int i = 1; i < artifactTypes.Length; i++)
            {
                artifactChain[i].Lock(_user);
                artifactChain[i].MoveArtifact(_user, artifactChain[0].Id);
            }

            // Execute:
            var ex = Assert.Throws<Http409ConflictException>(() => artifactChain.Last().Discard(_user),
                "'POST {0}' should return 409 Conflict if the Artifact has parent artifact which is not discarded!", DISCARD_PATH);

            // Verify:
            TestHelper.ValidateServiceError(ex.RestResponse, InternalApiErrorCodes.CannotDiscardOverDependencies,
                "Specified artifacts have dependent artifacts to discard.");
        }

        [TestCase(BaseArtifactType.Process, BaseArtifactType.TextualRequirement, BaseArtifactType.UseCase)]
        [TestRail(182333)]
        [Description("Create & publish grand parent, parent & child artifacts in a chain.  Swap the parent & child artifacts.  Discard the child artifact.  " +
            "Verify it returns 409 Conflict.")]
        public void DiscardArtifact_GrandParentAndParentAndChildArtifacts_SwapParentWithChild_OnlyDiscardChild_409Conflict(
            BaseArtifactType grandParentType, BaseArtifactType parentType, BaseArtifactType childType)
        {
            // Setup:
            var grandParentArtifact = Helper.CreateAndPublishArtifact(_project, _user, grandParentType);
            var parentArtifact = Helper.CreateAndPublishArtifact(_project, _user, parentType, grandParentArtifact);
            var childArtifact = Helper.CreateAndPublishArtifact(_project, _user, childType, parentArtifact);

            SwapTwoArtifacts(ref parentArtifact, ref childArtifact);

            // Execute:
            var ex = Assert.Throws<Http409ConflictException>(() => Helper.ArtifactStore.DiscardArtifact(childArtifact, _user),
                "'POST {0}' should return 409 Conflict if the Artifact has parent artifact which is not discarded!", DISCARD_PATH);

            // Verify:
            TestHelper.ValidateServiceError(ex.RestResponse, InternalApiErrorCodes.CannotDiscardOverDependencies,
                "Specified artifacts have dependent artifacts to discard.");
        }

        [TestCase(BaseArtifactType.Process, BaseArtifactType.UseCase, BaseArtifactType.TextualRequirement)]
        [TestRail(182334)]
        [Description("Create & publish grand parent, parent & child artifacts in a chain.  Move last child to grand parent & delete the parent.  " +
            "Discard the child artifact.  Verify it returns 409 Conflict.")]
        public void DiscardArtifact_GrandParentAndParentAndChildArtifacts_DeleteParent_OnlyDiscardChild_409Conflict(
            BaseArtifactType grandParentType, BaseArtifactType parentType, BaseArtifactType childType)
        {
            // Setup:
            var grandParentArtifact = Helper.CreateAndPublishArtifact(_project, _user, grandParentType);
            var parentArtifact = Helper.CreateAndPublishArtifact(_project, _user, parentType, grandParentArtifact);
            var childArtifact = Helper.CreateAndPublishArtifact(_project, _user, childType, parentArtifact);

            childArtifact.Lock();
            Helper.ArtifactStore.MoveArtifact(childArtifact, grandParentArtifact, _user);

            parentArtifact.Lock();
            parentArtifact.Delete();

            // Execute:
            var ex = Assert.Throws<Http409ConflictException>(() => Helper.ArtifactStore.DiscardArtifact(childArtifact, _user),
                "'POST {0}' should return 409 Conflict if the Artifact has deleted parent artifact which is not discarded!", DISCARD_PATH);

            // Verify:
            TestHelper.ValidateServiceError(ex.RestResponse, InternalApiErrorCodes.CannotDiscardOverDependencies,
                "Specified artifacts have dependent artifacts to discard.");
        }

        #endregion 409 Conflict tests

        #region private call

        /// <summary>
        /// Creates a list of artifact types.
        /// </summary>
        /// <param name="numberOfArtifacts">The number of artifact types to add to the list.</param>
        /// <param name="artifactType">The artifact type.</param>
        /// <returns>A list of artifact types.</returns>
        private static List<ItemTypePredefined> CreateListOfArtifactTypes(int numberOfArtifacts, ItemTypePredefined artifactType)
        {
            var artifactTypes = new List<ItemTypePredefined>();

            for (int i = 0; i < numberOfArtifacts; i++)
            {
                artifactTypes.Add(artifactType);
            }

            return artifactTypes;
        }

        /// <summary>
        /// Asserts that returned artifact details from the discard call match with artifacts that were discarded.
        /// </summary>
        /// <param name="discardArtifactResponse">The response from Nova discard call.</param>
        /// <param name="artifactsTodiscard">artifacts that are being discarded</param>
        /// <param name="user">(optional) The user to authenticate with.  By default _user is used.</param>
        private void VerifyDiscardResponse(
            INovaArtifactsAndProjectsResponse discardArtifactResponse,
            List<IArtifactBase> artifactsTodiscard,
            IUser user = null)
        {
            ThrowIf.ArgumentNull(discardArtifactResponse, nameof(discardArtifactResponse));
            ThrowIf.ArgumentNull(artifactsTodiscard, nameof(artifactsTodiscard));
            var tempIds = new List<int>();
            discardArtifactResponse.Artifacts.ForEach(a => tempIds.Add(a.Id));

            user = user ?? _user;

            foreach (var artifact in artifactsTodiscard)
            {
                Assert.That(tempIds.Contains(artifact.Id),
                    "The discarded artifact whose Id is {0} does not exist on the response from the discard call.",artifact.Id);

                // Try to get the artifact and verify that you get a 404 if it was never published, or you can get it if it was published.
                if (artifact.IsPublished)
                {
                    Assert.DoesNotThrow(() => Helper.ArtifactStore.GetArtifactDetails(user, artifact.Id),
                        "Artifact ID {0} should still exist after discard!", artifact.Id);
                }
                else
                {
                    Assert.Throws<Http404NotFoundException>(() => Helper.ArtifactStore.GetArtifactDetails(user, artifact.Id),
                        "Artifact ID {0} should not exist after discard!", artifact.Id);
                }
            }
        }

        /// <summary>
        /// Asserts that returned artifact details from the discard call match with artifacts that were discarded.
        /// </summary>
        /// <param name="discardArtifactResponse">The response from Nova discard call.</param>
        /// <param name="artifactsTodiscard">artifacts that are being discarded</param>
        /// <param name="user">(optional) The user to authenticate with.  By default _user is used.</param>
        private void VerifyDiscardResponse(
            INovaArtifactsAndProjectsResponse discardArtifactResponse,
            List<ArtifactWrapper> artifactsTodiscard,
            IUser user = null)
        {
            ThrowIf.ArgumentNull(discardArtifactResponse, nameof(discardArtifactResponse));
            ThrowIf.ArgumentNull(artifactsTodiscard, nameof(artifactsTodiscard));
            var tempIds = new List<int>();
            discardArtifactResponse.Artifacts.ForEach(a => tempIds.Add(a.Id));

            user = user ?? _user;

            foreach (var artifact in artifactsTodiscard)
            {
                Assert.That(tempIds.Contains(artifact.Id),
                    "The discarded artifact whose Id is {0} does not exist on the response from the discard call.", artifact.Id);

                // Try to get the artifact and verify that you get a 404 if it was never published, or you can get it if it was published.
                if (artifact.ArtifactState.IsPublished)
                {
                    Assert.DoesNotThrow(() => Helper.ArtifactStore.GetArtifactDetails(user, artifact.Id),
                        "Artifact ID {0} should still exist after discard!", artifact.Id);
                }
                else
                {
                    Assert.Throws<Http404NotFoundException>(() => Helper.ArtifactStore.GetArtifactDetails(user, artifact.Id),
                        "Artifact ID {0} should not exist after discard!", artifact.Id);
                }
            }
        }

        /// <summary>
        /// Swaps parent artifact with it's child.
        /// </summary>
        /// <param name="firstArtifact">Parent artifact to swap</param>
        /// <param name="secondArtifact">Child artifact to swap</param>
        private void SwapTwoArtifacts(ref IArtifact firstArtifact, ref IArtifact secondArtifact)
        {
            ThrowIf.ArgumentNull(firstArtifact, nameof(firstArtifact));
            ThrowIf.ArgumentNull(secondArtifact, nameof(secondArtifact));

            Assert.AreNotEqual(firstArtifact.Id, secondArtifact.Id, "The first & second artifacts are the same!");

            int oldParentOfFirstArtifact = firstArtifact.ParentId;

            secondArtifact.Lock();
            Helper.ArtifactStore.MoveArtifact(secondArtifact, _project.Id, _user);

            firstArtifact.Lock();
            Helper.ArtifactStore.MoveArtifact(firstArtifact, secondArtifact, _user);

            secondArtifact.Lock();
            Helper.ArtifactStore.MoveArtifact(secondArtifact, oldParentOfFirstArtifact, _user);
        }

        #endregion private call
    }
}
