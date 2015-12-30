using Model.Impl;

namespace Model.Factories
{
    /// <summary>
    /// Creates a new ServiceErrorMessage.
    /// </summary>
    /// <returns>A new ServiceErrorMessage object.</returns>
    public static class ServiceErrorMessageFactory
    {
        public static IServiceErrorMessage CreateServiceErrorMessage(int errorCode, string errorMessage)
        {
            return new ServiceErrorMessage(errorMessage, errorCode);
        }
    }
}
