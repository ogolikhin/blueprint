using System.Data;
using CustomAttributes;
using Model.Impl;

namespace Model.Factories
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
        public static IAccessControl GetAccessControlFromTestConfig()
        {
            string address = FactoryCommon.GetServiceAddressFromTestConfig(Categories.AccessControl);
            return CreateAccessControl(address);
        }
    }
}
