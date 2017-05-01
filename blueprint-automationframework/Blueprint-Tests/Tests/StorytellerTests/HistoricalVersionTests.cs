using CustomAttributes;
using Helper;
using Model;
using Model.ArtifactModel;
using Model.ArtifactModel.Impl;
using Model.Factories;
using Model.StorytellerModel;
using Model.StorytellerModel.Enums;
using Model.StorytellerModel.Impl;
using NUnit.Framework;
using System;
using TestCommon;

namespace StorytellerTests
{
    // TODO: Fix this to have 1 copy using NovaProcess and 1 copy using ST 1.0 Process
    [TestFixture]
    [Category(Categories.Storyteller)]
    public class HistoricalVersionTests : TestBase
    {
        private const int UnpublishedVersion = -1;
        private const int FirstPublishedVersion = 1;

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

        #endregion Setup and Cleanup

        #region Tests

        [TestCase]
        [TestRail(303354)]
        [Description("Gets a historical version of an artifact after changes have been made to the live version then " +
             "verifies that the historical version matches the original state of the artifact before the changes" +
             "were made.")]
        public void HistoricalVersion_PublishingProcessWithChanges_VerifyHistoricalVersion()
        {
            var novaProcess = Helper.Storyteller.CreateAndSaveNovaProcessArtifact(_project, _user);
            var stProcess = Helper.Storyteller.GetProcess(_user, novaProcess.Id);

            // Assert that the created process is non historical version by checking status and version
            ValidateProcessVersion(novaProcess, stProcess, UnpublishedVersion);
            Assert.AreEqual(ProcessType.BusinessProcess, stProcess.ProcessType,
                "'{0}' was expected from the process but '{1}' was returned",
                Enum.GetName(typeof(ProcessType), ProcessType.BusinessProcess),
                Enum.GetName(typeof(ProcessType), stProcess.ProcessType));

            Helper.Storyteller.PublishNovaProcess(_user, novaProcess);
            novaProcess = Helper.Storyteller.GetNovaProcess(_user, novaProcess.Id);
            stProcess = Helper.Storyteller.GetProcess(_user, novaProcess.Id);

            // Assert that the created process is non historical version by checking status and version
            ValidateProcessVersion(novaProcess, stProcess, FirstPublishedVersion);

            // Modify process type to User to System Process (savedProcess)
            stProcess.ProcessType = ProcessType.UserToSystemProcess;
            var savedProcess = StorytellerTestHelper.UpdateAndVerifyProcess(stProcess, Helper.Storyteller, _user);
            var savedNovaProcess = Helper.Storyteller.GetNovaProcess(_user, savedProcess.Id);

            // Assert that saving a change to a process does not increment the process version
            Assert.AreEqual(savedNovaProcess.Version, novaProcess.Version,
                "The process version should not change after a process is updated.");

            Helper.Storyteller.PublishNovaProcess(_user, savedNovaProcess);

            var publishedNovaProcess = Helper.Storyteller.GetNovaProcess(_user, novaProcess.Id);

            // Assert that version Id is incremented
            Assert.AreEqual(FirstPublishedVersion + 1, publishedNovaProcess.Version,
                "The expected process version is {0} but {1} is returned.",
                FirstPublishedVersion, publishedNovaProcess);

            // Get the process for the first published (historical) version
            var historicalNovaProcess = Helper.Storyteller.GetNovaProcess(_user, novaProcess.Id, FirstPublishedVersion);
            var historicalStProcess = Helper.Storyteller.GetProcess(_user, novaProcess.Id, FirstPublishedVersion);
            
            Assert.IsTrue(IsHistoricalVersion(historicalStProcess), "A historical version was expected but not returned.");

            // Verifies that the historical process matches the original version of the published process before a change was made.
            NovaArtifactBase.AssertAreEqual(novaProcess, historicalNovaProcess);

        }

        [Ignore(IgnoreReasons.TestBug)]
        [TestCase]
        [TestRail(125513)]
        [Description("Gets a historical version of an artifact after changes have been made to the live version then " +
             "verifies that the historical version matches the original state of the artifact before the changes" +
             "were made.")]
        public void GetHistoricalVersionOfPublishedProcessWithChange_VerifyReturnedHistoricalVersionDoesNotContainChange()
        {
            var artifact = Helper.Storyteller.CreateAndSaveProcessArtifact(_project, _user);

            int savedArtifactVersion = artifact.Version;

            var savedProcess = Helper.Storyteller.GetProcess(_user, artifact.Id);

            Assert.IsFalse(IsHistoricalVersion(savedProcess), "A historical version was returned when it was not expected.");

            // Assert that version of the saved artifact and saved process are equal
            Assert.AreEqual(savedArtifactVersion, savedProcess.Status.VersionId,
                "The saved artifact version does not equal the saved process version.");

            // Assert that the version of the saved process is correct (-1)
            Assert.AreEqual(savedProcess.Status.VersionId, UnpublishedVersion,
                "The process version Id was {0} but {1} was expected", savedProcess.Status.VersionId, UnpublishedVersion);

            // Assert that the process type is Business Process
            Assert.That(savedProcess.ProcessType == ProcessType.BusinessProcess, "Process was not a Business Process.");

            Helper.Storyteller.PublishProcess(_user, savedProcess);

            var publishedProcess = Helper.Storyteller.GetProcess(_user, artifact.Id);

            Assert.IsFalse(IsHistoricalVersion(publishedProcess), "A historical version was returned when it was not expected.");

            // Assert that the published process has the first published version Id (1)
            Assert.AreEqual(publishedProcess.Status.VersionId, FirstPublishedVersion,
                "The process version Id was {0} but {1} was expected",
                publishedProcess.Status.VersionId, FirstPublishedVersion);

            // Modify process type to User to System Process
            publishedProcess.ProcessType = ProcessType.UserToSystemProcess;

            var savedProcess2 = StorytellerTestHelper.UpdateAndVerifyProcess(publishedProcess, Helper.Storyteller, _user);

            // Assert that saving a change to a process does not increment the process version
            Assert.AreEqual(publishedProcess.Status.VersionId, savedProcess2.Status.VersionId,
                "The process version should not change after a process is updated.");

            Helper.Storyteller.PublishProcess(_user, savedProcess2);

            var publishedProcess2 = Helper.Storyteller.GetProcess(_user, artifact.Id);

            // Assert that version Id is incremented (2)
            Assert.AreEqual(publishedProcess2.Status.VersionId, FirstPublishedVersion + 1,
                "The process version Id was {0} but {1} was expected",
                publishedProcess2.Status.VersionId, FirstPublishedVersion + 1);

            // Get the process for the first published (historical) version (1)
            var historicalProcess = Helper.Storyteller.GetProcess(_user, artifact.Id, FirstPublishedVersion);

            Assert.IsTrue(IsHistoricalVersion(historicalProcess), "A historical version was expected but not returned.");

            Assert.IsTrue(historicalProcess.RequestedVersionInfo.IsVersionInformationProvided,
                "Version information was provided but not returned.");

            // Check if the historical process version Id matches the original published process version Id
            Assert.AreEqual(publishedProcess.Status.VersionId, historicalProcess.RequestedVersionInfo.VersionId,
                "The historical version Id did not match the expected version Id.");

            // Verifies that the historical process matches the original version of the published process
            // before a change was made.
            Process.AssertAreEqual(publishedProcess, historicalProcess);
        }

        #endregion Tests

        #region private functions

        /// <summary>
        /// Validate Process version
        /// </summary>
        /// <param name="novaProcess">the nova process to validate</param>
        /// <param name="stProcess">the equvalent storyteller 1.0 process to validate</param>
        /// <param name="expectedVersion">the expected version number to validate against</param>
        private static void ValidateProcessVersion(INovaProcess novaProcess, IProcess stProcess, int expectedVersion)
        {
            // Assert that the created process is non historical version by checking status and version
            Assert.IsFalse(IsHistoricalVersion(stProcess), "A historical version was returned when it was not expected.");
            Assert.AreEqual(expectedVersion, novaProcess.Version,
                "The expected process version is {0} but {1} is returned.", expectedVersion, novaProcess.Version);
        }

        /// <summary>
        /// Check whether a process is a historical version
        /// </summary>
        /// <param name="process">The process to be checked as historical</param>
        /// <returns>Returns true if the process is a historical version; Else returns false.</returns>
        private static bool IsHistoricalVersion(IProcess process)
        {
            return process.Status.IsReadOnly && !process.RequestedVersionInfo.IsHeadOrSavedDraftVersion;
        }

        #endregion private functions
    }
}
