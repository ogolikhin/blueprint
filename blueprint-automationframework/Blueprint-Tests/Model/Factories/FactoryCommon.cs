using System.Data;
using Common;
using TestConfig;

namespace Model.Factories
{
    public static class FactoryCommon
    {
        /// <summary>
        /// Gets the address for the specified service from the TestConfiguration.
        /// </summary>
        /// <param name="keyName">The key name of the service in TestConfiguration.xml</param>
        /// <returns>The address of the specified service.</returns>
        /// <exception cref="DataException">If there was an error reading required information from the TestConfiguration.</exception>
        public static string GetServiceAddressFromTestConfig(string keyName)
        {
            TestConfiguration testConfig = TestConfiguration.GetInstance();

            if (!testConfig.Services.ContainsKey(keyName))
            {
                string msg = I18NHelper.FormatInvariant("No <Service> tag named '{0}' was found in the TestConfiguration.xml file!  Please update it.", keyName);
                Logger.WriteError(msg);
                throw new DataException(msg);
            }

            return testConfig.Services[keyName].Address;
        }
    }
}
