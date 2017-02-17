using CustomAttributes;
using Helper;
using Model;
using NUnit.Framework;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using Common;
using Model.Factories;
using Model.OpenApiModel.UserModel;
using Newtonsoft.Json;
using TestCommon;
using Utilities;
using Utilities.Facades;
using Utilities.Factories;

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
                var userToDelete = Helper.CreateUserAndAddToDatabase();
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
                var userToDelete = Helper.CreateUserAndAddToDatabase();
                usersToDelete.Add(userToDelete);
                activeUsersToDelete.Add(userToDelete);

                // Add a user and then delete it.
                userToDelete = Helper.CreateUserAndAddToDatabase();
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

        #region 400 tests

        [TestCase]
        [TestRail(246539)]
        [Description("Delete a user and pass invalid parameters in the JSON body.  Verify it returns 400 Bad Request.")]
        public void DeleteUser_InvalidUserParameters_400BadRequest()
        {
            // Setup:
            var userToDelete = Helper.CreateUserAndAddToDatabase();
            var badData = new Dictionary<string, int> { { userToDelete.Username, userToDelete.Id } };

            // Execute:
            var ex = Assert.Throws<Http400BadRequestException>(() => DeleteUserWithInvalidBody(_adminUser, badData),
                "'DELETE {0}' should return '400 Bad Request' when invalid data is passed to it!", DELETE_PATH);

            // Verify:
            TestHelper.ValidateServiceError(ex.RestResponse, InternalApiErrorCodes.NotAcceptable, "TODO: Add real message here");
        }

        #endregion 400 tests

        #region 401 tests

        [TestCase(null)]
        [TestCase(CommonConstants.InvalidToken)]
        [TestRail(246543)]
        [Description("Delete a user and pass invalid or missing token.  Verify it returns 401 Unauthorized.")]
        public void DeleteUser_InvalidToken_401Unauthorized(string invalidToken)
        {
            // Setup:
            _adminUser.Token.OpenApiToken = invalidToken;

            var usernamesToDelete = new List<string> { _adminUser.Username };

            // Execute:
            DeleteUserResultSet result = null;

            var ex = Assert.Throws<Http401UnauthorizedException>(() => result = Helper.OpenApi.DeleteUser(_adminUser, usernamesToDelete),
                "'DELETE {0}' should return '401 Unauthorized' when an invalid or missing token is passed to it!", DELETE_PATH);

            // Verify:
            TestHelper.ValidateServiceError(ex.RestResponse, ErrorCodes.UnauthorizedAccess, "TODO: Add real message here");
        }

        #endregion 401 tests

        #region 403 tests

        [TestCase]
        [TestRail(246544)]
        [Description("Delete a user and pass a token for a user without permission to delete users.  Verify it returns 403 Forbidden.")]
        public void DeleteUser_InsufficientPermissions_403Forbidden()
        {
            // Setup:
            // Create an Author user.  Authors shouldn't have permission to delete other users.
            var project = ProjectFactory.GetProject(_adminUser, shouldRetrieveArtifactTypes: false);

            var authorUser = Helper.CreateUserWithProjectRolePermissions(TestHelper.ProjectRole.AuthorFullAccess, project);
            var usernamesToDelete = new List<string> { authorUser.Username };

            // Execute:
            var ex = Assert.Throws<Http403ForbiddenException>(() => Helper.OpenApi.DeleteUser(authorUser, usernamesToDelete),
                "'DELETE {0}' should return '403 Forbidden' when called by a user without permission to delete users!", DELETE_PATH);

            // Verify:
            TestHelper.ValidateServiceError(ex.RestResponse, InternalApiErrorCodes.Forbidden, "TODO: Add real message here");
        }

        #endregion 403 tests

        #region 409 tests

        [TestCase]
        [TestRail(246545)]
        [Description("Delete a user that was already deleted.  Verify it returns 409 Conflict.")]
        public void DeleteUser_DeletedUser_409Conflict()
        {
            // Setup:
            var userToDelete = Helper.CreateUserAndAddToDatabase();
            var usernamesToDelete = new List<string> { userToDelete.Username };

            userToDelete.DeleteUser(useSqlUpdate: true);

            // Execute:
            var ex = Assert.Throws<Http409ConflictException>(() => Helper.OpenApi.DeleteUser(_adminUser, usernamesToDelete),
                "'DELETE {0}' should return '409 Conflict' when passed a username that doesn't exist!", DELETE_PATH);

            // Verify:
            TestHelper.ValidateServiceError(ex.RestResponse, InternalApiErrorCodes.NotFound, "TODO: Add real message here");

            var expectedFailedDeletedUsers = new Dictionary<int, IUser> { { InternalApiErrorCodes.NotFound, userToDelete } };

            var result = JsonConvert.DeserializeObject<DeleteUserResultSet>(ex.RestResponse.Content);
            VerifyDeleteUserResultSet(result, expectedFailedDeletedUsers: expectedFailedDeletedUsers, expectedStatusCode: HttpStatusCode.Conflict);
        }

        [TestCase]
        [TestRail(246546)]
        [Description("Delete a user that doesn't exist.  Verify it returns 409 Conflict.")]
        public void DeleteUser_NonExistingUsername_409Conflict()
        {
            // Setup:
            var userToDelete = UserFactory.CreateUserOnly();
            var usernamesToDelete = new List<string> { userToDelete.Username };

            // Execute:
            var ex = Assert.Throws<Http409ConflictException>(() => Helper.OpenApi.DeleteUser(_adminUser, usernamesToDelete),
                "'DELETE {0}' should return '409 Conflict' when passed a username that doesn't exist!", DELETE_PATH);

            // Verify:
            TestHelper.ValidateServiceError(ex.RestResponse, InternalApiErrorCodes.NotFound, "TODO: Add real message here");

            var expectedFailedDeletedUsers = new Dictionary<int, IUser> { { InternalApiErrorCodes.NotFound, userToDelete } };

            var result = JsonConvert.DeserializeObject<DeleteUserResultSet>(ex.RestResponse.Content);
            VerifyDeleteUserResultSet(result, expectedFailedDeletedUsers: expectedFailedDeletedUsers, expectedStatusCode: HttpStatusCode.Conflict);
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
        /// <param name="expectedFailedDeletedUsers">(optional) A map of InternalApiErrorCodes and Users that we expect got errors when we tried to delete them.</param>
        /// <param name="expectedStatusCode">(optional) The expected main ReturnCode.</param>
        private void VerifyDeleteUserResultSet(
            DeleteUserResultSet resultSet,
            List<IUser> expectedSuccessfullyDeletedUsers = null,
            Dictionary<int, IUser> expectedFailedDeletedUsers = null,
            HttpStatusCode expectedStatusCode = HttpStatusCode.OK)
        {
            ThrowIf.ArgumentNull(resultSet, nameof(resultSet));

            expectedSuccessfullyDeletedUsers = expectedSuccessfullyDeletedUsers ?? new List<IUser>();
            expectedFailedDeletedUsers = expectedFailedDeletedUsers ?? new Dictionary<int, IUser>();

            Assert.AreEqual(expectedStatusCode, resultSet.ReturnCode,
                "'{0}' should be '{1}'!", nameof(resultSet.ReturnCode), expectedStatusCode);

            foreach (var deletedUser in expectedSuccessfullyDeletedUsers)
            {
                var result = resultSet.Results.Find(a => a.User.Username == deletedUser.Username);

                Assert.AreEqual("User successfully deleted.", result.Message, "'{0}' is incorrect!", nameof(result.Message));
                Assert.AreEqual(InternalApiErrorCodes.Ok, result.ResultCode, "'{0}' should be 200 OK!", nameof(result.ResultCode));

                VerifyUserIsDeleted(deletedUser);
            }

            VerifyUnsuccessfullyDeletedUsers(resultSet, expectedFailedDeletedUsers);
        }

        /// <summary>
        /// A map of InternalApiErrorCodes to error message for the DeleteUserResultSet.
        /// </summary>
        private Dictionary<int, string> ErrorCodeToErrorMessageMap { get; } = new Dictionary<int, string>
        {
            { InternalApiErrorCodes.Ok, "User successfully deleted." }
        };

        /// <summary>
        /// Verifies that the users that were NOT successfully deleted have the expected status codes.
        /// </summary>
        /// <param name="resultSet">Result set from delete users call.</param>
        /// <param name="expectedFailedDeletedUsers">(optional) A map of InternalApiErrorCodes and Users that we expect got errors when we tried to delete them.</param>
        private void VerifyUnsuccessfullyDeletedUsers(
            DeleteUserResultSet resultSet,
            Dictionary<int, IUser> expectedFailedDeletedUsers = null)
        {
            ThrowIf.ArgumentNull(resultSet, nameof(resultSet));

            expectedFailedDeletedUsers = expectedFailedDeletedUsers ?? new Dictionary<int, IUser>();

            foreach (var statusAndDeletedUser in expectedFailedDeletedUsers)
            {
                var returnCode = statusAndDeletedUser.Key;
                var deletedUser = statusAndDeletedUser.Value;
                var result = resultSet.Results.Find(a => a.User.Username == deletedUser.Username);

                Assert.AreEqual(ErrorCodeToErrorMessageMap[returnCode], result.Message, "'{0}' is incorrect!", nameof(result.Message));
                Assert.AreEqual(returnCode, result.ResultCode, "'{0}' should be '{1}'!", nameof(result.ResultCode), returnCode);

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
