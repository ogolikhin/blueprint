using System.Data;
using CustomAttributes;
using Model.Impl;

namespace Model.Factories
{
    public static class ConfigControlFactory
    {
        /// <summary>
        /// Creates a new IConfigControl.
        /// </summary>
        /// <param name="address">The URI address of the ConfigControl.</param>
        /// <returns>An IConfigControl object.</returns>
        public static IConfigControl CreateConfigControl(string address)
        {
            IConfigControl filestore = new ConfigControl(address);
            return filestore;
        }

        /// <summary>
        /// Creates a ConfigControl object with the settings defined in the TestConfiguration.
        /// </summary>
        /// <returns>The ConfigControl object.</returns>
        /// <exception cref="DataException">If there was an error reading required information from the TestConfiguration.</exception>
        public static IConfigControl GetConfigControlFromTestConfig()
        {
            string address = FactoryCommon.GetServiceAddressFromTestConfig(Categories.ConfigControl);
            return CreateConfigControl(address);
        }
    }
}
