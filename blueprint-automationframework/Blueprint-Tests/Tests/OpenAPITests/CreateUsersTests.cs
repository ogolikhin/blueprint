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

        [TestCase("Default Instance Administrator")]
        [TestCase("Administer ALL Projects")]
        [TestCase("Assign Instance Administrators")]
        [TestCase("Blueprint Analytics")]
        [TestCase("Email, Active Directory, SAML Settings")]
        [TestCase("Instance Standards Manager")]
        [TestCase("Log Gathering and License Reporting")]
        [TestCase("Manage Administrator Roles")]
        [TestCase("Provision Projects")]
        [TestCase("Provision Users")]
        [TestRail(246611)]
        [Description("Create one user with specific admin role and verify that the user was created.")]
        public void CreateUser_SpecificAdminRole_VerifyUserCreated(string instanceAdminRole)
        {
            // Setup:
            var userToCreate = GenerateListOfUserModels(numberOfUsersToCreate: 1);

            userToCreate[0].InstanceAdminRole = instanceAdminRole;

            // Execute:
            UserCallResultCollection result = null;

            Assert.DoesNotThrow(() => result = Helper.OpenApi.CreateUsers(_adminUser, userToCreate),
                "'CREATE {0}' should return '201 Created' when valid data is passed to it!", CREATE_PATH);

            // Verify:
            VerifyCreateUserResultSet(userToCreate, result, expectedHttpCode: 201, expectedMessage: "User has been created successfully");
        }

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
            VerifyCreateUserResultSet(usersToCreate, result, expectedHttpCode: 201, expectedMessage: "User has been created successfully");
        }

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
            VerifyCreateUserResultSet(usersToCreate, result, expectedHttpCode: 201, expectedMessage: "User has been created successfully");
            VerifyCreateUserResultSet(existingUsersToCreate, result, expectedHttpCode: 1192, expectedMessage: "User login name must be unique");
        }

        #endregion Positive tests

        // TODO: 409 when paasword is alphabetical & special characters
        // TODO: 409 when password is special and numerical characters
        // TODO: 409 when password is alphabetical & numeric characters
        // TODO: 207 for each type of error
        // TODO: 409 Admin role does not exists
        // TODO: 400 Missing property
        // TODO: Missing Login name

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
                    Groups = groupList,
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
        /// <param name="expectedHttpCode">Expected message for this user</param>
        private static void VerifyCreateUserResultSet(List<UserDataModel> userList, UserCallResultCollection resultSet, int expectedHttpCode, string expectedMessage)
        {
            Assert.IsNotNull(userList, "The list of expected users is empty!");
            Assert.IsNotNull(resultSet, "Result set from create users call is empty!");

            foreach (var user in userList)
            {
                var result = resultSet.Find(u => u.User.Username == user.Username);

                Assert.AreEqual(expectedHttpCode, result.ResultCode, "'{0}' is incorrect!", nameof(result.ResultCode));
                Assert.AreEqual(expectedMessage, result.Message, "'{0}' is incorrect!", nameof(result.Message));
            }
        }

    #endregion Private methods
    }
}


