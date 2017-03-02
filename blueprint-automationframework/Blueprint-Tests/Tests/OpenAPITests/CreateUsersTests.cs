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
using Model.Factories;
using Model.Common.Enums;
using Newtonsoft.Json;
using System.Text.RegularExpressions;

namespace OpenAPITests
{
    [TestFixture]
    [Category(Categories.OpenApi)]
    public class CreateUsersTests : TestBase
    {
        private IUser _adminUser = null;

        private const string CREATE_PATH = RestPaths.OpenApi.USERS;
        private const string USER_CREATED_SUCCESSFULLY_MESSAGE = "User has been created successfully";
        private const int PARTIAL = 207;

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

        [TestCase(InstanceAdminRole.DefaultInstanceAdministrator)]
        [TestCase(InstanceAdminRole.AdministerALLProjects)]
        [TestCase(InstanceAdminRole.AssignInstanceAdministrators)]
        [TestCase(InstanceAdminRole.BlueprintAnalytics)]
        [TestCase(InstanceAdminRole.Email_ActiveDirectory_SAMLSettings)]
        [TestCase(InstanceAdminRole.InstanceStandardsManager)]
        [TestCase(InstanceAdminRole.LogGatheringAndLicenseReporting)]
        [TestCase(InstanceAdminRole.ManageAdministratorRoles)]
        [TestCase(InstanceAdminRole.ProvisionProjects)]
        [TestCase(InstanceAdminRole.ProvisionUsers)]
        [TestRail(246611)]
        [Description("Create a user with specific admin role and verify that the user was created successfully.")]
        public void CreateUsers_SpecificAdminRole_VerifyUserCreated(InstanceAdminRole instanceAdminRole)
        {
            // Setup:
            var userToCreate = GenerateListOfUserModels(numberOfUsersToCreate: 1);

            userToCreate[0].InstanceAdminRole = InstanceAdminRoleExtensions.ToInstanceAdminRoleString(instanceAdminRole);

            // Execute:
            UserCallResultCollection result = null;

            Assert.DoesNotThrow(() => result = Helper.OpenApi.CreateUsers(_adminUser, userToCreate),
                "'CREATE {0}' should return '201 Created' when valid data is passed to it!", CREATE_PATH);

            // Verify:
            Assert.AreEqual(userToCreate.Count, result.Count, "Wrong number of User results were returned!");
            VerifyCreateUserResultSet(userToCreate, result, BusinessLayerErrorCodes.Created, USER_CREATED_SUCCESSFULLY_MESSAGE);
        }

        [TestCase(5)]
        [TestRail(246547)]
        [Description("Create multiple users and verify that the users were created successfully.")]
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

            VerifyCreateUserResultSet(usersToCreate, result, BusinessLayerErrorCodes.Created, USER_CREATED_SUCCESSFULLY_MESSAGE);
        }

        [TestCase]
        [TestRail(266466)]
        [Description("Create a user with the username of removed user and verify the user created successfully")]
        public void CreateUser_UserWithThisUsernameWasPreviouslyDeleted_VerifyUserCreated()
        {
            // Setup:
            var project = ProjectFactory.GetProject(_adminUser, shouldRetrieveArtifactTypes: false);
            var authorUser = Helper.CreateUserWithProjectRolePermissions(TestHelper.ProjectRole.Author, project);
            authorUser.DeleteUser();

            var usersToCreate = GenerateListOfUserModels(numberOfUsersToCreate: 1);
            usersToCreate[0].Username = authorUser.Username;

            // Execute:
            UserCallResultCollection result = null;
            Assert.DoesNotThrow(() => result = Helper.OpenApi.CreateUsers(_adminUser, usersToCreate),
                "'CREATE {0}' should return '201 Created' when valid data is passed to it!", CREATE_PATH);

            // Verify:
            Assert.AreEqual(usersToCreate.Count, result.Count, "Wrong number of User results were returned!");

            VerifyCreateUserResultSet(usersToCreate, result, BusinessLayerErrorCodes.Created, USER_CREATED_SUCCESSFULLY_MESSAGE);
        }

        [TestCase]
        [TestRail(246548)]
        [Description("Create a list of users (some users already created) and verify 207 Partial Success HTTP status was returned and " +
            "that the users that were already existing are reported as already existing.")]
        public void CreateUsers_ListOfUsersAndAlreadyExistingUsers_207PartialSuccess()
        {
            // Setup:

            const int NUMBER_OF_USERS_TO_CREATE = 3;

            var existingUsersToCreate = GenerateListOfUserModels(NUMBER_OF_USERS_TO_CREATE);
            Helper.OpenApi.CreateUsers(_adminUser, existingUsersToCreate);

            var usersToCreate = GenerateListOfUserModels(NUMBER_OF_USERS_TO_CREATE);

            var newAndExistingUsersToCreate = new List<UserDataModel>(usersToCreate);
            newAndExistingUsersToCreate.AddRange(existingUsersToCreate);

            // Execute:
            UserCallResultCollection result = null;

            Assert.DoesNotThrow(() => result = Helper.OpenApi.CreateUsers(_adminUser, newAndExistingUsersToCreate,
                new List<HttpStatusCode> { (HttpStatusCode)PARTIAL }),
                "'CREATE {0}' should return '207 Partial Success' when valid data is passed to it and some users exist!", CREATE_PATH);

            // Verify:
            Assert.AreEqual(newAndExistingUsersToCreate.Count, result.Count, "Wrong number of User results were returned!");
            VerifyCreateUserResultSet(usersToCreate, result, BusinessLayerErrorCodes.Created, USER_CREATED_SUCCESSFULLY_MESSAGE);
            VerifyCreateUserResultSet(existingUsersToCreate, result, BusinessLayerErrorCodes.UserValidationFailed, "User login name must be unique");
        }

        [TestCase("Username", "Login name is required")]
        [TestCase("DisplayName", "Display name is required")]
        [TestCase("FirstName", "First name is required")]
        [TestCase("LastName", "Last name is required")]
        [TestCase("Password", "Password is required")]
        [TestRail(246644)]
        [Description("Create couple of users (one user with all required values and second one with empty property). " +
            "Verify 207 Partial Success HTTP status was returned")]
        public void CreateUsers_EmptyProperties_207PartialSuccess(string propertyName, string errorMessage)
        {
            // Setup:
            var validUserToCreate = GenerateListOfUserModels(numberOfUsersToCreate: 1);
            var userWithEmptyProperty = GenerateListOfUserModels(numberOfUsersToCreate: 1);

            CSharpUtilities.SetProperty(propertyName, "", userWithEmptyProperty[0]);

            var allUsersToCreate = new List<UserDataModel>(validUserToCreate);
            allUsersToCreate.AddRange(userWithEmptyProperty);

            // Execute:
            UserCallResultCollection result = null;

            Assert.DoesNotThrow(() => result = Helper.OpenApi.CreateUsers(_adminUser, allUsersToCreate,
                new List<HttpStatusCode> { (HttpStatusCode)PARTIAL }),
                "'CREATE {0}' should return '207 Partial Success' when one of users has invalid data!", CREATE_PATH);

            // Verify:
            Assert.AreEqual(allUsersToCreate.Count, result.Count, "Wrong number of User results were returned!");
            VerifyCreateUserResultSet(validUserToCreate, result, BusinessLayerErrorCodes.Created, USER_CREATED_SUCCESSFULLY_MESSAGE);
            VerifyCreateUserResultSet(userWithEmptyProperty, result, BusinessLayerErrorCodes.UserValidationFailed, errorMessage);
        }

        [TestCase]
        [TestRail(246645)]
        [Description("Create couple of users (one user with all required values and second one with non-existing instance admin role). " +
            "Verify 207 Partial Success HTTP status was returned")]
        public void CreateUsers_NonExistingRole_207PartialSuccess()
        {
            // Setup:
            var validUserToCreate = GenerateListOfUserModels(numberOfUsersToCreate: 1);
            var userWithNonExistingRole = GenerateListOfUserModels(numberOfUsersToCreate: 1);

            userWithNonExistingRole[0].InstanceAdminRole = "Non-Existing Role";

            var allUsersToCreate = new List<UserDataModel>(validUserToCreate);
            allUsersToCreate.AddRange(userWithNonExistingRole);

            // Execute:
            UserCallResultCollection result = null;

            Assert.DoesNotThrow(() => result = Helper.OpenApi.CreateUsers(_adminUser, allUsersToCreate,
                new List<HttpStatusCode> { (HttpStatusCode)PARTIAL }),
                "'CREATE {0}' should return '207 Partial Success' when one of users has invalid data!", CREATE_PATH);

            // Verify:
            Assert.AreEqual(allUsersToCreate.Count, result.Count, "Wrong number of User results were returned!");
            VerifyCreateUserResultSet(validUserToCreate, result, BusinessLayerErrorCodes.Created, USER_CREATED_SUCCESSFULLY_MESSAGE);
            VerifyCreateUserResultSet(userWithNonExistingRole, result, BusinessLayerErrorCodes.UserAddInstanceAdminRoleFailed,
                "Specified Instance admin role doesn't exist");
        }

        [TestCase]
        [TestRail(246646)]
        [Description("Create couple of users (one user with all required values and second one with non-existing group id). " +
            "Verify 207 Partial Success HTTP status was returned")]
        public void CreateUsers_NonExistingGroup_207PartialSuccess()
        {
            // Setup:
            var validUserToCreate = GenerateListOfUserModels(numberOfUsersToCreate: 1);
            var userWithNonExistingGroup = GenerateListOfUserModels(numberOfUsersToCreate: 1);

            userWithNonExistingGroup[0].GroupIds[0] = int.MaxValue;

            var allUsersToCreate = new List<UserDataModel>(validUserToCreate);
            allUsersToCreate.AddRange(userWithNonExistingGroup);

            // Execute:
            UserCallResultCollection result = null;

            Assert.DoesNotThrow(() => result = Helper.OpenApi.CreateUsers(_adminUser, allUsersToCreate,
                new List<HttpStatusCode> { (HttpStatusCode)PARTIAL }),
                "'CREATE {0}' should return '207 Partial Success' when one of users has invalid data!", CREATE_PATH);

            // Verify:
            Assert.AreEqual(allUsersToCreate.Count, result.Count, "Wrong number of User results were returned!");
            VerifyCreateUserResultSet(validUserToCreate, result, BusinessLayerErrorCodes.Created, USER_CREATED_SUCCESSFULLY_MESSAGE);
            VerifyCreateUserResultSet(userWithNonExistingGroup, result, BusinessLayerErrorCodes.UserAddToGroupFailed,
                "User is created, but cannot be added to a group");
        }

        [TestCase("1111&&&&", "Password must contain an upper-case letter")]
        [TestCase("1111AAAA", "Password must contain a non-alphanumeric character")]
        [TestCase("AAAA&&&&", "Password must contain a number")]
        [TestCase("1&A!1&A", "Password must be between 8 and 128 characters")]
        [TestCase("vGvK1lJu0Io1wvqYg50Tbooi55l9Q0gSYbdJ4XFREwZrxLKLP0PvoIlStitsC1fRqtASQtPNHfmzyLTdTU8FeKL9KWVGuw4vGbgQ1bjeX63ZrK2HwMlMFc3I7qNOCJbtvB",
            "Password must be between 8 and 128 characters\r\nPassword must contain a non-alphanumeric character")]
        [TestRail(246666)]
        [Description("Create couple of users (one user with all required values and second one with password in invalid format). " +
            "Verify 207 Partial Success HTTP status was returned")]
        public void CreateUsers_InvalidPassword_207PartialSuccess(string invalidPassword, string errorMessage)
        {
            // Setup:
            var validUserToCreate = GenerateListOfUserModels(numberOfUsersToCreate: 1);
            var userWithInvalidPassword = GenerateListOfUserModels(numberOfUsersToCreate: 1);

            userWithInvalidPassword[0].Password = invalidPassword;

            var allUsersToCreate = new List<UserDataModel>(validUserToCreate);
            allUsersToCreate.AddRange(userWithInvalidPassword);

            // Execute:
            UserCallResultCollection result = null;
            Assert.DoesNotThrow(() => result = Helper.OpenApi.CreateUsers(_adminUser, allUsersToCreate,
                new List<HttpStatusCode> { (HttpStatusCode)PARTIAL }),
                "'CREATE {0}' should return '207 Partial Success' when one of users has invalid data!", CREATE_PATH);

            // Verify:
            Assert.AreEqual(allUsersToCreate.Count, result.Count, "Wrong number of User results were returned!");
            VerifyCreateUserResultSet(validUserToCreate, result, BusinessLayerErrorCodes.Created, USER_CREATED_SUCCESSFULLY_MESSAGE);
            VerifyCreateUserResultSet(userWithInvalidPassword, result, BusinessLayerErrorCodes.UserValidationFailed, errorMessage);
        }

        [TestCase("Username", "User")]
        [TestCase("DisplayName", "Display")]
        [TestRail(261315)]
        [Description("Create couple of users (one user with all required values and second one with the password that the same as Username " +
            "or DiplayName). Verify 207 Partial Success HTTP status was returned")]
        public void CreateUsers_PasswordEqualToUserNameOrDisplayName_207PartialSuccess(string propertyName, string message)
        {
            // Setup:
            var validUserToCreate = GenerateListOfUserModels(numberOfUsersToCreate: 1);
            var userWithInvalidPassword = GenerateListOfUserModels(numberOfUsersToCreate: 1);
             
            CSharpUtilities.SetProperty(propertyName, userWithInvalidPassword[0].Password, userWithInvalidPassword[0]);

            var allUsersToCreate = new List<UserDataModel>(validUserToCreate);
            allUsersToCreate.AddRange(userWithInvalidPassword);

            UserCallResultCollection result = null;
            Assert.DoesNotThrow(() => result = Helper.OpenApi.CreateUsers(_adminUser, allUsersToCreate,
                new List<HttpStatusCode> { (HttpStatusCode)PARTIAL }),
                "'CREATE {0}' should return '207 Partial Success' when one of users has invalid data!", CREATE_PATH);

            // Verify:
            Assert.AreEqual(allUsersToCreate.Count, result.Count, "Wrong number of User results were returned!");
            VerifyCreateUserResultSet(validUserToCreate, result, BusinessLayerErrorCodes.Created, USER_CREATED_SUCCESSFULLY_MESSAGE);
            VerifyCreateUserResultSet(userWithInvalidPassword, result, BusinessLayerErrorCodes.UserValidationFailed, 
                "Password must not be similar to " + message + " Name");
        }

        [TestCase]
        [TestRail(246700)]
        [Description("Create couple of users (one user with all required values and second one with missing property). " + 
            "Verify 207 PartialSuccess HTTP status was returned")]
        public void CreateUsers_MissingProperty_207PartialSuccess()
        {
            // Setup:
            var validUserToCreate = GenerateListOfUserModels(numberOfUsersToCreate: 1);
            var userWithInvalidData = GenerateListOfUserModels(numberOfUsersToCreate: 1);

            var allUsersToCreate = new List<UserDataModel>(validUserToCreate);
            allUsersToCreate.AddRange(userWithInvalidData);

            userWithInvalidData[0].LastName = null;

            // Execute:
            UserCallResultCollection result = null;
            Assert.DoesNotThrow(() => result = Helper.OpenApi.CreateUsers(_adminUser, allUsersToCreate,
                new List<HttpStatusCode> { (HttpStatusCode)PARTIAL }),
                "'CREATE {0}' should return '207 Partial Success' when one of users has invalid data!", CREATE_PATH);

            // Verify:
            Assert.AreEqual(allUsersToCreate.Count, result.Count, "Wrong number of User results were returned!");
            VerifyCreateUserResultSet(validUserToCreate, result, BusinessLayerErrorCodes.Created, "User has been created successfully");
            VerifyCreateUserResultSet(userWithInvalidData, result, BusinessLayerErrorCodes.UserValidationFailed, "Last name is required");
        }

        #endregion Positive tests

        #region 400 Bad Request

        [TestCase]
        [TestRail(246665)]
        [Description("Create a user with parameter in invalid format). Verify 400 Bad request HTTP status was returned")]
        public void CreateUser_InvalidParameterFormat_400BadRequest()
        {
            // Setup:
            var userWithInvalidData = GenerateListOfUserModels(numberOfUsersToCreate: 1);

            // Serializes user model and replaces generated user lastname value with value of invalid format
            var validJson = JsonConvert.SerializeObject(userWithInvalidData);
            var toReplace = "\"" + userWithInvalidData[0].LastName + "\"";
            var corruptedJson = Regex.Replace(validJson, @toReplace, "$1");

            // Execute:
            var ex = Assert.Throws<Http400BadRequestException>(() => CreateUserInListWithInvalidParameters(
                Helper.ArtifactStore.Address,corruptedJson, _adminUser),
                "'POST {0}' should return 400 Bad Request when trying to create user with invalid parameter!", CREATE_PATH);

            // Verify:
            TestHelper.ValidateServiceError(ex.RestResponse, ApiBusinessErrorCodes.BadRequestDueToIncorrectQueryParameterValue,
                "The request parameter is missing or invalid");
        }

        #endregion 400 Bad Request

        #region 401 Unauthorized

        [TestCase]
        [TestRail(246690)]
        [Description("Create a user with no open api token in a header. Verify 401 Unauthorized HTTP status was returned")]
        public void CreateUsers_MissingOpenApiToken_401Unauthorized()
        {
            // Setup:
            var userToCreate = GenerateListOfUserModels(numberOfUsersToCreate: 1);

            var validJson = JsonConvert.SerializeObject(userToCreate);

            // Execute:
            var ex = Assert.Throws<Http401UnauthorizedException>(() => CreateUserInListWithInvalidParameters(
                Helper.ArtifactStore.Address, validJson, user: null),
                "'POST {0}' should return 401 Unauthorized when trying to create user with no token in a header!", CREATE_PATH);

            // Verify:
            TestHelper.ValidateServiceError(ex.RestResponse, "Unauthorized call.");
        }

        #endregion 401 Unauthorized

        #region 403 Forbidden

        [TestCase]
        [TestRail(266435)]
        [Description("Create a user with no permissions to create another users. Verify 403 Forbidden HTTP status was returned.")]
        public void CreateUser_InsufficientPermissions_403Forbidden()
        {
            var validUserToCreate = GenerateListOfUserModels(numberOfUsersToCreate: 1);

            var project = ProjectFactory.GetProject(_adminUser, shouldRetrieveArtifactTypes: false);
            var authorUser = Helper.CreateUserWithProjectRolePermissions(TestHelper.ProjectRole.Author, project);

            var ex = Assert.Throws<Http403ForbiddenException>(() => Helper.OpenApi.CreateUsers(authorUser, validUserToCreate),
                "'CREATE {0}' should return '403 Forbidden' when user doesn't have permission to create users!", CREATE_PATH);

            TestHelper.ValidateServiceError(ex.RestResponse, ApiBusinessErrorCodes.Forbidden, 
                "The user does not have the privileges required to perform the action.");
        }

        #endregion 403 Forbidden

        #region 409 Conflict

        [TestCase("Username", "Login name is required")]
        [TestCase("DisplayName", "Display name is required")]
        [TestCase("FirstName", "First name is required")]
        [TestCase("LastName", "Last name is required")]
        [TestCase("Password", "Password is required")]
        [TestRail(266429)]
        [Description("Create a user with missing property. Verify 409 Conflict HTTP status was returned")]
        public void CreateUser_MissingProperty_409Conflict(string propertyName, string errorMessage)
        {
            // Setup:
            var userWithMissingProperty = GenerateListOfUserModels(numberOfUsersToCreate: 1);

            CSharpUtilities.SetProperty<string>(propertyName, null, userWithMissingProperty[0]);

            // Execute:
            UserCallResultCollection result = null;
            Assert.DoesNotThrow(() => result = Helper.OpenApi.CreateUsers(_adminUser, userWithMissingProperty,
                new List<HttpStatusCode> { HttpStatusCode.Conflict }),
                "'CREATE {0}' should return '409 Conflict' when user has missing property!", CREATE_PATH);

            // Verify:
            VerifyCreateUserResultSet(userWithMissingProperty, result, BusinessLayerErrorCodes.UserValidationFailed, errorMessage);
        }

        [TestCase]
        [TestRail(266436)]
        [Description("Create a user with non-existing admin role. Verify 409 Conflict HTTP status was returned")]
        public void CreateUser_NonExistingAdminRole_409Conflict()
        {
            // Setup:
            var userToCreate = GenerateListOfUserModels(numberOfUsersToCreate: 1);

            userToCreate[0].InstanceAdminRole = RandomGenerator.RandomAlphaNumeric(10);

            // Execute:
            UserCallResultCollection result = null;
            Assert.DoesNotThrow(() => result = Helper.OpenApi.CreateUsers(_adminUser, userToCreate,
                new List<HttpStatusCode> { HttpStatusCode.Conflict }),
                "'CREATE {0}' should return '409 Conflict' when instance administrator role does not exists!", CREATE_PATH);

            // Verify:
            Assert.AreEqual(userToCreate.Count, result.Count, "Wrong number of User results were returned!");
            VerifyCreateUserResultSet(userToCreate, result, BusinessLayerErrorCodes.UserAddInstanceAdminRoleFailed, 
                "Specified Instance admin role doesn't exist");
        }

        [TestCase]
        [TestRail(266450)]
        [Description("Create a user with non-existing group id. Verify 409 Conflict HTTP status was returned")]
        public void CreateUser_NonExistingGroup_409Conflict()
        {
            // Setup:
            var userWithNonExistingGroup = GenerateListOfUserModels(numberOfUsersToCreate: 1);

            userWithNonExistingGroup[0].GroupIds[0] = int.MaxValue;

            // Execute:
            UserCallResultCollection result = null;
            Assert.DoesNotThrow(() => result = Helper.OpenApi.CreateUsers(_adminUser, userWithNonExistingGroup,
                new List<HttpStatusCode> { HttpStatusCode.Conflict }),
                "'CREATE {0}' should return '409 Conflict' when one of users has invalid data!", CREATE_PATH);

            // Verify:
            Assert.AreEqual(userWithNonExistingGroup.Count, result.Count, "Wrong number of User results were returned!");
            VerifyCreateUserResultSet(userWithNonExistingGroup, result, BusinessLayerErrorCodes.UserAddToGroupFailed,
                "User is created, but cannot be added to a group");
        }

        //  Bug http://svmtfs2015:8080/tfs/svmtfs2015/Blueprint/_workitems?_a=edit&id=5486 Allowed in BP emails at the moment
        //  [TestCase(".email@domain.com")]
        //  [TestCase("email.@domain.com")]
        //  [TestCase("email..email@domain.com")]
        //  [TestCase("あいうえお@domain.com")]
        //  [TestCase("email@-domain.com")]
        [TestCase("plainaddress")]
        [TestCase("A#@%^%#$@#$@#.com")]
        [TestCase("@domain.com ")]
        [TestCase("Joe Smith <email@domain.com>")]
        [TestCase("email.domain.com")]
        [TestCase("email@domain@domain.com")]
        [TestCase("email@domain.com (Joe Smith)")]
        [TestCase("email@domain")]
        [TestCase("email@111.222.333.44444")]
        [TestCase("email@domain..com")]
        [TestRail(266467)]
        [Description("Create user with invalid email. Verify 409 Conflict HTTP status was returned")]
        public void CreateUsers_InvalidEmail_409Conflict(string wrongEmailAddress)
        {
            // Setup:
            var usersWithWrongEmailAddress = GenerateListOfUserModels(numberOfUsersToCreate: 1);
            usersWithWrongEmailAddress[0].Email = wrongEmailAddress;

            // Execute:
            UserCallResultCollection result = null;
            Assert.DoesNotThrow(() => result = Helper.OpenApi.CreateUsers(_adminUser, usersWithWrongEmailAddress,
                new List<HttpStatusCode> { HttpStatusCode.Conflict }),
                "'CREATE {0}' should return '409 Conflict' when one of users has invalid data!", CREATE_PATH);

            // Verify:
            Assert.AreEqual(usersWithWrongEmailAddress.Count, result.Count, "Wrong number of User results were returned!");

            VerifyCreateUserResultSet(usersWithWrongEmailAddress, result, BusinessLayerErrorCodes.UserValidationFailed, 
                "Invalid email address. Use following format: user@company.com");
        }

        #endregion 409 Conflict

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

                userToCreate.Username = RandomGenerator.RandomAlphaNumeric(10);
                userToCreate.DisplayName = RandomGenerator.RandomAlphaNumeric(10);
                userToCreate.FirstName = RandomGenerator.RandomAlphaNumeric(10);
                userToCreate.LastName = RandomGenerator.RandomAlphaNumeric(10);
                userToCreate.Password = "Password" + RandomGenerator.RandomSpecialChars(3) + RandomGenerator.RandomNumber(3);

                userToCreate.Email = I18NHelper.FormatInvariant("{0}@{1}.com", RandomGenerator.RandomAlphaNumeric(5),
                    RandomGenerator.RandomAlphaNumeric(5));
                userToCreate.Title = RandomGenerator.RandomAlphaNumeric(10);
                userToCreate.Department = RandomGenerator.RandomAlphaNumeric(10);
                userToCreate.Groups = groupList.ConvertAll(o => (Model.Impl.Group)o);
                userToCreate.GroupIds = groupIds;
                userToCreate.InstanceAdminRole = "Default Instance Administrator";
                userToCreate.ExpirePassword = true;
                userToCreate.Enabled = true;

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
        private void VerifyCreateUserResultSet(
            List<UserDataModel> userList, UserCallResultCollection resultSet, int expectedHttpCode, string expectedMessage)
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
                    var getUserResult = Helper.OpenApi.GetUser(_adminUser, result.User.Id.Value);
                    Assert.IsNotNull(getUserResult, "User does not exists!");

                    UserDataModel.AssertAreEqual(result.User, getUserResult, skipPropertiesNotReturnedByOpenApi: true);
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
        public static RestResponse CreateUserInListWithInvalidParameters(string address, string requestBody, IUser user)
        {
            var restApi = new RestApiFacade(address, user?.Token?.OpenApiToken);
            const string contentType = "application/json";
            
            return restApi.SendRequestBodyAndGetResponse(
                CREATE_PATH,
                RestRequestMethod.POST,
                requestBody,
                contentType);
        }

        #endregion Private methods
    }
}

