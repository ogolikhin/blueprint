using CustomAttributes;
using Model;
using Model.ArtifactModel;
using Model.Factories;
using NUnit.Framework;
using Model.StorytellerModel;
using Helper;
using TestCommon;

namespace StorytellerTests
{
    [TestFixture]
    [Category(Categories.Storyteller)]
    public class HistoricalVersionTests : TestBase
    {
        private const int SavedVersionId = -1;
        private const int FirstPublishedVersionId = 1;

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

        [TestCase]
        [TestRail(125513)]
        [Description("Gets a historical version of an artifact after changes have been made to the live version then " +
                     "verifies that the historical version matches the original state of the artifact before the changes" +
                     "were made.")]
        public void GetHistoricalVersion_VerifyReturnedProcess()
        {
            var artifact = Helper.Storyteller.CreateAndSaveProcessArtifact(_project, BaseArtifactType.Process, _user);

            int savedArtifactVersion = artifact.Version;

            var process = Helper.Storyteller.GetProcess(_user, artifact.Id);

            Assert.IsFalse(IsHistoricalVersion(process), "A historical version was returned when it was not expected.");

            // Assert that version of the saved artifact and saved process are equal
            Assert.AreEqual(savedArtifactVersion, process.Status.VersionId,  
                "The saved artifact version does not equal the saved process version.");

            // Assert that the version of the saved process is correct (-1)
            Assert.AreEqual(process.Status.VersionId, SavedVersionId, 
                "The process version Id was {0} but {1} was expected", process.Status.VersionId, SavedVersionId);

            // Assert that the process type is Business Process
            Assert.That(process.ProcessType == ProcessType.BusinessProcess, "Process was not a Business Process.");

            var savedProcess = StorytellerTestHelper.UpdateAndVerifyProcess(process, Helper.Storyteller, _user);

            Helper.Storyteller.PublishProcess(_user, savedProcess);

            var publishedProcess = Helper.Storyteller.GetProcess(_user, artifact.Id);

            Assert.IsFalse(IsHistoricalVersion(publishedProcess), "A historical version was returned when it was not expected.");

            // Assert that the published process has the first published version Id (1)
            Assert.AreEqual(publishedProcess.Status.VersionId, FirstPublishedVersionId,
                "The process version Id was {0} but {1} was expected", 
                publishedProcess.Status.VersionId, FirstPublishedVersionId);

            // Modify process type to User to System Process
            publishedProcess.ProcessType = ProcessType.UserToSystemProcess;

            var savedProcess2 = StorytellerTestHelper.UpdateAndVerifyProcess(publishedProcess, Helper.Storyteller, _user);

            // Assert that saving a change to a process does not increment the process version
            Assert.AreEqual(publishedProcess.Status.VersionId, savedProcess2.Status.VersionId,
                "The process version should not change after a process is updated.");

            Helper.Storyteller.PublishProcess(_user, savedProcess2);

            var publishedProcess2 = Helper.Storyteller.GetProcess(_user, artifact.Id);

            // Assert that version Id is incremented (2)
            Assert.AreEqual(publishedProcess2.Status.VersionId, FirstPublishedVersionId + 1,
                "The process version Id was {0} but {1} was expected", 
                publishedProcess2.Status.VersionId, FirstPublishedVersionId + 1);

            // Get the process for the first published version (1)
            var historicalProcess = Helper.Storyteller.GetProcess(_user, artifact.Id, FirstPublishedVersionId);

            Assert.IsTrue(IsHistoricalVersion(historicalProcess), "A historical version was expected but not returned.");

            Assert.IsTrue(historicalProcess.RequestedVersionInfo.IsVersionInformationProvided, 
                "Version information was provided but not returned.");

            // Check if the historical process version Id matches the original published process version Id
            Assert.AreEqual(publishedProcess.Status.VersionId, historicalProcess.RequestedVersionInfo.VersionId, 
                "The historical version Id did not match the expected version Id.");

            // Verifies that the historical process matches the original version of the published process
            // before a change was made.
            StorytellerTestHelper.AssertProcessesAreIdentical(publishedProcess, historicalProcess);
        }

        /// <summary>
        /// Check whether a returned process is a historical version
        /// </summary>
        /// <param name="returnedProcess">The process returned from the Get call</param>
        /// <returns>Returns true is process is a hsitorical version; Else returns false.</returns>
        private static bool IsHistoricalVersion(IProcess returnedProcess)
        {
            
            return returnedProcess.Status.IsReadOnly && !returnedProcess.RequestedVersionInfo.IsHeadOrSavedDraftVersion;
        }
    }
}
