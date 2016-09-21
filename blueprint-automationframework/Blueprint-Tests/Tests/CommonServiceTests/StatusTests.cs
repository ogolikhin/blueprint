using System.Collections.Generic;
using CustomAttributes;
using Helper;
using NUnit.Framework;
using Utilities;

namespace CommonServiceTests
{
    public static class StatusTests
    {
        [TestCase]
        [TestRail(106948)]
        [Description("Calls the /status endpoint for the main Blueprint site with a valid preAuthorizedKey and verifies that it returns 200 OK and returns detailed info about all services.")]
        public static void StatusWithPreAuthorizedKey_ValidKey_ReturnsDetailedStatus()
        {
            using (TestHelper helper = new TestHelper())
            {
                string content = null;

                Assert.DoesNotThrow(() =>
                {
                    content = helper.BlueprintServer.GetStatus();
                }, "The GET /status endpoint should return 200 OK!");

                var extraExpectedStrings = new List<string> { "AdminStorageDB", "RaptorDB", "AccessControl", "AdminStore", "ConfigControl", "FileStore", "\"accessInfo\":\"data source=" };

                CommonServiceHelper.ValidateStatusResponseContent(content, extraExpectedStrings);
            }
        }

        [TestCase]
        [TestRail(166144)]
        [Description("Calls the /status endpoint for the main Blueprint site with a valid preAuthorizedKey and verifies that it returns 200 OK and returns basic info about all services.")]
        public static void Status_WithNoPreAuthorizedKey_ReturnsBasicStatus()
        {
            using (TestHelper helper = new TestHelper())
            {
                string content = null;

                Assert.DoesNotThrow(() =>
                {
                    content = helper.BlueprintServer.GetStatus();
                }, "The GET /status endpoint should return 200 OK!");

                var extraExpectedStrings = new List<string> { "AdminStorageDB", "RaptorDB", "AccessControl", "AdminStore", "ConfigControl", "FileStore" };

                CommonServiceHelper.ValidateStatusResponseContent(content, extraExpectedStrings);

                // Verify secure info isn't returned:
                Assert.IsFalse(content.Contains("accessInfo=data source"), "Connection string info was returned without a pre-authorized key!");
            }
        }

        [TestCase("ABCDEFG123456")]
        [TestRail(106949)]
        [Description("Calls the /status endpoint for the main Blueprint site and passes invalid preAuthorizedKey values.  Verifies that it returns a 401 error.")]
        public static void StatusWithBadKeys_Expect401Unauthorized(string preAuthorizedKey)
        {
            using (TestHelper helper = new TestHelper())
            {
                Assert.Throws<Http401UnauthorizedException>(() =>
                {
                    helper.BlueprintServer.GetStatus(preAuthorizedKey);
                }, "The GET /status endpoint should return 401 Unauthorized when we pass an invalid or missing preAuthorizedKey!");
            }
        }
    }
}
