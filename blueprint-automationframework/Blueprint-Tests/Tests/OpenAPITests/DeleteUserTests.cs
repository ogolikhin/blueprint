using CustomAttributes;
using Helper;
using Model;
using NUnit.Framework;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using Common;
using Model.Common.Constants;
using Model.Factories;
using Model.OpenApiModel.UserModel.Results;
using Newtonsoft.Json;
using TestCommon;
using Utilities;
using Utilities.Facades;

namespace OpenAPITests
{
    [TestFixture]
    [Category(Categories.OpenApi)]
    public class DeleteUserTests : TestBase
    {
        private const string DELETE_PATH = RestPaths.OpenApi.USERS;

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
        public void DeleteUsers_ValidUserParameters_VerifyUserDeleted(int numberOfUsersToDelete)
        {
            // Setup:
            var usersToDelete = new List<IUser>();

            for (int i = 0; i < numberOfUsersToDelete; ++i)
            {
                var userToDelete = Helper.CreateUserAndAddToDatabase();
                usersToDelete.Add(userToDelete);
            }

            var usernamesToDelete = usersToDelete.Select(u => u.Username).ToList();

            // Execute:
            UserCallResultCollection result = null;

            Assert.DoesNotThrow(() => result = Helper.OpenApi.DeleteUsers(_adminUser, usernamesToDelete), 
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
            var deletedUsersToDelete = new Dictionary<IUser, int>();

            for (int i = 0; i < numberOfUsersToDelete; ++i)
            {
                // Add an active user.
                var userToDelete = Helper.CreateUserAndAddToDatabase();
                usersToDelete.Add(userToDelete);
                activeUsersToDelete.Add(userToDelete);

                // Add a user and then delete it.
                userToDelete = Helper.CreateUserAndAddToDatabase();
                usersToDelete.Add(userToDelete);
                deletedUsersToDelete.Add(userToDelete, BusinessLayerErrorCodes.LoginDoesNotExist);
                userToDelete.DeleteUser(useSqlUpdate: true);
            }

            var usernamesToDelete = usersToDelete.Select(u => u.Username).ToList();

            // Execute:
            UserCallResultCollection result = null;

            Assert.DoesNotThrow(() => result = Helper.OpenApi.DeleteUsers(_adminUser, usernamesToDelete, new List<HttpStatusCode> { (HttpStatusCode)207 }),
                "'DELETE {0}' should return '207 Partial Success' when valid data is passed to it!", DELETE_PATH);

            // Verify:
            VerifyDeleteUserResultSet(result,
                expectedSuccessfullyDeletedUsers: activeUsersToDelete,
                expectedFailedDeletedUsers: deletedUsersToDelete);
        }

        #endregion Positive tests

        #region 400 tests

        private const string REQUEST_IS_MISSING_OR_INVALID_MESSAGE = "The request parameter is missing or invalid";

        [TestCase]
        [TestRail(246539)]
        [Description("Call the Delete Users API with an invalid JSON body.  Verify it returns 400 Bad Request.")]
        public void DeleteUsers_InvalidJsonBody_400BadRequest()
        {
            // Setup:
            var userToDelete = Helper.CreateUserAndAddToDatabase();

            // DeleteUsers REST call expects a List of strings.  Try passing a Dictionary instead.
            var badData = new Dictionary<string, int> { { userToDelete.Username, userToDelete.Id } };

            // Execute:
            var ex = Assert.Throws<Http400BadRequestException>(() => DeleteUserWithInvalidBody(_adminUser, badData),
                "'DELETE {0}' should return '400 Bad Request' when invalid data is passed to it!", DELETE_PATH);

            // Verify:
            TestHelper.ValidateServiceErrorMessage(ex.RestResponse, REQUEST_IS_MISSING_OR_INVALID_MESSAGE);
        }

        [TestCase]
        [TestRail(246642)]
        [Description("Call the Delete Users API with an empty request body.  Verify it returns 400 Bad Request.")]
        public void DeleteUsers_EmptyBody_400BadRequest()
        {
            // Setup:
            List<string> nullListOfUsers = null;

            // Execute:
            var ex = Assert.Throws<Http400BadRequestException>(() => DeleteUserWithInvalidBody(_adminUser, nullListOfUsers),
                "'DELETE {0}' should return '400 Bad Request' when invalid data is passed to it!", DELETE_PATH);

            // Verify:
            TestHelper.ValidateServiceErrorMessage(ex.RestResponse, REQUEST_IS_MISSING_OR_INVALID_MESSAGE);
        }

        [TestCase]
        [TestRail(246643)]
        [Description("Call the Delete Users API with an empty list in the JSON body.  Verify it returns 400 Bad Request.")]
        public void DeleteUsers_EmptyListInBody_400BadRequest()
        {
            // Setup:
            var emptyListOfUsers = new List<string>();

            // Execute:
            var ex = Assert.Throws<Http400BadRequestException>(() => Helper.OpenApi.DeleteUsers(_adminUser, emptyListOfUsers),
                "'DELETE {0}' should return '400 Bad Request' when invalid data is passed to it!", DELETE_PATH);

            // Verify:
            TestHelper.ValidateServiceErrorMessage(ex.RestResponse, REQUEST_IS_MISSING_OR_INVALID_MESSAGE);
        }

        #endregion 400 tests

        #region 401 tests

        [TestCase(null)]
        [TestCase(CommonConstants.InvalidToken)]
        [TestRail(246543)]
        [Description("Delete a user and pass invalid or missing token.  Verify it returns 401 Unauthorized.")]
        public void DeleteUsers_InvalidToken_401Unauthorized(string invalidToken)
        {
            // Setup:
            _adminUser.Token.OpenApiToken = invalidToken;

            var usernamesToDelete = new List<string> { _adminUser.Username };

            // Execute:
            var ex = Assert.Throws<Http401UnauthorizedException>(() => Helper.OpenApi.DeleteUsers(_adminUser, usernamesToDelete),
                "'DELETE {0}' should return '401 Unauthorized' when an invalid or missing token is passed to it!", DELETE_PATH);

            // Verify:
            TestHelper.ValidateServiceError(ex.RestResponse, "Unauthorized call.");
        }

        #endregion 401 tests

        #region 403 tests

        [TestCase]
        [TestRail(246544)]
        [Description("Delete a user and pass a token for a user without permission to delete users.  Verify it returns 403 Forbidden.")]
        public void DeleteUsers_InsufficientPermissions_403Forbidden()
        {
            // Setup:
            // Create an Author user.  Authors shouldn't have permission to delete other users.
            var project = ProjectFactory.GetProject(_adminUser, shouldRetrieveArtifactTypes: false);

            var authorUser = Helper.CreateUserWithProjectRolePermissions(TestHelper.ProjectRole.AuthorFullAccess, project);
            var usernamesToDelete = new List<string> { authorUser.Username };

            // Execute:
            var ex = Assert.Throws<Http403ForbiddenException>(() => Helper.OpenApi.DeleteUsers(authorUser, usernamesToDelete),
                "'DELETE {0}' should return '403 Forbidden' when called by a user without permission to delete users!", DELETE_PATH);

            // Verify:
            TestHelper.ValidateServiceErrorMessage(ex.RestResponse, "The user does not have the privileges required to perform the action.");
        }

        #endregion 403 tests

        #region 409 tests

        [TestCase]
        [TestRail(246545)]
        [Description("Delete a user that was already deleted.  Verify it returns 409 Conflict.")]
        public void DeleteUsers_DeletedUser_409Conflict()
        {
            // Setup:
            var userToDelete = Helper.CreateUserAndAddToDatabase();
            var usernamesToDelete = new List<string> { userToDelete.Username };

            userToDelete.DeleteUser(useSqlUpdate: true);

            // Execute:
            var ex = Assert.Throws<Http409ConflictException>(() => Helper.OpenApi.DeleteUsers(_adminUser, usernamesToDelete),
                "'DELETE {0}' should return '409 Conflict' when passed a username that was deleted!", DELETE_PATH);

            // Verify:
            var expectedFailedDeletedUsers = new Dictionary<IUser, int> { { userToDelete, BusinessLayerErrorCodes.LoginDoesNotExist } };

            var result = JsonConvert.DeserializeObject<UserCallResultCollection>(ex.RestResponse.Content);
            VerifyDeleteUserResultSet(result, expectedFailedDeletedUsers: expectedFailedDeletedUsers);
        }

        [TestCase]
        [TestRail(246546)]
        [Description("Delete a user that doesn't exist.  Verify it returns 409 Conflict.")]
        public void DeleteUsers_NonExistingUsername_409Conflict()
        {
            // Setup:
            var userToDelete = UserFactory.CreateUserOnly();
            var usernamesToDelete = new List<string> { userToDelete.Username };

            // Execute:
            var ex = Assert.Throws<Http409ConflictException>(() => Helper.OpenApi.DeleteUsers(_adminUser, usernamesToDelete),
                "'DELETE {0}' should return '409 Conflict' when passed a username that doesn't exist!", DELETE_PATH);

            // Verify:
            var expectedFailedDeletedUsers = new Dictionary<IUser, int> { { userToDelete, BusinessLayerErrorCodes.LoginDoesNotExist } };

            var result = JsonConvert.DeserializeObject<UserCallResultCollection>(ex.RestResponse.Content);
            VerifyDeleteUserResultSet(result, expectedFailedDeletedUsers: expectedFailedDeletedUsers, checkIfUserExists: false);
        }

        #endregion 409 tests

        #region Private methods

        /// <summary>
        /// Runs the OpenAPI Delete User call with invalid data in the body.
        /// </summary>
        /// <typeparam name="T">The data type to send in the body.  Valid type is a List of strings.</typeparam>
        /// <param name="userToAuthenticate">The user to authenticate with.</param>
        /// <param name="jsonBody">The data to send in the body.</param>
        /// <returns>A DeleteUserResultSet object if successful.</returns>
        private UserCallResultCollection DeleteUserWithInvalidBody<T>(IUser userToAuthenticate, T jsonBody) where T : new()
        {
            var restApi = new RestApiFacade(Helper.OpenApi.Address, userToAuthenticate?.Token?.OpenApiToken);
            string path = DELETE_PATH;

            return restApi.SendRequestAndDeserializeObject<UserCallResultCollection, T>(
                path,
                RestRequestMethod.DELETE,
                jsonBody);
        }

        /// <summary>
        /// Verifies that all the specified users were deleted.
        /// </summary>
        /// <param name="resultSet">Result set from delete users call.</param>
        /// <param name="expectedSuccessfullyDeletedUsers">(optional) A list of users that we expect to be successfully deleted.</param>
        /// <param name="expectedFailedDeletedUsers">(optional) A map of Users to InternalApiErrorCodes that we expect got errors when we tried to delete them.</param>
        /// <param name="checkIfUserExists">(optional) Pass false if you don't want to check if the user exists.</param>
        private void VerifyDeleteUserResultSet(
            UserCallResultCollection resultSet,
            List<IUser> expectedSuccessfullyDeletedUsers = null,
            Dictionary<IUser, int> expectedFailedDeletedUsers = null,
            bool checkIfUserExists = true)
        {
            ThrowIf.ArgumentNull(resultSet, nameof(resultSet));

            expectedSuccessfullyDeletedUsers = expectedSuccessfullyDeletedUsers ?? new List<IUser>();
            expectedFailedDeletedUsers = expectedFailedDeletedUsers ?? new Dictionary<IUser, int>();

            int expectedTotalUserCount = expectedSuccessfullyDeletedUsers.Count + expectedFailedDeletedUsers.Count;
            Assert.AreEqual(expectedTotalUserCount, resultSet.Count, "Wrong number of User results were returned!");

            foreach (var deletedUser in expectedSuccessfullyDeletedUsers)
            {
                var result = resultSet.Find(a => a.User.Username == deletedUser.Username);

                Assert.AreEqual(ErrorCodeToErrorMessageMap[InternalApiErrorCodes.Ok], result.Message, "'{0}' is incorrect!", nameof(result.Message));
                Assert.AreEqual(InternalApiErrorCodes.Ok, result.ResultCode, "'{0}' should be 200 OK!", nameof(result.ResultCode));

                if (checkIfUserExists)
                {
                    VerifyUserIsDeleted(deletedUser);
                }
            }

            VerifyUnsuccessfullyDeletedUsers(resultSet, expectedFailedDeletedUsers, checkIfUserExists);
        }

        /// <summary>
        /// A map of InternalApiErrorCodes to error message for the DeleteUserResultSet.
        /// </summary>
        private Dictionary<int, string> ErrorCodeToErrorMessageMap { get; } = new Dictionary<int, string>
        {
            { InternalApiErrorCodes.Ok, "User has been successfully deleted." },
            { BusinessLayerErrorCodes.LoginDoesNotExist, "User with User Name={0} does not exist" }
        };

        /// <summary>
        /// Verifies that the users that were NOT successfully deleted have the expected status codes.
        /// </summary>
        /// <param name="resultSet">Result set from delete users call.</param>
        /// <param name="expectedFailedDeletedUsers">(optional) A map of Users to InternalApiErrorCodes that we expect got errors when we tried to delete them.</param>
        /// <param name="checkIfUserExists">(optional) Pass false if you don't want to check if the user exists.</param>
        private void VerifyUnsuccessfullyDeletedUsers(
            UserCallResultCollection resultSet,
            Dictionary<IUser, int> expectedFailedDeletedUsers = null,
            bool checkIfUserExists = true)
        {
            ThrowIf.ArgumentNull(resultSet, nameof(resultSet));

            expectedFailedDeletedUsers = expectedFailedDeletedUsers ?? new Dictionary<IUser, int>();

            foreach (var deletedUserAndStatus in expectedFailedDeletedUsers)
            {
                var deletedUser = deletedUserAndStatus.Key;
                var returnCode = deletedUserAndStatus.Value;
                var result = resultSet.Find(a => a.User.Username == deletedUser.Username);
                string expectedErrorMessage = I18NHelper.FormatInvariant(ErrorCodeToErrorMessageMap[returnCode],
                    deletedUser.Username);

                Assert.AreEqual(expectedErrorMessage, result.Message, "'{0}' is incorrect!", nameof(result.Message));
                Assert.AreEqual(returnCode, result.ResultCode, "'{0}' should be '{1}'!", nameof(result.ResultCode), returnCode);

                if (checkIfUserExists)
                {
                    VerifyUserIsDeleted(deletedUser);
                }
            }
        }

        /// <summary>
        /// Tries to get the specified user and verifies that GetUser doesn't find the user because it was deleted.
        /// </summary>
        /// <param name="deletedUser">The user that was deleted.</param>
        private void VerifyUserIsDeleted(IUser deletedUser)
        {
            // Try to get the deleted user and verify an error is returned.
            var ex = Assert.Throws<Http404NotFoundException>(() => Helper.OpenApi.GetUser(_adminUser, deletedUser.Id),
                "GetUser should return 404 Not Found if the user was deleted!");

            const string expectedErrorMessage = "The requested user is not found.";

            TestHelper.ValidateServiceErrorMessage(ex.RestResponse, expectedErrorMessage);
        }

        #endregion Private methods
    }
}
