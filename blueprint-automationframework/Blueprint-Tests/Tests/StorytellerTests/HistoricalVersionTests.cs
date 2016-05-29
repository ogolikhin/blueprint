using System.Linq;
using CustomAttributes;
using Model;
using Model.ArtifactModel;
using Model.ArtifactModel.Impl;
using Model.Factories;
using NUnit.Framework;
using System.Collections.Generic;
using Model.StorytellerModel;
using Model.StorytellerModel.Impl;
using Helper;
using System.Net;
using TestCommon;
using Utilities.Factories;

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

            // Modify process name
            process.Name = RandomGenerator.RandomValueWithPrefix("ModifiedProcess", 4);
            
            var savedProcess = StorytellerTestHelper.UpdateAndVerifyProcess(process, Helper.Storyteller, _user);

            
            // Assert that saving a change to a process does not increment the process version
            Assert.AreEqual(process.Status.VersionId, savedProcess.Status.VersionId,
                "The process version should not change after a process is updated.");

            Helper.Storyteller.PublishProcess(_user, savedProcess);

            var publishedProcess = Helper.Storyteller.GetProcess(_user, savedProcess.Id);

            // Assert that the published process has the first published version Id (1)
            Assert.AreEqual(publishedProcess.Status.VersionId, FirstPublishedVersionId,
                "The process version Id was {0} but {1} was expected", publishedProcess.Status.VersionId, FirstPublishedVersionId);

            // Modify process name again
            publishedProcess.Name = RandomGenerator.RandomValueWithPrefix("ModifiedProcess", 4);

            var savedProcess2 = StorytellerTestHelper.UpdateAndVerifyProcess(publishedProcess, Helper.Storyteller, _user);

            Helper.Storyteller.PublishProcess(_user, savedProcess2);

            var publishedProcess2 = Helper.Storyteller.GetProcess(_user, savedProcess2.Id);

            // Assert that version Id is incremented
            Assert.AreEqual(publishedProcess2.Status.VersionId, FirstPublishedVersionId + 1,
                "The process version Id was {0} but {1} was expected", publishedProcess2.Status.VersionId, FirstPublishedVersionId + 1);


        }
    }
}
