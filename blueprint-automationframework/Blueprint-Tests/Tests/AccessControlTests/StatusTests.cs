using System.Collections.Generic;
using NUnit.Framework;
using CustomAttributes;
using Helper;
using TestCommon;

namespace AccessControlTests
{
    [TestFixture]
    [Category(Categories.AccessControl)]
    public class StatusTests : TestBase
    {
        [SetUp]
        public void SetUp()
        {
            Helper = new TestHelper();
        }

        [TestCase]
        [TestRail(96117)]
        [Description("Check that GET /svc/AccessControl/status returns 200 OK and returns a JSON structure with the status of all dependent services.")]
        public void GetStatus_WithNoPreAuthorizedKey_ReturnsBasicStatus()
        {
            string content = null;

            Assert.DoesNotThrow(() =>
            {
                content = Helper.AccessControl.GetStatus();
            }, "'GET /status' should return 200 OK.");

            var extraExpectedStrings = new List<string> {"AccessControl", "AdminStorage"};

            CommonServiceHelper.ValidateStatusResponseContent(content, extraExpectedStrings);
        }

        [TestCase]
        [TestRail(106951)]
        [Description("Checks that GET /svc/accesscontrol/status/upcheck returns 200 OK.")]
        public void GetStatus_UpcheckOnly_OK()
        {
            Assert.DoesNotThrow(() =>
            {
                Helper.AccessControl.GetStatusUpcheck();
            }, "'GET /status/upcheck' should return 200 OK.");
        }
    }
}
