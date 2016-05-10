using System.Collections.Generic;
using CustomAttributes;
using Helper;
using Model;
using Model.Factories;
using NUnit.Framework;

namespace ConfigControlTests
{
    public class StatusTests
    {
        private IConfigControl _configControl = ConfigControlFactory.GetConfigControlFromTestConfig();

        [TestCase]
        [TestRail(106946)]
        [Description("Check that GET /svc/configcontrol/status returns 200 OK")]
        public void GetStatus_OK()
        {
            string content = null;

            Assert.DoesNotThrow(() =>
            {
                content = _configControl.GetStatus();
            }, "'GET /status' should return 200 OK.");

            var extraExpectedStrings = new List<string> { "AdminStorage", "ConfigControl" };

            CommonServiceHelper.ValidateStatusResponseContent(content, extraExpectedStrings);
        }


        [TestCase]
        [TestRail(106952)]
        [Description("Check that GET /svc/configcontrol/status/upcheck endpoint for ConfigControl and verifies that it returns 200 OK.")]
        public void GetStatusUpcheck_OK()
        {
            Assert.DoesNotThrow(() =>
            {
                _configControl.GetStatusUpcheck();
            });
        }

    }
}
