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

        #region License Usage"

//        [Ignore(IgnoreReasons.UnderDevelopmentQaDev)]
        [TestCase(null, null)]
        [TestCase(10, null)]
        [TestCase(null, 2016)]
        [TestCase(10, 2016)]
        [TestRail(227232)]
        [Description("Check that GET info about license transactions returns 200 OK")]
        public void GetLicenseUsage_WithMonthAndYear_VerifyUsageDataReturned(int? month, int? year)
        {
            // Setup:
            IList<LicenseUsage> response = null;

            // Execute:
            Assert.DoesNotThrow(() =>
            {
                response = Helper.AccessControl.GetLicenseUsage(month, year);
            });

            // Verify:
            VerifySomeProperties(response);
        }

        //        [Ignore(IgnoreReasons.UnderDevelopmentQaDev)]
        [TestCase(-1, null)]
        [TestCase(null, 1)]
        [TestCase(null, 3000)]
        [TestCase(12, 2017)]
        [TestRail(0)]
        [Description("Tries to retrieve license usage information by passing out of range parameters. Verify GetLicenseUsage returns 404 Not Found")]
        public void GetLicenseUsageInfo_OutOfRange_404NotFound(int? month, int? year)
        {
            // Setup:
            string path = RestPaths.Svc.AccessControl.Licenses.USAGE;

            // Execute:
            var ex = Assert.Throws<Http404NotFoundException>(() =>
                Helper.AccessControl.GetLicenseUsage(month, year), "'GET {0}' should return 404 Not Found if parameters out of range!", path);

            // Verify:
            string expectedExceptionMessage = I18NHelper.FormatInvariant("Artifact with ID {0} is deleted.", 1);
            TestHelper.ValidateServiceError(ex.RestResponse, InternalApiErrorCodes.ItemNotFound, expectedExceptionMessage);
        }

        #region Private functions

        private static void VerifySomeProperties(IList<LicenseUsage> response)
        {
            Assert.IsNotNull(response, "There is no response object created!");

            if (response.Count != 0)
            {
                Assert.IsTrue(response[0].ActivityMonth.Equals(11), "The month should be 11!");
                Assert.IsTrue(response[0].ActivityYear.Equals(2016), "The year should be 2016!");
                Assert.IsTrue(response[0].MaxConCurrentAuthors.Equals(1), "MaxConCurrentAuthors should be 1!");
                Assert.IsTrue(response[0].UniqueAuthors.Equals(2), "UniqueAuthors should be 2");

                Assert.IsTrue(response[1].ActivityMonth.Equals(12), "The month should be 12!");
                Assert.IsTrue(response[1].ActivityYear.Equals(2016), "The year should be 2016!");
                Assert.IsTrue(response[1].MaxConCurrentAuthors.Equals(1), "MaxConCurrentAuthors should be 1!");
                Assert.IsTrue(response[1].UniqueAuthors.Equals(1), "UniqueAuthors should be 2");
            }
            else
            {
                Assert.IsEmpty(response);
            }
        }

        #endregion Private functions

        #endregion License Usage"
    }
}
