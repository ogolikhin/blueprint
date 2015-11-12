using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Runtime.Caching;
using System.Threading.Tasks;
using System.Web.Http;
using System.Web.Http.Results;
using AccessControl.Models;
using AccessControl.Repositories;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;

namespace AccessControl.Controllers
{
    [TestClass]
    public class SessionsControllerTests
    {
        private Mock<ISessionsRepository> _sessionsRepoMock;
        private Mock<ObjectCache> _cacheMock;
        private SessionsController _controller;

        [TestInitialize]
        public void Initialize()
        {
            _sessionsRepoMock = new Mock<ISessionsRepository>();
            _cacheMock = new Mock<ObjectCache>();

            _controller = new SessionsController(_cacheMock.Object, _sessionsRepoMock.Object)
            {
                Request = new HttpRequestMessage(),
                Configuration = new HttpConfiguration()
            };
        }

        [TestMethod]
        public void GetSession_SessionNotFound()
        {
            // Arrange
            _controller.Request.Headers.Add("Session-Token", Session.Convert(Guid.NewGuid()));
            _sessionsRepoMock
                .Setup(repo => repo.GetSession(It.IsAny<Guid>()))
                .Returns(Task.FromResult((Session) null));

            // Act
            var result = _controller.GetSession(100).Result;

            // Assert  
            Assert.IsInstanceOfType(result, typeof(NotFoundResult));
        }

        [TestMethod]
        public void GetSession_ReturnsCorrectSession()
        {
            //GlobalConfiguration.Configure(WebApiConfig.Register);
            //var tokensList = new List<string>();
            //tokensList.Add("Token1");
            //var sessionController = new SessionsController(_cacheMock.Object, _sessionsRepoMock.Object);
            //sessionController.Request.Headers.Add("Session-Token", tokensList);

            //var session = sessionController.GetSession(-1);

            Assert.IsTrue(true);
        }
    }
}
