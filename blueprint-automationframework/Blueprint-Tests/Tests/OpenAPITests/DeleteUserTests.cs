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
using Utilities.Facades;

namespace OpenAPITests
{
    [Explicit(IgnoreReasons.UnderDevelopmentDev)]   // US4967
    [TestFixture]
    [Category(Categories.OpenApi)]
    public class DeleteUserTests : TestBase
    {
        private const string DELETE_PATH = RestPaths.OpenApi.Users.DELETE;

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
                "'DELETE {0}' should return '200 OK' when valid data is passed to it!", DELETE_PATH);
                
            // Verify:
            VerifyDeleteUserResultSet(result, usersToDelete);
        }

        [TestCase]
        [TestRail(246538)]
        [Description("Delete a list of users (some users are active and others are already deleted) and verify a 207 HTTP status was returned and " +
            "that the active users were deleted and the deleted users are reported as already deleted.")]
        public void DeleteUsers_ListOfActiveAndDeletedUsers_207PartialSuccess()
        {
            // Setup:
            const int numberOfUsersToDelete = 3;
            var usersToDelete = new List<IUser>();
            var activeUsersToDelete = new List<IUser>();

            for (int i = 0; i < numberOfUsersToDelete; ++i)
            {
                // Add an active user.
                var userToDelete = Helper.CreateUserAndAuthenticate(TestHelper.AuthenticationTokenTypes.BothAccessControlAndOpenApiTokens);
                usersToDelete.Add(userToDelete);
                activeUsersToDelete.Add(userToDelete);

                // Add a user and then delete it.
                userToDelete = Helper.CreateUserAndAuthenticate(TestHelper.AuthenticationTokenTypes.BothAccessControlAndOpenApiTokens);
                usersToDelete.Add(userToDelete);
                userToDelete.DeleteUser(useSqlUpdate: true);
            }

            var usernamesToDelete = usersToDelete.Select(u => u.Username).ToList();

            // Execute:
            DeleteUserResultSet result = null;

            Assert.DoesNotThrow(() => result = Helper.OpenApi.DeleteUser(_adminUser, usernamesToDelete, new List<HttpStatusCode> { (HttpStatusCode)207 }),
                "'DELETE {0}' should return '207 Partial Success' when valid data is passed to it!", DELETE_PATH);

            // Verify:
            VerifyDeleteUserResultSet(result, usersToDelete);
        }

        #endregion Positive tests

        #region Negative tests

        [TestCase]
        [TestRail(246539)]
        [Description("Delete a user and pass invalid parameters in the JSON body.  Verify it returns 400 Bad Request.")]
        public void DeleteUser_InvalidUserParameters_400BadRequest()
        {
            // Setup:
            var userToDelete = Helper.CreateUserAndAuthenticate(TestHelper.AuthenticationTokenTypes.BothAccessControlAndOpenApiTokens);
            var badData = new Dictionary<string, int> { { userToDelete.Username, userToDelete.Id } };

            // Execute:
            var ex = Assert.Throws<Http400BadRequestException>(() => DeleteUserWithInvalidBody(_adminUser, badData),
                "'DELETE {0}' should return '400 Bad Request' when invalid data is passed to it!", DELETE_PATH);

            // Verify:
            TestHelper.ValidateServiceError(ex.RestResponse, InternalApiErrorCodes.NotAcceptable, "TODO: Add real message here");
        }

        // TODO: Add 401 tests.
        // TODO: Add 403 tests.
        // TODO: Add 404 tests.
        // TODO: Add 409 tests.

        #endregion Negative tests

        #region Private methods

        /// <summary>
        /// Runs the OpenAPI Delete User call with invalid data in the body.
        /// </summary>
        /// <typeparam name="T">The data type to send in the body.  Valid type is a List of strings.</typeparam>
        /// <param name="userToAuthenticate">The user to authenticate with.</param>
        /// <param name="jsonBody">The data to send in the body.</param>
        /// <returns>A DeleteUserResultSet object if successful.</returns>
        private DeleteUserResultSet DeleteUserWithInvalidBody<T>(IUser userToAuthenticate, T jsonBody) where T : new()
        {
            var restApi = new RestApiFacade(Helper.OpenApi.Address, userToAuthenticate?.Token?.OpenApiToken);
            string path = RestPaths.OpenApi.Users.DELETE;

            return restApi.SendRequestAndDeserializeObject<DeleteUserResultSet, T>(
                path,
                RestRequestMethod.DELETE,
                jsonBody);
        }

        /// <summary>
        /// Verifies that all the specified users were deleted.
        /// </summary>
        /// <param name="resultSet">Result set from delete users call.</param>
        /// <param name="expectedSuccessfullyDeletedUsers">(optional) A list of users that we expect to be successfully deleted.</param>
        /// <param name="expectedFailedDeletedUsers">(optional) A map of Http Status Codes and Users that we expect got errors when we tried to delete them.</param>
        /// <param name="expectedStatusCode">(optional) The expected main ReturnCode.</param>
        private void VerifyDeleteUserResultSet(
            DeleteUserResultSet resultSet,
            List<IUser> expectedSuccessfullyDeletedUsers = null,
            Dictionary<HttpStatusCode, IUser> expectedFailedDeletedUsers = null,
            HttpStatusCode expectedStatusCode = HttpStatusCode.OK)
        {
            ThrowIf.ArgumentNull(resultSet, nameof(resultSet));

            expectedSuccessfullyDeletedUsers = expectedSuccessfullyDeletedUsers ?? new List<IUser>();
            expectedFailedDeletedUsers = expectedFailedDeletedUsers ?? new Dictionary<HttpStatusCode, IUser>();

            Assert.AreEqual(expectedStatusCode, resultSet.ReturnCode,
                "'{0}' should be '{1}'!", nameof(resultSet.ReturnCode), expectedStatusCode);

            foreach (var deletedUser in expectedSuccessfullyDeletedUsers)
            {
                var result = resultSet.Results.Find(a => a.Username == deletedUser.Username);

                Assert.AreEqual("User successfully deleted.", result.Message, "'{0}' is incorrect!", nameof(result.Message));
                Assert.AreEqual(HttpStatusCode.OK, result.ReturnCode, "'{0}' should be 200 OK!", nameof(result.ReturnCode));

                VerifyUserIsDeleted(deletedUser);
            }

            VerifyUnsuccessfullyDeletedUsers(resultSet, expectedFailedDeletedUsers);
        }

        /// <summary>
        /// A map of HttpStatusCode to error message for the DeleteUserResultSet.
        /// </summary>
        private Dictionary<HttpStatusCode, string> ReturnCodeToErrorMessageMap { get; } = new Dictionary<HttpStatusCode, string>
        {
            { HttpStatusCode.OK, "User successfully deleted." }
        };

        /// <summary>
        /// Verifies that the users that were NOT successfully deleted have the expected status codes.
        /// </summary>
        /// <param name="resultSet">Result set from delete users call.</param>
        /// <param name="expectedFailedDeletedUsers">(optional) A map of Http Status Codes and Users that we expect got errors when we tried to delete them.</param>
        private void VerifyUnsuccessfullyDeletedUsers(
            DeleteUserResultSet resultSet,
            Dictionary<HttpStatusCode, IUser> expectedFailedDeletedUsers = null)
        {
            ThrowIf.ArgumentNull(resultSet, nameof(resultSet));

            expectedFailedDeletedUsers = expectedFailedDeletedUsers ?? new Dictionary<HttpStatusCode, IUser>();

            foreach (var statusAndDeletedUser in expectedFailedDeletedUsers)
            {
                var returnCode = statusAndDeletedUser.Key;
                var deletedUser = statusAndDeletedUser.Value;
                var result = resultSet.Results.Find(a => a.Username == deletedUser.Username);

                Assert.AreEqual(ReturnCodeToErrorMessageMap[returnCode], result.Message, "'{0}' is incorrect!", nameof(result.Message));
                Assert.AreEqual(returnCode, result.ReturnCode, "'{0}' should be '{1}'!", nameof(result.ReturnCode), returnCode);

                VerifyUserIsDeleted(deletedUser);
            }
        }

        /// <summary>
        /// Tries to get the specified user and verifies that GetUser doesn't find the user because it was deleted.
        /// </summary>
        /// <param name="deletedUser">The user that was deleted.</param>
        private void VerifyUserIsDeleted(IUser deletedUser)
        {
            // Try to get the deleted user and verify an error is returned.
            var ex = Assert.Throws<Http500InternalServerErrorException>(() => Helper.OpenApi.GetUser(_adminUser, deletedUser.Id),
                "GetUser should return 500 Internal Server Error if the user was deleted!");

            string expectedErrorMessage =
                I18NHelper.FormatInvariant("Message=DUser with Id: {0} was deleted by some other user. Please refresh.",
                    deletedUser.Id);

            TestHelper.ValidateServiceError(ex.RestResponse, expectedErrorMessage);
        }

        #endregion Private methods
    }
}
