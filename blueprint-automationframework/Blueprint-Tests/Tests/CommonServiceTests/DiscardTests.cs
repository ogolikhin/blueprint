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

        [TestCase]
        [TestRail(107373)]
        [Description("Create process artifact, save, publish, discard - must return nothing to discard.")]
        public void Discard_PublishedArtifact_VerifyNothingToDiscard()
        {
            // Setup:
            var artifact = Helper.CreateAndPublishNovaArtifact(_user, _project, ItemTypePredefined.Process);

            List<NovaDiscardArtifactResult> discardResultList = null;
            string expectedMessage = I18NHelper.FormatInvariant("Artifact {0} has nothing to discard", artifact.Id);

            // Execute:
            Assert.DoesNotThrow(() =>
            {
                discardResultList = Helper.SvcShared.DiscardArtifacts(_user, new List<int>() { artifact.Id });
            }, "Discard must not throw errors.");

            // Verify:
            var expectedResultCode = NovaDiscardArtifactResult.ResultCode.ArtifactHasNothingToDiscard;

            Assert.AreEqual(expectedResultCode, discardResultList[0].Result, "Returned code must be {0}, but {1} was returned",
                (int)expectedResultCode, discardResultList[0].Result);
            Assert.AreEqual(expectedMessage, discardResultList[0].Message, "Returned message must be {0}, but {1} was returned",
                expectedMessage, discardResultList[0].Message);

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
        [Description("Create process artifact, save, don't publish, discard - must return successfully discarded.")]
        public void Discard_DraftUnpublishedArtifact_ArtifactIsDiscarded()
        {
            // Setup:
            var artifact = Helper.CreateNovaArtifact(_user, _project, ItemTypePredefined.Process);
            artifact.Update(_user, artifact.Artifact);

            List<NovaDiscardArtifactResult> discardResultList = null;
            string expectedMessage = "Successfully discarded";

            // Execute:
            Assert.DoesNotThrow(() =>
            {
                discardResultList = Helper.SvcShared.DiscardArtifacts(_user, new List<int>() { artifact.Id });
            }, "Discard must throw no errors.");

            // Verify:
            var expectedResultCode = NovaDiscardArtifactResult.ResultCode.Success;

            Assert.AreEqual(expectedResultCode, discardResultList[0].Result, "Returned code must be {0}, but {1} was returned",
                (int)expectedResultCode, discardResultList[0].Result);
            Assert.AreEqual(expectedMessage, discardResultList[0].Message, "Returned message must be {0}, but {1} was returned",
                expectedMessage, discardResultList[0].Message);

            // Make sure the artifact really is discarded.
            Assert.Throws<Http404NotFoundException>(() =>
            {
                Helper.OpenApi.GetArtifact(_project, artifact.Id, _user);
            }, "Artifact {0} still exists after it was discarded!", artifact.Id);
        }

        [TestCase]
        [TestRail(107372)]
        [Description("Create process artifact, save, publish, delete, discard - must return successfully discarded.")]
        public void Discard_MarkedForDeleteArtifact_ArtifactIsNotMarkedForDeletion()
        {
            // Setup:
            var artifact = Helper.CreateAndPublishNovaArtifact(_user, _project, ItemTypePredefined.Process);
            artifact.Delete(_user);

            List<NovaDiscardArtifactResult> discardResultList = null;

            // Execute:
            Assert.DoesNotThrow(() =>
            {
                discardResultList = Helper.SvcShared.DiscardArtifacts(_user, new List<int>() { artifact.Id });
            }, "Discard must throw no errors.");

            // Verify:
            const string expectedMessage = "Successfully discarded";
            var expectedResultCode = NovaDiscardArtifactResult.ResultCode.Success;

            Assert.AreEqual(expectedResultCode, discardResultList[0].Result, "Returned code must be {0}, but {1} was returned",
                (int)expectedResultCode, discardResultList[0].Result);
            Assert.AreEqual(expectedMessage, discardResultList[0].Message, "Returned message must be '{0}', but '{1}' was returned",
                expectedMessage, discardResultList[0].Message);

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
            var artifact = Helper.CreateAndPublishNovaArtifact(_user, _project, ItemTypePredefined.Process);
            artifact.Delete(_user);
            artifact.Publish(_user);

            List<NovaDiscardArtifactResult> discardResultList = null;
            string expectedMessage = I18NHelper.FormatInvariant("The requested artifact ID {0} is not found.",
                artifact.Id);

            // Execute:
            Assert.DoesNotThrow(() =>
            {
                discardResultList = Helper.SvcShared.DiscardArtifacts(_user, new List<int>() { artifact.Id });
            }, "Discard must throw no errors.");

            // Verify:
            var expectedResultCode = NovaDiscardArtifactResult.ResultCode.Failure;

            Assert.AreEqual(expectedResultCode, discardResultList[0].Result, "Returned code must be {0}, but {1} was returned",
                (int)expectedResultCode, discardResultList[0].Result);
            Assert.AreEqual(expectedMessage, discardResultList[0].Message, "Returned message must be '{0}', but '{1}' was returned",
                expectedMessage, discardResultList[0].Message);

            // Make sure the artifact really is still deleted.
            Assert.Throws<Http404NotFoundException>(() =>
            {
                Helper.OpenApi.GetArtifact(_project, artifact.Id, _user);
            }, "Deleted Artifact {0} exists after it was discarded!", artifact.Id);
        }
    }
}
