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
        [Description("")]
        public void GetHistoricalVersion_VerifyReturnedProcess()
        {
            var artifact = Helper.Storyteller.CreateAndSaveProcessArtifact(_project, BaseArtifactType.Process, _user);

            int savedArtifactVersion = artifact.Version;

            var process = Helper.Storyteller.GetProcess(_user, artifact.Id);

            // Assert that version of the saved artifact and saved process are equal
            Assert.AreEqual(savedArtifactVersion, process.Status.VersionId,  
                "The saved artifact version does not equal the saved process version.");

            // Assert that the version of the saved process is correct (-1)
            Assert.AreEqual(process.Status.VersionId, SavedVersionId, 
                "The process version Id was {0} but {1} was expected", process.Status.VersionId, SavedVersionId);

            // Assert that the process type is Business Process
            Assert.That(process.ProcessType == ProcessType.BusinessProcess, "Process was not a Business Process.");

            // Modify process type to User to System Process
            process.ProcessType = ProcessType.UserToSystemProcess;

            var savedProcess = StorytellerTestHelper.UpdateAndVerifyProcess(process, Helper.Storyteller, _user);

            // Assert that saving a change to a process does not increment the process version
            Assert.AreEqual(process.Status.VersionId, savedProcess.Status.VersionId,
                "The process version should not change after a process is updated.");

            Helper.Storyteller.PublishProcess(_user, savedProcess);

            var publishedProcess = Helper.Storyteller.GetProcess(_user, process.Id);

            // Assert that the published process has the first published version Id (1)
            Assert.AreEqual(publishedProcess.Status.VersionId, FirstPublishedVersionId,
                "The process version Id was {0} but {1} was expected", 
                publishedProcess.Status.VersionId, FirstPublishedVersionId);

            // Modify process type back to Business Process
            publishedProcess.ProcessType = ProcessType.BusinessProcess;

            var savedProcess2 = StorytellerTestHelper.UpdateAndVerifyProcess(publishedProcess, Helper.Storyteller, _user);

            Helper.Storyteller.PublishProcess(_user, savedProcess2);

            var publishedProcess2 = Helper.Storyteller.GetProcess(_user, process.Id);

            // Assert that version Id is incremented
            Assert.AreEqual(publishedProcess2.Status.VersionId, FirstPublishedVersionId + 1,
                "The process version Id was {0} but {1} was expected", 
                publishedProcess2.Status.VersionId, FirstPublishedVersionId + 1);

            // Get the process for the first published version (1)
            var historicalProcess = Helper.Storyteller.GetProcess(_user, artifact.Id, FirstPublishedVersionId);

            Assert.IsFalse(historicalProcess.RequestedVersionInfo.IsHeadOrSavedDraftVersion, 
                "A historical version was expected but not returned.");

            Assert.IsTrue(historicalProcess.RequestedVersionInfo.IsVersionInformationProvided, 
                "Version information was provided but not returned.");

            Assert.AreEqual(publishedProcess.Status.VersionId, historicalProcess.RequestedVersionInfo.VersionId, 
                "The historical version Id did not match the expected version Id.");

            StorytellerTestHelper.AssertProcessesAreIdentical(publishedProcess, historicalProcess);
        }
    }
}
