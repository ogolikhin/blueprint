using Model;
using Model.Impl;

namespace Helper.Factories
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
    }
}
