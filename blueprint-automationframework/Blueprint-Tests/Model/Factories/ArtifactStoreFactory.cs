using System.Data;
using CustomAttributes;
using Model.Impl;

namespace Model.Factories
{
    public static class ArtifactStoreFactory
    {
        /// <summary>
        /// Creates a new IArtifactStore.
        /// </summary>
        /// <param name="address">The URI address of the Admin Store service.</param>
        /// <returns>An IArtifactStore object.</returns>
        public static IArtifactStore CreateArtifactStore(string address)
        {
            IArtifactStore adminStore = new ArtifactStore(address);
            return adminStore;
        }

        /// <summary>
        /// Creates an ArtifactStore object with the settings defined in the TestConfiguration.
        /// </summary>
        /// <returns>The ArtifactStore object.</returns>
        /// <exception cref="DataException">If there was an error reading required information from the TestConfiguration.</exception>
        public static IArtifactStore GetArtifactStoreFromTestConfig()
        {
            string address = FactoryCommon.GetServiceAddressFromTestConfig(Categories.ArtifactStore);
            return CreateArtifactStore(address);
        }
    }
}
