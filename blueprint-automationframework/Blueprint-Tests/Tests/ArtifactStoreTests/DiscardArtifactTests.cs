﻿using CustomAttributes;
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
        [TestRail(0)]
        [Explicit(IgnoreReasons.UnderDevelopment)]
        [Description("Set draft artifacts by just saving. Execute Discard - Must return successful discard response")]
        //Create process artifact, save, don't publish, discard - must return successfully discarded.
        public void DiscardArtifacts_DiscardSavedArtifacts_VerifyArtifactDiscarded(int numberOfArtifacts, BaseArtifactType artifactType)
        {
            // Setup:
            // Create artifact(s) with save for discard test
            var savedArtifacts = Helper.CreateAndSaveMultipleArtifacts(_project, _user, artifactType, numberOfArtifacts);

            INovaPublishResponse discardArtifactResponse = null;

            // Execute:
            Assert.DoesNotThrow(() => discardArtifactResponse = Helper.ArtifactStore.DiscardArtifacts(savedArtifacts, _user), "DiscardArtifacts() failed when discarding saved artifact(s)!");

            // Validation: Verify that discarded artifact information from discardedArtifactResponse match with that from savedArtifacts

            Assert.That(discardArtifactResponse.Artifacts.Count.Equals(savedArtifacts.Count), "Number of discarded artifact is {0} but discarded item count from the response of the discard is {1}", savedArtifacts.Count, discardArtifactResponse.Artifacts.Count);

            DiscardVerification(discardArtifactResponse, savedArtifacts);

            // Preperation for cleanup:
            foreach (var artifact in savedArtifacts)
            {
                artifact.IsSaved = false;
            }
        }

        [TestCase(2, BaseArtifactType.Actor)]
        [TestRail(0)]
        [Explicit(IgnoreReasons.UnderDevelopment)]
        [Description("Set draft artifacts by just saving. Execute Discard with optional parameter all=true - Must return successful discard response")]
        //Create process artifact, save, don't publish, discard - must return successfully discarded.
        public void DiscardArtifacts_DiscardSavedArtifactsWithDiscardAll_VerifyArtifactDiscarded(int numberOfArtifacts, BaseArtifactType artifactType)
        {
            // Setup:
            // Create artifact(s) with save for discard test
            var savedArtifacts = Helper.CreateAndSaveMultipleArtifacts(_project, _user, artifactType, numberOfArtifacts);

            INovaPublishResponse discardArtifactResponse = null;

            // Execute:
            Assert.DoesNotThrow(() => discardArtifactResponse = Helper.ArtifactStore.DiscardArtifacts(savedArtifacts, _user, all: true), "DiscardArtifacts() failed when discarding saved artifact(s)!");

            // Validation: Verify that discarded artifact information from discardedArtifactResponse match with that from savedArtifacts

            Assert.That(discardArtifactResponse.Artifacts.Count.Equals(savedArtifacts.Count), "Number of discarded artifact is {0} but discarded item count from the response of the discard is {1}", savedArtifacts.Count, discardArtifactResponse.Artifacts.Count);

            DiscardVerification(discardArtifactResponse, savedArtifacts);

            // Preperation for cleanup:
            foreach (var artifact in savedArtifacts)
            {
                artifact.IsSaved = false;
            }
        }

        [TestCase(2, BaseArtifactType.Actor)]
        [TestRail(0)]
        [Explicit(IgnoreReasons.UnderDevelopment)]
        [Description("Set draft artifacts by saving, publishing, and saving. Execute Discard - Must return successful discard response")]
        public void DiscardArtifacts_DiscardChangesFromPreviouslyPublishedArtifacts_VerifyArtifactDiscarded(int numberOfArtifacts, BaseArtifactType artifactType)
        {
            // Setup:
            // Create artifact(s) with save and publish for discard test
            var publishedArtifacts = Helper.CreateAndPublishMultipleArtifacts(_project, _user, artifactType, numberOfArtifacts).ConvertAll(x => (IArtifact)x);

            foreach (var publishedArtifact in publishedArtifacts)
            {
                publishedArtifact.Save();
            }

            var changedPublishedArtifacts = publishedArtifacts.ConvertAll(x => (IArtifactBase)x);

            INovaPublishResponse discardArtifactResponse = null;

            // Execute:
            Assert.DoesNotThrow(() => discardArtifactResponse = Helper.ArtifactStore.DiscardArtifacts(changedPublishedArtifacts, _user), "DiscardArtifacts() failed when discarding saved artifact(s)!");

            // Validation: Verify that discarded artifact information from discardedArtifactResponse match with that from savedArtifacts
            Assert.That(discardArtifactResponse.Artifacts.Count.Equals(changedPublishedArtifacts.Count), "Number of discarded artifact is {0} but discarded item count from the response of the discard is {1}", changedPublishedArtifacts.Count, discardArtifactResponse.Artifacts.Count);

            DiscardVerification(discardArtifactResponse, changedPublishedArtifacts);
        }

        [TestCase(2, BaseArtifactType.Actor)]
        [TestRail(0)]
        [Explicit(IgnoreReasons.UnderDevelopment)]
        [Description("Set draft artifacts by saving, publishing, and saving. Execute Discard with optional parameter all=true - Must return successful discard response")]
        public void DiscardArtifacts_DiscardChangesFromPreviouslyPublishedArtifactsWithDiscardAll_VerifyArtifactDiscarded(int numberOfArtifacts, BaseArtifactType artifactType)
        {
            // Setup:
            // Create artifact(s) with save and publish for discard test
            var publishedArtifacts = Helper.CreateAndPublishMultipleArtifacts(_project, _user, artifactType, numberOfArtifacts).ConvertAll(x => (IArtifact)x);

            foreach (var publishedArtifact in publishedArtifacts)
            {
                publishedArtifact.Save();
            }

            var changedPublishedArtifacts = publishedArtifacts.ConvertAll(x => (IArtifactBase)x);

            INovaPublishResponse discardArtifactResponse = null;

            // Execute:
            Assert.DoesNotThrow(() => discardArtifactResponse = Helper.ArtifactStore.DiscardArtifacts(changedPublishedArtifacts, _user, all: true), "DiscardArtifacts() failed when discarding saved artifact(s)!");

            // Validation: Verify that discarded artifact information from discardedArtifactResponse match with that from savedArtifacts
            Assert.That(discardArtifactResponse.Artifacts.Count.Equals(changedPublishedArtifacts.Count), "Number of discarded artifact is {0} but discarded item count from the response of the discard is {1}", changedPublishedArtifacts.Count, discardArtifactResponse.Artifacts.Count);

            DiscardVerification(discardArtifactResponse, changedPublishedArtifacts);
        }

        [TestCase(2, BaseArtifactType.Actor)]
        [TestRail(0)]
        [Explicit(IgnoreReasons.UnderDevelopment)]
        [Description("Set published artifacts by saving and publishing. Execute Discard with optional parameter all=true - Must return the 200 nothing to discard")]
        public void DiscardArtifacts_DiscardPublishedArtifactsWithNoChildrenWithDiscardAll_VerifyNothingToDiscard(int numberOfArtifacts, BaseArtifactType artifactType)
        {
            // Setup:
            // Create artifact(s) with save and publish for discard test
            var publishedArtifacts = Helper.CreateAndPublishMultipleArtifacts(_project, _user, artifactType, numberOfArtifacts);

            INovaPublishResponse discardArtifactResponse = null;

            // Execute:
            Assert.DoesNotThrow(() => discardArtifactResponse = Helper.ArtifactStore.DiscardArtifacts(publishedArtifacts, _user, all: true), "DiscardArtifacts() failed when discarding saved artifact(s)!");

            Assert.That(discardArtifactResponse.Artifacts.Count.Equals(0), "Number of discarded artifact is {0} but discarded item count from the response of the discard is {1}", 0, discardArtifactResponse.Artifacts.Count);
        }

        #endregion 200 OK Tests

        #region 400 Bad Request tests

        [TestCase(2, BaseArtifactType.Actor)]
        [TestRail(0)]
        [Explicit(IgnoreReasons.UnderDevelopment)]
        [Description("Set published artifacts by saving and publishing. Execute Discard - Must return nothing to discard")]
        public void DiscardArtifacts_DiscardPublishedArtifactsWithNoChildren_VerifyNothingToDiscard(int numberOfArtifacts, BaseArtifactType artifactType)
        {
            // Setup:
            // Create artifact(s) with save and publish for discard test
            var publishedArtifacts = Helper.CreateAndPublishMultipleArtifacts(_project, _user, artifactType, numberOfArtifacts);

            INovaPublishResponse discardArtifactResponse = null;

            // Execute:
            var ex = Assert.Throws<Http400BadRequestException>(() => discardArtifactResponse = Helper.ArtifactStore.DiscardArtifacts(publishedArtifacts, _user),
                "We should get a 400 BadRequestException when a user trying to discard published artifact(s) which has nothing to discard!");

            // Validation: Exception should contain expected message.
            string expectedExceptionMessage = "has nothing to discard";
            Assert.That(ex.RestResponse.Content.Contains(expectedExceptionMessage), "{0} was not found in returned message of discard published artifact(s) which has nothing to discard.", expectedExceptionMessage);
        }

        [TestCase(2, BaseArtifactType.Actor)]
        [TestRail(0)]
        [Explicit(IgnoreReasons.UnderDevelopment)]
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

            INovaPublishResponse discardArtifactResponse = null;

            // Execute:
            var ex = Assert.Throws<Http400BadRequestException>(() => discardArtifactResponse = Helper.ArtifactStore.DiscardArtifacts(publishedArtifacts, _user),
                "We should get a 400 BadRequestException when a user trying to discard published artifact(s) which has nothing to discard!");

            // Validation: Exception should contain expected message.
            string expectedExceptionMessage = "has nothing to discard";
            Assert.That(ex.RestResponse.Content.Contains(expectedExceptionMessage), "{0} was not found in returned message of discard published artifact(s) which has nothing to discard.", expectedExceptionMessage);
        }

        [TestCase(2, BaseArtifactType.Actor)]
        [TestRail(0)]
        [Explicit(IgnoreReasons.UnderDevelopment)]
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

            INovaPublishResponse discardArtifactResponse = null;

            // Execute:
            var ex = Assert.Throws<Http400BadRequestException>(() => discardArtifactResponse = Helper.ArtifactStore.DiscardArtifacts(publishedArtifacts, _user),
                "We should get a 400 BadRequestException when a user trying to discard published artifact(s) which has nothing to discard!");

            // Validation: Exception should contain expected message.
            string expectedExceptionMessage = "has nothing to discard";
            Assert.That(ex.RestResponse.Content.Contains(expectedExceptionMessage), "{0} was not found in returned message of discard published artifact(s) which has nothing to discard.", expectedExceptionMessage);
        }

        #endregion 400 Bad Request tests

        #region 401 Unauthorized tests
        #endregion 401 Unauthorized tests

        #region 403 Forbidden tests
        #endregion 403 Forbidden tests

        #region 404 Not Found tests
        #endregion 404 Not Found tests

        #region 409 Conflict tests
        #endregion 409 Conflict tests

        #region private call
        public static void DiscardVerification(INovaPublishResponse discardArtifactResponse, List<IArtifactBase> artifacts)
        {
            ThrowIf.ArgumentNull(discardArtifactResponse, nameof(discardArtifactResponse));
            ThrowIf.ArgumentNull(artifacts, nameof(artifacts));
            List<int> tempIds = new List<int>();
            discardArtifactResponse.Artifacts.ForEach(a => tempIds.Add(a.Id));

            for (int i = 0; i < artifacts.Count; i++)
            {
                Assert.That(tempIds.Contains(artifacts[i].Id), "The discarded artifact whos Id is {0} does not exist on the response from the discard call.", artifacts[i].Id);
            }
        }

        #endregion private call

    }
}
