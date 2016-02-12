using Model.Impl;
using TestConfig;

namespace Model.Factories
{
    public static class ArtifactFactory
    {
        /// <summary>
        /// Create an artifact object.
        /// </summary>
        /// <returns>The new project object.</returns>
        public static IOpenApiArtifact CreateArtifact(string address)
        {
            IOpenApiArtifact artifact = new OpenApiArtifact(address);
            return artifact;
        }
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1024:UsePropertiesWhereAppropriate")]   // Ignore this warning.
        public static IOpenApiArtifact CreateArtifactFromTestConfig()
        {
            TestConfiguration testConfig = TestConfiguration.GetInstance();
            return CreateArtifact(testConfig.BlueprintServerAddress);
        }
    }
}
