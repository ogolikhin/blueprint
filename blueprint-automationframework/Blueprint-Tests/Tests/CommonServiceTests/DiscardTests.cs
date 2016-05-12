using Common;
using CustomAttributes;
using Model;
using Model.ArtifactModel;
using Model.ArtifactModel.Impl;
using Model.Factories;
using NUnit.Framework;
using System.Net;
using System.Collections.Generic;

namespace CommonServiceTests
{
    public class DiscardTests
    {
        private IAdminStore _adminStore;
        private IBlueprintServer _blueprintServer;
        private IUser _user;
        private IProject _project;

        #region Setup and Cleanup

        [TestFixtureSetUp]
        public void ClassSetUp()
        {
            _adminStore = AdminStoreFactory.GetAdminStoreFromTestConfig();
            _blueprintServer = BlueprintServerFactory.GetBlueprintServerFromTestConfig();
            _user = UserFactory.CreateUserAndAddToDatabase();
            _project = ProjectFactory.GetProject(_user);

            // Get a valid Access Control token for the user (for the new Storyteller REST calls).
            ISession session = _adminStore.AddSession(_user);
            _user.SetToken(session.SessionId);

            Assert.IsFalse(
                string.IsNullOrWhiteSpace(_user.Token.AccessControlToken),
                "The user didn't get an Access Control token!");

            // Get a valid OpenApi token for the user (for the OpenApi artifact REST calls).
            _blueprintServer.LoginUsingBasicAuthorization(_user, string.Empty);

            Assert.IsFalse(string.IsNullOrWhiteSpace(_user.Token.OpenApiToken), "The user didn't get an OpenApi token!");
        }

        [TestFixtureTearDown]
        public void ClassTearDown()
        {
            if (_adminStore != null)
            {
                // Delete all the sessions that were created.
                foreach (var session in _adminStore.Sessions.ToArray())
                {
                    _adminStore.DeleteSession(session);
                }
            }

            if (_user != null)
            {
                _user.DeleteUser();
                _user = null;
            }
        }

        #endregion

        [TestCase]
        [TestRail(107373)]
        [Description("Create process artifact, save, publish, discard - must return nothing to discard.")]
        public void DiscardPublishedArtifact_VerifyNothingToDiscard()
        {
            var artifact = ArtifactFactory.CreateArtifact(_project, _user, BaseArtifactType.Process);

            artifact.Save(_user);
            artifact.Publish(_user);

            List<DiscardArtifactResult> discardResultList = null;
            try {
                string expectedMessage = I18NHelper.FormatInvariant("Artifact {0} has nothing to discard", artifact.Id);
                Assert.DoesNotThrow(() =>
                {
                    discardResultList = artifact.NovaDiscard(_user);
                }, "Discard must not throw errors.");
                Assert.AreEqual(expectedMessage, discardResultList[0].Message, "Returned message must be {0}, but {1} was returned",
                        expectedMessage, discardResultList[0].Message);
                Assert.AreEqual((HttpStatusCode)0, discardResultList[0].ResultCode, "Returned code must be {0}, but {1} was returned",
                    (HttpStatusCode)0, discardResultList[0].ResultCode);
            }

            finally
            {
                artifact.Delete(_user);
                artifact.Publish(_user);
            }
        }

        [TestCase]
        [TestRail(107374)]
        [Description("Create process artifact, save, don't publish, discard - must return successfully discarded.")]
        public void DiscardDraftUnpublishedArtifact_VerifyResult()
        {
            var artifact = ArtifactFactory.CreateArtifact(_project, _user, BaseArtifactType.Process);

            artifact.Save(_user);

            List<DiscardArtifactResult> discardResultList = null;
            string expectedMessage = "Successfully discarded";
            Assert.DoesNotThrow(() =>
                {
                    discardResultList = artifact.NovaDiscard(_user);
                }, "Discard must throw no errors.");
            Assert.AreEqual(expectedMessage, discardResultList[0].Message, "Returned message must be {0}, but {1} was returned",
                        expectedMessage, discardResultList[0].Message);
            Assert.AreEqual((HttpStatusCode)0, discardResultList[0].ResultCode, "Returned code must be {0}, but {1} was returned",
                (HttpStatusCode)0, discardResultList[0].ResultCode);
            /// TODO: delete artifact created during the test.
        }

        [TestCase]
        [TestRail(107372)]
        [Explicit(IgnoreReasons.ProductBug)]
        [Description("Create process artifact, save, publish, delete, discard - must return successfully discarded.")]
        public void PublishDeleteDiscardArtifact_VerifyResult()
        {
            var artifact = ArtifactFactory.CreateArtifact(_project, _user, BaseArtifactType.Process);

            artifact.Save(_user);
            artifact.Publish(_user);
            artifact.Delete(_user);

            List<DiscardArtifactResult> discardResultList = null;
            string expectedMessage = "Successfully discarded";
            Assert.DoesNotThrow(() =>
            {
                discardResultList = artifact.NovaDiscard(_user);
            }, "Discard must throw no errors.");
            Assert.AreEqual(expectedMessage, discardResultList[0].Message, "Returned message must be {0}, but {1} was returned",
                expectedMessage, discardResultList[0].Message);
            Assert.AreEqual((HttpStatusCode)0, discardResultList[0].ResultCode, "Returned code must be {0}, but {1} was returned",
                (HttpStatusCode)0, discardResultList[0].ResultCode);
        }

        [TestCase]
        [TestRail(107379)]
        [Explicit(IgnoreReasons.ProductBug)]
        [Description("Create process artifact, save, publish, delete, publish, discard - must return successfully discarded.")]
        public void DiscardDeletedArtifact_VerifyResult()
        {
            var artifact = ArtifactFactory.CreateArtifact(_project, _user, BaseArtifactType.Process);

            artifact.Save(_user);
            artifact.Publish(_user);
            artifact.Delete(_user);
            artifact.Publish(_user);

            List<DiscardArtifactResult> discardResultList = null;
            string expectedMessage = "Successfully discarded";
            Assert.DoesNotThrow(() =>
            {
                discardResultList = artifact.NovaDiscard(_user);
            }, "Discard must throw no errors.");
            Assert.AreEqual(expectedMessage, discardResultList[0].Message, "Returned message must be {0}, but {1} was returned",
                expectedMessage, discardResultList[0].Message);
            Assert.AreEqual((HttpStatusCode)0, discardResultList[0].ResultCode, "Returned code must be {0}, but {1} was returned",
                (HttpStatusCode)0, discardResultList[0].ResultCode);
        }
    }
}
