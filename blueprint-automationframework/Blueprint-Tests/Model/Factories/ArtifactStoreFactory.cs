﻿using System;
using System.Data;
using Model.Impl;
using TestConfig;
using Common;

namespace Model.Factories
{
    public static class ArtifactStoreFactory
    {
        public static IArtifactStore CreateArtifactStore(string address)
        {
            IArtifactStore artifactstore = new ArtifactStore(address);
            return artifactstore;
        }

        [Obsolete("Under developement")]
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
