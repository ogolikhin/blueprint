
using NUnit.Framework;
using Utilities;

namespace Model.Impl
{
    public class ServiceErrorMessage : IServiceErrorMessage
    {
        public string Message { get; private set; }

        public int ErrorCode { get; private set; }
                
        public ServiceErrorMessage(string message, int errorCode)
        {
            this.ErrorCode = errorCode;
            this.Message = message;
        }

        /// <seealso cref="IServiceErrorMessage.AssertEquals(IServiceErrorMessage)"/>
        public void AssertEquals(IServiceErrorMessage expectedErrorMessage)
        {
            ThrowIf.ArgumentNull(expectedErrorMessage, nameof(expectedErrorMessage));

            AssertEquals(this, expectedErrorMessage);
        }

        /// <summary>
        /// Tests whether the specified ServiceErrorMessage is equal to this one.
        /// </summary>
        /// <param name="actualError">The actual error received.</param>
        /// <param name="expectedErrorMessage">The ServiceErrorMessage to compare.</param>
        public static void AssertEquals(IServiceErrorMessage actualError, IServiceErrorMessage expectedErrorMessage)
        {
            ThrowIf.ArgumentNull(actualError, nameof(actualError));
            ThrowIf.ArgumentNull(expectedErrorMessage, nameof(expectedErrorMessage));

            Assert.AreEqual(expectedErrorMessage.ErrorCode, actualError.ErrorCode, "The ErrorCode's don't match!");
            Assert.AreEqual(expectedErrorMessage.Message, actualError.Message, "The Messages don't match!");
        }
    }
}
