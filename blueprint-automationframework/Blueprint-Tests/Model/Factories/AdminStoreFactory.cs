using CustomAttributes;
using Model.Impl;

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
            var adminStore = new AdminStore(address);
            return adminStore;
        }

        /// <summary>
        /// Creates an AdminStore object with the settings defined in the TestConfiguration.
        /// </summary>
        /// <returns>The AdminStore object.</returns>
        /// <exception cref="DataException">If there was an error reading required information from the TestConfiguration.</exception>
        public static IAdminStore GetAdminStoreFromTestConfig()
        {
            string address = FactoryCommon.GetServiceAddressFromTestConfig(Categories.AdminStore);
            return CreateAdminStore(address);
        }
    }
}
