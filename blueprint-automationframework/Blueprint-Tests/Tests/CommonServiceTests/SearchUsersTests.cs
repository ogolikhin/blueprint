﻿using System;
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

        private const string REST_PATH = RestPaths.Svc.Shared.Users.SEARCH;
        private const string LIMIT_EXCEEDS_MAX = "Query parameter 'limit' exceeds the max value 500.";
        private const string LIMIT_BELOW_MIN = "Query parameter 'limit' must be greater than 0.";

        private IUser _user;
        private List<IUser> _users = new List<IUser>();
        private List<IGroup> _groups = new List<IGroup>();

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

            foreach (IGroup group in _groups)
            {
                group.DeleteGroup();    // TODO: Add a deleteFromDatabase parameter to the DeleteGroup() function.
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
            var users = CreateUsersWithRandomEmailAddresses(numberOfUsers: 6);
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
            var users = CreateUsersWithRandomEmailAddresses(displayNamePrefix, numberOfUsers: 2);
            _users.AddRange(users);
            users = CreateUsers(displayNamePrefix, numberOfUsers: 3);
            _users.AddRange(users);

            List<UserOrGroupInfo> userOrGroupInfo = null;

            // Execute:
            Assert.DoesNotThrow(() => userOrGroupInfo = Helper.SvcShared.FindUserOrGroup(_user, search: displayNamePrefix, allowEmptyEmail: allowEmptyEmail),
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

            // Verify the users we found are correct.
            foreach (UserOrGroupInfo userOrGroup in userOrGroupInfo)
            {
                ValidateReturnedUser(userOrGroup, isGuest: false, isBlocked: false, nameSearch: displayNamePrefix);
            }
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
            var users = CreateUsersWithRandomEmailAddresses(displayNamePrefix, numberOfUsers: 2);
            _users.AddRange(users);
            users = CreateUsersWithRandomEmailAddresses(displayNamePrefix, numberOfUsers: 3, isGuest: true);
            _users.AddRange(users);

            List<UserOrGroupInfo> userOrGroupInfo = null;

            // Execute:
            Assert.DoesNotThrow(() => userOrGroupInfo = Helper.SvcShared.FindUserOrGroup(_user, search: displayNamePrefix, includeGuests: includeGuests),
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

            // Verify the users we found are correct.
            foreach (UserOrGroupInfo userOrGroup in userOrGroupInfo)
            {
                ValidateReturnedUserSkipGuestAndIsBlocked(userOrGroup, nameSearch: displayNamePrefix);
            }
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
            var users = CreateUsersWithRandomEmailAddresses(displayNamePrefix, numberOfUsers: limit + 2);
            _users.AddRange(users);

            List<UserOrGroupInfo> userOrGroupInfo = null;

            // Execute:
            Assert.DoesNotThrow(() => userOrGroupInfo = Helper.SvcShared.FindUserOrGroup(_user, search: displayNamePrefix, limit: limit),
                "'GET {0}' should return 200 OK for valid parameters!", REST_PATH);

            // Verify:
            Assert.AreEqual(limit, userOrGroupInfo.Count,
                "If limit={0}, we should find {0} users, but {1} were found!",
                limit, userOrGroupInfo.Count);

            // Verify the users we found are correct.
            foreach (UserOrGroupInfo userOrGroup in userOrGroupInfo)
            {
                ValidateReturnedUser(userOrGroup, isGuest: false, isBlocked: false, nameSearch: displayNamePrefix);
            }
        }

        [TestCase(-1, LIMIT_BELOW_MIN)]
        [TestCase(0, LIMIT_BELOW_MIN)]
        [TestCase(501, LIMIT_EXCEEDS_MAX)]
        [TestCase(int.MaxValue, LIMIT_EXCEEDS_MAX)]
        [TestRail(157084)]
        [Description("FindUserOrGroup with limit=x (where x is out of bounds).  Verify it returns 400 BadRequest.")]
        public void FindUserOrGroupWithLimit_LimitIsOutOfBounds_400BadRequest(int limit, string expectedMessage)
        {
            // Execute:
            var ex = Assert.Throws<Http400BadRequestException>(() => Helper.SvcShared.FindUserOrGroup(_user, limit: limit),
                "'GET {0}' should return 400 BadRequest when an invalid limit parameter is passed!", REST_PATH);

            // Verify:
            MessageResult messageResult = JsonConvert.DeserializeObject<MessageResult>(ex.RestResponse.Content);

            Assert.AreEqual(expectedMessage, messageResult.Message,
                "If limit={0}, we should get an error message of '{1}'!",
                limit, expectedMessage);
        }

        [TestCase]
        [TestRail(157124)]
        [Description("Create 6 users (3 with the same E-mail prefix).  FindUserOrGroup with Search equal to the E-mail prefix.  Verify it finds all users with that E-mail prefix.")]
        public void FindUserOrGroupWithSearch_SearchForEmailPrefix_ReturnsUsersWithSpecifiedEmailPrefix()
        {
            // Setup:
            string emailUserPrefix = RandomGenerator.RandomAlphaNumericUpperAndLowerCase(8);

            var users = CreateUsersWithRandomEmailAddresses(numberOfUsers: 3);
            _users.AddRange(users);
            users = CreateUsers(emailUserPrefix: emailUserPrefix, numberOfUsers: 3);
            _users.AddRange(users);

            List<UserOrGroupInfo> userOrGroupInfo = null;

            // Execute:
            Assert.DoesNotThrow(() => userOrGroupInfo = Helper.SvcShared.FindUserOrGroup(_user, emailUserPrefix),
                "'GET {0}' should return 200 OK for valid parameters!", REST_PATH);

            // Verify:
            const int expectedUserCount = 3;
            Assert.AreEqual(expectedUserCount, userOrGroupInfo.Count,
                "If Search is set to the Email prefix of the users we created, we should find {0} users!", expectedUserCount);

            // Verify the users we found are correct.
            foreach (UserOrGroupInfo userOrGroup in userOrGroupInfo)
            {
                ValidateReturnedUser(userOrGroup, isGuest: false, isBlocked: false, emailSearch: emailUserPrefix);
            }
        }

        [TestCase]
        [TestRail(157125)]
        [Description("Create 6 users (3 with the same strings in the middle of their E-mail addresses).  FindUserOrGroup with Search equal to the string in their E-mail addresses." +
            "  Verify it finds all users with that string in their E-mail address.")]
        public void FindUserOrGroupWithSearch_SearchForStringInMiddleOfEmail_ReturnsUsersWithSpecifiedStringInEmail()
        {
            // Setup:
            string emailCommonString = RandomGenerator.RandomAlphaNumericUpperAndLowerCase(8);

            var users = CreateUsersWithRandomEmailAddresses(numberOfUsers: 3);
            _users.AddRange(users);

            for (int i = 0; i < 3; ++i)
            {
                string emailUserPrefix = I18NHelper.FormatInvariant("{0}{1}{0}", RandomGenerator.RandomAlphaNumericUpperAndLowerCase(8), emailCommonString);
                users = CreateUsers(emailUserPrefix: emailUserPrefix);
                _users.AddRange(users);
            }

            List<UserOrGroupInfo> userOrGroupInfo = null;

            // Execute:
            Assert.DoesNotThrow(() => userOrGroupInfo = Helper.SvcShared.FindUserOrGroup(_user, emailCommonString),
                "'GET {0}' should return 200 OK for valid parameters!", REST_PATH);

            // Verify:
            const int expectedUserCount = 3;
            Assert.AreEqual(expectedUserCount, userOrGroupInfo.Count,
                "If Search is set to the Email sub-string of the users we created, we should find {0} users!", expectedUserCount);

            // Verify the users we found are correct.
            foreach (UserOrGroupInfo userOrGroup in userOrGroupInfo)
            {
                ValidateReturnedUser(userOrGroup, isGuest: false, isBlocked: false, emailSearch: emailCommonString);
            }
        }

        [TestCase]
        [TestRail(157126)]
        [Description("Create 6 users (3 with the same strings in the middle of their Display Names).  FindUserOrGroup with Search equal to the string in their Display Names." +
            "  Verify it finds all users with that string in their Display Names.")]
        public void FindUserOrGroupWithSearch_SearchForStringInMiddleOfDisplayName_ReturnsUsersWithSpecifiedStringInDisplayName()
        {
            // Setup:
            string displayNameCommonString = RandomGenerator.RandomAlphaNumericUpperAndLowerCase(8);

            var users = CreateUsersWithRandomEmailAddresses(numberOfUsers: 3);
            _users.AddRange(users);

            for (int i = 0; i < 3; ++i)
            {
                string displayNamePrefix = I18NHelper.FormatInvariant("{0}{1}{0}", RandomGenerator.RandomAlphaNumericUpperAndLowerCase(8), displayNameCommonString);
                users = CreateUsersWithRandomEmailAddresses(displayNamePrefix);
                _users.AddRange(users);
            }

            List<UserOrGroupInfo> userOrGroupInfo = null;

            // Execute:
            Assert.DoesNotThrow(() => userOrGroupInfo = Helper.SvcShared.FindUserOrGroup(_user, displayNameCommonString),
                "'GET {0}' should return 200 OK for valid parameters!", REST_PATH);

            // Verify:
            const int expectedUserCount = 3;
            Assert.AreEqual(expectedUserCount, userOrGroupInfo.Count,
                "If Search is set to the Display Name sub-string of the users we created, we should find {0} users!", expectedUserCount);

            // Verify the users we found are correct.
            foreach (UserOrGroupInfo userOrGroup in userOrGroupInfo)
            {
                ValidateReturnedUser(userOrGroup, isGuest: false, isBlocked: false, nameSearch: displayNameCommonString);
            }
        }

        [TestCase]
        [TestRail(157127)]
        [Description("Create 6 users (3 with the same strings in the domain of their E-mail addresses).  FindUserOrGroup with Search equal to the string in their E-mail addresses." +
            "  Verify it finds all users with that string in their E-mail address.")]
        public void FindUserOrGroupWithSearch_SearchForStringInDomainOfEmail_ReturnsUsersWithSpecifiedStringInEmail()
        {
            // Setup:
            string emailUserPrefix = RandomGenerator.RandomAlphaNumericUpperAndLowerCase(8);
            string emailDomain = RandomGenerator.RandomAlphaNumericUpperAndLowerCase(8);

            var users = CreateUsersWithRandomEmailAddresses(numberOfUsers: 3);
            _users.AddRange(users);
            users = CreateUsers(emailUserPrefix: emailUserPrefix, emailDomain: emailDomain, numberOfUsers: 3);
            _users.AddRange(users);

            List<UserOrGroupInfo> userOrGroupInfo = null;

            // Execute:
            Assert.DoesNotThrow(() => userOrGroupInfo = Helper.SvcShared.FindUserOrGroup(_user, emailDomain),
                "'GET {0}' should return 200 OK for valid parameters!", REST_PATH);

            // Verify:
            const int expectedUserCount = 3;
            Assert.AreEqual(expectedUserCount, userOrGroupInfo.Count,
                "If Search is set to the Email domain of the users we created, we should find {0} users!", expectedUserCount);

            // Verify the users we found are correct.
            foreach (UserOrGroupInfo userOrGroup in userOrGroupInfo)
            {
                ValidateReturnedUser(userOrGroup, isGuest: false, isBlocked: false, emailSearch: emailDomain);
            }
        }

        [TestCase]
        [TestRail(157128)]
        [Description("Create 5 guest users whose Display Names all begin with the same string.  FindUserOrGroup without the guest parameter." +
            "  Verify it finds the 5 the guest users we created.")]
        public void FindUserOrGroup_GuestUsers_ReturnsGuestUsersByDefault()
        {
            // Setup:
            string displayNamePrefix = RandomGenerator.RandomAlphaNumericUpperAndLowerCase(8);

            // Create 5 guest users.
            var users = CreateUsersWithRandomEmailAddresses(displayNamePrefix, numberOfUsers: 5, isGuest: true);
            _users.AddRange(users);

            List<UserOrGroupInfo> userOrGroupInfo = null;

            // Execute:
            Assert.DoesNotThrow(() => userOrGroupInfo = Helper.SvcShared.FindUserOrGroup(_user, search: displayNamePrefix),
                "'GET {0}' should return 200 OK for valid parameters!", REST_PATH);

            // Verify:
            int expectedUserCount = 5;

            Assert.AreEqual(expectedUserCount, userOrGroupInfo.Count,
                "By default includeGuests should be true, so we should find {0} users, but {1} were found!",
                expectedUserCount, userOrGroupInfo.Count);

            var guestUsers = userOrGroupInfo.FindAll(u => u.Guest);

            Assert.AreEqual(expectedUserCount, guestUsers.Count,
                "All the users we found should be guests, but {0} guests were returned!", guestUsers.Count);

            // Verify the users we found are correct.
            foreach (UserOrGroupInfo userOrGroup in userOrGroupInfo)
            {
                ValidateReturnedUser(userOrGroup, isGuest: true, isBlocked: true, nameSearch: displayNamePrefix);
            }
        }

        [TestCase]
        [TestRail(157159)]
        [Description("Create 3 groups whose Display Names all begin with the same string.  FindUserOrGroup with Search equal to the common string in their Display Names." +
            "  Verify it finds the 3 the groups we created.")]
        public void FindUserOrGroupWithSearch_FindGroupsByDisplayName_ReturnsExpectedGroups()
        {
            // Setup:
            string namePrefix = RandomGenerator.RandomAlphaNumericUpperAndLowerCase(8);

            // Create 3 groups.
            const int numberOfGroups = 3;
            var groups = CreateGroupsWithRandomEmailAddresses(namePrefix, numberOfGroups);
            _groups.AddRange(groups);

            List<UserOrGroupInfo> userOrGroupInfo = null;

            // Execute:
            Assert.DoesNotThrow(() => userOrGroupInfo = Helper.SvcShared.FindUserOrGroup(_user, search: namePrefix),
                "'GET {0}' should return 200 OK for valid parameters!", REST_PATH);

            // Verify:
            Assert.AreEqual(numberOfGroups, userOrGroupInfo.Count,
                "We should find {0} groups, but {1} were found!", numberOfGroups, userOrGroupInfo.Count);

            // Verify the group names it found are correct.
            foreach (UserOrGroupInfo userOrGroup in userOrGroupInfo)
            {
                ValidateReturnedGroup(userOrGroup, namePrefix);
            }
        }

        [TestCase]
        [TestRail(157160)]
        [Description("Create 3 users whose DisplayNames begin with the same string, then delete them.  FindUserOrGroup with Search equal to the common sub-string.  Verify no users are returned.")]
        public void FindUserOrGroupWithSearch_FindDeletedUsers_NoUsersAreReturned()
        {
            // Setup:
            string displayNamePrefix = RandomGenerator.RandomAlphaNumericUpperAndLowerCase(8);
            const int numberOfUsers = 3;

            var users = CreateUsersWithRandomEmailAddresses(displayNamePrefix, numberOfUsers: numberOfUsers);
            _users.AddRange(users);

            // Delete the users.
            foreach (IUser user in users)
            {
                user.DeleteUser();
            }

            List<UserOrGroupInfo> userOrGroupInfo = null;

            // Execute:
            Assert.DoesNotThrow(() => userOrGroupInfo = Helper.SvcShared.FindUserOrGroup(_user, displayNamePrefix),
                "'GET {0}' should return 200 OK for valid parameters!", REST_PATH);

            // Verify:
            const int expectedUsersFound = 0;
            Assert.AreEqual(expectedUsersFound, userOrGroupInfo.Count, "We shouldn't find any deleted users!");
        }

        [TestCase]
        [TestRail(157161)]
        [Description("Create 3 groups whose DisplayNames begin with the same string, then delete them.  FindUserOrGroup with Search equal to the common sub-string.  Verify no groups are returned.")]
        public void FindUserOrGroupWithSearch_FindDeletedGroups_NoGroupsAreReturned()
        {
            // Setup:
            string namePrefix = RandomGenerator.RandomAlphaNumericUpperAndLowerCase(8);
            const int numberOfGroups = 3;

            var groups = CreateGroupsWithRandomEmailAddresses(namePrefix, numberOfGroups: numberOfGroups);
            _groups.AddRange(groups);

            // Delete the groups.
            foreach (IGroup group in groups)
            {
                group.DeleteGroup();
            }

            List<UserOrGroupInfo> userOrGroupInfo = null;

            // Execute:
            Assert.DoesNotThrow(() => userOrGroupInfo = Helper.SvcShared.FindUserOrGroup(_user, namePrefix),
                "'GET {0}' should return 200 OK for valid parameters!", REST_PATH);

            // Verify:
            const int expectedGroupsFound = 0;
            Assert.AreEqual(expectedGroupsFound, userOrGroupInfo.Count, "We shouldn't find any deleted groups!");
        }

        [TestCase]
        [TestRail(157162)]
        [Description("Create 5 users & 5 groups with the same substring in their name but beginning with unordered characters.  FindUserOrGroup with Search equal to the string in their Names." +
            "  Verify it returns all users & groups (in alphabetical order) with that substring in their Names and that users are returned first.")]
        public void FindUserOrGroupWithSearch_AddUsersAndGroupsInNonAlphabeticalOrder_ResultsAreInAlphabeticalOrderWithUsersFirst()
        {
            // Setup:
            string nameSubstring = RandomGenerator.RandomAlphaNumericUpperAndLowerCase(8);
            string[] namePrefixUnordered = new string[] { "cc", "aa", "ee", "bb", "dd" };
            string[] namePrefixOrdered = new string[] { "aa", "bb", "cc", "dd", "ee" };
            int numberOfUsersAndGroups = namePrefixUnordered.Length;

            for (int i = 0; i < numberOfUsersAndGroups; ++i)
            {
                var namePrefix = I18NHelper.FormatInvariant("{0}{1}", namePrefixUnordered[i], nameSubstring);

                var users = CreateUsersWithRandomEmailAddresses(namePrefix);
                _users.AddRange(users);

                var groups = CreateGroupsWithRandomEmailAddresses(namePrefix);
                _groups.AddRange(groups);
            }

            List<UserOrGroupInfo> userOrGroupInfo = null;

            // Execute:
            Assert.DoesNotThrow(() => userOrGroupInfo = Helper.SvcShared.FindUserOrGroup(_user, nameSubstring, limit: (numberOfUsersAndGroups * 2) + 1),
                "'GET {0}' should return 200 OK for valid parameters!", REST_PATH);

            // Verify:
            Assert.AreEqual(numberOfUsersAndGroups * 2, userOrGroupInfo.Count,
                "We should find {0} users & groups!", numberOfUsersAndGroups * 2);

            // Verify the users we found are correct and in alphabetical order starting with users.
            for (int i = 0; i < userOrGroupInfo.Count; ++i)
            {
                UserOrGroupInfo userOrGroup = userOrGroupInfo[i];
                string expectedSubstring = I18NHelper.FormatInvariant("{0}{1}", namePrefixOrdered[i % numberOfUsersAndGroups], nameSubstring);

                if (i < numberOfUsersAndGroups)
                {
                    ValidateReturnedUser(userOrGroup, isGuest: false, isBlocked: false, nameSearch: expectedSubstring);
                }
                else
                {
                    ValidateReturnedGroup(userOrGroup, expectedSubstring);
                }
            }
        }

        [TestCase("")]
        [TestCase(BlueprintToken.NO_TOKEN)]
        [TestCase(CommonConstants.InvalidToken)]
        [TestRail(157163)]
        [Description("FindUserOrGroup with a bad or missing token.  Verify it returns 401 Unauthorized.")]
        public void FindUserOrGroup_BadOrMissingToken_401Unauthorized(string token)
        {
            // Setup:
            IUser userWithBadOrMissingToken = UserFactory.CreateUserAndAddToDatabase();
            userWithBadOrMissingToken.Token.SetToken(_user.Token.AccessControlToken);
            userWithBadOrMissingToken.Token.AccessControlToken = token;

            // Execute:
            var ex = Assert.Throws<Http401UnauthorizedException>(() => Helper.SvcShared.FindUserOrGroup(userWithBadOrMissingToken),
                "'GET {0}' should return 401 Unauthorized when a bad or missing token is passed!", REST_PATH);

            // Verify:
            MessageResult messageResult = JsonConvert.DeserializeObject<MessageResult>(ex.RestResponse.Content);
            const string expectedMessage = "Unauthorized call";

            Assert.AreEqual(expectedMessage, messageResult.Message,
                "If a bad or missing token is passed, we should get an error message of '{0}'!", expectedMessage);
        }

        // TODO: Verify that Users with images have 'HasImage=true' property.

        #region Private functions

        /// <summary>
        /// Asserts that the specified UserOrGroupInfo contains the specified search string(s) and has the correct properties set for groups.
        /// </summary>
        /// <param name="userOrGroupInfo">The UserOrGroupInfo to validate.</param>
        /// <param name="nameSearch">(optional) The sub-string expected in the Name property.  If null, emailSearch must be specified.</param>
        /// <param name="emailSearch">(optional) The sub-string expected in the Email property.  If null, nameSearch must be specified.</param>
        private static void ValidateReturnedGroup(UserOrGroupInfo userOrGroupInfo, string nameSearch = null, string emailSearch = null)
        {
            Assert.IsFalse(userOrGroupInfo.HasImage, "The HasImage property should be false for groups that were found!");
            Assert.IsFalse(userOrGroupInfo.Guest, "The Guest property should be false for groups that were found!");
            Assert.IsFalse(userOrGroupInfo.IsBlocked, "The IsBlocked property should be false for groups that were found!");
            Assert.IsTrue(userOrGroupInfo.IsGroup, "The IsGroup property should be true for groups that were found!");

            ValidateReturnedNameAndEmail(userOrGroupInfo, nameSearch, emailSearch);
        }

        /// <summary>
        /// Asserts that the specified UserOrGroupInfo contains the specified search string(s) and has the correct properties set for users.
        /// </summary>
        /// <param name="userOrGroupInfo">The UserOrGroupInfo to validate.</param>
        /// <param name="isGuest"></param>
        /// <param name="isBlocked"></param>
        /// <param name="nameSearch">(optional) The sub-string expected in the Name property.  If null, emailSearch must be specified.</param>
        /// <param name="emailSearch">(optional) The sub-string expected in the Email property.  If null, nameSearch must be specified.</param>
        private static void ValidateReturnedUser(UserOrGroupInfo userOrGroupInfo, bool isGuest, bool isBlocked, string nameSearch = null, string emailSearch = null)
        {
            Assert.AreEqual(isGuest, userOrGroupInfo.Guest, "The Guest property should be {0} for users that were found!", isGuest.ToString());

            // TODO:  The expected IsBlocked value will be different depending on whether you have "All users" or "Registered users only" enabled in Instance Administration
            //     in Email Settings -> "Allow projects to enable email notifications" -> Enable discussions via email replies" -> Edit Settings.
            //Assert.AreEqual(isBlocked, userOrGroupInfo.IsBlocked, "The IsBlocked property should be {0} for users that were found!", isBlocked.ToString());
            isBlocked = !isBlocked; // XXX: This is to make the compiler happy since isBlocked isn't used now that the line above is commented out.

            ValidateReturnedUserSkipGuestAndIsBlocked(userOrGroupInfo, nameSearch, emailSearch);
        }

        /// <summary>
        /// Asserts that the specified UserOrGroupInfo contains the specified search string(s) and has the correct properties set for users,
        /// but doesn't verify the Guest or IsBlocked properties.
        /// </summary>
        /// <param name="userOrGroupInfo">The UserOrGroupInfo to validate.</param>
        /// <param name="nameSearch">(optional) The sub-string expected in the Name property.  If null, emailSearch must be specified.</param>
        /// <param name="emailSearch">(optional) The sub-string expected in the Email property.  If null, nameSearch must be specified.</param>
        private static void ValidateReturnedUserSkipGuestAndIsBlocked(UserOrGroupInfo userOrGroupInfo, string nameSearch = null, string emailSearch = null)
        {
            // NOTE: We can't currently add images to users, so HasImage should always be false.
            Assert.IsFalse(userOrGroupInfo.HasImage, "The HasImage property should be false for users that were found!");
            Assert.IsFalse(userOrGroupInfo.IsGroup, "The IsGroup property should be false for users that were found!");

            ValidateReturnedNameAndEmail(userOrGroupInfo, nameSearch, emailSearch);
        }

        /// <summary>
        /// Asserts that the specified UserOrGroupInfo contains the specified name or E-mail search string.
        /// </summary>
        /// <param name="userOrGroupInfo">The UserOrGroupInfo to validate.</param>
        /// <param name="nameSearch">(optional) The sub-string expected in the Name property.  If null, emailSearch must be specified.</param>
        /// <param name="emailSearch">(optional) The sub-string expected in the Email property.  If null, nameSearch must be specified.</param>
        private static void ValidateReturnedNameAndEmail(UserOrGroupInfo userOrGroupInfo, string nameSearch = null, string emailSearch = null)
        {
            if ((nameSearch == null) && (emailSearch == null))
            {
                throw new ArgumentException("The 'nameSearch' and 'emailSearch' cannot both be null!");
            }

            if (nameSearch != null)
            {
                Assert.That(userOrGroupInfo.Name.Contains(nameSearch), "Can't find '{0}' in the returned UserOrGroupInfo Name!", nameSearch);
            }

            if (emailSearch != null)
            {
                Assert.That(userOrGroupInfo.Email.Contains(emailSearch), "Can't find '{0}' in the returned UserOrGroupInfo Email!", emailSearch);
            }
        }

        /// <summary>
        /// Creates a specified number of users that have DisplayNames that start with a specified prefix and have random E-mail addresses.
        /// </summary>
        /// <param name="displayNamePrefix">(optional) The Display Name prefix.  By default a random Display Name is created.</param>
        /// <param name="isGuest">(optional) Pass true to make this a guest user.</param>
        /// <param name="numberOfUsers">The number of users to create.</param>
        /// <returns>The list of users created.</returns>
        private static List<IUser> CreateUsersWithRandomEmailAddresses(string displayNamePrefix = null,
            bool isGuest = false,
            int numberOfUsers = 1)
        {
            string emailUserPrefix = RandomGenerator.RandomAlphaNumericUpperAndLowerCase(8);
            string emailDomain = RandomGenerator.RandomAlphaNumericUpperAndLowerCase(8);

            return CreateUsers(displayNamePrefix, emailUserPrefix, emailDomain, isGuest, numberOfUsers);
        }

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
        private static List<IUser> CreateUsers(string displayNamePrefix = null,
            string emailUserPrefix = null,
            string emailDomain = null,
            bool isGuest = false,
            int numberOfUsers = 1)
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

        /// <summary>
        /// Creates a specified number of groups that have Names that start with a specified prefix and have random E-mail addresses.
        /// </summary>
        /// <param name="namePrefix">(optional) The Name prefix.  By default a random Name is created.</param>
        /// <param name="numberOfGroups">The number of groups to create.</param>
        /// <returns>The list of groups created.</returns>
        private static List<IGroup> CreateGroupsWithRandomEmailAddresses(string namePrefix = null, int numberOfGroups = 1)
        {
            string emailUserPrefix = RandomGenerator.RandomAlphaNumericUpperAndLowerCase(8);
            string emailDomain = RandomGenerator.RandomAlphaNumericUpperAndLowerCase(8);

            return CreateGroups(namePrefix, emailUserPrefix, emailDomain, numberOfGroups);
        }

        /// <summary>
        /// Creates a specified number of groups that have Names and Email addresses that start with a specified prefix.
        /// </summary>
        /// <param name="namePrefix">(optional) The Name prefix.  By default a random Name is created.</param>
        /// <param name="emailUserPrefix">(optional) The E-mail user prefix (i.e. the part before the @).</param>
        /// <param name="emailDomain">(optional) The domain of the E-mail address (i.e. the part after the @).
        ///     If this is null and an emailUserPrefix was specified, a random domain is assigned.</param>
        /// <param name="numberOfGroups">The number of groups to create.</param>
        /// <returns>The list of groups created.</returns>
        private static List<IGroup> CreateGroups(string namePrefix = null,
            string emailUserPrefix = null,
            string emailDomain = null,
            int numberOfGroups = 1)
        {
            List<IGroup> groups = new List<IGroup>();

            for (int i = 0; i < numberOfGroups; ++i)
            {
                IGroup group = GroupFactory.CreateGroup();

                if (namePrefix != null)
                {
                    group.Name = I18NHelper.FormatInvariant("{0}{1}", namePrefix, i);
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

                group.Email = email;

                group.AddGroupToDatabase();
                groups.Add(group);
            }

            return groups;
        }

        #endregion Private functions
    }
}
