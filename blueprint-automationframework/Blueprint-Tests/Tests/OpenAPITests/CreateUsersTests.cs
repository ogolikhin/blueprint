using CustomAttributes;
using Helper;
using Model;
using Model.Impl;
using NUnit.Framework;
using System.Collections.Generic;
using TestCommon;
using Utilities.Factories;
using Common;
using Model.OpenApiModel.UserModel.Results;
using System.Net;
using Model.Common.Constants;

namespace OpenAPITests
{
    [TestFixture]
    [Category(Categories.OpenApi)]
    public class CreateUsersTests : TestBase
    {
        private IUser _adminUser = null;

        private const string CREATE_PATH = RestPaths.OpenApi.USERS;

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

        [Explicit(IgnoreReasons.TestBug)]   // JSON Serialization compare fails for groups.
        [TestCase(1)]
        [TestCase(5)]
        [TestRail(246547)]
        [Description("Create one or more users and verify that the users were created.")]
        public void CreateUsers_ValidUserParameters_VerifyUserCreated(int numberOfUsersToCreate)
        {
            // Setup:
            var usersToCreate = GenerateListOfUserModels(numberOfUsersToCreate);

            // Execute:
            UserCallResultCollection result = null;

            Assert.DoesNotThrow(() => result = Helper.OpenApi.CreateUsers(_adminUser, usersToCreate),
                "'CREATE {0}' should return '201 Created' when valid data is passed to it!", CREATE_PATH);

            // Verify:
            Assert.AreEqual(usersToCreate.Count, result.Count, "Wrong number of User results were returned!");
            VerifyCreateUserResultSet(usersToCreate, result, BusinessLayerErrorCodes.Created, "User has been created successfully");
        }

        [Explicit(IgnoreReasons.TestBug)]   // JSON Serialization compare fails for groups.
        [TestCase]
        [TestRail(246548)]
        [Description("Create a list of users (some users already created) and verify a 207 HTTP status was returned and " +
        "that the users that were already existing are reported as already existing.")]
        public void CreateUsers_ListOfUsersAndAlreadyExistingUsers_207PartialSuccess()
        {
            // Setup:
            const int NUMBER_OF_USERS_TO_CREATE = 3;
            const int PARTIAL = 207;

            var existingUsersToCreate = GenerateListOfUserModels(NUMBER_OF_USERS_TO_CREATE);
            Helper.OpenApi.CreateUsers(_adminUser, existingUsersToCreate);

            var usersToCreate = GenerateListOfUserModels(NUMBER_OF_USERS_TO_CREATE);

            var newAndExistingUsersToCreate = new List<UserDataModel>(usersToCreate);

            newAndExistingUsersToCreate.AddRange(existingUsersToCreate);

            // Execute:
            UserCallResultCollection result = null;

            Assert.DoesNotThrow(() => result = Helper.OpenApi.CreateUsers(_adminUser, newAndExistingUsersToCreate, new List<HttpStatusCode> { (HttpStatusCode)PARTIAL }),
                "'CREATE {0}' should return '207 Partial Success' when valid data is passed to it and some users exist!", CREATE_PATH);

            // Verify:
            Assert.AreEqual(newAndExistingUsersToCreate.Count, result.Count, "Wrong number of User results were returned!");
            VerifyCreateUserResultSet(usersToCreate, result, BusinessLayerErrorCodes.Created, "User has been created successfully");
            VerifyCreateUserResultSet(existingUsersToCreate, result, BusinessLayerErrorCodes.UserValidationFailed, "User login name must be unique");
        }

        #endregion Positive tests

        // TODO: 409 when paasword is alphabetical & special characters
        // TODO: 409 when password is special and numerical characters
        // TODO: 409 when password is alphabetical & numeric characters
        // TODO: 207 for each type of error
        // TODO: 409 Admin role does not exists
        // TODO: 400 Missing property
        // TODO: Missing Login name
        // TODO: After I merge my PR #3994 you should change this to:
        // var userToCreate = UserDataModelFactory.CreateUserDataModel();
        // Then you can remove the UserOrGroupType = "User" step since it's done in the factory.

        #region Private methods

        /// <summary>
        /// Generates specific number of random users
        /// </summary>
        /// <param name="numberOfUsersToCreate">Number of users to generate</param>
        /// <returns>List of generated users</returns>
        public List<UserDataModel> GenerateListOfUserModels(int numberOfUsersToCreate)
        {
            var usersToCreate = new List<UserDataModel>();

            var groupList = new List<IGroup>
            {
                Helper.CreateGroupAndAddToDatabase()
            };

            var groupIds = new List<int>
            {
                groupList[0].GroupId
            };

            for (int i = 0; i < numberOfUsersToCreate; ++i)
            {
                var userToCreate = new UserDataModel()
                {
                    Username = RandomGenerator.RandomAlphaNumeric(10),
                    DisplayName = RandomGenerator.RandomAlphaNumeric(10),
                    FirstName = RandomGenerator.RandomAlphaNumeric(10),
                    LastName = RandomGenerator.RandomAlphaNumeric(10),
                    Password = "Password" + RandomGenerator.RandomSpecialChars(3) + RandomGenerator.RandomNumber(3),

                    Email = I18NHelper.FormatInvariant("{0}@{1}.com", RandomGenerator.RandomAlphaNumeric(5), RandomGenerator.RandomAlphaNumeric(5)),
                    Title = RandomGenerator.RandomAlphaNumeric(10),
                    Department = RandomGenerator.RandomAlphaNumeric(10),
                    Groups = groupList.ConvertAll(o => (Group)o),
                    GroupIds = groupIds,
                    InstanceAdminRole = "Default Instance Administrator",
                    ExpirePassword = true,
                    Enabled = true,
                    UserOrGroupType = "User"
                };
                usersToCreate.Add(userToCreate);
            }
            return usersToCreate;
        }

        /// <summary>
        /// Verifies create users result against user list.
        /// </summary>
        /// <param name="userList">List of users that was supposed to be created</param>
        /// <param name="resultSet">Result of create users call</param>
        /// <param name="expectedHttpCode">Expected HTTP code for this user</param>
        /// <param name="expectedMessage">Expected message for this user</param>
        private void VerifyCreateUserResultSet(List<UserDataModel> userList, UserCallResultCollection resultSet, int expectedHttpCode, string expectedMessage)
        {
            Assert.IsNotNull(userList, "The list of expected users is not created!");
            Assert.IsNotNull(resultSet, "Result set from create users call is not created!");
            Assert.IsTrue(userList.Count > 0, "The list of expected users is empty!");
            Assert.IsTrue(resultSet.Count > 0, "The list of resulted users is empty!");

            foreach (var user in userList)
            {
                var result = resultSet.Find(u => u.User.Username == user.Username);

                Assert.AreEqual(expectedHttpCode, result.ResultCode, "'{0}' is incorrect!", nameof(result.ResultCode));
                Assert.AreEqual(expectedMessage, result.Message, "'{0}' is incorrect!", nameof(result.Message));

                if (expectedHttpCode == BusinessLayerErrorCodes.Created)
                {
                    var getUserResult = Helper.OpenApi.GetUser(_adminUser, result.User.Id);
                    Assert.IsNotNull(getUserResult, "User does not exists!");

                    // TODO: After PR #3994 is merged, change the Asserts below to call: UserDataModel.AssertAreEqual()
                    Assert.AreEqual(result.User.Department, getUserResult.Department, "Department is not matching!");
                    Assert.AreEqual(result.User.DisplayName, getUserResult.DisplayName, "DisplayName is not matching!");
                    Assert.AreEqual(result.User.Email, getUserResult.Email, "Email is not matching!");
                    Assert.AreEqual(result.User.FirstName, getUserResult.FirstName, "FirstName is not matching!");
                    Assert.AreEqual(result.User.LastName, getUserResult.LastName, "LastName is not matching!");
                    Assert.AreEqual(result.User.Title, getUserResult.Title, "Title is not matching!");
                    Assert.AreEqual(result.User.Username, getUserResult.Username, "Username is not matching!");
                    Assert.AreEqual(result.User.UserOrGroupType, getUserResult.UserOrGroupType, "UserOrGroupType is not matching!");
                }
            }
        }

    #endregion Private methods
    }
}


