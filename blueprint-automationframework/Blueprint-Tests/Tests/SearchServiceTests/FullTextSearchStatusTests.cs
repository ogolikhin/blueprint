using CustomAttributes;
using Helper;
using NUnit.Framework;
using System.Collections.Generic;
using Utilities;

namespace SearchServiceTests
{
    [TestFixture]
    [Category(Categories.SearchService)]
    public static class FullTextSearchStatusTests
    {
        [TestCase]
        [TestRail(182409)]
        [Description("Calls the /status endpoint for SearchService with a valid preAuthorizedKey and verifies that it returns 200 OK and a JSON structure containing detailed status of dependent services.")]
        public static void GetStatusWithPreAuthorizedKey_ValidPreAuthorizedKey_ReturnsDetailedStatus()
        {
            using (TestHelper helper = new TestHelper())
            {
                string content = null;

                Assert.DoesNotThrow(() =>
                {
                    content = helper.FullTextSearch.GetStatus();
                }, "The GET /status endpoint should return 200 OK!");

                var extraExpectedStrings = new List<string> { "SearchService", "Blueprint", "\"accessInfo\":\"data source=" };

                CommonServiceHelper.ValidateStatusResponseContent(content, extraExpectedStrings);
            }
        }

        [TestCase]
        [TestRail(182410)]
        [Description("Calls the /status endpoint for SearchService with a valid preAuthorizedKey and verifies that it returns 200 OK and a JSON structure containing basic status of dependent services.")]
        public static void GetStatus_WithNoPreAuthorizedKey_ReturnsBasicStatus()
        {
            using (TestHelper helper = new TestHelper())
            {
                string content = null;

                Assert.DoesNotThrow(() =>
                {
                    content = helper.FullTextSearch.GetStatus();
                }, "The GET /status endpoint should return 200 OK!");

                var extraExpectedStrings = new List<string> { "SearchService", "Blueprint" };

                CommonServiceHelper.ValidateStatusResponseContent(content, extraExpectedStrings);

                // Verify secure info isn't returned:
                Assert.IsFalse(content.Contains("accessInfo=data source"), "Connection string info was returned without a pre-authorized key!");
            }
        }

        [TestCase("ABCDEFG123456")]
        [TestRail(182411)]
        [Description("Calls the /status endpoint for SearchService and passes invalid preAuthorizedKey values.  Verifies that it returns a 401 error.")]
        public static void GetStatus_InvalidPreAuthorizedKey_UnauthorizedException(string preAuthorizedKey)
        {
            using (TestHelper helper = new TestHelper())
            {
                Assert.Throws<Http401UnauthorizedException>(() =>
                {
                    helper.FullTextSearch.GetStatus(preAuthorizedKey);
                }, "The GET /status endpoint should return 401 Unauthorized when we pass an invalid or missing preAuthorizedKey!");
            }
        }

        [TestCase]
        [TestRail(182412)]
        [Description("Calls the /status/upcheck endpoint for SearchService and verifies that it returns 200 OK.")]
        public static void GetStatus_UpcheckOnly_OK()
        {
            using (TestHelper helper = new TestHelper())
            {
                Assert.DoesNotThrow(() =>
                {
                    helper.FullTextSearch.GetStatusUpcheck();
                });
            }
        }
    }
}
