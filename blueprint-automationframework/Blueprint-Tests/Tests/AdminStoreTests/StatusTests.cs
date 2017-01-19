using System.Collections.Generic;
using NUnit.Framework;
using CustomAttributes;
using Helper;
using Model;
using TestCommon;
using Utilities;

namespace AdminStoreTests
{
    [TestFixture]
    [Category(Categories.AdminStore)]
    public class StatusTests : TestBase
    {
        private readonly string preAuthorizedKey = CommonConstants.PreAuthorizedKeyForStatus;

        [SetUp]
        public void SetUp()
        {
            Helper = new TestHelper();
        }

        [TestCase]
        [TestRail(146323)]
        [Description("Calls the /status endpoint for AdminStore with a valid preAuthorizedKey and verifies that it returns 200 OK and returns detailed info about AdminStore.")]
        public void GetStatus_WithPreAuthorizedKey_ReturnsDetailedStatus()
        {
            string content = null;

            Assert.DoesNotThrow(() =>
            {
                content = Helper.AdminStore.GetStatus(preAuthorizedKey: preAuthorizedKey);
            }, "The GET /status endpoint should return 200 OK!");

            var extraExpectedStrings = new List<string> {"AdminStore", "AdminStorage", "RaptorDB", "data source"};

            CommonServiceHelper.ValidateStatusResponseContent(content, extraExpectedStrings);
        }

        [TestCase]
        [TestRail(166142)]
        [Description("Calls the /status endpoint for AdminStore with no preAuthorizedKey and verifies that it returns 200 OK and returns basic info about AdminStore.")]
        public void GetStatus_WithNoPreAuthorizedKey_ReturnsBasicStatus()
        {
            string content = null;

            Assert.DoesNotThrow(() =>
            {
                content = Helper.AdminStore.GetStatus(preAuthorizedKey: null);
            }, "The GET /status endpoint should return 200 OK!");

            var extraExpectedStrings = new List<string> { "AdminStore", "AdminStorage", "RaptorDB" };

            CommonServiceHelper.ValidateStatusResponseContent(content, extraExpectedStrings);

            // Verify secure info isn't returned:
            Assert.IsFalse(content.Contains("data source"), "Connection string info was returned without a pre-authorized key!");
        }

        [TestCase("ABCDEFG123456")]
        [TestRail(146324)]
        [Description("Calls the /status endpoint for AdminStore and passes invalid preAuthorizedKey values.  Verifies that it returns a 401 Unauthorized error.")]
        public void GetStatus_InvalidPreAuthorizedKey_UnauthorizedException(string invalidPreAuthorizedKey)
        {
            Assert.Throws<Http401UnauthorizedException>(() =>
            {
                Helper.AdminStore.GetStatus(preAuthorizedKey: invalidPreAuthorizedKey);
            }, "The GET /status endpoint should return 401 Unauthorized when we pass an invalid or missing preAuthorizedKey!");
        }

        [TestCase]
        [TestRail(146325)]
        [Description("Calls the /status/upcheck endpoint for AdminStore and verifies that it returns 200 OK.")]
        public void GetStatus_UpcheckOnly_OK()
        {
            Assert.DoesNotThrow(() =>
            {
                Helper.AdminStore.GetStatusUpcheck();
            }, "'GET /status/upcheck' should return 200 OK.");
        }
    }
}
