using CustomAttributes;
using Helper;
using Model;
using Model.Factories;
using Model.Impl;
using NUnit.Framework;
using System.Collections.Generic;
using System.Net;
using TestCommon;
using Utilities;

namespace OpenAPITests
{
    [TestFixture]
    [Category(Categories.OpenApi)]
    public class DeleteUserTests : TestBase
    {
//        private IBlueprintServer _server = BlueprintServerFactory.GetBlueprintServerFromTestConfig();
//        private IUser _user = null;

        [SetUp]
        public void SetUp()
        {
            Helper = new TestHelper();
//            _user = Helper.CreateUserAndAuthenticate(TestHelper.AuthenticationTokenTypes.OpenApiToken);
        }

        [TearDown]
        public void TearDown()
        {
            Helper?.Dispose();
        }

        [TestCase]
        [TestRail(0)]
        [Description("Delete list of users and verify")]
        public static void CreateUser_VerifyUserCreated(/*List<string> usernamesToRemove*/)
        {
            // Setup:

        }

        #region Private methods

        /// <summary>
        /// Validates that all users from usernames list were removed
        /// </summary>
        /// <param name="resultSet">Result set from delete users call</param>
        /// <param name="usernamesToRemove">List of usernames to delete</param>
        public static void DeleteUserVerification(DeleteResultSet resultSet, List<string> usernamesToRemove)
        {
            ThrowIf.ArgumentNull(usernamesToRemove, nameof(usernamesToRemove));
            ThrowIf.ArgumentNull(resultSet, nameof(resultSet));

            Assert.AreEqual(HttpStatusCode.Created, resultSet.ReturnCode, "It should be 201 Created when all users removed!");

            foreach (var userToDelete in usernamesToRemove)
            {
                var result = resultSet.Results.Find(a => a.Username == userToDelete);

                Assert.AreEqual("User successfully deleted.", result.Message, "Message is incorrect!");
                Assert.AreEqual(HttpStatusCode.Created, result.ReturnCode, "Return code should be 201 Created!");
            }
        }

        #endregion Private methods
    }
}
