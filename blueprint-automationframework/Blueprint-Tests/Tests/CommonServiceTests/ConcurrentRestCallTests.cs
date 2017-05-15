using CustomAttributes;
using Helper;
using Model;
using Model.ArtifactModel.Enums;
using Model.ArtifactModel.Impl;
using Model.Factories;
using Model.ModelHelpers;
using NUnit.Framework;
using System.Collections.Generic;
using System.Linq;
using TestCommon;
using Utilities;

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
        [Explicit(IgnoreReasons.ManualOnly)]    // Often fails with a deadlock error.  Run manually only.
        [TestRail(154502)]
        [Description("Tries to lock, discard, save, publish, get artifact info, get properties for Rapid Review, get artifact details, post discussion and delete an artifact.  " +
            "Verifies all REST calls succeeded.")]
        public void MultipleRestCallsForArtifact_ValidParams_Success(int numThreads, int iterations)
        {
            // Setup:
            var threadHelper = new ConcurrentTestHelper(Helper);

            // Create the users & threads.
            for (int i = 0; i < numThreads; ++i)
            {
                var user = Helper.CreateUserAndAuthenticate(TestHelper.AuthenticationTokenTypes.BothAccessControlAndOpenApiTokens);
                var artifact = Helper.CreateAndPublishNovaArtifact(user, _project, ItemTypePredefined.Actor);

                threadHelper.AddTestFunctionToThread(() =>
                {
                    LockDiscardSavePublishGetArtifactInfoGetPropertiesForRapidReviewGetVersionPostDiscussionsAndDelete(
                        Helper,
                        user, artifact);
                }, iterations);
            }

            // Execute & Verify:
            threadHelper.RunThreadsAndWaitToCompletion();
        }

        /// <summary>
        /// Tries to lock, discard, save, publish, get artifact info, get properties for Rapid Review,
        /// get artifact details, post discussion and delete an artifact.
        /// </summary>
        /// <param name="testHelper">TestHelper reference to which contains adminstore to re-authenticate the user to prevent a long
        /// running thread's session from expiring.</param>
        /// <param name="user">The user to perform all the REST calls with.</param>
        /// <param name="artifact">The artifact to perform the operations against.</param>
        /// <exception cref="NUnit.Framework.AssertionException">If any of the REST calls fail.</exception>
        private static void LockDiscardSavePublishGetArtifactInfoGetPropertiesForRapidReviewGetVersionPostDiscussionsAndDelete(
            TestHelper testHelper,
            IUser user,
            ArtifactWrapper artifact)
        {
            ThrowIf.ArgumentNull(testHelper, nameof(testHelper));
            ThrowIf.ArgumentNull(user, nameof(user));
            ThrowIf.ArgumentNull(artifact, nameof(artifact));

            List<LockResultInfo> lockResultInfo = null;

            Assert.DoesNotThrow(() => lockResultInfo = artifact.Lock(user));
            Assert.AreEqual(LockResult.Success, lockResultInfo.First().Result,
                "The lockResultInfo.Result should be Success!");

            Assert.DoesNotThrow(() => artifact.Discard(user));
            Assert.DoesNotThrow(() => artifact.Lock(user));
            Assert.DoesNotThrow(() => artifact.SaveWithNewDescription(user));
            Assert.DoesNotThrow(() => artifact.Publish(user));

            Assert.DoesNotThrow(() => testHelper.SvcComponents.GetArtifactInfo(artifact.Id, user));
            Assert.DoesNotThrow(() => testHelper.SvcComponents.GetRapidReviewArtifactsProperties(user, new List<int> { artifact.Id }));
            Assert.DoesNotThrow(() => testHelper.ArtifactStore.GetArtifactDetails(user, artifact.Id));
            Assert.DoesNotThrow(() => testHelper.SvcComponents.PostRapidReviewDiscussion(user, artifact.Id, "Discussion text"));

            Assert.DoesNotThrow(() => artifact.Delete(user));
            Assert.DoesNotThrow(() => artifact.Discard(user));

            // Refresh the token.
            testHelper.AdminStore.AddSession(user, force: true);
        }
    }
}
