using Common;
using CustomAttributes;
using Model;
using Model.ArtifactModel;
using Model.ArtifactModel.Impl;
using Model.Factories;
using NUnit.Framework;
using System.Collections.Generic;
using Helper;
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
            var artifact = Helper.CreateAndPublishArtifact(_project, _user, BaseArtifactType.Process);

            List<NovaDiscardArtifactResult> discardResultList = null;
            string expectedMessage = I18NHelper.FormatInvariant("Artifact {0} has nothing to discard", artifact.Id);

            // Execute:
            Assert.DoesNotThrow(() =>
            {
                discardResultList = artifact.NovaDiscard(_user);
            }, "Discard must not throw errors.");

            // Verify:
            var expectedResultCode = NovaDiscardArtifactResult.ResultCode.ArtifactHasNothingToDiscard;

            Assert.AreEqual(expectedResultCode, discardResultList[0].Result, "Returned code must be {0}, but {1} was returned",
                (int)expectedResultCode, discardResultList[0].Result);
            Assert.AreEqual(expectedMessage, discardResultList[0].Message, "Returned message must be {0}, but {1} was returned",
                expectedMessage, discardResultList[0].Message);

            // Make sure artifact still exists.
            IOpenApiArtifact retrievedArtifact = null;

            Assert.DoesNotThrow(() =>
            {
                retrievedArtifact = OpenApiArtifact.GetArtifact(artifact.Address, _project, artifact.Id, _user);
            }, "GetArtifact() failed after discarding a published artifact!");

            TestHelper.AssertArtifactsAreEqual(artifact, retrievedArtifact);
        }

        [TestCase]
        [TestRail(107374)]
        [Description("Create process artifact, save, don't publish, discard - must return successfully discarded.")]
        public void Discard_DraftUnpublishedArtifact_ArtifactIsDiscarded()
        {
            // Setup:
            var artifact = Helper.CreateArtifact(_project, _user, BaseArtifactType.Process);
            artifact.Save(_user);

            List<NovaDiscardArtifactResult> discardResultList = null;
            string expectedMessage = "Successfully discarded";

            // Execute:
            Assert.DoesNotThrow(() =>
            {
                discardResultList = artifact.NovaDiscard(_user);
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
                OpenApiArtifact.GetArtifact(artifact.Address, _project, artifact.Id, _user);
            }, "Artifact {0} still exists after it was discarded!", artifact.Id);
        }

        [TestCase]
        [TestRail(107372)]
        [Description("Create process artifact, save, publish, delete, discard - must return successfully discarded.")]
        public void Discard_MarkedForDeleteArtifact_ArtifactIsNotMarkedForDeletion()
        {
            // Setup:
            var artifact = Helper.CreateAndPublishArtifact(_project, _user, BaseArtifactType.Process);
            artifact.Delete(_user);

            List<NovaDiscardArtifactResult> discardResultList = null;

            // Execute:
            Assert.DoesNotThrow(() =>
            {
                discardResultList = artifact.NovaDiscard(_user);
            }, "Discard must throw no errors.");

            // Verify:
            const string expectedMessage = "Successfully discarded";
            var expectedResultCode = NovaDiscardArtifactResult.ResultCode.Success;

            Assert.AreEqual(expectedResultCode, discardResultList[0].Result, "Returned code must be {0}, but {1} was returned",
                (int)expectedResultCode, discardResultList[0].Result);
            Assert.AreEqual(expectedMessage, discardResultList[0].Message, "Returned message must be '{0}', but '{1}' was returned",
                expectedMessage, discardResultList[0].Message);

            // Make sure artifact still exists.
            IOpenApiArtifact retrievedArtifact = null;

            Assert.DoesNotThrow(() =>
            {
                retrievedArtifact = OpenApiArtifact.GetArtifact(artifact.Address, _project, artifact.Id, _user);
            }, "GetArtifact() failed after discarding a published artifact!");

            TestHelper.AssertArtifactsAreEqual(artifact, retrievedArtifact);
        }

        [TestCase]
        [TestRail(107379)]
        [Description("Create process artifact, save, publish, delete, publish, discard - must return artifact not found message.")]
        public void Discard_DeletedArtifact_ArtifactNotFoundMessage()
        {
            // Setup:
            var artifact = Helper.CreateAndPublishArtifact(_project, _user, BaseArtifactType.Process);
            artifact.Delete(_user);
            artifact.Publish(_user);

            List<NovaDiscardArtifactResult> discardResultList = null;
            string expectedMessage = I18NHelper.FormatInvariant("The requested artifact ID {0} is not found.", 
                artifact.Id);
            
            // Execute:
            Assert.DoesNotThrow(() =>
            {
                discardResultList = artifact.NovaDiscard(_user);
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
                OpenApiArtifact.GetArtifact(artifact.Address, _project, artifact.Id, _user);
            }, "Deleted Artifact {0} exists after it was discarded!", artifact.Id);
        }

        [TestCase(2, BaseArtifactType.Actor)]
        [TestRail(0)]
        [Explicit(IgnoreReasons.UnderDevelopment)]
        [Description("Set a published process by saving and publishing. Perform Nova Discard - Must return nothing to discard")]
        public void NovaDiscardArtifact_PublishedArtifactWithNoChildren_VerifyNothingToDiscard(int numberOfArtifacts, BaseArtifactType artifactType)
        {
            // Setup:
            //Create artifact(s) with save and publish for discard call test
            var publishedArtifacts = Helper.CreateAndPublishMultipleArtifacts(_project, _user, artifactType, numberOfArtifacts);

            INovaPublishResponse discardArtifactResponse = null;

            // Execute & Verify:
            var ex = Assert.Throws<Http400BadRequestException>(() => discardArtifactResponse = Helper.ArtifactStore.DiscardArtifacts(publishedArtifacts, _user),
                "We should get a 400 BadRequestException when a user trying to discard published artifact(s) which has nothing to discard!");

            // Verify: Exception should contain expected message.
            string expectedExceptionMessage = "has nothing to discard";
            Assert.That(ex.RestResponse.Content.Contains(expectedExceptionMessage), "{0} was not found in returned message of discard published artifact(s) which has nothing to discard.", expectedExceptionMessage);
        }

        [TestCase(2, BaseArtifactType.Actor)]
        [TestRail(0)]
        [Explicit(IgnoreReasons.UnderDevelopment)]
        [Description("Set a draft process by saving. Discard - must return successfully discarded.")]
        //Create process artifact, save, don't publish, discard - must return successfully discarded.
        public void NovaDiscard_SavedArtifact_VerifyArtifactDiscarded(int numberOfArtifacts, BaseArtifactType artifactType)
        {
            // Setup:
            //Create artifact(s) with save for discard call test
            var savedArtifacts = Helper.CreateAndSaveMultipleArtifacts(_project, _user, artifactType, numberOfArtifacts);

            INovaPublishResponse discardArtifactResponse = null;

            // Execute:
            Assert.DoesNotThrow(() => discardArtifactResponse = Helper.ArtifactStore.DiscardArtifacts(savedArtifacts, _user), "DiscardArtifacts() failed when discarding saved artifact(s)!");
            // Validation:
            

        }

        /*
        [TestCase]
        [TestRail(0)]
        [Ignore(IgnoreReasons.UnderDevelopment)]
        [Description("Set a published process by saving and publishing. Discard - must return nothing to discard")]
        public void NovaDiscard_PublishedArtifact_VerifyNothingToDiscard()
        {
        
            // Setup:
            var artifact = Helper.CreateAndPublishArtifact(_project, _user, BaseArtifactType.Process);
            IArtifact artifact = Helper.CreateAndPublishArtifact(_project, _user, artifactType);

            List<NovaDiscardArtifactResult> discardResultList = null;
            string expectedMessage = I18NHelper.FormatInvariant("Artifact {0} has nothing to discard", artifact.Id);

            // Execute:
            // Validation:
        
        }


        */
    }
}
