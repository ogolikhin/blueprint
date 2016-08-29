using System.Data;
using Model.Impl;
using TestConfig;

namespace Model.Factories
{
    public static class SvcSharedFactory
    {
        /// <summary>
        /// Creates a new ISvcShared.
        /// </summary>
        /// <param name="address">The URI address of the Blueprint server.</param>
        /// <returns>An ISvcShared object.</returns>
        public static ISvcShared CreateArtifactStore(string address)
        {
            ISvcShared adminStore = new SvcShared(address);
            return adminStore;
        }

        /// <summary>
        /// Creates an ISvcShared object with the settings defined in the TestConfiguration.
        /// </summary>
        /// <returns>The ISvcShared object.</returns>
        /// <exception cref="DataException">If there was an error reading required information from the TestConfiguration.</exception>
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1024:UsePropertiesWhereAppropriate")]   // Ignore this warning.
        public static ISvcShared GetSvcSharedFromTestConfig()
        {
            TestConfiguration testConfig = TestConfiguration.GetInstance();

            return CreateArtifactStore(testConfig.BlueprintServerAddress);
        }
    }
}
