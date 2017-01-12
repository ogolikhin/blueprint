using System.Data;
using CustomAttributes;
using Model.Impl;

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
        public static IFileStore GetFileStoreFromTestConfig()
        {
            string address = FactoryCommon.GetServiceAddressFromTestConfig(Categories.FileStore);
            return CreateFileStore(address);
        }
    }
}
