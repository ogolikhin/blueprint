using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Common;
using Model.Impl;
using TestConfig;

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
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1024:UsePropertiesWhereAppropriate")]   // Ignore this warning.
        public static IArtifactStore GetArtifactStoreFromTestConfig()
        {
            TestConfiguration testConfig = TestConfiguration.GetInstance();
            const string keyName = "ArtifactStore";

            if (!testConfig.Services.ContainsKey(keyName))
            {
                string msg = I18NHelper.FormatInvariant("No <Service> tag named '{0}' was found in the TestConfiguration.xml file!  Please update it.", keyName);
                Logger.WriteError(msg);
                throw new DataException(msg);
            }

            return CreateArtifactStore(testConfig.Services[keyName].Address);
        }
    }
}
