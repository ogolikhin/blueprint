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
        public async Task GetSession_ReturnsCorrectSession()
        {
            // Arrange
            int uid = 999;
            var newGuid = Guid.NewGuid();            
            var session = new Session();
            _sessionsRepoMock.Setup(r => r.GetSession(newGuid)).Returns(Task.FromResult(session));
            _controller.Request.Headers.Add("Session-Token", newGuid.ToString("N"));
            //repositoryMock.Setup(r => r.BeginSession(It.IsAny<int>())).Returns(Task.FromResult(guids));
            //repositoryMock.Setup(r => r.EndSession(It.IsAny<Guid>())).Returns(Task.FromResult(new object()));
            
            //var sessionController = new SessionsController(repositoryMock.Object);
           
            // HttpPropertyKeys.HttpConfigurationKey
            //_controller.Request.Properties.Add("Config", new HttpConfiguration());            

            // Act
            var resultSession = await _controller.GetSession(uid);

            // Assert
            Assert.IsNotNull(resultSession);
            var responseResult = resultSession as ResponseMessageResult;
            Assert.IsNotNull(responseResult); 
            Assert.IsTrue(responseResult.Response.IsSuccessStatusCode);
        }

        [TestMethod]
        public async Task GetSession_FormatError()
        {
            // Arrange
            int uid = 999;
            var newGuid = Guid.NewGuid();
            var session = new Session();
            _sessionsRepoMock.Setup(r => r.GetSession(newGuid)).Returns(Task.FromResult(session));
            _controller.Request.Headers.Add("Session-Token", "null");                      

            // Act
            var resultSession = await _controller.GetSession(uid);

            // Assert
            Assert.IsNotNull(resultSession);
            var responseResult = resultSession as BadRequestResult;
            Assert.IsNotNull(responseResult);            
        }

        [TestMethod]
        public async Task GetSession_NoSessionToken()
        {
            // Arrange
            int uid = 999;
            var newGuid = Guid.NewGuid();
            var session = new Session();
            _sessionsRepoMock.Setup(r => r.GetSession(newGuid)).Returns(Task.FromResult(session));            

            // Act
            var resultSession = await _controller.GetSession(uid);

            // Assert
            Assert.IsNotNull(resultSession);
            var responseResult = resultSession as InternalServerErrorResult;
            Assert.IsNotNull(responseResult);
        }

       

    }
}
