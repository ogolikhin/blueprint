using Model.StorytellerModel;
using Model.StorytellerModel.Impl;

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
            var storyteller = new Storyteller(address);
            return storyteller;
        }

        /// <summary>
        /// Creates a Storyteller object with the settings defined in the TestConfiguration.
        /// </summary>
        /// <returns>The Storyteller object.</returns>
        /// <exception cref="DataException">If there was an error reading required information from the TestConfiguration.</exception>
        public static IStoryteller GetStorytellerFromTestConfig()
        {
            var testConfig = TestConfig.TestConfiguration.GetInstance();
            return CreateStoryteller(testConfig.BlueprintServerAddress);
        }
    }
}
