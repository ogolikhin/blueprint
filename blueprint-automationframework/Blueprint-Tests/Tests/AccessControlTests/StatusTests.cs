﻿using System.Collections.Generic;
using NUnit.Framework;
using CustomAttributes;
using Helper;
using Model;
using Model.Factories;

namespace AccessControlTests
{
    [TestFixture]
    [Category(Categories.AccessControl)]
    public class StatusTests
    {
        private IAccessControl _accessControl = AccessControlFactory.GetAccessControlFromTestConfig();

        [TestCase]
        [TestRail(96117)]
        [Description("Check that GET /svc/AccessControl/status returns 200 OK and returns a JSON structure with the status of all dependent services.")]
        public void GetStatus_OK()
        {
            string content = null;

            Assert.DoesNotThrow(() =>
            {
                content = _accessControl.GetStatus();
            }, "'GET /status' should return 200 OK.");

            var extraExpectedStrings = new List<string> { "AccessControl", "AdminStorage" };

            CommonServiceHelper.ValidateStatusResponseContent(content, extraExpectedStrings);
        }

        [TestCase]
        [TestRail(106951)]
        [Description("Checks that GET /svc/accesscontrol/status/upcheck returns 200 OK.")]
        public void GetStatusUpcheck_OK()
        {
            Assert.DoesNotThrow(() =>
            {
                _accessControl.GetStatusUpcheck();
            });
        }
    }
}
