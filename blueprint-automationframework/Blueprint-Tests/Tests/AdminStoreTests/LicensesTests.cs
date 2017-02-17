using System.Collections.Generic;
using NUnit.Framework;
using CustomAttributes;
using Model;
using Helper;
using Model.Impl;
using TestCommon;
using Utilities;

namespace AdminStoreTests
{
    [TestFixture]
    [Category(Categories.AdminStore)]
    public class LicensesTests : TestBase
    {
        private IUser _user = null;

        [SetUp]
        public void SetUp()
        {
            Helper = new TestHelper();
            _user = Helper.CreateUserAndAuthenticate(TestHelper.AuthenticationTokenTypes.AccessControlToken);
        }

        [TearDown]
        public void TearDown()
        {
            Helper?.Dispose();
        }

        [TestCase(1)]
        [TestCase(96238)]
        [TestCase(int.MaxValue, Explicit = true, IgnoreReason = IgnoreReasons.ProductBugWillNotFix)]  // BUG: 1362  Returns 401 Unauthorized if the days parameter >= 96239
        [Description("Call:  GET /licenses/transactions with the 'days' parameter > 0.  Verify a valid list of transactions is returned.")]
        [TestRail(146081)]
        public void GetLicenseTransactions_PositiveDays_ReturnsLicenseTransactionList(int days)
        {
            IList<LicenseActivity> licenseActivity = null;

            //right now we test that REST call returns valid list of License Activities
            //we don't check specific values for License Activities
            Assert.DoesNotThrow(() =>
            {
                licenseActivity = Helper.AdminStore.GetLicenseTransactions(_user, numberOfDays: days);
            }, "GetLicenseTransactions() should return 200 OK when passed a positive number of days.");

            Assert.NotNull(licenseActivity, "GetLicenseTransactions() returned null!");
            Assert.That(licenseActivity.Count > 0, "There were no license transactions returned!");
        }

        [TestCase]
        [Description("Call:  GET /licenses/transactions?days=0   Verify an empty list of transactions is returned.")]
        [TestRail(146082)]
        public void GetLicenseTransactions_ZeroDays_ReturnsEmptyList()
        {
            IList<LicenseActivity> licenseActivity = null;

            //right now we test that REST call returns valid list of License Activities
            //we don't check specific values for License Activities
            Assert.DoesNotThrow(() =>
            {
                licenseActivity = Helper.AdminStore.GetLicenseTransactions(_user, numberOfDays: 0);
            }, "GetLicenseTransactions() should return 200 OK when passed 0 days.");

            Assert.NotNull(licenseActivity, "GetLicenseTransactions() returned null!");
            Assert.That(licenseActivity.Count == 0, "There were license transactions returned, even though we specified 'days=0'!");
        }

        [TestCase]
        [Description("Call:  GET /licenses/transactions?days=0  but don't pass any Session-Token header.  Verify 401 Unauthorized is returned.")]
        [TestRail(146083)]
        public void GetLicenseTransactions_NoTokenHeader_401Unauthorized()
        {
            Assert.Throws<Http401UnauthorizedException>(() =>
            {
                Helper.AdminStore.GetLicenseTransactions(numberOfDays: 0);
            }, "GetLicenseTransactions() should return 401 Unauthorized when no token header field was passed.");
        }

        [TestCase]
        [Description("Call:  GET /licenses/transactions  with no 'days' parameter.  Verify 404 Not Found is returned.")]
        [TestRail(146084)]
        public void GetLicenseTransactions_MissingDaysParameter_404NotFound()
        {
            Assert.Throws<Http404NotFoundException>(() =>
            {
                Helper.AdminStore.GetLicenseTransactions(_user, numberOfDays: null);
            }, "GetLicenseTransactions() should return 404 Not Found when the 'days' parameter is missing.");
        }
    }
}