using System.Collections.Generic;
using CustomAttributes;
using Helper;
using NUnit.Framework;
using Utilities;

namespace ArtifactStoreTests
{
    [TestFixture]
    [Category(Categories.ArtifactStore)]
    public static class StatusTests
    {
        [TestCase]
        [TestRail(106928)]
        [Explicit(IgnoreReasons.ProductBug)]    // Bug: 4135  Gets a 500 error.
        [Description("Calls the /status endpoint for ArtifactStore with a valid preAuthorizedKey and verifies that it returns 200 OK and returns the proper data content.")]
        public static void Status_ValidateReturnedContent()
        {
            using (TestHelper helper = new TestHelper())
            {
                string content = null;

                Assert.DoesNotThrow(() =>
                {
                    content = helper.ArtifactStore.GetStatus();
                }, "The GET /status endpoint should return 200 OK!");

                var extraExpectedStrings = new List<string> {"ArtifactStore"};

                CommonServiceHelper.ValidateStatusResponseContent(content, extraExpectedStrings);
            }
        }

        [TestCase(null)]
        [TestCase("ABCDEFG123456")]
        [TestRail(106929)]
        [Explicit(IgnoreReasons.ProductBug)]    // Bug: 4135  Gets a 500 error.
        [Description("Calls the /status endpoint for ArtifactStore and passes invalid preAuthorizedKey values.  Verifies that it returns a 401 error.")]
        public static void StatusWithBadKeys_Expect401Unauthorized(string preAuthorizedKey)
        {
            using (TestHelper helper = new TestHelper())
            {
                Assert.Throws<Http401UnauthorizedException>(() =>
                {
                    helper.ArtifactStore.GetStatus(preAuthorizedKey);
                }, "The GET /status endpoint should return 401 Unauthorized when we pass an invalid or missing preAuthorizedKey!");
            }
        }

        [TestCase]
        [TestRail(106930)]
        [Description("Calls the /status/upcheck endpoint for ArtifactStore and verifies that it returns 200 OK.")]
        public static void GetStatusUpcheck_OK()
        {
            using (TestHelper helper = new TestHelper())
            {
                Assert.DoesNotThrow(() =>
                {
                    helper.ArtifactStore.GetStatusUpcheck();
                });
            }
        }
    }
}
