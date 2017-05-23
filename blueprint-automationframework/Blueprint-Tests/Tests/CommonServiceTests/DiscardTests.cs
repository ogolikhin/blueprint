using Common;
using CustomAttributes;
using Helper;
using Model;
using Model.ArtifactModel;
using Model.ArtifactModel.Enums;
using Model.ArtifactModel.Impl;
using Model.Factories;
using NUnit.Framework;
using System.Collections.Generic;
using TestCommon;
using Utilities;

namespace CommonServiceTests
{
    public class DiscardTests : TestBase
    {
        private IUser _user;
        private IProject _project;

        #region Setup and Cleanup

        [TestFixtureSetUp]
        public void ClassSetUp()
        {
            Helper = new TestHelper();
            _user = Helper.CreateUserAndAuthenticate(TestHelper.AuthenticationTokenTypes.BothAccessControlAndOpenApiTokens);
            _project = ProjectFactory.GetProject(_user);
        }

        [TestFixtureTearDown]
        public void ClassTearDown()
        {
            Helper?.Dispose();
        }

        #endregion

        #region Tests

        [TestCase]
        [TestRail(107373)]
        [Description("Create artifact, save, publish, discard - must return nothing to discard.")]
        public void Discard_PublishedArtifact_VerifyNothingToDiscard()
        {
            // Setup:
            var artifact = Helper.CreateAndPublishNovaArtifact(_user, _project, ItemTypePredefined.Actor);

            List<NovaDiscardArtifactResult> discardResultList = null;

            // Execute:
            Assert.DoesNotThrow(() =>
            {
                discardResultList = Helper.SvcShared.DiscardArtifacts(_user, new List<int>() { artifact.Id });
            }, "Discard must not throw errors.");

            // Verify:
            var expectedResultCode = NovaDiscardArtifactResult.ResultCode.ArtifactHasNothingToDiscard;
            string expectedMessage = I18NHelper.FormatInvariant("Artifact {0} has nothing to discard", artifact.Id);

            ValidateDiscardResult(expectedResultCode, expectedMessage, discardResultList);

            // Make sure artifact still exists.
            INovaArtifactDetails retrievedArtifact = null;

            Assert.DoesNotThrow(() =>
            {
                retrievedArtifact = Helper.ArtifactStore.GetArtifactDetails(_user, artifact.Id);
            }, "GetArtifactDetails() failed after discarding a published artifact!");

            NovaArtifactBase.AssertAreEqual(artifact.Artifact, retrievedArtifact);
        }

        [TestCase]
        [TestRail(107374)]
        [Description("Create artifact, save, don't publish, discard - must return successfully discarded.")]
        public void Discard_DraftUnpublishedArtifact_ArtifactIsDiscarded()
        {
            // Setup:
            var artifact = Helper.CreateNovaArtifact(_user, _project, ItemTypePredefined.Actor);
            artifact.SaveWithNewDescription(_user);

            List<NovaDiscardArtifactResult> discardResultList = null;

            // Execute:
            Assert.DoesNotThrow(() =>
            {
                discardResultList = Helper.SvcShared.DiscardArtifacts(_user, new List<int>() { artifact.Id });
            }, "Discard must throw no errors.");

            // Verify:
            var expectedResultCode = NovaDiscardArtifactResult.ResultCode.Success;
            string expectedMessage = "Successfully discarded";

            ValidateDiscardResult(expectedResultCode, expectedMessage, discardResultList);

            // Make sure the artifact really is discarded.
            Assert.Throws<Http404NotFoundException>(() =>
            {
                Helper.ArtifactStore.GetArtifactDetails(_user, artifact.Id);
            }, "Artifact {0} still exists after it was discarded!", artifact.Id);
        }

        [TestCase]
        [TestRail(107372)]
        [Description("Create process artifact, save, publish, delete, discard - must return successfully discarded.")]
        public void Discard_MarkedForDeleteArtifact_ArtifactIsNotMarkedForDeletion()
        {
            // Setup:
            var artifact = Helper.CreateAndPublishNovaArtifact(_user, _project, ItemTypePredefined.Actor);
            artifact.Delete(_user);

            List<NovaDiscardArtifactResult> discardResultList = null;

            // Execute:
            Assert.DoesNotThrow(() =>
            {
                discardResultList = Helper.SvcShared.DiscardArtifacts(_user, new List<int>() { artifact.Id });
            }, "Discard must throw no errors.");

            // Verify:
            var expectedResultCode = NovaDiscardArtifactResult.ResultCode.Success;
            const string expectedMessage = "Successfully discarded";

            ValidateDiscardResult(expectedResultCode, expectedMessage, discardResultList);

            // Make sure artifact still exists.
            INovaArtifactDetails retrievedArtifact = null;

            Assert.DoesNotThrow(() =>
            {
                retrievedArtifact = Helper.ArtifactStore.GetArtifactDetails(_user, artifact.Id);
            }, "GetArtifactDetails() failed after discarding a published artifact!");

            NovaArtifactBase.AssertAreEqual(artifact.Artifact, retrievedArtifact);
        }

        [TestCase]
        [TestRail(107379)]
        [Description("Create process artifact, save, publish, delete, publish, discard - must return artifact not found message.")]
        public void Discard_DeletedArtifact_ArtifactNotFoundMessage()
        {
            // Setup:
            var artifact = Helper.CreateAndPublishNovaArtifact(_user, _project, ItemTypePredefined.Actor);
            artifact.Delete(_user);
            artifact.Publish(_user);

            List<NovaDiscardArtifactResult> discardResultList = null;

            // Execute:
            Assert.DoesNotThrow(() =>
            {
                discardResultList = Helper.SvcShared.DiscardArtifacts(_user, new List<int>() { artifact.Id });
            }, "Discard must throw no errors.");

            // Verify:
            var expectedResultCode = NovaDiscardArtifactResult.ResultCode.Failure;
            string expectedMessage = I18NHelper.FormatInvariant("The requested artifact ID {0} is not found.", artifact.Id);

            ValidateDiscardResult(expectedResultCode, expectedMessage, discardResultList);

            // Make sure the artifact really is still deleted.
            Assert.Throws<Http404NotFoundException>(() =>
            {
                Helper.ArtifactStore.GetArtifactDetails(_user, artifact.Id);
            }, "Deleted Artifact {0} exists after it was discarded!", artifact.Id);
        }

        #endregion Tests

        #region private functions

        /// <summary>
        /// Validates the result returned from POST svc/shared/artifacts/discard call with expected values.
        /// </summary>
        /// <param name="expectedDiscardCode">the expected discard code.</param>
        /// <param name="expectedDiscardMessage">the expected discard message.</param>
        /// <param name="actualDiscardResultList">the response returned from POST svc/shared/artifacts/discard call.</param>
        private static void ValidateDiscardResult(
            NovaDiscardArtifactResult.ResultCode expectedDiscardCode,
            string expectedDiscardMessage,
            List<NovaDiscardArtifactResult> actualDiscardResultList)
        {

            Assert.AreEqual(expectedDiscardCode, actualDiscardResultList[0].Result, "Returned code must be {0}, but {1} was returned",
                (int)expectedDiscardCode, actualDiscardResultList[0].Result);

            Assert.AreEqual(expectedDiscardMessage, actualDiscardResultList[0].Message, "Returned message must be '{0}', but '{1}' was returned",
                expectedDiscardMessage, actualDiscardResultList[0].Message);
        }

        #endregion private functions

    }
}
