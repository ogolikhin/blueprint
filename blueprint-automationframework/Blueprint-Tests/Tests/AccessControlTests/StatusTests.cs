using System.Collections.Generic;
using NUnit.Framework;
using CustomAttributes;
using Helper;

namespace AccessControlTests
{
    [TestFixture]
    [Category(Categories.AccessControl)]
    public static class StatusTests
    {
        [TestCase]
        [TestRail(96117)]
        [Description("Check that GET /svc/AccessControl/status returns 200 OK and returns a JSON structure with the status of all dependent services.")]
        public static void GetStatus_OK()
        {
            using (TestHelper helper = new TestHelper())
            {
                string content = null;

                Assert.DoesNotThrow(() =>
                {
                    content = helper.AccessControl.GetStatus();
                }, "'GET /status' should return 200 OK.");

                var extraExpectedStrings = new List<string> {"AccessControl", "AdminStorage"};

                CommonServiceHelper.ValidateStatusResponseContent(content, extraExpectedStrings);
            }
        }

        [TestCase]
        [TestRail(106951)]
        [Description("Checks that GET /svc/accesscontrol/status/upcheck returns 200 OK.")]
        public static void GetStatusUpcheck_OK()
        {
            using (TestHelper helper = new TestHelper())
            {
                Assert.DoesNotThrow(() =>
                {
                    helper.AccessControl.GetStatusUpcheck();
                }, "'GET /status/upcheck' should return 200 OK.");
            }
        }
    }
}
