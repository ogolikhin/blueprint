using Microsoft.VisualStudio.TestTools.UnitTesting;
using ServiceLibrary.Helpers;

namespace ServiceLibrary.Exceptions
{
    [TestClass]
    public class AuthorizationExceptionTests
    {
        [TestMethod]
        public void AuthorizationException_DefaultConstructorCalled_VerifyDefaultMessage()
        {
            //Act
            AuthorizationException exception = new AuthorizationException();

            //Assert
            Assert.AreEqual(string.Empty, exception.Message);
            Assert.AreEqual(ErrorCodes.Forbidden, exception.ErrorCode);
            Assert.AreEqual(null, exception.Content);
        }

        [TestMethod]
        public void AuthorizationException_ConstructorCalledWithMessage_MessageSetSuccessfully()
        {
            //Arrange
            var message = "This is a test";

            //Act
            AuthorizationException exception = new AuthorizationException(message);

            //Assert
            Assert.AreEqual(message, exception.Message);
            Assert.AreEqual(ErrorCodes.Forbidden, exception.ErrorCode);
            Assert.AreEqual(null, exception.Content);
        }

        [TestMethod]
        public void AuthorizationException_ConstructorCalledWithMessageAndErrorCode_MessageAndErrorCodeSetSuccesfully()
        {
            //Arrange
            var message = "This is a test";
            var errorCode = ErrorCodes.BadRequest;

            //Act
            AuthorizationException exception = new AuthorizationException(message, errorCode);

            //Assert
            Assert.AreEqual(message, exception.Message);
            Assert.AreEqual(ErrorCodes.BadRequest, exception.ErrorCode);
            Assert.AreEqual(null, exception.Content);
        }

    }
}
