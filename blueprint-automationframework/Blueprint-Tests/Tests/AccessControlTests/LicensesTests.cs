using Common;
using CustomAttributes;
using Helper;
using Model;
using Model.Factories;
using Model.Impl;
using NUnit.Framework;
using System.Collections.Generic;
using TestCommon;
using Utilities;
using Model.Common.Enums;
using System.Linq;

namespace AccessControlTests
{
    [TestFixture]
    [Category(Categories.AccessControl)]
    public class LicensesTests : TestBase
    {
        private const string YEAR_IS_INVALID = "Specified year is invalid";
        private const string MONTH_IS_INVALID = "Specified month is invalid";
        private const string YEAR_NOT_SPECIFIED = "A year must be specified";
        private const string MONTH_NOT_SPECIFIED = "A month must be specified";

        private const string REST_PATH_ACTIVE = RestPaths.Svc.AccessControl.Licenses.ACTIVE;
        private const string REST_PATH_LOCKED = RestPaths.Svc.AccessControl.Licenses.LOCKED;

        private Dictionary<LicenseLevel, TestHelper.ProjectRole> _licenseLevelToProjectRole = new Dictionary<LicenseLevel, TestHelper.ProjectRole>
        {
            {LicenseLevel.Author, TestHelper.ProjectRole.Author},
            {LicenseLevel.Collaborator, TestHelper.ProjectRole.Collaborator},
            {LicenseLevel.Viewer, TestHelper.ProjectRole.Viewer}
        };

        private Dictionary<LicenseLevel, GroupLicenseType> _licenseLevelToGroupLicenseType = new Dictionary<LicenseLevel, GroupLicenseType>
        {
            {LicenseLevel.Author, GroupLicenseType.Author},
            {LicenseLevel.Collaborator, GroupLicenseType.Collaborate},
            {LicenseLevel.Viewer, GroupLicenseType.None}
        };

        private IUser _user = null;
        private IProject _project = null;

        [SetUp]
        public void SetUp()
        {
            Helper = new TestHelper();
            _user = Helper.CreateUserAndAuthenticate(TestHelper.AuthenticationTokenTypes.BothAccessControlAndOpenApiTokens);
            _project = ProjectFactory.GetProject(_user); // TODO: Change tests to not require a project.
        }

        [TearDown]
        public void TearDown()
        {
            Logger.WriteTrace("TearDown() is deleting all sessions created by the tests...");
            Helper?.Dispose();
        }

        #region Private functions

        /// <summary>
        /// Adds (POST's) the specified session to the AccessControl service.
        /// </summary>
        /// <param name="session">The session to add.</param>
        /// <returns>The session that was added including the session token.</returns>
        private ISession AddSessionToAccessControl(ISession session)
        {
            // POST the new session.
            var createdSession = Helper.AccessControl.AddSession(session);

            // Verify that the POST returned the expected session.
            Assert.True(session.Equals(createdSession),
                "POST returned a different session than expected!  Got '{0}', but expected '{1}'.",
                createdSession.SessionId, session.SessionId);

            return createdSession;
        }

        /// <summary>
        /// Creates a random session and adds (POST's) it to the AccessControl service.
        /// </summary>
        /// <param name="sessionUser">The user for whom the session is created.</param>
        /// <returns>The session that was added including the session token.</returns>
        private ISession CreateAndAddSessionToAccessControl(IUser sessionUser = null)
        {
            sessionUser = sessionUser ?? _user;
            var session = SessionFactory.CreateSession(sessionUser);

            // POST the new session.
            return AddSessionToAccessControl(session);
        }

        #endregion Private functions

        #region GetLicensesInfo tests

        [Category(Categories.CannotRunInParallel)] // This test may fail if sessions are created or deleted while it's in progress. 
        [TestCase(LicenseLevel.Author)]
        [TestCase(LicenseLevel.Collaborator)]
        [TestCase(LicenseLevel.Viewer)]
        [TestRail(96107)]
        [Description("Check that GET svc/AccessControl/licenses/active returns 200 OK. Check that license count is " +
            "incremented as new sessions are added.")]
        public void GetActiveLicensesInfo_UserAddedAndAuthenticated_LicenseCountIsIncremented(LicenseLevel level)
        {
            // Setup:
            IList<IAccessControlLicensesInfo> licenses = null;

            Assert.DoesNotThrow(() =>
            {
                licenses = Helper.AccessControl.GetLicensesInfo(LicenseState.active);
            }, "'GET {0}' should return 200 OK when getting active licenses.", REST_PATH_ACTIVE);

            int licBefore = 0;
            foreach (var element in licenses)
            {
                if (element.LicenseLevel == (int)level)
                {
                    licBefore = element.Count;
                }
            }

            Helper.CreateUserWithProjectRolePermissions(_licenseLevelToProjectRole[level], _project, licenseType: _licenseLevelToGroupLicenseType[level]);

            // Execute:
            Assert.DoesNotThrow(() =>
            {
                licenses = Helper.AccessControl.GetLicensesInfo(LicenseState.active);
            }, "'GET {0}' should return 200 OK when getting active licenses.", REST_PATH_ACTIVE);

            // Verify:
            int licAfter = 0;
            foreach (IAccessControlLicensesInfo element in licenses)
            {
                if (element.LicenseLevel == (int)level)
                {
                    licAfter = element.Count;
                }
            }

            Assert.AreEqual((licBefore + 1), licAfter,
                "The expected number of active {0} licenses does not match the actual number.", level.ToString());

        }

        [Category(Categories.CannotRunInParallel)] // This test may fail if sessions are created or deleted while it's in progress. 
        [TestCase(LicenseLevel.Author)]
        [TestCase(LicenseLevel.Collaborator)]
        [TestCase(LicenseLevel.Viewer)]
        [TestRail(96110)]
        [Description("Check that GET svc/AccessControl/licenses/locked returns 200 OK. Check that license count is " +
            "incremented as new sessions are added.")]
        public void GetLockedLicensesInfo_UserAddedAndAuthenticated_LicenseCountIsIncremented(LicenseLevel level)
        {
            // Setup:
            var sessionUser = Helper.CreateUserWithProjectRolePermissions(_licenseLevelToProjectRole[level], _project, licenseType: _licenseLevelToGroupLicenseType[level]);
            string session = sessionUser.Token.AccessControlToken;

            IList<IAccessControlLicensesInfo> licenseInfo = null;
            int licBefore = 0;
            Assert.DoesNotThrow(() =>
            {
                licenseInfo = Helper.AccessControl.GetLicensesInfo(LicenseState.locked, token: session);
            }, "'GET {0}' should return 200 OK when getting the number of active licenses for the current user's license level, excluding any active license for the current user.", REST_PATH_LOCKED);
            licBefore = licenseInfo[0].Count;

            Assert.AreEqual(licenseInfo.Count, 1, "licenseInfo should contain only one item.");

            Helper.CreateUserWithProjectRolePermissions(_licenseLevelToProjectRole[level], _project, licenseType: _licenseLevelToGroupLicenseType[level]);

            // Execute:
            int licAfter = 0;
            Assert.DoesNotThrow(() =>
            {
                licenseInfo = Helper.AccessControl.GetLicensesInfo(LicenseState.locked, token: session);
            }, "'GET {0}' should return 200 OK when getting the number of active licenses for the current user's license level, excluding any active license for the current user.", REST_PATH_LOCKED);
            licAfter = licenseInfo[0].Count;

            Assert.AreEqual(1, licenseInfo.Count, "licenseInfo should contain only one item.");

            // Verify:
            Assert.AreEqual((licBefore + 1), licAfter,
                "The expected number of active {0} licenses does not match the actual number", level.ToString());
        }

        [TestCase(null)]
        [TestCase(CommonConstants.InvalidToken, Explicit = true, IgnoreReason = IgnoreReasons.ProductBug)] // TFS bug 5823
        [TestRail(267191)]
        [Description("Check that GET svc/AccessControl/licenses/locked with an invalid token return 401 Unauthorized.")]
        public void GetLockedLicensesInfo_InvalidToken_401Unauthorized(string token)
        {
            // Execute:
            var ex = Assert.Throws<Http401UnauthorizedException>(() =>
            {
                Helper.AccessControl.GetLicensesInfo(LicenseState.locked, token);
            }, "'GET {0}' should return 401 Unauthorized the token provided in invalid.", REST_PATH_LOCKED);

            // Verify:
            TestHelper.AssertResponseBodyIsEmpty(ex.RestResponse); // To be changed after bug 5823 is fixed.
        }

        #endregion GetLicensesInfo tests

        #region GetLicenseTransactions tests

        [TestCase]
        [TestRail(96109)]
        [Description("Check that GET info about license transactions returns 200 OK")]
        public void GetLicenseTransactionsInfo_200OK()
        {
            // Setup: Create a session for test.
            var session = CreateAndAddSessionToAccessControl();
            int numberOfDays = 1;

            // In our test environment we have only transactions related to consumerType = 1 - regular client
            int consumerType = 1;

            // TODO: add expected results
            Assert.DoesNotThrow(() =>
            {
                Helper.AccessControl.GetLicenseTransactions(numberOfDays, consumerType, session);
            });
        }

        #endregion GetLicenseTransactions tests

        #region GetLicenseUsage tests

        [Category(Categories.GoldenData)]
        [TestCase(null, null, false)]
        [TestCase(2016, 9, false)]
        [TestCase(2116, 10, true)]
        [TestCase(1999, 10, false)]
        [TestRail(227232)]
        [Description("Pass valid month and/or year values to GetLicenseUsage and verify it returns 200 OK with the correct usage data.")]
        public void GetLicenseUsage_WithValidMonthAndYear_VerifyUsageDataReturned(int? year, int? month, bool expectedEmptyResponse)
        {
            // Setup:
            LicenseUsage response = null;

            // Execute:
            Assert.DoesNotThrow(() =>
            {
                response = Helper.AccessControl.GetLicenseUsage(year, month);
            });

            // Verify:
            VerifySomeProperties(response, expectedEmptyResponse, year, month);
        }

        [TestCase(0, 10, YEAR_IS_INVALID)]
        [TestCase(9999, 10, YEAR_IS_INVALID)]
        [TestCase(2016, 0, MONTH_IS_INVALID)]
        [TestCase(2016, 13, MONTH_IS_INVALID)]
        [TestCase(null, 10, YEAR_NOT_SPECIFIED)]
        [TestCase(2016, null, MONTH_NOT_SPECIFIED)]
        [TestRail(227249)]
        [Description("Pass invalid month or year values to GetLicenseUsage and verify it returns 400 Bad Request.")]
        public void GetLicenseUsage_WithInvalidMonthOrYear_400BadRequest(int? year, int? month, string expectedError)
        {
            // Setup: None

            // Execute:
            var ex = Assert.Throws<Http400BadRequestException>(() =>
            {
                Helper.AccessControl.GetLicenseUsage(year, month);
            });

            // Verify:
            StringAssert.Contains(expectedError, ex.RestResponse.Content,
                "The response body should contain the error: '{0}'", expectedError);
        }

        #region Private functions

        /// <summary>
        /// Verifying couple of elements in the response list
        /// </summary>
        /// <param name="licenseUsageInfo">License usage information broken down by months</param>
        /// <param name="expectedEmptyResponse">Pass true if the response should be empty or false if it should contain data.</param>
        /// <param name="year">The year requested in the REST call.</param>
        /// <param name="month">The month requested in the REST call.</param>
        private static void VerifySomeProperties(LicenseUsage licenseUsageInfo, bool expectedEmptyResponse, int? year, int? month)
        {
            Assert.IsNotNull(licenseUsageInfo, "License usage information should ever be null!");

            if (!expectedEmptyResponse)
            {
                Assert.IsNotEmpty(licenseUsageInfo.Summary, "Summary has no values!");
                Assert.IsNotEmpty(licenseUsageInfo.UserActivities, "UserActivities has no values!");

                const int FIRST_YEAR_IN_DB = 2016;
                const int FIRST_MONTH_IN_DB = 9;
                const int JANUARY_2017 = 201701;
                const int NOVEMBER_2016 = 201611;

                // If we passed null month & year, default to the first month & year in the golden data.
                year = year ?? FIRST_YEAR_IN_DB;
                month = month ?? FIRST_MONTH_IN_DB;

                // If the year is before our golden database was created, set month & year to the first ones in the golden DB.
                if (year < FIRST_YEAR_IN_DB)
                {
                    year = FIRST_YEAR_IN_DB;
                    month = FIRST_MONTH_IN_DB;
                }

                // First verify that the license usage starts from the month & year we requested.
                int yearMonth = (int)(year * 100 + month);

                if (yearMonth < JANUARY_2017)
                {
                    var licenseUsageSummary = licenseUsageInfo.Summary.Find(u => u.YearMonth.Equals(yearMonth));
                    var licenseUserActivity = licenseUsageInfo.UserActivities.Find(u => u.YearMonth.Equals(yearMonth));

                    if (yearMonth == NOVEMBER_2016)
                    {
                        VerifyLicenseUsageValues(licenseUsageSummary, yearMonth, uniqueAuthors: 2);
                    }
                    else
                    {
                        VerifyLicenseUsageValues(licenseUsageSummary, yearMonth, uniqueAuthors: 1);
                    }
                    VerifyLicenseUserActivityValues(licenseUserActivity, userId: 1, licenseType: 3, yearMonth: yearMonth);

                    licenseUsageSummary = licenseUsageInfo.Summary.Find(u => u.YearMonth.Equals(JANUARY_2017));
                    licenseUserActivity = licenseUsageInfo.UserActivities.Find(u => u.YearMonth.Equals(JANUARY_2017));

                    VerifyLicenseUsageValues(licenseUsageSummary, yearMonth: 201701, uniqueAuthors: 4, uniqueCollaborators: 3, uniqueViewers: 6, usersFromRestApi: 0,
                        usersFromAnalytics: 0, maxConcurrentAuthors: 2, maxConcurrentCollaborators: 2, maxConcurrentViewers: 3);
                    VerifyLicenseUserActivityValues(licenseUserActivity, userId: 1, licenseType: 3, yearMonth: JANUARY_2017);
                }
            }
            else
            {
                Assert.IsEmpty(licenseUsageInfo.Summary);
                Assert.IsEmpty(licenseUsageInfo.UserActivities);
            }
        }

        /// <summary>
        /// Verifies that the specified LicenseUsageSummary contains the specified values.
        /// </summary>
        /// <param name="licenseUsageSummary">The LicenseUsageSummary to verify.</param>
        /// <param name="yearMonth">The expected year and month.</param>
        /// <param name="uniqueAuthors">The expected uniqueAuthors.</param>
        /// <param name="uniqueCollaborators">The expected uniqueCollaborators.</param>
        /// <param name="uniqueViewers">The expected uniqueViewers.</param>
        /// <param name="usersFromAnalytics">The expected usersFromAnalytics.</param>
        /// <param name="usersFromRestApi">The expected usersFromRestApi.</param>
        /// <param name="maxConcurrentAuthors">The expected maxConcurrentAuthors.</param>
        /// <param name="maxConcurrentCollaborators">The expected maxConcurrentCollaborators.</param>
        /// <param name="maxConcurrentViewers">The expected maxConcurrentViewers.</param>
        private static void VerifyLicenseUsageValues(LicenseUsageSummary licenseUsageSummary, int yearMonth, int uniqueAuthors, int uniqueCollaborators = 0,
            int uniqueViewers = 0, int usersFromAnalytics = 0, int usersFromRestApi = 0, int maxConcurrentAuthors = 1, int maxConcurrentCollaborators = 0, 
            int maxConcurrentViewers = 0)
        {
            Assert.AreEqual(yearMonth, licenseUsageSummary.YearMonth, "The yearMonth should be {0}!", yearMonth);
            Assert.AreEqual(uniqueAuthors, licenseUsageSummary.UniqueAuthors, "UniqueAuthors should be {0}!", uniqueAuthors);
            Assert.AreEqual(uniqueCollaborators, licenseUsageSummary.UniqueCollaborators, "UniqueCollaborators should be {0}!", uniqueCollaborators);
            Assert.AreEqual(uniqueViewers, licenseUsageSummary.UniqueViewers, "UniqueCollaborators should be {0}!", uniqueViewers);
            Assert.AreEqual(maxConcurrentAuthors, licenseUsageSummary.MaxConcurrentAuthors, "MaxConcurrentAuthors should be {0}!", maxConcurrentAuthors);
            Assert.AreEqual(maxConcurrentCollaborators, licenseUsageSummary.MaxConcurrentCollaborators, "MaxConcurrentCollaborators should be {0}!", maxConcurrentCollaborators);
            Assert.AreEqual(maxConcurrentViewers, licenseUsageSummary.MaxConcurrentViewers, "MaxConcurrentViewers should be {0}!", maxConcurrentViewers);
            Assert.AreEqual(usersFromAnalytics, licenseUsageSummary.UsersFromAnalytics, "UsersFromAnalytics should be {0}!", usersFromAnalytics);
            Assert.AreEqual(usersFromRestApi, licenseUsageSummary.UsersFromRestApi, "UsersFromRestApi should be {0}!", usersFromRestApi);
        }

        /// <summary>
        /// Verifies that the specified LicenseUserActivity contains the specified values.
        /// </summary>
        /// <param name="licenseUserActivity">The LicenseUserActivity to verify</param>
        /// <param name="userId">The expected user id</param>
        /// <param name="licenseType">The expected license type</param>
        /// <param name="yearMonth">The expected year & month</param>
        private static void VerifyLicenseUserActivityValues(LicenseUserActivity licenseUserActivity, int userId, int licenseType, int yearMonth)
        {
            Assert.AreEqual(userId, licenseUserActivity.UserId, "The User Id should be {0}!", userId);
            Assert.AreEqual(licenseType, licenseUserActivity.LicenseType, "The User Id should be {0}!", licenseType);
            Assert.AreEqual(yearMonth, licenseUserActivity.YearMonth, "The User Id should be {0}!", yearMonth);
        }

        #endregion Private functions

        #endregion GetLicenseUsage tests
    }
}
