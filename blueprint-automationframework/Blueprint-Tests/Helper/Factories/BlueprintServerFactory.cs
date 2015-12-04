﻿using Model;
using Model.Impl;
using TestConfig;

namespace Helper.Factories
{
    public static class BlueprintServerFactory
    {
        /// <summary>
        /// Creates a new BlueprintServer object.
        /// </summary>
        /// <param name="serverAddress">The URL of the Blueprint server.</param>
        /// <returns>The BlueprintServer object.</returns>
        public static IBlueprintServer CreateBlueprintServer(string serverAddress)
        {
            IBlueprintServer server = new BlueprintServer(serverAddress);
            return server;
        }

        /// <summary>
        /// Creates a BlueprintServer object with the settings defined in the TestConfiguration.
        /// </summary>
        /// <returns>The BlueprintServer object.</returns>
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1024:UsePropertiesWhereAppropriate")]   // Ignore this warning.
        public static IBlueprintServer GetBlueprintServerFromTestConfig()
        {
            TestConfiguration testConfig = TestConfiguration.GetInstance();
            return CreateBlueprintServer(testConfig.BlueprintServerAddress);
        }
    }
}
