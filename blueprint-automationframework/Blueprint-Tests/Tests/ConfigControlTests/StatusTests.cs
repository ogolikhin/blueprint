using System.Collections.Generic;
using CustomAttributes;
using Helper;
using NUnit.Framework;

namespace ConfigControlTests
{
    public static class StatusTests
    {
        [TestCase]
        [TestRail(106946)]
        [Description("Check that GET /svc/configcontrol/status returns 200 OK")]
        public static void GetStatus_OK()
        {
            using (TestHelper helper = new TestHelper())
            {
                string content = null;

                Assert.DoesNotThrow(() =>
                {
                    content = helper.ConfigControl.GetStatus();
                }, "'GET /status' should return 200 OK.");

                var extraExpectedStrings = new List<string> {"AdminStorage", "ConfigControl"};

                CommonServiceHelper.ValidateStatusResponseContent(content, extraExpectedStrings);
            }
        }


        [TestCase]
        [TestRail(106952)]
        [Description("Check that GET /svc/configcontrol/status/upcheck endpoint for ConfigControl and verifies that it returns 200 OK.")]
        public static void GetStatusUpcheck_OK()
        {
            using (TestHelper helper = new TestHelper())
            {
                Assert.DoesNotThrow(() =>
                {
                    helper.ConfigControl.GetStatusUpcheck();
                });
            }
        }

    }
}
