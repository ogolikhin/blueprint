using System;
using CustomAttributes;
using Model;
using Model.Factories;
using NUnit.Framework;
using Helper;
using Model.StorytellerModel;
using Model.StorytellerModel.Impl;

namespace StorytellerTests
{
    [TestFixture]
    [Category(Categories.Storyteller)]
    public class DragAndDropTests
    {
        private IAdminStore _adminStore;
        private IBlueprintServer _blueprintServer;
        private IStoryteller _storyteller;
        private IUser _user;
        private IProject _project;
        private bool _deleteChildren = true;

        #region Setup and Cleanup

        [TestFixtureSetUp]
        public void ClassSetUp()
        {
            _adminStore = AdminStoreFactory.GetAdminStoreFromTestConfig();
            _blueprintServer = BlueprintServerFactory.GetBlueprintServerFromTestConfig();
            _storyteller = StorytellerFactory.GetStorytellerFromTestConfig();
            _user = UserFactory.CreateUserAndAddToDatabase();
            _project = ProjectFactory.GetProject(_user);

            // Get a valid Access Control token for the user (for the new Storyteller REST calls).
            ISession session = _adminStore.AddSession(_user.Username, _user.Password);
            _user.SetToken(session.SessionId);

            Assert.IsFalse(string.IsNullOrWhiteSpace(_user.Token.AccessControlToken), "The user didn't get an Access Control token!");

            // Get a valid OpenApi token for the user (for the OpenApi artifact REST calls).
            _blueprintServer.LoginUsingBasicAuthorization(_user, string.Empty);

            Assert.IsFalse(string.IsNullOrWhiteSpace(_user.Token.OpenApiToken), "The user didn't get an OpenApi token!");
        }

        [TestFixtureTearDown]
        public void ClassTearDown()
        {
            if (_storyteller.Artifacts != null)
            {
                // TODO: implement discard artifacts for test cases that doesn't publish artifacts
                // Delete all the artifacts that were added.
                foreach (var artifact in _storyteller.Artifacts.ToArray())
                {
                    _storyteller.DeleteProcessArtifact(artifact, deleteChildren: _deleteChildren);
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

        #endregion Setup and Cleanup

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Performance", "CA1822:MarkMembersAsStatic")]
        [TestCase]
        [Description("")]
        public void MoveUserAndSystemTask1_VerifyReturnedProcess()
        {
            /*
            If you start with this:
                [S]--[P]--+--[[UT1]--+--[ST2]]--+--[UT3]--+--[ST4]--+--[E]


            It becomes this:
                [S]--[P]--+--[UT3]--+--[ST4]--+--[[UT1]--+--[ST2]]--+--[E]
            */

            var process = StorytellerTestHelper.CreateAndGetDefaultProcess(_storyteller, _project, _user);

            var defaultUserTask = process.GetProcessShapeByShapeName(Process.DefaultUserTaskName);

            var endShape = process.GetProcessShapeByShapeName(Process.EndName);

            process.MoveUserAndSystemTaskBeforeShape(defaultUserTask, endShape);

            // Update and Verify the modified process
            StorytellerTestHelper.UpdateVerifyAndPublishProcess(process, _storyteller, _user);
        }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Performance", "CA1822:MarkMembersAsStatic")]
        [TestCase]
        [Description("")]
        public void MoveUserAndSystemTask2_VerifyReturnedProcess()
        {
            /*
            If you start with this:
                [S]--[P]--+--[UT1]--+--[ST2]--+--[[UT3]--+--[ST4]]--+--[E]


            It becomes this:
                [S]--[P]--+--[[UT3]--+--[ST4]]--+--[UT1]--+--[ST2]--+--[E]
            */

            throw new NotImplementedException();
        }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Performance", "CA1822:MarkMembersAsStatic")]
        [TestCase]
        [Description("")]
        public void MoveUserAndSystemTask3_VerifyReturnedProcess()
        {
            /*
            If you start with this:
                [S]--[P]--+--<UD>--+--[UT1]--+--[ST2]--+--+--[E]
                              |                           |
                              +----+--[UT3]--+--[ST4]--+--+

            It becomes this:
                [S]--[P]--+--<UD>--+--[UT1]--+--[ST2]--+--[UT3]--+--[ST4]--+--+--[E]
                              |                                               |
                              +----+--[UT5]--+--[ST6]--+----------------------+
            */

            throw new NotImplementedException();
        }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Performance", "CA1822:MarkMembersAsStatic")]
        [TestCase]
        [Description("")]
        public void MoveUserAndSystemTask4_VerifyReturnedProcess()
        {
            /*
            If you start with this:
                [S]--[P]--+--<UD>--+--[UT1]--+--[ST2]--+--+--[E]
                              |                           |
                              +----+--[UT3]--+--[ST4]--+--+

            It becomes this:
                [S]--[P]--+--<UD>--+--[UT3]--+--[ST4]--+--[UT1]--+--[ST2]--+--+--[E]
                              |                                               |
                              +----+--[UT5]--+--[ST6]--+----------------------+
            */

            throw new NotImplementedException();
        }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Performance", "CA1822:MarkMembersAsStatic")]
        [TestCase]
        [Description("")]
        public void MoveUserAndSystemTask5_VerifyReturnedProcess()
        {
            /*
            If you start with this:
                [S]--[P]--+--<UD>--+--[UT1]--+--[ST2]--+--+--[E]
                              |                           |
                              +----+--[UT3]--+--[ST4]--+--+

            It becomes this:
                [S]--[P]--+--[UT3]--+--[ST4]--+--<UD>--+--[UT1]--+--[ST2]--+--+--[E]
                                                  |                           |
                                                  +----+--[UT5]--+--[ST6]--+--+
            */

            throw new NotImplementedException();
        }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Performance", "CA1822:MarkMembersAsStatic")]
        [TestCase]
        [Description("")]
        public void MoveUserAndSystemTask6_VerifyReturnedProcess()
        {
            /*
            If you start with this:
                [S]--[P]--+--<UD>--+--[UT1]--+--[ST2]--+--+--[E]
                              |                           |
                              +----+--[UT3]--+--[ST4]--+--+

            It becomes this:
                [S]--[P]--+--<UD>--+--[UT1]--+--[ST2]--+--+--[UT3]--+--[ST4]--+--[E]
                              |                           |
                              +----+--[UT5]--+--[ST6]--+--+
            */

            throw new NotImplementedException();
        }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Performance", "CA1822:MarkMembersAsStatic")]
        [TestCase]
        [Description("")]
        public void MoveUserAndSystemTask7_VerifyReturnedProcess()
        {
            /*
            If you start with this:
                [S]--[P]--+--[UT3]--+--[ST4]--+--<UD>--+--[UT1]--+--[ST2]--+--+--[E]
                                                  |                           |
                                                  +----+--[UT5]--+--[ST6]--+--+

            It becomes this:
                [S]--[P]--+--<UD>--+--[UT1]--+--[ST2]--+----------------------+--[E]
                              |                                               |
                              +----+--[UT5]--+--[ST6]--+--[UT3]--+--[ST4]--+--+
            */

            throw new NotImplementedException();
        }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Performance", "CA1822:MarkMembersAsStatic")]
        [TestCase]
        [Description("")]
        public void MoveUserAndSystemTask8_VerifyReturnedProcess()
        {
            /*
            If you start with this:
                [S]--[P]--+--[UT3]--+--[ST4]--+--<UD>--+--[UT1]--+--[ST2]--+--+--[E]
                                                  |                           |
                                                  +----+--[UT5]--+--[ST6]--+--+

            It becomes this:
                [S]--[P]--+--<UD>--+--[UT1]--+--[ST2]--+----------------------+--[E]
                              |                                               |
                              +----+--[UT3]--+--[ST4]--+--[UT5]--+--[ST6]--+--+
            */

            throw new NotImplementedException();
        }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Performance", "CA1822:MarkMembersAsStatic")]
        [TestCase]
        [Description("")]
        public void MoveUserAndSystemTask9_VerifyReturnedProcess()
        {
            /*
            If you start with this:
                [S]--[P]--+--<UD>--+--[UT1]--+--[ST2]--+--+--[UT3]--+--[ST4]--+--[E]
                              |                           |
                              +----+--[UT5]--+--[ST6]--+--+

            It becomes this:
                [S]--[P]--+--<UD>--+--[UT1]--+--[ST2]--+----------------------+--[E]
                              |                                               |
                              +----+--[UT3]--+--[ST4]--+--[UT5]--+--[ST6]--+--+
            */

            throw new NotImplementedException();
        }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Performance", "CA1822:MarkMembersAsStatic")]
        [TestCase]
        [Description("")]
        public void MoveUserAndSystemTask10_VerifyReturnedProcess()
        {
            /*
            If you start with this:
                [S]--[P]--+--<UD>--+--[UT1]--+--[ST2]--+--+--[UT3]--+--[ST4]--+--[E]
                              |                           |
                              +----+--[UT5]--+--[ST6]--+--+

            It becomes this:
                [S]--[P]--+--<UD>--+--[UT1]--+--[ST2]--+----------------------+--[E]
                              |                                               |
                              +----+--[UT5]--+--[ST6]--+--[UT3]--+--[ST4]--+--+
            */

            throw new NotImplementedException();
        }
    }
}
