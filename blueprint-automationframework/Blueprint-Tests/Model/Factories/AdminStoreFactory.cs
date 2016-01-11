using System.Data;
using Common;
using Model.Impl;
using TestConfig;

namespace Model.Factories
{
    public static class AdminStoreFactory
    {
        /// <summary>
        /// Creates a new IAdminStore.
        /// </summary>
        /// <param name="address">The URI address of the Admin Store service.</param>
        /// <returns>An IAdminStore object.</returns>
        public static IAdminStore CreateAdminStore(string address)
        {
            IAdminStore adminStore = new AdminStore(address);
            return adminStore;
        }

        /// <summary>
        /// Creates an AdminStore object with the settings defined in the TestConfiguration.
        /// </summary>
        /// <returns>The AdminStore object.</returns>
        /// <exception cref="DataException">If there was an error reading required information from the TestConfiguration.</exception>
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1024:UsePropertiesWhereAppropriate")]   // Ignore this warning.
        public static IAdminStore GetAdminStoreFromTestConfig()
        {
            TestConfiguration testConfig = TestConfiguration.GetInstance();
            const string keyName = "AdminStore";

            if (!testConfig.Services.ContainsKey(keyName))
            {
                string msg = string.Format("No <Service> tag named '{0}' was found in the TestConfiguration.xml file!  Please update it.", keyName);
                Logger.WriteError(msg);
                throw new DataException(msg);
            }

            return CreateAdminStore(testConfig.Services[keyName].Address);
        }
    }
}
