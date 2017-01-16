using System.Collections.Generic;
using CustomAttributes;
using Helper;
using NUnit.Framework;
using TestCommon;

namespace ConfigControlTests
{
    public class StatusTests : TestBase
    {
        [SetUp]
        public void SetUp()
        {
            Helper = new TestHelper();
        }

        [TestCase]
        [TestRail(106946)]
        [Description("Calls the /status endpoint for ConfigControl with no preAuthorizedKey and verifies that it returns 200 OK and a JSON structure containing basic status of dependent services.")]
        public void GetStatus_WithNoPreAuthorizedKey_ReturnsBasicStatus()
        {
            string content = null;

            Assert.DoesNotThrow(() =>
            {
                content = Helper.ConfigControl.GetStatus();
            }, "The GET /status endpoint should return 200 OK!");

            var extraExpectedStrings = new List<string> { "AdminStorage", "ConfigControl" };

            CommonServiceHelper.ValidateStatusResponseContent(content, extraExpectedStrings);

            // Verify secure info isn't returned:
            Assert.IsFalse(content.Contains("data source"), "Connection string info was returned without a pre-authorized key!");
        }

        [TestCase]
        [TestRail(106952)]
        [Description("Check that GET /svc/configcontrol/status/upcheck endpoint for ConfigControl and verifies that it returns 200 OK.")]
        public void GetStatus_UpcheckOnly_OK()
        {
            Assert.DoesNotThrow(() =>
            {
                Helper.ConfigControl.GetStatusUpcheck();
            });
        }
    }
}
