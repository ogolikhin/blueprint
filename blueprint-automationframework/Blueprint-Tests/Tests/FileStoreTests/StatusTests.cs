using System.Collections.Generic;
using CustomAttributes;
using Helper;
using NUnit.Framework;
using Utilities;

namespace FileStoreTests
{
    [TestFixture]
    [Category(Categories.FileStore)]
    public static class StatusTests
    {
        [TestCase]
        [TestRail(106953)]
        [Description("Calls the /status endpoint for FileStore with a valid preAuthorizedKey and verifies that it returns 200 OK and a JSON structure containing detailed status of dependent services.")]
        public static void GetStatusWithPreAuthorizedKey_ValidPreAuthorizedKey_ReturnsDetailedStatus()
        {
            using (TestHelper helper = new TestHelper())
            {
                string content = null;

                Assert.DoesNotThrow(() =>
                {
                    content = helper.FileStore.GetStatus();
                }, "The GET /status endpoint should return 200 OK!");

                var extraExpectedStrings = new List<string> {"FileStore", "FileStorageDB", "\"accessInfo\":\"data source=" };

                CommonServiceHelper.ValidateStatusResponseContent(content, extraExpectedStrings);
            }
        }

        [TestCase]
        [TestRail(166143)]
        [Description("Calls the /status endpoint for FileStore with a valid preAuthorizedKey and verifies that it returns 200 OK and a JSON structure containing basic status of dependent services.")]
        public static void GetStatus_WithNoPreAuthorizedKey_ReturnsBasicStatus()
        {
            using (TestHelper helper = new TestHelper())
            {
                string content = null;

                Assert.DoesNotThrow(() =>
                {
                    content = helper.FileStore.GetStatus();
                }, "The GET /status endpoint should return 200 OK!");

                var extraExpectedStrings = new List<string> { "FileStore", "FileStorageDB" };

                CommonServiceHelper.ValidateStatusResponseContent(content, extraExpectedStrings);

                // Verify secure info isn't returned:
                Assert.IsFalse(content.Contains("accessInfo=data source"), "Connection string info was returned without a pre-authorized key!");
            }
        }

        [TestCase("ABCDEFG123456")]
        [TestRail(106954)]
        [Description("Calls the /status endpoint for FileStore and passes invalid preAuthorizedKey values.  Verifies that it returns a 401 error.")]
        public static void GetStatus_InvalidPreAuthorizedKey_UnauthorizedException(string preAuthorizedKey)
        {
            using (TestHelper helper = new TestHelper())
            {
                Assert.Throws<Http401UnauthorizedException>(() =>
                {
                    helper.FileStore.GetStatus(preAuthorizedKey);
                }, "The GET /status endpoint should return 401 Unauthorized when we pass an invalid or missing preAuthorizedKey!");
            }
        }

        [TestCase]
        [TestRail(106955)]
        [Description("Calls the /status/upcheck endpoint for FileStore and verifies that it returns 200 OK.")]
        public static void GetStatus_UpcheckOnly_OK()
        {
            using (TestHelper helper = new TestHelper())
            {
                Assert.DoesNotThrow(() =>
                {
                    helper.FileStore.GetStatusUpcheck();
                });
            }
        }
    }
}
