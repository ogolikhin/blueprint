using System;
using System.Collections.Generic;
using System.Net;
using Common;
using CustomAttributes;
using Model;
using Model.Factories;
using NUnit.Framework;
using Utilities;
using Utilities.Factories;

namespace AdminStoreTests
{
    public class UserTests
    {
        private IAdminStore _adminStore = AdminStoreFactory.GetAdminStoreFromTestConfig();
        private IUser _user = null;

        [SetUp]
        public void SetUp()
        {
            _user = UserFactory.CreateUserAndAddToDatabase();
        }

        [TearDown]
        public void TearDown()
        {
            if (_adminStore != null)
            {
                // Delete all the sessions that were created.
                foreach (var session in _adminStore.Sessions.ToArray())
                {
                    // AdminStore removes and adds a new session in some cases, so we should expect a 404 error in some cases.
                    List<HttpStatusCode> expectedStatusCodes = new List<HttpStatusCode> { HttpStatusCode.OK, HttpStatusCode.Unauthorized };
                    _adminStore.DeleteSession(session, expectedStatusCodes);
                }
            }

            if (_user != null)
            {
                _user.DeleteUser();
                _user = null;
            }
        }

        [TestCase(MinPasswordLength)]
        [TestCase(MaxPasswordLength)]
        [TestRail(103064)]
        [Description("Reset a user's password to a valid new password.  The password should be reset successfully and the user should be able to login with the new password.")]
        public void ResetUserPassword_SendValidNewPassword_VerifyUserCanLoginWithNewPassword(uint length)
        {
            // Setup: generate a valid password.
            string newPassword = CreateValidPassword(length);

            // Execute: change the user's password.
            Assert.DoesNotThrow(() =>
            {
                _adminStore.ResetPassword(_user, newPassword);
            }, "Password reset failed when we passed a valid username & password!");

            // Verify: make sure user can login with the new password.
            Assert.DoesNotThrow(() =>
            {
                _adminStore.AddSession(_user.Username, newPassword);
            }, "User couldn't login after resetting their password!");
        }

        [TestCase]
        [TestRail(103066)]
        [Description("Try to reset the password of a user that doesn't exist.  AdminStore should return a 401 error.")]
        public void ResetUserPassword_SendNonExistentUser_Verify401Error()
        {
            // Setup: generate a valid password.
            string newPassword = CreateValidPassword(MinPasswordLength);
            IUser missingUser = UserFactory.CreateUserOnly();

            // Execute & Verify: try to change the user's password.
            var ex = Assert.Throws<Http401UnauthorizedException>(() =>
            {
                _adminStore.ResetPassword(missingUser, newPassword);
            }, "Password reset should get a 401 Unauthorized when passing a user that doesn't exist!");

            const string expectedExceptionMessage = "Invalid username or password";
            Assert.That(ex.RestResponse.Content.Contains(expectedExceptionMessage),
                "Expected error message to contain '{0}', but instead it was '{1}'", expectedExceptionMessage, ex.RestResponse.Content);
        }

        [TestCase]
        [TestRail(103527)]
        [Description("Try to reset the password of a user but don't send an old password.  " +
            "AdminStore should return a 401 error and the user should still be able to login with their old password.")]
        public void ResetUserPassword_DoNotSendOldPassword_Verify401Error()
        {
            // Setup: generate a valid password.
            string newPassword = CreateValidPassword(MinPasswordLength);
            IUser userWithNullPassword = UserFactory.CreateCopyOfUser(_user);
            userWithNullPassword.Password = null;

            // Execute & Verify: try to change the user's password.
            var ex = Assert.Throws<Http401UnauthorizedException>(() =>
            {
                _adminStore.ResetPassword(userWithNullPassword, newPassword);
            }, "Password reset should get a 401 Unauthorized when passing a user with no Old Password!");

            // Verify: make sure user can still login with their old password.
            Assert.DoesNotThrow(() =>
            {
                _adminStore.AddSession(_user.Username, _user.Password);
            }, "User couldn't login with their old password after an unsuccessful password reset!");

            const string expectedExceptionMessage = "Username and password cannot be empty";
            Assert.That(ex.RestResponse.Content.Contains(expectedExceptionMessage),
                "Expected error message to contain '{0}', but instead it was '{1}'", expectedExceptionMessage, ex.RestResponse.Content);
        }

        [TestCase]
        [TestRail(103489)]
        [Description("Try to reset the user password to a valid new password, but specify the wrong old password.  " +
            "AdminStore should return a 401 error and the user should still be able to login with their old password.")]
        public void ResetUserPassword_SendWrongOldPassword_Verify401Error()
        {
            // Setup: generate a valid password.
            string newPassword = CreateValidPassword(MinPasswordLength);
            IUser userWithBadPassword = UserFactory.CreateCopyOfUser(_user);
            string wrongPassword = CreateValidPassword(MinPasswordLength);
            userWithBadPassword.Password = wrongPassword;

            // Execute: try to change the user's password.
            var ex = Assert.Throws<Http401UnauthorizedException>(() =>
            {
                _adminStore.ResetPassword(userWithBadPassword, newPassword);
            }, "Password reset should get a 401 Unauthorized when passing a user that doesn't exist!");

            // Verify: make sure user can still login with their old password.
            Assert.DoesNotThrow(() =>
            {
                _adminStore.AddSession(_user.Username, _user.Password);
            }, "User couldn't login with their old password after an unsuccessful password reset!");

            const string expectedExceptionMessage = "Invalid username or password";
            Assert.That(ex.RestResponse.Content.Contains(expectedExceptionMessage),
                "Expected error message to contain '{0}', but instead it was '{1}'", expectedExceptionMessage, ex.RestResponse.Content);
        }

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
                _adminStore.ResetPassword(_user, newPassword);
            }, "Password reset should get a 400 Bad Request when passing a null or empty new password!");

            // Verify: make sure user can still login with their old password.
            Assert.DoesNotThrow(() =>
            {
                _adminStore.AddSession(_user.Username, _user.Password);
            }, "User couldn't login with their old password after an unsuccessful password reset!");

            const string expectedExceptionMessage = "Password reset failed, new password cannot be empty";
            Assert.That(ex.RestResponse.Content.Contains(expectedExceptionMessage),
                "Expected error message to contain '{0}', but instead it was '{1}'", expectedExceptionMessage, ex.RestResponse.Content);
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
                _adminStore.ResetPassword(_user, _user.Password);
            }, "Password reset should get a 400 Bad Request when old and new passwords are the same!");

            // Verify: make sure user can still login with their old password.
            Assert.DoesNotThrow(() =>
            {
                _adminStore.AddSession(_user.Username, _user.Password);
            }, "User couldn't login with their old password after an unsuccessful password reset!");

            const string expectedExceptionMessage = "Password reset failed, new password cannot be equal to the old one";
            Assert.That(ex.RestResponse.Content.Contains(expectedExceptionMessage),
                "Expected error message to contain '{0}', but instead it was '{1}'", expectedExceptionMessage, ex.RestResponse.Content);
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
                _adminStore.ResetPassword(_user, newPassword);
            }, "Password reset should get a 400 Bad Request when the new password doesn't meet the complexity rules!");

            // Verify: make sure user can still login with their old password.
            Assert.DoesNotThrow(() =>
            {
                _adminStore.AddSession(_user.Username, _user.Password);
            }, "User couldn't login with their old password after an unsuccessful password reset!");

            const string expectedExceptionMessage = "Password reset failed, new password is invalid";
            Assert.That(ex.RestResponse.Content.Contains(expectedExceptionMessage),
                "Expected error message to contain '{0}', but instead it was '{1}'", expectedExceptionMessage, ex.RestResponse.Content);
        }

        [TestCase(MinPasswordLength - 1)]
        [TestCase(MaxPasswordLength + 1)]
        [TestRail(103493)]
        [Description("Try to reset the user's password to one that is too short or too long." +
            "AdminStore should return a 400 error and the user should still be able to login with their old password.")]
        public void ResetUserPassword_SendNewPasswordWithInvalidLength_Verify400Error(uint length)
        {
            // Setup: generate an invalid password.
            string newPassword = CreateValidPassword(length, skipLengthRequirement: true);

            // Execute: try to change the user's password to an invalid password.
            var ex = Assert.Throws<Http400BadRequestException>(() =>
            {
                _adminStore.ResetPassword(_user, newPassword);
            }, "Password reset should get a 400 Bad Request when passing new password that is too short or too long!");

            // Verify: make sure user can still login with their old password.
            Assert.DoesNotThrow(() =>
            {
                _adminStore.AddSession(_user.Username, _user.Password);
            }, "User couldn't login with their old password after an unsuccessful password reset!");

            const string expectedExceptionMessage = "Password reset failed, new password is invalid";
            Assert.That(ex.RestResponse.Content.Contains(expectedExceptionMessage),
                "Expected error message to contain '{0}', but instead it was '{1}'", expectedExceptionMessage, ex.RestResponse.Content);
        }

        #region Private functions

        private const uint MinPasswordLength = 8;
        private const uint MaxPasswordLength = 128;

        private static string CreateValidPassword(uint length, bool skipLengthRequirement = false)
        {
            if (((length >= MinPasswordLength) && (length <= MaxPasswordLength)) || skipLengthRequirement)
            {
                // A valid password needs at least 1 of each of these: number, lower case, upper case, special char.
                string passwordComplexityChars = I18NHelper.FormatInvariant("{0}{1}{2}{3}",
                    RandomGenerator.RandomNumber(9),
                    RandomGenerator.RandomLowerCase(1),
                    RandomGenerator.RandomUpperCase(1),
                    RandomGenerator.RandomSpecialChars(1));

                return RandomGenerator.RandomAlphaNumericUpperAndLowerCase(length - (uint)passwordComplexityChars.Length) + passwordComplexityChars;
            }

            throw new ArgumentException(I18NHelper.FormatInvariant("Length must be between {0} and {1}.", MinPasswordLength, MaxPasswordLength), nameof(length));
        }

        #endregion Private functions
    }
}
