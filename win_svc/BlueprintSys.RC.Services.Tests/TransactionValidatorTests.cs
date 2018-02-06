using System;
using System.Linq;
using System.Threading.Tasks;
using BlueprintSys.RC.Services.Helpers;
using BlueprintSys.RC.Services.MessageHandlers;
using BluePrintSys.Messaging.CrossCutting.Models.Exceptions;
using BluePrintSys.Messaging.Models.Actions;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;

namespace BlueprintSys.RC.Services.Tests
{
    [TestClass]
    public class TransactionValidatorTests
    {
        private TransactionValidator _transactionValidator;
        private ActionMessage _message;
        private TenantInformation _tenant;
        private Mock<IBaseRepository> _baseRepoMock;

        [TestInitialize]
        public void TestInitialize()
        {
            _transactionValidator = new TransactionValidator();
            _message = new NotificationMessage
            {
                TransactionId = 123
            };
            _tenant = new TenantInformation();
            _baseRepoMock = new Mock<IBaseRepository>(MockBehavior.Strict);
        }

        [TestMethod]
        public async Task TransactionValidator_ReturnsCommitted_WhenRepositoryReturnsCommittedInt()
        {
            //arrange
            const TransactionStatus expectedStatus = TransactionStatus.Committed;
            _baseRepoMock.Setup(m => m.GetTransactionStatus(It.IsAny<long>())).ReturnsAsync((int) expectedStatus);
            //act
            var status = await _transactionValidator.GetStatus(_message, _tenant, _baseRepoMock.Object);
            //assert
            Assert.AreEqual(expectedStatus, status);
            _baseRepoMock.Verify(m => m.GetTransactionStatus(It.IsAny<long>()), Times.Once);
        }

        [TestMethod]
        public async Task TransactionValidator_ReturnsRolledBack_WhenRepositoryReturnsRolledBackInt()
        {
            //arrange
            const TransactionStatus expectedStatus = TransactionStatus.RolledBack;
            _baseRepoMock.Setup(m => m.GetTransactionStatus(It.IsAny<long>())).ReturnsAsync((int) expectedStatus);
            //act
            var status = await _transactionValidator.GetStatus(_message, _tenant, _baseRepoMock.Object);
            //assert
            Assert.AreEqual(expectedStatus, status);
            _baseRepoMock.Verify(m => m.GetTransactionStatus(It.IsAny<long>()), Times.Once);
        }

        [TestMethod]
        [ExpectedException(typeof(EntityNotFoundException))]
        public async Task TransactionValidator_ThrowsException_WhenRepositoryRepeatedlyReturnsUncommittedInt()
        {
            //arrange
            const TransactionStatus expectedStatus = TransactionStatus.Uncommitted;
            _baseRepoMock.Setup(m => m.GetTransactionStatus(It.IsAny<long>())).ReturnsAsync((int) expectedStatus);
            //act
            try
            {
                await _transactionValidator.GetStatus(_message, _tenant, _baseRepoMock.Object);
            }
            catch (EntityNotFoundException)
            {
                //assert
                _baseRepoMock.Verify(m => m.GetTransactionStatus(It.IsAny<long>()), Times.Exactly(TransactionValidator.TriesMax));
                throw;
            }
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentOutOfRangeException))]
        public async Task TransactionValidator_ThrowsException_WhenRepositoryReturnsUnhandledInt()
        {
            //arrange
            var maxStatus = Enum.GetValues(typeof(TransactionStatus)).Cast<int>().Max();
            var unhandledStatus = maxStatus + 1;
            _baseRepoMock.Setup(m => m.GetTransactionStatus(It.IsAny<long>())).ReturnsAsync(unhandledStatus);
            //act
            try
            {
                await _transactionValidator.GetStatus(_message, _tenant, _baseRepoMock.Object);
            }
            catch (ArgumentOutOfRangeException)
            {
                //assert
                _baseRepoMock.Verify(m => m.GetTransactionStatus(It.IsAny<long>()), Times.Once);
                throw;
            }
        }
    }
}
