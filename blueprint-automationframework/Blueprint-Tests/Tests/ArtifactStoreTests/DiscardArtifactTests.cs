using CustomAttributes;
using Helper;
using Model;
using Model.ArtifactModel;
using Model.Factories;
using NUnit.Framework;
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

        [TestCase(2, BaseArtifactType.Actor)]
        [TestRail(0)]
        [Explicit(IgnoreReasons.UnderDevelopment)]
        [Description("Set draft artifacts by just saving. Execute Discard - must return successful discard response")]
        //Create process artifact, save, don't publish, discard - must return successfully discarded.
        public void DiscardArtifacts_DiscardSavedArtifacts_VerifyArtifactDiscarded(int numberOfArtifacts, BaseArtifactType artifactType)
        {
            // Setup:
            // Create artifact(s) with save for discard test
            var savedArtifacts = Helper.CreateAndSaveMultipleArtifacts(_project, _user, artifactType, numberOfArtifacts);

            INovaPublishResponse discardArtifactResponse = null;

            // Execute:
            Assert.DoesNotThrow(() => discardArtifactResponse = Helper.ArtifactStore.DiscardArtifacts(savedArtifacts, _user), "DiscardArtifacts() failed when discarding saved artifact(s)!");
            
            // Validation: TODO
            
            // Preperation for cleanup:
            foreach(var artifact in savedArtifacts)
            {
                artifact.IsSaved = false;
            }
        }

        [TestCase(2, BaseArtifactType.Actor)]
        [TestRail(0)]
        [Explicit(IgnoreReasons.UnderDevelopment)]
        [Description("Set draft artifacts by saving, publishing, and saving. Execute Discard - must return successful discard response")]
        public void DiscardArtifacts_DiscardChangesFromPreviouslyPublishedArtifacts_VerifyArtifactDiscarded(int numberOfArtifacts, BaseArtifactType artifactType)
        {
            // Setup:
            // Create artifact(s) with save and publish for discard test
            var publishedArtifacts = Helper.CreateAndPublishMultipleArtifacts(_project, _user, artifactType, numberOfArtifacts).ConvertAll(x => (IArtifact)x);

            foreach(var publishedArtifact in publishedArtifacts)
            {
                publishedArtifact.Save();
            }

            var changedPublishedArtifacts = publishedArtifacts.ConvertAll(x => (IArtifactBase)x);

            INovaPublishResponse discardArtifactResponse = null;
            
            // Execute:
            Assert.DoesNotThrow(() => discardArtifactResponse = Helper.ArtifactStore.DiscardArtifacts(changedPublishedArtifacts, _user), "DiscardArtifacts() failed when discarding saved artifact(s)!");

            // Validation:TODO
        }

        [TestCase(2, BaseArtifactType.Actor)]
        [TestRail(0)]
        [Explicit(IgnoreReasons.UnderDevelopment)]
        [Description("Set published artifacts by saving and publishing. Execute Nova Discard - Must return nothing to discard")]
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
    }
}
