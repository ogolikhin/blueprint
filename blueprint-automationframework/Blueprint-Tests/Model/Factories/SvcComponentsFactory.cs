using System.Data;
using Model.Impl;
using TestConfig;

namespace Model.Factories
{
    public static class SvcComponentsFactory
    {
        /// <summary>
        /// Creates a new ISvcComponents.
        /// </summary>
        /// <param name="address">The URI address of the Blueprint server.</param>
        /// <returns>An ISvcComponents object.</returns>
        public static ISvcComponents CreateSvcComponents(string address)
        {
            var service = new SvcComponents(address);
            return service;
        }

        /// <summary>
        /// Creates an ISvcComponents object with the settings defined in the TestConfiguration.
        /// </summary>
        /// <returns>The ISvcComponents object.</returns>
        /// <exception cref="DataException">If there was an error reading required information from the TestConfiguration.</exception>
        public static ISvcComponents GetSvcSharedFromTestConfig()
        {
            var testConfig = TestConfiguration.GetInstance();

            return CreateSvcComponents(testConfig.BlueprintServerAddress);
        }
    }
}
