using Common;
using CustomAttributes;
using Model;
using Model.ArtifactModel;
using Model.ArtifactModel.Impl;
using Model.Factories;
using NUnit.Framework;
using System.Net;
using System.Collections.Generic;
using Model.StorytellerModel;
using Model.StorytellerModel.Impl;
using System.Linq;

namespace CommonServiceTests
{
    public class DiscardTests
    {
        private IAdminStore _adminStore;
        private IBlueprintServer _blueprintServer;
        private IStoryteller _storyteller;
        private IUser _user;
        //[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Performance", "CA1823:AvoidUnusedPrivateFields")]
        private IProject _project;

        #region Setup and Cleanup

        [TestFixtureSetUp]
        public void ClassSetUp()
        {
            _adminStore = AdminStoreFactory.GetAdminStoreFromTestConfig();
            _blueprintServer = BlueprintServerFactory.GetBlueprintServerFromTestConfig();
            _user = UserFactory.CreateUserAndAddToDatabase();
            _project = ProjectFactory.GetProject(_user);
            _storyteller = StorytellerFactory.GetStorytellerFromTestConfig();

            // Get a valid Access Control token for the user (for the new Storyteller REST calls).
            ISession session = _adminStore.AddSession(_user.Username, _user.Password);
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
            //if (_artifactVersion. != null)
            //{
            //    // Delete or Discard all the artifacts that were added.
            //    var savedArtifactsList = new List<IOpenApiArtifact>();
            //    foreach (var artifact in _storyteller.Artifacts.ToArray())
            //    {
            //        if (artifact.IsPublished)
            //        {
            //            _storyteller.DeleteProcessArtifact(artifact, deleteChildren: true);
            //        }
            //        else
            //        {
            //            savedArtifactsList.Add(artifact);
            //        }
            //    }
            //    if (savedArtifactsList.Any())
            //    {
            //        Storyteller.DiscardProcessArtifacts(savedArtifactsList, _blueprintServer.Address, _user);
            //    }
            //}
            if (_storyteller.Artifacts != null)
            {
                // Delete or Discard all the artifacts that were added.
                var savedArtifactsList = new List<IArtifactBase>();
                foreach (var artifact in _storyteller.Artifacts.ToArray())
                {
                    if (artifact.IsPublished)
                    {
                        _storyteller.DeleteProcessArtifact(artifact, deleteChildren: true);
                    }
                    else
                    {
                        savedArtifactsList.Add(artifact);
                    }
                }
                if (savedArtifactsList.Any())
                {
                    Storyteller.DiscardProcessArtifacts(savedArtifactsList, _blueprintServer.Address, _user);
                }
            }

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
        public void DiscardArtifact_VerifyNothingToDiscard()
        {
            var artifact = ArtifactFactory.CreateArtifact(_project, _user, BaseArtifactType.Process);

            artifact.Save(_user);
            artifact.Publish(_user);

            List<DiscardArtifactResult> discardResultList = null;
            Assert.DoesNotThrow(() =>
            {
                string expectedMessage = I18NHelper.FormatInvariant("Artifact {0} has nothing to discard", artifact.Id);
                discardResultList = artifact.NovaDiscard(_user);
                Assert.AreEqual(discardResultList[0].Message, expectedMessage, "error message");
                Assert.AreEqual(discardResultList[0].ResultCode, (HttpStatusCode)0, "error message");
            }, "");
        }

        [TestCase]
        [TestRail(107374)]
        [Description("Create process artifact, save, don't publish, discard - must return successfully discarded.")]
        public void DiscardDraftUnpublishedArtifact_VerifyResult()
        {
            var artifact = ArtifactFactory.CreateArtifact(_project, _user, BaseArtifactType.Process);

            artifact.Save(_user);

            List<DiscardArtifactResult> discardResultList = null;
            Assert.DoesNotThrow(() =>
            {
                discardResultList = artifact.NovaDiscard(_user);
                Assert.AreEqual(discardResultList[0].Message, "Successfully discarded", "error message");
                Assert.AreEqual(discardResultList[0].ResultCode, (HttpStatusCode)0, "error message");
            }, "");
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
            Assert.DoesNotThrow(() =>
            {
                discardResultList = artifact.NovaDiscard(_user);
                Assert.AreEqual(discardResultList[0].Message, "?", "error message");
                Assert.AreEqual(discardResultList[0].ResultCode, 0, "error message");
            }, "");
        }
    }
}
