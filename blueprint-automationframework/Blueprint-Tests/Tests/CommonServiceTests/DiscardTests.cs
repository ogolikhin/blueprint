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
        public void DiscardPublishedArtifact_VerifyNothingToDiscard()
        {
            var artifact = Helper.CreateArtifact(_project, _user, BaseArtifactType.Process);

            artifact.Save(_user);
            artifact.Publish(_user);

            List<NovaDiscardArtifactResult> discardResultList = null;

            string expectedMessage = I18NHelper.FormatInvariant("Artifact {0} has nothing to discard", artifact.Id);

            Assert.DoesNotThrow(() =>
            {
                discardResultList = artifact.NovaDiscard(_user);
            }, "Discard must not throw errors.");

            Assert.AreEqual(expectedMessage, discardResultList[0].Message, "Returned message must be {0}, but {1} was returned",
                expectedMessage, discardResultList[0].Message);
            Assert.AreEqual(NovaDiscardArtifactResult.ResultCode.Failure, discardResultList[0].Result, "Returned code must be {0}, but {1} was returned",
                NovaDiscardArtifactResult.ResultCode.Failure, discardResultList[0].Result);
        }

        [TestCase]
        [TestRail(107374)]
        [Description("Create process artifact, save, don't publish, discard - must return successfully discarded.")]
        public void DiscardDraftUnpublishedArtifact_VerifyResult()
        {
            var artifact = Helper.CreateArtifact(_project, _user, BaseArtifactType.Process);

            artifact.Save(_user);

            List<NovaDiscardArtifactResult> discardResultList = null;
            string expectedMessage = "Successfully discarded";

            Assert.DoesNotThrow(() =>
            {
                discardResultList = artifact.NovaDiscard(_user);
            }, "Discard must throw no errors.");

            Assert.AreEqual(expectedMessage, discardResultList[0].Message, "Returned message must be {0}, but {1} was returned",
                expectedMessage, discardResultList[0].Message);
            Assert.AreEqual(NovaDiscardArtifactResult.ResultCode.Success, discardResultList[0].Result, "Returned code must be {0}, but {1} was returned",
                NovaDiscardArtifactResult.ResultCode.Success, discardResultList[0].Result);
        }

        [TestCase]
        [TestRail(107372)]
        [Explicit(IgnoreReasons.ProductBug)]
        [Description("Create process artifact, save, publish, delete, discard - must return successfully discarded.")]
        public void PublishDeleteDiscardArtifact_VerifyResult()
        {
            var artifact = Helper.CreateArtifact(_project, _user, BaseArtifactType.Process);

            artifact.Save(_user);
            artifact.Publish(_user);
            artifact.Delete(_user);

            List<NovaDiscardArtifactResult> discardResultList = null;
            string expectedMessage = "Successfully discarded";

            Assert.DoesNotThrow(() =>
            {
                discardResultList = artifact.NovaDiscard(_user);
            }, "Discard must throw no errors.");

            Assert.AreEqual(expectedMessage, discardResultList[0].Message, "Returned message must be {0}, but {1} was returned",
                expectedMessage, discardResultList[0].Message);
            Assert.AreEqual(NovaDiscardArtifactResult.ResultCode.Success, discardResultList[0].Result, "Returned code must be {0}, but {1} was returned",
                NovaDiscardArtifactResult.ResultCode.Success, discardResultList[0].Result);
        }

        [TestCase]
        [TestRail(107379)]
        [Description("Create process artifact, save, publish, delete, publish, discard - must return already deleted message.")]
        public void DiscardDeletedArtifact_VerifyAlreadyDiscardedMessage()
        {
            var artifact = Helper.CreateArtifact(_project, _user, BaseArtifactType.Process);

            artifact.Save(_user);
            artifact.Publish(_user);
            artifact.Delete(_user);
            artifact.Publish(_user);

            List<NovaDiscardArtifactResult> discardResultList = null;
            string expectedMessage = I18NHelper.FormatInvariant("DItem with Id: {0} was deleted by some other user. Please refresh.", 
                artifact.Id);
            
            Assert.DoesNotThrow(() =>
            {
                discardResultList = artifact.NovaDiscard(_user);
            }, "Discard must throw no errors.");

            Assert.AreEqual(expectedMessage, discardResultList[0].Message, "Returned message must be {0}, but {1} was returned",
                expectedMessage, discardResultList[0].Message);
            Assert.AreEqual(NovaDiscardArtifactResult.ResultCode.Failure, discardResultList[0].Result, "Returned code must be {0}, but {1} was returned",
                NovaDiscardArtifactResult.ResultCode.Failure, discardResultList[0].Result);
        }
    }
}
