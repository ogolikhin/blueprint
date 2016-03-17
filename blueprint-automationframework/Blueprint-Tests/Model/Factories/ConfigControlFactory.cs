using System.Data;
using Common;
using Model.Impl;
using TestConfig;

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
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1024:UsePropertiesWhereAppropriate")]   // Ignore this warning.
        public static IConfigControl GetConfigControlFromTestConfig()
        {
            TestConfiguration testConfig = TestConfiguration.GetInstance();
            const string keyName = "ConfigControl";

            if (!testConfig.Services.ContainsKey(keyName))
            {
                string msg = I18NHelper.FormatInvariant("No <Service> tag named '{0}' was found in the TestConfiguration.xml file!  Please update it.", keyName);
                Logger.WriteError(msg);
                throw new DataException(msg);
            }

            return CreateConfigControl(testConfig.Services[keyName].Address);
        }
    }
}
