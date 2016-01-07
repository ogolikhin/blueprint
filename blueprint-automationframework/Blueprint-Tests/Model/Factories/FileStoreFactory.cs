using System.Data;
using Logging;
using Model.Impl;
using TestConfig;

namespace Model.Factories
{
    public static class FileStoreFactory
    {
        /// <summary>
        /// Creates a new IFileStore.
        /// </summary>
        /// <param name="address">The URI address of the filestore.</param>
        /// <returns>An IFileStore object.</returns>
        public static IFileStore CreateFileStore(string address)
        {
            IFileStore filestore = new FileStore(address);
            return filestore;
        }

        /// <summary>
        /// Creates a FileStore object with the settings defined in the TestConfiguration.
        /// </summary>
        /// <returns>The FileStore object.</returns>
        /// <exception cref="DataException">If there was an error reading required information from the TestConfiguration.</exception>
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1024:UsePropertiesWhereAppropriate")]   // Ignore this warning.
        public static IFileStore GetFileStoreFromTestConfig()
        {
            TestConfiguration testConfig = TestConfiguration.GetInstance();
            const string keyName = "FileStore";

            if (!testConfig.Services.ContainsKey(keyName))
            {
                string msg = string.Format("No <Service> tag named '{0}' was found in the TestConfiguration.xml file!  Please update it.", keyName);
                Logger.WriteError(msg);
                throw new DataException(msg);
            }

            return CreateFileStore(testConfig.Services[keyName].Address);
        }
    }
}
