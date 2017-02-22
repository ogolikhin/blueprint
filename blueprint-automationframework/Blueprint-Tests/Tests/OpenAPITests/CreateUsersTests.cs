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
using Utilities.Facades;
using Newtonsoft.Json;

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
            VerifyCreateUserResultSet(usersToCreate, result, BusinessLayerErrorCodes.Created, expectedMessage: "User has been created successfully");
        }

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

        #endregion Positive tests

        // TODO: 409 when paasword is alphabetical & special characters
        // TODO: 409 when password is special and numerical characters
        // TODO: 409 when password is alphabetical & numeric characters
        // TODO: 207 for each type of error
        // TODO: 409 Admin role does not exists
        // TODO: 400 Missing property


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

 // Under development part 
 /*
        /// <summary>
        /// Runs the OpenAPI Create User call with invalid data in the body.
        /// </summary>
        /// <typeparam name="T">The data type to send in the body.  Valid type is a List of strings.</typeparam>
        /// <param name="userToAuthenticate">The user to authenticate with.</param>
        /// <param name="jsonBody">The data to send in the body.</param>
        /// <returns>CreateUserResultSet object if successful.</returns>
        private UserCallResultCollection CreateUserWithInvalidBody<T>(IUser userToAuthenticate, T jsonBody) where T : new()
        {
            var restApi = new RestApiFacade(Helper.OpenApi.Address, userToAuthenticate?.Token?.OpenApiToken);
            string path = CREATE_PATH;

            return restApi.SendRequestAndDeserializeObject<UserCallResultCollection, T>(
                path,
                RestRequestMethod.POST,
                jsonBody);
        }

        /// <summary>
        /// Try to create an invalid user with Property Changes.  Use this for testing cases where the save is expected to fail.
        /// </summary>
        /// <param name="address">The base address used for the REST call.</param>
        /// <param name="requestBody">The request body (i.e. artifact to be updated).</param>
        /// <param name="userModel">The UserDatamodel to create user.</param>
        /// <param name="user">The user creating another user.</param>
        /// <returns>The body content returned.</returns>
        public string CreateUserWithInvalidParameters(string address, string requestBody,
            UserDataModel userModel, IUser user)
        {
            ThrowIf.ArgumentNull(user, nameof(user));

            string tokenValue = user.Token?.AccessControlToken;

            string path = I18NHelper.FormatInvariant(RestPaths.Svc.ArtifactStore.ARTIFACTS_id_, userModel);
            RestApiFacade restApi = new RestApiFacade(address, tokenValue);
            const string contentType = "application/json";

            var response = restApi.SendRequestBodyAndGetResponse(
                path,
                RestRequestMethod.POST,
                requestBody,
                contentType);

            return response.Content;
        }*/

        #endregion Private methods
    }
}


