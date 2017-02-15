using System.Data;
using Model.OpenApiModel.Services;
using TestConfig;

namespace Model.Factories
{
    public static class OpenApiFactory
    {
        /// <summary>
        /// Creates a new IOpenApi.
        /// </summary>
        /// <param name="address">The URI address of the Blueprint server.</param>
        /// <returns>An IOpenApi object.</returns>
        public static IOpenApi CreateOpenApi(string address)
        {
            var adminStore = new Impl.OpenApi(address);
            return adminStore;
        }

        /// <summary>
        /// Creates an IOpenApi object with the settings defined in the TestConfiguration.
        /// </summary>
        /// <returns>The IOpenApi object.</returns>
        /// <exception cref="DataException">If there was an error reading required information from the TestConfiguration.</exception>
        public static IOpenApi GetOpenApiFromTestConfig()
        {
            var testConfig = TestConfiguration.GetInstance();

            return CreateOpenApi(testConfig.BlueprintServerAddress);
        }
    }
}
