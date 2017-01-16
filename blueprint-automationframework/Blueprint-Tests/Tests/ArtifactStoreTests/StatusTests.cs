using System.Collections.Generic;
using CustomAttributes;
using Helper;
using Model;
using NUnit.Framework;
using TestCommon;
using Utilities;

namespace ArtifactStoreTests
{
    [TestFixture]
    [Category(Categories.ArtifactStore)]
    public class StatusTests : TestBase
    {
        private readonly string preAuthorizedKey = CommonConstants.PreAuthorizedKeyForStatus;

        [SetUp]
        public void SetUp()
        {
            Helper = new TestHelper();
        }

        [TestCase]
        [TestRail(106928)]
        [Description("Calls the /status endpoint for ArtifactStore with a valid preAuthorizedKey and verifies that it returns 200 OK and returns the proper data content.")]
        public void GetStatus_WithPreAuthorizedKey_ReturnsDetailedStatus()
        {
            string content = null;

            Assert.DoesNotThrow(() =>
            {
                content = Helper.ArtifactStore.GetStatus(preAuthorizedKey: preAuthorizedKey);
            }, "The GET /status endpoint should return 200 OK!");

            var extraExpectedStrings = new List<string> {"ArtifactStore", "RaptorDB", "data source"};

            CommonServiceHelper.ValidateStatusResponseContent(content, extraExpectedStrings);
        }

        [TestCase]
        [TestRail(106929)]
        [Description("Calls the /status endpoint for ArtifactStore with NO preAuthorizedKey and verifies that it returns 200 OK and a JSON structure containing basic status of dependent services.")]
        public void GetStatus_WithNoPreAuthorizedKey_ReturnsBasicStatus()
        {
            string content = null;

            Assert.DoesNotThrow(() =>
            {
                content = Helper.ArtifactStore.GetStatus(preAuthorizedKey: null);
            }, "The GET /status endpoint should return 200 OK!");

            var extraExpectedStrings = new List<string> { "ArtifactStore", "RaptorDB" };

            CommonServiceHelper.ValidateStatusResponseContent(content, extraExpectedStrings);

            // Verify secure info isn't returned:
            Assert.IsFalse(content.Contains("data source"), "Connection string info was returned without a pre-authorized key!");
        }

        [TestCase("ABCDEFG123456")]
        [TestRail(227317)]
        [Description("Calls the /status endpoint for ArtifactStore and passes invalid preAuthorizedKey values.  Verifies that it returns a 401 error.")]
        public void GetStatus_InvalidPreAuthorizedKey_UnauthorizedException(string invalidPreAuthorizedKey)
        {
            Assert.Throws<Http401UnauthorizedException>(() =>
            {
                Helper.ArtifactStore.GetStatus(preAuthorizedKey: invalidPreAuthorizedKey);
            }, "The GET /status endpoint should return 401 Unauthorized when we pass an invalid or missing preAuthorizedKey!");
        }

        [TestCase]
        [TestRail(106930)]
        [Description("Calls the /status/upcheck endpoint for ArtifactStore and verifies that it returns 200 OK.")]
        public void GetStatus_UpcheckOnly_OK()
        {
            Assert.DoesNotThrow(() =>
            {
                Helper.ArtifactStore.GetStatusUpcheck();
            });
        }
    }
}
