using System;
using System.Collections.Generic;
using System.Configuration;
using System.Diagnostics;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using System.Web.Http;
using AccessControl.Controllers;
using AccessControl.Models;
using AccessControl.Repositories;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;

namespace AccessControl.Tests.Controllers
{
    [TestClass]
    public class SessionsControllerTest
    {
        [TestMethod]
        public async Task GetSession_ReturnsCorrectSession()
        {
            var newGuid = Guid.NewGuid();
            //var guids = new Guid?[] { newGuid, null };
            var repositoryMock = new Mock<ISessionsRepository>();
            var session = new Session();
            repositoryMock.Setup(r => r.GetSession(newGuid)).Returns(Task.FromResult(session));
            //repositoryMock.Setup(r => r.BeginSession(It.IsAny<int>())).Returns(Task.FromResult(guids));
            //repositoryMock.Setup(r => r.EndSession(It.IsAny<Guid>())).Returns(Task.FromResult(new object()));
           

            int uid = 999;
            var sessionController = new SessionsController(repositoryMock.Object);


            sessionController.Request = new HttpRequestMessage
            {
                RequestUri = new Uri("http://localhost/files")                
            };
            sessionController.Request.Properties.Add(System.Net.Http.HttpPropertyKeys.HttpConfigurationKey, new HttpConfiguration());
            sessionController.Request.Headers.Add("Session-Token", newGuid.ToString("N"));

            var resultSession = await sessionController.GetSession(uid);

            Assert.IsNotNull(resultSession);

        }
    }
}
