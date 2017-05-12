using Common;
using CustomAttributes;
using Helper;
using Model;
using Model.ArtifactModel.Enums;
using Model.ArtifactModel.Impl;
using Model.Factories;
using Model.ModelHelpers;
using NUnit.Framework;
using System.Collections.Generic;
using TestCommon;
using Utilities;

namespace CommonServiceTests
{
    public class PublishTests : TestBase
    {
        private IUser _user;
        private IProject _project;
        private const string SVC_PATH = RestPaths.Svc.Shared.Artifacts.PUBLISH;

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

        #endregion Setup and Cleanup

        #region Tests

        [TestCase(ItemTypePredefined.Actor)]
        [TestRail(125503)]
        [Description("Create, save, publish artifact, check returned results.")]
        public void Publish_SavedArtifact_PublishWasSuccessful(ItemTypePredefined artifactType)
        {
            // Setup:
            var artifact = Helper.CreateNovaArtifact(_user, _project, artifactType);
            
            List<NovaPublishArtifactResult> publishResult = null;

            // Execute:
            Assert.DoesNotThrow(() =>
            {
                publishResult = Helper.SvcShared.PublishArtifacts(
                    _user, new List<int> { artifact.Id });
            }, "POST {0} failed when publishing a saved artifact!", SVC_PATH);

            // Verify:
            const string expectedMessage = "Successfully published";

            ValidateSvcSharedPublishResult(new List<ArtifactWrapper> { artifact },
                NovaPublishArtifactResult.Result.Success,
                expectedMessage,
                publishResult);
        }

        [TestCase]
        [TestRail(125504)]
        [Description("Create, save, publish artifact, publish again, check returned results.")]
        public void Publish_PublishedArtifactWithNoDraftChanges_ArtifactAlreadyPublishedMessage()
        {
            // Setup:
            var artifact = Helper.CreateAndPublishNovaArtifact(_user, _project, ItemTypePredefined.Actor);

            List<NovaPublishArtifactResult> publishResult = null;

            // Execute:
            Assert.DoesNotThrow(() =>
            {
                publishResult = Helper.SvcShared.PublishArtifacts(_user,
                    new List<int> { artifact.Id });
            }, "POST {0} with a published artifact should return 200 OK!", SVC_PATH);

            // Verify:
            string expectedMessage = I18NHelper.FormatInvariant("Artifact {0} is already published in the project", artifact.Id);

            ValidateSvcSharedPublishResult(new List<ArtifactWrapper> { artifact },
                NovaPublishArtifactResult.Result.ArtifactAlreadyPublished,
                expectedMessage,
                publishResult);
        }

        #endregion Tests

        #region private functions

        /// <summary>
        /// Validates the returned PublishResult from POST /svc/shared/artifacts/publish
        /// </summary>
        /// <param name="artifacts"> the list of artifacts that are published. </param>
        /// <param name="expectedPublishStatus"> the expected publish status. </param>
        /// <param name="expectedPublishMessage"> the expected publish message. </param>
        /// <param name="actualPublishResult"> the returned publish result. </param>
        private static void ValidateSvcSharedPublishResult (
            List<ArtifactWrapper> artifacts,
            NovaPublishArtifactResult.Result expectedPublishStatus,
            string expectedPublishMessage,
            List<NovaPublishArtifactResult> actualPublishResult)
        {
            ThrowIf.ArgumentNull(artifacts, nameof(artifacts));
            ThrowIf.ArgumentNull(expectedPublishStatus, nameof(expectedPublishStatus));
            ThrowIf.ArgumentNull(expectedPublishMessage, nameof(expectedPublishMessage));
            ThrowIf.ArgumentNull(actualPublishResult, nameof(actualPublishResult));

            //Verify that number of artifacts published is same as the number items returned from actual publishResult
            Assert.AreEqual(artifacts.Count, actualPublishResult.Count,
                "the expected number of artifacts published is {0} but the actual response contains {1}.",
                artifacts.Count, actualPublishResult.Count);

            for (int i = 0; i < artifacts.Count; i++)
            {
                //Update status with based on the response status from the publish call.
                if (actualPublishResult[i].StatusCode == NovaPublishArtifactResult.Result.Success)
                {
                    artifacts[i].ArtifactState.IsPublished = true;
                }

                // Verify:
                Assert.AreEqual(expectedPublishStatus, actualPublishResult[i].StatusCode,
                    "the expected result status is {0} but the returned result status is {1}.",
                    expectedPublishStatus, actualPublishResult[i].StatusCode);

                Assert.AreEqual(expectedPublishMessage, actualPublishResult[i].Message,
                    "the expected result message is {0} but the returned result message is {1}.",
                    expectedPublishMessage, actualPublishResult[i].Message);
            }
        }

        #endregion private functions

    }
}