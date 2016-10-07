using CustomAttributes;
using Helper;
using Model;
using Model.ArtifactModel;
using Model.ArtifactModel.Impl;
using Model.Factories;
using NUnit.Framework;
using TestCommon;

namespace CommonServiceTests
{
    [TestFixture]
    [Category(Categories.ArtifactStore)]
    [Category(Categories.ConcurrentTest)]
    [Category(Categories.OpenApi)]
    [Category(Categories.Storyteller)]
    public class ConcurrentRestCallTests : TestBase
    {
        private IUser _user;
        private IProject _project;

        #region Setup and Cleanup

        [SetUp]
        public void ClassSetUp()
        {
            Helper = new TestHelper();
            _user = Helper.CreateUserAndAuthenticate(TestHelper.AuthenticationTokenTypes.BothAccessControlAndOpenApiTokens);
            _project = ProjectFactory.GetProject(_user);
        }

        [TearDown]
        public void ClassTearDown()
        {
            Helper?.Dispose();
        }

        #endregion Setup and Cleanup

        [TestCase(1, 1)]
        [TestCase(5, 5)]
        [TestCase(10, 10)]
        [TestCase(50, 50)]
        [Explicit(IgnoreReasons.ProductBug)]    // Often fails with a deadlock error.  Run manually only.
        [TestRail(154502)]
        [Description("Tries to lock, discard, save, publish, get artifact info, get properties for Rapid Review, get version, post discussion and delete an artifact.  Verifies all REST calls succeeded.")]
        public void MultipleRestCallsForArtifact_ValidParams_Success(int numThreads, int iterations)
        {
            // Setup:
            ConcurrentTestHelper threadHelper = new ConcurrentTestHelper(Helper);

            // Create the users & threads.
            for (int i = 0; i < numThreads; ++i)
            {
                IUser user = Helper.CreateUserAndAuthenticate(TestHelper.AuthenticationTokenTypes.BothAccessControlAndOpenApiTokens);
                IArtifact artifact = Helper.CreateAndPublishArtifact(_project, user, BaseArtifactType.Process);

                threadHelper.AddTestFunctionToThread(() =>
                {
                    LockDiscardSavePublishGetArtifactInfoGetPropertiesForRapidReviewGetVersionPostDiscussionsAndDelete(
                        user, artifact, Helper.AdminStore);
                }, iterations);
            }

            // Execute & Verify:
            threadHelper.RunThreadsAndWaitToCompletion();
        }

        /// <summary>
        /// Tries to lock, discard, save, publish, get artifact info, get properties for Rapid Review,
        /// get version, post discussion and delete an artifact.
        /// </summary>
        /// <param name="user">The user to perform all the REST calls with.</param>
        /// <param name="artifact">The artifact to perform the operations against.</param>
        /// <param name="adminStore">AdminStore reference to re-authenticate the user to prevent a long running thread's session from expiring.</param>
        /// <exception cref="NUnit.Framework.AssertionException">If any of the REST calls fail.</exception>
        private static void LockDiscardSavePublishGetArtifactInfoGetPropertiesForRapidReviewGetVersionPostDiscussionsAndDelete(
            IUser user,
            IArtifact artifact,
            IAdminStore adminStore)
        {
            LockResultInfo lockResultInfo = null;

            Assert.DoesNotThrow(() => { lockResultInfo = artifact.Lock(user); });
            Assert.AreEqual(LockResult.Success, lockResultInfo.Result, "The lockResultInfo.Result should be Success!");

            Assert.DoesNotThrow(() => { artifact.Discard(user); });
            Assert.DoesNotThrow(() => { artifact.Save(user); });
            Assert.DoesNotThrow(() => { artifact.Publish(user); });

            Assert.DoesNotThrow(() => { artifact.GetArtifactInfo(user); });
            Assert.DoesNotThrow(() => { artifact.GetPropertiesForRapidReview(user); });
            Assert.DoesNotThrow(() => { artifact.GetVersion(user); });
            Assert.DoesNotThrow(() => { artifact.PostRaptorDiscussions("Discussion text", user); });

            Assert.DoesNotThrow(() => { artifact.Save(user); });
            NovaPublishArtifactResult publishResult = null;
            Assert.DoesNotThrow(() => { publishResult = artifact.StorytellerPublish(user); });
            Assert.AreEqual(NovaPublishArtifactResult.Result.Success, publishResult.StatusCode, "NovaPublish failed!");

            Assert.DoesNotThrow(() => { artifact.Delete(user); });
            Assert.DoesNotThrow(() => { artifact.NovaDiscard(user); });

            // Refresh the token.
            adminStore.AddSession(user, force: true);
        }
    }
}
