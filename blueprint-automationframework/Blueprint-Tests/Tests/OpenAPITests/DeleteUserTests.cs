using CustomAttributes;
using Helper;
using Model;
using NUnit.Framework;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using Common;
using Model.OpenApiModel.UserModel;
using TestCommon;
using Utilities;

namespace OpenAPITests
{
    [TestFixture]
    [Category(Categories.OpenApi)]
    public class DeleteUserTests : TestBase
    {
        private const string DELETE_PATH = RestPaths.OpenApi.Users.DELETE;

//        private IBlueprintServer _server = BlueprintServerFactory.GetBlueprintServerFromTestConfig();
        private IUser _adminUser = null;

        [SetUp]
        public void SetUp()
        {
            Helper = new TestHelper();
            _adminUser = Helper.CreateUserAndAuthenticate(TestHelper.AuthenticationTokenTypes.OpenApiToken);
        }

        [TearDown]
        public void TearDown()
        {
            Helper?.Dispose();
        }

        #region Positive tests

        [Explicit(IgnoreReasons.UnderDevelopmentDev)]
        [TestCase(1)]
        [TestCase(5)]
        [TestRail(246522)]
        [Description("Delete one or more users and verify that the users were deleted.")]
        public void DeleteUser_ValidUserParameters_VerifyUserDeleted(int numberOfUsersToDelete)
        {
            // Setup:
            var usersToDelete = new List<IUser>();

            for (int i = 0; i < numberOfUsersToDelete; ++i)
            {
                var userToDelete = Helper.CreateUserAndAuthenticate(TestHelper.AuthenticationTokenTypes.BothAccessControlAndOpenApiTokens);
                usersToDelete.Add(userToDelete);
            }

            var usernamesToDelete = usersToDelete.Select(u => u.Username).ToList();

            // Execute:
            DeleteUserResultSet result = null;

            Assert.DoesNotThrow(() => result = Helper.OpenApi.DeleteUser(_adminUser, usernamesToDelete), 
                "'DELETE {0}' should return 200 OK when valid data is passed to it!", DELETE_PATH);
                
            // Execute:
            DeleteUserVerification(result, usersToDelete);
        }

        #endregion Positive tests

        #region Negative tests

        #endregion Negative tests

        #region Private methods

        /// <summary>
        /// Verifies that all the specified users were deleted.
        /// </summary>
        /// <param name="resultSet">Result set from delete users call.</param>
        /// <param name="deletedUsers">List of users that were deleted.</param>
        public void DeleteUserVerification(DeleteUserResultSet resultSet, IEnumerable<IUser> deletedUsers)
        {
            ThrowIf.ArgumentNull(deletedUsers, nameof(deletedUsers));
            ThrowIf.ArgumentNull(resultSet, nameof(resultSet));

            Assert.AreEqual(HttpStatusCode.OK, resultSet.ReturnCode,
                "'{0}' should be 200 OK when all users were deleted!", nameof(resultSet.ReturnCode));

            foreach (var deletedUser in deletedUsers)
            {
                var result = resultSet.Results.Find(a => a.Username == deletedUser.Username);

                Assert.AreEqual("User successfully deleted.", result.Message, "'{0}' is incorrect!", nameof(result.Message));
                Assert.AreEqual(HttpStatusCode.OK, result.ReturnCode, "'{0}' should be 200 OK!", nameof(result.ReturnCode));

                // Try to get the deleted user and verify an error is returned.
                var ex = Assert.Throws<Http500InternalServerErrorException>(() => Helper.OpenApi.GetUser(_adminUser, deletedUser.Id),
                    "GetUser should return 500 Internal Server Error if the user was deleted!");

                string expectedErrorMessage =
                    I18NHelper.FormatInvariant("Message=DUser with Id: {0} was deleted by some other user. Please refresh.",
                        deletedUser.Id);

                TestHelper.ValidateServiceError(ex.RestResponse, expectedErrorMessage);
            }
        }

        #endregion Private methods
    }
}
