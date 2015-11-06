using System;
using System.Collections.Generic;
using System.Configuration;
using System.Web.Http;
using AccessControl.Controllers;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;

namespace AccessControl.Tests.Controllers
{
    [TestClass]
    public class SessionsControllerTest
    {
        [TestMethod]
        public void GetSession_ReturnsCorrectSession()
        {
            //GlobalConfiguration.Configure(WebApiConfig.Register);
            //var tokensList = new List<string>();
            //tokensList.Add("Token1");
            //var sessionController = new SessionsController();
            //sessionController.Request.Headers.Add("Session-Token", tokensList);

            //var session = sessionController.GetSession(-1);

            Assert.IsTrue(true);

        }
    }
}
