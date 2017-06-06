﻿using System;
using System.Collections.Generic;
using System.Linq;
using Common;
using CustomAttributes;
using Helper;
using Model;
using Model.Factories;
using NUnit.Framework;
using TestCommon;
using Utilities;
using Utilities.Factories;

namespace AdminStoreTests.UsersTests
{
    [TestFixture]
    [Category(Categories.AdminStore)]
    public class ChangePasswordTests : TestBase
    {
        private const uint MinPasswordLength = AdminStoreHelper.MinPasswordLength;
        private const uint MaxPasswordLength = AdminStoreHelper.MaxPasswordLength;

        private const string REST_PATH = RestPaths.Svc.AdminStore.Users.RESET;
        private const string CANNOTUSELASTPASSWORDS = "CannotUseLastPasswords";

        private IUser _adminUser = null;

        #region Setup and Cleanup

        [SetUp]
        public void SetUp()
        {
            Helper = new TestHelper();

            _adminUser = Helper.CreateUserAndAddToDatabase(
                // TODO: Improve CreateUser(UserSource source = UserSource.Database) to handle special charactors properly.
                // After that, replace belows with:
                // - username: CreateValidPassword(MinPasswordLength)
                // - displayname: CreateValidPassword(MinPasswordLength)
                username: RandomGenerator.RandomAlphaNumeric(MinPasswordLength),
                password: AdminStoreHelper.GenerateValidPassword(),
                displayname: RandomGenerator.RandomAlphaNumeric(MinPasswordLength)
                );
        }

        [TearDown]
        public void TearDown()
        {
            Helper?.Dispose();
        }

        #endregion Setup and Cleanup

        #region 200 OK Tests

        [TestCase(MinPasswordLength)]
        [TestCase(MaxPasswordLength)]
        [TestRail(103064)]
        [Description("Reset a user's password to a valid new password.  The password should be reset successfully and the user should be able to login with the new password.")]
        public void ResetUserPassword_SendValidNewPassword_VerifyUserCanLoginWithNewPassword(uint length)
        {
            // Setup: generate a valid password.
            string newPassword = TestHelper.CreateValidPassword(length, MinPasswordLength, MaxPasswordLength);

            // Execute: change the user's password.
            Assert.DoesNotThrow(() =>
            {
                Helper.AdminStore.ChangePassword(_adminUser, newPassword);
            }, "'POST {0}' failed when we passed a valid username & password!", REST_PATH);

            // Verify: make sure user can login with the new password.
            Helper.VerifyLogin(_adminUser.Username, newPassword);
        }

        [TestCase(MinPasswordLength)]
        [TestCase(MaxPasswordLength)]
        [TestRail(234571)]
        [Description("Reset the user's password after 24-hours password reset cooldown period." +
            "Verify that password reset works and that the user can login with the changed password.")]
        public void ResetUserPassword_ChangingPasswordAfter24HoursCooldown_VerifyResetUserPasswordWorks(uint length)
        {
            // Setup: Reset the password with valid new password
            SetUserWithValidPassword(_adminUser, length);

            // Execute: Attempt to change the password again after the 24-hours password reset cooldown period.
            string newPassword = TestHelper.CreateValidPassword(length, MinPasswordLength, MaxPasswordLength);
            Assert.DoesNotThrow(() => Helper.AdminStore.ChangePassword(user: _adminUser, newPassword: newPassword),
                "'POST {0}' failed when user tried to reset the password after 24-hours password reset cooldown period.",
                REST_PATH);

            // Verify: Make sure the user can login with the new password.
            Helper.VerifyLogin(_adminUser.Username, newPassword);
        }

        [TestCase(MaxPasswordLength, "3")]
        [TestRail(266426)]
        [Description("Reset the user's password with the password which was used before but not in the password history. " +
            "Verify that password reset works and that the user can login with the updated password.")]
        public void ResetUserPassword_UsingPreviousPasswordGreaterThanPasswordHistoryLimit_PasswordIsChanged(uint length, string cannotUseLastPasswords)
        {
            // Setup: Reset the password with valid new password.
            // Update the CannotUseLastPasswords value by setting new value and generate list of valid passwords one 
            // more than the password history limit value
            SetUserWithValidPassword(_adminUser, length);

            int resetExecutionCount = cannotUseLastPasswords.ToInt32Invariant() + 1;

            List<string> usedValidPasswords = new List<string> { _adminUser.Password };

            for (int i = 0; i < resetExecutionCount; i++)
            {
                usedValidPasswords.Add(TestHelper.CreateValidPassword(length, MinPasswordLength, MaxPasswordLength));
            }

            var originalCannotUseLastPassword = TestHelper.GetValueFromInstancesTable(CANNOTUSELASTPASSWORDS);
            TestHelper.UpdateValueFromInstancesTable(CANNOTUSELASTPASSWORDS, cannotUseLastPasswords);

            try
            {
                // Reset password cannotUseLastPasswords + 1 times so that the first used password from
                // usedValidPasswords can be reused
                for (int i = 1; i < resetExecutionCount + 1; i++)
                {
                    Helper.AdminStore.ChangePassword(_adminUser, usedValidPasswords[i]);
                    _adminUser.Password = usedValidPasswords[i];
                    SimulateTimeSpentAfterLastPasswordUpdate(_adminUser);
                }

                // Execute: Change the password using the very first used password which was used before but not in the password history.
                Assert.DoesNotThrow(() => Helper.AdminStore.ChangePassword(user: _adminUser, newPassword: usedValidPasswords.First()),
                    "'POST {0}' failed when user tried to reset password with the password which used before but not in the password history.",
                    REST_PATH);

                // Verify: Make sure the user can still login with the updated password.
                Helper.VerifyLogin(_adminUser.Username, usedValidPasswords.First());
            }
            finally
            {
                // Restore CannotUserLastPasswords back to original value.
                TestHelper.UpdateValueFromInstancesTable(CANNOTUSELASTPASSWORDS, originalCannotUseLastPassword);
            }
        }

        [TestCase(MinPasswordLength)]
        [TestRail(266427)]
        [Description("Disable CannotUseLastPassword by setting its value to '0' and Verify that CannotUseLastPasswords " +
            "feature is disabled by resetting user's password with the same password twice.")]
        public void ResetUserPassword_UsingPreviousPasswordWhenPasswordHistoryLimitDisabled_PasswordIsChanged(uint length)
        {
            // Setup: Reset the password with valid new password
            SetUserWithValidPassword(_adminUser, length);

            // Setup: Disable the CannotUseLastPasswords feature by setting the value to '0' 
            var originalCannotUseLastPassword = TestHelper.GetValueFromInstancesTable(CANNOTUSELASTPASSWORDS);
            TestHelper.UpdateValueFromInstancesTable(CANNOTUSELASTPASSWORDS, "0");

            try
            {
                // Setup: Reset the password with first updated password
                string firstUpdatedPassword = TestHelper.CreateValidPassword(length, MinPasswordLength, MaxPasswordLength);
                ResetPassword25HoursAgo(_adminUser, firstUpdatedPassword);

                // Setup: Reset the password with second updated password
                string secondUpdatedPassword = TestHelper.CreateValidPassword(length, MinPasswordLength, MaxPasswordLength);
                ResetPassword25HoursAgo(_adminUser, secondUpdatedPassword);

                // Execute: Attempt to change the password with the first updated password
                Assert.DoesNotThrow(() => Helper.AdminStore.ChangePassword(user: _adminUser, newPassword: firstUpdatedPassword),
                    "'POST {0}' failed when user tried to reset the password with previously used password when PasswordHistoryLimit is disabled.",
                    REST_PATH);

                // Verify: Make sure the user can login with the new password, which is same as the first changed value.
                Helper.VerifyLogin(_adminUser.Username, firstUpdatedPassword);
            }
            finally
            {
                // Restore CannotUserLastPasswords back to original value.
                TestHelper.UpdateValueFromInstancesTable(CANNOTUSELASTPASSWORDS, originalCannotUseLastPassword);
            }
        }

        #endregion 200 OK Tests

        #region 400 Bad Request Tests

        [TestCase(null)]
        [TestCase("")]
        [TestRail(103490)]
        [Description("Try to reset the user's password to a null or blank password." +
            "AdminStore should return a 400 error and the user should still be able to login with their old password.")]
        public void ResetUserPassword_SendBlankNewPassword_Verify400Error(string newPassword)
        {
            // Execute: try to change the user's password to an invalid password.
            var ex = Assert.Throws<Http400BadRequestException>(() =>
            {
                Helper.AdminStore.ChangePassword(_adminUser, newPassword);
            }, "'POST {0}' should get a 400 Bad Request when passing a null or empty new password!", REST_PATH);

            // Verify: make sure user can still login with their old password.
            Helper.VerifyLogin(_adminUser.Username, _adminUser.Password);

            const string expectedExceptionMessage = "Password reset failed, new password cannot be empty";
            TestHelper.ValidateServiceError(ex.RestResponse, ErrorCodes.EmptyPassword, expectedExceptionMessage);
        }

        [TestCase]
        [TestRail(103491)]
        [Description("Try to reset the user's password to the same password." +
            "AdminStore should return a 400 error and the user should still be able to login with their old password.")]
        public void ResetUserPassword_SendSamePassword_Verify400Error()
        {
            // Execute: try to change the user's password to the same password.
            var ex = Assert.Throws<Http400BadRequestException>(() =>
            {
                Helper.AdminStore.ChangePassword(_adminUser, _adminUser.Password);
            }, "'POST {0}' should get a 400 Bad Request when old and new passwords are the same!", REST_PATH);

            // Verify: make sure user can still login with their old password.
            Helper.VerifyLogin(_adminUser.Username, _adminUser.Password);

            const string expectedExceptionMessage = "Password reset failed, new password cannot be equal to the old one";
            TestHelper.ValidateServiceError(ex.RestResponse, ErrorCodes.SamePassword, expectedExceptionMessage);
        }

        [TestCase]
        [TestRail(103492)]
        [Description("Try to reset the user's password to a password that doesn't meet the complexity rules." +
            "AdminStore should return a 400 error and the user should still be able to login with their old password.")]
        public void ResetUserPassword_SendNonComplexPassword_Verify400Error()
        {
            // Setup: generate an invalid password.
            // The random password doesn't have special characters.
            string newPassword = RandomGenerator.RandomAlphaNumericUpperAndLowerCase(MinPasswordLength);

            // Execute: try to change the user's password to an invalid password.
            var ex = Assert.Throws<Http400BadRequestException>(() =>
            {
                Helper.AdminStore.ChangePassword(_adminUser, newPassword);
            }, "'POST {0}' should get a 400 Bad Request when the new password doesn't meet the complexity rules!", REST_PATH);

            // Verify: make sure user can still login with their old password.
            Helper.VerifyLogin(_adminUser.Username, _adminUser.Password);

            const string expectedExceptionMessage = "Password reset failed, new password is invalid";
            TestHelper.ValidateServiceError(ex.RestResponse, ErrorCodes.TooSimplePassword, expectedExceptionMessage);
        }

        [TestCase(MinPasswordLength - 1)]
        [TestCase(MaxPasswordLength + 1)]
        [TestRail(103493)]
        [Description("Try to reset the user's password to one that is too short or too long." +
            "AdminStore should return a 400 error and the user should still be able to login with their old password.")]
        public void ResetUserPassword_SendNewPasswordWithInvalidLength_Verify400Error(uint length)
        {
            // Setup: generate an invalid password.
            string newPassword = TestHelper.CreateValidPassword(length, MinPasswordLength, MaxPasswordLength, skipLengthRequirement: true);

            // Execute: try to change the user's password to an invalid password.
            var ex = Assert.Throws<Http400BadRequestException>(() =>
            {
                Helper.AdminStore.ChangePassword(_adminUser, newPassword);
            }, "'POST {0}' should get a 400 Bad Request when passing new password that is too short or too long!", REST_PATH);

            // Verify: make sure user can still login with their old password.
            Helper.VerifyLogin(_adminUser.Username, _adminUser.Password);

            const string expectedExceptionMessage = "Password reset failed, new password is invalid";
            TestHelper.ValidateServiceError(ex.RestResponse, ErrorCodes.TooSimplePassword, expectedExceptionMessage);
        }

        [TestCase]
        [TestRail(234560)]
        [Description("Try to reset the user's password to a string value identical to the username of the user." +
            "Verify that 400 BadRequest is returned and that the user still can login with its password.")]
        public void ResetUserPassword_SendUserNameAsNewPassword_400BadRequest()
        {
            // Execute:
            var ex = Assert.Throws<Http400BadRequestException>(
                () => Helper.AdminStore.ChangePassword(user: _adminUser, newPassword: _adminUser.Username),
                "'POST {0}' should get a 400 Bad Request when passing its user name as a new password!", REST_PATH);

            // Verify: Make sure the user can still login with their old password.
            Helper.VerifyLogin(_adminUser.Username, _adminUser.Password);

            const string expectedExceptionMessage = "Password reset failed, new password cannot be equal to login name";
            TestHelper.ValidateServiceError(ex.RestResponse, ErrorCodes.PasswordSameAsLogin, expectedExceptionMessage);
        }

        [TestCase]
        [TestRail(234570)]
        [Description("Try to reset the user's password to a string value identical to the user's display name." +
            "Verify that 400 BadRequest response and that the user still can login with its password.")]
        public void ResetUserPassword_SendDisplayNameAsNewPassword_400BadRequest()
        {
            // Execute:
            var ex = Assert.Throws<Http400BadRequestException>(
                () => Helper.AdminStore.ChangePassword(user: _adminUser, newPassword: _adminUser.DisplayName),
                "'POST {0}' should get a 400 Bad Request when passing its display name as a new password!", REST_PATH);

            // Verify: Make sure the user can still login with their old password.
            Helper.VerifyLogin(_adminUser.Username, _adminUser.Password);

            const string expectedExceptionMessage = "Password reset failed, new password cannot be equal to display name";
            TestHelper.ValidateServiceError(ex.RestResponse, ErrorCodes.PasswordSameAsDisplayName, expectedExceptionMessage);
        }

        [TestCase(MinPasswordLength, "2")]
        [TestRail(266428)]
        [Description("Reset the user's password with the previously used password which is in password history." +
            "Verify that 400 BadRequest response and that the user still can login with its current password.")]
        public void ResetUserPassword_UsingPreviousPasswordLessThanPasswordHistoryLimit_400BadRequest(uint length, string cannotUseLastPasswords)
        {
            // Setup: Reset the password with valid new password
            SetUserWithValidPassword(_adminUser, length);

            int resetExecutionCount = cannotUseLastPasswords.ToInt32Invariant() + 1;

            // Setup: Update the CannotUseLastPasswords by setting new value and generate list of valid passwords
            List<string> usedValidPasswords = new List<string>();
            for (int i = 0; i < resetExecutionCount; i++)
            {
                usedValidPasswords.Add(TestHelper.CreateValidPassword(length, MinPasswordLength, MaxPasswordLength));
            }

            var originalCannotUseLastPassword = TestHelper.GetValueFromInstancesTable(CANNOTUSELASTPASSWORDS);
            TestHelper.UpdateValueFromInstancesTable(CANNOTUSELASTPASSWORDS, cannotUseLastPasswords);

            try
            {
                // Setup: Reset password multiple times which equals current CannotUseLastPasswords value
                foreach (string password in usedValidPasswords)
                {
                    ResetPassword25HoursAgo(_adminUser, password);
                }

                // Execute: Attempt to change the password using the used password which is in password history
                var ex = Assert.Throws<Http400BadRequestException>(
                    () => Helper.AdminStore.ChangePassword(user: _adminUser, newPassword: usedValidPasswords.First()),
                    "'POST {0}' should get a 400 Bad Request when passing the used password which is in password history.",
                    REST_PATH);

                // Verify: Make sure the user can still login with their old password.
                Helper.VerifyLogin(_adminUser.Username, _adminUser.Password);

                const string expectedExceptionMessage = "The new password matches a previously used password.";
                TestHelper.ValidateServiceError(ex.RestResponse, ErrorCodes.PasswordAlreadyUsedPreviously, expectedExceptionMessage);
            }
            finally
            {
                // Restore CannotUserLastPasswords back to original value.
                TestHelper.UpdateValueFromInstancesTable(CANNOTUSELASTPASSWORDS, originalCannotUseLastPassword);
            }
        }

        #endregion 400 Bad Request Tests

        #region 401 Unauthorized Tests

        [TestCase]
        [TestRail(103066)]
        [Description("Try to reset the password of a user that doesn't exist.  AdminStore should return a 401 error.")]
        public void ResetUserPassword_SendNonExistentUser_Verify401Error()
        {
            // Setup: generate a valid password.
            string newPassword = TestHelper.CreateValidPassword(MinPasswordLength, MinPasswordLength, MaxPasswordLength);
            var missingUser = UserFactory.CreateUserOnly();

            // Execute & Verify: try to change the user's password.
            var ex = Assert.Throws<Http401UnauthorizedException>(() =>
            {
                Helper.AdminStore.ChangePassword(missingUser, newPassword);
            }, "'POST {0}' should get a 401 Unauthorized when passing a user that doesn't exist!", REST_PATH);

            const string expectedExceptionMessage = "Invalid username or password";
            TestHelper.ValidateServiceError(ex.RestResponse, ErrorCodes.InvalidCredentials, expectedExceptionMessage);
        }

        [TestCase]
        [TestRail(103527)]
        [Description("Try to reset the password of a user but don't send an old password.  " +
            "AdminStore should return a 401 error and the user should still be able to login with their old password.")]
        public void ResetUserPassword_DoNotSendOldPassword_Verify401Error()
        {
            // Setup: generate a valid password.
            string newPassword = TestHelper.CreateValidPassword(MinPasswordLength, MinPasswordLength, MaxPasswordLength);
            var userWithNullPassword = UserFactory.CreateCopyOfUser(_adminUser);
            userWithNullPassword.Password = null;

            // Execute & Verify: try to change the user's password.
            var ex = Assert.Throws<Http401UnauthorizedException>(() =>
            {
                Helper.AdminStore.ChangePassword(userWithNullPassword, newPassword);
            }, "'POST {0}' should get a 401 Unauthorized when passing a user with no Old Password!", REST_PATH);

            // Verify: make sure user can still login with their old password.
            Helper.VerifyLogin(_adminUser.Username, _adminUser.Password);

            const string expectedExceptionMessage = "Username and password cannot be empty";
            TestHelper.ValidateServiceError(ex.RestResponse, ErrorCodes.EmptyCredentials, expectedExceptionMessage);
        }

        [TestCase]
        [TestRail(103489)]
        [Description("Try to reset the user password to a valid new password, but specify the wrong old password.  " +
            "AdminStore should return a 401 error and the user should still be able to login with their old password.")]
        public void ResetUserPassword_SendWrongOldPassword_Verify401Error()
        {
            // Setup: generate a valid password.
            string newPassword = TestHelper.CreateValidPassword(MinPasswordLength, MinPasswordLength, MaxPasswordLength);
            var userWithBadPassword = UserFactory.CreateCopyOfUser(_adminUser);
            string wrongPassword = TestHelper.CreateValidPassword(MinPasswordLength, MinPasswordLength, MaxPasswordLength);
            userWithBadPassword.Password = wrongPassword;

            // Execute: try to change the user's password.
            var ex = Assert.Throws<Http401UnauthorizedException>(() =>
            {
                Helper.AdminStore.ChangePassword(userWithBadPassword, newPassword);
            }, "'POST {0}' should get a 401 Unauthorized when passing a user that doesn't exist!", REST_PATH);

            // Verify: make sure user can still login with their old password.
            Helper.VerifyLogin(_adminUser.Username, _adminUser.Password);

            const string expectedExceptionMessage = "Invalid username or password";
            TestHelper.ValidateServiceError(ex.RestResponse, ErrorCodes.InvalidCredentials, expectedExceptionMessage);
        }

        #endregion 401 Unauthorized Tests

        #region 409 Conflict Tests

        [TestCase(MinPasswordLength, 1)]
        [TestCase(MaxPasswordLength, 23)]
        [TestRail(234569)]
        [Description("Try to reset the user's password which was changed within 24-hours password reset cooldown period.  " +
                     "Verify that 409 Conflict is returned and that the user still can login with its password.")]
        public void ResetUserPassword_ChangingPasswordWithin24HoursCooldown_409Conflict(uint length, int hoursPassedAfterPasswordReset)
        {
            // Setup: Reset the password with valid new password
            string changedPassword = TestHelper.CreateValidPassword(length, MinPasswordLength, MaxPasswordLength);
            Helper.AdminStore.ChangePassword(_adminUser, changedPassword);
            _adminUser.Password = changedPassword;

            // Execute: Attempt to change the password again after resetting the password.
            SimulateTimeSpentAfterLastPasswordUpdate(_adminUser, timespent: hoursPassedAfterPasswordReset);

            string newPassword = TestHelper.CreateValidPassword(length, MinPasswordLength, MaxPasswordLength);
            var ex = Assert.Throws<Http409ConflictException>(
                () => Helper.AdminStore.ChangePassword(user: _adminUser, newPassword: newPassword),
                "'POST {0}' should get a 409 Conflict if the password was updated within 24-hours password reset cooldown period!",
                REST_PATH);

            // Verify: Make sure the user can login with their last successfully changed password.
            Helper.VerifyLogin(_adminUser.Username, changedPassword);

            const string expectedExceptionMessage = "Password reset failed, password reset cooldown in effect";
            TestHelper.ValidateServiceError(ex.RestResponse, ErrorCodes.ChangePasswordCooldownInEffect, expectedExceptionMessage);
        }

        #endregion 409 Conflict Tests

        #region Private functions

        /// <summary>
        /// Prepares a user with valid password
        /// </summary>
        /// <param name="user">user that will have the valid password</param>
        /// <param name="length">length of the password</param>
        private void SetUserWithValidPassword(IUser user, uint length)
        {
            ThrowIf.ArgumentNull(user, nameof(user));

            string successfullyChangedPassword = TestHelper.CreateValidPassword(length, MinPasswordLength, MaxPasswordLength);
            ResetPassword25HoursAgo(user, successfullyChangedPassword);
        }

        /// <summary>
        /// Resets password for the user and sets the time of last password change to 25h ago.
        /// </summary>
        /// <param name="user">User whose password is being changed.</param>
        /// <param name="password">The new password.</param>
        private void ResetPassword25HoursAgo(IUser user, string password)
        {
            ThrowIf.ArgumentNull(user, nameof(user));

            Helper.AdminStore.ChangePassword(user, password);
            user.Password = password;
            SimulateTimeSpentAfterLastPasswordUpdate(user);
        }

        /// <summary>
        /// Simulates the time spent after user changed his/her password
        /// </summary>
        /// <param name="user">user that changed the password.</param>
        /// <param name="timespent">hour that spent after tha last password update.</param>
        private static void SimulateTimeSpentAfterLastPasswordUpdate(IUser user, int timespent = 25)
        {
            ThrowIf.ArgumentNull(user, nameof(user));

            DateTime alteredLastPasswordChangeTimestamp = DateTime.UtcNow.AddHours(-timespent);
            user.ChangeLastPasswordChangeTimestamp(alteredLastPasswordChangeTimestamp);
        }

        #endregion Private functions
    }
}
