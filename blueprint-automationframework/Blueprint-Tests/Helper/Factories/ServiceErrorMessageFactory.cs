using Model;
using Model.Factories;
using Model.Impl;

namespace Helper.Factories
{
    /// <summary>
    /// Creates a new ServiceErrorMessage.
    /// </summary>
    /// <returns>A new ServiceErrorMessage object.</retur
    public static class ServiceErrorMessageFactory
    {
        public static IServiceErrorMessage CreateServiceErrorMessage(int errorCode, string errorMessage)
        {
            return new ServiceErrorMessage(errorMessage, errorCode);
        }
    }
}
