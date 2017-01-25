using CustomAttributes;
using Helper;
using NUnit.Framework;
using System.Collections.Generic;
using Model;
using TestCommon;
using Utilities;

namespace SearchServiceTests
{
    [TestFixture]
    [Category(Categories.SearchService)]
    public class SearchStatusTests : TestBase
    {
        private readonly string preAuthorizedKey = CommonConstants.PreAuthorizedKeyForStatus;

        [SetUp]
        public void SetUp()
        {
            Helper = new TestHelper();
        }

        [TestCase]
        [TestRail(182409)]
        [Description("Calls the /status endpoint for SearchService with a valid preAuthorizedKey and verifies that it returns 200 OK and a JSON structure " +
            "containing detailed status of dependent services.")]
        public void GetStatus_WithValidPreAuthorizedKey_ReturnsDetailedStatus()
        {
            string content = null;

            Assert.DoesNotThrow(() =>
            {
                content = Helper.SearchService.GetStatus(preAuthorizedKey: preAuthorizedKey);
            }, "The GET /status endpoint should return 200 OK!");

            var extraExpectedStrings = new List<string> { "SearchService", "Blueprint", "data source" };

            CommonServiceHelper.ValidateStatusResponseContent(content, extraExpectedStrings);
        }

        [TestCase]
        [TestRail(182410)]
        [Description("Calls the /status endpoint for SearchService with a valid preAuthorizedKey and verifies that it returns 200 OK and a JSON structure " +
            "containing basic status of dependent services.")]
        public void GetStatus_WithNoPreAuthorizedKey_ReturnsBasicStatus()
        {
            string content = null;

            Assert.DoesNotThrow(() =>
            {
                content = Helper.SearchService.GetStatus(preAuthorizedKey: null);
            }, "The GET /status endpoint should return 200 OK!");

            var extraExpectedStrings = new List<string> { "SearchService", "Blueprint" };

            CommonServiceHelper.ValidateStatusResponseContent(content, extraExpectedStrings);

            // Verify secure info isn't returned:
            Assert.IsFalse(content.Contains("data source"), "Connection string info was returned without a pre-authorized key!");
        }

        [TestCase("ABCDEFG123456")]
        [TestRail(182411)]
        [Description("Calls the /status endpoint for SearchService and passes invalid preAuthorizedKey values.  Verifies that it returns a 401 error.")]
        public void GetStatus_InvalidPreAuthorizedKey_UnauthorizedException(string invalidPreAuthorizedKey)
        {
            Assert.Throws<Http401UnauthorizedException>(() =>
            {
                Helper.SearchService.GetStatus(preAuthorizedKey: invalidPreAuthorizedKey);
            }, "The GET /status endpoint should return 401 Unauthorized when we pass an invalid or missing preAuthorizedKey!");
        }

        [TestCase]
        [TestRail(182412)]
        [Description("Calls the /status/upcheck endpoint for SearchService and verifies that it returns 200 OK.")]
        public void GetStatus_UpcheckOnly_OK()
        {
            Assert.DoesNotThrow(() =>
            {
                Helper.SearchService.GetStatusUpcheck();
            });
        }
    }
}
