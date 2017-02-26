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
using Utilities;
using Model.Common.Constants;
using System.Text.RegularExpressions;
using Newtonsoft.Json;
using Utilities.Facades;
using System;

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
        [Description("Create a user with specific admin role and verify that the user was created.")]
        public void CreateUsers_SpecificAdminRole_VerifyUserCreated(string instanceAdminRole)
        {
            // Setup:
            var userToCreate = GenerateListOfUserModels(numberOfUsersToCreate: 1);

            userToCreate[0].InstanceAdminRole = instanceAdminRole;

            // Execute:
            UserCallResultCollection result = null;

            Assert.DoesNotThrow(() => result = Helper.OpenApi.CreateUsers(_adminUser, userToCreate),
                "'CREATE {0}' should return '201 Created' when valid data is passed to it!", CREATE_PATH);

            // Verify:
            Assert.AreEqual(userToCreate.Count, result.Count, "Wrong number of User results were returned!");
            VerifyCreateUserResultSet(userToCreate, result, BusinessLayerErrorCodes.Created, "User has been created successfully");
        }

        [Explicit(IgnoreReasons.TestBug)]   // JSON Serialization compare fails for groups.
        [TestCase(5)]
        [TestRail(246547)]
        [Description("Create multiple users and verify that the users were created.")]
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
            const int PARTIAL = 207;
            const int NUMBER_OF_USERS_TO_CREATE = 3;

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

        [TestCase("Username", "Login name is required")]
        [TestCase("DisplayName", "Display name is required")]
        [TestCase("FirstName", "First name is required")]
        [TestCase("LastName", "Last name is required")]
        [TestCase("Password", "Password is required")]
        [TestRail(246644)]
        [Description("Create couple of users (one user with all required values and second one with empty property) and verify a 207 HTTP status was returned")]
        public void CreateUsers_EmptyProperties_207PartialSuccess(string propertyName, string errorMessage)
        {
            // Setup:
            const int PARTIAL = 207;

            var validUserToCreate = GenerateListOfUserModels(numberOfUsersToCreate: 1);
            var userWithEmptyProperty = GenerateListOfUserModels(numberOfUsersToCreate: 1);

            CSharpUtilities.SetProperty(propertyName, "", userWithEmptyProperty[0]);

            var allUsersToCreate = new List<UserDataModel>(validUserToCreate);
            allUsersToCreate.AddRange(userWithEmptyProperty);

            // Execute:
            UserCallResultCollection result = null;

            Assert.DoesNotThrow(() => result = Helper.OpenApi.CreateUsers(_adminUser, allUsersToCreate, new List<HttpStatusCode> { (HttpStatusCode)PARTIAL }),
                "'CREATE {0}' should return '207 Partial Success' when one of users has invalid data!", CREATE_PATH);

            // Verify:
            Assert.AreEqual(allUsersToCreate.Count, result.Count, "Wrong number of User results were returned!");
            VerifyCreateUserResultSet(validUserToCreate, result, BusinessLayerErrorCodes.Created, "User has been created successfully");
            VerifyCreateUserResultSet(userWithEmptyProperty, result, BusinessLayerErrorCodes.UserValidationFailed, errorMessage);
        }

        [TestCase]
        [TestRail(246645)]
        [Description("Create couple of users (one user with all required values and second one with non-existing instance admin role). " +
            "Verify a 207 HTTP status was returned")]
        public void CreateUsers_NonExistingRole_207PartialSuccess()
        {
            // Setup:
            const int PARTIAL = 207;

            var validUserToCreate = GenerateListOfUserModels(numberOfUsersToCreate: 1);
            var userWithNonExistingRole = GenerateListOfUserModels(numberOfUsersToCreate: 1);

            userWithNonExistingRole[0].InstanceAdminRole = "Non-Existing Role";

            var allUsersToCreate = new List<UserDataModel>(validUserToCreate);
            allUsersToCreate.AddRange(userWithNonExistingRole);

            // Execute:
            UserCallResultCollection result = null;

            Assert.DoesNotThrow(() => result = Helper.OpenApi.CreateUsers(_adminUser, allUsersToCreate, new List<HttpStatusCode> { (HttpStatusCode)PARTIAL }),
                "'CREATE {0}' should return '207 Partial Success' when one of users has invalid data!", CREATE_PATH);

            // Verify:
            Assert.AreEqual(allUsersToCreate.Count, result.Count, "Wrong number of User results were returned!");
            VerifyCreateUserResultSet(validUserToCreate, result, BusinessLayerErrorCodes.Created, "User has been created successfully");
            VerifyCreateUserResultSet(userWithNonExistingRole, result, BusinessLayerErrorCodes.UserAddInstanceAdminRoleFailed,
                "Specified Instance admin role doesn't exist");
        }

        [TestCase]
        [TestRail(246646)]
        [Description("Create couple of users (one user with all required values and second one with non-existing group id). Verify a 207 HTTP status was returned")]
        public void CreateUsers_NonExistingGroup_207PartialSuccess()
        {
            // Setup:
            const int PARTIAL = 207;

            var validUserToCreate = GenerateListOfUserModels(numberOfUsersToCreate: 1);
            var userWithNonExistingGroup = GenerateListOfUserModels(numberOfUsersToCreate: 1);

            userWithNonExistingGroup[0].GroupIds[0] = int.MaxValue;

            var allUsersToCreate = new List<UserDataModel>(validUserToCreate);
            allUsersToCreate.AddRange(userWithNonExistingGroup);

            // Execute:
            UserCallResultCollection result = null;

            Assert.DoesNotThrow(() => result = Helper.OpenApi.CreateUsers(_adminUser, allUsersToCreate, new List<HttpStatusCode> { (HttpStatusCode)PARTIAL }),
                "'CREATE {0}' should return '207 Partial Success' when one of users has invalid data!", CREATE_PATH);

            // Verify:
            Assert.AreEqual(allUsersToCreate.Count, result.Count, "Wrong number of User results were returned!");
            VerifyCreateUserResultSet(validUserToCreate, result, BusinessLayerErrorCodes.Created, "User has been created successfully");
            VerifyCreateUserResultSet(userWithNonExistingGroup, result, BusinessLayerErrorCodes.UserAddToGroupFailed, "User is created, but cannot be added to a group");
        }

        [TestCase]
        [TestRail(246665)]
        [Description("Create couple of users (one user with all required values and second one with parameter in invalid format). Verify a 207 HTTP status was returned")]
        public void CreateUsers_InvalidParameterFormat_207PartialSuccess()
        {
            // Setup:
            const int PARTIAL = 207;

            var validUserToCreate = GenerateListOfUserModels(numberOfUsersToCreate: 1);
            var userWithInvalidData = GenerateListOfUserModels(numberOfUsersToCreate: 1);

            var allUsersToCreate = new List<UserDataModel>(validUserToCreate);
            allUsersToCreate.AddRange(userWithInvalidData);

            var validJson = JsonConvert.SerializeObject(allUsersToCreate); 
            var toReplace = "\"" + userWithInvalidData[0].LastName + "\"";
            var corruptedJson = Regex.Replace(validJson, @toReplace, "1");

            // Execute:
            RestResponse result = null;

            Assert.DoesNotThrow(() => result = UpdateUserInListWithInvalidParameters(Helper.ArtifactStore.Address, corruptedJson, _adminUser, new List<HttpStatusCode> { (HttpStatusCode)PARTIAL }),
                "'CREATE {0}' should return '207 Partial Success' when one of users has invalid data!", CREATE_PATH);
/*
            // Verify:
            Assert.AreEqual(allUsersToCreate.Count, result.Count, "Wrong number of User results were returned!");
            VerifyCreateUserResultSet(validUserToCreate, result, BusinessLayerErrorCodes.Created, "User has been created successfully");
            VerifyCreateUserResultSet(userWithInvalidData, result, BusinessLayerErrorCodes.UserAddToGroupFailed, "User is created, but cannot be added to a group");
*/        }

        [TestCase("1111&&&&AAA", "Password must contain a low-case letter")]
        [TestCase("1111&&&&", "Password must contain an upper-case letter")]
        [TestCase("1111AAAA", "Password must contain a non-alphanumeric character")]
        [TestCase("AAAA&&&&", "Password must contain a number")]
        [TestCase("1&A!1&A", "Password must be between 8 and 128 characters")]
        [TestCase("vGvK1lJu0Io1wvqYg50Tbooi55l9Q0gSYbdJ4XFREwZrxLKLP0PvoIlStitsC1fRqtASQtPNHfmzyLTdTU8FeKL9KWVGuw4vGbgQ1bjeX63ZrK2HwMlMFc3I7qNOCJbtvB",
            "Password must be between 8 and 128 characters\r\nPassword must contain a non-alphanumeric character")]
        [TestRail(246666)]
        [Description("Create couple of users (one user with all required values and second one with password in invalid format). Verify a 207 HTTP status was returned")]
        public void CreateUsers_InvalidPassword_207PartialSuccess(string invalidPassword, string errorMessage)
        {
            // Setup:
            const int PARTIAL = 207;

            errorMessage = errorMessage + "";

            var validUserToCreate = GenerateListOfUserModels(numberOfUsersToCreate: 1);
            var userWithInvalidData = GenerateListOfUserModels(numberOfUsersToCreate: 1);

            userWithInvalidData[0].Password = invalidPassword;

            var allUsersToCreate = new List<UserDataModel>(validUserToCreate);
            allUsersToCreate.AddRange(userWithInvalidData);

            var jsonBody = JsonConvert.SerializeObject(allUsersToCreate);

            // Execute:
            RestResponse result = null;

            Assert.DoesNotThrow(() => result = UpdateUserInListWithInvalidParameters(Helper.ArtifactStore.Address, jsonBody, _adminUser, new List<HttpStatusCode> { (HttpStatusCode)PARTIAL }),
                "'CREATE {0}' should return '207 Partial Success' when one of users has invalid data!", CREATE_PATH);
            /*
                        // Verify:
                        Assert.AreEqual(allUsersToCreate.Count, result.Count, "Wrong number of User results were returned!");
                        VerifyCreateUserResultSet(validUserToCreate, result, BusinessLayerErrorCodes.Created, "User has been created successfully");
                        VerifyCreateUserResultSet(userWithInvalidData, result, BusinessLayerErrorCodes.UserAddToGroupFailed, "User is created, but cannot be added to a group");
            */
        }
        #endregion Positive tests

        /*************** 401
        [TestCase]
        [TestRail(0)]
        [Description("Create couple of users (one user with all required values and second one with missing open API token). Verify a 207 HTTP status was returned")]
        public void CreateUsers_MissingOpenApiToken_207PartialSuccess()
        {
            // Setup:
            //            const int PARTIAL = 207;

            var validUserToCreate = GenerateListOfUserModels(numberOfUsersToCreate: 1);
            var userWithInvalidData = GenerateListOfUserModels(numberOfUsersToCreate: 1);

            var allUsersToCreate = new List<UserDataModel>(validUserToCreate);
            allUsersToCreate.AddRange(userWithInvalidData);

            var validJson = JsonConvert.SerializeObject(allUsersToCreate);
            var toReplace = "\"" + userWithInvalidData[0].LastName + "\"";
            var corruptedJson = Regex.Replace(validJson, @toReplace, "1");

            // Execute:
            RestResponse result = null;

            Assert.DoesNotThrow(() => result = UpdateUserInListWithInvalidParameters(Helper.ArtifactStore.Address, corruptedJson, user: null),
                "'CREATE {0}' should return '207 Partial Success' when one of users has invalid data!", CREATE_PATH);
            /*
                        // Verify:
                        Assert.AreEqual(allUsersToCreate.Count, result.Count, "Wrong number of User results were returned!");
                        VerifyCreateUserResultSet(validUserToCreate, result, BusinessLayerErrorCodes.Created, "User has been created successfully");
                        VerifyCreateUserResultSet(userWithInvalidData, result, BusinessLayerErrorCodes.UserAddToGroupFailed, "User is created, but cannot be added to a group");
            
        }
*/

        // TODO: 207 for each type of error
        // TODO: 409 Admin role does not exists
        // TODO: 400 Missing property
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
                var userToCreate = UserDataModelFactory.CreateUserDataModel();
///                var userToCreate = new UserDataModel()
                {
                    Username = RandomGenerator.RandomAlphaNumeric(10),
                    DisplayName = RandomGenerator.RandomAlphaNumeric(10),
                    FirstName = RandomGenerator.RandomAlphaNumeric(10),
                    LastName = RandomGenerator.RandomAlphaNumeric(10),
                    Password = "Password" + RandomGenerator.RandomSpecialChars(3) + RandomGenerator.RandomNumber(3),

                    Email = I18NHelper.FormatInvariant("{0}@{1}.com", RandomGenerator.RandomAlphaNumeric(5), RandomGenerator.RandomAlphaNumeric(5)),
                    Title = RandomGenerator.RandomAlphaNumeric(10),
                    Department = RandomGenerator.RandomAlphaNumeric(10),
                    Groups = groupList.ConvertAll(o => (Model.Impl.Group)o),
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

        /// <summary>
        /// Creates a user with specific json body.  Use this for testing cases where the create user is expected to fail.
        /// </summary>
        /// <param name="address">The base address used for the REST call.</param>
        /// <param name="requestBody">The request body (i.e. user to be created).</param>
        /// <param name="user">The user creating another user.</param>
        /// <returns>The call response returned.</returns>
        public static RestResponse UpdateUserInListWithInvalidParameters(string address, string requestBody, IUser user, List<HttpStatusCode> expectedStatusCodes)
        {
            ThrowIf.ArgumentNull(user, nameof(user));

            var restApi = new RestApiFacade(address, user?.Token?.OpenApiToken);
            const string contentType = "application/json";

            var response = restApi.SendRequestBodyAndGetResponse(
                CREATE_PATH,
                RestRequestMethod.POST,
                requestBody,
                contentType,
                expectedStatusCodes: expectedStatusCodes);

            return response;
        }

        #endregion Private methods
    }
}


