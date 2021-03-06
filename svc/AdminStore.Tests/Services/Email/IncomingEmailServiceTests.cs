﻿using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using ServiceLibrary.Exceptions;
using ServiceLibrary.Helpers;

namespace AdminStore.Services.Email
{
    [TestClass]
    public class IncomingEmailServiceTests
    {
        private IncomingEmailService _incomingEmailService;

        private Mock<IEmailClient> _emailClientMock;

        private const EmailClientType EmailClientType = Email.EmailClientType.Imap;

        private EmailClientConfig _clientConfig;

        [TestInitialize]
        public void Initialize()
        {
            _emailClientMock = new Mock<IEmailClient>();

            var emailClientFactoryMock = new Mock<IEmailClientFactory>();

            emailClientFactoryMock.Setup(factory => factory.Make(EmailClientType)).Returns(_emailClientMock.Object);

            _incomingEmailService = new IncomingEmailService(emailClientFactoryMock.Object);

            _clientConfig = new EmailClientConfig()
            {
                ClientType = EmailClientType,
                EnableSsl = true,
                ServerAddress = "smtp.test.com",
                Port = 1234,
                AccountUsername = "admin",
                AccountPassword = "password"
            };
        }

        [TestMethod]
        public void Should_Set_UseSsl_To_Configs_EnableSsl_Case_True()
        {
            // Act
            _incomingEmailService.TryConnect(_clientConfig);

            // Assert
            _emailClientMock.VerifySet(client => client.UseSsl = true);
        }

        [TestMethod]
        public void Should_Set_UseSsl_To_Configs_EnableSsl_Case_False()
        {
            // Arrange
            _clientConfig.EnableSsl = false;

            // Act
            _incomingEmailService.TryConnect(_clientConfig);

            // Assert
            _emailClientMock.VerifySet(client => client.UseSsl = false);
        }

        [TestMethod]
        public void Should_Connect_To_Config_ServerAddress_And_Port()
        {
            // Act
            _incomingEmailService.TryConnect(_clientConfig);

            // Assert
            _emailClientMock.Verify(client => client.Connect(_clientConfig.ServerAddress, _clientConfig.Port));
        }

        [TestMethod]
        public void Should_Login_With_Username_And_Password()
        {
            // Act
            _incomingEmailService.TryConnect(_clientConfig);

            // Assert
            _emailClientMock.Verify(client => client.Login(_clientConfig.AccountUsername, _clientConfig.AccountPassword));
        }

        [TestMethod]
        public void Should_Disconnect()
        {
            // Act
            _incomingEmailService.TryConnect(_clientConfig);

            // Assert
            _emailClientMock.Verify(client => client.Disconnect());
        }

        [TestMethod]
        public void Should_Dispose_EmailClient()
        {
            // Act
            _incomingEmailService.TryConnect(_clientConfig);

            // Assert
            _emailClientMock.Verify(client => client.Dispose());
        }

        [TestMethod]
        public void Should_Throw_BadRequestException_When_Connect_Throws_EmailException()
        {
            // Arrange
            _emailClientMock.Setup(client => client.Connect(_clientConfig.ServerAddress, _clientConfig.Port))
                            .Throws(new EmailException("Error Message", ErrorCodes.IncomingMailServerInvalidHostname));

            // Act
            try
            {
                _incomingEmailService.TryConnect(_clientConfig);
            }
            // Assert
            catch (BadRequestException ex)
            {
                Assert.AreEqual("Error Message", ex.Message);
                Assert.AreEqual(ErrorCodes.IncomingMailServerInvalidHostname, ex.ErrorCode);

                return;
            }

            Assert.Fail("No BadRequestException was thrown.");
        }

        [TestMethod]
        public void Should_Throw_BadRequestException_When_Login_Throws_EmailException()
        {
            // Arrange
            _emailClientMock.Setup(client => client.Login(_clientConfig.AccountUsername, _clientConfig.AccountPassword))
                            .Throws(new EmailException("Error Message", ErrorCodes.IncomingMailServerInvalidCredentials));

            // Act
            try
            {
                _incomingEmailService.TryConnect(_clientConfig);
            }
            // Assert
            catch (BadRequestException ex)
            {
                Assert.AreEqual("Error Message", ex.Message);
                Assert.AreEqual(ErrorCodes.IncomingMailServerInvalidCredentials, ex.ErrorCode);

                return;
            }

            Assert.Fail("No BadRequestException was thrown.");
        }

        [TestMethod]
        public void Should_Throw_BadRequestException_When_Disconnect_Throws_EmailException()
        {
            // Arrange
            _emailClientMock.Setup(client => client.Disconnect())
                            .Throws(new EmailException("Error Message", ErrorCodes.UnknownIncomingMailServerError));

            // Act
            try
            {
                _incomingEmailService.TryConnect(_clientConfig);
            }
            // Assert
            catch (BadRequestException ex)
            {
                Assert.AreEqual("Error Message", ex.Message);
                Assert.AreEqual(ErrorCodes.UnknownIncomingMailServerError, ex.ErrorCode);

                return;
            }

            Assert.Fail("No BadRequestException was thrown.");
        }
    }
}
