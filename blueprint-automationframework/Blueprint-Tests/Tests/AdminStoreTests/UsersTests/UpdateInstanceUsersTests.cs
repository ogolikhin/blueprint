﻿using System;
using System.Collections.Generic;
using System.Net;
using Common;
using CustomAttributes;
using Helper;
using Model;
using Model.Common.Enums;
using Model.Impl;
using NUnit.Framework;
using TestCommon;
using Utilities;
using Utilities.Facades;
using Utilities.Factories;

namespace AdminStoreTests.UsersTests
{
    [TestFixture]
    [Category(Categories.AdminPortal)]
    [Category(Categories.AdminStore)]

    public class UpdateInstanceUserTests : TestBase
    {
        private const string USER_PATH = RestPaths.Svc.AdminStore.Users.USERS;
        private const string USER_PATH_ID = RestPaths.Svc.AdminStore.Users.USERS_id_;

        private IUser _adminUser = null;

        #region Setup and Cleanup

        [SetUp]
        public void SetUp()
        {
            Helper = new TestHelper();
            _adminUser = Helper.CreateUserAndAuthenticate(TestHelper.AuthenticationTokenTypes.AccessControlToken);
        }

        [TearDown]
        public void TearDown()
        {
            Helper?.Dispose();
        }

        #endregion Setup and Cleanup

        // TODO: Add test that edits a user with the login name of a deleted user
        // TODO: Add edge case tests for field and password validation?
        // TODO: Verify license type calculation is performed correctly
        // TODO: Add test for editing a Guest user
        // TODO: Add test for SSO

        #region 200 OK Tests

        [TestCase]
        [Description("Create and add an instance user. Modify multiple fields of the user that was just created. Update the user. " +
                     "Verify the user updates correctly.")]
        [TestRail(303364)]
        public void UpdateInstanceUser_MultipleChanges_UserUpdatesCorrectly()
        {
            // Setup:
            var createdUser = AdminStoreHelper.GenerateRandomInstanceUser();

            int createdUserId = 0;

            Assert.DoesNotThrow(() =>
            {
                createdUserId = Helper.AdminStore.AddUser(_adminUser, createdUser);
            }, "'POST {0}' should return 201 OK for a valid session token!", USER_PATH);

            // Update user Id with returned value and incremented version
            createdUser.Id = createdUserId;
            createdUser.CurrentVersion++;

            // Modify all properties that can be updated via UpdateUser
            createdUser.Login += RandomGenerator.RandomAlphaNumeric(2);
            createdUser.FirstName += RandomGenerator.RandomAlphaNumeric(2);
            createdUser.LastName += RandomGenerator.RandomAlphaNumeric(2);
            createdUser.DisplayName += RandomGenerator.RandomAlphaNumeric(2);
            createdUser.Title += RandomGenerator.RandomAlphaNumeric(2);
            createdUser.Department += RandomGenerator.RandomAlphaNumeric(2);

            createdUser.Email = RandomGenerator.RandomAlphaNumeric(2) + createdUser.Email;

            createdUser.ImageId = 1;
            createdUser.InstanceAdminRoleId = InstanceAdminRole.DefaultInstanceAdministrator;

            createdUser.Enabled = !createdUser.Enabled;
            createdUser.AllowFallback = !createdUser.AllowFallback;
            createdUser.ExpirePassword = !createdUser.ExpirePassword;

            createdUser.Login += RandomGenerator.RandomAlphaNumeric(2);

            // Execute:
            Assert.DoesNotThrow(() =>
            {
                Helper.AdminStore.UpdateUser(_adminUser, createdUser);
            }, "'PUT {0}' should return 200 OK for a valid session token!", USER_PATH_ID);

            // Updating the user to an Instance Administrator changes the user license type to author
            createdUser.LicenseType = LicenseLevel.Author;

            InstanceUser updatedUser = null;

            Assert.DoesNotThrow(() =>
            {
                updatedUser = Helper.AdminStore.GetUserById(_adminUser, createdUserId);
            }, "'GET {0}' should return 200 OK for a valid session token!", USER_PATH_ID);

            // Verify:
            // Update CurrentVersion in CreatedUser for comparison
            createdUser.CurrentVersion++;

            AdminStoreHelper.AssertAreEqual(createdUser, updatedUser);
        }

        [TestCase("Login", "ModifiedLogin")]
        [TestCase("FirstName", "ModifiedFirstName")]
        [TestCase("LastName", "ModifiedLastName")]
        [TestCase("DisplayName", "Modified DisplayName")]
        [TestCase("Title", "ModifiedTitle")]
        [TestCase("Department", "ModifiedDepartment")]
        [TestCase("Email", "Modified.Email@domain.com")]
        [TestCase("ImageId", 1)]
        [TestCase("InstanceAdminRoleId", InstanceAdminRole.DefaultInstanceAdministrator)]
        [TestCase("Enabled", false)]
        [TestCase("AllowFallback", true)]
        [TestCase("ExpirePassword", false)]
        [Description("Create and add an instance user. Modify a single field of the user that was just created. Update the user. " +
                     "Verify the user updates correctly.")]
        [TestRail(303638)]
        public void UpdateInstanceUser_SingleChange_UserUpdatesCorrectly(string property, object value)
        {
            // Setup:
            var createdUser = AdminStoreHelper.GenerateRandomInstanceUser();

            int createdUserId = 0;

            Assert.DoesNotThrow(() =>
            {
                createdUserId = Helper.AdminStore.AddUser(_adminUser, createdUser);
            }, "'POST {0}' should return 201 OK for a valid session token!", USER_PATH);

            // Update user Id with returned value and incremented version
            createdUser.Id = createdUserId;
            createdUser.CurrentVersion++;

            // Need this to ensure unique login
            if (property == "Login")
            {
                value += RandomGenerator.RandomAlphaNumericUpperAndLowerCase(5);
            }

            // Modify the property
            CSharpUtilities.SetProperty(property, value, createdUser);

            // Execute:
            Assert.DoesNotThrow(() =>
            {
                Helper.AdminStore.UpdateUser(_adminUser, createdUser);
            }, "'PUT {0}' should return 200 OK for a valid session token!", USER_PATH_ID);

            if (property == "InstanceAdminRoleId" && value != null)
            {
                // Updating the user to an Instance Administrator changes the user license type to author
                createdUser.LicenseType = LicenseLevel.Author;
            }

            InstanceUser updatedUser = null;

            Assert.DoesNotThrow(() =>
            {
                updatedUser = Helper.AdminStore.GetUserById(_adminUser, createdUserId);
            }, "'GET {0}' should return 200 OK for a valid session token!", USER_PATH_ID);

            // Verify:
            // Update CurrentVersion in CreatedUser for comparison
            createdUser.CurrentVersion++;

            AdminStoreHelper.AssertAreEqual(createdUser, updatedUser);
        }

        [TestCase(InstanceAdminRole.AssignInstanceAdministrators)]
        [TestCase(InstanceAdminRole.DefaultInstanceAdministrator)]
        [TestCase(InstanceAdminRole.ProvisionUsers)]
        [Description("Create and add an instance user. Modify the user with another user that has " +
                     "permission to manage users. Verify that the user was modified.")]
        [TestRail(303611)]
        public void UpdateInstanceUser_PermissionsToManageUsers_UserUpdatesCorrectly(InstanceAdminRole adminRole)
        {
            // Setup:
            var userPermissionsToManageUsers = Helper.CreateUserAndAuthenticate(
                TestHelper.AuthenticationTokenTypes.AccessControlToken, adminRole);

            var createdUser = AdminStoreHelper.GenerateRandomInstanceUser();

            int createdUserId = 0;

            Assert.DoesNotThrow(() =>
            {
                createdUserId = Helper.AdminStore.AddUser(_adminUser, createdUser);
            }, "'POST {0}' should return 201 OK for a valid session token!", USER_PATH);

            // Update user Id with returned value and incremented version
            createdUser.Id = createdUserId;
            createdUser.CurrentVersion++;

            // Modify any field
            createdUser.DisplayName = "Modified Display Name";

            // Execute:
            Assert.DoesNotThrow(() =>
            {
                Helper.AdminStore.UpdateUser(userPermissionsToManageUsers, createdUser);
            }, "'PUT {0}' should return 200 OK for a valid session token!", USER_PATH_ID);

            InstanceUser addedUser = null;

            Assert.DoesNotThrow(() =>
            {
                addedUser = Helper.AdminStore.GetUserById(userPermissionsToManageUsers, createdUserId);
            }, "'GET {0}' should return 200 OK for a valid session token!", USER_PATH_ID);

            // Verify:
            // CurrentVersion in CreatedUser for comparison
            createdUser.CurrentVersion++;

            AdminStoreHelper.AssertAreEqual(createdUser, addedUser);
        }

        [TestCase]
        [Description("Create and add an instance user. Remove the source field. Add the user. " +
             "Verify that the user is updated.")]
        [TestRail(303413)]
        public void UpdateInstanceUser_MissingSource_UserUpdatesCorrectly()
        {
            // Setup:
            var createdUser = AdminStoreHelper.GenerateRandomInstanceUser();

            int createdUserId = 0;

            Assert.DoesNotThrow(() =>
            {
                createdUserId = Helper.AdminStore.AddUser(_adminUser, createdUser);
            }, "'POST {0}' should return 201 OK for a valid session token!", USER_PATH);

            // Update user Id with returned value and incremented version
            createdUser.Id = createdUserId;
            createdUser.CurrentVersion++;

            // Source defaults to Database, so we need to remove it here
            createdUser.Source = null;

            // Execute:
            Assert.DoesNotThrow(() =>
            {
                Helper.AdminStore.UpdateUser(_adminUser, createdUser);
            }, "'PUT {0}' should return 200 OK for a valid session token!", USER_PATH_ID);

            InstanceUser addedUser = null;

            Assert.DoesNotThrow(() =>
            {
                addedUser = Helper.AdminStore.GetUserById(_adminUser, createdUserId);
            }, "'GET {0}' should return 200 OK for a valid session token!", USER_PATH_ID);

            // Verify:
            // Update CurrentVersion and Source in CreatedUser for comparison
            createdUser.CurrentVersion++;
            createdUser.Source = UserSource.Database;

            AdminStoreHelper.AssertAreEqual(createdUser, addedUser);
        }

        #endregion 200 OK Tests

        #region 400 Bad Request Tests

        [TestCase]
        [Description("Create and add an instance user. Try to update the user without sending the user object. " +
            "Verify that 400 Bad Request is returned.")]
        [TestRail(303655)]
        public void UpdateInstanceUser_UserMissing_400BadRequest()
        {
            // Setup:
            var createdUser = AdminStoreHelper.GenerateRandomInstanceUser();

            int createdUserId = 0;

            Assert.DoesNotThrow(() =>
            {
                createdUserId = Helper.AdminStore.AddUser(_adminUser, createdUser);
            }, "'POST {0}' should return 201 OK for a valid session token!", USER_PATH);

            // Update user Id with returned value and incremented version
            createdUser.Id = createdUserId;
            createdUser.CurrentVersion++;

            // Execute:
            var ex = Assert.Throws<Http400BadRequestException>(() => { UpdateUserWithNoBodyInRestCall(_adminUser, createdUser); },
                "'PUT {0}' should return 400 Bad Request!", USER_PATH_ID);

            // Verify:
            TestHelper.ValidateServiceErrorMessage(ex.RestResponse, InstanceAdminErrorMessages.UserModelIsEmpty);
        }

        [TestCase(0)]
        [TestCase(-1)]
        [Description("Create and add an instance user. Try to update the user with an incorrect Id. " +
                     "Verify that 400 Bad Request is returned.")]
        [TestRail(303656)]
        public void UpdateInstanceUser_InvalidUserId_400BadRequest(int invalidId)
        {
            // Setup:
            var createdUser = AdminStoreHelper.GenerateRandomInstanceUser();

            Assert.DoesNotThrow(() =>
            {
                Helper.AdminStore.AddUser(_adminUser, createdUser);
            }, "'POST {0}' should return 201 OK for a valid session token!", USER_PATH);

            // Update user Id with returned value and incremented version
            createdUser.Id = invalidId;
            createdUser.CurrentVersion++;

            //Execute:
            var ex = Assert.Throws<Http400BadRequestException>(() => { Helper.AdminStore.UpdateUser(_adminUser, createdUser); },
                "'PUT {0}' should return 400 Bad Request!", USER_PATH_ID);

            // Verify:
            TestHelper.ValidateServiceErrorMessage(ex.RestResponse, InstanceAdminErrorMessages.IncorrectUserId);
        }

        [TestCase("", Description = "Login is empty")]
        [TestCase(null, Description = "Login is null")]
        [Description("Create and add a default instance user. Remove the login. Update the user. " +
                     "Verify that 400 Bad Request is returned.")]
        [TestRail(303409)]
        public void UpdateInstanceUser_MissingLogin_400BadRequest(string login)
        {
            UpdateDefaultInstanceUserWithInvalidPropertyVerify400BadRequest(
                _adminUser,
                "Login",
                login,
                InstanceAdminErrorMessages.LoginRequired);
        }

        [TestCase((uint)3, Description = "Minimum 4 characters")]
        [TestCase((uint)256, Description = "Maximum 255 characters")]
        [Description("Create and add a default instance user. Modify the login to contain an invalid" +
                     "value. Update the user. Verify that 400 Bad Request is returned.")]
        [TestRail(303410)]
        public void UpdateInstanceUser_InvalidLogin_400BadRequest(uint numCharacters)
        {
            UpdateDefaultInstanceUserWithInvalidPropertyVerify400BadRequest(
                _adminUser,
                "Login",
                RandomGenerator.RandomAlphaNumericUpperAndLowerCase(numCharacters),
                InstanceAdminErrorMessages.LoginFieldLimitation);
        }

        [TestCase("Raptor_RC_ExpiredUser", Description = "Raptor_RC_ExpiredUser")]
        [TestCase("Raptor_RC_UserLogout", Description = "Raptor_RC_UserLogout")]
        [TestCase("Raptor_RC_InvalidUser", Description = "Raptor_RC_InvalidUser")]
        [Description("Create and add a default instance. Modify the login to a reserved login. " +
             "Verify that 400 Bad Request is returned.")]
        [TestRail(303662)]
        public void UpdateInstanceUser_ReservedLogin_400BadRequest(string reservedLogin)
        {
            UpdateDefaultInstanceUserWithInvalidPropertyVerify400BadRequest(
                _adminUser,
                "Login",
                reservedLogin,
                InstanceAdminErrorMessages.LoginInvalid);
        }

        [TestCase]
        [Description("Create and add a default instance. Modify the login to an existing login. " +
                     "Verify that 400 Bad Request is returned.")]
        [TestRail(303677)]
        public void UpdateInstanceUser_ModifiedLoginAlreadyExists_400BadRequest()
        {
            // Setup:
            var firstUser = AdminStoreHelper.GenerateRandomInstanceUser();

            Helper.AdminStore.AddUser(_adminUser, firstUser);

            var secondUser = AdminStoreHelper.GenerateRandomInstanceUser();

            var secondUserId = Helper.AdminStore.AddUser(_adminUser, secondUser);

            // Update user Id with returned value and incremented version
            secondUser.Id = secondUserId;
            secondUser.CurrentVersion++;

            secondUser.Login = firstUser.Login;

            // Execute:
            var ex = Assert.Throws<Http400BadRequestException>(() => { Helper.AdminStore.UpdateUser(_adminUser, secondUser); },
                "'PUT {0}' should return 400 Bad Request!", USER_PATH_ID);

            // Verify:
            TestHelper.ValidateServiceErrorMessage(ex.RestResponse, InstanceAdminErrorMessages.LoginNameUnique);
        }

        [TestCase("", Description = "DisplayName is empty")]
        [TestCase(null, Description = "DisplayName is null")]
        [Description("Create and add a default instance user. Remove the display name. Update the user. " +
                     "Verify that 400 Bad Request is returned.")]
        [TestRail(303411)]
        public void UpdateInstanceUser_MissingDisplayName_400BadRequest(string displayName)
        {
            UpdateDefaultInstanceUserWithInvalidPropertyVerify400BadRequest(
                _adminUser,
                "DisplayName",
                displayName,
                InstanceAdminErrorMessages.DisplayNameRequired);
        }

        [TestCase((uint)1, Description = "Minimum 2 characters")]
        [TestCase((uint)256, Description = "Maximum 255 characters")]
        [Description("Create and add a default instance user. Modify the display name of the user to an invalid value. " +
                     "Update the user. Verify 400 Bad Request is returned.")]
        [TestRail(303610)]
        public void UpdateInstanceUser_InvalidDisplayName_400BadRequest(uint numCharacters)
        {
            UpdateDefaultInstanceUserWithInvalidPropertyVerify400BadRequest(
                _adminUser,
                "DisplayName",
                RandomGenerator.RandomAlphaNumericUpperAndLowerCase(numCharacters),
                InstanceAdminErrorMessages.DisplayNameFieldLimitation);
        }

        [TestCase((uint)3, InstanceAdminErrorMessages.EmailFieldLimitation, Description = "Minimum 4 characters")]
        [TestCase((uint)256, InstanceAdminErrorMessages.EmailFieldLimitation, Description = "Maximum 255 characters")]
        [TestCase("@domain.com", InstanceAdminErrorMessages.EmailFormatIncorrect, Description = "No local part")]
        [TestCase("domain.com", InstanceAdminErrorMessages.EmailFormatIncorrect, Description = "No @ character")]
        [TestCase("user@name@domain.com", InstanceAdminErrorMessages.EmailFormatIncorrect, Description = "Only one @ allowed")]
        [TestCase("user@domain..com", InstanceAdminErrorMessages.EmailFormatIncorrect, Description = "Double dot after @")]
        [TestCase("user\"Name\"@domain.com", InstanceAdminErrorMessages.EmailFormatIncorrect, Description = "Quotes must be dot separated")]
        [TestCase("user name@domain.com", InstanceAdminErrorMessages.EmailFormatIncorrect, Description = "No spaces")]
        [TestCase("user\\name@domain.com", InstanceAdminErrorMessages.EmailFormatIncorrect, Description = "No backslashes")]
        [TestCase("user(name@domain.com", InstanceAdminErrorMessages.EmailFormatIncorrect, Description = "No left parenthesis")]
        [TestCase("user)name@domain.com", InstanceAdminErrorMessages.EmailFormatIncorrect, Description = "No right parenthesis")]
        [TestCase("user,name@domain.com", InstanceAdminErrorMessages.EmailFormatIncorrect, Description = "No comma")]
        [TestCase("user:name@domain.com", InstanceAdminErrorMessages.EmailFormatIncorrect, Description = "No colon")]
        [TestCase("user;name@domain.com", InstanceAdminErrorMessages.EmailFormatIncorrect, Description = "No semi-colon")]
        [TestCase("user<name@domain.com", InstanceAdminErrorMessages.EmailFormatIncorrect, Description = "No less than")]
        [TestCase("user>name@domain.com", InstanceAdminErrorMessages.EmailFormatIncorrect, Description = "No greater than")]
        [TestCase("user[name@domain.com", InstanceAdminErrorMessages.EmailFormatIncorrect, Description = "No left bracket")]
        [TestCase("user]name@domain.com", InstanceAdminErrorMessages.EmailFormatIncorrect, Description = "No right bracket")]
        [Description("Create and add an instance user with a valid email. Try to update with an invalid email. " +
                     "Verify that 400 Bad Request is returned.")]
        [TestRail(303412)]
        public void UpdateInstanceUser_InvalidEmail_400BadRequest(object value, string errorMessage)
        {
            string emailAddress;

            if (value is uint)
            {
                emailAddress = RandomGenerator.RandomAlphaNumericUpperAndLowerCase((uint)value);
            }
            else
            {
                emailAddress = (string)value;
            }

            UpdateDefaultInstanceUserWithInvalidPropertyVerify400BadRequest(
                _adminUser,
                "Email",
                emailAddress,
                errorMessage
                );
        }

        [TestCase((UserSource)0xFF, InstanceAdminErrorMessages.CreateOnlyDatabaseUsers)]
        [TestCase(UserSource.Unknown, InstanceAdminErrorMessages.CreateOnlyDatabaseUsers)]
        [TestCase(UserSource.Windows, InstanceAdminErrorMessages.CreateOnlyDatabaseUsers)]
        [Description("Create and add a default instance user.  Modify the source to an invalid value. " +
                     "Update the user. Verify that 400 Bad Request is returned.")]
        [TestRail(303414)]
        public void UpdateInstanceUser_InvalidSource_400BadRequest(UserSource? source, string errorMessage)
        {
            UpdateDefaultInstanceUserWithInvalidPropertyVerify400BadRequest(
                _adminUser,
                "Source",
                source,
                errorMessage);
        }

        [TestCase((uint)1, Description = "Minimum 2 characters")]
        [TestCase((uint)256, Description = "Maximum 255 characters")]
        [Description("Create and add a default instance user.  Modify the first name to an invalid value. " +
                     "Update the user. Verify that 400 Bad Request is returned.")]
        [TestRail(303418)]
        public void UpdateInstanceUser_InvalidFirstName_400BadRequest(uint numCharacters)
        {
            UpdateDefaultInstanceUserWithInvalidPropertyVerify400BadRequest(
                _adminUser,
                "FirstName",
                RandomGenerator.RandomAlphaNumericUpperAndLowerCase(numCharacters),
                InstanceAdminErrorMessages.FirstNameFieldLimitation);
        }

        [TestCase("", Description = "FirstName is empty")]
        [TestCase(null, Description = "FirstName is null")]
        [Description("Create and add a default instance user.  Remove the first name. " +
                     "Update the user. Verify that 400 Bad Request is returned.")]
        [TestRail(303612)]
        public void UpdateInstanceUser_MissingFirstName_400BadRequest(string firstName)
        {
            UpdateDefaultInstanceUserWithInvalidPropertyVerify400BadRequest(
                _adminUser,
                "FirstName",
                firstName,
                InstanceAdminErrorMessages.FirstNameRequired);
        }

        [TestCase((uint)1, Description = "Minimum 2 characters")]
        [TestCase((uint)256, Description = "Maximum 255 characters")]
        [Description("Create and add a default instance user.  Modify the last name to an invalid value. " +
                     "Update the user. Verify that 400 Bad Request is returned.")]
        [TestRail(3034197)]
        public void UpdateInstanceUser_InvalidLastName_400BadRequest(uint numCharacters)
        {
            UpdateDefaultInstanceUserWithInvalidPropertyVerify400BadRequest(
                _adminUser,
                "LastName",
                RandomGenerator.RandomAlphaNumericUpperAndLowerCase(numCharacters),
                InstanceAdminErrorMessages.LastNameFieldLimitation);
        }

        [TestCase("", Description = "LastName is empty")]
        [TestCase(null, Description = "LastName is null")]
        [Description("Create and add a default instance user.  Remove the last name. " +
                     "Update the user. Verify that 400 Bad Request is returned.")]
        [TestRail(303613)]
        public void UpdateInstanceUser_MissingLastName_400BadRequest(string lastName)
        {
            UpdateDefaultInstanceUserWithInvalidPropertyVerify400BadRequest(
                _adminUser,
                "LastName",
                lastName,
                InstanceAdminErrorMessages.LastNameRequired);
        }

        [TestCase((uint)256, Description = "Maximum 255 characters")]
        [Description("Create and add a default instance user.  Modify the department to an invalid value. " +
                     "Update the user. Verify that 400 Bad Request is returned.")]
        [TestRail(303420)]
        public void UpdateInstanceUser_InvalidDepartment_400BadRequest(uint numCharacters)
        {
            UpdateDefaultInstanceUserWithInvalidPropertyVerify400BadRequest(
                _adminUser,
                "Department",
                RandomGenerator.RandomAlphaNumericUpperAndLowerCase(numCharacters),
                InstanceAdminErrorMessages.DepartmentFieldLimitation);
        }

        [TestCase((uint)1, Description = "Minimum 2 characters")]
        [TestCase((uint)256, Description = "Maximum 255 characters")]
        [Description("Create and add a default instance user.  Modify the title to an invalid value. " +
                     "Update the user. Verify that 400 Bad Request is returned.")]
        [TestRail(303421)]
        public void UpdateInstanceUser_InvalidTitle_400BadRequest(uint numCharacters)
        {
            UpdateDefaultInstanceUserWithInvalidPropertyVerify400BadRequest(
                _adminUser,
                "Title",
                RandomGenerator.RandomAlphaNumericUpperAndLowerCase(numCharacters),
                InstanceAdminErrorMessages.TitleFieldLimitation);
        }

        #endregion 400 Bad Request Tests

        #region 401 Unauthorized Tests

        [TestCase("")]
        [TestCase(CommonConstants.InvalidToken)]
        [Description("Create and add an instance user. Try to update the user using an invalid token header. " +
             "Verify that 401 Unauthorized is returned.")]
        [TestRail(303404)]
        public void UpdateInstanceUser_InvalidTokenHeader_401Unauthorized(string tokenString)
        {
            // Setup:
            var createdUser = AdminStoreHelper.GenerateRandomInstanceUser();

            int createdUserId = 0;

            Assert.DoesNotThrow(() =>
            {
                createdUserId = Helper.AdminStore.AddUser(_adminUser, createdUser);
            }, "'POST {0}' should return 201 OK for a valid session token!", USER_PATH);

            // Update user Id with returned value and incremented version
            createdUser.Id = createdUserId;
            createdUser.CurrentVersion++;

            var userWithInvalidTokenHeader = Helper.CreateUserWithInvalidToken(
                TestHelper.AuthenticationTokenTypes.AccessControlToken, 
                InstanceAdminRole.DefaultInstanceAdministrator, 
                badToken: tokenString);

            // Execute:
            var ex = Assert.Throws<Http401UnauthorizedException>(() =>
            {
                Helper.AdminStore.UpdateUser(userWithInvalidTokenHeader, createdUser);
            }, "'PUT {0}' should return 401 Unauthorized with invalid token header!", USER_PATH_ID);

            // Verify:
            TestHelper.ValidateServiceErrorMessage(ex.RestResponse, "Token is invalid.");
        }

        #endregion 401 Unauthorized Tests

        #region 403 Forbidden Tests

        [TestCase(null)]
        [TestCase(InstanceAdminRole.AdministerALLProjects)]
        [TestCase(InstanceAdminRole.BlueprintAnalytics)]
        [TestCase(InstanceAdminRole.Email_ActiveDirectory_SAMLSettings)]
        [TestCase(InstanceAdminRole.InstanceStandardsManager)]
        [TestCase(InstanceAdminRole.LogGatheringAndLicenseReporting)]
        [TestCase(InstanceAdminRole.ManageAdministratorRoles)]
        [TestCase(InstanceAdminRole.ProvisionProjects)]
        [Description("Create and add an instance user.  Try to update the user with another user that does not have" +
             "permission to manage users. Verify that 401 Unauthorized is returned.")]
        [TestRail(303449)]
        public void UpdateInstanceUser_NoPermissionsToManageUsers_403Forbidden(InstanceAdminRole? adminRole)
        {
            // Setup:
            var createdUser = AdminStoreHelper.GenerateRandomInstanceUser();

            int createdUserId = 0;

            Assert.DoesNotThrow(() =>
            {
                createdUserId = Helper.AdminStore.AddUser(_adminUser, createdUser);
            }, "'POST {0}' should return 201 OK for a valid session token!", USER_PATH);

            // Update user Id with returned value and incremented version
            createdUser.Id = createdUserId;
            createdUser.CurrentVersion++;

            var userWithNoPermissionsToManageUsers = Helper.CreateUserAndAuthenticate(
                TestHelper.AuthenticationTokenTypes.AccessControlToken, adminRole);

            // Execute:
            var ex = Assert.Throws<Http403ForbiddenException>(() =>
            {
                Helper.AdminStore.UpdateUser(userWithNoPermissionsToManageUsers, createdUser);
            },
            "'PUT {0}' should return 403 Forbidden when the user updating the user has no permissions to manage users!", USER_PATH_ID);

            // Verify:
            TestHelper.ValidateServiceErrorMessage(ex.RestResponse, "The user does not have permissions.");
        }

        #endregion 403 Forbidden Tests

        #region 404 Not Found Tests

        [TestCase]
        [Description("Create an instance user. Try to update a non-existing user. " +
                     "Verify that 404 Not Found is returned.")]
        [TestRail(303422)]
        public void UpdateInstanceUser_UserDoesntExist_404NotFound()
        {
            // Setup:
            var createdUser = AdminStoreHelper.GenerateRandomInstanceUser();

            createdUser.Id = int.MaxValue;

            // Execute:
            var ex = Assert.Throws<Http404NotFoundException>(() =>
            {
                Helper.AdminStore.UpdateUser(_adminUser, createdUser);
            }, "'PUT {0}' should return 404 Not Found for nonexistent user!", USER_PATH_ID);

            // Verify:
            TestHelper.ValidateServiceErrorMessage(ex.RestResponse, InstanceAdminErrorMessages.UserNotExist);
        }

        [Explicit(IgnoreReasons.UnderDevelopmentQaDev)]
        [TestCase]
        [Description("Create and add an instance user. Delete the user.  Try to update the deleted user. " +
             "Verify that 404 Not Found is returned.")]
        [TestRail(303454)]
        public static void UpdateInstanceUser_UserDeleted_404NotFound()
        {
            throw new NotImplementedException();
        }

        #endregion 404 Not Found Tests

        #region 409 Conflict Tests

        [Description("Create and add an instance user.  Try to update the user using an incorrect CurrentVersion. " +
                     "Verify that 409 Conflict is returned.")]
        [TestRail(303425)]
        public void UpdateInstanceUser_CurrentVersionMismatch_409Conflict()
        {
            // Setup:
            var createdUser = AdminStoreHelper.GenerateRandomInstanceUser();

            int createdUserId = 0;

            Assert.DoesNotThrow(() =>
            {
                createdUserId = Helper.AdminStore.AddUser(_adminUser, createdUser);
            }, "'POST {0}' should return 201 OK for a valid session token!", USER_PATH);

            // Update user Id with returned value and but don't update the current version
            createdUser.Id = createdUserId;

            // Execute:
            var ex = Assert.Throws<Http409ConflictException>(() =>
            {
                Helper.AdminStore.UpdateUser(_adminUser, createdUser);
            },
            "'PUT {0}' should return 409 Conflict when ttrying to update a user with an incorrect Current Version!", USER_PATH_ID);

            // Verify:
            TestHelper.ValidateServiceErrorMessage(ex.RestResponse, InstanceAdminErrorMessages.UserVersionsNotEqual);
        }

        #endregion 409 Conflict Tests

        #region Private Methods

        /// <summary>
        /// Updates a default Instance User with an invalid property and verifies 400 Bad Request when trying to add the user.
        /// </summary>
        /// <param name="adminUser">The user adding the created user.</param>
        /// <param name="property">The property of the user to be made invalid.</param>
        /// <param name="propertyValue">The invalid property value.</param>
        /// <param name="expectedErrorMessage">The expected error message returned from the server.</param>
        private void UpdateDefaultInstanceUserWithInvalidPropertyVerify400BadRequest(
            IUser adminUser,
            string property,
            object propertyValue,
            string expectedErrorMessage)
        {
            // Setup:
            var createdUser = AdminStoreHelper.GenerateRandomInstanceUser();

            int createdUserId = 0;

            Assert.DoesNotThrow(() =>
            {
                createdUserId = Helper.AdminStore.AddUser(_adminUser, createdUser);
            }, "'POST {0}' should return 201 OK for a valid session token!", USER_PATH);

            // Update user Id with returned value and incremented version
            createdUser.Id = createdUserId;
            createdUser.CurrentVersion++;

            // Update with invalid value
            if (property != null)
            {
                CSharpUtilities.SetProperty(property, propertyValue, createdUser);
            }

            // Execute:
            var ex = Assert.Throws<Http400BadRequestException>(() => { Helper.AdminStore.UpdateUser(adminUser, createdUser); },
                "'PUT {0}' should return 400 Bad Request!", USER_PATH_ID);

            // Verify:
            TestHelper.ValidateServiceErrorMessage(ex.RestResponse, expectedErrorMessage);
        }

        /// <summary>
        /// Attempts to update a user with a null body in the PUT call
        /// </summary>
        /// <param name="adminUser">The user attempting to perform the update.</param>
        /// <param name="user">The user to be updated.</param>
        private void UpdateUserWithNoBodyInRestCall(IUser adminUser, InstanceUser user)
        {
            ThrowIf.ArgumentNull(user, nameof(user));

            var restApi = new RestApiFacade(Helper.ArtifactStore.Address, adminUser?.Token?.AccessControlToken);
            string path = I18NHelper.FormatInvariant(RestPaths.Svc.AdminStore.Users.USERS_id_, user.Id);

            try
            {
                Logger.WriteInfo("Updating user with Id: {0}", user.Id);

                restApi.SendRequestAndGetResponse<InstanceUser>(
                    path,
                    RestRequestMethod.PUT,
                    bodyObject: null,
                    expectedStatusCodes: new List<HttpStatusCode> { HttpStatusCode.OK });
            }
            catch (WebException ex)
            {
                Logger.WriteError("Content = '{0}'", restApi.Content);
                Logger.WriteError("Error while performing UpdateUser - {0}", ex.Message);
                throw;
            }
        }

        #endregion Private Methods
    }
}