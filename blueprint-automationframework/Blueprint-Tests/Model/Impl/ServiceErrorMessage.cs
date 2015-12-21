
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

        /// <summary>
        /// Tests whether the specified ServiceErrorMessage is equal to this one.
        /// </summary>
        /// <param name="session">The ServiceErrorMessage to compare.</param>
        /// <returns>True if the objects are equal, otherwise false.</returns>
        public bool Equals(IServiceErrorMessage errorMessage)
        {
            if (errorMessage == null)
            {
                return false;
            }

            return (this.ErrorCode == errorMessage.ErrorCode) && (this.Message == errorMessage.Message);
        }
    }
}
