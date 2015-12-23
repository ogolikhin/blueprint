using System.Data;

using Logging;
using Model;
using Model.Impl;
using TestConfig;

namespace Helper.Factories
{
    public static class AccessControlFactory
    {
        /// <summary>
        /// Creates a new IAccessControl.
        /// </summary>
        /// <param name="address">The URI address of the Access Control service.</param>
        /// <returns>An IAccessControl object.</returns>
        public static IAccessControl CreateAccessControl(string address)
        {
            IAccessControl accessControl = new AccessControl(address);
            return accessControl;
        }

        /// <summary>
        /// Creates an AccessControl object with the settings defined in the TestConfiguration.
        /// </summary>
        /// <returns>The AccessControl object.</returns>
        /// <exception cref="DataException">If there was an error reading required information from the TestConfiguration.</exception>
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1024:UsePropertiesWhereAppropriate")]   // Ignore this warning.
        public static IAccessControl GetAccessControlFromTestConfig()
        {
            TestConfiguration testConfig = TestConfiguration.GetInstance();
            const string keyName = "AccessControl";

            if (!testConfig.Services.ContainsKey(keyName))
            {
                string msg = string.Format("No <Service> tag named '{0}' was found in the TestConfiguration.xml file!  Please update it.", keyName);
                Logger.WriteError(msg);
                throw new DataException(msg);
            }

            return CreateAccessControl(testConfig.Services[keyName].Address);
        }
    }
}
