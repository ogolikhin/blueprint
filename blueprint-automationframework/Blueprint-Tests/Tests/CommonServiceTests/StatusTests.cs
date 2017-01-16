using System.Collections.Generic;
using CustomAttributes;
using Helper;
using Model;
using NUnit.Framework;
using Utilities;
using TestCommon;

namespace CommonServiceTests
{
    public class StatusTests : TestBase
    {
        private readonly string preAuthorizedKey = CommonConstants.PreAuthorizedKeyForStatus;

        [SetUp]
        public void SetUp()
        {
            Helper = new TestHelper();
        }

        [TestCase]
        [TestRail(106948)]
        [Description("Calls the /status endpoint for the main Blueprint site with a valid preAuthorizedKey and verifies that it returns 200 OK and returns detailed info about all services.")]
        public void GetStatus_WithPreAuthorizedKey_ReturnsDetailedStatus()
        {
            string content = null;

            Assert.DoesNotThrow(() =>
            {
                content = Helper.BlueprintServer.GetStatus(preAuthorizedKey: preAuthorizedKey);
            }, "The GET /status endpoint should return 200 OK!");

            var extraExpectedStrings = new List<string> { "AdminStorageDB", "RaptorDB", "FileStorageDB", "AccessControl", "AdminStore", "ConfigControl", "FileStore", "Blueprint", "data source" };

            CommonServiceHelper.ValidateStatusResponseContent(content, extraExpectedStrings);
        }

        [TestCase]
        [TestRail(166144)]
        [Description("Calls the /status endpoint for the main Blueprint site with NO preAuthorizedKey and verifies that it returns 200 OK and returns basic info about all services.")]
        public void GetStatus_WithNoPreAuthorizedKey_ReturnsBasicStatus()
        {
            string content = null;

            Assert.DoesNotThrow(() =>
            {
                content = Helper.BlueprintServer.GetStatus(preAuthorizedKey: null);
            }, "The GET /status endpoint should return 200 OK!");

            var extraExpectedStrings = new List<string> { "AdminStorageDB", "RaptorDB", "FileStorageDB", "AccessControl", "AdminStore", "ConfigControl", "FileStore", "Blueprint" };

            CommonServiceHelper.ValidateStatusResponseContent(content, extraExpectedStrings);

            // Verify secure info isn't returned:
            Assert.IsFalse(content.Contains("data source"), "Connection string info was returned without a pre-authorized key!");
        }

        [TestCase("ABCDEFG123456")]
        [TestRail(106949)]
        [Description("Calls the /status endpoint for the main Blueprint site and passes invalid preAuthorizedKey values.  Verifies that it returns a 401 error.")]
        public void GetStatus_InvalidPreAuthorizedKey_UnauthorizedException(string invalidPreAuthorizedKey)
        {
            Assert.Throws<Http401UnauthorizedException>(() =>
            {
                Helper.BlueprintServer.GetStatus(preAuthorizedKey: invalidPreAuthorizedKey);
            }, "The GET /status endpoint should return 401 Unauthorized when we pass an invalid or missing preAuthorizedKey!");
        }
    }
}
