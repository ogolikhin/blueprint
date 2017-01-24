using Common;
using CustomAttributes;
using Helper;
using Model;
using Model.Factories;
using Model.Impl;
using NUnit.Framework;
using System.Collections.Generic;
using System.Linq;
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
            List<LicenseUsage> response = null;

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
        private static void VerifySomeProperties(List<LicenseUsage> licenseUsageInfo, bool expectedEmptyResponse, int? year, int? month)
        {
            Assert.IsNotNull(licenseUsageInfo, "License usage information should ever be null!");

            if (!expectedEmptyResponse)
            {
                const int FIRST_YEAR_IN_DB = 2016;
                const int FIRST_MONTH_IN_DB = 9;

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
                Assert.AreEqual(year, licenseUsageInfo.First().UsageYear, "The year should be {0}!", year);
                Assert.AreEqual(month, licenseUsageInfo.First().UsageMonth, "The month should be {0}!", month);

                var licenseUsage = licenseUsageInfo.Find(u => u.UsageYear.Equals(2016) && u.UsageMonth.Equals(10));
                VerifyLicenseUsageValues(licenseUsage, usageMonth: 10, usageYear: 2016, uniqueAuthors: 1, uniqueAuthorUserIds: "1", authorsCreatedToDate: 1,
                    registeredAuthorsCreated: 0, registeredAuthorsCreatedUserIds: null);

                licenseUsage = licenseUsageInfo.Find(u => u.UsageYear.Equals(2016) && u.UsageMonth.Equals(11));
                VerifyLicenseUsageValues(licenseUsage, usageMonth: 11, usageYear: 2016, uniqueAuthors: 2, uniqueAuthorUserIds: "1,2", authorsCreatedToDate: 2,
                    registeredAuthorsCreated: 1, registeredAuthorsCreatedUserIds: "2");
            }
            else
            {
                Assert.IsEmpty(licenseUsageInfo);
            }
        }

        /// <summary>
        /// Verifies that the specified LicenseUsage contains the specified values.
        /// </summary>
        /// <param name="licenseUsage">The LicenseUsage to verify.</param>
        /// <param name="usageYear">The expected usageYear.</param>
        /// <param name="usageMonth">The expected usageMonth.</param>
        /// <param name="uniqueAuthors">The expected uniqueAuthors.</param>
        /// <param name="uniqueAuthorUserIds">The expected uniqueAuthorUserIds.</param>
        /// <param name="authorsCreatedToDate">The expected authorsCreatedToDate.</param>
        /// <param name="registeredAuthorsCreated">The expected registeredAuthorsCreated.</param>
        /// <param name="registeredAuthorsCreatedUserIds">The expected registeredAuthorsCreatedUserIds.</param>
        private static void VerifyLicenseUsageValues(LicenseUsage licenseUsage, int usageYear, int usageMonth, int uniqueAuthors, string uniqueAuthorUserIds, int authorsCreatedToDate,
            int registeredAuthorsCreated, string registeredAuthorsCreatedUserIds)
        {
            // These properties are always the same in our Golden DB (so far).
            const string registeredCollaboratorCreatedUserIds = null;
            const string uniqueCollaboratorUserIds = null;
            const int uniqueCollaborators = 0;
            const int collaboratorsCreatedToDate = 0;
            const int maxConcurrentViewers = 0;
            const int maxConcurrentAuthors = 1;
            const int maxConcurrentCollaborators = 0;
            const int registeredCollaboratorsCreated = 0;
            const int usersFromAnalytics = 0;
            const int usersFromRestApi = 0;

            Assert.AreEqual(usageYear, licenseUsage.UsageYear, "The UsageYear should be {0}!", usageYear);
            Assert.AreEqual(usageMonth, licenseUsage.UsageMonth, "The UsageMonth should be {0}!", usageMonth);
            Assert.AreEqual(uniqueAuthors, licenseUsage.UniqueAuthors, "UniqueAuthors should be {0}!", uniqueAuthors);
            Assert.AreEqual(uniqueCollaborators, licenseUsage.UniqueCollaborators, "UniqueCollaborators should be {0}!", uniqueCollaborators);
            Assert.AreEqual(uniqueAuthorUserIds, licenseUsage.UniqueAuthorUserIds, "UniqueAuthorUserIds should be {0}!", uniqueAuthorUserIds);
            Assert.AreEqual(uniqueCollaboratorUserIds, licenseUsage.UniqueCollaboratorUserIds, "UniqueCollaboratorUserIds should be {0}!", uniqueCollaboratorUserIds);
            Assert.AreEqual(registeredAuthorsCreated, licenseUsage.RegisteredAuthorsCreated, "RegisteredAuthorsCreated should be {0}!", registeredAuthorsCreated);
            Assert.AreEqual(registeredAuthorsCreatedUserIds, licenseUsage.RegisteredAuthorsCreatedUserIds, "RegisteredAuthorsCreatedUserIds should be {0}!", registeredAuthorsCreatedUserIds);
            Assert.AreEqual(registeredCollaboratorsCreated, licenseUsage.RegisteredCollaboratorsCreated, "RegisteredCollaboratorsCreated should be {0}!", registeredCollaboratorsCreated);
            Assert.AreEqual(registeredCollaboratorCreatedUserIds, licenseUsage.RegisteredCollaboratorCreatedUserIds, "RegisteredCollaboratorCreatedUserIds should be {0}!", registeredCollaboratorCreatedUserIds);
            Assert.AreEqual(authorsCreatedToDate, licenseUsage.AuthorsCreatedToDate, "AuthorsCreatedToDate should be {0}!", authorsCreatedToDate);
            Assert.AreEqual(collaboratorsCreatedToDate, licenseUsage.CollaboratorsCreatedToDate, "CollaboratorsCreatedToDate should be {0}!", collaboratorsCreatedToDate);
            Assert.AreEqual(maxConcurrentViewers, licenseUsage.MaxConcurrentViewers, "MaxConcurrentViewers should be {0}!", maxConcurrentViewers);
            Assert.AreEqual(maxConcurrentAuthors, licenseUsage.MaxConcurrentAuthors, "MaxConcurrentAuthors should be {0}!", maxConcurrentAuthors);
            Assert.AreEqual(maxConcurrentCollaborators, licenseUsage.MaxConcurrentCollaborators, "MaxConcurrentCollaborators should be {0}!", maxConcurrentCollaborators);
            Assert.AreEqual(usersFromAnalytics, licenseUsage.UsersFromAnalytics, "UsersFromAnalytics should be {0}!", usersFromAnalytics);
            Assert.AreEqual(usersFromRestApi, licenseUsage.UsersFromRestApi, "UsersFromRestApi should be {0}!", usersFromRestApi);
        }

        #endregion Private functions

        #endregion GetLicenseUsage tests
    }
}
