﻿using Common;
using CustomAttributes;
using Helper;
using Model;
using Model.Factories;
using Model.Impl;
using NUnit.Framework;
using TestCommon;
using Utilities;

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

        private IUser _user = null;

        [SetUp]
        public void SetUp()
        {
            Helper = new TestHelper();
            _user = Helper.CreateUserAndAddToDatabase();
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
        /// <returns>The session that was added including the session token.</returns>
        private ISession CreateAndAddSessionToAccessControl()
        {
            var session = SessionFactory.CreateSession(_user);

            // POST the new session.
            return AddSessionToAccessControl(session);
        }

        #endregion Private functions

        #region GetLicensesInfo tests

        [TestCase]
        [TestRail(96107)]
        [Description("Check that GET active licenses info returns 200 OK")]
        public void GetActiveLicensesInfo_200OK()
        {
            // TODO: add expected results
            Assert.DoesNotThrow(() =>
            {
                Helper.AccessControl.GetLicensesInfo(LicenseState.active);
            });
        }

        [TestCase]
        [TestRail(96110)]
        [Description("Check that GET locked licenses info returns 200 OK")]
        public void GetLockedLicensesInfo_200OK()
        {
            var session = CreateAndAddSessionToAccessControl();

            // TODO: add expected results
            Assert.DoesNotThrow(() =>
            {
                Helper.AccessControl.GetLicensesInfo(LicenseState.locked, session: session);
            });
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

        [Explicit(IgnoreReasons.ProductBug)]    // Bug: https://trello.com/c/T2Lpe0Th
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

                    VerifyLicenseUsageValues(licenseUsageSummary, yearMonth: 201701, uniqueAuthors: 4, uniqueCollaborators: 3, uniqueViewers: 4, usersFromRestApi: 0,
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
        /// <param name="uniqueAuthors">The expected uniqueCollaborators.</param>
        /// <param name="uniqueAuthors">The expected uniqueViewers.</param>
        /// <param name="uniqueAuthors">The expected usersFromAnalytics.</param>
        /// <param name="uniqueAuthors">The expected usersFromRestApi.</param>
        /// <param name="uniqueAuthors">The expected maxConcurrentAuthors.</param>
        /// <param name="uniqueAuthors">The expected maxConcurrentCollaborators.</param>
        /// <param name="uniqueAuthors">The expected maxConcurrentViewers.</param>
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
