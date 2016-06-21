using System.Collections.Generic;
using NUnit.Framework;
using CustomAttributes;
using Helper;
using Utilities;

namespace AdminStoreTests
{
    [TestFixture]
    [Category(Categories.AdminStore)]
    public static class StatusTests
    {

        [TestCase]
        [Description("Calls the /status endpoint for AdminStore with a valid preAuthorizedKey and verifies that it returns 200 OK and returns the proper data content.")]
        public static void Status_ValidateReturnedContent()
        {
            using (TestHelper helper = new TestHelper())
            {
            string content = null;

            Assert.DoesNotThrow(() =>
            {
                    content = helper.AdminStore.GetStatus();
            }, "The GET /status endpoint should return 200 OK!");

                var extraExpectedStrings = new List<string> {"AdminStore", "AdminStorage", "RaptorDB"};

            CommonServiceHelper.ValidateStatusResponseContent(content, extraExpectedStrings);
        }
        }

        [TestCase(null)]
        [TestCase("ABCDEFG123456")]
        [Description("Calls the /status endpoint for AdminStore and passes invalid preAuthorizedKey values.  Verifies that it returns a 401 error.")]
        public static void StatusWithBadKeys_Expect401Unauthorized(string preAuthorizedKey)
        {
            using (TestHelper helper = new TestHelper())
        {
            Assert.Throws<Http401UnauthorizedException>(() =>
            {
                    helper.AdminStore.GetStatus(preAuthorizedKey);
            }, "The GET /status endpoint should return 401 Unauthorized when we pass an invalid or missing preAuthorizedKey!");
        }
        }

        [TestCase]
        [Description("Calls the /status/upcheck endpoint for AdminStore and verifies that it returns 200 OK")]
        public static void GetStatusUpcheck_OK()
        {
            using (TestHelper helper = new TestHelper())
        {
            Assert.DoesNotThrow(() =>
            {
                    helper.AdminStore.GetStatusUpcheck();
                }, "'GET /status/upcheck' should return 200 OK.");
            }
        }
    

    }
}
