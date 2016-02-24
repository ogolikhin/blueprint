﻿using System.Data;
using Common;
using Model.Impl;
using TestConfig;

namespace Model.Factories
{
    public static class StorytellerFactory
    {
        /// <summary>
        /// Creates a new IStoryteller.
        /// </summary>
        /// <param name="address">The URI address of the Storyteller REST API.</param>
        /// <returns>An IStoryteller object.</returns>
        public static IStoryteller CreateStoryteller(string address)
        {
            IStoryteller storyteller = new Storyteller(address);
            return storyteller;
        }

        /// <summary>
        /// Creates a Storyteller object with the settings defined in the TestConfiguration.
        /// </summary>
        /// <returns>The Storyteller object.</returns>
        /// <exception cref="DataException">If there was an error reading required information from the TestConfiguration.</exception>
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1024:UsePropertiesWhereAppropriate")]   // Ignore this warning.
        public static IStoryteller GetStorytellerFromTestConfig()
        {
            TestConfiguration testConfig = TestConfiguration.GetInstance();
            return CreateStoryteller(testConfig.BlueprintServerAddress);
        }
    }
}
