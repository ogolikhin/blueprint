using System.Collections.Generic;
using Common;
using CustomAttributes;
using Helper;
using Model;
using Model.ArtifactModel.Impl;
using Model.Factories;
using Model.Impl;
using Newtonsoft.Json;
using NUnit.Framework;
using TestCommon;
using Utilities;
using Utilities.Factories;

namespace CommonServiceTests
{
    public class SearchUsersTests : TestBase
    {
        /// <summary>
        /// This is the structure returned by the REST call to display error messages.
        /// </summary>
        public class MessageResult
        {
            public string Message { get; set; }
        }

        const string REST_PATH = RestPaths.Svc.Shared.Users.SEARCH;

        private IUser _user;
        private List<IUser> _users = new List<IUser>();

        #region Setup and Cleanup

        [TestFixtureSetUp]
        public void ClassSetUp()
        {
            Helper = new TestHelper();
            _user = Helper.CreateUserAndAuthenticate(TestHelper.AuthenticationTokenTypes.AccessControlToken);
        }

        [TestFixtureTearDown]
        public void ClassTearDown()
        {
            foreach (IUser user in _users)
            {
                user.DeleteUser(deleteFromDatabase: true);
            }

            Helper?.Dispose();
        }

        #endregion

        [TestCase]
        [TestRail(157080)]
        [Description("Create 6 users.  FindUserOrGroup with no parameters.  Verify it defaults to a limit of 5 search results.")]
        public void FindUserOrGroup_Create6Users_DefaultLimitOf5UsersIsReturned()
        {
            // Setup:
            var users = CreateUsers(numberOfUsers: 6);
            _users.AddRange(users);
            List<UserOrGroupInfo> userOrGroupInfo = null;

            // Execute:
            Assert.DoesNotThrow(() => userOrGroupInfo = Helper.SvcShared.FindUserOrGroup(_user),
                "'GET {0}' should return 200 OK for valid parameters!", REST_PATH);

            // Verify:
            const int defaultLimit = 5;
            Assert.AreEqual(defaultLimit, userOrGroupInfo.Count,
                "The default limit for 'GET {0}' should be {1} if not specified!", REST_PATH, defaultLimit);
        }

        [TestCase(false)]
        [TestCase(true)]
        [TestRail(157081)]
        [Description("Create 5 users whose Display Names all begin with the same string (some with and some without E-mail addresses).  FindUserOrGroup with allowEmptyEmail parameter." +
            "  Verify it finds all the users with E-mail addresses and if allowEmptyEmail=true it should also find the users without E-mail addresses.")]
        public void FindUserOrGroupWithAllowEmptyEmail_UsersWithAndWithoutEmailAddress_ReturnsExpectedUsersAndGroups(bool allowEmptyEmail)
        {
            // Setup:
            string displayNamePrefix = RandomGenerator.RandomAlphaNumericUpperAndLowerCase(8);

            // Create 2 users with and 3 without E-mail addresses.
            var users = CreateUsers(displayNamePrefix, emailUserPrefix: displayNamePrefix, numberOfUsers: 2);
            _users.AddRange(users);
            users = CreateUsers(displayNamePrefix, numberOfUsers: 3);
            _users.AddRange(users);

            List<UserOrGroupInfo> userOrGroupInfo = null;

            // Execute:
            Assert.DoesNotThrow(() => userOrGroupInfo = Helper.SvcShared.FindUserOrGroup(_user, displayNameOrEmail: displayNamePrefix, allowEmptyEmail: allowEmptyEmail),
                "'GET {0}' should return 200 OK for valid parameters!", REST_PATH);

            // Verify:
            int expectedUserCount = (allowEmptyEmail ? 5 : 2);

            Assert.AreEqual(expectedUserCount, userOrGroupInfo.Count,
                "If allowEmptyEmail={0}, we should find {1} users, but {2} were found!",
                allowEmptyEmail.ToString(), expectedUserCount, userOrGroupInfo.Count);

            int expectedUsersWithoutEmailsCount = (allowEmptyEmail ? 3 : 0);
            var usersWithoutEmail = userOrGroupInfo.FindAll(u => string.IsNullOrEmpty(u.Email));

            Assert.AreEqual(expectedUsersWithoutEmailsCount, usersWithoutEmail.Count,
                "If allowEmptyEmail={0}, we should find {1} users, but {2} were found!",
                allowEmptyEmail.ToString(), expectedUsersWithoutEmailsCount, usersWithoutEmail.Count);

            var usersWithEmail = userOrGroupInfo.FindAll(u => !string.IsNullOrEmpty(u.Email));
            Assert.AreEqual(2, usersWithEmail.Count,
                "We should find 2 users with E-mail addresses, but {0} were found!", usersWithEmail.Count);
        }

        [TestCase(false)]
        [TestCase(true)]
        [TestRail(157085)]
        [Description("Create 5 users whose Display Names all begin with the same string (some are guests, some are not).  FindUserOrGroup with the guest parameter." +
            "  Verify it finds all the non-guest users and if guest=true it should also find the guest users.")]
        public void FindUserOrGroupWithIncludeGuests_GuestAndNonGuestUsers_ReturnsExpectedUsersAndGroups(bool includeGuests)
        {
            // Setup:
            string displayNamePrefix = RandomGenerator.RandomAlphaNumericUpperAndLowerCase(8);

            // Create 2 users and 3 guest users.
            var users = CreateUsers(displayNamePrefix, emailUserPrefix: displayNamePrefix, numberOfUsers: 2);
            _users.AddRange(users);
            users = CreateUsers(displayNamePrefix, emailUserPrefix: displayNamePrefix, numberOfUsers: 3, isGuest: true);
            _users.AddRange(users);

            List<UserOrGroupInfo> userOrGroupInfo = null;

            // Execute:
            Assert.DoesNotThrow(() => userOrGroupInfo = Helper.SvcShared.FindUserOrGroup(_user, displayNameOrEmail: displayNamePrefix, includeGuests: includeGuests),
                "'GET {0}' should return 200 OK for valid parameters!", REST_PATH);

            // Verify:
            int expectedUserCount = (includeGuests ? 5 : 2);

            Assert.AreEqual(expectedUserCount, userOrGroupInfo.Count,
                "If includeGuests={0}, we should find {1} users, but {2} were found!",
                includeGuests.ToString(), expectedUserCount, userOrGroupInfo.Count);

            int expectedGuestUsers = (includeGuests ? 3 : 0);
            var guestUsers = userOrGroupInfo.FindAll(u => u.Guest);

            Assert.AreEqual(expectedGuestUsers, guestUsers.Count,
                "If includeGuests={0}, we should find {1} guest users, but {2} were found!",
                includeGuests.ToString(), expectedGuestUsers, guestUsers.Count);

            var nonGuestUsers = userOrGroupInfo.FindAll(u => !u.Guest);
            Assert.AreEqual(2, nonGuestUsers.Count,
                "We should find 2 non-guest users, but {0} were found!", nonGuestUsers.Count);
        }

        [TestCase(1)]
        [TestCase(3)]
        [TestCase(8)]
        [TestCase(500)]
        [TestRail(157083)]
        [Description("Create (x + 2) number of users whose Display Names all begin with the same string (and who have E-mail addresses).  FindUserOrGroup with limit=x.  Verify only x users are returned.")]
        public void FindUserOrGroupWithLimit_CreateMoreUsersThanLimit_ReturnsLimitedNumberOfUsers(int limit)
        {
            // Setup:
            string displayNamePrefix = RandomGenerator.RandomAlphaNumericUpperAndLowerCase(8);

            // Create 2 more users than the limit we're testing with.
            var users = CreateUsers(displayNamePrefix, emailUserPrefix: displayNamePrefix, numberOfUsers: limit + 2);
            _users.AddRange(users);

            List<UserOrGroupInfo> userOrGroupInfo = null;

            // Execute:
            Assert.DoesNotThrow(() => userOrGroupInfo = Helper.SvcShared.FindUserOrGroup(_user, displayNameOrEmail: displayNamePrefix, limit: limit),
                "'GET {0}' should return 200 OK for valid parameters!", REST_PATH);

            // Verify:
            Assert.AreEqual(limit, userOrGroupInfo.Count,
                "If limit={0}, we should find {0} users, but {1} were found!",
                limit, userOrGroupInfo.Count);
        }

        [TestCase(-1, Explicit = true, IgnoreReason = IgnoreReasons.ProductBug)]    // Trello bug: https://trello.com/c/WejtkryM
        [TestCase(0, Explicit = true, IgnoreReason = IgnoreReasons.ProductBug)]    // Trello bug: https://trello.com/c/WejtkryM
        [TestCase(501)]
        [TestCase(int.MaxValue)]
        [TestRail(157084)]
        [Description("FindUserOrGroup with limit=x (where x is out of bounds).  Verify it returns 400 BadRequest.")]
        public void FindUserOrGroupWithLimit_LimitIsOutOfBounds_400BadRequest(int limit)
        {
            // Execute:
            var ex = Assert.Throws<Http400BadRequestException>(() => Helper.SvcShared.FindUserOrGroup(_user, limit: limit),
                "'GET {0}' should return 400 BadRequest when an invalid limit parameter is passed!", REST_PATH);

            // Verify:
            const string expectedMessage = "Query parameter 'limit' exceeds the max value 500.";
            MessageResult messageResult = JsonConvert.DeserializeObject<MessageResult>(ex.RestResponse.Content);

            Assert.AreEqual(expectedMessage, messageResult.Message,
                "If limit={0}, we should get an error message of '{1}'!",
                limit, expectedMessage);
        }

        #region Private functions

        /// <summary>
        /// Creates a specified number of users that have DisplayNames and Email addresses that start with a specified prefix.
        /// </summary>
        /// <param name="displayNamePrefix">(optional) The Display Name prefix.  By default a random Display Name is created.</param>
        /// <param name="emailUserPrefix">(optional) The E-mail user prefix (i.e. the part before the @).</param>
        /// <param name="emailDomain">(optional) The domain of the E-mail address (i.e. the part after the @).
        ///     If this is null and an emailUserPrefix was specified, a random domain is assigned.</param>
        /// <param name="isGuest">(optional) Pass true to make this a guest user.</param>
        /// <param name="numberOfUsers">The number of users to create.</param>
        /// <returns>The list of users created.</returns>
        private static List<IUser> CreateUsers(string displayNamePrefix = null, string emailUserPrefix = null, string emailDomain = null, bool isGuest = false, int numberOfUsers = 1)
        {
            List<IUser> users = new List<IUser>();

            for (int i = 0; i < numberOfUsers; ++i)
            {
                IUser user = UserFactory.CreateUserOnly();

                if (displayNamePrefix != null)
                {
                    user.DisplayName = I18NHelper.FormatInvariant("{0}{1}", displayNamePrefix, i);
                }

                string email = string.Empty;

                if (emailUserPrefix != null)
                {
                    if (emailDomain == null)
                    {
                        emailDomain = I18NHelper.FormatInvariant("{0}.com", RandomGenerator.RandomAlphaNumericUpperAndLowerCase(8));
                    }

                    email = I18NHelper.FormatInvariant("{0}{1}@{2}", emailUserPrefix, i, emailDomain);
                }

                user.Email = email;
                ((User)user).Guest = isGuest;

                user.CreateUser();  // Adds the user to the database.
                users.Add(user);
            }

            return users;
        }

        #endregion Private functions
    }
}
