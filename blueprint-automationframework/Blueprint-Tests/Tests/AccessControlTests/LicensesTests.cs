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
            ISession createdSession = Helper.AccessControl.AddSession(session);

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
            ISession session = SessionFactory.CreateSession(_user);

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
            ISession session = CreateAndAddSessionToAccessControl();

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
            ISession session = CreateAndAddSessionToAccessControl();
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
        [TestCase(null, null, false, 0)]
        [TestCase(9, 2016, false, 1)]
        [TestCase(10, 2116, true, 0)]
        [TestCase(10, 1999, false, 0)]
        [TestRail(227232)]
        [Description("Pass valid month and/or year values to GetLicenseUsage and verify it returns 200 OK with the correct usage data.")]
        public void GetLicenseUsage_WithValidMonthAndYear_VerifyUsageDataReturned(int? month, int? year, bool isEmpty, int offset)
        {
            // Setup:
            IList<LicenseUsage> response = null;

            // Execute:
            Assert.DoesNotThrow(() =>
            {
                response = Helper.AccessControl.GetLicenseUsage(month, year);
            });

            // Verify:
            VerifySomeProperties(response, isEmpty, offset);
        }

        [TestCase(10, -1, YEAR_IS_INVALID)]
        [TestCase(10, 0, YEAR_IS_INVALID)]
        [TestCase(10, 9999, YEAR_IS_INVALID)]
        [TestCase(-1, 2016, MONTH_IS_INVALID)]
        [TestCase(12, 2016, MONTH_IS_INVALID)]
        [TestCase(10, null, YEAR_NOT_SPECIFIED)]
        [TestCase(null, 2016, MONTH_NOT_SPECIFIED)]
        [TestRail(227249)]
        [Description("Pass invalid month or year values to GetLicenseUsage and verify it returns 400 Bad Request.")]
        public void GetLicenseUsage_WithInvalidMonthOrYear_400BadRequest(int? month, int? year, string expectedError)
        {
            // Setup: None

            // Execute:
            var ex = Assert.Throws<Http400BadRequestException>(() =>
            {
                Helper.AccessControl.GetLicenseUsage(month, year);
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
        /// <param name="isEmpty">Verifies if the response empty</param>
        /// <param name="offset">Offset from the total response</param>
        private static void VerifySomeProperties(IList<LicenseUsage> licenseUsageInfo, bool isEmpty, int offset)
        {
            Assert.IsNotNull(licenseUsageInfo, "License usage information should ever be null!");

            if (!isEmpty)
            {
                Assert.IsTrue(licenseUsageInfo[2 - offset].ActivityMonth.Equals(11), "The month should be 11!");
                Assert.IsTrue(licenseUsageInfo[2 - offset].ActivityYear.Equals(2016), "The year should be 2016!");
                Assert.IsTrue(licenseUsageInfo[2 - offset].MaxConcurrentAuthors.Equals(1), "MaxconCurrentAuthors should be 1!");
                Assert.IsTrue(licenseUsageInfo[2 - offset].UniqueAuthors.Equals(2), "UniqueAuthors should be 2");

                Assert.IsTrue(licenseUsageInfo[3 - offset].ActivityMonth.Equals(12), "The month should be 12!");
                Assert.IsTrue(licenseUsageInfo[3 - offset].ActivityYear.Equals(2016), "The year should be 2016!");
                Assert.IsTrue(licenseUsageInfo[3 - offset].MaxConcurrentAuthors.Equals(1), "MaxConcurrentAuthors should be 1!");
                Assert.IsTrue(licenseUsageInfo[3 - offset].UniqueAuthors.Equals(1), "UniqueAuthors should be 2");
            }
            else
            {
                Assert.IsEmpty(response);
            }
        }

        #endregion Private functions

        #endregion GetLicenseUsage tests

    }
}
