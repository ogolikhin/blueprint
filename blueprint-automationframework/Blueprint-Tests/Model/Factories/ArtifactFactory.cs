using Model.OpenApiModel;
using Model.OpenApiModel.Impl;
using TestConfig;
using Utilities;
using Utilities.Factories;

namespace Model.Factories
{
    public static class ArtifactFactory
    {
        /// <summary>
        /// Create an artifact object and populate required attribute values with ArtifactTypeId, ArtifactTypeName, and ProjectId based the target project
        /// </summary>
        /// <param name="address">address for Blueprint application server</param>
        /// <param name="user">user for authentication</param>
        /// <param name="project">The target project</param>
        /// <param name="artifactType">artifactType</param>
        /// <returns>new artifact object for the target project with selected artifactType</returns>
        public static IOpenApiArtifact CreateOpenApiArtifact(string address, IUser user, IProject project, BaseArtifactType artifactType)
        {
            ThrowIf.ArgumentNull(project, nameof(project));
            
            IOpenApiArtifact artifact = new OpenApiArtifact(address);
            artifact.ArtifactTypeName = artifactType.ToString();
            artifact.BaseArtifactType = artifactType;
            artifact.Name = "OpenApi_Artifact_" + artifact.ArtifactTypeName + "_" + RandomGenerator.RandomAlphaNumeric(5);

            artifact.ProjectId = project.Id;
            artifact.ArtifactTypeId = project.GetArtifactTypeId(address: address, projectId: project.Id, baseArtifactTypeName: artifactType,
                user: user);
            return artifact;
        }

        /// <summary>
        /// Create an artifact object using the Blueprint application server address from the TestConfiguration file
        /// </summary>
        /// <returns>new artifact object</returns>
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1024:UsePropertiesWhereAppropriate")]   // Ignore this warning.
        public static IOpenApiArtifact CreateOpenApiArtifact(IProject project, IUser user, BaseArtifactType artifactType)
        {
            TestConfiguration testConfig = TestConfiguration.GetInstance();
            return CreateOpenApiArtifact(testConfig.BlueprintServerAddress, user, project, artifactType);
        }
    }
}
