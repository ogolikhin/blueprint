using CustomAttributes;
using Helper;
using Model;
using Model.Factories;
using Model.StorytellerModel.Impl;
using NUnit.Framework;
using TestCommon;

namespace StorytellerTests
{
    [TestFixture]
    [Category(Categories.Storyteller)]
    public class DragAndDropTests : TestBase
    {
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
        [Description("Move a user and system task to a different location in the process tree.  Verify that the returned process" +
                     "has the user and system task in the new location.")]
        public void MoveUserAndSystemTask_VerifyReturnedProcess()
        {
            /*
            If you start with this:
                [S]--[P]--+--[UT1]--+--[ST2]--+--[UT3]--+--[ST4]--+--[E]


            It becomes this:
                [S]--[P]--+--[UT3]--+--[ST4]--+--[UT1]--+--[ST2]--+--[E]
            */

            var novaProcess = StorytellerTestHelper.CreateAndGetDefaultNovaProcessWithTwoSequentialUserTasks(_project, _user);

            var defaultUserTask = novaProcess.Process.GetProcessShapeByShapeName(Process.DefaultUserTaskName);

            var endShape = novaProcess.Process.GetProcessShapeByShapeName(Process.EndName);

            novaProcess.Process.MoveUserAndSystemTaskBeforeShape(defaultUserTask, endShape);

            // Update and Verify the modified process
            StorytellerTestHelper.UpdateVerifyAndPublishNovaProcess(novaProcess.NovaProcess, _user);
        }
    }
}
