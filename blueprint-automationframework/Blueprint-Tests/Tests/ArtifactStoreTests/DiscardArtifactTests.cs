using CustomAttributes;
using Helper;
using Model;
using Model.ArtifactModel;
using Model.Factories;
using NUnit.Framework;
using System.Collections.Generic;
using TestCommon;
using Utilities;

namespace ArtifactStoreTests
{
    [TestFixture]
    [Category(Categories.ArtifactStore)]
    public class DiscardArtifactTests : TestBase
    {
        private IUser _user = null;
        private IProject _project = null;
        const string DISCARD_PATH = RestPaths.Svc.ArtifactStore.Artifacts.DISCARD;

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
        [Description("Set draft artifacts by just saving. Execute Discard - Must return successful discard response")]
        //Create process artifact, save, don't publish, discard - must return successfully discarded.
        public void DiscardArtifacts_DiscardSavedArtifacts_VerifyArtifactDiscarded(int numberOfArtifacts, BaseArtifactType artifactType)
        {
            // Setup:
            // Create artifact(s) with save for discard test
            var savedArtifacts = Helper.CreateAndSaveMultipleArtifacts(_project, _user, artifactType, numberOfArtifacts);
            INovaArtifactsAndProjectsResponse discardArtifactResponse = null;

            // Execute:
            Assert.DoesNotThrow(() => discardArtifactResponse = Helper.ArtifactStore.DiscardArtifacts(savedArtifacts, _user),
                "DiscardArtifacts() failed when discarding saved artifact(s)!");

            // Validation: Verify that discarded artifact information from discardedArtifactResponse match with that from savedArtifacts

            Assert.That(discardArtifactResponse.Artifacts.Count.Equals(savedArtifacts.Count),
                "Number of discarded artifact is {0} but discarded item count from the response of the discard is {1}",
                savedArtifacts.Count, discardArtifactResponse.Artifacts.Count);

            DiscardVerification(discardArtifactResponse, savedArtifacts);
        }

        [TestCase(2, BaseArtifactType.Actor)]
        [TestRail(166134)]
        [Description("Set draft artifacts by just saving. Execute Discard with optional parameter all=true - Must return successful discard response")]
        //Create process artifact, save, don't publish, discard - must return successfully discarded.
        public void DiscardArtifacts_DiscardSavedArtifactsWithDiscardAll_VerifyArtifactDiscarded(int numberOfArtifacts, BaseArtifactType artifactType)
        {
            // Setup:
            // Create artifact(s) with save for discard test
            var savedArtifacts = Helper.CreateAndSaveMultipleArtifacts(_project, _user, artifactType, numberOfArtifacts);
            INovaArtifactsAndProjectsResponse discardArtifactResponse = null;

            // Execute:
            Assert.DoesNotThrow(() => discardArtifactResponse = Helper.ArtifactStore.DiscardArtifacts(savedArtifacts, _user, all: true),
                "DiscardArtifacts() failed when discarding saved artifact(s)!");

            // Validation: Verify that discarded artifact information from discardedArtifactResponse match with that from savedArtifacts

            Assert.That(discardArtifactResponse.Artifacts.Count.Equals(savedArtifacts.Count),
                "Number of discarded artifact is {0} but discarded item count from the response of the discard is {1}", savedArtifacts.Count, discardArtifactResponse.Artifacts.Count);

            DiscardVerification(discardArtifactResponse, savedArtifacts);
        }

        [Explicit(IgnoreReasons.UnderDevelopment)]  // POST (Save) functionality failing.
        [TestCase(2, BaseArtifactType.Actor)]
        [TestRail(166135)]
        [Description("Set draft artifacts by saving, publishing, and saving. Execute Discard - Must return successful discard response")]
        public void DiscardArtifacts_DiscardChangesFromPreviouslyPublishedArtifacts_VerifyArtifactDiscarded(int numberOfArtifacts, BaseArtifactType artifactType)
        {
            // Setup:
            // Create artifact(s) with save and publish for discard test
            var changedPublishedArtifacts = Helper.CreateAndPublishMultipleArtifacts(_project, _user, artifactType, numberOfArtifacts);

            foreach (var publishedArtifact in changedPublishedArtifacts.ConvertAll(x => (IArtifact)x))
            {
                publishedArtifact.Save();
            }

            INovaArtifactsAndProjectsResponse discardArtifactResponse = null;

            // Execute:
            Assert.DoesNotThrow(() => discardArtifactResponse = Helper.ArtifactStore.DiscardArtifacts(changedPublishedArtifacts, _user),
                "DiscardArtifacts() failed when discarding saved artifact(s)!");

            // Validation: Verify that discarded artifact information from discardedArtifactResponse match with that from savedArtifacts
            Assert.That(discardArtifactResponse.Artifacts.Count.Equals(changedPublishedArtifacts.Count),
                "Number of discarded artifact is {0} but discarded item count from the response of the discard is {1}",
                changedPublishedArtifacts.Count, discardArtifactResponse.Artifacts.Count);

            DiscardVerification(discardArtifactResponse, changedPublishedArtifacts);
        }

        [Explicit(IgnoreReasons.UnderDevelopment)]  // POST (Save) functionality failing.
        [TestCase(2, BaseArtifactType.Actor)]
        [TestRail(166136)]
        [Description("Set draft artifacts by saving, publishing, and saving. Execute Discard with optional parameter all=true - Must return successful discard response")]
        public void DiscardArtifacts_DiscardChangesFromPreviouslyPublishedArtifactsWithDiscardAll_VerifyArtifactDiscarded(int numberOfArtifacts, BaseArtifactType artifactType)
        {
            // Setup:
            // Create artifact(s) with save and publish for discard test
            var changedPublishedArtifacts = Helper.CreateAndPublishMultipleArtifacts(_project, _user, artifactType, numberOfArtifacts);

            foreach (var publishedArtifact in changedPublishedArtifacts.ConvertAll(x => (IArtifact)x))
            {
                publishedArtifact.Save();
            }

            INovaArtifactsAndProjectsResponse discardArtifactResponse = null;

            // Execute:
            Assert.DoesNotThrow(() => discardArtifactResponse = Helper.ArtifactStore.DiscardArtifacts(changedPublishedArtifacts, _user, all: true),
                "DiscardArtifacts() failed when discarding saved artifact(s)!");

            // Validation: Verify that discarded artifact information from discardedArtifactResponse match with that from savedArtifacts
            Assert.That(discardArtifactResponse.Artifacts.Count.Equals(changedPublishedArtifacts.Count),
                "Number of discarded artifact is {0} but discarded item count from the response of the discard is {1}", changedPublishedArtifacts.Count, discardArtifactResponse.Artifacts.Count);

            DiscardVerification(discardArtifactResponse, changedPublishedArtifacts);
        }

        [TestCase(2, BaseArtifactType.Actor)]
        [TestRail(166137)]
        [Description("Set published artifacts by saving and publishing. Execute Discard with optional parameter all=true - Must return empty response which represents nothing to be discarded")]
        public void DiscardArtifacts_DiscardPublishedArtifactsnWithDiscardAll_VerifyNothingToDiscard(int numberOfArtifacts, BaseArtifactType artifactType)
        {
            // Setup:
            // Create artifact(s) with save and publish for discard test
            var publishedArtifacts = Helper.CreateAndPublishMultipleArtifacts(_project, _user, artifactType, numberOfArtifacts);

            INovaArtifactsAndProjectsResponse discardArtifactResponse = null;

            // Execute:
            Assert.DoesNotThrow(() => discardArtifactResponse = Helper.ArtifactStore.DiscardArtifacts(publishedArtifacts, _user, all: true),
                "DiscardArtifacts() failed when discarding saved artifact(s)!");

            // Validation: Verify that response from discard call is empty since published artifact is not discardable
            Assert.That(discardArtifactResponse.Artifacts.Count.Equals(0),
                "Number of discarded artifact is {0} but discarded item count from the response of the discard is {1}", 0, discardArtifactResponse.Artifacts.Count);
        }

        [TestCase(2, BaseArtifactType.Actor)]
        [TestRail(166138)]
        [Description("Set published artifacts by saving and publishing. Execute Discard with optional parameter all=true - Must return list of saved artifacts that got dicarded")]
        public void DiscardArtifacts_DiscardMixedListOfPublishedAndSavedArtifactsWithDiscardAll_DiscardSavedArtifacts(int numberOfArtifacts, BaseArtifactType artifactType)
        {
            // Setup:
            // Create artifact(s) with save and publish for discard test
            var publishedArtifacts = Helper.CreateAndPublishMultipleArtifacts(_project, _user, artifactType, numberOfArtifacts);

            // Create artifact(s) with save for discard test
            List<IArtifactBase> savedArtifacts = Helper.CreateAndSaveMultipleArtifacts(_project, _user, artifactType, numberOfArtifacts);

            List<IArtifactBase> mixedArtifacts = new List<IArtifactBase>();
            mixedArtifacts.AddRange(publishedArtifacts);
            mixedArtifacts.AddRange(savedArtifacts);

            INovaArtifactsAndProjectsResponse discardArtifactResponse = null;

            // Execute:
            Assert.DoesNotThrow(() => discardArtifactResponse = Helper.ArtifactStore.DiscardArtifacts(mixedArtifacts, _user, all: true),
                "DiscardArtifacts() failed when discarding saved artifact(s)!");

            // Validation: Makesure that returned body contains artifact details from savedArtifacts, ones taht are valid for successful discard. 
            Assert.That(discardArtifactResponse.Artifacts.Count.Equals(numberOfArtifacts),
                "Number of discarded artifact is {0} but discarded item count from the response of the discard is {1}",
                numberOfArtifacts, discardArtifactResponse.Artifacts.Count);
        }

        // TODO Discard of removed artifact

        #endregion 200 OK Tests

        #region 400 Bad Request tests

        [TestCase(2, BaseArtifactType.Actor)]
        [TestRail(166139)]
        [Description("Set published artifacts by saving and publishing. Execute Discard - Must return nothing to discard")]
        public void DiscardArtifacts_DiscardPublishedArtifactsWithNoChildren_VerifyNothingToDiscard(int numberOfArtifacts, BaseArtifactType artifactType)
        {
            // Setup:
            // Create artifact(s) with save and publish for discard test
            var publishedArtifacts = Helper.CreateAndPublishMultipleArtifacts(_project, _user, artifactType, numberOfArtifacts);

            INovaArtifactsAndProjectsResponse discardArtifactResponse = null;

            // Execute:
            var ex = Assert.Throws<Http400BadRequestException>(() => discardArtifactResponse = Helper.ArtifactStore.DiscardArtifacts(publishedArtifacts, _user),
                "We should get a 400 BadRequestException when a user trying to discard published artifact(s) which has nothing to discard!");

            // Validation: Exception should contain expected message.
            string expectedExceptionMessage = "has nothing to discard";
            Assert.That(ex.RestResponse.Content.Contains(expectedExceptionMessage),
                "{0} was not found in returned message of discard published artifact(s) which has nothing to discard.", expectedExceptionMessage);
        }

        [TestCase(2, BaseArtifactType.Actor)]
        [TestRail(166140)]
        [Description("Set mixed list of published and saved artifacts. Execute Discard - Must return nothing to discard")]
        public void DiscardArtifacts_DiscardMixedListOfPublishedAndSavedArtifacts_VerifyNothingToDiscard(int numberOfArtifacts, BaseArtifactType artifactType)
        {
            // Setup:
            // Create artifact(s) with save and publish for discard test
            List<IArtifactBase> publishedArtifacts = Helper.CreateAndPublishMultipleArtifacts(_project, _user, artifactType, numberOfArtifacts);

            // Create artifact(s) with save for discard test
            List<IArtifactBase> savedArtifacts = Helper.CreateAndSaveMultipleArtifacts(_project, _user, artifactType, numberOfArtifacts);

            List<IArtifactBase> mixedArtifacts = new List<IArtifactBase>();
            mixedArtifacts.AddRange(publishedArtifacts);
            mixedArtifacts.AddRange(savedArtifacts);

            INovaArtifactsAndProjectsResponse discardArtifactResponse = null;

            // Execute:
            var ex = Assert.Throws<Http400BadRequestException>(() => discardArtifactResponse = Helper.ArtifactStore.DiscardArtifacts(mixedArtifacts, _user),
                "We should get a 400 BadRequestException when a user trying to discard published artifact(s) which has nothing to discard!");

            // Validation: Exception should contain expected message.
            string expectedExceptionMessage = "has nothing to discard";
            Assert.That(ex.RestResponse.Content.Contains(expectedExceptionMessage),
                "{0} was not found in returned message of discard published artifact(s) which has nothing to discard.", expectedExceptionMessage);
        }

        [TestCase(2, BaseArtifactType.Actor)]
        [TestRail(166141)]
        [Description("Set mixed list of saved and published artifacts. Execute Discard - Must return nothing to discard")]
        public void DiscardArtifacts_DiscardMixedListOfSavedPublishedArtifacts_VerifyNothingToDiscard(int numberOfArtifacts, BaseArtifactType artifactType)
        {
            // Setup:
            // Create artifact(s) with save for discard test
            List<IArtifactBase> savedArtifacts = Helper.CreateAndSaveMultipleArtifacts(_project, _user, artifactType, numberOfArtifacts);

            // Create artifact(s) with save and publish for discard test
            List<IArtifactBase> publishedArtifacts = Helper.CreateAndPublishMultipleArtifacts(_project, _user, artifactType, numberOfArtifacts);

            List<IArtifactBase> mixedArtifacts = new List<IArtifactBase>();
            mixedArtifacts.AddRange(savedArtifacts);
            mixedArtifacts.AddRange(publishedArtifacts);

            INovaArtifactsAndProjectsResponse discardArtifactResponse = null;

            // Execute:
            var ex = Assert.Throws<Http400BadRequestException>(() => discardArtifactResponse = Helper.ArtifactStore.DiscardArtifacts(mixedArtifacts, _user),
                "We should get a 400 BadRequestException when a user trying to discard published artifact(s) which has nothing to discard!");

            // Validation: Exception should contain expected message.
            string expectedExceptionMessage = "has nothing to discard";
            Assert.That(ex.RestResponse.Content.Contains(expectedExceptionMessage),
                "{0} was not found in returned message of discard published artifact(s) which has nothing to discard.", expectedExceptionMessage);
        }

        [TestCase]
        [TestRail(166145)]
        [Description("Send empty list of artifacts and Discard, checks returned result is 400 Bad Request.")]
        public void DiscardArtifacts_EmptyArtifactList_BadRequest()
        {
            // Setup:
            List<IArtifactBase> artifacts = new List<IArtifactBase>();

            INovaArtifactsAndProjectsResponse discardArtifactResponse = null;

            // Execute:
            var ex = Assert.Throws<Http400BadRequestException>(() => discardArtifactResponse = Helper.ArtifactStore.DiscardArtifacts(artifacts, _user),
            "'POST {0}' should return 400 Bad Request if body of the request does not have any artifact ids!", DISCARD_PATH);

            // Verify:
            const string expectedExceptionMessage = "The list of artifact Ids is empty.";
            Assert.That(ex.RestResponse.Content.Contains(expectedExceptionMessage),
                "{0} was not found in returned message of discard artifact(s) which has empty list of Ids.", expectedExceptionMessage);
        }

        #endregion 400 Bad Request tests

        #region 401 Unauthorized tests

        [TestCase(2, BaseArtifactType.Actor)]
        [TestRail(166148)]
        [Description("Create & save a single artifact.  Discard the artifact with wrong token.  Verify publish returns 401 Unauthorized.")]
        public void DiscardArtifacts_InvalidToken_Unauthorized(int numberOfArtifacts, BaseArtifactType artifactType)
        {
            // Setup:
            // Create artifact(s) with save and publish for discard test
            List<IArtifactBase> publishedArtifacts = Helper.CreateAndPublishMultipleArtifacts(_project, _user, artifactType, numberOfArtifacts);

            IUser userWithBadToken = Helper.CreateUserWithInvalidToken(TestHelper.AuthenticationTokenTypes.AccessControlToken);

            // Execute:
            var ex = Assert.Throws<Http401UnauthorizedException>(() => Helper.ArtifactStore.DiscardArtifacts(publishedArtifacts, userWithBadToken),
                "'POST {0}' should return 401 Unauthorized if a token is invalid!", DISCARD_PATH);

            // Verify:
            const string expectedExceptionMessage = "Unauthorized call";
            Assert.That(ex.RestResponse.Content.Contains(expectedExceptionMessage),
                "{0} was not found in returned message of discard published artifact(s) which has invalid token.", expectedExceptionMessage);
        }

        #endregion 401 Unauthorized tests

        #region 404 Not Found tests

        [TestCase(2, BaseArtifactType.Process)]
        [TestRail(166152)]
        [Description("Create, save, publish, delete Process artifact by another user, checks returned result is 404 Not Found.")]
        public void DiscardArtifacts_PublishedArtifactDeleted_NotFound(int numberOfArtifacts, BaseArtifactType artifactType)
        {
            // Setup:
            // Create artifact(s) with save and publish for discard test
            List<IArtifactBase> publishedArtifacts = Helper.CreateAndPublishMultipleArtifacts(_project, _user, artifactType, numberOfArtifacts);

            publishedArtifacts[publishedArtifacts.Count - 1].Delete(_user);
            publishedArtifacts[publishedArtifacts.Count - 1].Publish(_user);

            // Execute:
            var ex = Assert.Throws<Http404NotFoundException>(() => Helper.ArtifactStore.DiscardArtifacts(publishedArtifacts, _user),
                "'POST {0}' should return 404 Not Found if the Artifact ID doesn't exist", DISCARD_PATH);

            // Verify:
            string expectedExceptionMessage = "Artifact with Id " + publishedArtifacts[publishedArtifacts.Count - 1].Id + " is deleted.";
            Assert.That(ex.RestResponse.Content.Contains(expectedExceptionMessage),
                "{0} was not found in returned message of discard published artifact(s) which has removed artifact Id.", expectedExceptionMessage);
        }

        [TestCase(2, BaseArtifactType.Process, int.MaxValue)]
        [TestRail(166153)]
        [Description("Try to discard an artifact with a non-existent Artifact ID.  Verify 404 Not Found is returned.")]
        public void DiscardArtifact_NonExistentArtifactId_NotFound(int numberOfArtifacts, BaseArtifactType artifactType, int nonExistentArtifactId)
        {
            // Setup:
            // Create artifact(s) with save and publish for discard test
            List<IArtifactBase> publishedArtifacts = Helper.CreateAndPublishMultipleArtifacts(_project, _user, artifactType, numberOfArtifacts);

            // Preservs real Id
            int realId = publishedArtifacts[publishedArtifacts.Count - 1].Id;

            // Replace ProjectId with a fake ID that shouldn't exist.
            publishedArtifacts[publishedArtifacts.Count - 1].Id = nonExistentArtifactId;

            try
            {
                // Execute:
                var ex = Assert.Throws<Http404NotFoundException>(() => Helper.ArtifactStore.DiscardArtifacts(publishedArtifacts, _user),
                "'POST {0}' should return 404 Not Found if the Artifact ID doesn't exist", DISCARD_PATH);

                // Verify:
                string expectedExceptionMessage = "Item with Id " + publishedArtifacts[publishedArtifacts.Count - 1].Id + " is not found.";
                Assert.That(ex.RestResponse.Content.Contains(expectedExceptionMessage),
                    "{0} was not found in returned message of discard published artifact(s) which has non existent artifact Id.", expectedExceptionMessage);
            }
            finally
            {
                // Returns real Id to artifact
                publishedArtifacts[publishedArtifacts.Count - 1].Id = realId;
            }
        }

        #endregion 404 Not Found tests

        #region 409 Conflict tests

        [Explicit(IgnoreReasons.UnderDevelopment)]  // POST (Save) functionality failing.
        [TestCase(2, BaseArtifactType.Process)]
        [TestRail(166158)]
        [Description("Create, save, parent artifact with two children, discard parent artifact, checks returned result is 409 Conflict.")]
        public void DiscardArtifact_ParentAndChildArtifacts_OnlyDiscardParent_Conflict(int numberOfArtifacts, BaseArtifactType artifactType)
        {
            // Setup:
            // Create artifact(s) with save and publish for discard test
            var changedPublishedArtifacts = Helper.CreateAndPublishMultipleArtifacts(_project, _user, artifactType, numberOfArtifacts);

            foreach (var publishedArtifact in changedPublishedArtifacts.ConvertAll(x => (IArtifact)x))
            {
                publishedArtifact.Save();
            }

            INovaArtifactsAndProjectsResponse discardArtifactResponse = null;

            // Execute:
            Assert.DoesNotThrow(() => discardArtifactResponse = Helper.ArtifactStore.DiscardArtifacts(changedPublishedArtifacts, _user, all: true),
                "DiscardArtifacts() failed when discarding saved artifact(s)!");

            List<IArtifactBase> oneArtifactList = new List<IArtifactBase>();
            oneArtifactList.Add(changedPublishedArtifacts[2]);
            
            // Execute:
            var ex = Assert.Throws<Http409ConflictException>(() => Helper.ArtifactStore.DiscardArtifacts(oneArtifactList, _user),
                "'POST {0}' should return 409 Conflict if the Artifact has parent artifact which is not published!", DISCARD_PATH);

            // Verify:
            string expectedExceptionMessage = "Specified artifacts have dependent artifacts to publish.";
            Assert.That(ex.RestResponse.Content.Contains(expectedExceptionMessage),
                "{0} was not found in returned message of discard published artifact(s) which has dependend not discarded child artifact Id.", expectedExceptionMessage);
        }

        #endregion 409 Conflict tests

        #region private call

        /// <summary>
        /// Asserts that returned artifact details from the discard call match with artifacts that were discarded.
        /// </summary>
        /// <param name="discardArtifactResponse">The response from Nova discard call.</param>
        /// <param name="artifactsTodiscard">artifacts that are being discarded</param>
        public void DiscardVerification(INovaArtifactsAndProjectsResponse discardArtifactResponse, List<IArtifactBase> artifactsTodiscard)
        {
            ThrowIf.ArgumentNull(discardArtifactResponse, nameof(discardArtifactResponse));
            ThrowIf.ArgumentNull(artifactsTodiscard, nameof(artifactsTodiscard));
            List<int> tempIds = new List<int>();
            discardArtifactResponse.Artifacts.ForEach(a => tempIds.Add(a.Id));

            for (int i = 0; i < artifactsTodiscard.Count; i++)
            {
                Assert.That(tempIds.Contains(artifactsTodiscard[i].Id),
                    "The discarded artifact whose Id is {0} does not exist on the response from the discard call.",artifactsTodiscard[i].Id);
            }

            foreach (var artifact in discardArtifactResponse.Artifacts)
            {
                Assert.Throws<Http404NotFoundException>(() => Helper.ArtifactStore.GetArtifactDetails(_user, artifact.Id),
                    "After discarding artifact ID {0} we were still able to get it!", artifact.Id);
            }
        }

        #endregion private call
    }
}
