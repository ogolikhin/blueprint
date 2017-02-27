using System.Collections.Generic;
using System.Linq;
using System.Net;
using Common;
using CustomAttributes;
using Helper;
using Model;
using Model.Common.Constants;
using Model.Factories;
using Model.Impl;
using Model.OpenApiModel.UserModel.Results;
using NUnit.Framework;
using TestCommon;
using Utilities;
using Utilities.Factories;

namespace OpenAPITests
{
    [TestFixture]
    [Category(Categories.OpenApi)]
    public class UpdateUserTests : TestBase
    {
        private const string UPDATE_PATH = RestPaths.OpenApi.USERS;

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

        [Explicit(IgnoreReasons.ProductBug)]    // Trello: https://trello.com/c/XvPTyExu  GetUser doesn't return all user properties.
        [TestCase(1, nameof(UserDataModel.Department))]
        [TestCase(5, nameof(UserDataModel.DisplayName))]
        [TestRail(246613)]
        [Description("Update some properties of one or more users and verify that the users were updated.")]
        public void UpdateUser_ValidUserParameters_VerifyUserUpdated(int numberOfUsersToUpdate, string propertyToUpdate)
        {
            // Setup:
            var usersToUpdate = new List<IUser>();

            for (int i = 0; i < numberOfUsersToUpdate; ++i)
            {
                var userToUpdate = Helper.CreateUserAndAddToDatabase();
                usersToUpdate.Add(userToUpdate);
            }

            var propertiesToUpdate = new Dictionary<string, string>
            {
                { propertyToUpdate, RandomGenerator.RandomAlphaNumericUpperAndLowerCase(10) }
            };

            var usernamesToUpdate = usersToUpdate.Select(u => u.Username).ToList();
            var userDataToUpdate = CreateUserDataModelsForUpdate(usernamesToUpdate, propertiesToUpdate);

            // Execute:
            UserCallResultCollection result = null;

            Assert.DoesNotThrow(() => result = Helper.OpenApi.UpdateUsers(_adminUser, userDataToUpdate),
                "'PATCH {0}' should return '200 OK' when valid data is passed to it!", UPDATE_PATH);

            // Verify:
            VerifyUpdateUserResultSet(result, usersToUpdate, expectedSuccessfullyUpdatedUsers: userDataToUpdate);
        }

        [Explicit(IgnoreReasons.ProductBug)]    // Trello: https://trello.com/c/XvPTyExu  GetUser doesn't return all user properties.
        [TestCase]
        [TestRail(246614)]
        [Description("Update a list of users (some users are active and others are deleted) and verify a 207 HTTP status was returned and " +
                     "that the active users were updated and the deleted users are reported as deleted.")]
        public void UpdateUsers_ListOfActiveAndDeletedUsers_207PartialSuccess()
        {
            // Setup:
            var activeUsersToUpdate = new List<IUser> { Helper.CreateUserAndAddToDatabase() };
            var deletedUsersToUpdate = new List<IUser> { Helper.CreateUserAndAddToDatabase() };
            deletedUsersToUpdate[0].DeleteUser(useSqlUpdate: true);

            var propertiesToUpdate = new Dictionary<string, string>
            {
                { nameof(UserDataModel.DisplayName), RandomGenerator.RandomAlphaNumericUpperAndLowerCase(10) },
                { nameof(UserDataModel.Department), RandomGenerator.RandomAlphaNumericUpperAndLowerCase(10) }
            };

            var usernamesToUpdate = activeUsersToUpdate.Select(u => u.Username).ToList();
            var activeUserDataToUpdate = CreateUserDataModelsForUpdate(usernamesToUpdate, propertiesToUpdate);

            usernamesToUpdate = deletedUsersToUpdate.Select(u => u.Username).ToList();
            var deletedUserDataToUpdate = CreateUserDataModelsForUpdate(usernamesToUpdate, propertiesToUpdate);

            var allUserDataToUpdate = new List<UserDataModel>(activeUserDataToUpdate);
            allUserDataToUpdate.AddRange(deletedUserDataToUpdate);

            // Execute:
            UserCallResultCollection result = null;

            Assert.DoesNotThrow(() => result = Helper.OpenApi.UpdateUsers(_adminUser, allUserDataToUpdate, new List<HttpStatusCode> { (HttpStatusCode)207 }),
                "'PATCH {0}' should return '207 Partial Success' when some users were updated and others weren't!", UPDATE_PATH);

            // Verify:
            var deletedUserDataAndErrorCode = new Dictionary<IUser, int> { { deletedUsersToUpdate[0], BusinessLayerErrorCodes.LoginDoesNotExist } };
            var allUsersToUpdate = new List<IUser>(activeUsersToUpdate);
            allUsersToUpdate.AddRange(deletedUsersToUpdate);

            VerifyUpdateUserResultSet(result, allUsersToUpdate,
                expectedSuccessfullyUpdatedUsers: activeUserDataToUpdate,
                expectedFailedUpdatedUsers: deletedUserDataAndErrorCode);
        }

        [Explicit(IgnoreReasons.ProductBug)]    // Trello:  https://trello.com/c/G62ah53O  Returns 200 instead of 207.
        [TestCase]
        [TestRail(266425)]
        [Description("Update a list of users and change the Type of some users to 'Group' and verify a 207 HTTP status was returned and that the users " +
                     "with Type 'User' were updated and the ones with Type 'Group' failed to be updated.")]
        public void UpdateUsers_ListOfUsersAndChangeSomeToGroups_207PartialSuccess()
        {
            // Setup:
            var usersToUpdate = new List<IUser> { Helper.CreateUserAndAddToDatabase() };
            var usersToUpdateToGroups = new List<IUser> { Helper.CreateUserAndAddToDatabase() };

            var propertiesToUpdate = new Dictionary<string, string>
            {
                { nameof(UserDataModel.DisplayName), RandomGenerator.RandomAlphaNumericUpperAndLowerCase(10) },
                { nameof(UserDataModel.Department), RandomGenerator.RandomAlphaNumericUpperAndLowerCase(10) }
            };

            var usernamesToUpdate = usersToUpdate.Select(u => u.Username).ToList();
            var activeUserDataToUpdate = CreateUserDataModelsForUpdate(usernamesToUpdate, propertiesToUpdate);

            // Change the Type of some users to "Group", which should fail.
            propertiesToUpdate.Add(nameof(UserDataModel.UserOrGroupType), "Group");

            usernamesToUpdate = usersToUpdateToGroups.Select(u => u.Username).ToList();
            var deletedUserDataToUpdate = CreateUserDataModelsForUpdate(usernamesToUpdate, propertiesToUpdate);

            var allUserDataToUpdate = new List<UserDataModel>(activeUserDataToUpdate);
            allUserDataToUpdate.AddRange(deletedUserDataToUpdate);

            // Execute:
            UserCallResultCollection result = null;

            Assert.DoesNotThrow(() => result = Helper.OpenApi.UpdateUsers(_adminUser, allUserDataToUpdate, new List<HttpStatusCode> { (HttpStatusCode)207 }),
                "'PATCH {0}' should return '207 Partial Success' when some users were updated and others weren't!", UPDATE_PATH);

            // Verify:
            var allUsersToUpdate = new List<IUser>(usersToUpdate);
            allUsersToUpdate.AddRange(usersToUpdateToGroups);

            VerifyUpdateUserResultSet(result, allUsersToUpdate);   // TODO: Add expectedSuccessfullyUpdatedUsers & expectedFailedUpdatedUsers.
        }

        #endregion Positive tests

        #region Private functions

        /// <summary>
        /// Creates a list of new UserDataModels with only the properties we want to update set to a value.
        /// </summary>
        /// <param name="usernames">The usernames of the users to update.</param>
        /// <param name="userPropertiesToUpdate">A map of user property names and values to update.</param>
        /// <returns>The list of new UserDataModels to send to the UpdateUsers REST call.</returns>
        private static List<UserDataModel> CreateUserDataModelsForUpdate(
            List<string> usernames,
            Dictionary<string, string> userPropertiesToUpdate)
        {
            List<UserDataModel> userDataModels = new List<UserDataModel>();

            foreach (string username in usernames)
            {
                var userData = CreateUserDataModelForUpdate(username, userPropertiesToUpdate);
                userDataModels.Add(userData);
            }

            return userDataModels;
        }

        /// <summary>
        /// Creates a new UserDataModel with only the properties we want to update set to a value.
        /// </summary>
        /// <param name="username">The username of the user to update.</param>
        /// <param name="userPropertiesToUpdate">A map of user property names and values to update.</param>
        /// <returns>The new UserDataModel to send to the UpdateUsers REST call.</returns>
        private static UserDataModel CreateUserDataModelForUpdate(
            string username,
            Dictionary<string, string> userPropertiesToUpdate)
        {
            var newUserData = UserDataModelFactory.CreateUserDataModel(username);

            foreach (var propertyNameValue in userPropertiesToUpdate)
            {
                CSharpUtilities.SetProperty(propertyNameValue.Key, propertyNameValue.Value, newUserData);
            }

            return newUserData;
        }

        /// <summary>
        /// Returns a new UserDataModel with all properties of the baseUserDataModel but with non-null properties from updateUserDataModel
        /// overwriting the baseUserDataModel values.  i.e. This is how the user should look after it is updated.
        /// </summary>
        /// <param name="baseUserDataModel">The base UserDataModel to get most of the properties from.</param>
        /// <param name="updateUserDataModel">The UserDataModel used to update the user.  All non-null properties will be copied into the
        ///     UserDataModel that is returned.</param>
        /// <returns>A UserDataModel with properties from baseUserDataModel and updateUserDataModel merged together.</returns>
        private static UserDataModel MergeNewAndOldUserData(UserDataModel baseUserDataModel, UserDataModel updateUserDataModel)
        {
            var newUserData = UserDataModelFactory.CreateCopyOfUserDataModel(baseUserDataModel);

            foreach (var property in updateUserDataModel.GetType().GetProperties())
            {
                if ((property.DeclaringType == typeof (UserDataModel)) && (property.GetValue(updateUserDataModel) != null))
                {
                    CSharpUtilities.SetProperty(property.Name, property.GetValue(updateUserDataModel), newUserData);
                }
            }

            return newUserData;
        }

        /// <summary>
        /// Verifies that all the specified users were updated properly.
        /// </summary>
        /// <param name="resultSet">Result set from update users call.</param>
        /// <param name="usersBeforeUpdate">The original users before they were updated.</param>
        /// <param name="expectedSuccessfullyUpdatedUsers">(optional) A list of users that we expect to be successfully updated.</param>
        /// <param name="expectedFailedUpdatedUsers">(optional) A map of Users and InternalApiErrorCodes that we expect got errors when we tried to update them.</param>
        private void VerifyUpdateUserResultSet(
            UserCallResultCollection resultSet,
            List<IUser> usersBeforeUpdate,
            List<UserDataModel> expectedSuccessfullyUpdatedUsers = null,
            Dictionary<IUser, int> expectedFailedUpdatedUsers = null)
        {
            ThrowIf.ArgumentNull(resultSet, nameof(resultSet));

            expectedSuccessfullyUpdatedUsers = expectedSuccessfullyUpdatedUsers ?? new List<UserDataModel>();
            expectedFailedUpdatedUsers = expectedFailedUpdatedUsers ?? new Dictionary<IUser, int>();

            int expectedUpdateCount = expectedSuccessfullyUpdatedUsers.Count + expectedFailedUpdatedUsers.Count;

            Assert.AreEqual(expectedUpdateCount, resultSet.Count,
                "The number of users returned by the update call is different than expected!");

//            Assert.AreEqual((int)expectedStatusCode, resultSet.ResultCode,
//                "'{0}' should be '{1}'!", nameof(resultSet.ResultCode), expectedStatusCode);

            VerifySuccessfullyUpdatedUsers(resultSet, expectedSuccessfullyUpdatedUsers, usersBeforeUpdate);
            VerifyUnsuccessfullyUpdatedUsers(resultSet, expectedFailedUpdatedUsers);
        }

        /// <summary>
        /// Verifies that all the specified users were updated properly.
        /// </summary>
        /// <param name="resultSet">Result set from update users call.</param>
        /// <param name="expectedSuccessfullyUpdatedUsers">A list of users that we expect to be successfully updated with only the updated properties set.</param>
        /// <param name="originalUsers">The original users before they were updated.</param>
        private void VerifySuccessfullyUpdatedUsers(
            UserCallResultCollection resultSet,
            List<UserDataModel> expectedSuccessfullyUpdatedUsers,
            List<IUser> originalUsers)
        {
            foreach (var updatedUser in expectedSuccessfullyUpdatedUsers)
            {
                var result = resultSet.Find(a => a.User.Username == updatedUser.Username);

                Assert.AreEqual("User information has been updated successfully", result.Message, "'{0}' is incorrect!", nameof(result.Message));
                Assert.AreEqual(InternalApiErrorCodes.Ok, result.ResultCode, "'{0}' should be 200 OK!", nameof(result.ResultCode));

                var originalUserData = originalUsers.Find(u => u.Username == updatedUser.Username);
                var expectedUserData = MergeNewAndOldUserData(originalUserData.UserData, updatedUser);

                VerifyUserIsUpdated(expectedUserData);
            }
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
        /// Verifies that the users that were NOT successfully updated have the expected status codes.
        /// </summary>
        /// <param name="resultSet">Result set from update users call.</param>
        /// <param name="expectedFailedDeletedUsers">(optional) A map of Users to InternalApiErrorCodes that we expect got errors when we tried to update them.</param>
        /// <param name="checkIfUserExists">(optional) Pass false if you don't want to check if the user exists.</param>
        private void VerifyUnsuccessfullyUpdatedUsers(
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
        /// Gets the user and compares against the expected user data.
        /// </summary>
        /// <param name="expectedUserData">The expected user data values after an update.</param>
        private void VerifyUserIsUpdated(UserDataModel expectedUserData)
        {
            var actualUserData = Helper.OpenApi.GetUser(_adminUser, expectedUserData.Id.Value);

            UserDataModel.AssertAreEqual(expectedUserData, actualUserData);
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
                I18NHelper.FormatInvariant("DUser with Id: {0} was deleted by some other user. Please refresh.",
                    deletedUser.Id);

            TestHelper.ValidateServiceErrorMessage(ex.RestResponse, expectedErrorMessage);
        }

        #endregion Private functions
    }
}
